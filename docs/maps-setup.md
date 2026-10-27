# TaskTap – OpenStreetMap + Leaflet maps

The four painted maps (guest Browse Tasks, member Find Tasks, and both Task Details pages) are now a real
**OpenStreetMap** map drawn by **Leaflet** inside a WebView2 control (`Map/MapView.cs`).

## Requirements
- **Microsoft Edge WebView2 Runtime** (already installed on Windows 11 and most up-to-date Windows 10).
  If it is missing, the old painted map is shown instead, with a note explaining why.
- Internet access for the map tiles (`tile.openstreetmap.org`) and Leaflet (`unpkg.com`).
  For offline use, put Leaflet 1.9.4's `leaflet.js`, `leaflet.css` and its `images` folder in
  `assets/map/leaflet/`; the page uses that copy first and only falls back to the CDN.
- `dotnet restore` downloads the `Microsoft.Web.WebView2` NuGet package.

## What it does
| Place | Behaviour |
|---|---|
| Find Tasks / Browse Tasks | Markers for the tasks that match your filters; clicking a marker highlights the task and its popup has **View details**. Clicking a task in the list highlights its marker. |
| Task Details (both) | Shows an **approximate circle** (300 m with a pin, 700 m for barangay-only), never the exact spot. |
| Post Task / Edit Task | **Pin the spot on the map (optional)** opens a picker: click the map or search a street/landmark (OpenStreetMap Nominatim). |

## Privacy
The exact pin is stored in `tasks.latitude/longitude` but is never drawn for other members. Every marker/circle is
shifted by a stable, task-specific offset (at most 55% of the radius), so the true location is always inside the
circle but cannot be read off the map. Tasks without a pin use the barangay centre.

## Barangay centres
The first time a barangay is needed, TaskTap looks it up once through Nominatim (one request per second, as the
usage policy requires) and caches it in `barangays.latitude/longitude`. Until then, the city centre from
`appsettings.json` is used. Set `Maps:ContactEmail` in `appsettings.json` so OpenStreetMap can identify the app.

## Database upgrade
Existing databases are upgraded automatically at startup (`Data/Migrations.cs` adds the missing columns,
including `users.bio` for databases created by older builds). Nothing to run by hand.

## Usage policy reminders
Map tiles come from the community OpenStreetMap server: keep the attribution visible (it is), and do not use the
public tile or Nominatim servers for heavy traffic. For a public launch, switch to a tile provider or your own server.
