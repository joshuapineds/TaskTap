namespace TaskTap;

public sealed class PostTaskPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(113, 131, 155);
    private static readonly Color Page = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel scrollHost = new();
    private readonly Panel canvas = new();
    private readonly Panel stepper = new();
    private readonly Panel formCard = new();
    private readonly Panel previewCard = new();
    private readonly Panel tipCard = new();
    private readonly Panel checklistCard = new();
    private readonly Panel[] stepPanels = new Panel[4];
    private readonly Button[] stepButtons = new Button[4];
    private readonly Label stepKicker = new();
    private readonly Label stepHeading = new();
    private readonly Label stepDescription = new();
    private readonly TextBox taskTitle = new();
    private readonly ComboBox category = new();
    private readonly ComboBox taskType = new();
    private readonly TextBox description = new();
    private readonly TextBox requirements = new();
    private readonly ComboBox duration = new();
    private readonly ComboBox paymentMethod = new();
    private readonly ComboBox budgetType = new();
    private readonly TextBox amount = new();
    private readonly DateTimePicker deadline = new();
    private readonly DateTimePicker startTime = new();
    private readonly ComboBox barangay = new();
    private readonly TextBox scheduleNotes = new();
    private readonly Label descriptionCount = new();
    private readonly Label errorLabel = new();
    private readonly Label previewTitle = new();
    private readonly Label previewPrice = new();
    private readonly Label previewCategory = new();
    private readonly Label previewArea = new();
    private readonly Label previewDate = new();
    private readonly Label previewDescription = new();
    private readonly Label previewImageText = new();
    private readonly PictureBox previewImage = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2400 };
    private int currentStep = 1;
    private string? selectedPhotoPath;
    private bool isUpdatingPreview;

    public override Route RouteName => Route.PostTask;

    public PostTaskPage()
    {
        BuildPage();
    }

    private void BuildPage()
    {
        scrollHost.Dock = DockStyle.Fill;
        scrollHost.AutoScroll = true;
        scrollHost.BackColor = Page;
        scrollHost.Resize += (_, _) => LayoutPage();
        Controls.Add(scrollHost);
        canvas.BackColor = Page;
        scrollHost.Controls.Add(canvas);
        BuildContent();
        toast.Size = new Size(420, 40);
        toast.BackColor = Navy;
        toast.ForeColor = Color.White;
        toast.TextAlign = ContentAlignment.MiddleCenter;
        toast.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toast.Visible = false;
        Controls.Add(toast);
        toast.BringToFront();
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); toast.Visible = false; };
        Resize += (_, _) => toast.Location = new Point((ClientSize.Width - toast.Width) / 2, ClientSize.Height - toast.Height - 18);
        LayoutPage();
        ShowStep(1);
        UpdatePreview();
    }

    private void BuildContent()
    {
        var eyebrow = MakeLabel("CREATE A TASK", 9, Blue, true);
        eyebrow.Name = "postTaskEyebrow";
        canvas.Controls.Add(eyebrow);
        var heading = MakeLabel("Post a Task", 26, Navy, true);
        heading.Name = "postTaskHeading";
        canvas.Controls.Add(heading);
        var subtitle = MakeLabel("Tell your community what you need, when you need it, and what you’re offering.", 10, Muted);
        subtitle.Name = "postTaskSubtitle";
        canvas.Controls.Add(subtitle);
        var cancel = MakeAction("Cancel", "PostTaskCancelButton", false, 82);
        cancel.Click += (_, _) => Nav.Back();
        canvas.Controls.Add(cancel);
        BuildStepper();
        BuildSteps();
        BuildPreview();
        BuildSideTips();
        BuildFooterButtons();
        foreach (var control in new Control[] { taskTitle, category, taskType, description, requirements, amount, paymentMethod, budgetType, deadline, startTime, duration, barangay, scheduleNotes })
        {
            control.TextChanged += (_, _) => UpdatePreview();
            if (control is ComboBox combo)
                combo.SelectedIndexChanged += (_, _) => UpdatePreview();
            if (control is DateTimePicker picker)
                picker.ValueChanged += (_, _) => UpdatePreview();
        }
    }

    private void BuildStepper()
    {
        stepper.Name = "postTaskStepper";
        stepper.BackColor = Color.White;
        stepper.BorderStyle = BorderStyle.FixedSingle;
        var labels = new[] { ("Basic Info", "What do you need?"), ("Details", "Describe the task"), ("Compensation", "Set the budget"), ("Schedule", "When and where") };
        for (var index = 0; index < labels.Length; index++)
        {
            var number = index + 1;
            var button = new Button { Name = $"PostTaskStep{number}Button", Text = $"{number}   {labels[index].Item1}\n     {labels[index].Item2}", TextAlign = ContentAlignment.MiddleLeft, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Muted, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false, Tag = number };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) => { var target = (int)button.Tag!; if (target <= currentStep) ShowStep(target); else AdvanceTo(target); };
            stepButtons[index] = button;
            stepper.Controls.Add(button);
        }
        canvas.Controls.Add(stepper);
    }

    private void BuildSteps()
    {
        formCard.Name = "postTaskFormCard";
        formCard.BackColor = Color.White;
        formCard.BorderStyle = BorderStyle.FixedSingle;
        canvas.Controls.Add(formCard);
        for (var step = 1; step <= 4; step++)
        {
            var panel = new Panel { Name = $"postTaskStepPanel{step}", BackColor = Color.White };
            var kicker = MakeLabel($"STEP {step} OF 4", 8, Blue, true);
            kicker.Name = "stepKicker";
            kicker.Location = new Point(18, 16);
            panel.Controls.Add(kicker);
            var title = MakeLabel(string.Empty, 18, Navy, true);
            title.Name = "stepTitle";
            title.Location = new Point(18, 35);
            panel.Controls.Add(title);
            var subtitle = MakeLabel(string.Empty, 8.5F, Muted);
            subtitle.Name = "stepDescription";
            subtitle.Location = new Point(18, 63);
            panel.Controls.Add(subtitle);
            stepPanels[step - 1] = panel;
            formCard.Controls.Add(panel);
        }
        ConfigureStepOne();
        ConfigureStepTwo();
        ConfigureStepThree();
        ConfigureStepFour();
    }

    private void ConfigureStepOne()
    {
        var panel = stepPanels[0];
        SetStepText(panel, "Start with the basics", "Give your task a clear title and choose the closest category.");
        AddFieldLabel(panel, "Task title *", 18, 99);
        taskTitle.Name = "postTaskTitleInput";
        taskTitle.MaxLength = 80;
        taskTitle.PlaceholderText = "e.g. Fix a leaking faucet";
        taskTitle.Location = new Point(18, 122);
        taskTitle.Size = new Size(650, 34);
        panel.Controls.Add(taskTitle);
        AddHint(panel, "Keep it short and specific.", 18, 160);
        AddFieldLabel(panel, "Category *", 18, 186);
        category.Name = "postTaskCategory";
        category.DropDownStyle = ComboBoxStyle.DropDownList;
        category.Items.Add("Select a category");
        category.Items.AddRange(DemoDataStore.ActiveCategoryNames());
        category.SelectedIndex = 0;
        category.Location = new Point(18, 209);
        category.Size = new Size(310, 32);
        panel.Controls.Add(category);
        AddFieldLabel(panel, "Task type", 352, 186);
        taskType.Name = "postTaskType";
        taskType.DropDownStyle = ComboBoxStyle.DropDownList;
        taskType.Items.AddRange(["One-time task", "Short-term / recurring"]);
        taskType.SelectedIndex = 0;
        taskType.Location = new Point(352, 209);
        taskType.Size = new Size(310, 32);
        panel.Controls.Add(taskType);
        AddTip(panel, "Good task titles get better requests. Include the action and item involved.", 18, 260, false);
    }

    private void ConfigureStepTwo()
    {
        var panel = stepPanels[1];
        SetStepText(panel, "Add task details", "Explain what needs to be done so helpers can decide if they’re a good fit.");
        AddFieldLabel(panel, "Description *", 18, 99);
        description.Name = "postTaskDescription";
        description.Multiline = true;
        description.MaxLength = 1000;
        description.ScrollBars = ScrollBars.Vertical;
        description.PlaceholderText = "Describe the task, what needs to be done, and anything a helper should know...";
        description.Location = new Point(18, 122);
        description.Size = new Size(650, 100);
        panel.Controls.Add(description);
        descriptionCount.Name = "postTaskDescriptionCount";
        descriptionCount.Text = "0 / 1000 characters";
        descriptionCount.Location = new Point(18, 226);
        descriptionCount.AutoSize = true;
        descriptionCount.ForeColor = Muted;
        panel.Controls.Add(descriptionCount);
        AddFieldLabel(panel, "Requirements (optional, one per line)", 18, 252);
        requirements.Name = "postTaskRequirements";
        requirements.Multiline = true;
        requirements.ScrollBars = ScrollBars.Vertical;
        requirements.PlaceholderText = "e.g. Basic plumbing experience\r\nBring basic tools";
        requirements.Location = new Point(18, 275);
        requirements.Size = new Size(310, 92);
        panel.Controls.Add(requirements);
        AddFieldLabel(panel, "Task photo (optional)", 352, 252);
        var upload = MakeAction("Choose photo…", "PostTaskChoosePhotoButton", false, 160);
        upload.Location = new Point(352, 278);
        upload.Click += (_, _) => ChoosePhoto();
        panel.Controls.Add(upload);
        var clear = MakeTextButton("Remove", "PostTaskRemovePhotoButton");
        clear.Location = new Point(520, 285);
        clear.Click += (_, _) => ClearPhoto();
        panel.Controls.Add(clear);
        var photoName = MakeLabel("JPG or PNG, up to 5 MB", 8, Muted);
        photoName.Name = "postTaskPhotoName";
        photoName.Location = new Point(352, 318);
        panel.Controls.Add(photoName);
        AddTip(panel, "Do not include passwords, OTPs, bank details, or other sensitive information.", 18, 382, false);
    }

    private void ConfigureStepThree()
    {
        var panel = stepPanels[2];
        SetStepText(panel, "Set compensation", "Choose a fair amount and tell helpers how the payment will be arranged.");
        AddFieldLabel(panel, "Compensation amount *", 18, 99);
        var currency = MakeLabel("₱", 11, Navy, true);
        currency.Location = new Point(20, 130);
        panel.Controls.Add(currency);
        amount.Name = "postTaskAmount";
        amount.PlaceholderText = "600";
        amount.Location = new Point(43, 122);
        amount.Size = new Size(285, 34);
        amount.MaxLength = 6;
        amount.TextChanged += (_, _) => { if (amount.Text.Any(character => !char.IsDigit(character))) { var caret = amount.SelectionStart; amount.Text = new string(amount.Text.Where(char.IsDigit).ToArray()); amount.SelectionStart = Math.Min(caret, amount.TextLength); } };
        panel.Controls.Add(amount);
        AddHint(panel, "Enter a total amount from ₱1 to ₱999,999.", 18, 160);
        AddFieldLabel(panel, "Suggested amounts", 18, 190);
        var values = new[] { "300", "500", "600", "1,000" };
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index].Replace(",", string.Empty);
            var suggestion = MakeAction($"₱{values[index]}", $"PostTaskSuggestedAmount{value}", false, 70);
            suggestion.Location = new Point(18 + index * 78, 214);
            suggestion.Click += (_, _) => { amount.Text = value; UpdatePreview(); };
            panel.Controls.Add(suggestion);
        }
        AddFieldLabel(panel, "Payment method *", 18, 268);
        paymentMethod.Name = "postTaskPaymentMethod";
        paymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        paymentMethod.Items.AddRange(["Select a method", "Cash", "GCash", "Bank transfer", "Other agreed method"]);
        paymentMethod.SelectedIndex = 0;
        paymentMethod.Location = new Point(18, 291);
        paymentMethod.Size = new Size(310, 32);
        panel.Controls.Add(paymentMethod);
        AddFieldLabel(panel, "Budget type", 352, 268);
        budgetType.Name = "postTaskBudgetType";
        budgetType.DropDownStyle = ComboBoxStyle.DropDownList;
        budgetType.Items.AddRange(["Fixed total", "Hourly rate"]);
        budgetType.SelectedIndex = 0;
        budgetType.Location = new Point(352, 291);
        budgetType.Size = new Size(310, 32);
        panel.Controls.Add(budgetType);
        AddTip(panel, "TaskTap does not process payments. Members arrange payment directly.", 18, 348, true);
    }

    private void ConfigureStepFour()
    {
        var panel = stepPanels[3];
        SetStepText(panel, "Set the schedule and area", "Give helpers a practical idea of when and where the task should happen.");
        AddFieldLabel(panel, "Deadline *", 18, 99);
        deadline.Name = "postTaskDeadline";
        deadline.Format = DateTimePickerFormat.Short;
        deadline.MinDate = DateTime.Today;
        deadline.Value = DateTime.Today.AddDays(1);
        deadline.Location = new Point(18, 122);
        deadline.Size = new Size(310, 32);
        panel.Controls.Add(deadline);
        AddFieldLabel(panel, "Preferred start time", 352, 99);
        startTime.Name = "postTaskStartTime";
        startTime.Format = DateTimePickerFormat.Time;
        startTime.ShowUpDown = true;
        startTime.Location = new Point(352, 122);
        startTime.Size = new Size(310, 32);
        panel.Controls.Add(startTime);
        AddFieldLabel(panel, "Estimated duration", 18, 171);
        duration.Name = "postTaskDuration";
        duration.DropDownStyle = ComboBoxStyle.DropDownList;
        duration.Items.AddRange(["Less than 1 hour", "1–2 hours", "2–3 hours", "Half day", "Full day", "Multiple days"]);
        duration.SelectedIndex = 1;
        duration.Location = new Point(18, 194);
        duration.Size = new Size(310, 32);
        panel.Controls.Add(duration);
        AddFieldLabel(panel, "Barangay / task area *", 352, 171);
        barangay.Name = "postTaskBarangay";
        barangay.DropDownStyle = ComboBoxStyle.DropDownList;
        barangay.Items.Add("Select barangay");
        barangay.Items.AddRange(DemoDataStore.ActiveBarangayNames());
        barangay.SelectedIndex = 0;
        barangay.Location = new Point(352, 194);
        barangay.Size = new Size(310, 32);
        panel.Controls.Add(barangay);
        AddFieldLabel(panel, "Schedule notes (optional)", 18, 242);
        scheduleNotes.Name = "postTaskScheduleNotes";
        scheduleNotes.Multiline = true;
        scheduleNotes.PlaceholderText = "Flexible timing, access notes, or other scheduling information";
        scheduleNotes.Location = new Point(18, 265);
        scheduleNotes.Size = new Size(644, 38);
        panel.Controls.Add(scheduleNotes);
        BuildPinControls(panel, 310);
        AddTip(panel, "Only share an approximate task area. Do not post a private home address.", 18, 347, false);
    }

    private double? pinLat;
    private double? pinLng;
    private readonly Button pinButton = new();
    private readonly Label pinLabel = new();

    private void BuildPinControls(Panel panel, int top)
    {
        pinButton.Name = "taskPinButton";
        pinButton.Text = "📍 Pin the spot on the map (optional)";
        pinButton.Location = new Point(18, top);
        pinButton.Size = new Size(262, 30);
        pinButton.FlatStyle = FlatStyle.Flat;
        pinButton.BackColor = Color.White;
        pinButton.ForeColor = Color.FromArgb(8, 117, 232);
        pinButton.FlatAppearance.BorderColor = Color.FromArgb(8, 117, 232);
        pinButton.Cursor = Cursors.Hand;
        pinButton.Click += (_, _) => PickLocation();
        panel.Controls.Add(pinButton);
        pinLabel.Name = "taskPinLabel";
        pinLabel.AutoSize = true;
        pinLabel.ForeColor = Color.FromArgb(91, 111, 134);
        pinLabel.Font = new Font("Segoe UI", 8F);
        pinLabel.Location = new Point(290, top + 7);
        panel.Controls.Add(pinLabel);
        UpdatePinLabel();
    }

    private void PickLocation()
    {
        var area = string.IsNullOrEmpty(barangay.Text) || barangay.Text.StartsWith("Select", StringComparison.Ordinal) ? "" : barangay.Text;
        if (LocationPickerDialog.Pick(this, pinLat, pinLng, area) is { } picked)
        {
            pinLat = picked.Lat;
            pinLng = picked.Lng;
            UpdatePinLabel();
        }
    }

    private void UpdatePinLabel() => pinLabel.Text = pinLat is null
        ? "Not pinned: the barangay area is used."
        : "Pinned. Others see only an approximate area.";

    private void BuildPreview()
    {
        previewCard.Name = "postTaskLivePreview";
        previewCard.BackColor = Color.White;
        previewCard.BorderStyle = BorderStyle.FixedSingle;
        AddCardHeader(previewCard, "LIVE PREVIEW", "Your task");
        previewImage.Name = "postTaskPreviewImage";
        previewImage.Location = new Point(14, 47);
        previewImage.Size = new Size(220, 100);
        previewImage.BackColor = Color.FromArgb(239, 246, 253);
        previewImage.SizeMode = PictureBoxSizeMode.Zoom;
        previewImageText.Name = "postTaskPreviewImageText";
        previewImageText.Text = "＋\r\nTask photo";
        previewImageText.TextAlign = ContentAlignment.MiddleCenter;
        previewImageText.ForeColor = Muted;
        previewImageText.Location = new Point(14, 47);
        previewImageText.Size = new Size(220, 100);
        previewCard.Controls.Add(previewImage);
        previewCard.Controls.Add(previewImageText);
        var draftBadge = MakeBadge("Draft", 14, 154);
        previewCard.Controls.Add(draftBadge);
        previewTitle.Name = "postTaskPreviewTitle";
        previewTitle.Text = "Your task title";
        previewTitle.Location = new Point(14, 177);
        previewTitle.MaximumSize = new Size(220, 35);
        previewTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        previewTitle.ForeColor = Navy;
        previewCard.Controls.Add(previewTitle);
        previewPrice.Name = "postTaskPreviewPrice";
        previewPrice.Text = "₱0";
        previewPrice.Location = new Point(14, 212);
        previewPrice.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        previewPrice.ForeColor = Navy;
        previewCard.Controls.Add(previewPrice);
        previewCategory.Name = "postTaskPreviewCategory";
        previewArea.Name = "postTaskPreviewArea";
        previewDate.Name = "postTaskPreviewDate";
        AddPreviewPill(previewCard, previewCategory, "Category", 14, 245);
        AddPreviewPill(previewCard, previewArea, "Barangay", 84, 245);
        AddPreviewPill(previewCard, previewDate, "Deadline", 158, 245);
        previewDescription.Name = "postTaskPreviewDescription";
        previewDescription.Text = "Your task description will appear here as you fill out the form.";
        previewDescription.Location = new Point(14, 274);
        previewDescription.MaximumSize = new Size(220, 50);
        previewDescription.Font = new Font("Segoe UI", 7.5F);
        previewDescription.ForeColor = Muted;
        previewCard.Controls.Add(previewDescription);
        canvas.Controls.Add(previewCard);
    }

    private static void SetStepText(Panel panel, string title, string subtitle)
    {
        var titleLabel = panel.Controls.Find("stepTitle", false).FirstOrDefault();
        var descriptionLabel = panel.Controls.Find("stepDescription", false).FirstOrDefault();
        if (titleLabel is not null)
            titleLabel.Text = title;
        if (descriptionLabel is not null)
            descriptionLabel.Text = subtitle;
    }

    private void BuildSideTips()
    {
        tipCard.Name = "postTaskTipsCard";
        tipCard.BackColor = Color.FromArgb(238, 247, 255);
        tipCard.BorderStyle = BorderStyle.FixedSingle;
        var icon = MakeLabel("?", 13, Blue, true);
        icon.Location = new Point(12, 12);
        tipCard.Controls.Add(icon);
        var title = MakeLabel("Need a good task post?", 8, Navy, true);
        title.Location = new Point(40, 12);
        tipCard.Controls.Add(title);
        var text = MakeLabel("Use a specific title, explain the expected result, set a clear budget, and choose the broad task area.", 7.5F, Muted);
        text.Name = "postTaskTipsText";
        text.Location = new Point(40, 31);
        text.MaximumSize = new Size(190, 43);
        tipCard.Controls.Add(text);
        var link = MakeTextButton("View posting tips →", "PostTaskTipsButton");
        link.Location = new Point(40, 75);
        link.Click += (_, _) => Nav.Navigate(Route.HelpFaq);
        tipCard.Controls.Add(link);
        canvas.Controls.Add(tipCard);

        checklistCard.Name = "postTaskChecklistCard";
        checklistCard.BackColor = Color.White;
        checklistCard.BorderStyle = BorderStyle.FixedSingle;
        var heading = MakeLabel("Before you publish", 8, Navy, true);
        heading.Location = new Point(12, 12);
        checklistCard.Controls.Add(heading);
        var checks = new[] { "Clear task description", "Compensation agreed", "Deadline selected", "Approximate area only" };
        for (var index = 0; index < checks.Length; index++)
        {
            AddThinDivider(checklistCard, 36 + index * 26);
            var check = MakeLabel("✓", 8, Color.FromArgb(35, 175, 137), true);
            check.Location = new Point(14, 42 + index * 26);
            checklistCard.Controls.Add(check);
            var item = MakeLabel(checks[index], 7.5F, Muted);
            item.Location = new Point(32, 43 + index * 26);
            checklistCard.Controls.Add(item);
        }
        canvas.Controls.Add(checklistCard);
    }

    private void BuildFooterButtons()
    {
        var back = MakeAction("← Back", "PostTaskBackButton", false, 86);
        back.Name = "postTaskBackButton";
        back.Click += (_, _) => { if (currentStep > 1) ShowStep(currentStep - 1); };
        canvas.Controls.Add(back);
        var save = MakeTextButton("Save draft", "PostTaskSaveDraftButton");
        save.Click += (_, _) => ShowToast("Drafts are not saved yet. Publish when you are ready.");
        canvas.Controls.Add(save);
        var next = MakeAction("Continue →", "PostTaskContinueButton", true, 100);
        next.Click += (_, _) => AdvanceTo(currentStep + 1);
        canvas.Controls.Add(next);
        var publish = MakeAction("Publish Task", "PostTaskPublishButton", true, 110);
        publish.Click += (_, _) => PublishTask();
        canvas.Controls.Add(publish);
        errorLabel.Name = "postTaskValidationError";
        errorLabel.AutoSize = false;
        errorLabel.ForeColor = Color.FromArgb(185, 55, 67);
        errorLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        errorLabel.Visible = false;
        canvas.Controls.Add(errorLabel);
    }

    private void LayoutPage()
    {
        if (scrollHost.ClientSize.Width <= 0 || canvas.Controls.Find("postTaskEyebrow", false).Length == 0)
            return;
        var width = Math.Max(840, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 32);
        canvas.Size = new Size(width, 840);
        canvas.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - width) / 2), 0);
        var left = 22;
        var right = width - 22;
        Position("postTaskEyebrow", left, 18);
        Position("postTaskHeading", left, 37);
        Position("postTaskSubtitle", left, 96);
        var cancel = Find<Button>("PostTaskCancelButton");
        if (cancel is not null) cancel.Location = new Point(right - cancel.Width, 42);
        stepper.Bounds = new Rectangle(left, 118, width - 44, 64);
        var stepWidth = (stepper.Width - 16) / 4;
        for (var index = 0; index < stepButtons.Length; index++)
        {
            stepButtons[index].Bounds = new Rectangle(8 + index * stepWidth, 8, stepWidth, 46);
            var active = index + 1 == currentStep;
            var done = index + 1 < currentStep;
            stepButtons[index].ForeColor = active || done ? Blue : Muted;
            stepButtons[index].Font = new Font("Segoe UI", 8F, active ? FontStyle.Bold : FontStyle.Regular);
        }
        var bodyTop = 196;
        var previewWidth = 254;
        var gap = 14;
        var mainWidth = width - 44 - previewWidth - gap;
        formCard.Bounds = new Rectangle(left, bodyTop, mainWidth, 380);
        for (var index = 0; index < stepPanels.Length; index++)
        {
            stepPanels[index].Bounds = new Rectangle(0, 0, mainWidth, 380);
            LayoutStepControls(stepPanels[index], mainWidth);
        }
        previewCard.Bounds = new Rectangle(left + mainWidth + gap, bodyTop, previewWidth, 285);
        LayoutPreview(previewWidth);
        tipCard.Bounds = new Rectangle(left + mainWidth + gap, bodyTop + 299, previewWidth, 105);
        checklistCard.Bounds = new Rectangle(left + mainWidth + gap, bodyTop + 418, previewWidth, 152);
        var controlsBottom = bodyTop + 425;
        var back = Find<Button>("postTaskBackButton");
        var save = Find<Button>("PostTaskSaveDraftButton");
        var next = Find<Button>("PostTaskContinueButton");
        var publish = Find<Button>("PostTaskPublishButton");
        if (back is not null) back.Location = new Point(left, controlsBottom);
        if (save is not null) save.Location = new Point(left + mainWidth - 300, controlsBottom + 7);
        if (next is not null) next.Location = new Point(left + mainWidth - 196, controlsBottom);
        if (publish is not null) publish.Location = new Point(left + mainWidth - 88, controlsBottom);
        errorLabel.Location = new Point(left + 100, controlsBottom + 9);
        errorLabel.Size = new Size(Math.Max(100, mainWidth - 410), 26);
        canvas.Height = bodyTop + 485;
        ShowStep(currentStep);
    }

    private void LayoutStepControls(Panel panel, int width)
    {
        foreach (Control control in panel.Controls)
        {
            if (control is TextBox textBox && textBox.Name is "postTaskTitleInput" or "postTaskDescription")
                textBox.Width = width - 36;
            if (control is TextBox textBox2 && textBox2.Name == "postTaskScheduleNotes")
                textBox2.Width = width - 36;
            if (control is TextBox textBox3 && textBox3.Name == "postTaskDescription")
                textBox3.Width = width - 36;
            if (control is ComboBox combo && combo.Name is "postTaskCategory" or "postTaskType" or "postTaskPaymentMethod" or "postTaskBudgetType" or "postTaskDuration" or "postTaskBarangay")
                combo.Width = (width - 54) / 2;
            if (control is ComboBox combo2 && combo2.Name is "postTaskType" or "postTaskBudgetType" or "postTaskBarangay")
                combo2.Left = (width - 54) / 2 + 18;
        }
        if (panel.Controls.Find("postTaskDescription", false).FirstOrDefault() is TextBox descriptionInput)
        {
            descriptionInput.TextChanged -= UpdateDescriptionCount;
            descriptionInput.TextChanged += UpdateDescriptionCount;
        }
    }

    private void LayoutPreview(int width)
    {
        foreach (Control control in previewCard.Controls)
        {
            if (control.Name == "postTaskPreviewImage" || control.Name == "postTaskPreviewImageText")
                control.Width = width - 28;
        }
        previewTitle.MaximumSize = new Size(width - 28, 35);
        previewDescription.MaximumSize = new Size(width - 28, 50);
    }

    private void UpdateDescriptionCount(object? sender, EventArgs e)
    {
        descriptionCount.Text = $"{description.TextLength} / 1000 characters";
    }

    private void UpdatePreview()
    {
        if (isUpdatingPreview || previewTitle.IsDisposed)
            return;
        isUpdatingPreview = true;
        previewTitle.Text = string.IsNullOrWhiteSpace(taskTitle.Text) ? "Your task title" : taskTitle.Text.Trim();
        previewPrice.Text = $"₱{(int.TryParse(amount.Text, out var parsed) ? parsed : 0):N0}";
        previewCategory.Text = category.SelectedIndex > 0 ? category.Text : "Category";
        previewArea.Text = barangay.SelectedIndex > 0 ? barangay.Text : "Barangay";
        previewDate.Text = currentStep < 4 || deadline.Value == DateTime.Today.AddDays(1) ? "Deadline" : deadline.Value.ToString("MMM d, yyyy");
        previewDescription.Text = string.IsNullOrWhiteSpace(description.Text) ? "Your task description will appear here as you fill out the form." : description.Text.Trim();
        descriptionCount.Text = $"{description.TextLength} / 1000 characters";
        isUpdatingPreview = false;
    }

    private void ShowStep(int step)
    {
        currentStep = Math.Clamp(step, 1, 4);
        var headings = new[] { "Start with the basics", "Add task details", "Set compensation", "Set the schedule and area" };
        var descriptions = new[]
        {
            "Give your task a clear title and choose the closest category.",
            "Explain what needs to be done so helpers can decide if they’re a good fit.",
            "Choose a fair amount and tell helpers how the payment will be arranged.",
            "Give helpers a practical idea of when and where the task should happen."
        };
        for (var index = 0; index < stepPanels.Length; index++)
        {
            stepPanels[index].Visible = index + 1 == currentStep;
            stepPanels[index].BringToFront();
            var titleLabel = stepPanels[index].Controls.Find("stepTitle", false).FirstOrDefault();
            var descriptionLabel = stepPanels[index].Controls.Find("stepDescription", false).FirstOrDefault();
            var kickerLabel = stepPanels[index].Controls.Find("stepKicker", false).FirstOrDefault();
            if (titleLabel is not null) titleLabel.Text = headings[index];
            if (descriptionLabel is not null) descriptionLabel.Text = descriptions[index];
            if (kickerLabel is not null) kickerLabel.Text = $"STEP {index + 1} OF 4";
        }
        var back = Find<Button>("postTaskBackButton");
        var next = Find<Button>("PostTaskContinueButton");
        var publish = Find<Button>("PostTaskPublishButton");
        if (back is not null) back.Visible = currentStep > 1;
        if (next is not null) next.Visible = currentStep < 4;
        if (publish is not null) publish.Visible = currentStep == 4;
        errorLabel.Visible = false;
        for (var index = 0; index < stepButtons.Length; index++)
        {
            stepButtons[index].ForeColor = index + 1 <= currentStep ? Blue : Muted;
            stepButtons[index].BackColor = index + 1 == currentStep ? Color.FromArgb(239, 247, 255) : Color.White;
        }
        previewCard.BringToFront();
        tipCard.BringToFront();
        checklistCard.BringToFront();
        UpdatePreview();
    }

    private void AdvanceTo(int target)
    {
        if (target < 1 || target > 4)
            return;
        for (var step = currentStep; step < target; step++)
        {
            if (!ValidateStep(step))
                return;
        }
        ShowStep(target);
    }

    private bool ValidateStep(int step)
    {
        var valid = step switch
        {
            1 => !string.IsNullOrWhiteSpace(taskTitle.Text) && category.SelectedIndex > 0,
            2 => !string.IsNullOrWhiteSpace(description.Text),
            3 => int.TryParse(amount.Text, out var value) && value is > 0 and <= 999999 && paymentMethod.SelectedIndex > 0,
            4 => deadline.Value.Date >= DateTime.Today && barangay.SelectedIndex > 0,
            _ => true
        };
        if (!valid)
        {
            errorLabel.Text = step switch
            {
                1 when string.IsNullOrWhiteSpace(taskTitle.Text) => "Please add a task title.",
                1 => "Choose a category.",
                2 => "Please describe what needs to be done.",
                3 when !int.TryParse(amount.Text, out var value) || value is < 1 or > 999999 => "Enter an amount from ₱1 to ₱999,999.",
                3 => "Choose a payment method.",
                _ when barangay.SelectedIndex == 0 => "Choose the task area.",
                _ => "Choose a valid deadline."
            };
            errorLabel.Visible = true;
            ShowToast("Please complete the required fields before continuing.");
            return false;
        }
        errorLabel.Visible = false;
        return true;
    }

    private void AdvanceToLastMissingStep()
    {
        for (var step = 1; step <= 4; step++)
        {
            if (!ValidateStep(step))
            {
                ShowStep(step);
                ValidateStep(step);
                return;
            }
        }
    }

    private void PublishTask()
    {
        for (var step = 1; step <= 4; step++)
        {
            if (!ValidateStep(step))
            {
                ShowStep(step);
                ValidateStep(step);
                return;
            }
        }
        var chosenCategory = category.SelectedItem?.ToString() ?? "Others";
        var categoryInfo = DemoDataStore.Categories.FirstOrDefault(item => item.Name == chosenCategory);
        var before = DemoDataStore.PostedTasks.Count;
        DemoDataStore.PostTask(new DemoTask(
            taskTitle.Text.Trim(),
            chosenCategory,
            int.TryParse(amount.Text, out var price) ? price : 0,
            barangay.SelectedItem?.ToString()?.Replace("Brgy. ", "") ?? "176",
            Math.Max(1, (deadline.Value.Date - DateTime.Today).Days),
            deadline.Value.ToString("MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture),
            categoryInfo?.Icon ?? "📌",
            description.Text.Trim(),
            categoryInfo?.Color ?? Color.FromArgb(232, 241, 255),
            new Point(Random.Shared.Next(15, 85), Random.Shared.Next(15, 85)))
        {
            Requirements = requirements.Text.Trim(),
            TaskType = taskType.SelectedItem?.ToString() ?? "",
            PaymentMethod = paymentMethod.SelectedItem?.ToString() ?? "",
            BudgetType = budgetType.SelectedItem?.ToString() ?? "",
            Duration = duration.SelectedItem?.ToString() ?? "",
            ScheduleNotes = scheduleNotes.Text.Trim(),
            Latitude = pinLat,
            Longitude = pinLng
        });
        if (DemoDataStore.PostedTasks.Count > before)
            Nav.Navigate(Route.MyTasks);
    }

    private void ChoosePhoto()
    {
        using var dialog = new OpenFileDialog { Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png", Title = "Choose a task photo" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        var info = new FileInfo(dialog.FileName);
        if (info.Length > 5 * 1024 * 1024)
        {
            ShowToast("That image is larger than 5 MB.");
            return;
        }
        selectedPhotoPath = dialog.FileName;
        previewImage.Image?.Dispose();
        using var source = Image.FromFile(selectedPhotoPath);
        previewImage.Image = new Bitmap(source);
        previewImageText.Visible = false;
        var label = Find<Label>("postTaskPhotoName");
        if (label is not null) label.Text = $"{info.Name} · {info.Length / 1024d / 1024d:F2} MB";
    }

    private void ClearPhoto()
    {
        selectedPhotoPath = null;
        previewImage.Image?.Dispose();
        previewImage.Image = null;
        previewImageText.Visible = true;
        var label = Find<Label>("postTaskPhotoName");
        if (label is not null) label.Text = "JPG or PNG, up to 5 MB";
    }

    private void ShowToast(string message)
    {
        toast.Text = message;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private void AddFieldLabel(Control parent, string text, int x, int y)
    {
        var label = MakeLabel(text, 8, Navy, true);
        label.Location = new Point(x, y);
        parent.Controls.Add(label);
    }

    private void AddHint(Control parent, string text, int x, int y)
    {
        var hint = MakeLabel(text, 7, Muted);
        hint.Location = new Point(x, y);
        parent.Controls.Add(hint);
    }

    private void AddTip(Control parent, string text, int x, int y, bool orange)
    {
        var tip = new Panel { Name = "postTaskTip", Location = new Point(x, y), Size = new Size(650, 48), BackColor = orange ? Color.FromArgb(255, 247, 235) : Color.FromArgb(238, 247, 255) };
        var icon = MakeLabel(orange ? "₱" : "i", 9, orange ? Color.FromArgb(224, 132, 52) : Blue, true);
        icon.Location = new Point(12, 12);
        tip.Controls.Add(icon);
        var body = MakeLabel(text, 7.5F, Muted);
        body.Location = new Point(32, 12);
        body.MaximumSize = new Size(595, 34);
        tip.Controls.Add(body);
        parent.Controls.Add(tip);
    }

    private void AddPreviewPill(Control parent, Label label, string fallback, int x, int y)
    {
        label.Text = fallback;
        label.AutoSize = false;
        label.Size = new Size(68, 20);
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Location = new Point(x, y);
        label.ForeColor = Muted;
        label.BackColor = Page;
        label.Font = new Font("Segoe UI", 6.5F);
        parent.Controls.Add(label);
    }

    private void AddCardHeader(Control parent, string eyebrow, string title)
    {
        var label = MakeLabel(eyebrow, 8, Blue, true);
        label.Location = new Point(14, 12);
        parent.Controls.Add(label);
        var heading = MakeLabel(title, 10, Navy, true);
        heading.Location = new Point(14, 27);
        parent.Controls.Add(heading);
    }

    private static void AddThinDivider(Control parent, int y)
    {
        parent.Controls.Add(new Panel { BackColor = Line, Location = new Point(12, y), Size = new Size(220, 1) });
    }

    private static Label MakeBadge(string text, int x, int y)
    {
        var badge = MakeLabel(text, 7, Color.FromArgb(192, 111, 56), true);
        badge.Location = new Point(x, y);
        badge.BackColor = Color.FromArgb(255, 241, 228);
        return badge;
    }

    private void Position(string name, int x, int y)
    {
        var control = canvas.Controls.Find(name, false).FirstOrDefault();
        if (control is not null) control.Location = new Point(x, y);
    }

    private TControl? Find<TControl>(string name) where TControl : Control => canvas.Controls.Find(name, false).FirstOrDefault() as TControl;

    private static Button MakeAction(string text, string name, bool primary, int width)
    {
        var button = new Button { Name = name, Text = text, Size = new Size(width, 38), FlatStyle = FlatStyle.Flat, BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Navy, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderColor = primary ? Blue : Line;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        return button;
    }

    private static Button MakeTextButton(string text, string name)
    {
        var button = new Button { Name = name, Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Blue, Font = new Font("Segoe UI", 8F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false)
    {
        return new Label { Text = text, AutoSize = true, ForeColor = color, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };
    }

    private static Button MakeTextButton(string text, string name, bool unused) => MakeTextButton(text, name);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            previewImage.Image?.Dispose();
            toastTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
