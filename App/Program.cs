using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace SearchSheltersBot
{
    class Program
    {
		static string GetDbConnectionString()
		{
			return
				Environment.GetEnvironmentVariable("DbConn") ??
				System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
		}

		static string GetBotToken()
		{
			return
				Environment.GetEnvironmentVariable("BotToken") ??
				System.Configuration.ConfigurationManager.AppSettings["BotToken"];
		}

        static async Task Main(string[] args)
        {
			//NLog
			var config = new ConfigurationBuilder().Build();

			var logger = LogManager.Setup()
								   .SetupExtensions(ext => ext.RegisterConfigSettings(config))
								   .GetCurrentClassLogger();

			SearchSheltersBot bot = null;
			try
			{
				//Настройка сервисов.
				using var servicesProvider = new ServiceCollection()
					.AddTransient<SearchSheltersBot>()
					.AddDbContext<BotContext>(
						dbOptionsBuilder => dbOptionsBuilder.UseSqlServer(
							GetDbConnectionString()
						)
					)
					.AddLazyCache()
					.AddTransient<IBotRepository, BotRepository>()
					.AddSingleton<UsersSettingsCache>()
					.AddSingleton<SheltersCache>()
					.AddLogging(loggingBuilder =>
					{
						loggingBuilder.ClearProviders();
						loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
						loggingBuilder.AddNLog();
					}).BuildServiceProvider();

				//Создаем бота.
				bot = servicesProvider.GetRequiredService<SearchSheltersBot>();

				//Запускаем по токену из конфига.
				bot.Run(
					GetBotToken()
				);

				await Task.Delay(-1);
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Stopped program because of exception");
				throw;
			}
			finally
			{
				LogManager.Shutdown();
				bot?.Stop();
			}
		}
	}
}
