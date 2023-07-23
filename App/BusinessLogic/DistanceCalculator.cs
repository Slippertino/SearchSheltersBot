using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSheltersBot
{
	/// <summary>
	/// Вычисляет расстояние(м) по значениям ширины и долготы двух точек.
	/// Ссылка на источник:
	/// https://www.kobzarev.com/programming/calculation-of-distances-between-cities-on-their-coordinates/#formuly
	/// </summary>
	public static class DistanceCalculator
	{
		/// <summary>
		/// Радиус Земли(м).
		/// </summary>
		private const double kEarthRadiusM = 6372795.0;

		/// <summary>
		/// Основной метод.
		/// </summary>
		/// <param name="lt1"> Ширина 1ой точки </param>
		/// <param name="lg1"> Долгота 1ой точки </param>
		/// <param name="lt2"> Ширина 2ой точки </param>
		/// <param name="lg2"> Долгота 2ой точки </param>
		/// <returns></returns>
		public static int Calculate(double lt1, double lg1, double lt2, double lg2)
		{
			var lat1  = lt1 * Math.PI / 180;
			var lat2  = lt2 * Math.PI / 180;
			var long1 = lg1 * Math.PI / 180;
			var long2 = lg2 * Math.PI / 180;

			var cl1 = Math.Cos(lat1);
			var cl2 = Math.Cos(lat2);
			var sl1 = Math.Sin(lat1);
			var sl2 = Math.Sin(lat2);
			var delta = long2 - long1;
			var cdelta = Math.Cos(delta);
			var sdelta = Math.Sin(delta);

			var y = Math.Sqrt(Math.Pow(cl2 * sdelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cdelta, 2));
			var x = sl1 * sl2 + cl1 * cl2 * cdelta;

			var ad = Math.Atan2(y, x);
			var dist = (int)(ad * kEarthRadiusM);

			return dist;
		}
	}
}
