using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SearchSheltersBot
{
	/// <summary>
	/// Сущность для кеширования настроек пользователей.
	/// </summary>
	public class UsersSettingsCache
	{
		/// <summary>
		/// Опции работы кеша.
		/// </summary>
		private readonly MemoryCacheEntryOptions _usersCacheOptions;

		/// <summary>
		/// Сущность кеша.
		/// </summary>
		private readonly IAppCache _cache;

		/// <summary>
		/// Логгер.
		/// </summary>
		private readonly ILogger<UsersSettingsCache> _logger;

		/// <summary>
		/// Конструктор DI с одним параметром.
		/// </summary>
		/// <param name="cache"></param>
		public UsersSettingsCache(IAppCache cache, ILogger<UsersSettingsCache> logger)
		{
			_usersCacheOptions = new MemoryCacheEntryOptions()
				.SetSize(300)
				.SetPriority(CacheItemPriority.Normal)
				.RegisterPostEvictionCallback(callback: OnEviction, state: this)
				.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10));

			_cache = cache;
			_logger = logger;

			_logger.LogInformation($"Cache for user's states was initialized.\n");
		}

		/// <summary>
		/// Попытка получить настройки пользователя по Id.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public bool TryGet(long userId, out UserSettings settings)
		{
			if (_cache.TryGetValue(userId.ToString(), out UserSettings us))
			{
				settings = us;
				return true;
			}

			settings = null;

			return false;
		}

		/// <summary>
		/// Добавляет или обновляет настройки пользователя.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task<UserSettings> AddOrUpdateAsync(UserSettings user)
		{
			var result = await _cache.GetOrAddAsync(user.Id.ToString(), () => Task.Run(() => user), _usersCacheOptions);

			_logger.LogInformation(
				$"Added to cache or updated: {result}\n"
			);

			return result;
		}

		/// <summary>
		/// Явно удаляет пользовательские настройки из кеша.
		/// </summary>
		/// <param name="userId"></param>
		public void Remove(long userId)
		{
			_cache.Remove(userId.ToString());
		}

		/// <summary>
		/// Обработчик события вытеснения запроса из кеша.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="reason"></param>
		/// <param name="state"></param>
		private static void OnEviction(object key, object value, EvictionReason reason, object state)
		{
			var main = state as UsersSettingsCache;
			var request = value as UserSettings;

			main._logger.LogInformation(
				$"Removed searching result from cache:\n{request}\n"
			);
		}
	}
}
