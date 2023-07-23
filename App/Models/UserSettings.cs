using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SearchSheltersBot
{
	/// <summary>
	/// Настройки пользователя.
	/// </summary>
	public class UserSettings
	{
		/// <summary>
		/// Идентификатор пользователя в Telegram.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Текущее состояние взаимодействие пользователя с ботом.
		/// </summary>
		public UserStateType State { get; set; }

		/// <summary>
		/// Минимальный радиус поиска укрытий.
		/// </summary>
		public int MinSearchRadius { get; set; } = 0;

		/// <summary>
		/// Максимальный радиус поиска укрытий.
		/// </summary>
		public int MaxSearchRadius { get; set; } = 100;

		/// <summary>
		/// Конвертирует в строку.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
