using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace SearchSheltersBot
{
	/// <summary>
	/// Интерфейс для работы с репозиторием данных.
	/// </summary>
	public interface IBotRepository
	{
		/// <summary>
		/// Меню.
		/// </summary>
		IReplyMarkup MenuMarkup { get; set; }

		/// <summary>
		/// Гарантирует создание записи о настройках пользователе в БД.
		/// </summary>
		/// <param name="userId"> Id пользователя </param>
		/// <param name="state"> Состояние взаимодействия </param>
		Task<UserSettings> EnsureUserCreatedAsync(long userId);

		/// <summary>
		/// Удаляет информацию о пользователе.
		/// </summary>
		/// <param name="userId"> Id пользователя </param>
		Task DeleteUserAsync(long userId);

		/// <summary>
		/// Возвращает состояние взаимодействия с пользователем.
		/// </summary>
		/// <param name="userId"> Id пользователя </param>
		/// <returns></returns>
		Task<UserStateType> GetUserStateAsync(long userId);

		/// <summary>
		/// Устанавливает состояние взаимодействия с пользователем.
		/// </summary>
		/// <param name="userId"> Id пользователя </param>
		/// <param name="state"> Состояние взаимодействия </param>
		Task SetUserStateAsync(long userId, UserStateType state);

		/// <summary>
		/// Находит ближайшие к заданной пользователем геолокации убежища.
		/// </summary>
		/// <param name="userId"> Id пользователя </param>
		/// <param name="latitude"> Значение ширины геолокации пользователя </param>
		/// <param name="longitude"> Значение долготы геолокации пользователя </param>
		/// <returns></returns>
		Task<IEnumerable<Shelter>> FindNearbySheltersAsync(long userId, double latitude, double longitude);

		/// <summary>
		/// Устанавливает минимальный и максимальный радиусы поиска убежищ.
		/// </summary>
		/// <param name="userId"> Id пользователя </param>
		/// <param name="minRadius"> Минимальный радиус поиска убежищ </param>
		/// <param name="maxRadius"> Максимальный радиус поиска убежищ </param>
		Task SetSearchRadiusRangeAsync(long userId, int minRadius, int maxRadius);
	}
}
