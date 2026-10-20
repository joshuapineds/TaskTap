# TaskTap Page Reference

**Scope:** every page and form in the TaskTap WinForms demo — Guest, User, and Admin.
**Status:** current (matches the delivered codebase).
**Related:** [`navigation-blueprint.md`](navigation-blueprint.md) (user-app sitemap) · [`admin-blueprint.md`](admin-blueprint.md) (admin portal).

TaskTap has three presentation layers. Each layer has one entry point and one navigation owner:

| Layer | Entry | Host window | Navigation owner | Page base |
|---|---|---|---|---|
| Guest | `Form1` (landing) | standalone `Form`s | direct `Show()` / `Hide()` | `Form` |
| User | `Form2` (login/register) | `MainShellForm` | `NavigationService` | `AppPage : UserControl` |
| Admin | `Form2` → `AdminLoginForm` | `AdminShellForm` | `AdminNavigationService` | `AdminPage : UserControl` |

**Page count:** 11 guest screens (7 core forms + 4 info articles) + 27 user-shell pages + 18 admin pages = **56 pages**.

---

## 1. Guest pages (11 screens)

Guest screens are standalone forms. They hide themselves, show the next form, and re-show on `FormClosed` (guarded with `IsDisposed`).

| # | Page | File | Opens from | What it does | Main actions |
|---|---|---|---|---|---|
| 1 | **Landing** | `Form1.cs` | App entry (`Program.cs`) | Marketing home: hero search, popular categories, "Available Near You", community trust, CTA, footer. | Login / Sign Up → `Form2` · Browse Tasks → `BrowseTasksForm` · category tiles filter the nearby tasks · task cards → `TaskDetailsForm` · How It Works / About · logo → landing |
| 2 | **Login / Register** | `Form2.cs` | Form1 "Login" / "Sign Up" | Split login + registration with a shared mock session. | Login → `MainShellForm` · Register → `MainShellForm` · Forgot password → `PasswordRecoveryForm` · **Admin sign in → `AdminLoginForm`** |
| 3 | **Password Recovery** | `PasswordRecoveryForm.cs` | Form2 "Forgot password?" | Collects an email/phone and confirms a demo reset link. | Send reset link (demo toast) · back to login |
| 4 | **Browse Tasks (guest)** | `BrowseTasksForm.cs` | Form1 "Browse Tasks" | Guest marketplace: search, filters (category, barangay, price, deadline), sort, list + map. | Filter/sort · View Details → `TaskDetailsForm` · Post a Task → `Form2` · How It Works / About |
| 5 | **Task Details (guest)** | `TaskDetailsForm.cs` | Browse Tasks "View Details" | Guest task preview with description, requirements, approximate location, poster. | Request task → `Form2` (continues to the task after login) · report · How It Works / About |
| 6 | **How It Works** | `HowItWorksForm.cs` *(base: `PublicInfoFormBase.cs`)* | Form1 / public nav | Explains the TaskTap flow for doers and posters. | Browse Tasks · Join TaskTap → `Form2` · About · logo → landing |
| 7 | **About** | `AboutForm.cs` *(base: `PublicInfoFormBase.cs`)* | Form1 / public nav | Mission, values, and the community story. | Browse Tasks · Join TaskTap → `Form2` · How It Works · logo → landing |
| 8 | **Help / FAQ · Guidelines · Terms · Privacy** | `InfoArticleForm.cs` *(base: `PublicInfoFormBase.cs`)* | Form1 footer | Renders the four help/policy articles from `InfoArticleCatalog`. | Browse Tasks · Join TaskTap → `Form2` · About · logo → landing |

---

## 2. User shell pages (27)

Hosted by `MainShellForm` (sidebar + topbar + content host). `NavigationService` swaps pages, keeps a back stack, and owns logout.

### 2.1 Global routes (sidebar, 6 + Post Task CTA)

The sidebar is **Dashboard · Find Tasks · My Tasks · Messages · Payments · Profile**, with **Post Task** as a primary CTA above the nav and **Notifications** reached from the topbar bell.

