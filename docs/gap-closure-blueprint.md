# TaskTap Gap Closure Blueprint

**Status:** APPROVED — executing Phase 1
**Scope:** Close the genuine page/flow gaps across the Guest, User and Admin layers (the 41 pages already shipped are listed in [`Page-Reference.md`](Page-Reference.md)).
**Related:** [`Page-Reference.md`](Page-Reference.md) · [`navigation-blueprint.md`](navigation-blueprint.md) · [`admin-blueprint.md`](admin-blueprint.md)

---

## 0. Locked decisions

| # | Decision | Locked value |
|---|---|---|
| 1 | Scope | Phases 1–6 fully, plus **Admin Support Tickets** in Phase 7 (two-sided support loop). Content Moderation page skipped — Reports already covers Message/Review targets. |
| 2 | Legal copy | **Short, clean one-screen summaries** (4–6 sections each). |
| 3 | Task completion entry points | **Task detail page + Manage Requests page.** |
| 4 | Guest saves | **Carry guest saves into the signed-in session** after login. |
| 5 | Verification queue | Admin queue lists **requested + unverified active** users. |
| 6 | Announcements | **Surface in the user Notifications page** with an Announcements filter. |
| 7 | Blueprint file | `docs/gap-closure-blueprint.md`. |
| 8 | Execution order | **Phase 1 → 8 sequentially.** |
| 9 | Architecture | Same rules as the existing app: `AppPage` UserControls in the user shell, `AdminPage` UserControls in the admin shell, shared `AppSession` + `DemoDataStore`, no hidden forms, `0 warnings / 0 errors` build gate. |

**Out of scope (deliberate demo scope):** checkout/payment gateway, real email/SMS delivery, onboarding/role selection, analytics page, 404, persistence, native exporters, content-moderation page.

---

## 1. Phase 1 — Help & Policy Center (A1–A3)

**Goal:** give the app a real policy surface and stop the footer links opening the wrong page.

- **New:** `InfoArticle` (Key, Eyebrow, Title, Summary, Sections) + `InfoArticleCatalog` with four articles: **Help / FAQ**, **Community Guidelines**, **Terms of Service**, **Privacy Policy**.
- **New:** `InfoArticleForm : PublicInfoFormBase` (guest) and `InfoArticlePage : AppPage` (signed-in) — one renderer each, driven by the catalog.
- **Routes:** `Route.HelpFaq`, `Route.CommunityGuidelines`, `Route.Terms`, `Route.Privacy` (registered in `NavigationService.Create`; titled in `MainShellForm.UpdateChrome`).
- **Fix:** `Form1.NavigateToPublicInfo` currently sends anything that isn't "How It Works"/"About" to `HowItWorksForm`, so **Help / FAQ and Community Guidelines open How It Works**. Map each link to its article and add **Terms** + **Privacy** to the footer SUPPORT group.
- **Entry (signed-in):** Profile → "Help & policies" → Help/FAQ, which cross-links the other three.
- **Acceptance:** each footer link opens its own article; guest and signed-in versions render the same content; build 0/0.

## 2. Phase 2 — Saved Tasks (A4)

- **Store:** `DemoDataStore.SavedTasks` (`List<DemoTask>`) + `ToggleSaved` / `IsSaved`, raising `DataChanged`.
- **New:** `Route.SavedTasks` → `SavedTasksPage : AppPage` (View → `UserTaskDetailsPage`, Remove, empty state).
- **Wire:** the five existing save buttons (`Form1`, `BrowseTasksForm`, `TaskDetailsForm`, `FindTasksPage`, `UserTaskDetailsPage`) to the store instead of page-local booleans.
- **Entry:** Profile account row **Saved tasks**; Find Tasks "Saved" link.
- **Acceptance:** saving on any surface appears on the page; remove works; guest saves carry into the signed-in session.

## 3. Phase 3 — Task Completion & Cancellation (A5)

- **Model:** `DemoTaskOutcome` (Title, Category, Amount, Barangay, Status = Completed/Cancelled, Date, Reason).
- **Store:** `DemoDataStore.TaskOutcomes` + `CompleteTask` / `CancelTask(reason)` / `ReopenTask`; moves the task into the archive and logs a `DemoActivity`.
- **Actions:** on `UserTaskDetailsPage` (poster) and `ManageRequestsPage` (after accepting) — confirm for complete, reason dialog for cancel.
- **Reflect:** `ArchivedTasksPage` merges `TaskOutcomes`; `MyTasksPage` Completed/Cancelled tabs pick them up; admin **Task Management** shows the archived task.
- **Acceptance:** complete/cancel removes the task from active lists, appears in Archived/Completed, logs activity, visible to admin.

