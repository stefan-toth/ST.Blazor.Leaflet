using System.Drawing;

namespace ST.Blazor.Leaflet.Models.Events
{
	public class MouseEvent : Event
	{
		public LatLng LatLng { get; set; }

		public PointF LayerPoint { get; set; }

		public PointF ContainerPoint { get; set; }

	}
}
