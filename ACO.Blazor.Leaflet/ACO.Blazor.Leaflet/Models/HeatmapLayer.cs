using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ACO.Blazor.Leaflet.Models
{
	public class HeatmapLayer : Layer
	{
		public HeatmapLayer(IEnumerable<HeatmapLatLng> points)
		{
			Points = points.ToList();
		}

		public List<HeatmapLatLng> Points { get; }

		public double MinOpacity { get; init; }
		public double MaxZoom { get; init; }
		public double MaxPointIntensity { get; init; } = 1.0;
		public int Radius { get; init; } = 25;
		public int Blur { get; init; } = 15;
		public IEnumerable<GradientPart> Gradient { get; init; }
	}

	public struct GradientPart
	{
		public GradientPart(double point, string color)
		{
			this.Point = point;
			this.Color = color;
		}

		public double Point { get; init; }
		public string Color { get; init; }
	}

	public class HeatmapLatLng : LatLng
	{
		public HeatmapLatLng(PointF position, float intensity = 1.0f) : base(position)
		{
			Alt = intensity;
		}

		public HeatmapLatLng(float lat, float lng, float intensity = 1.0f) : base(lat, lng, intensity)
		{
		}

		public double Intensity => Alt;
	}
}