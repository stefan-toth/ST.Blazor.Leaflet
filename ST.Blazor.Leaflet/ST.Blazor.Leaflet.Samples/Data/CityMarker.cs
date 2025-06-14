using ST.Blazor.Leaflet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST.Blazor.Leaflet.Samples.Data
{
	public class CityMarker : Marker
	{
		public City City { get; }
		public CityMarker(City city) : base(city.Coordinates)
		{
			City = city;
			Icon = new Icon
			{
				Url = city.CoatOfArmsImageUrl,
				ClassName = "map-icon",
				Size = new System.Drawing.Size(108,179),
				Anchor = new System.Drawing.Point(108/2,179)
			};
			Tooltip = new Tooltip
			{
				Content = city.Name,
			};
			Popup = new Popup
			{
				Content = city.Description,
			};
		}
	}
}
