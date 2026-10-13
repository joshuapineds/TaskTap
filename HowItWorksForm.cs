namespace TaskTap
{
    public sealed class HowItWorksForm : PublicInfoFormBase
    {
        private static readonly string[] StepTitles = ["Find a Task", "Request to Perform", "Complete the Task", "Build Your Reputation"];
        private static readonly string[] StepDescriptions =
        [
            "Browse available tasks and filter by category, location, compensation, and deadline.",
            "Review the details and send a request when the task matches your skills and availability.",
            "Coordinate with the task poster, complete the agreed work, and keep the task status updated.",
            "Record completed tasks and grow trust through your activity and community reviews."
        ];

        public HowItWorksForm() : base("How It Works")
        {
            BuildPage();
        }

        private void BuildPage()
        {
            var hero = new Panel { BackColor = Color.FromArgb(244, 250, 255) };
            AddSection(hero, 280, 0);
            var eyebrow = MakeLabel("HOW TASKTAP WORKS", 9, BlueColor, true);
            eyebrow.Location = new Point((hero.Width - eyebrow.PreferredWidth) / 2, 54);
            hero.Controls.Add(eyebrow);
            var title = MakeLabel("Small tasks.", 34, NavyColor, true);
            title.Location = new Point(235, 82);
            hero.Controls.Add(title);
            var highlight = MakeLabel("Real connections.", 34, Color.FromArgb(255, 125, 51), true);
            highlight.Location = new Point(455, 82);
            hero.Controls.Add(highlight);
            var description = MakeLabel("TaskTap makes it easier for people in the same community to find help, offer their skills, and get everyday tasks done.", 11, MutedColor);
            description.MaximumSize = new Size(650, 45);
            description.Location = new Point(168, 139);
            hero.Controls.Add(description);
            var browse = MakeAction("Browse Tasks", "HowBrowseTasksButton", true);
            browse.Location = new Point(365, 202);
            browse.Click += (_, _) => NavigateTo("Browse Tasks");
            hero.Controls.Add(browse);
            var join = MakeAction("Join TaskTap", "HowJoinButton", false);
            join.Location = new Point(530, 202);
            join.Click += (_, _) => NavigateTo("Sign Up");
            hero.Controls.Add(join);

            var steps = new Panel { BackColor = Color.White };
            AddSection(steps, 315, 296);
            AddCentered(steps, "FOR TASK DOERS", 0, 8, 9, BlueColor, true);
            AddCentered(steps, "Find a task. Help someone nearby. Get rewarded.", 0, 34, 22, NavyColor, true);
            AddCentered(steps, "A simple flow from discovery to completion.", 0, 67, 10, MutedColor);
            for (var index = 0; index < StepTitles.Length; index++)
                BuildStepCard(steps, index, 5 + index * 254);

            var callout = new Panel { BackColor = NavyColor };
            AddSection(callout, 190, 622);
            var poster = MakeLabel("POSTERS", 9, Color.FromArgb(93, 174, 255), true);
            poster.Location = new Point(36, 32);
            callout.Controls.Add(poster);
            var need = MakeLabel("Need something done?", 23, Color.White, true);
            need.Location = new Point(36, 55);
            callout.Controls.Add(need);
            var explanation = MakeLabel("Post a clear task with the details people need: what needs to be done, where the task is located, compensation, and deadline.", 10, Color.FromArgb(199, 216, 237));
            explanation.MaximumSize = new Size(470, 54);
            explanation.Location = new Point(36, 89);
            callout.Controls.Add(explanation);
            var create = MakeLink("Create an account →", "HowCreateAccountButton");
            create.ForeColor = Color.FromArgb(110, 183, 255);
            create.Location = new Point(36, 145);
            create.Click += (_, _) => NavigateTo("Sign Up");
            callout.Controls.Add(create);
            var checklist = MakeLabel("✓ Describe the task clearly\n\n✓ Set a fair compensation\n\n✓ Choose a deadline\n\n✓ Review requests from local task-doers", 10, Color.White);
            checklist.Location = new Point(585, 32);
            checklist.MaximumSize = new Size(400, 145);
            callout.Controls.Add(checklist);

            var trust = new Panel { BackColor = Color.White };
            AddSection(trust, 220, 827);
            AddCentered(trust, "KEEP IT COMMUNITY-FIRST", 0, 15, 9, BlueColor, true);
            AddCentered(trust, "Designed around clarity and trust", 0, 42, 22, NavyColor, true);
            var trustItems = new[]
            {
                ("Privacy-aware locations", "Public task views use approximate areas rather than exposing exact addresses."),
                ("Transparent task details", "See the important basics before deciding whether to request a task."),
                ("Reputation matters", "Completed work and community feedback help people make informed choices.")
            };
            for (var index = 0; index < trustItems.Length; index++)
            {
                var card = new Panel { Location = new Point(index * 350, 96), Size = new Size(340, 100), BackColor = Color.White };
                card.Paint += (_, e) =>
                {
                    using var pen = new Pen(LineColor);
                    e.Graphics.DrawLine(pen, 0, 0, card.Width, 0);
                };
                var heading = MakeLabel(trustItems[index].Item1, 10, NavyColor, true);
                heading.Location = new Point(10, 18);
                var body = MakeLabel(trustItems[index].Item2, 9, MutedColor);
                body.Location = new Point(10, 45);
                body.MaximumSize = new Size(315, 44);
                card.Controls.Add(heading);
                card.Controls.Add(body);
                trust.Controls.Add(card);
            }
            var footer = new Panel { BackColor = Color.FromArgb(6, 31, 67) };
            AddSection(footer, 55, 1057);
            var brand = MakeLabel("TaskTap", 13, Color.White, true);
            brand.Location = new Point(12, 17);
            footer.Controls.Add(brand);
            var copyright = MakeLabel("© 2026 TaskTap  ·  About", 8, Color.FromArgb(168, 197, 229));
            copyright.Location = new Point(870, 21);
            footer.Controls.Add(copyright);
            var aboutLink = MakeLink("About", "HowFooterAboutButton");
            aboutLink.Location = new Point(1035, 12);
            aboutLink.ForeColor = Color.FromArgb(168, 197, 229);
            aboutLink.BackColor = footer.BackColor;
            aboutLink.Click += (_, _) => NavigateTo("About");
            footer.Controls.Add(aboutLink);
        }

        private void BuildStepCard(Control parent, int index, int left)
        {
            var card = new Panel
            {
                Name = $"HowStepCard{index + 1}",
                Location = new Point(left, 0),
                Size = new Size(240, 190),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var icon = MakeLabel(index switch { 0 => "⌕", 1 => "↗", 2 => "✓", _ => "₱" }, 19, BlueColor, true);
            icon.TextAlign = ContentAlignment.MiddleCenter;
            icon.BackColor = Color.FromArgb(232, 243, 255);
            icon.Location = new Point(18, 18);
            icon.Size = new Size(42, 38);
            var number = MakeLabel($"0{index + 1}", 8, MutedColor, true);
            number.Location = new Point(202, 17);
            var heading = MakeLabel(StepTitles[index], 11, NavyColor, true);
            heading.Location = new Point(18, 77);
            var description = MakeLabel(StepDescriptions[index], 9, MutedColor);
            description.MaximumSize = new Size(202, 72);
            description.Location = new Point(18, 103);
            card.Controls.Add(icon);
            card.Controls.Add(number);
            card.Controls.Add(heading);
            card.Controls.Add(description);
            parent.Controls.Add(card);
        }

        private static void AddCentered(Control parent, string text, int x, int y, float size, Color color, bool bold = false)
        {
            var label = MakeLabel(text, size, color, bold);
            label.Location = new Point((parent.Width - label.PreferredWidth) / 2 + x, y);
            label.Anchor = AnchorStyles.Top;
            parent.Controls.Add(label);
        }
    }
}
