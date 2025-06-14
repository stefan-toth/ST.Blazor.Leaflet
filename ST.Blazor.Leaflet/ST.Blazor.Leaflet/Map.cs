using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ST.Blazor.Leaflet.Exceptions;
using ST.Blazor.Leaflet.Models;
using ST.Blazor.Leaflet.Models.Events;
using ST.Blazor.Leaflet.Utils;

namespace ST.Blazor.Leaflet
{
	public class Map
	{
		/// <summary>
		/// Initial geographic center of the map
		/// </summary>
		public LatLng Center { get; set; } = new LatLng();

		/// <summary>
		/// Initial map zoom level
		/// </summary>
		public float Zoom { get; set; }

        /// <summary>
        /// Whether the map can be zoomed by using the mouse wheel. 
        /// <para/>
        /// Defaults to true.
        /// </summary>
        public bool ScrollWheelZoom { get; set; } = true;

        /// <summary>
        /// Whether the map can be zoomed by touch-dragging with two fingers. 
        /// <para/>
        /// Defaults to true.
        /// </summary>
        public bool TouchZoom { get; set; } = true;

        /// <summary>
        /// Whether the map is draggable with mouse/touch or not.
        /// <para/>
        /// Defaults to true.
        /// </summary>
        public bool Dragging { get; set; } = true;

        /// <summary>
        /// Map bounds
        /// </summary>
        public Bounds Bounds { get; private set; }

		/// <summary>
		/// Minimum zoom level of the map. If not specified and at least one 
		/// GridLayer or TileLayer is in the map, the lowest of their minZoom
		/// options will be used instead.
		/// </summary>
		public float? MinZoom { get; set; }

		/// <summary>
		/// Maximum zoom level of the map. If not specified and at least one
		/// GridLayer or TileLayer is in the map, the highest of their maxZoom
		/// options will be used instead.
		/// </summary>
		public float? MaxZoom { get; set; }

		/// <summary>
		/// When this option is set, the map restricts the view to the given
		/// geographical bounds, bouncing the user back if the user tries to pan
		/// outside the view.
		/// </summary>
		public LatLngBounds MaxBounds { get; set; }

        /// <summary>
        /// Whether a attribution control is added to the map by default.
        /// <para/>
        /// Defaults to true.
        /// </summary>
        public bool AttributionControl { get; set; } = true;

        /// <summary>
        /// Whether a zoom control is added to the map by default.
        /// <para/>
        /// Defaults to true.
        /// </summary>
        public bool ZoomControl { get; set; } = true;

        /// <summary>
        /// Whether a layers control is added to the map by default.
        /// <para/>
        /// Defaults to false.
        /// </summary>
        public bool LayersControl { get; set; } = false;

        /// <summary>
        /// Event raised when the component has finished its first render.
        /// </summary>
        public event Action OnInitialized;

		public string Id { get; }

		private readonly ObservableCollection<Layer> _layers = new ObservableCollection<Layer>();

		private readonly IJSRuntime _jsRuntime;

		public bool IsInitialized { get; private set; }

		public Map(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
			Id = StringHelper.GetRandomString(10);

			_layers.CollectionChanged += OnLayersChanged;
		}

		/// <summary>
		/// This method MUST be called only once by the Blazor component upon rendering, and never by the user.
		/// </summary>
		public void RaiseOnInitialized()
		{
			IsInitialized = true;
			OnInitialized?.Invoke();
			RunTaskInBackground(UpdateBounds);
		}

		private async void RunTaskInBackground(Func<Task> task)
		{
			try
			{
				await task();
			}
			catch (Exception ex)
			{
				NotifyBackgroundExceptionOccurred(ex);
			}
		}

		/// <summary>
		/// Add a layer to the map.
		/// </summary>
		/// <param name="layer">The layer to be added.</param>
		/// <exception cref="System.ArgumentNullException">Throws when the layer is null.</exception>
		/// <exception cref="UninitializedMapException">Throws when the map has not been yet initialized.</exception>
		public void AddLayer(Layer layer)
		{
			if (layer is null)
			{
				throw new ArgumentNullException(nameof(layer));
			}

			if (!IsInitialized)
			{
				throw new UninitializedMapException();
			}

			_layers.Add(layer);
		}

        public ValueTask OpenMarkerPopup(Marker marker) => LeafletInterops.OpenLayerPopup(_jsRuntime, Id, marker);

		/// <summary>
		/// Remove a layer from the map.
		/// </summary>
		/// <param name="layer">The layer to be removed.</param>
		/// <exception cref="System.ArgumentNullException">Throws when the layer is null.</exception>
		/// <exception cref="UninitializedMapException">Throws when the map has not been yet initialized.</exception>
		public void RemoveLayer(Layer layer)
		{
			if (layer is null)
			{
				throw new ArgumentNullException(nameof(layer));
			}

			if (!IsInitialized)
			{
				throw new UninitializedMapException();
			}

			_layers.Remove(layer);
		}

