# TaskTap Navigation Blueprint (Phase 1 + Phase 2)

**Status:** LOCKED — approved defaults applied
**Source:** `Audit_Final.docx` (9-Phase Remediation Plan)
**Scope:** Sitemap, task-ownership vocabulary, and full route matrix for the WinForms frontend demo.
**Note:** This is a planning artifact only. No C# code was written or modified to produce it.

---

## 0. Locked decisions (recommended defaults, all approved)

| # | Decision | Locked value |
|---|---|---|
| 1 | Shell architecture | **Single main window** with a content host; signed-in pages become `UserControl`s swapped in the host. Modal `Form`s (`ShowDialog`) only for previews/confirmations. |
| 2 | Task-ownership vocabulary | **"My Tasks" = tasks I posted**, with sub-views **"Requests I sent"** (doer) and **"Requests received"** (poster) as tabs/subpages, and **"Archived"** as a My Tasks history tab. "Requests I sent" is a tab under My Tasks, not a separate sidebar item. |
| 3 | Return/back semantics | **Origin preservation.** Back from any sibling (especially Find Tasks) restores the exact originating page with filters/tab/scroll intact. **"Dashboard" always means the canonical dashboard**; breadcrumbs show the true parent hierarchy. |
| 4 | Guest vs signed-in discovery | Keep `BrowseTasksForm` (guest) and `FindTasksForm` (signed-in) as **two distinct audience experiences sharing one task repository**. |
| 5 | Logout target | Logout **clears the mock session and returns to `Form1` landing**, closing/hiding all open signed-in windows. |
| 6 | Toast placeholders policy | Wire **real routes for all global nav items now**; secondary/card actions become visibly **labeled "Demo" or disabled** until implemented. No silent dead ends. |
| 7 | Persistence | This file is the fixed reference for Phases 3–9. |

---

## 1. Sitemap (Phase 1)

### 1.1 Confirmed global destinations (7)

`Dashboard` · `Find Tasks` · `Post Task` · `My Tasks` · `Messages` · `Notifications` · `Profile`

### 1.2 Information architecture (one meaning, one primary parent)

```
TaskTap
├─ Guest (unauthenticated)                          [Form1 → Form2 → ...]
│   ├─ Landing ........................ Form1.cs
│   │   ├─ How It Works ................ HowItWorksForm
│   │   ├─ About ....................... AboutForm
│   │   ├─ Browse Tasks (guest) ........ BrowseTasksForm
│   │   │   └─ Task Details (preview) .. TaskDetailsForm ──"Request"──► Login/Register
│   │   └─ Login / Register ............ Form2.cs
│   │       ├─ Password Recovery ....... PasswordRecoveryForm
│   │       └─ (login) ──► Dashboard
│
└─ Signed-in app shell
    ├─ Dashboard (home) ................ UserDashboardForm
    ├─ Find Tasks (signed-in) .......... FindTasksForm ──► UserTaskDetailsForm
    ├─ Post Task ....................... PostTaskForm
    ├─ My Tasks ........................ MyTasksForm            ← poster's tasks
    │   ├─ Requests I sent ............. MyRequestsForm         ← doer view (currently "Requested"/"My Requests")
    │   ├─ Requests received ........... ManageRequestsForm     ← applications on my posts
    │   │   └─ Edit Task ................ EditTaskForm
    │   └─ Archived Tasks (history) .... ArchivedTasksForm      ← tab/filter under My Tasks, not top-level
    ├─ Messages ........................ MessagesForm
    ├─ Notifications ................... NotificationsForm
    └─ Profile ......................... ProfileForm
        ├─ Account Settings ............. AccountSettingsForm
        ├─ Reviews ...................... ReviewsForm
        ├─ Payment History .............. PaymentHistoryForm     ← single canonical home
        │   ├─ Payment Record ............ PaymentRecordForm
        │   ├─ Record Payment ............ RecordPaymentForm
        │   └─ Rate / Review ............. RateReviewForm
        └─ Public Profile (view another user) .. PublicProfileForm
```

### 1.3 Vocabulary lock

| Current / ambiguous | Locked meaning | Primary parent |
|---|---|---|
| "My Tasks" | Tasks **I posted** (poster view) | top-level |
| "Requested" / "My Requests" | **Requests I sent** to others (doer view) | My Tasks (tab) |
| "Manage Requests" | **Requests received** on my posted tasks | My Tasks (per-task) |
| "Archived Tasks" | History/visibility action on my tasks | My Tasks history |
| "Reviews" | Received/given reviews | Profile |
| "Payment History" | All payment records | Profile > Account (+ contextual task links) |
| "Browse Tasks" (guest) vs "Find Tasks" (signed-in) | Separate by audience; one shared data source | Landing vs Dashboard |

---

## 2. Route matrix (Phase 2)

### Legend

