using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSheltersBot
{
	/// <summary>
	/// Результаты обработки команды.
	/// </summary>
	public class CommandResultsInfo
	{ 
		/// <summary>
		/// Информация о запросе пользователя.
		/// </summary>
		public string RequestInfo { get; set; }

		/// <summary>
		/// Информация об ответе пользователю.
		/// </summary>
		public string ResponseInfo { get; set; }
	}
}
