# TaskTap Admin Portal Blueprint

**Status:** COMPLETE — all phases delivered
**Scope:** Admin Portal frontend — 15 pages (the 12 planned pages + the 3 approved extras)
**Companion:** `docs/navigation-blueprint.md` (locked user-app sitemap)
**Note:** Planning artifact only. No C# code was written or modified to produce it.

---

## 0. Locked decisions

| # | Decision | Locked value |
|---|---|---|
| 1 | Shell | **Dedicated `AdminShellForm` + `AdminNavigationService`**, parallel to `MainShellForm` + `NavigationService`. |
| 2 | Admin identity | **Extend `AppSession`** with `CurrentAdmin` / `IsAdminSession`; `CurrentUser` is never clobbered. |
| 3 | Demo credentials | **Any email + password works**; demo hint on the form; **sign-out returns to `AdminLoginForm`**. |
| 4 | Extra pages | **`Verification Queue`, `Roles & Permissions`, `Disputes`** added to the scope. |
| 5 | Cross-sync | Admin edits are **immediately visible** in the user app; suspensions **mark** the user without a forced session eject. |
| 6 | Visual identity | **Same TaskTap palette** with a bold **"ADMIN" badge**. |
| 7 | Archives | **Tasks, users, and resolved reports.** |
| 8 | Data ownership | Single source of truth: **`DemoDataStore`** + a `DataChanged` event. Admin pages mutate the same lists the user app reads. |
| 9 | Forms | **No hidden forms:** only `AdminLoginForm` and `AdminShellForm` are `Form`s; every other admin page is an `AdminPage : UserControl` hosted by the shell. |
| 10 | Build gate | Every phase ends with `dotnet build` → **0 warnings, 0 errors**. |

---

## 1. Architecture

### 1.1 Shell and navigation

`MainShellForm` today owns the sidebar, topbar, content host, toast and logout, and delegates page swaps to `NavigationService`. `NavigationService` is typed to `MainShellForm` (`Attach(MainShellForm)`, `ContentHost`, `UpdateChrome(Route)`, `ExitToLanding()`), and `Route` is the user enum.

To avoid polluting the user shell:

- **`AdminShellForm`** — new single window. Same visual recipe: left sidebar (240 px), topbar (62 px), content host, toast, `TaskTapLogo` with a bold **ADMIN** pill. Sidebar also exposes **"Return to TaskTap"** and **"Sign out"**.
- **`AdminNavigationService`** — new singleton, mirroring `NavigationService`: `Attach(AdminShellForm)`, `Navigate(AdminRoute)`, `Navigate(AdminPage)`, `Back()`, `GoHome()`, `Reset()`, `Logout()`, `ContentHost`, `UpdateChrome(AdminRoute)`. Own back stack; own singleton map.
- **`AdminPage : UserControl`** — base class mirroring `AppPage`: `Dock = Fill`, `AdminRouteName`, `Nav` (the admin navigation service), `OnNavigatedTo()`.
- **`AdminUi`** — shared helper layer (labels, cards, buttons, stat cards, pills, search box, filter combos, empty states, table headers, confirm dialog) so the 15 pages do not duplicate the helper recipes.

Pages are swapped inside `AdminShellForm.ContentHost`; pages never open or hide windows.

### 1.2 Admin login and identity

Flow (mirrors `Form2` → `MainShellForm`):

```
Form2 ──"Admin sign in"──► AdminLoginForm ──success──► AdminShellForm ──► AdminRoute.Dashboard
                                  │
                                  └─ back / cancel ──► Form2
AdminShellForm ──"Sign out"──► AdminLoginForm   (self-contained admin flow)
AdminShellForm ──"Return to TaskTap"──► Form1 landing
```

- `AdminLoginForm` — branded left panel + login card (email, password, show/hide eye, remember me, "Sign in as demo admin"), demo hint: *"Demo admin — any email and password works."*, link back to the user app.
- On success: `AppSession.SignInAdminAs(new AppUser(... Role: "Administrator"))` and open `AdminShellForm`.
- `AppSession` gains:
  - `AppUser? CurrentAdmin`
  - `bool IsAdminSession => CurrentAdmin is not null`
  - `SignInAdminAs(AppUser admin)`
  - `SignOutAdmin()`
  - `SignOut()` also clears `CurrentAdmin` (full reset).
