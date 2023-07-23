using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.ReplyMarkups;

namespace SearchSheltersBot.BusinessLogic
{
	/// <summary>
	/// Строитель меню бота.
	/// </summary>
	public class BotMenuBuilder
	{
		/// <summary>
		/// Разметка элементов.
		/// </summary>
		private List<List<KeyboardButton>> _markup;

		/// <summary>
		/// Конструктор по умолчанию.
		/// </summary>
		public BotMenuBuilder()
		{
			_markup = new List<List<KeyboardButton>>();
		}

		/// <summary>
		/// Добавляет кнопку.
		/// </summary>
		/// <param name="title"> Название кнопки </param>
		/// <param name="row"> Номер ряда </param>
		/// <param name="column"> Номер колонки </param>
		public void Add(string title, int row, int column)
		{
			while (row >= _markup.Count)
			{
				_markup.Add(new List<KeyboardButton>());
			}

			while (column >= _markup[row].Count)
			{
				_markup[row].Add(null);
			}

			_markup[row][column] = new KeyboardButton(title);
		}

		/// <summary>
		/// Создает и возвращает меню.
		/// </summary>
		/// <returns></returns>
		public IReplyMarkup Build()
		{
			return new ReplyKeyboardMarkup(_markup);
		}
	}
}
