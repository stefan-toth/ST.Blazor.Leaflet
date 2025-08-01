import "/_content/ST.Blazor.Leaflet/leaflet/leaflet.js";
import "/_content/ST.Blazor.Leaflet/leaflet/Leaflet.ImageOverlay.Rotated.js";
import "/_content/ST.Blazor.Leaflet/leaflet/leaflet-heat.js";

export const maps = {};
export const layers = {};
export const layerControls = {};

window.leafletBlazor = {
    create: function (map, objectReference) {
        var leafletMap = L.map(map.id, {
            center: map.center,
            zoom: map.zoom,
            scrollWheelZoom: map.scrollWheelZoom,
            touchZoom: map.touchZoom,
            dragging: map.dragging,
            attributionControl: map.attributionControl,
            zoomControl: map.zoomControl,           
            minZoom: map.minZoom ? map.minZoom : undefined,
            maxZoom: map.maxZoom ? map.maxZoom : undefined,
            maxBounds: map.maxBounds && map.maxBounds.northEast && map.maxBounds.southWest ? L.latLngBounds(map.maxBounds.northEast, map.maxBounds.southWest) : undefined,
        });

        connectMapEvents(leafletMap, objectReference);
        maps[map.id] = leafletMap;
        layers[map.id] = [];

        // If enabled, add a layer control to the map
        if (map.layersControl)
            this.addLayerControl(map.id);
    },
    addLayerControl: function (mapId) {
        if (!(mapId in layerControls)) {
            layerControls[mapId] = L.control.layers().addTo(maps[mapId]);
        }
    },
    removeLayerControl: function (mapId) {
        if (mapId in layerControls) {
            maps[map.id].removeLayer(layerControls[mapId]);
            delete layerControls[mapId];
        }
    },
    addTilelayer: function (mapId, tileLayer, objectReference) {
        const layer = L.tileLayer(tileLayer.urlTemplate, {
            attribution: tileLayer.attribution,
            pane: tileLayer.pane,
            // ---
            tileSize: tileLayer.tileSize ? L.point(tileLayer.tileSize.width, tileLayer.tileSize.height) : undefined,
            opacity: tileLayer.opacity,
            updateWhenZooming: tileLayer.updateWhenZooming,
            updateInterval: tileLayer.updateInterval,
            zIndex: tileLayer.zIndex,
            bounds: tileLayer.bounds && tileLayer.bounds.northEast && tileLayer.bounds.southWest ? L.latLngBounds(tileLayer.bounds.northEast, tileLayer.bounds.southWest) : undefined,
            // ---
            minZoom: tileLayer.minimumZoom,
            maxZoom: tileLayer.maximumZoom,
            subdomains: tileLayer.subdomains,
            errorTileUrl: tileLayer.errorTileUrl,
            zoomOffset: tileLayer.zoomOffset,
            // TMS
            zoomReverse: tileLayer.isZoomReversed,
            detectRetina: tileLayer.detectRetina,
            // crossOrigin
        });

        // Add layer to the map if it is not already added
        if (!(mapId in layers) || layers[mapId].length === 0) {
            addLayer(mapId, layer, tileLayer.id);
        }

        // Add layer to the layer control for switching between layers
        if (mapId in layerControls)
            layerControls[mapId].addBaseLayer(layer, tileLayer.name);
    },
    addMbTilesLayer: function (mapId, mbTilesLayer, objectReference) {
        const layer = L.tileLayer.mbTiles(mbTilesLayer.urlTemplate, {
            attribution: mbTilesLayer.attribution,
            minZoom: mbTilesLayer.minimumZoom,
            maxZoom: mbTilesLayer.maximumZoom
        });

        // Add layer to the map if it is not already added
        if (!(mapId in layers) || layers[mapId].length === 0) {
            addLayer(mapId, layer, tileLayer.id);
        }

        // Add layer to the layer control for switching between layers
        if (mapId in layerControls)
            layerControls[mapId].addBaseLayer(layer, tileLayer.name);
    },
    addShapefileLayer: function (mapId, shapefileLayer, objectReference) {
        const layer = L.shapefile(shapefileLayer.urlTemplate);
        addLayer(mapId, layer, shapefileLayer.id);
    },
    addHeatLayer: function (mapId, heatLayer, objectRef) {
        var gradient = null;
        if (heatLayer.gradient !== null) {
            gradient = {};
            for (const part of heatLayer.gradient) {
                gradient[part.point] = part.color;
            }
        }
        var options = {
            minOpacity: heatLayer.minOpacity,
            maxZoom: heatLayer.maxZoom,
            max: heatLayer.maxPointIntensity,
            radius: heatLayer.radius,
            blur: heatLayer.blur,
            gradient: gradient
        };

        const layer = L.heatLayer(heatLayer.points, options);

        addLayer(mapId, layer, heatLayer.id);
    },
    addMarker: function (mapId, marker, objectReference) {
        var options = {
            ...createInteractiveLayer(marker),
            keyboard: marker.isKeyboardAccessible,
            title: marker.title,
            alt: marker.alt,
            zIndexOffset: marker.zIndexOffset,
            opacity: marker.opacity,
            riseOnHover: marker.riseOnHover,
            riseOffset: marker.riseOffset,
            pane: marker.pane,
            bubblingMouseEvents: marker.isBubblingMouseEvents,
            draggable: marker.draggable,
            autoPan: marker.useAutopan,
            autoPanPadding: marker.autoPanPadding,
            autoPanSpeed: marker.autoPanSpeed
        };

        if (marker.icon !== null) {
            options.icon = createIcon(marker.icon);
        }
        const mkr = L.marker(marker.position, options);
        connectMarkerEvents(mkr, objectReference);
        addLayer(mapId, mkr, marker.id);
        setTooltipAndPopupIfDefined(marker, mkr);
    },
    addPolyline: function (mapId, polyline, objectReference) {
        const layer = L.polyline(shapeToLatLngArray(polyline.shape), createPolyline(polyline));
        addLayer(mapId, layer, polyline.id);
        setTooltipAndPopupIfDefined(polyline, layer);
    },
    updatePolyline: function (mapId, polyline) {
        let layer = layers[mapId].find(l => l.id === polyline.id);
        if (layer !== undefined) {
            layer.setLatLngs(shapeToLatLngArray(polyline.shape));
        }
    },
    addPolygon: function (mapId, polygon, objectReference) {
        const layer = L.polygon(shapeToLatLngArray(polygon.shape), createPolyline(polygon));
        addLayer(mapId, layer, polygon.id);
        setTooltipAndPopupIfDefined(polygon, layer);
    },
    updatePolygon: function (mapId, polygon) {
        let layer = layers[mapId].find(l => l.id === polygon.id);
        if (layer !== undefined) {
            layer.setLatLngs(shapeToLatLngArray(polygon.shape));
        }
    },
    addRectangle: function (mapId, rectangle, objectReference) {
        const layer = L.rectangle([[rectangle.shape.bottom, rectangle.shape.left], [rectangle.shape.top, rectangle.shape.right]], createPolyline(rectangle));
        addLayer(mapId, layer, rectangle.id);
        setTooltipAndPopupIfDefined(rectangle, layer);
    },
    updateRectangle: function (mapId, rectangle) {
        let layer = layers[mapId].find(l => l.id === rectangle.id);
        if (layer !== undefined) {
            layer.setBounds([[rectangle.shape.bottom, rectangle.shape.left], [rectangle.shape.top, rectangle.shape.right]]);
        }
    },
    addCircle: function (mapId, circle, objectReference) {
        const layer = L.circle(circle.position,
            {
                ...createPath(circle),
                radius: circle.radius
            });
        addLayer(mapId, layer, circle.id);
        setTooltipAndPopupIfDefined(circle, layer);
    },
    updateCircle: function (mapId, circle) {
        let layer = layers[mapId].find(l => l.id === circle.id);
        if (layer !== undefined) {
            layer.setRadius(circle.radius);
            layer.setLatLng(circle.position);
        }
    },
    bringPathToFront: function (mapId, path){
        let layer = layers[mapId].find(l => l.id ===path.id);
        layer.bringToFront();
    },
    bringPathToBack: function (mapId, path){
        let layer = layers[mapId].find(l => l.id ===path.id);
        layer.bringToBack();
    },
    addImageLayer: function (mapId, image, objectReference) {
        const layerOptions = {
            ...createInteractiveLayer(image),
            opacity: image.opacity,
            alt: image.alt,
            crossOrigin: image.crossOrigin,
            errorOverlayUrl: image.errorOverlayUrl,
            zIndex: image.zIndex,
            className: image.className
        };

        const corner1 = L.latLng(image.corner1.x, image.corner1.y);
        const corner2 = L.latLng(image.corner2.x, image.corner2.y);
        const bounds = L.latLngBounds(corner1, corner2);

        const imgLayer = L.imageOverlay(image.url, bounds, layerOptions);
        connectInteractiveLayerEvents(imgLayer, objectReference);
        addLayer(mapId, imgLayer, image.id);
    },
    addImageRotatedLayer: function (mapId, image, objectReference) {
        const layerOptions = {
            ...createInteractiveLayer(image),
            opacity: image.opacity,
            alt: image.alt,
            crossOrigin: image.crossOrigin,
            errorOverlayUrl: image.errorOverlayUrl,
            zIndex: image.zIndex,
            className: image.className
        };

        const corner1 = L.latLng(image.corner1.x, image.corner1.y);
        const corner2 = L.latLng(image.corner2.x, image.corner2.y);
        const corner3 = L.latLng(image.corner3.x, image.corner3.y);


        const imgLayer = L.imageOverlay.rotated(image.url, corner1, corner2, corner3, layerOptions);
        connectInteractiveLayerEvents(imgLayer, objectReference);
        addLayer(mapId, imgLayer, image.id);
    },
    addGeoJsonLayer: function (mapId, geodata, objectReference) {
        const geoDataObject = JSON.parse(geodata.geoJsonData);
        var options = {
            ...createInteractiveLayer(geodata),
            title: geodata.title,
            bubblingMouseEvents: geodata.isBubblingMouseEvents,
            onEachFeature: function onEachFeature(feature, layer) {
                connectInteractionEvents(layer, objectReference);
            }
        };

        const geoJsonLayer = L.geoJson(geoDataObject, options);
        addLayer(mapId, geoJsonLayer, geodata.id);
    },
    removeLayer: function (mapId, layerId) {
        const remainingLayers = layers[mapId].filter((layer) => layer.id !== layerId);
        const layersToBeRemoved = layers[mapId].filter((layer) => layer.id === layerId); // should be only one ...
        layers[mapId] = remainingLayers;

        layersToBeRemoved.forEach(m => m.removeFrom(maps[mapId]));
    },
    updatePopupContent: function (mapId, layerId, content) {
        let layer = layers[mapId].find(l => l.id === layerId);
        if (layer !== undefined) {
            var popup = layer.getPopup();
            if (popup !== undefined) {
                popup.setContent(content);
            }
        }
    },
    updateTooltipContent: function (mapId, layerId, content) {
        let layer = layers[mapId].find(l => l.id === layerId);
        if (layer !== undefined) {
            var tooltip = layer.getTooltip();
            if (tooltip !== undefined) {
                tooltip.setContent(content);
            }
        }
    },
    fitBounds: function (mapId, corner1, corner2, padding, maxZoom) {
        const corner1LL = L.latLng(corner1.x, corner1.y);
        const corner2LL = L.latLng(corner2.x, corner2.y);
        const mapBounds = L.latLngBounds(corner1LL, corner2LL);
        maps[mapId].fitBounds(mapBounds, {
            padding: padding == null ? null : L.point(padding.x, padding.y),
            maxZoom: maxZoom
        });
    },
    panTo: function (mapId, position, animate, duration, easeLinearity, noMoveStart) {
        const pos = L.latLng(position.x, position.y);
        maps[mapId].panTo(pos, {
            animate: animate,
            duration: duration,
            easeLinearity: easeLinearity,
            noMoveStart: noMoveStart
        });
    },
    getBoundsFromMarker: function (markers) {
        var rM = [];
        for (const m of markers) {
            rM.push(L.marker(m.position));
        }
        var group = new L.featureGroup(rM);
        return group.getBounds(group).pad(0.5);
    },
    getCenter: function (mapId) {
        return maps[mapId].getCenter();
    },
    getZoom: function (mapId) {
        return maps[mapId].getZoom();
    },
    getBounds: function (mapId) {
        return maps[mapId].getBounds();
    },
    zoomIn: function (mapId, e) {
        const map = maps[mapId];

        if (map.getZoom() < map.getMaxZoom()) {
            map.zoomIn(map.options.zoomDelta * (e.shiftKey ? 3 : 1));
        }
    },
    zoomOut: function (mapId, e) {
        const map = maps[mapId];

        if (map.getZoom() > map.getMinZoom()) {
            map.zoomOut(map.options.zoomDelta * (e.shiftKey ? 3 : 1));
        }
    },
    openLayerPopup: function (mapId, layerId) {
        let layer = layers[mapId].find(l => l.id === layerId);
        layer.openPopup();
    }
};

