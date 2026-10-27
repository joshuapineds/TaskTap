namespace TaskTap
{
    public sealed class AboutForm : PublicInfoFormBase
    {
        public AboutForm() : base("About")
        {
            BuildPage();
        }

        private void BuildPage()
        {
            var hero = new Panel { BackColor = Color.FromArgb(244, 250, 255) };
            AddSection(hero, 360, 0);
            var eyebrow = MakeLabel("ABOUT TASKTAP", 9, BlueColor, true);
            eyebrow.Location = new Point(45, 82);
            hero.Controls.Add(eyebrow);
            var titleLineOne = MakeLabel("Helping communities", 31, NavyColor, true);
            titleLineOne.Location = new Point(45, 116);
            hero.Controls.Add(titleLineOne);
            var titleLineTwo = MakeLabel("help each other.", 31, OrangeColor, true);
            titleLineTwo.Location = new Point(45, 160);
            hero.Controls.Add(titleLineTwo);
            var explanation = MakeLabel("TaskTap is a local task marketplace designed to make everyday help easier to find while creating opportunities for people to earn from their time and skills.", 11, MutedColor);
            explanation.Location = new Point(47, 212);
            explanation.MaximumSize = new Size(450, 62);
            hero.Controls.Add(explanation);
            var explore = MakeAction("Explore Local Tasks", "AboutExploreTasksButton", true);
            explore.Size = new Size(175, 42);
            explore.Location = new Point(45, 295);
            explore.Click += (_, _) => NavigateTo("Browse Tasks");
            hero.Controls.Add(explore);
            BuildCommunityIllustration(hero);

            var mission = new Panel { BackColor = Color.White };
            AddSection(mission, 290, 376);
            AddCentered(mission, "OUR PURPOSE", 0, 18, 9, BlueColor, true);
            AddCentered(mission, "Make local help more accessible.", 0, 46, 22, NavyColor, true);
            var missionItems = new[]
            {
                ("01", "Connect nearby people", "Bring task posters and task-doers together around practical, everyday needs in their community."),
                ("02", "Make opportunities visible", "Give people a straightforward way to discover small paid tasks that match their skills and availability."),
                ("03", "Build stronger trust", "Use clear task information, completion records, and community reputation to make interactions easier to understand.")
            };
            for (var index = 0; index < missionItems.Length; index++)
                BuildInfoCard(mission, index, missionItems[index].Item1, missionItems[index].Item2, missionItems[index].Item3, index * 350, 330, 90);

            var values = new Panel { BackColor = Color.FromArgb(244, 250, 255) };
            AddSection(values, 245, 676);
            AddCentered(values, "WHAT WE VALUE", 0, 30, 9, BlueColor, true);
            AddCentered(values, "Simple by design. Community by nature.", 0, 58, 22, NavyColor, true);
            var valueItems = new[]
            {
                ("Local", "Start with the communities people already know and live in."),
                ("Clear", "Keep task expectations, compensation, and deadlines visible."),
                ("Responsible", "Respect privacy and encourage safer, transparent interactions."),
                ("Inclusive", "Make it easier for people to contribute skills in ways that fit their circumstances.")
            };
            for (var index = 0; index < valueItems.Length; index++)
                BuildInfoCard(values, index, string.Empty, valueItems[index].Item1, valueItems[index].Item2, index * 255, 250, 100);

            var cta = new Panel { BackColor = Color.White };
            AddSection(cta, 225, 932);
            AddCentered(cta, "Ready to find your next task?", 0, 46, 24, NavyColor, true);
            AddCentered(cta, "Explore available opportunities or create your TaskTap account.", 0, 86, 10, MutedColor);
            var browse = MakeAction("Browse Tasks", "AboutBrowseTasksButton", true);
            browse.Location = new Point(385, 128);
            browse.Click += (_, _) => NavigateTo("Browse Tasks");
            cta.Controls.Add(browse);
            var create = MakeAction("Create Account", "AboutCreateAccountButton", false);
            create.Location = new Point(550, 128);
            create.Click += (_, _) => NavigateTo("Sign Up");
            cta.Controls.Add(create);

            var footer = new Panel { BackColor = Color.FromArgb(6, 31, 67) };
            AddSection(footer, 55, 1167);
            var brand = MakeLabel("TaskTap", 13, Color.White, true);
            brand.Location = new Point(12, 17);
            footer.Controls.Add(brand);
            var copyright = MakeLabel("© 2026 TaskTap", 8, Color.FromArgb(168, 197, 229));
            copyright.Location = new Point(800, 21);
            footer.Controls.Add(copyright);
            var howLink = MakeLink("How It Works", "AboutFooterHowItWorksButton");
            howLink.Location = new Point(950, 12);
            howLink.ForeColor = Color.FromArgb(168, 197, 229);
            howLink.BackColor = footer.BackColor;
            howLink.Click += (_, _) => NavigateTo("How It Works");
            footer.Controls.Add(howLink);
        }

        private static void BuildCommunityIllustration(Control parent)
        {
            var illustration = new Panel
            {
                Name = "aboutCommunityIllustration",
                Location = new Point(620, 60),
                Size = new Size(420, 270),
                BackColor = Color.FromArgb(232, 244, 255)
            };
            illustration.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var orbitPen = new Pen(Color.FromArgb(183, 215, 244)) { DashPattern = [2, 4] };
                e.Graphics.DrawEllipse(orbitPen, 76, 24, 270, 220);
                e.Graphics.DrawEllipse(orbitPen, 120, 66, 182, 136);
            };
            var pin = new Label
            {
                Text = "●",
                Name = "aboutCommunityPin",
                Location = new Point(190, 106),
                Size = new Size(48, 48),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = BlueColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            illustration.Controls.Add(pin);
            AddPerson(illustration, "JD", 90, 67);
            AddPerson(illustration, "MS", 300, 68);
            AddPerson(illustration, "AL", 72, 177);
            AddPerson(illustration, "RC", 318, 182);
            parent.Controls.Add(illustration);
        }

        private static void AddPerson(Control parent, string initials, int x, int y)
        {
            var person = new Label
            {
                Text = initials,
                Location = new Point(x, y),
                Size = new Size(38, 38),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(52, 98, 141),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            person.Paint += (_, e) =>
            {
                using var pen = new Pen(LineColor);
                e.Graphics.DrawEllipse(pen, 0, 0, person.Width - 1, person.Height - 1);
            };
            parent.Controls.Add(person);
        }

        private static void BuildInfoCard(Control parent, int index, string number, string title, string description, int left, int width, int top)
        {
            var card = new Panel
            {
                Name = $"AboutInfoCard{index + 1}",
                Location = new Point(left, top),
                Size = new Size(width, 180),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            if (!string.IsNullOrEmpty(number))
            {
                var numberLabel = MakeLabel(number, 8, BlueColor, true);
                numberLabel.Location = new Point(16, 16);
                card.Controls.Add(numberLabel);
            }
            var heading = MakeLabel(title, 10, NavyColor, true);
            heading.Location = new Point(16, string.IsNullOrEmpty(number) ? 23 : 44);
            card.Controls.Add(heading);
            var body = MakeLabel(description, 9, MutedColor);
            body.Location = new Point(16, string.IsNullOrEmpty(number) ? 53 : 72);
            body.MaximumSize = new Size(width - 32, 76);
            card.Controls.Add(body);
            card.Height = parent is Panel panel && panel.BackColor == Color.FromArgb(244, 250, 255) ? 100 : 180;
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
