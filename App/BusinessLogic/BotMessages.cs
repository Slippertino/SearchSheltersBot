using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace SearchSheltersBot
{
	/// <summary>
	/// Сущность с набором текстовых ответов бота.
	/// </summary>
	public static class BotMessages
	{
		public static string GreetingsMessage { get; } = 
			"Привет! Я помогу тебе найти укрытие в случае опасности.\n" +
			"Выполни команду /help из меню, чтобы узнать подробности.";

		public static string HelpMessage { get; } =
			"Ты можешь использовать следующие команды: \n" +
			"/search - выполняет поиск ближайших убежищ в заданном диапазоне по отправленной геолокации.\n" +
			"Диапазон по умолчанию до 100 метров.\n" +
			"/distance - позволяет настроить диапазон поиска убежищ.\n" +
			"Будь здоров, друг!";

		public static string NeedDistanceRangeMessage { get; } = "Выбери диапазон поиска укрытий.";

		public static string SettingsUpdatedMessage { get; } = "Настройки поиска успешно обновлены!";

		public static string NeedLocationTitle { get; } = "Поделиться геолокацией";

		public static string NeedLocationMessage { get; } = "Отправь свою геолокацию.";

		public static string NotRecognizedCommand { get; } = "Я тебя не понимаю...";

		public static string LocationExpectedErrorMessage { get; } = "Упс... Что-то пошло не так. Ожидалась твоя геолокация.";

		public static string NoNearbySheltersMessage { get; } = "К сожалению, рядом с тобой не было найдено укрытий.";

		public static string GetSheltersInfoMessage(IEnumerable<Shelter> shelters)
		{
			var builder = new StringBuilder();

			builder.Append($"Найдены укрытия({shelters.Count()}): \n");

			var i = 0;
			foreach (var shelter in shelters)
			{
				builder.Append($"{++i}: {shelter}\n");
			}

			return builder.ToString();
		}

		public static string GetNearestShelterDescriptionMessage(Shelter shelter)
		{
			return $"Ближайшее укрытие:\n{shelter.Description} по адресу {shelter}\n";
		}

		public static string Combine(params string[] msgs)
		{
			var builder = new StringBuilder();

			foreach(var msg in msgs)
			{
				builder.Append(msg);
			}

			return builder.ToString();
		}
	}
}
