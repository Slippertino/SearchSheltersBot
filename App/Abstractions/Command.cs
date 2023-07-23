using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Linq;
using SearchSheltersBot.BusinessLogic;

namespace SearchSheltersBot
{
	/// <summary>
	/// Сущность команды.
	/// </summary>
	public abstract class Command
	{
		/// <summary>
		/// Название команды.
		/// </summary>
		public abstract string? Name { get; }

		/// <summary>
		/// Описание команды.
		/// </summary>
		public abstract string? Description { get; }

		/// <summary>
		/// Список состояний, при которых необходимо выполнить команду.
		/// </summary>
		protected abstract ISet<UserStateType> RequiredStates { get; }

		/// <summary>
		/// Репозиторий данных.
		/// </summary>
		protected readonly IBotRepository _botRepository;

		/// <summary>
		/// Конструктор DI с одним параметром.
		/// </summary>
		/// <param name="botRepository"></param>
		protected Command(IBotRepository botRepository)
		{
			_botRepository = botRepository;
		}

		/// <summary>
		/// Выполняет команду.
		/// </summary>
		/// <param name="botClient"> Клиент бота </param>
		/// <param name="user"> Информация о пользователе </param>
		/// <param name="update"> Информация о сообщении </param>
		/// <param name="cancellationToken"> Токен останова </param>
		/// <returns></returns>
		public abstract Task<CommandResultsInfo> ExecuteAsync(
			ITelegramBotClient botClient,
			User user,
			Update update,
			CancellationToken cancellationToken
		);

		/// <summary>
		/// Добавляет команду в меню бота.
		/// </summary>
		/// <param name="builder"> Объект строителя меню </param>
		public abstract void AddToMenu(BotMenuBuilder builder);

		/// <summary>
		/// Проверяет, подходит ли команда для выполнения.
		/// </summary>
		/// <param name="msg"> Сообщение </param>
		/// <param name="userState"> Состояние пользователя </param>
		/// <returns></returns>
		public bool IsSuitToExecute(Message msg, UserStateType userState)
		{
			if (msg == null)
			{
				throw new ArgumentNullException(nameof(msg));
			}

			return
				msg.Text == Name ||
				msg.Text == Description ||
				RequiredStates.Contains(userState);
		}
	}
}
