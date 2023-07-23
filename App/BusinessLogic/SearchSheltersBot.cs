using Microsoft.Extensions.Logging;
using SearchSheltersBot.BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SearchSheltersBot
{
	/// <summary>
	/// Сущность бота.
	/// </summary>
	public class SearchSheltersBot
	{
		/// <summary>
		/// Сущность бота-пользователя.
		/// </summary>
		public User? Bot { get; private set; }

		/// <summary>
		/// Репозиторий для работы с данными.
		/// </summary>
		private readonly IBotRepository _botRepository;

		/// <summary>
		/// Логгер.
		/// </summary>
		private readonly ILogger<SearchSheltersBot> _logger;

		/// <summary>
		/// Список обрабатываемых команд.
		/// </summary>
		private readonly List<Command> _commands;

		/// <summary>
		/// Клиент бота.
		/// </summary>
		private TelegramBotClient _botClient;

		/// <summary>
		/// Флаг останова.
		/// </summary>
		private CancellationTokenSource _cancelTool = new CancellationTokenSource();

		/// <summary>
		/// Конструктор DI с двумя параметрами.
		/// </summary>
		/// <param name="botRepository"></param>
		/// <param name="logger"></param>
		public SearchSheltersBot(IBotRepository botRepository, ILogger<SearchSheltersBot> logger)
		{
			_botRepository = botRepository;
			_logger = logger;

			_commands = new List<Command>() 
			{
				new Start(_botRepository),
				new Help(_botRepository),
				new Search(_botRepository),
				new Distance(_botRepository),
			};

			var menuBuilder = new BotMenuBuilder();

			foreach(var command in _commands)
			{
				command.AddToMenu(menuBuilder);
			}

			_botRepository.MenuMarkup = menuBuilder.Build();
		}
		
		/// <summary>
		/// Запускает бота.
		/// </summary>
		/// <param name="botToken"> Токен бота </param>
		public async void Run(string botToken)
		{
			if (_cancelTool.Token.CanBeCanceled)
			{
				_cancelTool.Cancel();
				_cancelTool = new CancellationTokenSource();
			}

			_botClient = new TelegramBotClient(botToken);

			ReceiverOptions receiverOptions = new ReceiverOptions
			{
				AllowedUpdates = Array.Empty<UpdateType>()
			};

			_botClient.StartReceiving(
				updateHandler: HandleUpdateAsync,
				pollingErrorHandler: HandlePollingErrorAsync,
				receiverOptions: receiverOptions,
				cancellationToken: _cancelTool.Token
			);
			
			Bot = await _botClient.GetMeAsync();

			_logger.LogInformation($"Successful run for @{Bot.Username} id = @{Bot.Id}");
		}

		/// <summary>
		/// Останавливает бота.
		/// </summary>
		public void Stop()
		{
			_cancelTool.Cancel();
			_logger.LogInformation($"Successful stop for @{Bot.Username} id = @{Bot.Id}");
		}

		/// <summary>
		/// Обработка входящего сообщения.
		/// </summary>
		/// <param name="botClient"></param>
		/// <param name="update"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			try
			{
				var info = ExtractInfo(update);

				await _botRepository.EnsureUserCreatedAsync(info.user.Id);

				var command = await ChooseCommandAsync(info.user, info.msg);

				var result = await command.ExecuteAsync(botClient, info.user, update, cancellationToken);

				_logger.LogInformation(
					$"Ответ пользователю @{info.user.Username} с запросом \"{result.RequestInfo}\": " +
					$"\"{result.ResponseInfo}\""
				);
			}
			catch(Exception ex)
			{
				_logger.LogError($"Ошибка в ходе обработки сообщения: {ex}");
			}
		}

		/// <summary>
		/// Обработка ошибки.
		/// </summary>
		/// <param name="botClient"></param>
		/// <param name="exception"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			var ErrorMessage = exception switch
			{
				ApiRequestException apiRequestException
					=> $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};

			_logger.LogError(ErrorMessage);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Выбор обработчика команды.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		private async Task<Command> ChooseCommandAsync(User user, Message msg)
		{
			var state = await _botRepository.GetUserStateAsync(user.Id);

			var command =  _commands.FirstOrDefault(c => c.IsSuitToExecute(msg, state));

			return command ?? new Unknown(_botRepository);
		}

		/// <summary>
		/// Извлекает информацию об отправителе и сообщении из объекта.
		/// </summary>
		/// <param name="update"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		private (User user, Message msg) ExtractInfo(Update update)
		{
			switch(update.Type)
			{
				case UpdateType.Message:
					return (update.Message.From, update.Message);
				case UpdateType.CallbackQuery:
					return (update.CallbackQuery.From, update.CallbackQuery.Message);
				default:
					throw new ArgumentException("Неизвестный тип сообщения!");
			}
		}
	}
}