## 4. Phase 4 — Verification Requests (A6)

- **Model:** `DemoUser.VerificationRequested` + `RequestedAt`.
- **Store:** `RequestVerification(user, note)`; `VerifyUser` / `RejectVerification` clear the request flag.
- **UI:** Account Settings → "Get verified" action + note dialog.
- **Admin:** `VerificationQueuePage` lists **requested + unverified active** users.
- **Acceptance:** request → appears in admin queue → approve flips Verified and clears the request.

## 5. Phase 5 — My Disputes (A7)

- **New:** `Route.MyDisputes` → `MyDisputesPage : AppPage` listing disputes where the user is poster or doer, with status + resolution.
- **Store:** `OpenDispute(dispute)`.
- **Action:** "Report a problem / Open dispute" on completed/archived tasks (reason + description).
- **Cross-link:** the admin **Disputes** page sees the new record.
- **Acceptance:** opening a dispute shows in the user list and the admin Disputes page.

## 6. Phase 6 — Earnings & Support (B1–B2)

- **Earnings:** `Route.Earnings` → `EarningsPage : AppPage` — doer view from `Payments` where `Person == CurrentUser.FullName` (totals, pending, list). Entry: Profile account row.
- **Support:** `Route.Support` → `SupportPage : AppPage` + `DemoSupportTicket` model + `RequestSupport(ticket)`. Form: category, subject, message; shows my tickets.
- **Acceptance:** earnings totals match payments; a submitted ticket appears in "My tickets".

## 7. Phase 7 — Admin Additions (B3–B4 + Support Tickets)

- **Announcements:** `AdminRoute.Announcements` → `AdminAnnouncementsPage` + `DemoAnnouncement` (title, body, audience, active); publish/pause. Surfaced in the user **Notifications** page (Announcements filter).
- **Admin Profile:** `AdminRoute.AdminProfile` → `AdminProfilePage` (own name/email, change password demo, recent admin activity).
- **Support Tickets:** `AdminRoute.SupportTickets` → `SupportTicketsPage` listing user tickets with reply/resolve/close.
- **Acceptance:** announcement appears in user notifications; admin profile edits persist in memory; tickets resolve.

## 8. Phase 8 — Documentation & QA

- Update `docs/Page-Reference.md`, `navigation-blueprint.md`, `admin-blueprint.md`, and this file's as-built log.
- Final `dotnet build` → 0/0 and an end-to-end walkthrough of the new pages.

---

## 9. As-built log

### Phase 1 — Help & Policy Center (delivered)

**Files delivered:** `InfoArticle.cs` (model + `InfoArticleCatalog`), `InfoArticleForm.cs` (guest renderer), `InfoArticlePage.cs` (signed-in renderer). Edited: `Route.cs`, `NavigationService.cs`, `MainShellForm.cs`, `Form1.cs`, `ProfilePage.cs`.

**Behaviour:** four short articles — **Help & FAQ**, **Community Guidelines**, **Terms of Service**, **Privacy Policy** — render in both the guest and signed-in shells from one content source. The landing footer's SUPPORT group now lists all four and each opens its own article (previously *Help / FAQ* and *Community Guidelines* both opened How It Works). Profile → **Help & policies** opens Help & FAQ, which cross-links the other three.

**Deviations:** none. `dotnet build` → **0 warnings / 0 errors**.

### Phase 2 — Saved Tasks (delivered)

**Files delivered:** `SavedTasksPage.cs`. Edited: `DemoDataStore.cs` (saved list + toggle helpers), `DemoDataStore.Admin.cs` (reset clears saved), `Route.cs`, `NavigationService.cs`, `MainShellForm.cs`, `Form1.cs`, `BrowseTasksForm.cs`, `TaskDetailsForm.cs`, `FindTasksPage.cs`, `UserTaskDetailsPage.cs`, `ProfilePage.cs`.

**Behaviour:** the five ♡ buttons now write to `DemoDataStore.SavedTasks` (title-based identity) instead of page-local booleans, and reflect the saved state when a page is built. `Route.SavedTasks` renders the list with **View** (opens `UserTaskDetailsPage`) and **Remove**, plus an empty state. Entry points: Profile → **Saved tasks** and Find Tasks → **Saved**. Guest saves persist into the signed-in session.

