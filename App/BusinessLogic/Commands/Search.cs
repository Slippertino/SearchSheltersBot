using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using NLog.Fluent;
using SearchSheltersBot.BusinessLogic;

namespace SearchSheltersBot
{
	/// <summary>
	/// Обработчик команды /search.
	/// </summary>
	public class Search : Command
	{
		public override string? Name { get; } = "/search";

		public override string? Description { get; } = "Поиск укрытий";

		protected override ISet<UserStateType> RequiredStates { get; }
			= new HashSet<UserStateType>() { UserStateType.WAITING_LOCATION };

		public Search(IBotRepository botRepository) : base(botRepository)
		{ }

		public override async Task<CommandResultsInfo> ExecuteAsync(
			ITelegramBotClient botClient,
			User user,
			Update update,
			CancellationToken cancellationToken
		)
		{
			var state = await _botRepository.GetUserStateAsync(user.Id);
			var chatId = update.Message.Chat.Id;

			if (state == UserStateType.WAITING_LOCATION)
			{
				return await HandleWaitingLocationState(botClient, user, update, cancellationToken);
			}

			var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
			{
				KeyboardButton.WithRequestLocation(BotMessages.NeedLocationTitle)
			});

			await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: BotMessages.NeedLocationMessage,
				replyMarkup: replyKeyboardMarkup,
				cancellationToken: cancellationToken
			);

			await _botRepository.SetUserStateAsync(user.Id, UserStateType.WAITING_LOCATION);

			return new CommandResultsInfo
			{ 
				RequestInfo  = Name,
				ResponseInfo = BotMessages.NeedLocationMessage
			};
		}

		public override void AddToMenu(BotMenuBuilder builder) => builder.Add(Description, 0, 0);

		private async Task<CommandResultsInfo> HandleWaitingLocationState(
			ITelegramBotClient botClient,
			User user,
			Update update,
			CancellationToken cancellationToken
		)
		{
			var chatId = update.Message.Chat.Id;
			var loc = update.Message.Location;

			if (loc == null)
			{
				return await HandleNoRequiredLocation(botClient, chatId, user, cancellationToken);
			}

			var shelters = await _botRepository.FindNearbySheltersAsync(
				user.Id,
				loc.Latitude,
				loc.Longitude
			);

			if (shelters.Count() == 0)
			{
				return await HandleNoNearbyShelters(botClient, user, update, cancellationToken);
			}

			var nearest = shelters.First();

			var sheltersInfo = BotMessages.Combine(
				BotMessages.GetSheltersInfoMessage(shelters),
				BotMessages.GetNearestShelterDescriptionMessage(nearest)
			);

			var descMsg = await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: sheltersInfo,
				replyMarkup: _botRepository.MenuMarkup,
				cancellationToken: cancellationToken
			);

			var sendedLocation = await botClient.SendLocationAsync(
				chatId: chatId,
				latitude: nearest.Latitude,
				longitude: nearest.Longitude
			);

			await _botRepository.SetUserStateAsync(user.Id, UserStateType.FREED);

			return new CommandResultsInfo
			{
				RequestInfo = GetLocationDescription(loc),
				ResponseInfo = sheltersInfo
			};
		}

		private async Task<CommandResultsInfo> HandleNoNearbyShelters(
			ITelegramBotClient botClient,
			User user,
			Update update,
			CancellationToken cancellationToken
		)
		{
			await _botRepository.SetUserStateAsync(user.Id, UserStateType.FREED);

			await botClient.SendTextMessageAsync(
				chatId: update.Message.Chat.Id,
				text: BotMessages.NoNearbySheltersMessage,
				replyMarkup: _botRepository.MenuMarkup,
				cancellationToken: cancellationToken
			);

			return new CommandResultsInfo
			{
				RequestInfo = GetLocationDescription(update.Message.Location),
				ResponseInfo = BotMessages.NoNearbySheltersMessage
			};
		}

		private async Task<CommandResultsInfo> HandleNoRequiredLocation(
			ITelegramBotClient botClient, 
			ChatId chatId, 
			User user, 
			CancellationToken cancellationToken
		)
		{
			await _botRepository.SetUserStateAsync(user.Id, UserStateType.FREED);

			await botClient.SendTextMessageAsync(
				chatId: chatId,
				text: BotMessages.LocationExpectedErrorMessage,
				replyMarkup: _botRepository.MenuMarkup,
				cancellationToken: cancellationToken
			);

			return new CommandResultsInfo
			{
				RequestInfo = "неверные данные геолокации(Location = null)",
				ResponseInfo = BotMessages.LocationExpectedErrorMessage
			};
		}

		private string GetLocationDescription(Location loc)
		{
			return $"Геолокация по координатам: ({loc.Latitude}, {loc.Longitude})"; 
		}
	}
}