| Glyph | Meaning |
|---|---|
| `●` | Working route — navigates and returns correctly |
| `◐` | Navigates, but lifecycle/return is inconsistent (origin closed on return, hidden without restore, or the item *closes back to the caller* instead of the canonical destination) |
| `✕` | Dead end — renders but is toast-only or a no-op |
| `○` | Current page (the active route / "stays") |

### 2.1 Global sidebar items across signed-in forms

| Screen | Dash | Find | Post | My Tasks | Msgs | Notif | Profile |
|---|---|---|---|---|---|---|---|
| UserDashboardForm (home) | ○ | ● | ● | ● | ● | ● | ● |
| FindTasksForm | ● | ○ | ● | ● | ● | ● | ● |
| UserTaskDetailsForm | ◐ | ✕ | ● | ● | ● | ● | ● |
| PostTaskForm | ◐ | ◐ | ○ | ● | ● | ● | ● |
| MyTasksForm | ◐ | ◐ | ● | ○ | ● | ● | ● |
| MyRequestsForm | ◐ | ● | ● | ● | ● | ● | ● |
| ManageRequestsForm | ◐ | ◐ | ● | ○¹ | ● | ● | ● |
| EditTaskForm | ◐ | ◐ | ✕ | ○¹ | ● | ● | ● |
| ArchivedTasksForm | ✕ | ● | ● | ● | ● | ● | ● |
| MessagesForm | ◐ | ● | ● | ● | ○ | ✕ | ✕ |
| NotificationsForm | ◐² | ● | ● | ● | ● | ○ | ● |
| ProfileForm | ◐² | ● | ● | ● | ● | ● | ○ |
| AccountSettingsForm | ✕ | ● | ● | ● | ● | ● | ◐² |
| PublicProfileForm | ◐² | ● | ● | ● | ● | ● | ✕ |
| ReviewsForm | ✕ | ● | ● | ● | ● | ● | ● |
| PaymentHistoryForm | ✕ | ● | ● | ● | ● | ● | ● |
| PaymentRecordForm | ✕ | ● | ● | ✕ | ● | ● | ● |
| RecordPaymentForm | ✕ | ● | ● | ● | ● | ● | ● |
| RateReviewForm | ◐³ | ● | ● | ◐³ | ● | ● | ● |

Footnotes:
- **¹** `My Tasks` in ManageRequestsForm / EditTaskForm is a **no-op** (`return;`) — should return to parent My Tasks, currently does nothing.
- **²** `Dashboard` in NotificationsForm / ProfileForm / AccountSettingsForm / PublicProfileForm = `Close()` **if opened as a child**, otherwise toast. "Dashboard" actually means "go back to whoever opened me."
- **³** RateReviewForm treats `Dashboard` and `My Tasks` as `Close()` → returns to caller (PaymentRecordForm), not the real destination.

**Exposed patterns (from the audit):**
- **Dashboard is inconsistent** — real route from the shell and several forms, caller-dependent close from 4 forms, toast dead end from 6 forms, and `FormClosed → Close()` (origin-closing) from 6 forms.
- **Messages** shows Notifications and Profile in its sidebar but both fall to a toast (✕).
- **Logout is broken everywhere** — every signed-in form's `Log out` is `Click → Close()` only (verified in all 19 forms); it never clears a session or returns to Form1/Form2.

### 2.2 Entry & authentication routes

| Control | Source | Destination | Return | Status |
|---|---|---|---|---|
| Browse Tasks / How It Works / About | Form1 | BrowseTasksForm / HowItWorks / About | Form1 (FormClosed→Show) | ● |
| Login / Sign Up | Form1 | Form2 (register flag) | Form1 | ● |
| Task card → preview | Form1 | ShowTaskPreview (modal) | Form1 | ● (`ShowDialog`) |
| Request (in preview) | Form1 | Form2 | Form1 — **task context dropped** | ◐ |
| Login submit | Form2 | UserDashboardForm | Form2 | ● (demo: any non-empty) |
| Register submit | Form2 | **toast only** | — | ✕ |
| Forgot Password | Form2 | PasswordRecoveryForm | Form2 | ● |
| Back | Form2 | Close() | Form1 | ● |
| Admin / Social sign-in | Form2 | toast | — | ✕ demo |

### 2.3 Guest browse routes

| Control | Source | Destination | Status |
|---|---|---|---|
| Card → View Details | BrowseTasksForm | TaskDetailsForm | ● |
| Header Login / Sign Up | BrowseTasksForm | Form2 | ● |
| Header Browse / How It Works / About | BrowseTasksForm | toast | ✕ demo |
| Post Task button | BrowseTasksForm | toast ("sign in to post") | ✕ |
| Save task | BrowseTasksForm | toast | ✕ demo |
| Request to Perform | TaskDetailsForm | Form2 (**always**, no session check) | ◐ |
| Browse Tasks (back) | TaskDetailsForm | Close() → BrowseTasksForm | ● |
| Sign Up / Login | TaskDetailsForm | Form2 | ● |