		public void RemoveAllLayersOfType<TLayer>() where TLayer : Layer
		{
			if (!IsInitialized)
			{
				throw new UninitializedMapException();
			}

			var rm = _layers.Where(t => IsSameOrSubclass<TLayer>(t)).ToArray();
			foreach (var layer in rm)
			{
				_layers.Remove(layer);
			}
		}

		private static bool IsSameOrSubclass<TType>(object o)
		{
			var type = o.GetType();
			var ttype = typeof(TType);
			return type == ttype || type.IsSubclassOf(ttype);
		}

		/// <summary>
		/// Get a read only collection of the current layers.
		/// </summary>
		/// <returns>A read only collection of layers.</returns>
		public IReadOnlyCollection<Layer> GetLayers()
		{
			return _layers.ToList().AsReadOnly();
		}

		private async void OnLayersChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (var item in args.NewItems)
				{
					var layer = item as Layer;
					await LeafletInterops.AddLayer(_jsRuntime, Id, layer);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (var item in args.OldItems)
				{
					if (item is Layer layer)
					{
						await LeafletInterops.RemoveLayer(_jsRuntime, Id, layer.Id);
					}
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Replace
			         || args.Action == NotifyCollectionChangedAction.Move)
			{
				foreach (var oldItem in args.OldItems)
					if (oldItem is Layer layer)
						await LeafletInterops.RemoveLayer(_jsRuntime, Id, layer.Id);

				foreach (var newItem in args.NewItems)
					await LeafletInterops.AddLayer(_jsRuntime, Id, newItem as Layer);
			}
		}

		public ValueTask FitBounds(PointF corner1, PointF corner2, PointF? padding = null, float? maxZoom = null)
		{
			return LeafletInterops.FitBounds(_jsRuntime, Id, corner1, corner2, padding, maxZoom);
		}

		public ValueTask FitBounds(Bounds bounds, PointF? padding = null, float? maxZoom = null)
		{
			return LeafletInterops.FitBounds(_jsRuntime, Id, new PointF(bounds.NorthEast.Lat, bounds.NorthEast.Lng),
				new PointF(bounds.SouthWest.Lat, bounds.SouthWest.Lng), padding, maxZoom);
		}


		public ValueTask<Bounds> GetBoundsFromMarkers(IEnumerable<Marker> markers)
			=> LeafletInterops.GetBoundsFromMarkers(_jsRuntime, markers);

		public async ValueTask FitBounds(IEnumerable<Marker> markers, PointF? padding = null, float? maxZoom = null)
		{
			var bounds = await GetBoundsFromMarkers(markers);
			await FitBounds(bounds, padding, maxZoom);
		}

		public ValueTask PanTo(PointF position, bool animate = false, float duration = 0.25f,
			float easeLinearity = 0.25f,
			bool noMoveStart = false)
		{
			return LeafletInterops.PanTo(_jsRuntime, Id, position, animate, duration, easeLinearity, noMoveStart);
		}

		public async Task<LatLng> GetCenter() => await LeafletInterops.GetCenter(_jsRuntime, Id);
		public async Task<float> GetZoom() => await LeafletInterops.GetZoom(_jsRuntime, Id);
		public async Task<Bounds> GetBounds() => await LeafletInterops.GetBounds(_jsRuntime, Id);


		private async Task UpdateBounds()
		{
			Bounds = await GetBounds();
			Center = await GetCenter();
			Zoom = await GetZoom();
			OnBoundsChanged?.Invoke(this, new EventArgs());
		}

		/// <summary>
		/// Increases the zoom level by one notch.
		/// 
		/// If <c>shift</c> is held down, increases it by three.
		/// </summary>
		public async Task ZoomIn(MouseEventArgs e) => await LeafletInterops.ZoomIn(_jsRuntime, Id, e);

		/// <summary>
		/// Decreases the zoom level by one notch.
		/// 
		/// If <c>shift</c> is held down, decreases it by three.
		/// </summary>
		public async Task ZoomOut(MouseEventArgs e) => await LeafletInterops.ZoomOut(_jsRuntime, Id, e);

		#region events

		public delegate void MapEventHandler(object sender, Event e);

		public delegate void MapResizeEventHandler(object sender, ResizeEvent e);

		public event MapEventHandler OnZoomLevelsChange;

		[JSInvokable]
		public void NotifyZoomLevelsChange(Event e) => OnZoomLevelsChange?.Invoke(this, e);

		public event MapResizeEventHandler OnResize;

		[JSInvokable]
		public void NotifyResize(ResizeEvent e) => OnResize?.Invoke(this, e);

