using SearchSheltersBot.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SearchSheltersBot
{
	/// <summary>
	/// Обработчик команды /start.
	/// </summary>
	public class Start : Command
	{
		public override string? Name { get; } = "/start";

		public override string? Description { get; } = "Запустить";

		protected override ISet<UserStateType> RequiredStates { get; } = new HashSet<UserStateType>();
		
		public Start(IBotRepository botRepository) : base(botRepository)
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
				text: BotMessages.GreetingsMessage,
				replyMarkup: _botRepository.MenuMarkup,
				cancellationToken: cancellationToken
			);

			return new CommandResultsInfo
			{
				RequestInfo  = Name,
				ResponseInfo = BotMessages.GreetingsMessage
			};
		}

		public override void AddToMenu(BotMenuBuilder builder)
		{
			return;
		}
	}
}