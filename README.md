<div align="center">  
    <img src="ST.Blazor.Leaflet/ST.Blazor.Leaflet/logo.png" width=100 />
    <h1>ST.Blazor.Leaflet</h1>
    <div>
        <a href="#description">Description</a> •
        <a href="#installation">Installation</a> •
        <a href="#samples">Samples</a>
    </div>
</div>

# License

This is a fork of the project [ACO.Blazor.Leaflet](https://github.com/ACOAhlmann/ACO.Blazor.Leaflet) from [ACO Ahlmann SE & Co. KG](https://github.com/ACOAhlmann) and [BlazorLeafet](https://github.com/Mehigh17/BlazorLeaflet) from [Mehigh17](https://github.com/Mehigh17). 
The License itself can be found [here (MIT License)](LICENSE).

# Description

ST.Blazor.Leaflet is a wrapper offering easy-to-use Blazor components that expose the <a href="https://leafletjs.com/">Leaflet API</a> in C#. It allows you to create easily customizable maps without getting outside your existing .NET ecosystem.

The wrapper is still in its early days so it's very lackluster and doesn't expose the entirety of leaflet's API.

Check out the samples project to learn how to use it.

<img src="media/example1.gif" height=400>

# Installation

Install the package in the target project from [NuGet](https://www.nuget.org/packages/ST.Blazor.Leaflet):

```
dotnet add package ST.Blazor.Leaflet
```

In your `App.razor`/`_Host.cshtml`/`_Layout.cshtml` (Blazor Web App / Server) or `index.html` (Blazor WebAssembly), reference the interoperability script in the `<head>` element like so:

```html
<!-- ST.Blazor.Leaflet -->
<script src="_content/ST.Blazor.Leaflet/leafletBlazorInterops.js"></script>
<script type="module" src="_content/ST.Blazor.Leaflet/leafletBlazorInterops.js"></script>
```

You can now use the components and the rest of the library.

# Samples

Create the map

```html
@using ST.Blazor.Leaflet
@using ST.Blazor.Leaflet.Models
@inject IJSRuntime jsRuntime

<!-- You must wrap the map component in a container setting its actual size. -->
<div id="mapContainer" style="width: 300px; height: 300px;">
    <LeafletMap Map="_map" />
</div>
```

Bind the parameters to the respective objects like so

```cs
private Map _map = new Map(jsRuntime);
private PointF _startAt = new PointF(47.5574007f, 16.3918687f);
```

Add a marker with a tooltip and an icon

```cs
// Create the marker
var marker = new Marker(0.23f, 32f);
marker.Tooltip = new Tooltip { Content = "This is a nice location!" };
marker.Icon = new Icon { Url = "... some url" };

// Add it to the layers collection
_map.AddLayer(marker);
```

Or add a rectangle that highlights a zone

```cs
var rect = new Rectangle { Shape = new RectangleF(21f, 20f, 10f, 20f) };
rect.Fill = true; // This will fill the rectangle with a color
rect.FillColor = Color.Red; // Make the filled area red
rect.Popup = new Popup { Content = "This is a restricted area!" }; // Create a popup when the area is clicked

// Add it to the layers collection
_map.RemoveAdd(rect);
```

Or fit bounds on certain corners

```cs
_map.FitBounds(new PointF(45.943f, 24.967f), new PointF(46.943f, 25.967f), maxZoom: 5f);
```
