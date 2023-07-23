using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LazyCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SearchSheltersBot;
using Telegram.Bot.Types.ReplyMarkups;

namespace SearchSheltersBot
{
	/// <summary>
	/// Сущность для работы с данными.
	/// </summary>
	public class BotRepository : IBotRepository
	{
		/// <summary>
		/// Меню.
		/// </summary>
		public IReplyMarkup MenuMarkup { get; set; }

		/// <summary>
		/// ORM-сущность для работы с БД.
		/// </summary>
		private readonly BotContext _botContext;

		/// <summary>
		/// Кеш пользовательских настроек.
		/// </summary>
		private readonly UsersSettingsCache _usersCache;

		/// <summary>
		/// Кеш с результатами о ближайших укрытиях.
		/// </summary>
		private readonly SheltersCache _sheltersCache;

		/// <summary>
		/// Конструктор DI с тремя параметрами.
		/// </summary>
		/// <param name="botContext"></param>
		/// <param name="usersCache"></param>
		/// <param name="sheltersCache"></param>
		public BotRepository(BotContext botContext, UsersSettingsCache usersCache, SheltersCache sheltersCache)
		{
			_botContext = botContext;
			_usersCache = usersCache;
			_sheltersCache = sheltersCache;
		}

		public async Task<UserSettings> EnsureUserCreatedAsync(long userId)
		{
			var user = await _botContext.UserSettings.FindAsync(userId);

			if (user == null)
			{
				var newUser = new UserSettings { Id = userId, State = UserStateType.FREED };

				await _botContext.UserSettings.AddAsync(newUser);

				await _usersCache.AddOrUpdateAsync(newUser);
				await _botContext.SaveChangesAsync();

				return newUser;
			}
			else if (!_usersCache.TryGet(userId, out UserSettings temp))
			{
				await _usersCache.AddOrUpdateAsync(user);
			}

			return user;
		}

		public async Task DeleteUserAsync(long userId)
		{
			var user = await ExtractUserSettingsAsync(userId);

			_botContext.UserSettings.Remove(user);
			await _botContext.SaveChangesAsync();

			_usersCache.Remove(userId);
		}

		public async Task<UserStateType> GetUserStateAsync(long userId)
		{
			var user = await ExtractUserSettingsAsync(userId);

			return user.State;
		}

		public async Task SetUserStateAsync(long userId, UserStateType state)
		{
			var user = await ExtractUserSettingsAsync(userId);

			user.State = state;
			_botContext.Update(user);
			await _botContext.SaveChangesAsync();

			await _usersCache.AddOrUpdateAsync(user);
		}

		public async Task<IEnumerable<Shelter>> FindNearbySheltersAsync(long userId, double latitude, double longitude)
		{
			var user = await ExtractUserSettingsAsync(userId);

			var settings = new SearchSettings
			{
				Latitude = latitude,
				Longitude = longitude,
				MinRadiusM = user.MinSearchRadius,
				MaxRadiusM = user.MaxSearchRadius
			};

			var cachedResults = new List<Shelter>();
			(settings.MinRadiusM, settings.MaxRadiusM)
				= await _sheltersCache.GetEstimatedSheltersAsync(settings, cachedResults);

			if (settings.MinRadiusM == settings.MaxRadiusM)
			{
				return cachedResults
					.OrderBy(x => x.Distance)
					.AsEnumerable();
			}

			var calculatedShelters = _botContext.Shelters
				.Select(x => new Shelter(x, DistanceCalculator.Calculate(x.Latitude, x.Longitude, latitude, longitude)))
				.AsEnumerable();

			var result = calculatedShelters
				.Where(x => (settings.MinRadiusM <= x.Distance) && (x.Distance <= settings.MaxRadiusM))
				.Union(cachedResults)
				.OrderBy(x => x.Distance)
				.AsEnumerable();

			if (result.Count() != 0) 
			{
				await _sheltersCache.AddAsync(settings, result);
			}

			return result;
		}

		public async Task SetSearchRadiusRangeAsync(long userId, int minRadius, int maxRadius)
		{
			var user = await ExtractUserSettingsAsync(userId);

			user.MinSearchRadius = minRadius;
			user.MaxSearchRadius = maxRadius;

			_botContext.Update(user);
			await _botContext.SaveChangesAsync();

			await _usersCache.AddOrUpdateAsync(user);
		}

		private async Task<UserSettings> ExtractUserSettingsAsync(long userId)
		{
			if (_usersCache.TryGet(userId, out UserSettings us))
			{
				return us;
			}

			var user = await _botContext.UserSettings.FindAsync(userId);

			if (user == null)
			{
				throw new ArgumentNullException($"Пользователя с id = {userId} нет в базе.");
			}

			return user;
		}
	}
}