- The user persona (`CurrentUser`) is untouched by admin sign-in/sign-out.

### 1.3 Page pattern

Every admin page follows the user-page recipe that is already stable in this codebase:

1. `BuildContent()` — construct controls once (fields + `Name`d controls).
2. `LayoutPage()` — position everything from the host width; guard on `ClientSize.Width <= 0`.
3. A `Refresh()` / `ApplyFilters()` that re-reads `DemoDataStore` and re-renders rows; called from `OnNavigatedTo()` and the store's `DataChanged`.
4. Empty, loading and error states are explicit panels.
5. Row/table actions show a toast for demo-only operations; every real mutation logs a `DemoActivity`.

### 1.4 Data ownership

`DemoDataStore` remains the **single** in-memory source of truth. Admin and user surfaces read the same lists. New collections, models and mutations are described in §4.

---

## 2. Sitemap

```
Admin Portal
├─ Admin Login ...................... AdminLoginForm        (entry, guest Form)
└─ Admin Shell ...................... AdminShellForm
    ├─ Dashboard (home) ............. AdminDashboardPage
    ├─ Users ........................ UserManagementPage
    │   ├─ User Detail .............. UserDetailPage        (contextual, takes user id)
    │   └─ Verification Queue ....... VerificationQueuePage (contextual)
    ├─ Tasks ........................ TaskManagementPage
    ├─ Categories ................... CategoryManagementPage
    ├─ Reports ...................... ReportManagementPage
    │   └─ Disputes ................. DisputesPage           (contextual)
    ├─ Payments ..................... PaymentRecordsPage
    ├─ Archives ..................... ArchivesPage           (tasks / users / resolved reports)
    ├─ Barangays .................... BarangayManagementPage
    ├─ Activity Log ................. ActivityLogPage
    └─ Settings ..................... AdminSettingsPage
        └─ Roles & Permissions ...... RolesPermissionsPage  (contextual)
```

Sidebar (10 global items): **Dashboard · Users · Tasks · Categories · Reports · Payments · Archives · Barangays · Activity Log · Settings**. The four contextual pages are reached from their parent pages, not the sidebar (same "one meaning, one primary parent" rule as the user app).

---

## 3. Route matrix

| `AdminRoute` | Page | Kind | Entered from | Chrome title |
|---|---|---|---|---|
| `Dashboard` | `AdminDashboardPage` | global | sidebar / login | Dashboard |
| `Users` | `UserManagementPage` | global | sidebar | User Management |
| `UserDetail` | `UserDetailPage` | contextual | User Management row / Reports / Disputes | Users › User Detail |
| `VerificationQueue` | `VerificationQueuePage` | contextual | User Management / Dashboard | Users › Verification Queue |
| `RolesPermissions` | `RolesPermissionsPage` | contextual | Settings / sidebar (Settings) | Settings › Roles & Permissions |
| `Tasks` | `TaskManagementPage` | global | sidebar | Task Management |
| `Categories` | `CategoryManagementPage` | global | sidebar | Category Management |
| `Reports` | `ReportManagementPage` | global | sidebar | Report Management |
| `Disputes` | `DisputesPage` | contextual | Reports / Dashboard / Payment Records | Reports › Disputes |
| `Payments` | `PaymentRecordsPage` | global | sidebar | Payment Records |
| `Archives` | `ArchivesPage` | global | sidebar | Archives |
| `Barangays` | `BarangayManagementPage` | global | sidebar | Barangay Management |
| `ActivityLog` | `ActivityLogPage` | global | sidebar | Activity Log |
| `Settings` | `AdminSettingsPage` | global | sidebar | Settings |

Singleton routes are reused; contextual routes are created per call and pushed on the back stack (the established user-app pattern).

---

## 4. Shared data model

### 4.1 New models (`AdminModels.cs`)

