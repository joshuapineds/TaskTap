# TaskTap Frontend Demo

Plain HTML/CSS/JS frontend prototype for TaskTap: a local task and short-term job management system.

## Guest pages
- Landing
- Browse Tasks
- Task Details
- Login / Register
- Forgot Password
- Reset Password
- How It Works
- About

## User pages implemented
- U-01 Dashboard
- U-02 Find Tasks (list + approximate map)
- U-03 Task Details (authenticated full view)
- U-04 Post a Task (multi-step form)
- U-05 Edit Task (pre-filled multi-step form)

## U-04 prototype behavior
- Four-step Post a Task flow: Basic Info → Details → Compensation → Schedule.
- Required-field validation with step navigation and inline errors.
- Live task preview updates as the form is completed.
- Optional task photo upload with local preview and 5 MB demo limit.
- Suggested compensation shortcuts and payment method selection.
- Approximate barangay/task-area selection with privacy guidance.
- Save Draft stores the demo form locally in the browser.
- Publish confirmation modal and demo success redirect to Dashboard.

## U-03 prototype behavior
- Task details can be opened from Find Tasks or Dashboard.
- Six demo task records are supported through `?task=` query parameters.
- Task summary, description, requirements, schedule, compensation, poster information, and approximate location are shown.
- Save/unsave task interaction.
- Request-to-Perform modal with a short message and confirmation toast.
- Report Task modal with demo moderation reasons.
- Responsive authenticated shell and mobile bottom navigation.
- Public profile link remains a demo action until U-13 is implemented.

## Current prototype behavior
- Post Task navigation is now connected from the authenticated dashboard, Find Tasks, and Task Details pages.
- Frontend-only demo interactions
- Responsive desktop/mobile layouts
- Task search, category/barangay/price/deadline filters
- Sort by price/deadline
- Save/unsave task UI
- Approximate map pins with list synchronization
- Mobile list/map view switch
- No live GPS, precise location sharing, or real payment processing

The user-side map intentionally uses approximate task locations, consistent with the project scope.

## U-05 prototype behavior
- Reuses the four-step task editor: Basic Info → Details → Compensation → Schedule.
- Loads pre-filled demo task data, with `?task=` support for sample tasks such as `fix-leaking-faucet`, `house-cleaning`, and `math-tutor`.
- Edit-specific task status control: Active / Paused.
- Save Changes confirmation modal and local browser persistence for unsaved edits.
- Pause Requests control toggles whether new requests are accepted.
- Archive Task control confirms archiving and redirects to the Dashboard in the demo flow.
- Responsive editing layout with live preview and task-management controls.

## U-06 prototype behavior
- My Tasks brings the task lifecycle into one authenticated page.
- Tabs cover Posted, Requested, Accepted, In Progress, and Completed tasks.
- Search, category filtering, sorting, and list/compact views are available.
- Posted task cards link to Edit Task and authenticated Task Details; request counts surface task-management actions.
- Accepted and in-progress examples expose task conversation actions, while completed examples surface payment/review demo actions.
- Responsive task cards and mobile navigation match the authenticated TaskTap shell.

## U-07 Manage Requests
- `manage-requests.html` / `manage-requests.css` / `manage-requests.js`
- Review applicants for a posted task with search, filters, sorting, shortlist, message, profile, decline, and performer-selection interactions.
- Includes a selection confirmation flow and community-safety guidance.
- Connected request actions from My Tasks to the U-07 screen.

### U-08 — My Requests
- `my-requests.html`, `my-requests.css`, `my-requests.js`
- Tracks Pending, Accepted, Declined, and All task requests
- Search, sorting, request withdrawal confirmation, task links, and mobile layout
- Added My Requests shortcut from My Tasks

## U-09 Messages
- `messages.html`, `messages.css`, `messages.js`
- Conversation list with unread states, task context, filters, and search
- Responsive chat pane with task-linked conversations
- Demo message composer, send interactions, and mobile conversation navigation
- Safety guidance for keeping communication task-focused and avoiding sensitive information


## U-10 Notifications
- `notifications.html` / `notifications.css` / `notifications.js`
- Activity center with All, Unread, Tasks, Messages, and Payments filters
- Read/unread state, mark-all-read, period filter, notification preference toggles, responsive layout
- Connected authenticated notification navigation


## U-12 — Edit Profile / Account Settings
- `account-settings.html`, `account-settings.css`, `account-settings.js`
- Profile information, skills, contact/location, password/security, notifications, privacy and account controls.
- Frontend-only save state and localStorage demo persistence.
- Approximate-location privacy guidance and responsive mobile layout.

## U-13 — Public Profile
- `public-profile.html`, `public-profile.css`, `public-profile.js`
- Public-facing member profile with reputation, completed-task history, skills, reviews, trust signals, and safe contact actions.
- Message, share-profile, and report-profile demo modals.
- Responsive layout connected to the authenticated shell and existing profile links.

## U-14 Payment Record
- `payment-record.html`
- `payment-record.css`
- `payment-record.js`
- Completed-task payment summary, receipt-style record, reference number, payment method/status/date, task context, safety guidance, and links to edit/history/review.
- Frontend demo only; no real payment processing.


## U-15 — Record Payment
- `record-payment.html`, `record-payment.css`, `record-payment.js`
- Payment entry/update form with amount, method, status, date, reference, notes, confirmation, and live receipt preview.
- Frontend-only demo persistence and validation; no real money movement.

## U-16 — Payment History
- `payment-history.html`, `payment-history.css`, `payment-history.js`
- Payment history with total/paid/pending/average summary cards.
- Search by task/reference/person, status and payment-method filters, sorting, empty state, payment breakdown, and links to individual payment records.
- Connected from My Tasks, Profile, Payment Record, and authenticated notification/payment navigation.

## U-17 — Rate & Review
- `rate-review.html`
- `rate-review.css`
- `rate-review.js`

Frontend review workflow for completed tasks with 1–5 star rating, review title/body, helpful tags, community-safety confirmation, validation, and publish confirmation. Includes task and reviewer context plus responsive mobile layout. Demo only; no backend submission.

## U-18 — Report
- `report.html`, `report.css`, `report.js`
- Report a task or member with reason selection, detailed context, optional supporting indicators, contact preference, and good-faith/privacy confirmation.
- Private report reference confirmation, moderation-process explanation, safety guidance, and responsive layout.
- Supports `?type=task` and `?type=user` contexts; frontend-only demo, no real moderation ticket is created.

U-21 — My Archived Tasks (`archived-tasks.html`, `archived-tasks.css`, `archived-tasks.js`) — archived history, filters, restore/remove demo actions, payment/review links, responsive layout.
