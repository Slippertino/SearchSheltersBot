using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSheltersBot
{
	/// <summary>
	/// Типы состояний взаимодействия с пользователем.
	/// </summary>
	public enum UserStateType
	{
		/// <summary>
		/// Свободный: мы не ожидаем от пользователя каких-либо данных.
		/// </summary>
		FREED = 0,

		/// <summary>
		/// Тип ожидания ввода нового диапазона для радиуса поиска убежищ.
		/// </summary>
		WAITING_DISTANCE = 1,

		/// <summary>
		/// Тип ожидания геолокации от пользователя.
		/// </summary>
		WAITING_LOCATION = 2,
	}
}