| Model | Fields |
|---|---|
| `DemoUser` | `Id, FullName, FirstName, LastName, Initials, Email, Phone, Role, Barangay, Status, Verified, Rating, Reviews, CompletedTasks, PostedTasks, Joined, LastActive, AvatarColor, Skills` |
| `DemoCategory` | `Id, Name, Icon, Color, Active, SortOrder` |
| `DemoBarangay` | `Id, Name, City, Active` |
| `DemoReport` | `Id, ReporterName, TargetType, TargetLabel, Reason, Severity, Status, CreatedAt, Description, Resolution` |
| `DemoDispute` | `Id, TaskTitle, PosterName, DoerName, Amount, Reason, Status, OpenedAt, Resolution` |
| `DemoActivity` | `Id, Actor, Action, Target, Category, Timestamp, Detail` |
| `AdminSettings` | `PlatformName, SupportEmail, DefaultBarangay, AutoApproveThreshold, RequireVerification, MaintenanceMode, SessionTimeoutMinutes` |

`DemoUser.Status` ∈ `Active · Suspended · Unverified · Deactivated`; `DemoUser.Role` ∈ `Standard User · Task Doer · Administrator`.

### 4.2 `DemoDataStore` additions

New lists (seeded in the same catalog style as the existing 9-phase data):

- `Users` — seed reuses the existing cast: Joshua D. Pineda (current), Maria Santos, Juan Dela Cruz, Angela Reyes, Roberto Reyes, Carlo Mendoza, Ana Cruz, Liza Cruz, Jovan Floidazamie, Sean Dreza, Zef Ian Junio.
- `Categories` — Cleaning, Tutoring, Repairs, Shopping, Pet Care, Moving, Delivery, Others (matching the tiles/filters already in the user app).
- `Barangays` — 176, 177, 178, 179, 180 (matching the demo addresses).
- `Reports` — 6–8 seeded abuse/moderation reports across task/user/message/review targets, mixed severities and statuses.
- `Disputes` — 3–4 seeded task disputes (no-show, incomplete, refund request).
- `ActivityLog` — seeded audit trail (user verified, task archived, payment recorded, report resolved) so the log is not empty on first open.
- `AdminSettings` — one seeded settings object.

New mutation helpers (all raise `DataChanged` and append a `DemoActivity`):

```
SuspendUser / ActivateUser / VerifyUser / RejectVerification / DeleteUser
FeatureTask / PauseTask / ArchiveTask(re) / RestoreTask / DeleteTask
AddCategory / UpdateCategory / DeleteCategory / ToggleCategory / ReorderCategory
AddBarangay / UpdateBarangay / DeactivateBarangay
ResolveReport / DismissReport / EscalateReport
ResolveDispute / RefundDispute / DismissDispute
MarkPaymentPaid / VoidPayment / RefundPayment
AddAnnouncement (if Announcements is added later)
UpdateSettings
ResetDemoData
LogActivity(actor, action, target, category, detail)
```

Event: `public static event Action? DataChanged;` raised after any mutation. `AdminShellForm` and each page subscribe to refresh.

> `ArchiveTask`/`RestoreTask` already exist and move between `PostedTasks` and `ArchivedTasks`; the admin task page will reuse them, so the user app sees the same move immediately.

### 4.3 Seed parity

Admin seeds must match the existing demo story exactly (same names, amounts, barangays, categories). This is what makes the "same mock data" requirement visible: a category added in the admin portal appears in the user app's category tiles; a suspended user shows the status on their detail; a marked-paid payment appears in Payment History.

---

## 5. Cross-portal sync matrix