| Route | Page | File | What it does | Main actions |
|---|---|---|---|---|
| `Route.Dashboard` | **Dashboard** | `DashboardPage.cs` | Role-aware home: greeting + attention count, action-needed chips, Hiring/Working toggle, real KPI cards, task list, live recent activity, tip card. | Chips → Manage Requests / Messages / My Tasks / My Disputes / Saved Tasks · Post a Task · stat cards · task rows · activity rows |
| `Route.FindTasks` | **Find Tasks** | `FindTasksPage.cs` | Signed-in marketplace with list + map. | Search, category checkboxes, price, deadline, barangay, nearby-only, sort · View Details → `UserTaskDetailsPage` · Post a Task · Saved |
| `Route.PostTask` | **Post Task** | `PostTaskPage.cs` | 4-step create flow (Basics, Details, Compensation, Schedule) with live preview; opened from the sidebar CTA. | Save draft · Continue/Back · Publish → My Tasks |
| `Route.MyTasks` | **My Tasks** | `MyTasksPage.cs` | Poster's task workspace with tabs and stats. | Tabs (Posted, Requested, Accepted, In Progress, Completed) · search/category/sort · Edit Task · View Details · View Request(s) · Archived Tasks |
| `Route.Messages` | **Messages** | `MessagesPage.cs` | Conversation list + chat pane. | Search, All/Unread/Tasks filters · select chat · send message · New message · Safety tips |
| `Route.PaymentHistory` / `Route.Earnings` | **Payments** | `PaymentHistoryPage.cs` / `EarningsPage.cs` | Payments hub with a **History / Earnings** segmented switcher (`PaymentsTabs`). | History: totals, filters, rows → Payment Record · Record Payment · Earnings: paid/pending totals + rows |
| `Route.Profile` | **Profile** | `ProfilePage.cs` | Reputation and account hub. | Edit Profile → Account Settings · View public profile · View all reviews · Account settings / Reviews / Archived tasks / Disputes / Saved / Support / Help |
| `Route.Notifications` | **Notifications** (topbar bell) | `NotificationsPage.cs` | Activity center with actionable rows and preferences. | Filters (All, Unread, Tasks, Messages, Payments, Announcements) · period · Mark all read · row actions |

### 2.2 Contextual routes (16)

| Route | Page | File | Opens from | What it does | Main actions |
|---|---|---|---|---|---|
| `Route.MyRequests` | **Requests I Sent** | `MyRequestsPage.cs` | My Tasks | Doer-side view of requests the user sent. | Filter/sort · withdraw/cancel · open request |
| `Route.ManageRequests` | **Requests Received** | `ManageRequestsPage.cs` | My Tasks / task card | Applications on a posted task. | Shortlist · Accept · Decline · view applicant · View/Edit task |
| `Route.EditTask` | **Edit Task** | `EditTaskPage.cs` | My Tasks "Edit Task" | 4-step edit flow for a posted task. | Update basics/details/compensation/schedule · task controls · save |
| `Route.ArchivedTasks` | **Archived Tasks** | `ArchivedTasksPage.cs` | My Tasks | History of archived tasks. | Filter/search · restore · delete |
| `Route.AccountSettings` | **Account Settings** | `AccountSettingsPage.cs` | Profile "Edit Profile" | Profile, contact, password, notifications, privacy sections. | Edit fields · Save/Cancel · "All changes saved" |
| `Route.PublicProfile` | **Public Profile** | `PublicProfilePage.cs` | Reviews, Manage Requests, Task Details poster, Messages, Payment Record, My Disputes | Another member's public profile, rendered from their `DemoUser` record. | About, skills, reviews, activity · message/share/report · trust cards |
| `Route.Reviews` | **Reviews** | `ReviewsPage.cs` | Profile "View all" | Reputation summary and review list. | Average + breakdown · Received/Given tabs · search/rating/sort · review actions |
| `Route.PaymentHistory` | **Payment History** | `PaymentHistoryPage.cs` | Profile | Recorded payments and totals. | Stats · filters · payment rows → Payment Record · breakdown · Record Payment |
| `Route.PaymentRecord` | **Payment Record** | `PaymentRecordPage.cs` | Payment History row | One payment's details, receipt and context. | View details · Edit Payment Record → `RecordPaymentPage` · back to history |
| `Route.RecordPayment` | **Record Payment** | `RecordPaymentPage.cs` | Payment History / Payment Record | 4-section record form (Task & people, Details, Note, Confirmation). | Fill form · preview updates · save → history |
| `Route.RateReview` | **Rate & Review** | `RateReviewPage.cs` | Payment Record | Close the loop after a completed task. | Star rating · title · review text · tags · submit |
| `Route.UserTaskDetails` | **Task Details (signed-in)** | `UserTaskDetailsPage.cs` | Find Tasks / Dashboard / My Tasks | Signed-in task detail with map and poster. | Request to perform · save · report · view poster profile · **Mark complete / Cancel task** (poster) |
| `Route.SavedTasks` | **Saved Tasks** | `SavedTasksPage.cs` | Profile / Find Tasks "Saved" | Tasks the member saved with ♡. | View → User Task Details · Remove · empty state |
| `Route.MyDisputes` | **My Disputes** | `MyDisputesPage.cs` | Profile | Disputes the member opened on completed tasks. | Status + resolution · Open a dispute → Archived Tasks |
| `Route.Earnings` | **Earnings** | `EarningsPage.cs` | Profile | Payments recorded to the member as a doer. | Totals (paid/pending/records) · payment rows |
| `Route.Support` | **Support** | `SupportPage.cs` | Profile | Send a support request and track replies. | Category/subject/message · Send · My tickets |