### 2.4 Subpage, detail & card-action routes (signed-in)

| Control | Source → Destination | Status |
|---|---|---|
| Card → details | FindTasksForm → UserTaskDetailsForm | ● |
| Dashboard card → details | UserDashboardForm → TaskDetailsForm | ● |
| Edit Task | MyTasksForm → EditTaskForm | ● |
| Manage Requests | MyTasksForm → ManageRequestsForm | ● |
| My Requests | MyTasksForm → MyRequestsForm | ● |
| Payment History | MyTasksForm → PaymentHistoryForm | ● |
| Archived Tasks | MyTasksForm → ArchivedTasksForm | ● |
| Edit Task | ManageRequestsForm → EditTaskForm | ● |
| Account Settings / Public Profile / Payment History / Archived / Reviews | ProfileForm → each | ● |
| View all reviews | ProfileForm → toast | ✕ demo |
| Payment Record / Record Payment | PaymentHistoryForm → each | ● |
| Record Payment / Rate-Review | PaymentRecordForm → each | ● |
| View task (payment) | PaymentRecordForm → toast | ✕ demo |
| Task details (archived) | ArchivedTasksForm → toast | ✕ demo |
| Poster profile | UserTaskDetailsForm → toast | ✕ demo |
| Request to perform (signed-in) | UserTaskDetailsForm → toast | ✕ |
| Withdraw request | MyRequestsForm → confirm modal | ● (`ShowDialog`) |
| Dashboard "View All →" (tasks) | UserDashboardForm → FindTasksForm | ● |
| Dashboard "View All →" (activity) | UserDashboardForm → toast | ✕ demo |

---

## 3. Phase 1 / Phase 2 done-criteria status

- ✅ Sitemap exists; each label has one meaning and one primary parent.
- ✅ Guest vs signed-in discovery documented separately.
- ✅ Route matrix covers sidebar items, breadcrumbs, back/close, topbar shortcuts, card actions, and modal actions.
- ✅ Every control marked working / demo-only / dead end.
- ✅ Sibling/detail-page policy chosen (origin preservation + canonical Dashboard).
- ✅ Logout specified (clear session → Form1 landing).
- ✅ No silent dead ends remain as a *goal*; remediation targets are enumerated in §2.

**Remaining (Phase 3+):** implement the single-shell navigation owner, eliminate the ◐/✕ cells above, and replace demo-only toasts with labeled/disabled controls.

---

## 4. Phase 3 readiness

Blueprint is locked and complete. Phase 3 will introduce the application-level navigation owner (`Navigate`, `Back`, `ReturnToHome`) and the single content-host shell, then migrate the signed-in forms off ad-hoc `Hide`/`Show`/`Close` behavior. No C# will be written until the Phase 3 plan is approved.

---

## 5. Gap-closure additions (delivered)

The nine gap-closure phases added the following routes; see [`gap-closure-blueprint.md`](gap-closure-blueprint.md) and [`Page-Reference.md`](Page-Reference.md) for the authoritative list.

### 5.1 Guest
- Footer links **Help / FAQ**, **Community Guidelines**, **Terms of Service**, **Privacy Policy** open `InfoArticleForm` (four articles rendered from `InfoArticleCatalog`).

### 5.2 Signed-in
- `Route.SavedTasks` — Saved Tasks (Profile, Find Tasks "Saved").
- `Route.MyDisputes` — My Disputes (Profile; "Open dispute" on completed archived tasks).
- `Route.Earnings` — Earnings (Profile).
- `Route.Support` — Support (Profile).
- `Route.HelpFaq`, `Route.CommunityGuidelines`, `Route.Terms`, `Route.Privacy` — Info articles (Profile "Help & policies").

### 5.3 Admin
- `AdminRoute.Announcements` — publish/pause member announcements (surfaced in the user Notifications).
- `AdminRoute.SupportTickets` — reply, resolve and close member tickets.
- `AdminRoute.AdminProfile` — the signed-in administrator's own account.

### 5.4 Poster task controls
- Complete/cancel a posted task from **Task Details** and **Manage Requests**; outcomes land in Archived Tasks and My Tasks → Completed.

### 5.5 Revised sidebar & role-aware dashboard
- The signed-in sidebar is **Dashboard · Find Tasks · My Tasks · Messages · Payments · Profile** (6), with **Post Task** as a primary CTA above the nav and **Notifications** moved to the topbar bell.
- **Payments** is a hub: `PaymentHistoryPage` and `EarningsPage` share a **History / Earnings** segmented switcher and both highlight the Payments item.
- The **Dashboard** is role-aware (**Hiring / Working**): real KPI cards, an action-needed chip strip (requests, unread messages, due-soon, disputes, saved), live recent activity from the store, and a role-aware tip card. The former "activity not implemented" dead end is gone.
