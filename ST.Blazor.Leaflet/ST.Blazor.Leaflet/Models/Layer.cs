﻿using ST.Blazor.Leaflet.Models.Events;
using ST.Blazor.Leaflet.Utils;
using Microsoft.JSInterop;

namespace ST.Blazor.Leaflet.Models
{
	public abstract class Layer
	{
		/// <summary>
		/// Unique identifier used by the interoperability service on the client side to identify layers.
		/// </summary>
		public string Id { get; }

        /// <summary>
        /// The name of the tile layer. This is used to identify the layer in the map.
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// By default the layer will be added to the map's overlay pane. Overriding this option will cause the layer to be placed on another pane by default.
        /// </summary>
        public virtual string Pane { get; set; } = "overlayPane";

		/// <summary>
		/// String to be shown in the attribution control, e.g. "© OpenStreetMap contributors". It describes the layer data and is often a legal obligation towards copyright holders and tile providers.
		/// </summary>
		public string Attribution { get; set; }

		/// <summary>
		/// The tooltip assigned to this marker.
		/// </summary>
		public Tooltip Tooltip { get; set; }

		/// <summary>
		/// The popup shown when the marker is clicked.
		/// </summary>
		public Popup Popup { get; set; }

		protected Layer()
		{
			Id = StringHelper.GetRandomString(20);
		}

		#region events

		public delegate void EventHandler(Layer sender, Event e);

		public event EventHandler OnAdd;

		[JSInvokable]
		public void NotifyAdd(Event eventArgs)
		{
			OnAdd?.Invoke(this, eventArgs);
		}

		public event EventHandler OnRemove;

		[JSInvokable]
		public void NotifyRemove(Event eventArgs)
		{
			OnRemove?.Invoke(this, eventArgs);
		}

		public delegate void PopupEventHandler(Layer sender, PopupEvent e);

		public event PopupEventHandler OnPopupOpen;

		[JSInvokable]
		public void NotifyPopupOpen(PopupEvent eventArgs)
		{
			OnPopupOpen?.Invoke(this, eventArgs);
		}

		public event PopupEventHandler OnPopupClose;

		[JSInvokable]
		public void NotifyPopupClose(PopupEvent eventArgs)
		{
			OnPopupClose?.Invoke(this, eventArgs);
		}

		public delegate void TooltipEventHandler(Layer sender, TooltipEvent e);

		public event TooltipEventHandler OnTooltipOpen;

		[JSInvokable]
		public void NotifyTooltipOpen(TooltipEvent eventArgs)
		{
			OnTooltipOpen?.Invoke(this, eventArgs);
		}

		public event TooltipEventHandler OnTooltipClose;

		[JSInvokable]
		public void NotifyTooltipClose(TooltipEvent eventArgs)
		{
			OnTooltipClose?.Invoke(this, eventArgs);
		}

		#endregion
	}
}