function createIcon(icon) {
    return L.icon({
        iconUrl: icon.url,
        iconRetinaUrl: icon.retinaUrl,
        iconSize: icon.size ? L.point(icon.size.width, icon.size.height) : null,
        iconAnchor: icon.anchor ? L.point(icon.anchor.x, icon.anchor.y) : null,
        popupAnchor: L.point(icon.popupAnchor.x, icon.popupAnchor.y),
        tooltipAnchor: L.point(icon.tooltipAnchor.x, icon.tooltipAnchor.y),
        shadowUrl: icon.shadowUrl,
        shadowRetinaUrl: icon.shadowRetinaUrl,
        shadowSize: icon.shadowSize ? L.point(icon.shadowSize.width, icon.shadowSize.height) : null,
        shadowSizeAnchor: icon.shadowSizeAnchor ? L.point(icon.shadowSizeAnchor.width, icon.shadowSizeAnchor.height) : null,
        className: icon.className
    })
}

function shapeToLatLngArray(shape) {
    var latlngs = [];
    shape.forEach(pts => {
        var ll = [];
        pts.forEach(p => ll.push([p.x, p.y]));
        latlngs.push(ll);
    });

    return latlngs;
}

function createPolyline(polyline) {
    return {
        ...createPath(polyline),
        smoothFactor: polyline.smoothFactor,
        noClip: polyline.noClipEnabled
    };
}

