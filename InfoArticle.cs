namespace TaskTap;

/// <summary>One section of a static information article.</summary>
public sealed record InfoSection(string Heading, string Body);

/// <summary>A short static article rendered by the guest and signed-in info pages.</summary>
public sealed record InfoArticle(string Key, string Eyebrow, string Title, string Summary, InfoSection[] Sections);

/// <summary>
/// The Help / policy content shared by the guest form and the signed-in page.
/// Deliberately short — one screen each, 4–6 sections.
/// </summary>
public static class InfoArticleCatalog
{
    public static InfoArticle HelpFaq { get; } = new(
        "help",
        "SUPPORT",
        "Help & FAQ",
        "Quick answers about posting tasks, finding work, payments and reviews.",
        [
            new("How do I post a task?", "Open Post Task, describe the job, set a budget and deadline, choose a barangay, and publish. You can save a draft and finish it later."),
            new("How do I find work?", "Open Find Tasks, filter by category, barangay, price or deadline, then open a task and send a request with a short message."),
            new("How are payments handled?", "Payments are recorded manually from Payment History or after a completed task. Real transfers are out of scope."),
            new("How do reviews work?", "After a task is completed you can rate and review the other person. Reviews build the reputation shown on profiles."),
            new("What if something goes wrong?", "Report a task or profile from its detail page, or open a dispute from a completed task. Admins review reports and disputes."),
            new("Is this a real service?", "TaskTap connects neighbors for small local tasks. Members agree on the work and payment between themselves; TaskTap keeps the records.")
        ]);

    public static InfoArticle CommunityGuidelines { get; } = new(
        "guidelines",
        "COMMUNITY",
        "Community Guidelines",
        "The short version of how we keep TaskTap useful, fair and safe.",
        [
            new("Be clear about the task.", "Describe the work, the location, the budget and the deadline before people request it."),
            new("Respect people and their property.", "Arrive on time, follow instructions, and treat homes and belongings with care."),
            new("Keep communication on TaskTap.", "Use in-app messages for arrangements. Do not move to personal numbers before a task is confirmed."),
            new("Never share sensitive details.", "Do not post passwords, OTPs, ID numbers or payment credentials in tasks, messages or reviews."),
            new("Use honest reviews.", "Review the work, not the person. No harassment, discrimination or personal remarks."),
            new("Report, don't retaliate.", "If something goes wrong, report it or open a dispute instead of escalating in chat.")
        ]);

    public static InfoArticle Terms { get; } = new(
        "terms",
        "LEGAL",
        "Terms of Service",
        "The terms for using TaskTap.",
        [
            new("Use TaskTap responsibly.", "TaskTap helps neighbors find each other. Members are responsible for the tasks they post and accept, and for arranging payment directly."),
            new("Your account.", "You are responsible for the information you enter and for keeping your sign-in details private."),
            new("Task content.", "Only post tasks you are allowed to post, and do not use the app for illegal or harmful activity."),
            new("Payments.", "TaskTap does not move money. Payment records are notes that members keep for reference."),
            new("Suspension.", "Accounts that break these terms or the Community Guidelines can be suspended or removed."),
            new("Changes.", "These terms can change as TaskTap evolves.")
        ]);

    public static InfoArticle Privacy { get; } = new(
        "privacy",
        "LEGAL",
        "Privacy Policy",
        "How TaskTap handles the information you enter.",
        [
            new("What we collect.", "Your name, email, phone, barangay and the tasks, requests and messages you create. Passwords are stored as salted hashes."),
            new("How it is used.", "Only to render the pages you see: profiles, tasks, messages, payments and admin views."),
            new("Where it lives.", "TaskTap stores account, task, request, message, payment and review data in the configured MySQL database. Sign out on shared computers, and never share passwords, OTPs or payment credentials."),
            new("Approximate locations.", "Task maps show barangay-level areas, not exact addresses."),
            new("Your controls.", "Edit or clear your details in Account Settings. You can deactivate your account from Account Settings."),
            new("Contact.", "Raise questions through the Support page.")
        ]);

    public static IReadOnlyList<InfoArticle> All { get; } = [HelpFaq, CommunityGuidelines, Terms, Privacy];

    public static InfoArticle Get(string key) => key switch
    {
        "help" => HelpFaq,
        "guidelines" => CommunityGuidelines,
        "terms" => Terms,
        "privacy" => Privacy,
        _ => HelpFaq
    };

    public static Route RouteFor(string key) => key switch
    {
        "help" => Route.HelpFaq,
        "guidelines" => Route.CommunityGuidelines,
        "terms" => Route.Terms,
        "privacy" => Route.Privacy,
        _ => Route.HelpFaq
    };
}
