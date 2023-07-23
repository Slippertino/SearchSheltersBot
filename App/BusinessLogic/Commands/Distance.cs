using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using SearchSheltersBot.BusinessLogic;

namespace SearchSheltersBot
{
	/// <summary>
	/// Обработчик команды /distance.
	/// </summary>
	public class Distance : Command
	{
		public override string? Name { get; } = "/distance";

		public override string? Description { get; } = "Задать дистанцию";

		protected override ISet<UserStateType> RequiredStates { get; }
			= new HashSet<UserStateType>() { UserStateType.WAITING_DISTANCE };

		private static InlineKeyboardMarkup DistanceKeyboardMarkup = new InlineKeyboardMarkup(new[]
		{
			new []
			{
				InlineKeyboardButton.WithCallbackData(text: "до 100м",    callbackData: "0 100"),
				InlineKeyboardButton.WithCallbackData(text: "100-200м",   callbackData: "100 200"),
				InlineKeyboardButton.WithCallbackData(text: "300-500м",   callbackData: "300 500"),
				InlineKeyboardButton.WithCallbackData(text: "100-500м",   callbackData: "100 500"),
			},
			new []
			{
				InlineKeyboardButton.WithCallbackData(text: "500-1000м",  callbackData: "500 1000"),
				InlineKeyboardButton.WithCallbackData(text: "1-2км",      callbackData: "1000 2000"),
				InlineKeyboardButton.WithCallbackData(text: "2-3км",      callbackData: "2000 3000"),
				InlineKeyboardButton.WithCallbackData(text: "больше 3км", callbackData: "3000 1000000"),
			},
		});

		public Distance(IBotRepository botRepository) : base(botRepository)
		{ }

		public override async Task<CommandResultsInfo> ExecuteAsync(
			ITelegramBotClient botClient, 
			User user, 
			Update update, 
			CancellationToken cancellationToken
		)
		{
			var state = await _botRepository.GetUserStateAsync(user.Id);

			if (state == UserStateType.WAITING_DISTANCE)
			{
				return await HandleWaitingDistanceState(botClient, user, update, cancellationToken);
			}

			var chatId = update.Message.Chat.Id;

			await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: BotMessages.NeedDistanceRangeMessage,
				replyMarkup: DistanceKeyboardMarkup,
				cancellationToken: cancellationToken
			);

			await _botRepository.SetUserStateAsync(user.Id, UserStateType.WAITING_DISTANCE);

			return new CommandResultsInfo
			{
				RequestInfo  = Name,
				ResponseInfo = BotMessages.NeedDistanceRangeMessage
			};
		}

		public override void AddToMenu(BotMenuBuilder builder) => builder.Add(Description, 1, 0);

		private async Task<CommandResultsInfo> HandleWaitingDistanceState(
			ITelegramBotClient botClient,
			User user,
			Update update,
			CancellationToken cancellationToken
		)
		{
			var chatId = update.CallbackQuery.Message.Chat.Id;

			var values = update.CallbackQuery.Data
				.Split(' ')
				.Select(x => int.Parse(x))
				.ToList();

			await _botRepository.SetSearchRadiusRangeAsync(user.Id, values[0], values[1]);
			await _botRepository.SetUserStateAsync(user.Id, UserStateType.FREED);

			await botClient.EditMessageTextAsync(
				chatId: chatId,
				messageId: update.CallbackQuery.Message.MessageId,
				text: BotMessages.SettingsUpdatedMessage,
				cancellationToken: cancellationToken
			);

			return new CommandResultsInfo
			{
				RequestInfo = update.CallbackQuery.Message.Text,
				ResponseInfo = BotMessages.SettingsUpdatedMessage
			};
		}
	}
}