| Admin action | Store mutation | User-app surface that reflects it |
|---|---|---|
| Add / rename / deactivate category | `Categories` | `Form1` "Popular categories" tiles, Find Tasks category filter, Post Task category picker |
| Add / deactivate barangay | `Barangays` | `Form2` barangay dropdown, Find Tasks barangay filter, Post Task barangay picker |
| Archive / restore / feature task | `PostedTasks` / `ArchivedTasks` | My Tasks, Archived Tasks, Dashboard "Tasks for You", Find Tasks |
| Mark payment paid / void | `Payments` | Profile → Payment History, Payment Record |
| Suspend / activate / verify user | `Users` | User detail status (admin), public profile, message/request eligibility (demo) |
| Resolve / dismiss report | `Reports` | Admin list + Activity Log only (user app has no report history in the demo) |
| Resolve / refund dispute | `Disputes` | Admin only; payment record if refunded |
| Update settings | `AdminSettings` | Admin chrome/behaviour only |

---

## 6. Per-phase implementation plan

> 8 phases (A–H). Phase C is delivered in two reviewable chunks (C1, C2) so user-management and admin-account management can each be signed off.

### Phase A — Foundation, Admin Login, Admin Shell

**Goal:** a working, empty admin portal that can be entered and exited.

**Files (new):** `AdminRoute.cs`, `AdminPage.cs`, `AdminNavigationService.cs`, `AdminUi.cs`, `AdminShellForm.cs`, `AdminLoginForm.cs`, `AdminModels.cs`.
**Files (edit):** `AppSession.cs` (admin identity), `DemoDataStore.cs` (new lists + mutation skeletons + `DataChanged`), `Form2.cs` (wire `AdminSignInButton`).
**Deliverable:** `Form2` → `AdminLoginForm` → `AdminShellForm` → placeholder dashboard. Sidebar (10 items), topbar (back, crumb, `ADMIN` badge, admin avatar, sign out, "Return to TaskTap"), toast, back stack.
**Acceptance:**
- Any email + password signs in; empty fields show inline validation.
- Back button and sidebar highlight behave like the user shell.
- "Sign out" returns to `AdminLoginForm`; "Return to TaskTap" returns to `Form1`.
- `dotnet build` → 0/0.

### Phase B — Admin Dashboard

**Goal:** the operational landing page.

**Files (new):** `AdminDashboardPage.cs`.
**Deliverable:**
- KPI cards (total users, active tasks, open reports, payments this month, pending verifications).
- "Needs attention" queue (unverified users, open reports, disputes, suspended accounts) with jump links.
- Recent activity feed (from `ActivityLog`), recent sign-ups, quick actions (Add user, Post task, Add category, Send announcement placeholder).
- Mini charts (painted GDI+ bars/sparklines — no chart dependency).

**Acceptance:** numbers are computed from `DemoDataStore`; every quick action navigates to the right route; dashboard refreshes on `DataChanged`.

### Phase C1 — User Management + User Detail

**Goal:** manage the shared user list.

**Files (new):** `UserManagementPage.cs`, `UserDetailPage.cs`.
**Deliverable:**
- User Management: search, filter (role, status, barangay), sort (newest, most tasks, rating), pagination; status pills; row actions (View, Suspend/Activate, Verify, Message toast, Delete confirm); bulk suspend/verify.
- User Detail: header (avatar, name, role, status, contact, joined), tabs (Overview, Tasks, Payments, Reviews, Activity, Reports), actions (Suspend/Activate, Verify, Reset password toast, Message, Delete).

**Acceptance:** every action mutates `DemoDataStore.Users`, writes a `DemoActivity`, and the list/detail refresh. Contextual `UserDetail` is pushed on the back stack.

### Phase C2 — Verification Queue + Roles & Permissions

**Goal:** complete the admin/account lifecycle.

**Files (new):** `VerificationQueuePage.cs`, `RolesPermissionsPage.cs`.
**Deliverable:**
- Verification Queue: pending verifications, document preview placeholder, Approve/Reject with reason, filter by barangay/date.
- Roles & Permissions: list of admin accounts, add/remove admin, assign role (Super Admin, Moderator, Finance, Support), permission matrix (checkbox grid), last-active.

**Acceptance:** approvals flip `DemoUser.Verified`; admin accounts live in the same `Users` list with `Role = "Administrator"`; actions logged.

### Phase D — Task Management

**Goal:** moderate and curate all tasks.