function createPath(path) {
    return {
        ...createInteractiveLayer(path),
        stroke: path.drawStroke,
        color: getColorString(path.strokeColor),
        weight: path.strokeWidth,
        opacity: path.strokeOpacity,
        lineCap: path.lineCap,
        lineJoin: path.lineJoin,
        dashArray: path.strokeDashArray,
        dashOffset: path.strokeDashOffset,
        fill: path.fill,
        fillColor: getColorString(path.fillColor),
        fillOpacity: path.fillOpacity,
        fillRule: path.fillRule
    };
}

function createInteractiveLayer(layer) {
    return {
        ...createLayer(layer),
        interactive: layer.isInteractive,
        bubblingMouseEvents: layer.isBubblingMouseEvents
    };
}

function createLayer(obj) {
    return {
        id: obj.id,
        pane: obj.pane,
        attribution: obj.attribution
    };
}

function getColorString(color) {
    return "rgb(" + color.r + "," + color.g + "," + color.b + ")";
}

function unbindTooltipAndPopupIfDefined(layer) {
    if (layer.getTooltip()) {
        layer.unbindTooltip();
    }
    if (layer.getPopup()) {
        layer.unbindPopup();
    }
}

function setTooltipAndPopupIfDefined(layer, jsLayer) {
    if (layer.tooltip) {
        addTooltip(jsLayer, layer.tooltip);
    }
    if (layer.popup) {
        addPopup(jsLayer, layer.popup);
    }
}