### 2.3 Info & policy routes (4)

| Route | Page | File | Opens from | What it does |
|---|---|---|---|---|
| `Route.HelpFaq` | **Help & FAQ** | `InfoArticlePage.cs` | Profile "Help & policies" | Answer-style help article. |
| `Route.CommunityGuidelines` | **Community Guidelines** | `InfoArticlePage.cs` | Profile "Help & policies" | Community rules. |
| `Route.Terms` | **Terms of Service** | `InfoArticlePage.cs` | Profile "Help & policies" | Terms summary. |
| `Route.Privacy` | **Privacy Policy** | `InfoArticlePage.cs` | Profile "Help & policies" | Privacy summary. |

All four render from `InfoArticleCatalog` (`InfoArticle.cs`), so the copy lives in one place for both the guest form and the signed-in page.

---

## 3. Admin pages (18)

Hosted by `AdminShellForm` (sidebar + topbar with a bold **ADMIN** badge). `AdminNavigationService` swaps pages and keeps a back stack.

### 3.1 Entry

| # | Page | File | Opens from | What it does | Main actions |
|---|---|---|---|---|---|
| 1 | **Admin Login** | `AdminLoginForm.cs` | Form2 "Admin sign in" | Demo admin sign-in (any email + password). | Sign in → `AdminShellForm` · Back to TaskTap → `Form2` |

### 3.2 Global routes (sidebar, 13)

| Route | Page | File | What it does | Main actions |
|---|---|---|---|---|
| `AdminRoute.Dashboard` | **Admin Dashboard** | `AdminDashboardPage.cs` | KPIs, needs-attention queue, recent activity, newest members, quick actions, charts. | Jump to verification/reports/disputes/users · quick actions · chart views |
| `AdminRoute.Users` | **User Management** | `UserManagementPage.cs` | Searchable, filterable, paginated member directory. | View · Suspend/Activate · Verify · Archive/Restore · Delete · bulk suspend/verify · Verification queue |
| `AdminRoute.Tasks` | **Task Management** | `TaskManagementPage.cs` | All listings (Available / Posted / Archived) with moderation. | Filters (status, category, barangay, Featured, Paused, Reported) · View · Feature/Pause · Archive/Restore · Delete · bulk actions |
| `AdminRoute.Categories` | **Category Management** | `CategoryManagementPage.cs` | The category catalogue the user app reads. | Add/Edit/Delete · active toggle · reorder up/down · task counts |
| `AdminRoute.Reports` | **Report Management** | `ReportManagementPage.cs` | Moderation report triage. | Filters (status, severity, target) · Resolve (reason) · Dismiss · Escalate · Suspend user · Archive task · Open target · Disputes |
| `AdminRoute.Payments` | **Payment Records** | `PaymentRecordsPage.cs` | Recorded payments and totals. | Totals cards · filters/sort · Mark paid · Void · Refund · Open person · export (demo) |
| `AdminRoute.Archives` | **Archives** | `ArchivesPage.cs` | Soft-deleted tasks, users, and resolved reports. | Tabs (Tasks, Users, Resolved reports) · search · Restore/Reopen · Delete · bulk actions |
| `AdminRoute.Barangays` | **Barangay Management** | `BarangayManagementPage.cs` | The service-area catalogue the user app reads. | Add/Edit · Deactivate/Activate · city + task/member counts |
| `AdminRoute.ActivityLog` | **Activity Log** | `ActivityLogPage.cs` | Audit trail of every admin mutation. | Search · category/actor filters · newest/oldest · pagination |
| `AdminRoute.Settings` | **Settings** | `AdminSettingsPage.cs` | Platform configuration and demo controls. | Platform fields · require-verification / maintenance toggles · admin profile · Reset demo data · Roles & Permissions · Save/Cancel |
| `AdminRoute.Announcements` | **Announcements** | `AdminAnnouncementsPage.cs` | Publish and pause member announcements. | New announcement · toggle published/paused · delete |
| `AdminRoute.SupportTickets` | **Support Tickets** | `SupportTicketsPage.cs` | Member support requests. | Search/status filters · Reply · Resolve · Close |
| `AdminRoute.AdminProfile` | **Admin Profile** | `AdminProfilePage.cs` | The signed-in administrator's own account. | Edit name/email · Save · change password (demo) · recent activity |

