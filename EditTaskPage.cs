namespace TaskTap;

public sealed class EditTaskPage : AppPage
{
    private static readonly Color Navy = Color.FromArgb(8, 38, 87);
    private static readonly Color Blue = Color.FromArgb(8, 117, 232);
    private static readonly Color Muted = Color.FromArgb(113, 131, 155);
    private static readonly Color Page = Color.FromArgb(244, 248, 252);
    private static readonly Color Line = Color.FromArgb(225, 233, 241);
    private readonly Panel scrollHost = new();
    private readonly Panel canvas = new();
    private readonly Panel stepper = new();
    private readonly Panel editCard = new();
    private readonly Panel previewCard = new();
    private readonly Panel helpCard = new();
    private readonly Panel controlsCard = new();
    private readonly Panel[] stepPanels = new Panel[4];
    private readonly Button[] stepButtons = new Button[4];
    private readonly Label[] stepTitles = new Label[4];
    private readonly Label[] stepSubtitles = new Label[4];
    private readonly Label stepLabel = new();
    private readonly Label statusPreview = new();
    private readonly Label titlePreview = new();
    private readonly Label pricePreview = new();
    private readonly Label categoryPreview = new();
    private readonly Label areaPreview = new();
    private readonly Label datePreview = new();
    private readonly Label descriptionPreview = new();
    private readonly Label toast = new();
    private readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 2300 };
    private readonly TextBox taskTitle = new();
    private readonly ComboBox category = new();
    private readonly ComboBox taskType = new();
    private readonly ComboBox taskStatus = new();
    private readonly TextBox description = new();
    private readonly TextBox requirements = new();
    private readonly TextBox amount = new();
    private readonly ComboBox paymentMethod = new();
    private readonly DateTimePicker deadline = new();
    private readonly DateTimePicker startTime = new();
    private readonly ComboBox duration = new();
    private readonly ComboBox barangay = new();
    private readonly TextBox scheduleNotes = new();
    private readonly Label validation = new();
    private Button backButton = null!;
    private Button nextButton = null!;
    private Button saveButton = null!;
    private Button pauseButton = null!;
    private Button archiveButton = null!;
    private int currentStep = 1;
    private bool archived;
    private bool updating;
    private bool pageBuilt;
    private DemoTask? editing;

    public override Route RouteName => Route.EditTask;

    public EditTaskPage()
    {
        editing = DemoDataStore.EditingTask is { } picked && DemoDataStore.PostedTasks.Contains(picked)
            ? picked
            : DemoDataStore.PostedTasks.FirstOrDefault();
        DemoDataStore.EditingTask = editing;
        LoadTask();
        BuildPage();
    }

    private void LoadTask()
    {
        var task = editing;
        taskTitle.Text = task?.Title ?? "";
        category.Items.Add("Select a category");
        category.Items.AddRange(DemoDataStore.ActiveCategoryNames());
        category.SelectedItem = task?.Category;
        if (category.SelectedIndex < 0) category.SelectedIndex = 0;
        taskType.Items.AddRange(["One-time task", "Short-term / recurring"]);
        taskType.SelectedIndex = Math.Max(0, taskType.Items.IndexOf(task?.TaskType ?? ""));
        taskStatus.Items.AddRange(["Active — accepting requests", "Paused — no new requests"]);
        taskStatus.SelectedIndex = task is not null && DemoDataStore.PausedTasks.Contains(task) ? 1 : 0;
        description.Text = task?.Description ?? "";
        requirements.Text = (task?.Requirements ?? "").Replace("\r\n", "\n").Replace("\n", "\r\n");
        amount.Text = task?.Price.ToString() ?? "";
        paymentMethod.Items.AddRange(TaskFormHelpers.PaymentMethods);
        paymentMethod.SelectedIndex = Math.Max(0, paymentMethod.Items.IndexOf(task?.PaymentMethod ?? ""));
        TaskFormHelpers.ConfigureStartTime(startTime, task?.StartTime);
        var due = task is not null && DateTime.TryParseExact(task.Deadline, "MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed)
            ? parsed
            : DateTime.Today.AddDays(1);
        deadline.MinDate = due < DateTime.Today ? due : DateTime.Today;
        deadline.Value = due;
        deadline.Format = DateTimePickerFormat.Short;
        startTime.Format = DateTimePickerFormat.Time;
        startTime.ShowUpDown = true;
        startTime.Value = DateTime.Today.AddHours(9);
        duration.Items.AddRange(["Less than 1 hour", "1–2 hours", "2–3 hours", "Half day", "Full day", "Multiple days"]);
        duration.SelectedIndex = Math.Max(0, duration.Items.IndexOf(task?.Duration ?? ""));
        barangay.Items.Add("Select barangay");
        barangay.Items.AddRange(DemoDataStore.ActiveBarangayNames());
        barangay.SelectedItem = task is null ? null : $"Brgy. {task.Barangay}";
        if (barangay.SelectedIndex < 0) barangay.SelectedIndex = 0;
        scheduleNotes.Text = task?.ScheduleNotes ?? "";
        pinLat = task?.Latitude;
        pinLng = task?.Longitude;
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
        foreach (Control control in new Control[] { taskTitle, category, taskType, taskStatus, description, requirements, amount, paymentMethod, deadline, startTime, duration, barangay, scheduleNotes })
        {
            control.TextChanged += (_, _) => UpdatePreview();
            if (control is ComboBox combo)
                combo.SelectedIndexChanged += (_, _) => UpdatePreview();
            if (control is DateTimePicker picker)
                picker.ValueChanged += (_, _) => UpdatePreview();
        }
        pageBuilt = true;
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
        AddPageLabel("editTaskEyebrow", "MANAGE YOUR TASK", 9, Blue, true);
        AddPageLabel("editTaskHeading", "Edit Task", 26, Navy, true);
        AddPageLabel("editTaskSubtitle", "Update the details, budget, schedule, or visibility of a task you posted.", 10, Muted);
        var cancel = MakeAction("Cancel", "EditTaskCancelButton", false, 82);
        cancel.Click += (_, _) => Nav.Back();
        canvas.Controls.Add(cancel);
        var archiveTop = MakeAction("Archive Task", "EditTaskArchiveTopButton", false, 105);
        archiveTop.ForeColor = Color.FromArgb(166, 77, 57);
        archiveTop.Click += (_, _) => ArchiveTask();
        canvas.Controls.Add(archiveTop);

        stepper.Name = "editTaskStepper";
        stepper.BackColor = Color.White;
        stepper.BorderStyle = BorderStyle.FixedSingle;
        var steps = new[] { ("Basic Info", "What do you need?"), ("Details", "Describe the task"), ("Compensation", "Set the budget"), ("Schedule", "When and where") };
        for (var index = 0; index < steps.Length; index++)
        {
            var target = index + 1;
            var stepButton = new Button { Name = $"EditTaskStep{target}Button", Text = $"{target}   {steps[index].Item1}\r\n     {steps[index].Item2}", TextAlign = ContentAlignment.MiddleLeft, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Muted, Font = new Font("Segoe UI", 8F), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            stepButton.FlatAppearance.BorderSize = 0;
            stepButton.Click += (_, _) => { if (target <= currentStep) ShowStep(target); else AdvanceTo(target); };
            stepButtons[index] = stepButton;
            stepper.Controls.Add(stepButton);
        }
        canvas.Controls.Add(stepper);

        editCard.Name = "editTaskCard";
        editCard.BackColor = Color.White;
        editCard.BorderStyle = BorderStyle.FixedSingle;
        canvas.Controls.Add(editCard);
        BuildStepPanels();
        BuildPreview();
        BuildActions();
    }

    private void BuildStepPanels()
    {
        var headings = new[] { "Update the basics", "Update task details", "Update compensation", "Update schedule and area" };
        var details = new[]
        {
            "Keep the title and category accurate so helpers know what the task is about.",
            "Clarify what needs to be done and add any requirements.",
            "Change the agreed amount or payment method if the arrangement has changed.",
            "Keep the deadline and broad task area current for everyone involved."
        };
        for (var index = 0; index < stepPanels.Length; index++)
        {
            var panel = new Panel { Name = $"editTaskStepPanel{index + 1}", BackColor = Color.White };
            var kicker = MakeLabel($"STEP {index + 1} OF 4", 8, Blue, true);
            kicker.Name = "stepKicker";
            kicker.Location = new Point(18, 14);
            panel.Controls.Add(kicker);
            var title = MakeLabel(headings[index], 18, Navy, true);
            title.Name = "stepTitle";
            title.Location = new Point(18, 34);
            panel.Controls.Add(title);
            var subtitle = MakeLabel(details[index], 8, Muted);
            subtitle.Name = "stepSubtitle";
            subtitle.Location = new Point(18, 61);
            panel.Controls.Add(subtitle);
            stepTitles[index] = title;
            stepSubtitles[index] = subtitle;
            stepPanels[index] = panel;
            editCard.Controls.Add(panel);
        }
        BuildBasicFields(stepPanels[0]);
        BuildDetailFields(stepPanels[1]);
        BuildCompensationFields(stepPanels[2]);
        BuildScheduleFields(stepPanels[3]);
    }

    private void BuildBasicFields(Control panel)
    {
        AddFieldLabel(panel, "Task title *", 18, 96);
        taskTitle.Name = "editTaskTitle";
        taskTitle.MaxLength = 80;
        taskTitle.Location = new Point(18, 118);
        taskTitle.Size = new Size(650, 34);
        panel.Controls.Add(taskTitle);
        AddHint(panel, "Keep it short and specific.", 18, 155);
        AddFieldLabel(panel, "Category *", 18, 180);
        ConfigureCombo(panel, category, "editTaskCategory", 18, 203, ["Select a category", "Cleaning", "Tutoring", "Repairs", "Shopping", "Pet Care", "Delivery", "Others"]);
        AddFieldLabel(panel, "Task type", 352, 180);
        ConfigureCombo(panel, taskType, "editTaskType", 352, 203, ["One-time task", "Short-term / recurring"]);
        AddFieldLabel(panel, "Task status", 18, 250);
        taskStatus.Name = "editTaskStatus";
        taskStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        taskStatus.Location = new Point(18, 273);
        taskStatus.Size = new Size(310, 32);
        panel.Controls.Add(taskStatus);
        AddHint(panel, "Pause new requests without archiving the task.", 18, 307);
        AddTip(panel, "Good task titles get better requests. Include the action and item involved.", 18, 336, false);
    }

    private void BuildDetailFields(Control panel)
    {
        AddFieldLabel(panel, "Description *", 18, 96);
        description.Name = "editTaskDescription";
        description.Multiline = true;
        description.MaxLength = 1000;
        description.ScrollBars = ScrollBars.Vertical;
        description.Location = new Point(18, 118);
        description.Size = new Size(650, 130);
        panel.Controls.Add(description);
        AddHint(panel, "Description characters: 0 / 1000", 18, 251);
        AddFieldLabel(panel, "Requirements (one per line)", 18, 280);
        requirements.Name = "editTaskRequirements";
        requirements.Multiline = true;
        requirements.ScrollBars = ScrollBars.Vertical;
        requirements.Location = new Point(18, 303);
        requirements.Size = new Size(650, 100);
        panel.Controls.Add(requirements);
        var tip = MakeLabel("Do not include passwords, OTPs, bank details, or other sensitive information.", 8, Muted);
        tip.Location = new Point(18, 412);
        panel.Controls.Add(tip);
    }

    private void BuildCompensationFields(Control panel)
    {
        AddFieldLabel(panel, "Compensation amount *", 18, 96);
        amount.Name = "editTaskAmount";
        amount.Location = new Point(18, 118);
        amount.Size = new Size(310, 34);
        amount.MaxLength = 6;
        amount.TextChanged += (_, _) =>
        {
            if (amount.Text.Any(character => !char.IsDigit(character)))
            {
                var caret = amount.SelectionStart;
                amount.Text = new string(amount.Text.Where(char.IsDigit).ToArray());
                amount.SelectionStart = Math.Min(caret, amount.TextLength);
            }
        };
        panel.Controls.Add(amount);
        AddHint(panel, "Enter an amount from ₱1 to ₱999,999.", 18, 155);
        AddFieldLabel(panel, "Suggested amounts", 18, 183);
        var suggested = new[] { "₱300", "₱500", "₱600", "₱1,000" };
        for (var index = 0; index < suggested.Length; index++)
        {
            var value = suggested[index].Replace("₱", string.Empty).Replace(",", string.Empty);
            var button = MakeAction(suggested[index], $"EditSuggestedAmount{value}", false, 70);
            button.Location = new Point(18 + index * 78, 206);
            button.Click += (_, _) => amount.Text = value;
            panel.Controls.Add(button);
        }
        AddFieldLabel(panel, "Payment method *", 18, 260);
        ConfigureCombo(panel, paymentMethod, "editTaskPayment", 18, 283, TaskFormHelpers.PaymentMethods);
        AddTip(panel, "TaskTap does not process payments. Payment is arranged directly between members.", 18, 340, true);
    }

    private void BuildScheduleFields(Control panel)
    {
        AddFieldLabel(panel, "Deadline *", 18, 96);
        deadline.Name = "editTaskDeadline";
        deadline.Format = DateTimePickerFormat.Short;
        deadline.MinDate = DateTime.Today;
        deadline.Location = new Point(18, 118);
        deadline.Size = new Size(310, 32);
        panel.Controls.Add(deadline);
        AddFieldLabel(panel, "Preferred start time (optional)", 352, 96);
        startTime.Name = "editTaskStartTime";
        startTime.Location = new Point(352, 118);
        startTime.Size = new Size(310, 32);
        panel.Controls.Add(startTime);
        AddFieldLabel(panel, "Estimated duration", 18, 168);
        duration.Name = "editTaskDuration";
        ConfigureCombo(panel, duration, "editTaskDuration", 18, 191, ["Less than 1 hour", "1–2 hours", "2–3 hours", "Half day", "Full day", "Multiple days"]);
        AddFieldLabel(panel, "Barangay (from your profile)", 352, 168);
        barangay.Name = "editTaskBarangay";
        ConfigureCombo(panel, barangay, "editTaskBarangay", 352, 191, ["Select barangay"]);
        TaskFormHelpers.LockBarangay(barangay, editing is { Barangay.Length: > 0 } ? $"Brgy. {editing.Barangay}" : AppSession.CurrentUser.Barangay);
        AddFieldLabel(panel, "Additional schedule notes", 18, 239);
        scheduleNotes.Name = "editTaskScheduleNotes";
        scheduleNotes.Multiline = true;
        scheduleNotes.MaxLength = 500;
        scheduleNotes.Location = new Point(18, 262);
        scheduleNotes.Size = new Size(644, 38);
        panel.Controls.Add(scheduleNotes);
        BuildPinControls(panel, 308);
        AddTip(panel, "Task area is approximate. Avoid posting your exact home address.", 18, 350, false);
        var review = MakeLabel("Review  •  Task, budget, deadline, and area are shown in the live preview.", 8, Muted);
        review.Location = new Point(18, 410);
        panel.Controls.Add(review);
    }

    private void BuildPreview()
    {
        previewCard.Name = "editTaskPreviewCard";
        previewCard.BackColor = Color.White;
        previewCard.BorderStyle = BorderStyle.FixedSingle;
        var kicker = MakeLabel("LIVE PREVIEW", 8, Blue, true);
        kicker.Location = new Point(14, 13);
        previewCard.Controls.Add(kicker);
        var heading = MakeLabel("Task details", 10, Navy, true);
        heading.Location = new Point(14, 31);
        previewCard.Controls.Add(heading);
        statusPreview.Name = "editTaskPreviewStatus";
        statusPreview.Location = new Point(14, 51);
        statusPreview.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        previewCard.Controls.Add(statusPreview);
        titlePreview.Name = "editTaskPreviewTitle";
        titlePreview.Location = new Point(14, 75);
        titlePreview.MaximumSize = new Size(220, 42);
        titlePreview.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        titlePreview.ForeColor = Navy;
        previewCard.Controls.Add(titlePreview);
        pricePreview.Name = "editTaskPreviewPrice";
        pricePreview.Location = new Point(14, 116);
        pricePreview.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        pricePreview.ForeColor = Navy;
        previewCard.Controls.Add(pricePreview);
        categoryPreview.Name = "editTaskPreviewCategory";
        areaPreview.Name = "editTaskPreviewArea";
        datePreview.Name = "editTaskPreviewDate";
        AddPreviewPill(categoryPreview, "Category", 14);
        AddPreviewPill(areaPreview, "Barangay", 88);
        AddPreviewPill(datePreview, "Deadline", 162);
        descriptionPreview.Location = new Point(14, 174);
        descriptionPreview.Name = "editTaskPreviewDescription";
        descriptionPreview.MaximumSize = new Size(220, 88);
        descriptionPreview.Font = new Font("Segoe UI", 7.5F);
        descriptionPreview.ForeColor = Muted;
        previewCard.Controls.Add(descriptionPreview);
        canvas.Controls.Add(previewCard);

        helpCard.Name = "editTaskHelpCard";
        helpCard.BackColor = Color.FromArgb(238, 247, 255);
        var helpTitle = MakeLabel("Need a good task post?", 8, Navy, true);
        helpTitle.Location = new Point(12, 12);
        helpCard.Controls.Add(helpTitle);
        var helpBody = MakeLabel("Use a specific title, explain the result, set a clear budget, and choose a broad task area.", 7.5F, Muted);
        helpBody.Location = new Point(12, 31);
        helpBody.MaximumSize = new Size(250, 44);
        helpCard.Controls.Add(helpBody);
        var tips = MakeTextButton("View posting tips →", "EditTaskPostingTipsButton");
        tips.Location = new Point(12, 76);
        tips.Click += (_, _) => Nav.Navigate(Route.HelpFaq);
        helpCard.Controls.Add(tips);
        canvas.Controls.Add(helpCard);

        controlsCard.Name = "editTaskControlsCard";
        controlsCard.BackColor = Color.White;
        controlsCard.BorderStyle = BorderStyle.FixedSingle;
        var controlsHeading = MakeLabel("Task controls", 9, Navy, true);
        controlsHeading.Location = new Point(12, 10);
        controlsCard.Controls.Add(controlsHeading);
        pauseButton = MakeAction("Ⅱ  Pause requests", "EditTaskPauseButton", false, 230);
        pauseButton.TextAlign = ContentAlignment.MiddleLeft;
        pauseButton.Location = new Point(12, 34);
        pauseButton.Click += (_, _) => TogglePaused();
        controlsCard.Controls.Add(pauseButton);
        archiveButton = MakeAction("□  Archive task", "EditTaskArchiveButton", false, 230);
        archiveButton.TextAlign = ContentAlignment.MiddleLeft;
        archiveButton.ForeColor = Color.FromArgb(166, 77, 57);
        archiveButton.Location = new Point(12, 76);
        archiveButton.Click += (_, _) => ArchiveTask();
        controlsCard.Controls.Add(archiveButton);
        canvas.Controls.Add(controlsCard);
    }

    private void BuildActions()
    {
        backButton = MakeAction("← Back", "EditTaskBackButton", false, 88);
        backButton.Click += (_, _) => { if (currentStep > 1) ShowStep(currentStep - 1); };
        canvas.Controls.Add(backButton);
        var saveDraft = MakeTextButton("Save changes", "EditTaskSaveDraftButton");
        saveDraft.Click += (_, _) => SaveChanges();
        canvas.Controls.Add(saveDraft);
        nextButton = MakeAction("Continue →", "EditTaskContinueButton", true, 104);
        nextButton.Click += (_, _) => AdvanceTo(currentStep + 1);
        canvas.Controls.Add(nextButton);
        saveButton = MakeAction("Save Changes", "EditTaskSaveButton", true, 112);
        saveButton.Click += (_, _) => SaveChanges();
        canvas.Controls.Add(saveButton);
        validation.Name = "editTaskValidation";
        validation.AutoSize = false;
        validation.ForeColor = Color.FromArgb(185, 55, 67);
        validation.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        validation.Visible = false;
        canvas.Controls.Add(validation);
    }

    private void LayoutPage()
    {
        if (!pageBuilt || scrollHost.ClientSize.Width <= 0)
            return;
        var width = Math.Max(850, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 32);
        canvas.Size = new Size(width, 850);
        canvas.Location = new Point(Math.Max(0, (scrollHost.ClientSize.Width - width) / 2), 0);
        const int left = 22;
        Position("editTaskEyebrow", left, 18);
        Position("editTaskHeading", left, 37);
        Position("editTaskSubtitle", left, 86);
        var cancel = Find<Button>("EditTaskCancelButton");
        var archiveTop = Find<Button>("EditTaskArchiveTopButton");
        if (cancel is not null) cancel.Location = new Point(width - 22 - cancel.Width - (archiveTop?.Width ?? 0) - 10, 42);
        if (archiveTop is not null) archiveTop.Location = new Point(width - 22 - archiveTop.Width, 42);
        stepper.Bounds = new Rectangle(left, 104, width - 44, 64);
        var stepWidth = (stepper.Width - 24) / 4;
        for (var index = 0; index < stepButtons.Length; index++)
            stepButtons[index].Bounds = new Rectangle(6 + index * stepWidth, 7, stepWidth, 48);
        var contentTop = 182;
        var previewWidth = 254;
        var gap = 14;
        var editorWidth = width - 44 - previewWidth - gap;
        editCard.Bounds = new Rectangle(left, contentTop, editorWidth, 450);
        foreach (var panel in stepPanels)
            panel.Bounds = new Rectangle(0, 0, editorWidth, 450);
        ResizeStepControls(editorWidth);
        previewCard.Bounds = new Rectangle(left + editorWidth + gap, contentTop, previewWidth, 292);
        helpCard.Bounds = new Rectangle(left + editorWidth + gap, contentTop + 306, previewWidth, 104);
        controlsCard.Bounds = new Rectangle(left + editorWidth + gap, contentTop + 424, previewWidth, 132);
        foreach (Control child in controlsCard.Controls.OfType<Button>())
            child.Width = previewWidth - 24;
        backButton.Location = new Point(left, contentTop + 468);
        var saveDraft = Find<Button>("EditTaskSaveDraftButton");
        if (saveDraft is not null) saveDraft.Location = new Point(left + editorWidth - 294, contentTop + 475);
        nextButton.Location = new Point(left + editorWidth - 190, contentTop + 468);
        saveButton.Location = new Point(left + editorWidth - saveButton.Width - 4, contentTop + 468);
        validation.Location = new Point(left + 96, contentTop + 474);
        validation.Size = new Size(Math.Max(100, editorWidth - 400), 24);
        canvas.Height = contentTop + 530;
        ShowStep(currentStep);
    }

    private void ResizeStepControls(int width)
    {
        var halfWidth = (width - 54) / 2;
        taskTitle.Width = width - 36;
        description.Width = width - 36;
        requirements.Width = width - 36;
        scheduleNotes.Width = width - 36;
        foreach (var combo in new[] { category, taskType, taskStatus, paymentMethod, duration, barangay })
            combo.Width = halfWidth;
        taskType.Left = halfWidth + 18;
        barangay.Left = halfWidth + 18;
    }

    private void ShowStep(int step)
    {
        currentStep = Math.Clamp(step, 1, 4);
        for (var index = 0; index < 4; index++)
        {
            stepPanels[index].Visible = index + 1 == currentStep;
            stepButtons[index].ForeColor = index + 1 <= currentStep ? Blue : Muted;
            stepButtons[index].BackColor = index + 1 == currentStep ? Color.FromArgb(239, 247, 255) : Color.White;
            stepButtons[index].Font = new Font("Segoe UI", 8F, index + 1 == currentStep ? FontStyle.Bold : FontStyle.Regular);
        }
        backButton.Visible = currentStep > 1;
        nextButton.Visible = currentStep < 4;
        saveButton.Visible = currentStep == 4;
        validation.Visible = false;
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
        if (valid)
            return true;
        validation.Text = step switch
        {
            1 when string.IsNullOrWhiteSpace(taskTitle.Text) => "Please add a task title.",
            1 => "Choose a category.",
            2 => "Please describe what needs to be done.",
            3 when !int.TryParse(amount.Text, out var value) || value is < 1 or > 999999 => "Enter an amount from ₱1 to ₱999,999.",
            3 => "Choose a payment method.",
            4 when barangay.SelectedIndex == 0 => "Choose the task area.",
            _ => "Choose a valid deadline."
        };
        validation.Visible = true;
        ShowToast("Please complete the required fields before continuing.");
        return false;
    }

    private void UpdatePreview()
    {
        if (updating || titlePreview.IsDisposed)
            return;
        updating = true;
        titlePreview.Text = string.IsNullOrWhiteSpace(taskTitle.Text) ? "Your task title" : taskTitle.Text.Trim();
        pricePreview.Text = $"₱{(int.TryParse(amount.Text, out var value) ? value : 0):N0}";
        categoryPreview.Text = category.SelectedIndex > 0 ? category.Text : "Category";
        areaPreview.Text = barangay.SelectedIndex > 0 ? barangay.Text : "Barangay";
        datePreview.Text = deadline.Value.ToString("MMM d, yyyy");
        descriptionPreview.Text = string.IsNullOrWhiteSpace(description.Text) ? "Your task description will appear here as you edit it." : description.Text.Trim();
        var paused = taskStatus.SelectedIndex == 1;
        statusPreview.Text = archived ? "Archived" : paused ? "Paused" : "Published";
        statusPreview.ForeColor = archived ? Color.FromArgb(166, 77, 57) : paused ? Color.FromArgb(192, 119, 46) : Color.FromArgb(35, 153, 111);
        pauseButton.Text = paused ? "▶  Resume requests" : "Ⅱ  Pause requests";
        updating = false;
    }

    private void TogglePaused()
    {
        taskStatus.SelectedIndex = taskStatus.SelectedIndex == 1 ? 0 : 1;
        UpdatePreview();
        ShowToast(taskStatus.SelectedIndex == 1 ? "New requests will be paused when you save." : "Task will accept requests when you save.");
    }

    private void ArchiveTask()
    {
        var result = MessageBox.Show("Archiving removes this task from active listings and stops new requests. Continue?", "Archive task?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
            return;
        if (editing is not null)
            DemoDataStore.ArchiveTask(editing);
        archived = true;
        taskStatus.SelectedIndex = 1;
        UpdatePreview();
        ShowToast("Task archived and moved to Archived Tasks.");
        Nav.Navigate(Route.MyTasks);
    }

    private void SaveChanges()
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
        if (editing is null)
        {
            ShowToast("There is no task selected to edit.");
            return;
        }

        var result = MessageBox.Show($"Save these changes?\r\n\r\nTask: {taskTitle.Text.Trim()}\r\nBudget: ₱{int.Parse(amount.Text):N0}\r\nArea: {barangay.Text}", "Review changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        if (result != DialogResult.OK)
            return;

        var updated = editing with
        {
            Title = taskTitle.Text.Trim(),
            Category = category.SelectedItem?.ToString() ?? editing.Category,
            Price = int.Parse(amount.Text),
            Barangay = barangay.Text.Replace("Brgy. ", "").Trim(),
            Deadline = deadline.Value.ToString("MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture),
            Days = Math.Max(0, (deadline.Value.Date - DateTime.Today).Days),
            Description = description.Text.Trim(),
            Requirements = requirements.Text.Trim(),
            TaskType = taskType.SelectedItem?.ToString() ?? "",
            PaymentMethod = paymentMethod.SelectedItem?.ToString() ?? "",
            StartTime = TaskFormHelpers.StartTimeText(startTime),
            Duration = duration.SelectedItem?.ToString() ?? "",
            ScheduleNotes = scheduleNotes.Text.Trim(),
            Latitude = pinLat,
            Longitude = pinLng
        };
        DemoDataStore.UpdatePostedTask(editing, updated, taskStatus.SelectedIndex == 1);
        DemoDataStore.EditingTask = updated;
        editing = updated;
        ShowToast("Changes saved.");
        Nav.Navigate(Route.MyTasks);
    }

    private void AddPageLabel(string name, string text, float size, Color color, bool bold = false)
    {
        var label = MakeLabel(text, size, color, bold);
        label.Name = name;
        canvas.Controls.Add(label);
    }

    private void AddFieldLabel(Control parent, string text, int x, int y)
    {
        var label = MakeLabel(text, 8, Navy, true);
        label.Location = new Point(x, y);
        parent.Controls.Add(label);
    }

    private void AddHint(Control parent, string text, int x, int y)
    {
        var label = MakeLabel(text, 7, Muted);
        label.Location = new Point(x, y);
        parent.Controls.Add(label);
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

    private void AddTip(Control parent, string text, int x, int y, bool orange)
    {
        var box = new Panel { Location = new Point(x, y), Size = new Size(650, 44), BackColor = orange ? Color.FromArgb(255, 247, 235) : Color.FromArgb(238, 247, 255) };
        var symbol = MakeLabel(orange ? "₱" : "i", 9, orange ? Color.FromArgb(224, 132, 52) : Blue, true);
        symbol.Location = new Point(12, 12);
        box.Controls.Add(symbol);
        var textLabel = MakeLabel(text, 7.5F, Muted);
        textLabel.Location = new Point(32, 12);
        textLabel.MaximumSize = new Size(600, 30);
        box.Controls.Add(textLabel);
        parent.Controls.Add(box);
    }

    private void AddPreviewPill(Label label, string fallback, int x)
    {
        label.Text = fallback;
        label.Location = new Point(x, 148);
        label.Size = new Size(68, 19);
        label.AutoSize = false;
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.BackColor = Page;
        label.ForeColor = Muted;
        label.Font = new Font("Segoe UI", 6.5F);
        previewCard.Controls.Add(label);
    }

    private void ConfigureCombo(Control parent, ComboBox combo, string name, int x, int y, string[] items)
    {
        combo.Name = name;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        if (combo.Items.Count == 0)
            combo.Items.AddRange(items);
        if (combo.SelectedIndex < 0)
            combo.SelectedIndex = 0;
        combo.Location = new Point(x, y);
        combo.Size = new Size(310, 32);
        parent.Controls.Add(combo);
    }

    private void ShowToast(string text)
    {
        toast.Text = text;
        toast.Visible = true;
        toast.BringToFront();
        toastTimer.Stop();
        toastTimer.Start();
    }

    private void Position(string name, int x, int y)
    {
        var control = canvas.Controls.Find(name, false).FirstOrDefault();
        if (control is not null)
            control.Location = new Point(x, y);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            toastTimer.Dispose();
        base.Dispose(disposing);
    }
}