**Files (new):** `TaskManagementPage.cs`.
**Deliverable:** table across posted/available/archived, filters (status, category, barangay, poster), search, sort, bulk archive/restore/delete, row actions (View, Feature, Pause, Archive, Restore, Delete), moderation queue fed by `Reports`, and a "view in user app" cross-link toast.

**Acceptance:** archive/restore reuses `DemoDataStore.ArchiveTask`/`RestoreTask`, so My Tasks and Archived Tasks in the user app reflect it immediately.

### Phase E — Category Management + Barangay Management

**Goal:** manage the two catalogs the user app already reads.

**Files (new):** `CategoryManagementPage.cs`, `BarangayManagementPage.cs`.
**Deliverable:**
- Categories: list with icon, name, task count, active toggle, reorder (up/down), add/edit/delete modals.
- Barangays: list with name, city, task count, user count, active toggle, add/edit/deactivate.

**Acceptance:** user-app category tiles/filters and barangay dropdowns read the same `DemoDataStore.Categories`/`Barangays` (add a `DataChanged` read path to the relevant user pages).

### Phase F — Report Management, Disputes, Activity Log

**Goal:** trust & safety operations.

**Files (new):** `ReportManagementPage.cs`, `DisputesPage.cs`, `ActivityLogPage.cs`.
**Deliverable:**
- Reports: list, filters (status, severity, target type), detail panel, actions (Resolve, Dismiss, Escalate, Suspend target → `Users`, Archive target task → `Tasks`).
- Disputes: list, detail, actions (Resolve, Refund, Dismiss), links to poster/doer and payment.
- Activity Log: timestamped feed, filters (actor, action, category, date), pagination, row detail.

**Acceptance:** every report/dispute action updates status and appends an activity row; the log is the audit surface for other admin actions.

### Phase G — Payment Records + Archives

**Goal:** money and retained history.

**Files (new):** `PaymentRecordsPage.cs`, `ArchivesPage.cs`.
**Deliverable:**
- Payments: table (task, poster, doer, amount, method, status, date), filters, totals cards, mark paid / void / refund, row detail, demo export (toast).
- Archives: tabs for **Tasks**, **Users**, **Resolved reports**; search/filter; restore/delete with confirmation; bulk actions.

**Acceptance:** mark-paid reflects in user Payment History; archived users are restorable; archived tasks reuse the existing store methods.

### Phase H — Settings, Polish, QA

**Goal:** configuration, consistency, and final verification.

**Files (new):** `AdminSettingsPage.cs`.
**Files (edit):** polish across admin pages; `docs/admin-blueprint.md` "as-built" section.
**Deliverable:**
- Settings: platform name, support email, default barangay, auto-approve threshold, require-verification toggle, maintenance mode, session timeout; admin profile (name/email/password demo); **Reset demo data**; Save/Cancel.
- Link to Roles & Permissions.
- Empty/error/loading states, keyboard/back behaviour, demo labeling audit.

**Acceptance:** settings persist in memory and are read by the shell; reset restores seeds; `dotnet build` → 0/0; blueprint updated with as-built notes.

---

## 7. File inventory

**New (22 planned + 4 delivered extras):**

`AdminRoute.cs` · `AdminPage.cs` · `AdminNavigationService.cs` · `AdminUi.cs` · `AdminModels.cs` · `AdminShellForm.cs` · `AdminLoginForm.cs` · `AdminDashboardPage.cs` · `UserManagementPage.cs` · `UserDetailPage.cs` · `VerificationQueuePage.cs` · `RolesPermissionsPage.cs` · `TaskManagementPage.cs` · `CategoryManagementPage.cs` · `BarangayManagementPage.cs` · `ReportManagementPage.cs` · `DisputesPage.cs` · `ActivityLogPage.cs` · `PaymentRecordsPage.cs` · `ArchivesPage.cs` · `AdminSettingsPage.cs` · `docs/admin-blueprint.md`

Plus the files added during delivery: `AdminPlaceholderPage.cs`, `AdminCharts.cs`, `AdminDialogs.cs`, `DemoDataStore.Admin.cs`.

**Edited (3 planned + more delivered):**

