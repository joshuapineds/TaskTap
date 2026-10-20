using RequestRecord = TaskTap.DemoApplicant;

namespace TaskTap;

public sealed class ManageRequestsPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(104, 121, 144);
    private static readonly Color Page = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(226, 234, 242);
    private readonly Panel page = new();
    private readonly Panel contextCard = new();
    private readonly Panel requestsToolbar = new();
    private readonly Panel decisionPanel = new();
    private Button completeButton = null!;
    private Button cancelButton = null!;
    private readonly FlowLayoutPanel requestList = new();
    private readonly TextBox requestSearch = new();
    private readonly ComboBox requestSort = new();
    private readonly Label requestCount = new();
    private readonly Label selectedContent = new();
    private readonly Button acceptButton = new();
    private Button confirmButton = null!;
    private readonly Button clearSelectionButton = new();
    private readonly Panel modal = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
    private DemoTask? managedTask;
    private List<RequestRecord> requests = [];
    private readonly Dictionary<string, Button> filters = [];
    private readonly Dictionary<RequestRecord, Panel> cards = [];
    private string currentFilter = "all";
    private RequestRecord? selected;
    private bool pageBuilt;
    private bool accepted;

    public override Route RouteName => Route.ManageRequests;

    public ManageRequestsPage()
    {
        managedTask = DemoDataStore.ManagedTask is { } picked && DemoDataStore.PostedTasks.Contains(picked)
            ? picked
            : DemoDataStore.PostedTasks.FirstOrDefault(task => DemoDataStore.RequestsReceived.Any(request => request.TaskId == task.Id))
              ?? DemoDataStore.PostedTasks.FirstOrDefault();
        DemoDataStore.ManagedTask = managedTask;
        requests = DemoDataStore.RequestsReceived.Where(request => managedTask is not null && request.TaskId == managedTask.Id).ToList();
        accepted = requests.Any(request => request.Accepted);
        BuildPage();
    }

    private void BuildPage()
    {
        page.Dock = DockStyle.Fill;
        page.BackColor = Page;
        page.Padding = new Padding(22, 14, 22, 18);
        page.AutoScroll = true;
        Controls.Add(page);
        BuildContent();
        toast.Size = new Size(440, 40);
        toast.BackColor = Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
        Controls.Add(toast);
        toast.BringToFront();
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); toast.Visible = false; };
        Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
        pageBuilt = true;
        ApplyRequests();
        LayoutPage();
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← My Tasks   /   Manage Requests", 8, Muted, true);
        crumb.Location = new Point(0, 0);
        crumb.Cursor = Cursors.Hand;
        crumb.Click += (_, _) => Nav.Navigate(Route.MyTasks);
        page.Controls.Add(crumb);
        page.Controls.Add(MakeLabel("TASK MANAGEMENT", 7, Blue, true).At(0, 26));
        page.Controls.Add(MakeLabel("Manage Requests", 24, Navy, true).At(0, 42));
        page.Controls.Add(MakeLabel("Review people who want to perform your task and choose the best fit.", 9, Muted).At(0, 86));
        var viewTask = MakeButton("View Task", "ManageRequestsViewTaskButton", false, 84);
        viewTask.Location = new Point(0, 46);
        viewTask.Click += (_, _) =>
        {
            var task = managedTask;
            if (task is not null)
                Nav.Navigate(new UserTaskDetailsPage(task.Title, task.Category, task.Price, task.Barangay, task.Days, task.Deadline, task.Icon, task.Description, task.IconBack, task.Id));
            else
                Nav.Navigate(Route.MyTasks);
        };
        page.Controls.Add(viewTask);
        var editTask = MakeButton("Edit Task", "ManageRequestsEditTaskButton", true, 88);
        editTask.Location = new Point(0, 46);
        editTask.Click += (_, _) => { DemoDataStore.EditingTask = managedTask; Nav.Navigate(new EditTaskPage()); };
        page.Controls.Add(editTask);

        BuildTaskSummary();
        BuildTaskControls();
        BuildRequestToolbar();
        BuildDecisionPanel();
        BuildRequestList();
        BuildSafetyStrip();
        BuildDecisionModal();
    }

    private void BuildTaskSummary()
    {
        contextCard.Name = "manageRequestsTaskSummary";
        contextCard.BackColor = Color.White;
        contextCard.BorderStyle = BorderStyle.FixedSingle;
        var icon = new Label { Text = managedTask?.Icon ?? "📌", Size = new Size(66, 66), Location = new Point(12, 12), BackColor = managedTask?.IconBack ?? Color.FromArgb(232, 241, 255), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Emoji", 24F) };
        contextCard.Controls.Add(icon);
        var status = MakeLabel($"ACTIVE  ·  {requests.Count} REQUEST{(requests.Count == 1 ? "" : "S")}", 7, Color.FromArgb(10, 141, 102), true); status.Location = new Point(90, 10); contextCard.Controls.Add(status);
        var title = MakeLabel(managedTask?.Title ?? "No task selected", 13, Navy, true); title.Location = new Point(90, 26); contextCard.Controls.Add(title);
        var description = MakeLabel(managedTask?.Description ?? "Open My Tasks and choose a posted task to review its requests.", 8, Muted); description.Location = new Point(90, 48); description.MaximumSize = new Size(800, 28); contextCard.Controls.Add(description);
        var meta = MakeLabel(managedTask is null ? "" : $"⚒ {managedTask.Category}    ·    ⌖ Brgy. {managedTask.Barangay}    ·    ◷ Due {managedTask.Deadline}", 7, Muted); meta.Location = new Point(90, 80); contextCard.Controls.Add(meta);
        var amount = MakeLabel($"₱{managedTask?.Price ?? 0:N0}", 10, Navy, true); amount.Name = "ManageRequestsTaskAmount"; contextCard.Controls.Add(amount);
        var separator = new Panel { BackColor = Line, Width = 1 }; contextCard.Controls.Add(separator);
        var count = MakeLabel(requests.Count.ToString(), 17, Navy, true); count.Name = "ManageRequestsSummaryCount"; contextCard.Controls.Add(count);
        var received = MakeLabel("requests received", 7, Muted); contextCard.Controls.Add(received);
        var progressBack = new Panel { BackColor = Color.FromArgb(237, 242, 247), Height = 4 }; contextCard.Controls.Add(progressBack);
        var progress = new Panel { BackColor = Blue, Height = 4, Width = 72 }; progressBack.Controls.Add(progress);
        contextCard.Controls.Add(MakeLabel("Choose one performer", 6.5F, Muted));
        page.Controls.Add(contextCard);
    }

    private void BuildTaskControls()
    {
        completeButton = MakeButton("Mark complete", "ManageRequestsCompleteButton", true, 118);
        completeButton.Click += (_, _) => CompletePostedTask();
        page.Controls.Add(completeButton);

        cancelButton = MakeButton("Cancel task", "ManageRequestsCancelButton", false, 108);
        cancelButton.ForeColor = Color.FromArgb(185, 55, 67);
        cancelButton.Click += (_, _) => CancelPostedTask();
        page.Controls.Add(cancelButton);
    }

    private void CompletePostedTask()
    {
        var task = managedTask;
        if (task is null)
        {
            ShowToast("No active posted task to complete.");
            return;
        }

        if (MessageBox.Show(this, $"Mark \"{task.Title}\" complete?", "Complete task", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        DemoDataStore.CompleteTask(task);
        ShowToast("Task marked complete. It now appears in Archived Tasks.");
        Nav.Navigate(Route.MyTasks);
    }

    private void CancelPostedTask()
    {
        var task = managedTask;
        if (task is null)
        {
            ShowToast("No active posted task to cancel.");
            return;
        }

        using var dialog = new ReasonDialog("Cancel task", $"Why is \"{task.Title}\" being cancelled?", ["No longer needed", "Found help elsewhere", "Schedule changed", "Other"]);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        DemoDataStore.CancelTask(task, dialog.SelectedReason ?? "Cancelled.");
        ShowToast("Task cancelled. It now appears in Archived Tasks.");
        Nav.Navigate(Route.MyTasks);
    }

    private void BuildRequestToolbar()    {
        requestsToolbar.Name = "manageRequestsToolbar";
        requestsToolbar.BackColor = Color.White;
        requestsToolbar.BorderStyle = BorderStyle.FixedSingle;
        var countLine = MakeLabel($"{requests.Count} request{(requests.Count == 1 ? "" : "s")}", 8, Navy, true); countLine.Name = "ManageRequestsCountLabel"; countLine.Location = new Point(12, 10); requestsToolbar.Controls.Add(countLine);
        requestsToolbar.Controls.Add(MakeLabel("· newest first", 7, Muted).At(86, 11));
        requestSort.Name = "ManageRequestsSort";
        requestSort.DropDownStyle = ComboBoxStyle.DropDownList;
        requestSort.Items.AddRange(["Recommended", "Highest rating", "Most recent"]);
        requestSort.SelectedIndex = 0;
        requestSort.SelectedIndexChanged += (_, _) => ApplyRequests();
        requestsToolbar.Controls.Add(requestSort);
        requestSearch.Name = "ManageRequestsSearch";
        requestSearch.PlaceholderText = "⌕  Search applicants...";
        requestSearch.TextChanged += (_, _) => ApplyRequests();
        requestsToolbar.Controls.Add(requestSearch);
        foreach (var (key, label, count) in new[] { ("all", "All", requests.Count(request => !request.Declined).ToString()), ("new", "New", requests.Count(request => !request.Declined && !request.Shortlisted).ToString()), ("shortlisted", "Shortlisted", requests.Count(request => !request.Declined && request.Shortlisted).ToString()) })
        {
            var filter = new Button { Name = $"ManageRequestsFilter{key}", Text = $"{label}  {count}", Size = new Size(key == "shortlisted" ? 105 : 68, 30), FlatStyle = FlatStyle.Flat, BackColor = key == "all" ? Color.FromArgb(237, 245, 253) : Color.White, ForeColor = key == "all" ? Blue : Muted, Font = new Font("Segoe UI", 7F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            filter.FlatAppearance.BorderColor = Line;
            filter.Click += (_, _) => SelectFilter(key);
            filters.Add(key, filter);
            requestsToolbar.Controls.Add(filter);
        }
        page.Controls.Add(requestsToolbar);
    }

    private void BuildDecisionPanel()
    {
        decisionPanel.Name = "manageRequestsDecisionPanel";
        decisionPanel.BackColor = Color.White;
        decisionPanel.BorderStyle = BorderStyle.FixedSingle;
        var eyebrow = MakeLabel("SELECTION", 7, Blue, true); eyebrow.Location = new Point(12, 12); decisionPanel.Controls.Add(eyebrow);
        var title = MakeLabel("Choose a performer", 11, Navy, true); title.Location = new Point(12, 29); decisionPanel.Controls.Add(title);
        var count = new Label { Name = "ManageRequestsSelectedCount", Text = "1", Size = new Size(24, 24), Location = new Point(0, 10), BackColor = Color.FromArgb(234, 244, 255), ForeColor = Blue, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) }; decisionPanel.Controls.Add(count);
        var body = MakeLabel("Review profiles and messages before accepting a request. Once accepted, no other applicant can be selected for this task.", 7, Muted); body.Name = "ManageRequestsPanelCopy"; body.Location = new Point(12, 52); body.MaximumSize = new Size(245, 55); decisionPanel.Controls.Add(body);
        var selectedBox = new Panel { Name = "ManageRequestsSelectedBox", BackColor = Color.FromArgb(251, 253, 255), BorderStyle = BorderStyle.FixedSingle };
        var emptyIcon = new Label { Text = "◎", Size = new Size(34, 34), BackColor = Color.FromArgb(237, 244, 251), ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 13F, FontStyle.Bold) }; emptyIcon.Name = "ManageRequestsSelectionIcon"; selectedBox.Controls.Add(emptyIcon);
        var emptyTitle = MakeLabel("No one selected yet", 8, Navy, true); emptyTitle.Name = "ManageRequestsSelectionTitle"; selectedBox.Controls.Add(emptyTitle);
        var emptyText = MakeLabel("Select an applicant card to review them here.", 7, Muted); emptyText.Name = "ManageRequestsSelectionText"; emptyText.MaximumSize = new Size(220, 35); selectedBox.Controls.Add(emptyText);
        selectedContent.Text = "";
        selectedBox.Controls.Add(selectedContent);
        clearSelectionButton.Text = "Clear selection";
        clearSelectionButton.Name = "ManageRequestsClearSelection";
        clearSelectionButton.FlatStyle = FlatStyle.Flat;
        clearSelectionButton.ForeColor = Blue;
        clearSelectionButton.BackColor = Color.Transparent;
        clearSelectionButton.FlatAppearance.BorderSize = 0;
        clearSelectionButton.Visible = false;
        clearSelectionButton.Click += (_, _) => SetSelected(null);
        selectedBox.Controls.Add(clearSelectionButton);
        decisionPanel.Controls.Add(selectedBox);
        var note = new Panel { Name = "ManageRequestsSafetyNote", BackColor = Color.FromArgb(247, 251, 255), BorderStyle = BorderStyle.FixedSingle };
        var info = MakeLabel("i", 8, Blue, true); info.Location = new Point(8, 8); note.Controls.Add(info);
        var noteTitle = MakeLabel("Keep communication on TaskTap", 7, Navy, true); noteTitle.Location = new Point(27, 7); note.Controls.Add(noteTitle);
        var noteText = MakeLabel("Confirm scope and timing. Share exact addresses only when necessary.", 6.5F, Muted); noteText.Location = new Point(27, 24); noteText.MaximumSize = new Size(205, 28); note.Controls.Add(noteText);
        decisionPanel.Controls.Add(note);
        acceptButton.Name = "ManageRequestsAcceptButton";
        acceptButton.Text = "Accept selected request";
        acceptButton.Size = new Size(260, 36);
        acceptButton.Enabled = false;
        acceptButton.FlatStyle = FlatStyle.Flat;
        acceptButton.BackColor = Blue;
        acceptButton.ForeColor = Color.White;
        acceptButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        acceptButton.FlatAppearance.BorderSize = 0;
        acceptButton.Click += (_, _) => ShowDecisionModal();
        decisionPanel.Controls.Add(acceptButton);
        var compare = MakeButton("Compare shortlisted", "ManageRequestsCompareButton", false, 260);
        compare.Click += (_, _) => { var countShortlisted = requests.Count(request => request.Shortlisted && !request.Declined); ShowToast(countShortlisted == 0 ? "Shortlist applicants to compare them." : $"Comparing {countShortlisted} shortlisted applicant{(countShortlisted == 1 ? "" : "s")}…"); };
        decisionPanel.Controls.Add(compare);
        page.Controls.Add(decisionPanel);
    }

    private void BuildRequestList()
    {
        requestCount.Name = "ManageRequestsVisibleCount";
        requestCount.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        requestCount.ForeColor = Navy;
        page.Controls.Add(requestCount);
        requestList.Name = "ManageRequestsList";
        requestList.FlowDirection = FlowDirection.TopDown;
        requestList.WrapContents = false;
        requestList.AutoScroll = false;
        requestList.BackColor = Page;
        foreach (var request in requests)
        {
            var card = CreateRequestCard(request);
            cards.Add(request, card);
            requestList.Controls.Add(card);
        }
        page.Controls.Add(requestList);
        var empty = new Panel { Name = "ManageRequestsEmptyState", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Visible = false };
        empty.Controls.Add(MakeLabel("No matching requests", 13, Navy, true).At(16, 18));
        empty.Controls.Add(MakeLabel("Try another name, filter, or sort option.", 8, Muted).At(16, 45));
        var clear = MakeButton("Clear filters", "ManageRequestsClearFilters", false, 104);
        clear.Location = new Point(16, 72); clear.Click += (_, _) => ClearFilters(); empty.Controls.Add(clear);
        page.Controls.Add(empty);
    }

    private Panel CreateRequestCard(RequestRecord request)
    {
        var card = new Panel { Name = "RequestCard", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = request, Cursor = Cursors.Hand };
        card.Click += (_, _) => SetSelected(request);
        var avatar = new Label { Name = "RequestApplicantAvatar", Text = request.Initials, Size = new Size(48, 48), Location = new Point(12, 12), BackColor = request.AvatarColor, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        avatar.Click += (_, _) => SetSelected(request); card.Controls.Add(avatar);
        var state = MakeLabel(request.Shortlisted ? "SHORTLISTED" : "NEW REQUEST", 6.5F, request.Shortlisted ? Color.FromArgb(180, 106, 27) : Blue, true); state.Name = "RequestState"; state.Location = new Point(74, 9); state.Click += (_, _) => SetSelected(request); card.Controls.Add(state);
        var name = MakeLabel(request.Name, 11, Navy, true); name.Name = "RequestApplicantName"; name.Location = new Point(74, 23); name.Click += (_, _) => SetSelected(request); card.Controls.Add(name);
        var rating = MakeLabel($"{request.Rating:F1}  ★   {request.Reviews} reviews · {request.Completed} completed", 7, Muted); rating.Location = new Point(74, 43); rating.Click += (_, _) => SetSelected(request); card.Controls.Add(rating);
        var when = MakeLabel(request.RequestedAt, 6.5F, Muted); when.Name = "RequestReceivedAt"; when.Anchor = AnchorStyles.Top | AnchorStyles.Right; card.Controls.Add(when);
        var message = MakeLabel($"“{request.Message}”", 7.5F, Muted); message.Name = "RequestMessage"; message.Location = new Point(74, 63); message.MaximumSize = new Size(700, 34); message.Click += (_, _) => SetSelected(request); card.Controls.Add(message);
        var skillY = 101;
        var skillX = 74;
        foreach (var skill in request.Skills)
        {
            var pill = new Label { Text = skill, AutoSize = true, Padding = new Padding(7, 4, 7, 4), BackColor = Color.FromArgb(243, 246, 249), ForeColor = Muted, Font = new Font("Segoe UI", 6.5F), Location = new Point(skillX, skillY) };
            card.Controls.Add(pill);
            skillX += pill.PreferredWidth + 5;
        }
        var meta = MakeLabel($"✓ Verified phone   ·   ⌖ Brgy. {request.Barangay   }   ·   Member since {request.MemberSince}", 6.5F, Muted); meta.Name = "RequestMeta"; meta.Location = new Point(74, 129); card.Controls.Add(meta);
        var actions = new FlowLayoutPanel { Name = "RequestActions", Location = new Point(72, 152), Size = new Size(650, 32), WrapContents = false, BackColor = Color.Transparent };
        var shortlist = MakeButton(request.Shortlisted ? "Shortlisted" : "Shortlist", "RequestShortlistButton", true, request.Shortlisted ? 90 : 78);
        shortlist.Height = 27;
        shortlist.Click += (_, _) => ToggleShortlist(request);
        actions.Controls.Add(shortlist);
        AddRequestAction(actions, "Message", () => Nav.Navigate(Route.Messages));
        AddRequestAction(actions, "View profile", () => Nav.Navigate(ProfileLinks.For(request.Name)));
        AddRequestAction(actions, "Decline", () => Decline(request));
        card.Controls.Add(actions);
        return card;
    }

    private void BuildSafetyStrip()
    {
        var strip = new Panel { Name = "ManageRequestsSafetyStrip", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var icon = new Label { Text = "✓", Size = new Size(24, 24), Location = new Point(12, 10), BackColor = Color.FromArgb(232, 248, 241), ForeColor = Color.FromArgb(10, 141, 102), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }; strip.Controls.Add(icon);
        strip.Controls.Add(MakeLabel("Community safety", 7, Navy, true).At(44, 8));
        strip.Controls.Add(MakeLabel("Review ratings, task history, and each applicant’s message before choosing. Your decision is saved and the applicants are notified.", 6.5F, Muted).At(44, 23));
        var safety = MakeTextButton("Safety guidelines →", "ManageRequestsSafetyButton"); safety.Click += (_, _) => ShowToast("Review ratings and keep task conversations on TaskTap."); strip.Controls.Add(safety);
        page.Controls.Add(strip);
    }

    private void BuildDecisionModal()
    {
        modal.Name = "ManageRequestsDecisionModal";
        modal.Visible = false;
        modal.BackColor = Color.FromArgb(245, 248, 252);
        modal.BorderStyle = BorderStyle.FixedSingle;
        modal.Size = new Size(450, 240);
        modal.Anchor = AnchorStyles.None;
        var icon = new Label { Text = "✓", Size = new Size(42, 42), BackColor = Color.FromArgb(232, 248, 241), ForeColor = Color.FromArgb(10, 141, 102), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 17F, FontStyle.Bold), Location = new Point(204, 18) }; modal.Controls.Add(icon);
        var title = MakeLabel("Accept this request?", 14, Navy, true); title.Location = new Point(145, 68); modal.Controls.Add(title);
        var summary = MakeLabel("Select an applicant to review this decision.", 8, Muted); summary.Name = "ManageRequestsDecisionSummary"; summary.Location = new Point(52, 103); summary.MaximumSize = new Size(340, 45); modal.Controls.Add(summary);
        var cancel = MakeButton("Cancel", "ManageRequestsCancelDecision", false, 110); cancel.Location = new Point(103, 174); cancel.Click += (_, _) => modal.Visible = false; modal.Controls.Add(cancel);
        confirmButton = MakeButton("Confirm acceptance", "ManageRequestsConfirmDecision", true, 150); confirmButton.Location = new Point(221, 174); confirmButton.Click += (_, _) => ConfirmAcceptance(); modal.Controls.Add(confirmButton);
        page.Controls.Add(modal);
        modal.BringToFront();
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0)
            return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var right = width - page.Padding.Right;
        var left = page.Padding.Left;
        var actionX = right;
        var edit = page.Controls.Find("ManageRequestsEditTaskButton", false).FirstOrDefault();
        var view = page.Controls.Find("ManageRequestsViewTaskButton", false).FirstOrDefault();
        if (edit is not null) { edit.Location = new Point(actionX - edit.Width, 46); actionX -= edit.Width + 8; }
        if (view is not null) { view.Location = new Point(actionX - view.Width, 46); actionX -= view.Width + 8; }
        if (completeButton is not null) { completeButton.Location = new Point(actionX - completeButton.Width, 46); actionX -= completeButton.Width + 8; }
        if (cancelButton is not null) { cancelButton.Location = new Point(actionX - cancelButton.Width, 46); }
        contextCard.Bounds = new Rectangle(left, 105, width - left - page.Padding.Right, 102);
        var summaryAmount = contextCard.Controls.Find("ManageRequestsTaskAmount", false).FirstOrDefault();
        var separator = contextCard.Controls.OfType<Panel>().FirstOrDefault(panel => panel.Width == 1);
        var progressLabels = contextCard.Controls.OfType<Label>().Where(label => label.Name != "ManageRequestsTaskAmount").ToArray();
        if (summaryAmount is not null) summaryAmount.Location = new Point(contextCard.Width - 250, 76);
        if (separator is not null) separator.Bounds = new Rectangle(contextCard.Width - 224, 14, 1, 74);
        var summaryCount = contextCard.Controls.Find("ManageRequestsSummaryCount", false).FirstOrDefault();
        if (summaryCount is not null) summaryCount.Location = new Point(contextCard.Width - 208, 12);
        var received = progressLabels.FirstOrDefault(label => label.Text == "requests received");
        if (received is not null) received.Location = new Point(contextCard.Width - 208, 38);
        var progress = contextCard.Controls.OfType<Panel>().Where(panel => panel.Height == 4).ToList();
        if (progress.Count > 0) progress[0].Bounds = new Rectangle(contextCard.Width - 208, 57, 110, 4);
        if (progress.Count > 1) progress[1].Bounds = new Rectangle(0, 0, 72, 4);
        var choose = progressLabels.FirstOrDefault(label => label.Text == "Choose one performer");
        if (choose is not null) choose.Location = new Point(contextCard.Width - 208, 67);

        const int gap = 14;
        var decisionWidth = 276;
        var requestsWidth = width - left - page.Padding.Right - decisionWidth - gap;
        requestsToolbar.Bounds = new Rectangle(left, 219, requestsWidth, 96);
        var sort = requestSort;
        sort.Location = new Point(requestsToolbar.Width - sort.Width - 12, 8);
        var count = requestsToolbar.Controls.Find("ManageRequestsCountLabel", false).FirstOrDefault();
        if (count is not null) count.Location = new Point(12, 10);
        var sortRowBottom = 40;
        requestSearch.Location = new Point(12, sortRowBottom + 7);
        requestSearch.Size = new Size(Math.Max(180, requestsToolbar.Width - 236), 30);
        var filterX = requestSearch.Right + 8;
        foreach (var key in new[] { "all", "new", "shortlisted" })
        {
            if (filters.TryGetValue(key, out var filter))
            {
                filter.Location = new Point(filterX, sortRowBottom + 7);
                filterX += filter.Width + 5;
            }
        }
        requestCount.Location = new Point(left + 2, 326);
        var requestHeight = Math.Max(210, requests.Count(request => !request.Declined &&
            (currentFilter == "all" || currentFilter == "new" && !request.Shortlisted || currentFilter == "shortlisted" && request.Shortlisted) &&
            (string.IsNullOrEmpty(requestSearch.Text.Trim()) || $"{request.Name} {request.Message} {string.Join(" ", request.Skills)} {request.Barangay} {request.Rating}".Contains(requestSearch.Text.Trim(), StringComparison.OrdinalIgnoreCase))) * 195);
        requestList.Bounds = new Rectangle(left, 348, requestsWidth, requestHeight);
        foreach (Control card in requestList.Controls)
            LayoutRequestCard(card, requestsWidth);
        var empty = page.Controls.Find("ManageRequestsEmptyState", false).FirstOrDefault();
        if (empty is not null) empty.Bounds = new Rectangle(left, 348, requestsWidth, 160);

        decisionPanel.Bounds = new Rectangle(left + requestsWidth + gap, 219, decisionWidth, 456);
        LayoutDecisionPanel(decisionWidth);
        var safety = page.Controls.Find("ManageRequestsSafetyStrip", false).FirstOrDefault();
        if (safety is not null)
        {
            var safetyTop = 348 + requestHeight + 12;
            safety.Bounds = new Rectangle(left, safetyTop, width - left - page.Padding.Right, 48);
            var safetyButton = safety.Controls.Find("ManageRequestsSafetyButton", false).FirstOrDefault();
            if (safetyButton is not null) safetyButton.Location = new Point(safety.Width - safetyButton.Width - 10, 8);
            page.AutoScrollMinSize = new Size(0, safetyTop + 62);
        }
        modal.Location = new Point(Math.Max(0, (page.ClientSize.Width - modal.Width) / 2), Math.Max(0, (page.ClientSize.Height - modal.Height) / 2));
    }

    private void LayoutDecisionPanel(int width)
    {
        var selection = decisionPanel.Controls.Find("ManageRequestsSelectedBox", false).FirstOrDefault();
        if (selection is not null)
        {
            selection.Bounds = new Rectangle(12, 128, width - 24, 124);
            var icon = selection.Controls.Find("ManageRequestsSelectionIcon", false).FirstOrDefault();
            var title = selection.Controls.Find("ManageRequestsSelectionTitle", false).FirstOrDefault();
            var text = selection.Controls.Find("ManageRequestsSelectionText", false).FirstOrDefault();
            if (icon is not null) icon.Location = new Point((selection.Width - icon.Width) / 2, 14);
            if (title is not null) title.Location = new Point((selection.Width - title.Width) / 2, 54);
            if (text is not null) text.Location = new Point(12, 74);
            selectedContent.Location = new Point(12, 11);
            clearSelectionButton.Location = new Point(12, 91);
        }
        var note = decisionPanel.Controls.Find("ManageRequestsSafetyNote", false).FirstOrDefault();
        if (note is not null) note.Bounds = new Rectangle(12, 264, width - 24, 56);
        acceptButton.Location = new Point(12, 330);
        acceptButton.Width = width - 24;
        var compare = decisionPanel.Controls.Find("ManageRequestsCompareButton", false).FirstOrDefault();
        if (compare is not null) { compare.Location = new Point(12, 374); compare.Width = width - 24; }
    }

    private void LayoutRequestCard(Control control, int width)
    {
        if (control is not Panel card || card.Tag is not RequestRecord request)
            return;
        card.Width = width - 4;
        card.Height = 190;
        var time = card.Controls.Find("RequestReceivedAt", false).FirstOrDefault();
        if (time is not null) time.Location = new Point(card.Width - time.Width - 12, 12);
        var message = card.Controls.Find("RequestMessage", false).FirstOrDefault();
        if (message is not null) message.MaximumSize = new Size(Math.Max(250, card.Width - 115), 34);
        var meta = card.Controls.Find("RequestMeta", false).FirstOrDefault();
        if (meta is not null) meta.Text = $"✓ Verified phone   ·   ⌖ Brgy. {request.Barangay}   ·   Member since {request.MemberSince}";
        var actions = card.Controls.Find("RequestActions", false).FirstOrDefault();
        if (actions is not null) actions.Location = new Point(72, 153);
        card.BackColor = ReferenceEquals(selected, request) ? Color.FromArgb(240, 248, 255) : Color.White;
    }

    private void ApplyRequests()
    {
        if (requestList.IsDisposed)
            return;
        var query = requestSearch.Text.Trim();
        IEnumerable<RequestRecord> filtered = requests.Where(request => !request.Declined)
            .Where(request => currentFilter == "all" || currentFilter == "new" && !request.Shortlisted || currentFilter == "shortlisted" && request.Shortlisted)
            .Where(request => string.IsNullOrEmpty(query) || $"{request.Name} {request.Message} {string.Join(" ", request.Skills)} {request.Barangay} {request.Rating}".Contains(query, StringComparison.OrdinalIgnoreCase));
        filtered = requestSort.SelectedItem?.ToString() switch
        {
            "Highest rating" => filtered.OrderByDescending(request => request.Rating),
            "Most recent" => filtered.OrderByDescending(request => request.Date),
            _ => filtered.OrderByDescending(request => request.Shortlisted).ThenByDescending(request => request.Date)
        };
        var visible = filtered.ToHashSet();
        requestList.SuspendLayout();
        foreach (var (request, card) in cards)
        {
            card.Visible = visible.Contains(request);
            card.Parent = null;
        }
        foreach (var request in filtered)
            requestList.Controls.Add(cards[request]);
        requestList.ResumeLayout();
        requestCount.Text = $"{visible.Count} request{(visible.Count == 1 ? "" : "s")}";
        var countLabel = requestsToolbar.Controls.Find("ManageRequestsCountLabel", false).FirstOrDefault();
        if (countLabel is not null) countLabel.Text = requestCount.Text;
        var empty = page.Controls.Find("ManageRequestsEmptyState", false).FirstOrDefault();
        if (empty is not null) empty.Visible = visible.Count == 0;
        if (pageBuilt) LayoutPage();
    }

    private void SelectFilter(string filter)
    {
        currentFilter = filter;
        foreach (var (key, button) in filters)
        {
            var active = key == filter;
            button.BackColor = active ? Color.FromArgb(237, 245, 253) : Color.White;
            button.ForeColor = active ? Blue : Muted;
        }
        ApplyRequests();
    }

    private void ToggleShortlist(RequestRecord request)
    {
        DemoDataStore.ToggleShortlist(request);
        if (ReferenceEquals(selected, request)) UpdateSelectionPanel();
        ApplyRequests();
        ShowToast(request.Shortlisted ? $"{request.Name} shortlisted." : $"{request.Name} removed from shortlist.");
    }

    private void SetSelected(RequestRecord? request)
    {
        if (accepted)
        {
            ShowToast("A performer has already been selected.");
            return;
        }
        selected = request;
        foreach (var (record, card) in cards)
            LayoutRequestCard(card, requestList.ClientSize.Width > 0 ? requestList.ClientSize.Width : 500);
        UpdateSelectionPanel();
    }

    private void UpdateSelectionPanel()
    {
        var selectedBox = decisionPanel.Controls.Find("ManageRequestsSelectedBox", false).FirstOrDefault();
        if (selectedBox is null) return;
        foreach (var name in new[] { "ManageRequestsSelectionIcon", "ManageRequestsSelectionTitle", "ManageRequestsSelectionText" })
        {
            var control = selectedBox.Controls.Find(name, false).FirstOrDefault();
            if (control is not null) control.Visible = selected is null;
        }
        selectedContent.Visible = selected is not null;
        clearSelectionButton.Visible = selected is not null;
        selectedContent.Text = selected is null ? string.Empty : $"{selected.Initials}   {selected.Name}\r\n{selected.Rating:F1} ★ · ready to start\r\n\r\nYou can still message them before confirming.";
        selectedContent.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        selectedContent.ForeColor = Navy;
        selectedContent.MaximumSize = new Size(selectedBox.Width - 24, 76);
        acceptButton.Enabled = selected is not null && !accepted;
        acceptButton.Text = accepted ? "Performer selected" : "Accept selected request";
    }

    private void ShowDecisionModal()
    {
        if (selected is null) return;
        var summary = modal.Controls.Find("ManageRequestsDecisionSummary", false).FirstOrDefault();
        if (summary is not null) summary.Text = $"{selected.Name}\r\n{selected.Rating:F1} ★  ·  {selected.Reviews} reviews  ·  {selected.Completed} completed\r\n\r\nThe other applicants will be told the task is taken.";
        modal.Visible = true;
        modal.BringToFront();
    }

    private void ConfirmAcceptance()
    {
        if (selected is null) return;
        accepted = true;
        DemoDataStore.AcceptApplicant(selected);
        modal.Visible = false;
        var acceptedName = selected.Name;
        UpdateSelectionPanel();
        ApplyRequests();
        ShowToast($"{acceptedName} selected. They have been notified.");
    }

    private void Decline(RequestRecord request)
    {
        if (MessageBox.Show($"Decline {request.Name}'s request? They will be notified.", "Decline request", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        DemoDataStore.DeclineApplicant(request);
        if (ReferenceEquals(selected, request)) SetSelected(null);
        ApplyRequests();
        ShowToast("Request declined.");
    }

    private void ClearFilters()
    {
        requestSearch.Clear();
        SelectFilter("all");
        requestSort.SelectedIndex = 0;
    }

    private void AddRequestAction(FlowLayoutPanel panel, string text, Action action)
    {
        var button = MakeTextButton(text, "RequestActionButton");
        button.Height = 28;
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private TControl? Find<TControl>(string name) where TControl : Control => page.Controls.Find(name, false).FirstOrDefault() as TControl;

    private static Button MakeButton(string text, string name, bool primary, int width)
    {
        var button = new Button { Name = name, Text = text, Size = new Size(width, 34), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderColor = primary ? Blue : Line;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        return button;
    }

    private static Button MakeTextButton(string text, string name)
    {
        var button = new Button { Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new() { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}

internal static class ManageRequestsControlExtensions
{
    public static T At<T>(this T control, int x, int y) where T : Control
    {
        control.Location = new Point(x, y);
        return control;
    }
}
