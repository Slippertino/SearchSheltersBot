using SearchSheltersBot.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace SearchSheltersBot
{
	/// <summary>
	/// Обработчик неизвестной команды.
	/// </summary>
	public class Unknown : Command
	{
		public override string? Name { get; } = null;

		public override string? Description { get; } = null;

		protected override ISet<UserStateType> RequiredStates { get; } = new HashSet<UserStateType>();

		public Unknown(IBotRepository botRepository) : base(botRepository)
		{ }

		public override async Task<CommandResultsInfo> ExecuteAsync(
			ITelegramBotClient botClient,
			User user,
			Update update,
			CancellationToken cancellationToken
		)
		{
			await botClient.SendTextMessageAsync(
				chatId: update.Message.Chat.Id,
				text: BotMessages.NotRecognizedCommand,
				replyMarkup: _botRepository.MenuMarkup,
				cancellationToken: cancellationToken
			);

			return new CommandResultsInfo
			{
				RequestInfo = update.Message.Text,
				ResponseInfo = BotMessages.NotRecognizedCommand
			};
		}

		public override void AddToMenu(BotMenuBuilder builder)
		{
			return;
		}
	}
}
