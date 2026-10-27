# What changed in this version

**Requests use a CV instead of a message.** Messaging was removed. To apply for a task you attach a PDF
(CV, resume, certificates or past work). The PDF is stored in MySQL (`task_requests.attachment`), limited to 2 MB and
checked to really be a PDF. Only the applicant and the task's poster can open it ("View CV" in Manage Requests).
After a request is accepted, each side can see the other's phone/email ("Contact") to arrange the task.
If MySQL rejects larger files, raise `max_allowed_packet` in `my.ini`.

**Post Task / Edit Task.** Barangay is filled from your profile (read-only). Payment method is Cash only (TaskTap
records the method; it does not process payments). "Fixed total" budget type is gone.
Preferred start time is optional (tick the box), saved with the task and shown on the task details page.

**Payments.** Completing a task creates a Pending payment record for the selected performer using the task's
amount and method. Payment History shows only your own records, with a breakdown by method.

**Privacy.** Each member only loads their own archive, requests and payments. A member sign-in always ends any admin session.

**Admin.** Tasks: *Edit task* (the poster is told what changed and why) and *Delete* (a reason is required and the poster is told).
Accounts: restrict / restore access only (no account deletion). Verification: *Approve* is now a direct database update.

**Password fields** (login, register, change password, admin login) have a show/hide eye button.

**Maps.** All task maps are OpenStreetMap + Leaflet (see maps-setup.md).