### 3.3 Contextual routes (4)

| Route | Page | File | Opens from | What it does | Main actions |
|---|---|---|---|---|---|
| `AdminRoute.UserDetail` | **User Detail** | `UserDetailPage.cs` | User Management row / Reports / Disputes | One member's profile with tabbed context. | Suspend/Activate · Verify · Reset password (demo) · Message (demo) · Delete · tabs (Overview, Tasks, Payments, Reviews, Activity, Reports) |
| `AdminRoute.VerificationQueue` | **Verification Queue** | `VerificationQueuePage.cs` | Dashboard / User Management | Pending identity checks with a review panel. | Search/barangay filter · select request · Approve · Reject (reason modal) |
| `AdminRoute.RolesPermissions` | **Roles & Permissions** | `RolesPermissionsPage.cs` | Settings | Admin accounts and the role × permission matrix. | Add administrator · change role · remove (last-admin guard) · toggle permissions |
| `AdminRoute.Disputes` | **Disputes** | `DisputesPage.cs` | Reports / Dashboard / Payment Records | Poster/doer task disagreements. | Search/status filter · Resolve (reason) · Refund · Dismiss · open poster/doer/payments |

---

## 4. Shared infrastructure

These are not pages, but every page depends on them.

| Component | File | Responsibility |
|---|---|---|
| `MainShellForm` | `MainShellForm.cs` | User window: sidebar, topbar, content host, badges, toast, logout → landing. |
| `NavigationService` | `NavigationService.cs` | User navigation: singleton routes, contextual pages, back stack, `GoHome`, `Reset`, `Logout`. |
| `AppPage` | `AppPage.cs` | Base `UserControl` for every user page (`RouteName`, `Nav`, `OnNavigatedTo`). |
| `AdminShellForm` | `AdminShellForm.cs` | Admin window: sidebar (13 routes + ADMIN badge), topbar, content host, toast, sign-out. |
| `AdminNavigationService` | `AdminNavigationService.cs` | Admin navigation: singleton routes, contextual pages, back stack, `GoHome`, `Reset`, `Logout`. |
| `AdminPage` | `AdminPage.cs` | Base `UserControl` for every admin page (`AdminRouteName`, `Nav`, `OnNavigatedTo`). |
| `AppSession` | `AppSession.cs` | Shared mock session: `CurrentUser`, `CurrentAdmin`, `IsAdminSession`, sign-in/out, pending task after login. |
| `DemoDataStore` | `DemoDataStore.cs`, `DemoDataStore.Admin.cs` | Single in-memory source of truth (tasks, users, categories, barangays, reports, disputes, payments, activity, settings) + `DataChanged`. |
| `TaskTapLogo` | `TaskTapLogo.cs` | Shared two-dot brand control used on every shell/header. |
| `AppNavigation` | `AppNavigation.cs` | `GoToLanding()` — closes other windows and returns to `Form1`. |
| `AdminUi` | `AdminUi.cs` | Admin visual helpers (labels, cards, buttons, pills, find). |
| `AdminCharts` | `AdminCharts.cs` | Dependency-free painted bar chart for the admin dashboard. |
| `AdminDialogs` | `AdminDialogs.cs` | Reusable reason / category / barangay modal dialogs. |
| `PublicInfoFormBase` | `PublicInfoFormBase.cs` | Shared layout for How It Works and About. |

