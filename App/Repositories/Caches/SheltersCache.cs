using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SearchSheltersBot
{
	/// <summary>
	/// Настройки поиска укрытий.
	/// </summary>
	public class SearchSettings
	{
		/// <summary>
		/// Широта.
		/// </summary>
		public double Latitude { get; set; }

		/// <summary>
		/// Долгота.
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// Минимальный радиус поиска.
		/// </summary>
		public int MinRadiusM { get; set; }

		/// <summary>
		/// Максимальный радиус поиска.
		/// </summary>
		public int MaxRadiusM { get; set; }

		/// <summary>
		/// Возвращает строку.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		/// <summary>
		/// Возвращает хеш.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return (Latitude, Longitude, MinRadiusM, MaxRadiusM).GetHashCode();
		}
	}

	/// <summary>
	/// Сущность для кеширования результатов поиска укрытий.
	/// </summary>
	public class SheltersCache
	{
		/// <summary>
		/// Запрос на поиск укрытий.
		/// </summary>
		class SheltersRequest 
		{
			/// <summary>
			/// Настройки поиска.
			/// </summary>
			public SearchSettings ShelterKey { get; set; }

			/// <summary>
			/// Результаты поиска.
			/// </summary>
			public IEnumerable<Shelter> Shelters { get; set; }
		}

		/// <summary>
		/// Максимально допустимое отклонение от закешированной геолокации для использования уже выбранных укрытий.
		/// </summary>
		private const double kMaxEntryVarianceM = 10;

		/// <summary>
		/// Опции работы кеша.
		/// </summary>
		private readonly MemoryCacheEntryOptions _sheltersCacheOptions;

		/// <summary>
		/// Сущность кеша.
		/// </summary>
		private readonly IAppCache _cache;

		/// <summary>
		/// Логгер.
		/// </summary>
		private readonly ILogger<SheltersCache> _logger;

		/// <summary>
		/// Потокобезопасная хеш-таблица для хранения ключей из кэша.
		/// </summary>
		private ConcurrentDictionary<int, SearchSettings> _sheltersKeys;

		/// <summary>
		/// Конструктор DI с одним параметром.
		/// </summary>
		/// <param name="cache"></param>
		public SheltersCache(IAppCache cache, ILogger<SheltersCache> logger)
		{
			_sheltersCacheOptions = new MemoryCacheEntryOptions()
				.SetSize(50)
				.SetPriority(CacheItemPriority.Normal)
				.RegisterPostEvictionCallback(callback: OnEviction, state: this)
				.SetAbsoluteExpiration(DateTime.Now.AddMinutes(15));

			_sheltersKeys = new ConcurrentDictionary<int, SearchSettings>();
			_cache = cache;
			_logger = logger;

			_logger.LogInformation($"Cache for search shelters results was initialized.\n");
		}

		/// <summary>
		/// Добавляет результаты запроса в кеш.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="shelters"></param>
		/// <returns></returns>
		public async Task AddAsync(SearchSettings settings, IEnumerable<Shelter> shelters)
		{
			if (FindSimilar(settings).Count() != 0)
			{
				return;
			}

			var hash = settings.GetHashCode();

			if (_sheltersKeys.TryAdd(hash, settings))
			{
				var result = await _cache.GetOrAddAsync(
					hash.ToString(),
					() => Task.Run(() => shelters),
					_sheltersCacheOptions
				);

				_logger.LogInformation(
					$"Added searching result to cache:\n{settings}\n" +
					$"Cache size: {_sheltersKeys.Count}\n"
				);
			}
		}

		/// <summary>
		/// На основе ранее выполненных запросов частично находит укрытия в определенном диапазоне.
		/// </summary>
		/// <param name="settings"> Настройки поиска </param>
		/// <param name="results"> Контейнер для частичной записи </param>
		/// <returns> Оставшийся для обработки диапазон(сузившийся) </returns>
		public async Task<(int, int)> GetEstimatedSheltersAsync(SearchSettings settings, List<Shelter> results)
		{
			var nearbyCachedPoint = FindSimilar(settings);

			if (nearbyCachedPoint.Count() == 0)
			{
				return (settings.MinRadiusM, settings.MaxRadiusM);
			}

			_logger.LogInformation($"Useful cache result exists for key: {settings}\n");

			var cachedSettings = nearbyCachedPoint.Aggregate((x, y) => {
				return 
					(settings.MaxRadiusM - x.MinRadiusM > settings.MaxRadiusM - y.MinRadiusM) &&
					(settings.MinRadiusM < x.MaxRadiusM) ? x : y;
			});

			if (settings.MinRadiusM > cachedSettings.MaxRadiusM || 
				settings.MaxRadiusM < cachedSettings.MinRadiusM)
			{
				return (settings.MinRadiusM, settings.MaxRadiusM);
			}

			var shelters = await _cache.GetAsync<IEnumerable<Shelter>>(
				cachedSettings.GetHashCode().ToString()
			);

			if (shelters is null)
			{
				_logger.LogCritical($"Received cached result is null. Key: {cachedSettings}\n");
				return (settings.MinRadiusM, settings.MaxRadiusM);
			}

			var zone = IntersectSegments(
				(settings.MinRadiusM, settings.MaxRadiusM),
				(cachedSettings.MinRadiusM, cachedSettings.MaxRadiusM)
			);

			results.AddRange(
				shelters.Where(x => (x.Distance >= zone.Item1) && (x.Distance <= zone.Item2))
			);

			return SubstactSegments(
				(settings.MinRadiusM, settings.MaxRadiusM),
				(cachedSettings.MinRadiusM, cachedSettings.MaxRadiusM)
			);
		}

		private IEnumerable<SearchSettings> FindSimilar(SearchSettings settings)
		{
			return _sheltersKeys.Values.Where(x =>
				DistanceCalculator.Calculate(x.Latitude, x.Longitude, settings.Latitude, settings.Longitude) <= kMaxEntryVarianceM
			);
		}

		/// <summary>
		/// Находит пересечение отрезков типа [min, max].
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		private (int, int) IntersectSegments((int, int) s1, (int, int) s2)
		{
			return (Math.Max(s1.Item1, s2.Item1), Math.Min(s1.Item2, s2.Item2));
		}

		/// <summary>
		/// Находит разность между отрезками типа [min, max].
		/// </summary>
		/// <param name="s1"> Отрезок, из которого вычитают </param>
		/// <param name="s2"> Вычитаемый отрезок </param>
		/// <returns></returns>
		private (int, int) SubstactSegments((int, int) s1, (int, int) s2)
		{
			var inter = IntersectSegments(s1, s2);

			return s1.Item2 > s2.Item2 
				? (inter.Item2, s1.Item2) 
				: (s1.Item1, inter.Item1);
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
			var main = state as SheltersCache;
			var request = value as SheltersRequest;

			if (main._sheltersKeys.TryRemove(request.ShelterKey.GetHashCode(), out SearchSettings temp))
			{
				main._logger.LogInformation(
					$"Removed searching result from cache:\n{request.ShelterKey}\n" +
					$"Cache size: {main._sheltersKeys.Count}\n"
				);
			}
			else
			{
				main._logger.LogError(
					$"Error while trying to remove key from {nameof(main._sheltersKeys)} after cache eviction.\n"
				);
			}
		}
	}
}