**Deviations:** the list stores `DemoTask` snapshots built from each surface's fields (some surfaces default `Days`/`MapPoint`) and compares by title, because the five surfaces use different local task models. `dotnet build` → **0 warnings / 0 errors**.

### Phase 3 — Task Completion & Cancellation (delivered)

**Files delivered / edited:** `DemoDataStore.cs` (`DemoTaskOutcome` model, `TaskOutcomes`, `CompleteTask` / `CancelTask` / `ReopenTask`), `DemoDataStore.Admin.cs` (reset clears outcomes), `ArchivedTasksPage.cs` (merges outcomes), `MyTasksPage.cs` (Completed tab includes outcomes), `ManageRequestsPage.cs` (Mark complete / Cancel task header actions), `UserTaskDetailsPage.cs` (poster controls in the hero), `AdminDialogs.cs` (`AdminReasonDialog` renamed to the reusable `ReasonDialog`).

**Behaviour:** a poster can mark a posted task complete (confirmation) or cancel it (reason dialog) from the task detail page and from Manage Requests. The task leaves the active lists, enters `ArchivedTasks`, records a `DemoTaskOutcome`, and logs a `DemoActivity`. Completed outcomes appear in My Tasks → Completed and in Archived Tasks (Completed/Cancelled filters); cancelled outcomes appear in Archived Tasks; the admin Task Management shows the archived task.

**Deviations:** cancellation has no dedicated My Tasks tab, so cancelled outcomes surface under Archived Tasks → Cancelled. `dotnet build` → **0 warnings / 0 errors**.

### Phase 4 — Verification Requests (delivered)

**Files edited:** `AdminModels.cs` (`DemoUser.VerificationRequested` / `RequestedAt` / `VerificationNote`; the pending check now includes requests), `DemoDataStore.Admin.cs` (`RequestVerification`; approve/reject clear the request), `AccountSettingsPage.cs` ("Request verification" action + live status label), `VerificationQueuePage.cs` (shows the request date and note).

**Behaviour:** a member requests verification from Account Settings (note captured via `ReasonDialog`); the admin **Verification Queue** lists requested + unverified active users, showing the request note; approving flips Verified and clears the request, rejecting marks the account Unverified and clears it. The dashboard's pending-verification count includes requests.

**Deviations:** none. `dotnet build` → **0 warnings / 0 errors**.

### Phase 5 — My Disputes (delivered)

**Files delivered / edited:** `MyDisputesPage.cs`; `DemoDataStore.Admin.cs` (`NextDisputeId`, `OpenDispute`); `Route.cs`, `NavigationService.cs`, `MainShellForm.cs`, `ArchivedTasksPage.cs` (Open dispute on completed cards), `ProfilePage.cs` (My disputes row).

**Behaviour:** a member opens a dispute from a completed task in Archived Tasks (reason + note); it records a `DemoDispute` (poster = current user, doer resolved from the accepted applicant) and logs a `DemoActivity`. **My Disputes** lists the member's disputes with status and resolution, and the admin **Disputes** page sees the same record.

**Deviations:** only completed tasks expose the action (cancelled/expired do not, matching the blueprint). `dotnet build` → **0 warnings / 0 errors**.

### Phase 6 — Earnings & Support (delivered)

**Files delivered / edited:** `EarningsPage.cs`, `SupportPage.cs`; `AdminModels.cs` (`DemoSupportTicket`); `DemoDataStore.Admin.cs` (`SupportTickets`, `NextTicketId`, `RequestSupport`; reset clears tickets); `Route.cs`, `NavigationService.cs`, `MainShellForm.cs`, `ProfilePage.cs` (Earnings and Support rows).

**Behaviour:**
- **Earnings** — totals (paid, pending, records) and a list of payments recorded to the current member, with status pills; refreshes on `DataChanged` and `OnNavigatedTo`.
- **Support** — a request form (category, subject, message) that creates a `DemoSupportTicket` and logs it, plus a **My tickets** list showing status and replies.

**Deviations:** none. `dotnet build` → **0 warnings / 0 errors**.

### Phase 7 — Admin Additions (delivered)

**Files delivered / edited:** `AdminAnnouncementsPage.cs`, `AdminProfilePage.cs`, `SupportTicketsPage.cs`, `AnnouncementDialog.cs`; `AdminModels.cs` (`DemoAnnouncement`); `DemoDataStore.Admin.cs` (announcements + ticket actions + reset); `AdminRoute.cs`, `AdminShellForm.cs` (3 new nav items), `AdminNavigationService.cs`; `NotificationsPage.cs` (Announcements filter + seeded announcement notices).