function addTooltip(layerObj, tooltip) {
    layerObj.bindTooltip(tooltip.content,
        {
            pane: tooltip.pane,
            offset: L.point(tooltip.offset.x, tooltip.offset.y),
            direction: tooltip.direction,
            permanent: tooltip.isPermanent,
            sticky: tooltip.isSticky,
            opacity: tooltip.opacity
        });
}

function addPopup(layerObj, popup) {
    layerObj.bindPopup(popup.content, {
        pane: popup.pane,
        className: popup.className,
        maxWidth: popup.maximumWidth,
        minWidth: popup.minimumWidth,
        autoPan: popup.autoPanEnabled,
        autoPanPaddingTopLeft: popup.autoPanPaddingTopLeft ? L.point(popup.autoPanPaddingTopLeft.x, popup.autoPanPaddingTopLeft.y) : null,
        autoPanPaddingBottomRight: popup.autoPanPaddingBottomRight ? L.point(popup.autoPanPaddingBottomRight.x, popup.autoPanPaddingBottomRight.y) : null,
        autoPanPadding: L.point(popup.autoPanPadding.x, popup.autoPanPadding.y),
        keepInView: popup.keepInView,
        closeButton: popup.showCloseButton,
        autoClose: popup.autoClose,
        closeOnEscapeKey: popup.closeOnEscapeKey,
    });
}

