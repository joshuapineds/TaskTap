using TaskTap.Data;

namespace TaskTap;

public sealed class MessagesPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(104, 121, 144);
    private static readonly Color PageColor = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel page = new();
    private readonly Panel workspace = new();
    private readonly Panel conversationPane = new();
    private readonly Panel chatPane = new();
    private readonly Panel conversationList = new();
    private readonly Panel transcriptHost = new();
    private readonly FlowLayoutPanel transcript = new();
    private readonly TextBox conversationSearch = new();
    private readonly TextBox messageInput = new();
    private readonly ComboBox composeTask = new();
    private readonly Label conversationEmpty = new();
    private readonly Label conversationCount = new();
    private readonly Label chatAvatar = new();
    private readonly Label chatName = new();
    private readonly Label chatPresence = new();
    private readonly Label bannerTask = new();
    private readonly Label bannerDetails = new();
    private readonly Label bannerStatus = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2500 };
    private readonly Dictionary<string, Conversation> conversations = [];
    private readonly Dictionary<string, Button> conversationButtons = [];
    private readonly Dictionary<string, Button> filterButtons = [];
    private readonly Panel composeModal = new();
    private readonly Panel composeBackdrop = new();
    private readonly Panel placeholder = new();
    private string currentId = "";
    private string activeFilter = "all";
    private bool pageBuilt;
    private ContextMenuStrip? chatMenu;

    private sealed record ChatMessage(string Text, bool IsMine, string? Day = null, string? Time = null);
    private sealed class Conversation(string id, string name, string initials, Color color, string presence, string task, string taskId, string banner, string status, string latest, string search, bool unread, int unreadCount, List<ChatMessage> messages)
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public string Initials { get; } = initials;
        public Color Color { get; } = color;
        public string Presence { get; } = presence;
        public string Task { get; } = task;
        public string TaskId { get; } = taskId;
        public string Banner { get; } = banner;
        public string Status { get; } = status;
        public string Latest { get; set; } = latest;
        public string Search { get; } = search;
        public bool Unread { get; set; } = unread;
        public int UnreadCount { get; set; } = unreadCount;
        public List<ChatMessage> Messages { get; } = messages;
        public string TimeLabel { get; set; } = "";
        public int OtherId { get; set; }
    }

    public override Route RouteName => Route.Messages;

    public MessagesPage()
    {
        LoadConversations();
        BuildPage();
    }

    private static string ListTime(DateTime when) =>
        when.Date == DateTime.Today ? when.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture) : Backend.DayLabel(when);

    private void LoadConversations()
    {
        if (!AppSession.IsSignedIn)
            return;

        List<Backend.ConversationRow> rows = [];
        Db.Try(() => rows = Backend.LoadConversations(AppSession.CurrentUser.Id));
        foreach (var row in rows)
        {
            var messages = new List<ChatMessage>();
            string? lastDay = null;
            foreach (var message in row.Messages)
            {
                var day = Backend.DayLabel(message.SentAt);
                if (day != lastDay)
                {
                    messages.Add(new ChatMessage(day, false, day));
                    lastDay = day;
                }
                messages.Add(new ChatMessage(message.Body, message.SenderId == AppSession.CurrentUser.Id, Time: message.SentAt.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture)));
            }

            var latest = row.Messages.Count == 0 ? "No messages yet — say hello." : row.Messages[^1].Body;
            var id = row.Id.ToString();
            conversations.Add(id, new Conversation(
                id, row.OtherName, row.OtherInitials, Backend.AvatarFor(row.OtherId), row.Presence,
                row.TaskTitle, row.TaskId.ToString(), row.Banner, row.Status, latest,
                $"{row.OtherName} {row.TaskTitle}".ToLowerInvariant(), row.UnreadCount > 0, row.UnreadCount, messages)
            {
                TimeLabel = row.Messages.Count == 0 ? "" : ListTime(row.Messages[^1].SentAt),
                OtherId = row.OtherId
            });
        }

        currentId = conversations.Keys.FirstOrDefault() ?? "";
    }

    private void BuildPage()
    {
        page.Dock = DockStyle.Fill;
        page.AutoScroll = true;
        page.BackColor = PageColor;
        page.Resize += (_, _) => LayoutPage();
        Controls.Add(page);
        BuildContent();
        BuildComposeDialog();

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
        ApplyConversationFilters();
        if (currentId.Length > 0)
            SelectConversation(currentId);
        else
            ShowEmptyConversationState();
        LayoutPage();
    }

    private void ShowEmptyConversationState()
    {
        placeholder.Visible = true;
        chatPane.Visible = false;
        bannerTask.Text = "";
        bannerDetails.Text = "";
        bannerStatus.Text = "";
    }

    private void BuildContent()
    {
        var crumb = MakeLabel("← Dashboard   /   Messages", 8, Muted, true); crumb.Name = "messagesCrumb"; crumb.Cursor = Cursors.Hand; crumb.Click += (_, _) => Nav.Back(); page.Controls.Add(crumb);
        AddPageLabel("messagesEyebrow", "COMMUNICATION", 8, Blue, true);
        AddPageLabel("messagesHeading", "Messages", 25, Navy, true);
        AddPageLabel("messagesSubtitle", "Keep task conversations clear, organized, and connected to the work.", 9, Muted);
        var newMessage = MakeButton("＋ New message", "MessagesNewMessageButton", false, 132); newMessage.Click += (_, _) => OpenCompose(); page.Controls.Add(newMessage);

        workspace.Name = "messagesWorkspace"; workspace.BackColor = Color.White; workspace.BorderStyle = BorderStyle.FixedSingle; page.Controls.Add(workspace);
        conversationPane.Name = "messagesConversationPane"; conversationPane.BackColor = Color.White; conversationPane.BorderStyle = BorderStyle.FixedSingle; workspace.Controls.Add(conversationPane);
        var head = MakeLabel("Conversations", 11, Navy, true); head.Location = new Point(14, 13); conversationPane.Controls.Add(head);
        conversationCount.Name = "messagesConversationCount"; conversationCount.Text = $"{conversations.Count} chat{(conversations.Count == 1 ? "" : "s")}"; conversationCount.Font = new Font("Segoe UI", 7.5F); conversationCount.ForeColor = Muted; conversationCount.Location = new Point(14, 33); conversationPane.Controls.Add(conversationCount);
        var compose = MakeButton("＋", "MessagesComposeButton", false, 36); compose.Location = new Point(0, 10); compose.Anchor = AnchorStyles.Top | AnchorStyles.Right; compose.Click += (_, _) => OpenCompose(); conversationPane.Controls.Add(compose);
        conversationSearch.Name = "messagesConversationSearch"; conversationSearch.PlaceholderText = "⌕  Search conversations..."; conversationSearch.TextChanged += (_, _) => ApplyConversationFilters(); conversationPane.Controls.Add(conversationSearch);
        foreach (var (key, label) in new[] { ("all", "All"), ("unread", "Unread"), ("tasks", "Tasks") })
        {
            var filter = MakeButton(label, $"MessagesFilter{key}", key == "all", 72); filter.Font = new Font("Segoe UI", 7F, FontStyle.Bold); filter.Click += (_, _) => SelectFilter(key); filterButtons.Add(key, filter); conversationPane.Controls.Add(filter);
        }
        conversationList.Name = "messagesConversationList"; conversationList.AutoScroll = true; conversationList.BackColor = Color.White; conversationPane.Controls.Add(conversationList);
        conversationEmpty.Text = "No conversations found\r\nTry another search or filter."; conversationEmpty.Name = "messagesConversationEmpty"; conversationEmpty.Size = new Size(250, 70); conversationEmpty.TextAlign = ContentAlignment.MiddleCenter; conversationEmpty.ForeColor = Muted; conversationEmpty.Visible = false; conversationPane.Controls.Add(conversationEmpty);
        BuildConversationButtons();
        BuildMeStrip();

        chatPane.Name = "messagesChatPane"; chatPane.BackColor = Color.White; workspace.Controls.Add(chatPane);
        BuildChatPane();
        placeholder.Name = "messagesPlaceholder"; placeholder.BackColor = Color.FromArgb(251, 253, 255); placeholder.Controls.Add(MakeLabel("▱", 27, Muted, true).At(30, 25)); placeholder.Controls.Add(MakeLabel("Select a conversation", 14, Navy, true).At(0, 0)); placeholder.Controls.Add(MakeLabel("Choose a chat to view messages and coordinate your next task.", 8, Muted).At(0, 0)); workspace.Controls.Add(placeholder); placeholder.Visible = false;
        var safety = new Panel { Name = "messagesSafetyNote", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        safety.Controls.Add(MakeLabel("i", 10, Blue, true).At(12, 12)); safety.Controls.Add(MakeLabel("Message safely on TaskTap", 8, Navy, true).At(38, 8)); safety.Controls.Add(MakeLabel("Keep task communication here. Do not share sensitive information or payment details in chat. Messages are only stored for this demo session.", 7.5F, Muted).At(38, 26));
        var safetyTip = MakeTextButton("Safety tips →", "MessagesSafetyTipsButton"); safetyTip.Click += (_, _) => Nav.Navigate(Route.CommunityGuidelines); safety.Controls.Add(safetyTip); page.Controls.Add(safety);
    }

    private void BuildChatPane()
    {
        var header = new Panel { Name = "messagesChatHeader", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle }; chatPane.Controls.Add(header);
        var back = MakeButton("‹", "MessagesBackConversationsButton", false, 34); back.Visible = false; back.Click += (_, _) => { conversationPane.Visible = true; }; header.Controls.Add(back);
        chatAvatar.Name = "messagesChatAvatar"; chatAvatar.Size = new Size(38, 38); chatAvatar.Location = new Point(16, 10); chatAvatar.ForeColor = Color.White; chatAvatar.TextAlign = ContentAlignment.MiddleCenter; chatAvatar.Font = new Font("Segoe UI", 9F, FontStyle.Bold); header.Controls.Add(chatAvatar);
        chatName.Name = "messagesChatName"; chatName.Font = new Font("Segoe UI", 10F, FontStyle.Bold); chatName.ForeColor = Navy; chatName.Location = new Point(64, 9); header.Controls.Add(chatName);
        chatPresence.Name = "messagesChatPresence"; chatPresence.Font = new Font("Segoe UI", 7.5F); chatPresence.ForeColor = Muted; chatPresence.Location = new Point(64, 29); header.Controls.Add(chatPresence);
        var taskContext = MakeButton("▤  Task", "MessagesTaskContextButton", false, 150); taskContext.Anchor = AnchorStyles.Top | AnchorStyles.Right; taskContext.Click += (_, _) => OpenConversationTask(); header.Controls.Add(taskContext);
        var menu = MakeButton("⋮", "MessagesChatMenuButton", false, 36); menu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chatMenu = new ContextMenuStrip();
                chatMenu.Items.Add("Archive conversation", null, (_, _) => ArchiveConversation());
        chatMenu.Items.Add("Report conversation", null, (_, _) => ReportConversation());
        menu.Click += (_, _) => chatMenu.Show(menu, new Point(0, menu.Height));
        header.Controls.Add(menu);
        var banner = new Panel { Name = "messagesTaskBanner", BackColor = Color.FromArgb(247, 250, 254), BorderStyle = BorderStyle.FixedSingle }; chatPane.Controls.Add(banner);
        bannerTask.Name = "messagesBannerTask"; bannerTask.Font = new Font("Segoe UI", 8F, FontStyle.Bold); bannerTask.ForeColor = Navy; bannerTask.Location = new Point(12, 8); banner.Controls.Add(bannerTask);
        bannerDetails.Name = "messagesBannerDetails"; bannerDetails.Font = new Font("Segoe UI", 7F); bannerDetails.ForeColor = Muted; bannerDetails.Location = new Point(12, 26); banner.Controls.Add(bannerDetails);
        bannerStatus.Name = "messagesBannerStatus"; bannerStatus.AutoSize = true; bannerStatus.Padding = new Padding(8, 4, 8, 4); bannerStatus.Font = new Font("Segoe UI", 6.5F, FontStyle.Bold); banner.Controls.Add(bannerStatus);
        transcriptHost.Name = "messagesTranscriptHost"; transcriptHost.AutoScroll = true; transcriptHost.BackColor = Color.FromArgb(250, 252, 255); chatPane.Controls.Add(transcriptHost);
        transcript.Name = "messagesTranscript"; transcript.FlowDirection = FlowDirection.TopDown; transcript.WrapContents = false; transcript.AutoSize = true; transcript.AutoSizeMode = AutoSizeMode.GrowAndShrink; transcript.Padding = new Padding(12); transcript.BackColor = Color.FromArgb(250, 252, 255); transcriptHost.Controls.Add(transcript);
        var composer = new Panel { Name = "messagesComposer", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle }; chatPane.Controls.Add(composer);
        var attach = MakeButton("＋", "MessagesAttachButton", false, 38); attach.Location = new Point(10, 13); attach.Click += (_, _) => AttachFile(); composer.Controls.Add(attach);
        messageInput.Name = "messagesMessageInput"; messageInput.Multiline = true; messageInput.AcceptsReturn = true; messageInput.ScrollBars = ScrollBars.Vertical; messageInput.PlaceholderText = "Write a message..."; messageInput.Font = new Font("Segoe UI", 9F); composer.Controls.Add(messageInput);
        var hint = MakeLabel("Keep exact addresses and payment details private until necessary.", 6.5F, Muted); hint.Name = "messagesComposerHint"; composer.Controls.Add(hint);
        var send = MakeButton("➤", "MessagesSendButton", true, 42); send.Click += (_, _) => SendMessage(); composer.Controls.Add(send);
        messageInput.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter && !e.Shift) { e.SuppressKeyPress = true; SendMessage(); } };
    }

    private void BuildComposeDialog()
    {
        composeBackdrop.Name = "messagesComposeBackdrop"; composeBackdrop.Visible = false; composeBackdrop.BackColor = Color.FromArgb(110, 5, 22, 45); Controls.Add(composeBackdrop);
        composeModal.Name = "messagesComposeModal"; composeModal.Visible = false; composeModal.BackColor = Color.White; composeModal.BorderStyle = BorderStyle.FixedSingle;
        composeModal.Controls.Add(MakeLabel("Start a conversation", 15, Navy, true).At(24, 22));
        composeModal.Controls.Add(MakeLabel("Pick a task and the person you want to talk to.", 8, Muted).At(24, 54));
        composeModal.Controls.Add(MakeLabel("Task and person", 8, Navy, true).At(24, 96));
        composeTask.Name = "messagesComposeTask"; composeTask.DropDownStyle = ComboBoxStyle.DropDownList; composeTask.DisplayMember = nameof(Backend.ComposeTarget.TaskTitle); composeModal.Controls.Add(composeTask);
        var cancel = MakeButton("Cancel", "MessagesCancelComposeButton", false, 90); cancel.Click += (_, _) => CloseCompose(); composeModal.Controls.Add(cancel);
        var open = MakeButton("Open conversation", "MessagesStartComposeButton", true, 140); open.Click += (_, _) => StartConversation(); composeModal.Controls.Add(open);
        Controls.Add(composeModal); composeModal.BringToFront();
    }

    private void ApplyConversationFilters()
    {
        if (!pageBuilt) return;
        var query = conversationSearch.Text.Trim();
        var visible = conversations.Values.Where(conversation =>
            (activeFilter == "all" || activeFilter == "unread" && conversation.Unread || activeFilter == "tasks") &&
            (string.IsNullOrEmpty(query) || $"{conversation.Name} {conversation.Task} {conversation.Search} {conversation.Latest}".Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
        foreach (var (id, button) in conversationButtons) button.Visible = visible.Any(conversation => conversation.Id == id);
        conversationEmpty.Visible = visible.Count == 0;
        conversationCount.Text = $"{visible.Count} chat{(visible.Count == 1 ? "" : "s")}";
        LayoutConversationList();
    }

    private void SelectConversation(string id)
    {
        if (!conversations.TryGetValue(id, out var conversation)) return;
        currentId = id;
        chatPane.Visible = true;
        placeholder.Visible = false;
        if (conversation.UnreadCount > 0 && int.TryParse(id, out var openedId))
            Db.Try(() => Backend.MarkConversationRead(openedId, AppSession.CurrentUser.Id));
        conversation.Unread = false;
        conversation.UnreadCount = 0;
        foreach (var (key, button) in conversationButtons) button.BackColor = key == id ? Color.FromArgb(239, 247, 255) : Color.White;
        chatAvatar.Text = conversation.Initials; chatAvatar.BackColor = conversation.Color;
        chatName.Text = conversation.Name; chatPresence.Text = conversation.Presence;
        bannerTask.Text = conversation.Task; bannerDetails.Text = conversation.Banner;
        bannerStatus.Text = conversation.Status.ToUpperInvariant(); bannerStatus.BackColor = conversation.Status == "Accepted" || conversation.Status == "In progress" ? Color.FromArgb(232, 248, 241) : Color.FromArgb(255, 244, 223); bannerStatus.ForeColor = conversation.Status == "Accepted" || conversation.Status == "In progress" ? Color.FromArgb(7, 131, 90) : Color.FromArgb(173, 104, 0);
        RenderTranscript(conversation);
        UpdateConversationButton(conversation);
        ApplyConversationFilters();
        UpdateUnreadCount();
    }

    private void UpdateUnreadCount() => DemoDataStore.SetUnreadMessages(conversations.Values.Count(c => c.UnreadCount > 0));

    private void ReportConversation()
    {
        if (!conversations.TryGetValue(currentId, out var conversation))
            return;
        DemoDataStore.SubmitReport("Message", $"Conversation with {conversation.Name}", "Reported from Messages", $"Task: {conversation.Task}");
        ShowToast("Report submitted. The TaskTap team will review it.");
    }

    private void OpenConversationTask()
    {
        if (!conversations.TryGetValue(currentId, out var conversation))
            return;
        var task = DemoDataStore.AvailableTasks.FirstOrDefault(t => t.Title == conversation.Task)
                   ?? DemoDataStore.PostedTasks.FirstOrDefault(t => t.Title == conversation.Task);
        if (task is not null)
            Nav.Navigate(new UserTaskDetailsPage(task.Title, task.Category, task.Price, task.Barangay, task.Days, task.Deadline, task.Icon, task.Description, task.IconBack, task.Id));
        else
            Nav.Navigate(Route.FindTasks);
    }

    private void ArchiveConversation()
    {
        if (!conversations.TryGetValue(currentId, out var conversation))
            return;
        if (int.TryParse(currentId, out var hiddenId))
            Db.Try(() => Backend.HideConversation(hiddenId, AppSession.CurrentUser.Id));
        conversations.Remove(currentId);
        if (conversationButtons.TryGetValue(currentId, out var button))
        {
            conversationButtons.Remove(currentId);
            conversationList.Controls.Remove(button);
            button.Dispose();
        }
        currentId = conversations.Keys.FirstOrDefault() ?? "";
        UpdateUnreadCount();
        ApplyConversationFilters();
        if (currentId.Length > 0)
            SelectConversation(currentId);
        else
            placeholder.Visible = true;
        ShowToast($"Conversation with {conversation.Name} archived. A new message will bring it back.");
    }

    private void RenderTranscript(Conversation conversation)
    {
        transcript.SuspendLayout(); transcript.Controls.Clear();
        foreach (var message in conversation.Messages)
        {
            if (message.Day is not null)
            {
                var day = MakeLabel(message.Day, 7, Muted); day.Padding = new Padding(8, 4, 8, 4); day.BackColor = Color.FromArgb(238, 243, 249); day.Margin = new Padding(0, 10, 0, 10); day.Anchor = AnchorStyles.None; transcript.Controls.Add(day); continue;
            }
            var row = new Panel { Name = message.IsMine ? "MyMessageRow" : "OtherMessageRow", Height = 68, Width = Math.Max(250, transcriptHost.ClientSize.Width - 40), BackColor = Color.Transparent, Margin = new Padding(0, 2, 0, 2) };
            var bubble = new Panel { BackColor = message.IsMine ? Color.FromArgb(226, 241, 255) : Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10), AutoSize = true, MaximumSize = new Size(Math.Max(180, row.Width * 2 / 3), 0) };
            var copy = MakeLabel(message.Text, 8.5F, Navy); copy.MaximumSize = new Size(Math.Max(160, row.Width * 2 / 3 - 24), 0); bubble.Controls.Add(copy);
            var time = MakeLabel(message.Time ?? "", 6.5F, Muted); time.Location = new Point(10, copy.Bottom + 4); bubble.Controls.Add(time); bubble.Size = new Size(Math.Min(Math.Max(190, copy.PreferredWidth + 24), row.Width * 2 / 3), copy.PreferredHeight + 28);
            bubble.Location = message.IsMine ? new Point(row.Width - bubble.Width - 8, 4) : new Point(8, 4); row.Controls.Add(bubble); transcript.Controls.Add(row);
        }
        transcript.ResumeLayout(); transcriptHost.ScrollControlIntoView(transcript.Controls.Count > 0 ? transcript.Controls[^1] : transcript); transcriptHost.AutoScrollPosition = new Point(0, transcriptHost.VerticalScroll.Maximum);
    }

    private void AttachFile()
    {
        if (!conversations.ContainsKey(currentId)) return;
        using var dialog = new OpenFileDialog { Title = "Choose a file to mention in the conversation", CheckFileExists = true };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        messageInput.Text = string.IsNullOrWhiteSpace(messageInput.Text)
            ? $"[Attachment: {Path.GetFileName(dialog.FileName)}]"
            : $"{messageInput.Text.Trim()} [Attachment: {Path.GetFileName(dialog.FileName)}]";
        messageInput.Focus();
    }

    private void SendMessage()
    {
        var text = messageInput.Text.Trim();
        if (text.Length == 0) return;
        if (!conversations.TryGetValue(currentId, out var conversation) || !int.TryParse(currentId, out var conversationId))
            return;
        var sent = Db.Try(() => Backend.SendMessage(conversationId, AppSession.CurrentUser.Id, text));
        if (!sent)
            return;
        var time = DateTime.Now.ToString("h:mm tt");
        if (conversation.Messages.Count == 0 || conversation.Messages[^1].Day is null && conversation.Messages.All(message => message.Day != "Today"))
            conversation.Messages.Add(new ChatMessage("Today", false, "Today"));
        conversation.Messages.Add(new ChatMessage(text, true, Time: time));
        conversation.Latest = text;
        conversation.TimeLabel = time;
        messageInput.Clear();
        RenderTranscript(conversation);
        UpdateConversationButton(conversation);
    }

    private void BuildConversationButtons()
    {
        foreach (var conversation in conversations.Values)
        {
            var button = new Button { Name = $"MessagesConversation{conversation.Id}", Tag = conversation.Id, Size = new Size(320, 82), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Navy, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0), UseVisualStyleBackColor = false, Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0; button.Click += (_, _) => SelectConversation(conversation.Id);
            var avatar = new Label { Text = conversation.Initials, Size = new Size(38, 38), Location = new Point(10, 12), BackColor = conversation.Color, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) }; button.Controls.Add(avatar);
            var name = MakeLabel(conversation.Name, 8.5F, Navy, true); name.Name = "ConversationName"; name.Location = new Point(58, 10); name.Cursor = Cursors.Hand; name.Click += (_, _) => Nav.Navigate(ProfileLinks.For(conversation.Name)); button.Controls.Add(name);
            var task = MakeLabel(conversation.Task, 7, Muted); task.Name = "ConversationTask"; task.Location = new Point(58, 29); button.Controls.Add(task);
            var snippet = MakeLabel(conversation.Latest, 7, Muted); snippet.Name = "ConversationSnippet"; snippet.Location = new Point(58, 47); snippet.MaximumSize = new Size(230, 18); button.Controls.Add(snippet);
            var time = MakeLabel(conversation.TimeLabel, 6.5F, Muted); time.Name = "ConversationTime"; button.Controls.Add(time);
            if (conversation.UnreadCount > 0)
            {
                var unread = new Label { Name = "ConversationUnread", Text = conversation.UnreadCount.ToString(), Size = new Size(17, 17), BackColor = Blue, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 6F, FontStyle.Bold) }; button.Controls.Add(unread);
            }
            conversationButtons.Add(conversation.Id, button); conversationList.Controls.Add(button);
        }
    }

    private void BuildMeStrip()
    {
        var strip = new Panel { Name = "messagesMeStrip", BackColor = Color.FromArgb(247, 250, 254), BorderStyle = BorderStyle.FixedSingle };
        var avatar = new Label { Text = AppSession.CurrentUser.Initials, Size = new Size(32, 32), Location = new Point(12, 8), BackColor = Blue, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
        strip.Controls.Add(avatar);
        var name = MakeLabel($"You · {AppSession.CurrentUser.FirstName}", 8F, Navy, true); name.Location = new Point(54, 8); strip.Controls.Add(name);
        var role = MakeLabel(AppSession.CurrentUser.Role, 7F, Muted); role.Location = new Point(54, 27); strip.Controls.Add(role);
        conversationPane.Controls.Add(strip);
    }

    private void UpdateConversationButton(Conversation conversation)
    {
        if (!conversationButtons.TryGetValue(conversation.Id, out var button)) return;
        var snippet = button.Controls.Find("ConversationSnippet", false).FirstOrDefault(); if (snippet is not null) snippet.Text = conversation.Latest;
        var unread = button.Controls.Find("ConversationUnread", false).FirstOrDefault(); if (unread is not null) button.Controls.Remove(unread);
        conversationButtons[conversation.Id].Invalidate();
    }

    private void SelectFilter(string filter)
    {
        activeFilter = filter;
        foreach (var (key, button) in filterButtons)
        {
            var active = key == filter; button.BackColor = active ? Color.FromArgb(234, 243, 255) : Color.White; button.ForeColor = active ? Blue : Muted;
        }
        ApplyConversationFilters();
    }

    private void OpenCompose()
    {
        List<Backend.ComposeTarget> targets = [];
        Db.Try(() => targets = Backend.LoadComposeTargets(AppSession.CurrentUser.Id));
        composeTask.DisplayMember = "";
        composeTask.Items.Clear();
        composeTask.Items.AddRange([.. targets.Cast<object>()]);
        if (composeTask.Items.Count > 0) composeTask.SelectedIndex = 0;
        composeBackdrop.Visible = true; composeModal.Visible = true; composeBackdrop.BringToFront(); composeModal.BringToFront(); LayoutCompose();
    }

    private void CloseCompose() { composeModal.Visible = false; composeBackdrop.Visible = false; }

    private void StartConversation()
    {
        if (composeTask.SelectedItem is not Backend.ComposeTarget target)
        {
            CloseCompose();
            ShowToast("Request a task or receive a request first, then you can message about it.");
            return;
        }

        var conversationId = 0;
        if (!Db.Try(() => conversationId = Backend.EnsureConversation(target.TaskId, AppSession.CurrentUser.Id, target.OtherId)))
            return;
        CloseCompose();
        if (!conversations.ContainsKey(conversationId.ToString()))
        {
            // Rebuild the page so the new conversation appears in the list.
            Nav.Navigate(new MessagesPage());
            return;
        }
        SelectConversation(conversationId.ToString());
    }

    private void LayoutCompose()
    {
        var width = 450; var height = 235; composeModal.Size = new Size(width, height); composeModal.Location = new Point(Math.Max(0, (ClientSize.Width - width) / 2), Math.Max(0, (ClientSize.Height - height) / 2));
        composeBackdrop.Bounds = ClientRectangle;
        composeTask.Location = new Point(24, 119); composeTask.Size = new Size(width - 48, 32);
        var cancel = composeModal.Controls.Find("MessagesCancelComposeButton", false).FirstOrDefault(); if (cancel is not null) cancel.Location = new Point(width - 232, height - 50);
        var open = composeModal.Controls.Find("MessagesStartComposeButton", false).FirstOrDefault(); if (open is not null) open.Location = new Point(width - 128, height - 50);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || page.ClientSize.Width <= 0) return;
        var width = Math.Max(850, page.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        var left = 22; var contentWidth = width - 44;
        Position("messagesCrumb", left, 12); Position("messagesEyebrow", left, 40); Position("messagesHeading", left, 56); Position("messagesSubtitle", left, 104);
        var newMessage = page.Controls.Find("MessagesNewMessageButton", false).FirstOrDefault(); if (newMessage is not null) newMessage.Location = new Point(width - 22 - newMessage.Width, 65);
        workspace.Bounds = new Rectangle(left, 130, contentWidth, 590);
        var paneWidth = Math.Min(360, Math.Max(300, contentWidth / 3));
        conversationPane.Bounds = new Rectangle(0, 0, paneWidth, workspace.Height);
        chatPane.Bounds = new Rectangle(paneWidth, 0, contentWidth - paneWidth, workspace.Height);
        placeholder.Bounds = chatPane.Bounds;
        var safety = page.Controls.Find("messagesSafetyNote", false).FirstOrDefault(); if (safety is not null) { safety.Bounds = new Rectangle(left, 734, contentWidth, 58); var link = safety.Controls.Find("MessagesSafetyTipsButton", false).FirstOrDefault(); if (link is not null) link.Location = new Point(safety.Width - link.Width - 12, 15); }
        page.AutoScrollMinSize = new Size(0, 810);
        LayoutConversationList(); LayoutChatPane(); LayoutCompose();
    }

    private void LayoutConversationList()
    {
        if (conversationPane.Width <= 0) return;
        var width = conversationPane.ClientSize.Width;
        var compose = conversationPane.Controls.Find("MessagesComposeButton", false).FirstOrDefault(); if (compose is not null) compose.Location = new Point(width - compose.Width - 10, 10);
        conversationSearch.Location = new Point(10, 57); conversationSearch.Size = new Size(width - 20, 32);
        var x = 10; foreach (var (key, filter) in filterButtons) { filter.Location = new Point(x, 97); filter.Size = new Size(key == "unread" ? 95 : 72, 28); x += filter.Width + 6; }
        conversationList.Location = new Point(0, 133); conversationList.Size = new Size(width, conversationPane.Height - 138 - 50);
        var meStrip = conversationPane.Controls.Find("messagesMeStrip", false).FirstOrDefault();
        if (meStrip is not null) meStrip.Bounds = new Rectangle(0, conversationPane.Height - 50, width, 50);
        var y = conversationList.AutoScrollPosition.Y;
        foreach (var (id, button) in conversationButtons)
        {
            button.Size = new Size(width - 10, 78); button.Location = new Point(4, y); y += 80;
            var time = button.Controls.Find("ConversationTime", false).FirstOrDefault(); if (time is not null) time.Location = new Point(button.Width - time.Width - 8, 10);
            var unread = button.Controls.Find("ConversationUnread", false).FirstOrDefault(); if (unread is not null) unread.Location = new Point(button.Width - 25, 50);
        }
        conversationEmpty.Location = new Point(12, 200);
    }

    private void LayoutChatPane()
    {
        if (chatPane.Width <= 0) return;
        var width = chatPane.ClientSize.Width; var height = chatPane.ClientSize.Height;
        var header = chatPane.Controls.Find("messagesChatHeader", false).FirstOrDefault();
        if (header is not null)
        {
            header.Bounds = new Rectangle(0, 0, width, 60);
            var back = header.Controls.Find("MessagesBackConversationsButton", false).FirstOrDefault(); if (back is not null) back.Location = new Point(4, 12);
            chatAvatar.Location = new Point(16, 10); chatName.Location = new Point(64, 9); chatPresence.Location = new Point(64, 29);
            var menu = header.Controls.Find("MessagesChatMenuButton", false).FirstOrDefault(); if (menu is not null) menu.Location = new Point(width - menu.Width - 8, 10);
            var context = header.Controls.Find("MessagesTaskContextButton", false).FirstOrDefault();
            if (context is not null)
            {
                context.Location = new Point(width - (menu?.Width ?? 0) - context.Width - 12, 11);
                context.Text = conversations.TryGetValue(currentId, out var activeConversation) ? activeConversation.Task : "";
            }
        }
        var banner = chatPane.Controls.Find("messagesTaskBanner", false).FirstOrDefault(); if (banner is not null) { banner.Bounds = new Rectangle(8, 66, width - 16, 48); bannerStatus.Location = new Point(banner.Width - bannerStatus.Width - 10, 12); }
        transcriptHost.Bounds = new Rectangle(0, 120, width, Math.Max(160, height - 206));
        foreach (Control row in transcript.Controls) row.Width = Math.Max(250, transcriptHost.ClientSize.Width - 40);
        var composer = chatPane.Controls.Find("messagesComposer", false).FirstOrDefault();
        if (composer is not null)
        {
            composer.Bounds = new Rectangle(8, height - 78, width - 16, 70);
            var attach = composer.Controls.Find("MessagesAttachButton", false).FirstOrDefault(); if (attach is not null) attach.Location = new Point(8, 14);
            messageInput.Bounds = new Rectangle(54, 8, Math.Max(120, width - 132), 38);
            var hint = composer.Controls.Find("messagesComposerHint", false).FirstOrDefault(); if (hint is not null) hint.Location = new Point(56, 48);
            var send = composer.Controls.Find("MessagesSendButton", false).FirstOrDefault(); if (send is not null) send.Location = new Point(width - 68, 14);
        }
    }

    private void BuildConversationListControls()
    {
        BuildConversationButtons();
        SelectConversation(currentId);
    }

    private void ClearConversationFilter() { conversationSearch.Clear(); SelectFilter("all"); }

    private void AddPageLabel(string name, string text, float size, Color color, bool bold = false)
    {
        var label = MakeLabel(text, size, color, bold); label.Name = name; page.Controls.Add(label);
    }

    private void Position(string name, int x, int y)
    {
        var control = page.Controls.Find(name, false).FirstOrDefault(); if (control is not null) control.Location = new Point(x, y);
    }

    private void ShowToast(string message)
    {
        toast.Text = message; toast.Visible = true; toast.BringToFront(); toastTimer.Stop(); toastTimer.Start();
    }

    private static Button MakeButton(string text, string name, bool primary, int width)
    {
        var button = new Button { Name = name, Text = text, Size = new Size(width, 36), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false }; button.FlatAppearance.BorderColor = primary ? Blue : Line; button.FlatAppearance.BorderSize = primary ? 0 : 1; return button;
    }

    private static Button MakeTextButton(string text, string name)
    {
        var button = new Button { Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false }; button.FlatAppearance.BorderSize = 0; return button;
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new() { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            toastTimer.Dispose();
            chatMenu?.Dispose();
        }
        base.Dispose(disposing);
    }
}