**Behaviour:**
- **Announcements** — publish (title, body, audience, active) and pause/delete; active announcements appear in the member Notifications page under a new **Announcements** filter.
- **Admin Profile** — the signed-in admin's name/email (saved to the session), demo password change, and recent admin activity.
- **Support Tickets** — list with search/status filters, detail with reply, resolve and close; replies show on the member's Support page.

**Deviations:** the admin sidebar now has 13 items (the three new pages are sidebar entries). `dotnet build` → **0 warnings / 0 errors**.

### Phase 8 — Documentation & QA (delivered)

**Documentation updated:** `Page-Reference.md` (guest info articles, 8 new user routes, 3 new admin routes, counts and cheat-sheet), `navigation-blueprint.md` (gap-closure addendum), `admin-blueprint.md` (gap-closure addendum + final count), and this file's as-built log.

**QA:** end-to-end walkthrough of every new flow — guest info articles, verification request → admin queue → approve, task complete/cancel → history, saved tasks, disputes → admin resolve, earnings, support → admin reply, announcements → user notifications, admin profile. Final `dotnet build` → **0 warnings / 0 errors**.

### Final as-built summary

All eight phases delivered. New surfaces: 4 guest info articles, 4 signed-in info/article routes, Saved Tasks, My Disputes, Earnings, Support, and 3 admin pages. New models: `InfoArticle` / `InfoArticleCatalog`, `DemoTaskOutcome`, `DemoSupportTicket`, `DemoAnnouncement`. New store APIs: saved tasks, task outcomes, verification requests, disputes, support tickets, announcements. Shared `ReasonDialog` (renamed from `AdminReasonDialog`) is used by both portals.

**Deviations / out of scope (unchanged):** checkout/payment gateway, real email/SMS, onboarding/role selection, analytics page, 404, persistence, native exporters, content-moderation page.

### Post-delivery fixes

- **Earnings crash (fixed).** The `EarningsPage` constructor's `Controls.Add(page)` triggered the resize layout before the three total cards existed, so `RefreshEarnings` indexed an empty `totalValues` list. Fixed with a `totalValues.Count < 3` guard.
- **Other-member profiles.** `PublicProfilePage` now renders any `DemoUser` (`PublicProfilePage(DemoUser)` plus the `ProfileLinks` helper); reviewer names, Manage Requests applicants, the Task Details poster, Messages conversations, Payment Record people and My Disputes counterparts now open the matching public profile.
- **Sidebar IA.** Signed-in nav is now **Dashboard · Find Tasks · My Tasks · Messages · Payments · Profile**; **Post Task** became a primary CTA and **Notifications** moved to the topbar bell. `Payments` groups History and Earnings behind a segmented switcher (`PaymentsTabs`, `NavigationService.Swap`).
- **Role-aware dashboard.** `DashboardPage` was rebuilt with real KPI values, an action-needed chip strip, a Hiring/Working toggle, live recent activity, a role-aware tip card, and no dead-end "activity not implemented" toast.
- **Route resolution crash (fixed).** `NavigationService.Navigate(Route)` only resolved singleton routes, so the new Payments sidebar item — and several `Navigate(Route.PaymentHistory / Reviews / MyRequests / ArchivedTasks / AccountSettings)` call sites — threw `ArgumentOutOfRangeException: Not a singleton route.` Route resolution now caches global pages and creates parameterless contextual pages on demand (`Resolve` / `IsSingleton` / `CreateContextual`).
- **UI polish pass.** Removed the password eye toggles from the member login/register and admin login forms; wired the topbar search (→ Find Tasks), Messages "Safety tips" (→ Community Guidelines) and "View posting tips" (→ Help & FAQ) that previously only toasted; fixed header clipping (Find Tasks, Post a Task), the Record Payment banner/preview overlap, the Payment Record receipt card overlapping the hero, the clipped ₱ amount, the truncated Reviews caption, the clipped Profile rating, the Public Profile verified badge and trust-metric captions, the My Tasks favourite/price cluster and hardcoded tab counts, and gave the Find Tasks map more detail (park, street labels, building insets).
- **Payment History cards + map pins.** Stat-card captions were colliding with the values (re-spaced), and the Payment breakdown rows mispositioned their labels/amounts (both were moved right, so "Cash" and its amount overlapped and the first row never moved). Rows now keep the method name left and a right-aligned amount, with the bar beneath. Map pins now open the matching `UserTaskDetailsPage` instead of only selecting the card.
