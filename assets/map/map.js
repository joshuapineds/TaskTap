(function () {
  'use strict';

  var LEAFLET_VERSION = '1.9.4';
  var CDN = 'https://unpkg.com/leaflet@' + LEAFLET_VERSION + '/dist/';
  var CDN_JS_SRI = 'sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=';
  var CDN_CSS_SRI = 'sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=';

  var map, markerLayer, areaLayer, pickMarker;
  var pickerOn = false;
  var markers = {};
  var selectedId = null;

  function post(message) {
    try { window.chrome.webview.postMessage(JSON.stringify(message)); } catch (e) { /* not hosted in WebView2 */ }
  }

  function showError(text) {
    var box = document.getElementById('message');
    box.textContent = text;
    box.hidden = false;
    post({ type: 'error', message: text });
  }

  function addCss(href, integrity) {
    var link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    if (integrity) { link.integrity = integrity; link.crossOrigin = ''; }
    document.head.appendChild(link);
  }

  function addScript(src, integrity, ok, fail) {
    var script = document.createElement('script');
    script.src = src;
    if (integrity) { script.integrity = integrity; script.crossOrigin = ''; }
    script.onload = ok;
    script.onerror = fail;
    document.head.appendChild(script);
  }

  // Prefer a copy of Leaflet shipped in assets/map/leaflet (works offline); otherwise use the CDN.
  function loadLeaflet(done) {
    addScript('leaflet/leaflet.js', null, function () {
      addCss('leaflet/leaflet.css', null);
      done();
    }, function () {
      addScript(CDN + 'leaflet.js', CDN_JS_SRI, function () {
        addCss(CDN + 'leaflet.css', CDN_CSS_SRI);
        done();
      }, function () {
        showError('The map could not load. Check your internet connection and try again.');
      });
    });
  }

  function validColor(value) {
    return /^#[0-9a-fA-F]{6}$/.test(value || '') ? value : '#0875e8';
  }

  function pinIcon(color, selected, extraClass) {
    var el = document.createElement('div');
    el.className = 'tt-pin' + (selected ? ' selected' : '') + (extraClass ? ' ' + extraClass : '');
    if (!selected && !extraClass) el.style.background = validColor(color);
    return L.divIcon({ className: 'tt-pin-wrap', html: el, iconSize: [26, 26], iconAnchor: [13, 26], popupAnchor: [0, -24] });
  }

  function popupFor(task) {
    var box = document.createElement('div');
    box.className = 'tt-popup';
    var title = document.createElement('strong');
    title.textContent = task.title || '';
    var sub = document.createElement('span');
    sub.textContent = task.subtitle || '';
    var button = document.createElement('button');
    button.type = 'button';
    button.textContent = 'View details';
    button.addEventListener('click', function () { post({ type: 'open', id: task.id }); });
    box.appendChild(title);
    box.appendChild(sub);
    box.appendChild(button);
    return box;
  }

  function clearAll() {
    markerLayer.clearLayers();
    areaLayer.clearLayers();
    markers = {};
  }

  var TT = {
    setMarkers: function (tasks, fit) {
      areaLayer.clearLayers();
      markerLayer.clearLayers();
      markers = {};
      var bounds = [];
      tasks.forEach(function (task) {
        var marker = L.marker([task.lat, task.lng], { icon: pinIcon(task.color, task.id === selectedId) });
        marker.bindPopup(popupFor(task));
        marker.on('click', function () { selectedId = task.id; TT.highlight(task.id, true); post({ type: 'select', id: task.id }); });
        marker.addTo(markerLayer);
        markers[task.id] = { marker: marker, task: task };
        bounds.push([task.lat, task.lng]);
      });
      if (fit && bounds.length > 0) map.fitBounds(bounds, { padding: [40, 40], maxZoom: 16 });
    },

    highlight: function (id, fromMarker) {
      selectedId = id;
      Object.keys(markers).forEach(function (key) {
        var entry = markers[key];
        entry.marker.setIcon(pinIcon(entry.task.color, entry.task.id === id));
      });
      var current = markers[id];
      if (current && !fromMarker) {
        map.panTo(current.marker.getLatLng());
        current.marker.openPopup();
      }
    },

    fitAll: function () {
      var bounds = Object.keys(markers).map(function (key) { return markers[key].marker.getLatLng(); });
      if (bounds.length > 0) map.fitBounds(bounds, { padding: [40, 40], maxZoom: 16 });
    },

    // Approximate area for a single task (never the exact pin).
    showArea: function (area) {
      clearAll();
      var circle = L.circle([area.lat, area.lng], {
        radius: area.radius, color: '#0875e8', weight: 2, fillColor: '#0875e8', fillOpacity: 0.16
      }).addTo(areaLayer);
      if (area.label) circle.bindTooltip(area.label, { permanent: true, direction: 'center', className: 'tt-area-label' });
      map.fitBounds(circle.getBounds(), { padding: [20, 20], maxZoom: 16 });
    },

    setView: function (lat, lng, zoom) { map.setView([lat, lng], zoom || map.getZoom()); },

    setPicker: function (on, lat, lng) {
      pickerOn = !!on;
      map.getContainer().style.cursor = pickerOn ? 'crosshair' : '';
      if (lat !== null && lat !== undefined) TT.setPicked(lat, lng, 16);
    },

    setPicked: function (lat, lng, zoom) {
      if (pickMarker) pickMarker.remove();
      pickMarker = L.marker([lat, lng], { icon: pinIcon(null, false, 'tt-pick') }).addTo(map);
      map.setView([lat, lng], zoom || Math.max(map.getZoom(), 15));
    }
  };
  window.TT = TT;

  function start() {
    var query = new URLSearchParams(window.location.search);
    var lat = parseFloat(query.get('lat')) || 14.7011;
    var lng = parseFloat(query.get('lng')) || 121.0108;
    var zoom = parseInt(query.get('zoom'), 10) || 12;

    map = L.map('map', { minZoom: 9, maxZoom: 19 }).setView([lat, lng], zoom);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);
    L.control.scale({ imperial: false }).addTo(map);
    markerLayer = L.layerGroup().addTo(map);
    areaLayer = L.layerGroup().addTo(map);

    map.on('click', function (event) {
      if (!pickerOn) return;
      var picked = { lat: Math.round(event.latlng.lat * 1e6) / 1e6, lng: Math.round(event.latlng.lng * 1e6) / 1e6 };
      TT.setPicked(picked.lat, picked.lng, map.getZoom());
      post({ type: 'picked', lat: picked.lat, lng: picked.lng });
    });

    post({ type: 'ready' });
  }

  loadLeaflet(start);
})();
