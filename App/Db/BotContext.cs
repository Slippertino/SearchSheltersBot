using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Telegram.Bot.Types;

namespace SearchSheltersBot
{
	/// <summary>
	/// Сущность для работы с БД.
	/// </summary>
	public class BotContext : DbContext
	{
		/// <summary>
		/// Таблица Shelters в БД.
		/// </summary>
		public DbSet<Shelter> Shelters { get; set; } = null!;

		/// <summary>
		/// Таблица UserSettings в БД.
		/// </summary>
		public DbSet<UserSettings> UserSettings { get; set; } = null!;

		/// <summary>
		/// Конструктор с одним параметром.
		/// </summary>
		/// <param name="options"> Опции соеднинения(установлены в Main через DI) </param>
		public BotContext(DbContextOptions options) : base(options)
		{
			Database.EnsureCreated();

			using (var stream = new StreamReader(new FileStream("init.sql", FileMode.Open)))
			using(var conn = new SqlConnection(Database.GetConnectionString()))
			{
				conn.Open();
				var command = new SqlCommand(stream.ReadToEnd(), conn);
				command.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Обработчик события создания таблиц в БД по моделям.
		/// </summary>
		/// <param name="modelBuilder"></param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Shelter>(sh =>
			{
				sh.HasKey(s => s.Id);
				sh.Ignore(s => s.Distance);
				sh.HasAlternateKey(s => s.Address);
				sh.HasAlternateKey(s => new { s.Latitude, s.Longitude });
			});

			modelBuilder.Entity<UserSettings>(us =>
			{
				us.HasKey(u => u.Id);
				us.Property(u => u.Id).ValueGeneratedNever();
			});
		}
	}
}
