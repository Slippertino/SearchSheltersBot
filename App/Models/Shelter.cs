using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SearchSheltersBot
{
	/// <summary>
	/// Сущность укрытия.
	/// </summary>
	public class Shelter
	{
		/// <summary>
		/// Идентификатор записи.
		/// </summary>
		public Guid Id { get; set; } = Guid.Empty;

		/// <summary>
		/// Название города.
		/// </summary>
		public string City { get; set; } = string.Empty;

		/// <summary>
		/// Адрес укрытия.
		/// </summary>
		public string Address { get; set; } = string.Empty;

		/// <summary>
		/// Описание укрытия.
		/// </summary>
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// Дистанция до укрытия(опционально, не прописывается в БД).
		/// </summary>
		public int? Distance { get; set; } = null;

		/// <summary>
		/// Значение широты геолокации укрытия.
		/// </summary>
		public double Latitude { get; set; }

		/// <summary>
		/// Значение долготы геолокации укрытия.
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// Конструктор по умолчанию.
		/// </summary>
		public Shelter() { }

		/// <summary>
		/// Конструктор с двумя параметрами.
		/// Необходим для проекции сущности из БД на сущность для конкретной геолокации пользователя.
		/// </summary>
		/// <param name="shelter"></param>
		/// <param name="distance"></param>
		public Shelter(Shelter shelter, int distance)
		{
			Id = shelter.Id;
			City = shelter.City;
			Address = shelter.Address;
			Description = shelter.Description;
			Longitude = shelter.Longitude;
			Latitude = shelter.Latitude;
			Distance = distance;
		}

		/// <summary>
		/// Переопределенный метод перевода в строку.
		/// </summary>
		/// <returns></returns>
		public override string ToString()  
		{
			return $"{City}, {Address} ({Distance ?? 0} м)";
		}
	}
}