		public event MapEventHandler OnUnload;

		[JSInvokable]
		public void NotifyUnload(Event e) => OnUnload?.Invoke(this, e);

		public event MapEventHandler OnViewReset;

		[JSInvokable]
		public void NotifyViewReset(Event e) => OnViewReset?.Invoke(this, e);

		public event MapEventHandler OnLoad;

		[JSInvokable]
		public void NotifyLoad(Event e) => OnLoad?.Invoke(this, e);

		public event MapEventHandler OnZoomStart;

		[JSInvokable]
		public void NotifyZoomStart(Event e) => OnZoomStart?.Invoke(this, e);

		public event MapEventHandler OnMoveStart;

		[JSInvokable]
		public void NotifyMoveStart(Event e) => OnMoveStart?.Invoke(this, e);

		public event MapEventHandler OnZoom;

		[JSInvokable]
		public void NotifyZoom(Event e) => OnZoom?.Invoke(this, e);

		public event MapEventHandler OnMove;

		[JSInvokable]
		public void NotifyMove(Event e) => OnMove?.Invoke(this, e);

		public event MapEventHandler OnZoomEnd;

		[JSInvokable]
		public async void NotifyZoomEnd(Event e)
		{
			try
			{
				await UpdateBounds();
			}
			catch (Exception ex)
			{
				NotifyBackgroundExceptionOccurred(ex);
			}
			finally
			{
				OnZoomEnd?.Invoke(this, e);
			}
		}

		public event MapEventHandler OnMoveEnd;

		[JSInvokable]
		public async void NotifyMoveEnd(Event e)
		{
			try
			{
				await UpdateBounds();
			}
			catch (Exception ex)
			{
				NotifyBackgroundExceptionOccurred(ex);
			}
			finally
			{
				OnMoveEnd?.Invoke(this, e);
			}
		}

		public event EventHandler OnBoundsChanged;

		public event MouseEventHandler OnMouseMove;

		[JSInvokable]
		public void NotifyMouseMove(MouseEvent eventArgs) => OnMouseMove?.Invoke(this, eventArgs);

		public event MapEventHandler OnKeyPress;

		[JSInvokable]
		public void NotifyKeyPress(Event eventArgs) => OnKeyPress?.Invoke(this, eventArgs);

		public event MapEventHandler OnKeyDown;

		[JSInvokable]
		public void NotifyKeyDown(Event eventArgs) => OnKeyDown?.Invoke(this, eventArgs);

		public event MapEventHandler OnKeyUp;

		[JSInvokable]
		public void NotifyKeyUp(Event eventArgs) => OnKeyUp?.Invoke(this, eventArgs);

		public event MouseEventHandler OnPreClick;

		[JSInvokable]
		public void NotifyPreClick(MouseEvent eventArgs) => OnPreClick?.Invoke(this, eventArgs);

		public event EventHandler<Exception> BackgroundExceptionOccurred;

		private void NotifyBackgroundExceptionOccurred(Exception exception) =>
			BackgroundExceptionOccurred?.Invoke(this, exception);

		#endregion events

		#region InteractiveLayerEvents

		// Has the same events as InteractiveLayer, but it is not a layer. 
		// Could place this code in its own class and make Layer inherit from that, but not every layer is interactive...
		// Is there a way to not duplicate this code?

		public delegate void MouseEventHandler(Map sender, MouseEvent e);

		public event MouseEventHandler OnClick;

		[JSInvokable]
		public void NotifyClick(MouseEvent eventArgs) => OnClick?.Invoke(this, eventArgs);

		public event MouseEventHandler OnDblClick;

		[JSInvokable]
		public void NotifyDblClick(MouseEvent eventArgs) => OnDblClick?.Invoke(this, eventArgs);

		public event MouseEventHandler OnMouseDown;

		[JSInvokable]
		public void NotifyMouseDown(MouseEvent eventArgs) => OnMouseDown?.Invoke(this, eventArgs);

		public event MouseEventHandler OnMouseUp;

		[JSInvokable]
		public void NotifyMouseUp(MouseEvent eventArgs) => OnMouseUp?.Invoke(this, eventArgs);

		public event MouseEventHandler OnMouseOver;

		[JSInvokable]
		public void NotifyMouseOver(MouseEvent eventArgs) => OnMouseOver?.Invoke(this, eventArgs);

		public event MouseEventHandler OnMouseOut;

		[JSInvokable]
		public void NotifyMouseOut(MouseEvent eventArgs) => OnMouseOut?.Invoke(this, eventArgs);

		public event MouseEventHandler OnContextMenu;

		[JSInvokable]
		public void NotifyContextMenu(MouseEvent eventArgs) => OnContextMenu?.Invoke(this, eventArgs);

		#endregion InteractiveLayerEvents
	}
}