---

## 5. Cross-portal sync (admin → user app)

| Admin action | Where it shows in the user app |
|---|---|
| Archive / restore / delete a task | My Tasks, Archived Tasks, Dashboard "Tasks for You", Find Tasks |
| Mark a payment paid / void / refund | Profile → Payment History, Payment Record |
| Add / rename / reorder / pause a category | Form1 category tiles, Find Tasks category filters, Post Task picker, Edit Task picker, My Tasks filter |
| Add / edit / deactivate a barangay | Form2 registration dropdown, Find Tasks barangay filters, Post Task picker, Edit Task picker |
| Suspend / activate / verify a user | User Detail status, public profile, request/message eligibility (demo) |
| Resolve / dismiss / escalate a report | Admin lists + Activity Log only (no user-app report history in the demo) |
| Resolve / refund / dismiss a dispute | Admin lists + Activity Log (refund also affects the payment record); status and resolution show on **My Disputes** |
| Publish / pause an announcement | Notifications → **Announcements** filter |
| Approve / reject a verification request | Account Settings verification status |
| Reply to / resolve a support ticket | Support → **My tickets** status + reply |
| Complete / cancel a posted task | My Tasks → Completed, Archived Tasks, admin Task Management |
| Every mutation | `ActivityLog` (admin Activity Log page) |

---

## 6. Navigation cheat-sheet

```
Guest
  Form1 (Landing)
   ├─ How It Works ......... HowItWorksForm
   ├─ About ................ AboutForm
   ├─ Browse Tasks ......... BrowseTasksForm ──► TaskDetailsForm ──"Request"──► Form2
   ├─ Login / Sign Up ...... Form2
   │                          ├─ Password Recovery ..... PasswordRecoveryForm
   │                          ├─ (login/register) ...... MainShellForm → Dashboard
   │                          └─ "Admin sign in" ....... AdminLoginForm → AdminShellForm
   └─ (task cards) ......... TaskDetailsForm

User shell (MainShellForm)
  [＋ Post a Task CTA]  Dashboard · Find Tasks · My Tasks · Messages · Payments · Profile
  (topbar bell ──► Notifications)
  My Tasks  ├─ MyRequests          (Requests I sent)
            ├─ ManageRequests      (Requests received)
            ├─ EditTask
            └─ ArchivedTasks ──"Open dispute"──► MyDisputes
  Payments  ├─ PaymentHistory (History tab) ──► PaymentRecord ──► RecordPayment
            └─ Earnings (Earnings tab)
  Profile   ├─ AccountSettings ──"Request verification"
            ├─ PublicProfile
            ├─ Reviews
            ├─ MyDisputes
            ├─ SavedTasks
            ├─ Support
            └─ HelpFaq · CommunityGuidelines · Terms · Privacy
  Find Tasks / Dashboard ──► UserTaskDetails

Admin shell (AdminShellForm)
  Dashboard · Users · Tasks · Categories · Reports · Payments · Archives · Barangays · Activity Log · Settings ·
  Announcements · Support Tickets · Admin Profile
  Users    ├─ UserDetail           (contextual)
            └─ VerificationQueue    (contextual)
  Reports  └─ Disputes             (contextual)
  Settings └─ RolesPermissions     (contextual)
```

---

## 7. Maintenance notes

- **Adding a user page:** add a `Route` value, create an `AppPage`, and register it in `NavigationService.Create` (global) or navigate to `new YourPage()` (contextual). Update `MainShellForm.UpdateChrome` for the title/highlight.
- **Adding an admin page:** add an `AdminRoute` value, create an `AdminPage`, register it in `AdminNavigationService.Create`, and update `AdminShellForm.UpdateChrome` and `NavItems` if it belongs in the sidebar.
- **Data changes:** mutate through `DemoDataStore` methods so `DataChanged` fires and the dashboard/activity log stay in sync; never keep a page-local copy of shared data.
- **Build gate:** `dotnet build -nologo` must stay at **0 warnings / 0 errors**.