- `AppSession.cs` — `CurrentAdmin`, `IsAdminSession`, `SignInAdminAs`, `SignOutAdmin`.
- `DemoDataStore.cs` — admin lists, mutations, `DataChanged`, `ResetDemoData`.
- `Form2.cs` — wire `AdminSignInButton` to `AdminLoginForm`.

**Unchanged:** `Route.cs`, `NavigationService.cs`, `MainShellForm.cs`, `AppPage.cs`, and every user page/form. The user shell is not refactored.

---

## 8. Guardrails

1. **No hidden forms** — only `AdminLoginForm` and `AdminShellForm` are `Form`s; all 14 other pages are `AdminPage` UserControls in the shell.
2. **One source of truth** — admin pages never keep their own copy of users/tasks/payments; they read `DemoDataStore` and mutate through its methods.
3. **Navigation parity** — `AdminNavigationService` mirrors `NavigationService` (singleton routes, back stack, `GoHome`, `Reset`).
4. **Demo labeling** — the login shows "Demo admin — any email and password works."; admin-only write actions that are not persisted show a toast; Settings exposes **Reset demo data**.
5. **No silent dead ends** — every button either navigates, mutates, opens a confirm, or shows a labeled demo toast.
6. **Build gate** — `dotnet build -nologo` must end `0 Warning(s) 0 Error(s)` at the end of every phase.

---

## 9. As-built log

### Phase A — Foundation, Admin Login, Admin Shell (delivered)

**Files delivered:**
- `AdminRoute.cs`, `AdminModels.cs`, `AdminUi.cs`, `AdminPage.cs`, `AdminPlaceholderPage.cs`, `AdminNavigationService.cs`, `AdminShellForm.cs`, `AdminLoginForm.cs`, `AdminDashboardPage.cs`.
- `DemoDataStore.Admin.cs` — admin lists, seeds, `DataChanged`, mutations, `ResetDemoData`.
- Edited: `AppSession.cs` (admin identity), `DemoDataStore.cs` (made `partial`), `Form2.cs` (admin link → `AdminLoginForm`).

**Behaviour:** `Form2` → `AdminLoginForm` → `AdminShellForm` → Dashboard. The sidebar exposes the 10 global admin routes; routes not yet built open `AdminPlaceholderPage` (no dead ends). The topbar and sidebar both carry a bold **ADMIN** badge. Back stack, "Sign out" → `AdminLoginForm`, "Return to TaskTap" → `Form1`. `dotnet build` → **0 warnings / 0 errors**.

**Deviations from the blueprint:** added `AdminPlaceholderPage.cs` and `DemoDataStore.Admin.cs` (not in the original file inventory) to keep the shell navigable and the admin data modular. No other deviations.

**Deferred to later phases:** the dashboard is intentionally a Phase A placeholder (KPI cards, recent activity, quick actions); Phase B enriches it.

### Phase B — Admin Dashboard (delivered)

**Files delivered:** `AdminDashboardPage.cs` (rebuilt) and `AdminCharts.cs` (new `AdminBarChart` control).

**Behaviour:** five KPI cards (users, active tasks, open reports, payments in the latest recorded month, pending verifications); a **Needs attention** queue (pending verification, open/investigating reports, open disputes, suspended users) where each row has a count pill and a jump link; a five-item **Recent activity** feed; a **Newest members** list; five **Quick actions**; and two dependency-free painted bar charts (**Tasks by category**, **Payments by status**). All values are computed from `DemoDataStore` and refresh on `DataChanged` and on `OnNavigatedTo`.

**Deviations:** added `AdminCharts.cs` (not in the original inventory) for the painted chart control. Contextual links from the queue were given placeholder routes in `AdminNavigationService.Create` so no link throws before Phase C/F.

### Phase C1 — User Management + User Detail (delivered)

**Files delivered:** `UserManagementPage.cs`, `UserDetailPage.cs`; `AdminNavigationService.cs` now maps `AdminRoute.Users` to `UserManagementPage`.

