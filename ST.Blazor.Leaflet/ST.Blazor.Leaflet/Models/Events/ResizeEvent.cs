using System.Drawing;

namespace ST.Blazor.Leaflet.Models.Events
{
	public class ResizeEvent : Event
	{
		public PointF OldSize { get; set; }
		public PointF NewSize { get; set; }
	}
}
