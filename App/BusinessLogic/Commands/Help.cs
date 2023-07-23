using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using SearchSheltersBot.BusinessLogic;

namespace SearchSheltersBot
{
	/// <summary>
	/// Обработчик команды /help.
	/// </summary>
	public class Help : Command
	{
		public override string? Name { get; } = "/help";

		public override string? Description { get; } = "Помощь";

		protected override ISet<UserStateType> RequiredStates { get; } = new HashSet<UserStateType>();

		public Help(IBotRepository botRepository) : base(botRepository)
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
				text: BotMessages.HelpMessage,
				replyMarkup: _botRepository.MenuMarkup,
				cancellationToken: cancellationToken
			);

			return new CommandResultsInfo
			{
				RequestInfo  = Name,
				ResponseInfo = BotMessages.HelpMessage
			};
		}

		public override void AddToMenu(BotMenuBuilder builder) => builder.Add(Description, 1, 1);
	}
}