**Behaviour:**
- **User Management** — search, role/status/barangay filters, sorting (newest, most tasks, highest rating, name), pagination (8 per page), selectable rows, bulk suspend/verify, and row actions (View, Suspend/Activate, Verify, Delete with confirmation). All actions mutate `DemoDataStore.Users` and write a `DemoActivity`.
- **User Detail** — profile header (avatar, name, status pill, role, contact, barangay/joined/verification) plus admin actions (Suspend/Activate, Verify, Reset password toast, Message toast, Delete) and six tabs (Overview, Tasks, Payments, Reviews, Activity, Reports) fed from the shared store.

**Deviations:** delete uses a standard `MessageBox` confirmation (the app's custom modal pattern is deferred); the row omits a separate "Message" action (available in the detail header). No other deviations.

### Phase C2 — Verification Queue + Roles & Permissions (delivered)

**Files delivered:** `VerificationQueuePage.cs`, `RolesPermissionsPage.cs`, `AdminDialogs.cs` (reusable `AdminReasonDialog`); `AdminNavigationService.cs` maps both routes; `DemoDataStore.Admin.cs` gains `AdminRoles`, `AdminPermissionKeys`, `AdminPermissions`, `PromoteToAdmin` / `DemoteAdmin` / `SetAdminRole` / `SetRolePermission`, and `RejectVerification` now accepts a reason.

**Behaviour:**
- **Verification Queue** — pending requests with search + barangay filter, per-row Review selection, a review panel with a mock document placeholder, **Approve** (`VerifyUser`) and **Reject** (reason modal → `RejectVerification`). Reachable from the dashboard queue and from a new **Verification queue** button on User Management.
- **Roles & Permissions** — promote a member to administrator, assign a sub-role, remove administrators (last-admin guard), and a role × permission checkbox matrix that saves on change. Reachable from the Settings placeholder until Phase H.

**Deviations:** added `AdminDialogs.cs` (not in the original inventory) for the reusable reason modal; `DemoUser` gained `AdminRole`.

### Phase D — Task Management (delivered)

**Files delivered:** `TaskManagementPage.cs`; `DemoDataStore.Admin.cs` gains `FeaturedTasks`, `PausedTasks`, `ArchiveAdminTask`, `RestoreAdminTask`, `ToggleFeatured`, `TogglePaused` (and resets them in `ResetDemoData`).

**Behaviour:**
- Unified table across **Available / Posted / Archived** with search, status filter (including **Featured**, **Paused**, **Reported**), category and barangay filters, and sorting (deadline, price, title).
- Row actions: View, Archive/Restore, Delete (confirm). Bulk archive/restore/delete on selected rows.
- Right-hand detail panel: task summary, description, deadline, status pills, Feature/Unfeature, Pause/Resume, Archive/Restore, Delete, and a "View in user app" demo toast.
- Archive/restore reuses the shared `PostedTasks`/`AvailableTasks`/`ArchivedTasks` lists, so the user app's My Tasks, Archived Tasks, Dashboard and Find Tasks reflect the change immediately.

**Deviations:** feature/pause are admin-side curation flags; a user-app "Featured" badge is deferred as a later enhancement. No other deviations.

### Phase E — Category + Barangay Management (delivered)

**Files delivered:** `CategoryManagementPage.cs`, `BarangayManagementPage.cs`; `AdminDialogs.cs` gained `AdminCategoryDialog` / `AdminBarangayDialog`; `DemoCategory` gained `Accent`; `DemoDataStore.Admin.cs` gained `ActiveCategoryNames()` / `ActiveBarangayNames()` and the category seeds now carry accent colours.

**Behaviour:**
- **Category Management** — add/edit/delete (with duplicate guard), inline active/paused toggle, reorder up/down, icon and task count per row.
- **Barangay Management** — add/edit (with duplicate guard), active toggle, city + task/member counts per row.
- **Cross-sync** — Form1 category tiles, Find Tasks category checkboxes and barangay filters, Post Task category/barangay pickers, Edit Task pickers, My Tasks category filter and Form2 registration barangay dropdown all read the shared lists. Form1 rebuilds its tiles on activation so admin edits appear immediately.

**Deviations:** Find Tasks keeps a 6-slot category filter (layout constraint); categories created from the admin default to the blue accent unless the icon/name is edited.

### Phase F — Report Management + Disputes + Activity Log (delivered)

**Files delivered:** `ReportManagementPage.cs`, `DisputesPage.cs`, `ActivityLogPage.cs`; `AdminNavigationService.cs` maps all three routes.

**Behaviour:**
- **Report Management** — search plus status/severity/target filters; report list with severity and status pills; detail panel with description, resolution, **Open target**, **Resolve** (reason modal), **Dismiss**, **Escalate**, and context actions **Suspend user** / **Archive task**. Header link to **Disputes**.
- **Disputes** — search + status filter; dispute list; detail with poster/doer/payment links, **Resolve** (reason modal), **Refund**, **Dismiss**.
- **Activity Log** — search, category/actor/sort filters, pagination (12 per page) and a table of timestamp, action, detail, actor and category for every mutation written to the shared log.

**Deviations:** none.

### Phase G — Payment Records + Archives (delivered)

**Files delivered:** `PaymentRecordsPage.cs`, `ArchivesPage.cs`; `DemoDataStore.Admin.cs` gains `ArchiveUser` / `RestoreUser` / `DeleteReport`; `ArchivedTasks` seeded with two archived listings; `UserManagementPage` gained an Archive/Restore row action.

**Behaviour:**
- **Payment Records** — four totals cards (recorded, paid, pending, void/refunded), search plus status/method/sort filters, a payment list with status pills, and a detail panel with **Mark paid** (pending only), **Void**, **Refund**, and **Open person →**. After a status change the page re-selects the updated record.
- **Archives** — three tabs (**Tasks**, **Users**, **Resolved reports**) with search, per-row Restore/Reopen and Delete (with confirmation), plus bulk restore/reopen/delete. Archived users are `Deactivated` accounts; resolved/dismissed reports are reopenable.

**Deviations:** "Export" is a demo toast; deletes only affect the in-memory store.

### Phase H — Settings + Polish + QA (delivered)

**Files delivered:** `AdminSettingsPage.cs`; `DemoDataStore.cs` task and payment seeds extracted into factory methods, and `ResetDemoData` now restores tasks, payments, users, catalogues, reports, disputes, activity, permissions and settings; `AdminPlaceholderPage` simplified to a generic contextual fallback.

**Behaviour:**
- **Settings** — platform name, support email, default barangay, auto-approve threshold, session timeout, require-verification and maintenance toggles; admin profile (name/email, demo password change); **Roles & Permissions** shortcut; **Reset demo data** with confirmation; Save/Cancel with validation and inline feedback.
- **Polish** — no stale "Phase" placeholders remain (only the contextual User Detail fallback); every admin route has a real page; empty states, demo hints and confirmation dialogs are in place across the portal.

**Deviations:** the admin profile name/email and password are display-only (demo); settings persist in memory only.

**Final status:** all 15 admin pages delivered (12 planned + Verification Queue, Roles & Permissions, Disputes); `dotnet build` → **0 warnings / 0 errors**.

---

## Gap-closure additions (delivered)

**Files delivered:** `AdminAnnouncementsPage.cs`, `SupportTicketsPage.cs`, `AdminProfilePage.cs`, `AnnouncementDialog.cs`; `AdminRoute` + `AdminShellForm` (sidebar now 13 items) + `AdminNavigationService` updated; `DemoAnnouncement` model plus announcement and ticket store actions.

**Behaviour:**
- **Announcements** — publish, pause and delete member notices; active announcements appear in the user Notifications page under an **Announcements** filter.
- **Support Tickets** — search + status filters, ticket list, detail with **Reply**, **Resolve** and **Close**; replies surface on the member's Support page.
- **Admin Profile** — the signed-in administrator's name/email (saved to the session), a demo password change, and recent admin activity.

**Final status:** all **18** admin pages delivered (15 + 3 gap-closure additions); `dotnet build` → **0 warnings / 0 errors**.