function addLayer(mapId, layer, layerId) {
    layer.id = layerId;
    layers[mapId].push(layer);
    layer.addTo(maps[mapId]);
}

// #region events

// removes properties that can cause circular references
function cleanupEventArgsForSerialization(eventArgs) {

    const propertiesToRemove = [
        "target",
        "sourceTarget",
        "propagatedFrom",
        "originalEvent",
        "tooltip",
        "popup"
    ];

    const copy = {};

    for (let key in eventArgs) {
        if (!propertiesToRemove.includes(key) && eventArgs.hasOwnProperty(key)) {
            copy[key] = eventArgs[key];
        }
    }

    return copy;
}

function mapEvents(mapElement, objectReference, eventHandlerDict) {
    for (let key in eventHandlerDict) {

        const handlerName = eventHandlerDict[key];

        mapElement.on(key, function (eventArgs) {
            objectReference.invokeMethodAsync(handlerName,
                cleanupEventArgsForSerialization(eventArgs));
        });
    }
}

function connectMapEvents(map, objectReference) {

    connectInteractionEvents(map, objectReference);

    mapEvents(map, objectReference, {
        "zoomlevelschange": "NotifyZoomLevelsChange",
        "resize": "NotifyResize",
        "unload": "NotifyUnload",
        "viewreset": "NotifyViewReset",
        "load": "NotifyLoad",
        "zoomstart": "NotifyZoomStart",
        "movestart": "NotifyMoveStart",
        "zoom": "NotifyZoom",
        "move": "NotifyMove",
        "zoomend": "NotifyZoomEnd",
        "moveend": "NotifyMoveEnd",
        "mousemove": "NotifyMouseMove",
        "keypress": "NotifyKeyPress",
        "keydown": "NotifyKeyDown",
        "keyup": "NotifyKeyUp",
        "preclick": "NotifyPreClick",
    });
}

function connectLayerEvents(layer, objectReference) {
    mapEvents(layer, objectReference, {
        "add": "NotifyAdd",
        "remove": "NotifyRemove",
        "popupopen": "NotifyPopupOpen",
        "popupclose": "NotifyPopupClose",
        "tooltipopen": "NotifyTooltipOpen",
        "tooltipclose": "NotifyTooltipClose",
    });
}

function connectInteractiveLayerEvents(interactiveLayer, objectReference) {

    connectLayerEvents(interactiveLayer, objectReference);
    connectInteractionEvents(interactiveLayer, objectReference);
}

function connectMarkerEvents(marker, objectReference) {

    connectInteractiveLayerEvents(marker, objectReference);

    mapEvents(marker, objectReference, {
        "move": "NotifyMove",
        "dragstart": "NotifyDragStart",
        "movestart": "NotifyMoveStart",
        "drag": "NotifyDrag",
        "dragend": "NotifyDragEnd",
        "moveend": "NotifyMoveEnd",
    });
}

function connectInteractionEvents(interactiveObject, objectReference) {

    mapEvents(interactiveObject, objectReference, {
        "click": "NotifyClick",
        "dblclick": "NotifyDblClick",
        "mousedown": "NotifyMouseDown",
        "mouseup": "NotifyMouseUp",
        "mouseover": "NotifyMouseOver",
        "mouseout": "NotifyMouseOut",
        "contextmenu": "NotifyContextMenu",
    });
}

// #endregion
