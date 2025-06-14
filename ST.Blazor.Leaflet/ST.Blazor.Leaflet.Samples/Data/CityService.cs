using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ST.Blazor.Leaflet.Samples.Data
{
	public class CityService
	{
		private readonly List<City> _cities = new()
		{
			new City
			{
				CoatOfArmsImageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/19/ROU_Bucharest_CoA.png",
				Name = "Bucharest",
				Country = "Romania",
				Description = "Bucharest is the capital of Romania, also called <b>București</b> in romanian.",
				Coordinates = new PointF(44.4268f, 26.1025f),
			},
			new City
			{
				CoatOfArmsImageUrl = "https://upload.wikimedia.org/wikipedia/commons/f/fa/Grand_CoA_Warsaw.png",
				Name = "Warsaw",
				Country = "Poland",
				Description = "Warsaw is the capital of Poland, also called <b>Warszawa</b> in polish.",
				Coordinates = new PointF(52.2297f, 21.0122f),
			},
			new City
			{
				CoatOfArmsImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/da/Oslo_komm.svg/1200px-Oslo_komm.svg.png",
				Name = "Oslo",
				Country = "Norway",
				Description = "Oslo is the capital of Norway, also called <b>Oslo</b> in norwegian.",
				Coordinates = new PointF(59.9139f, 10.7522f),
			},
			new City()
			{
				CoatOfArmsImageUrl =
					"https://upload.wikimedia.org/wikipedia/commons/thumb/3/3d/Buedelsdorf_Wappen.png/140px-Buedelsdorf_Wappen.png",
				Name = "Büdelsdorf",
				Country = "Germany",
				Description = "Büdelsdorf (<i>Danish: Bydelstorp</i>) is a town in the district of Rendsburg-Eckernförde, " +
				              "in Schleswig-Holstein, Germany.<br/>Also the headquarter of the very good company ACO is here. ",
				Coordinates = new PointF(54.316667f, 9.683333f),
			},
		};

		public IEnumerable<City> FindCities(string? name)
			=> name is null
				? _cities
				: _cities.Where(c => c.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));
	}
}