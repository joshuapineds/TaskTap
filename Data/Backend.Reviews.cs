namespace TaskTap.Data;

public static partial class Backend
{
    public sealed record ReviewRow(string OtherName, string OtherInitials, int Rating, DateTime Date, string Task, string Title, string Body);

    /// <summary>Saves one review from each participant for a completed task.</summary>
    public static (bool Success, string Error) SaveReview(int reviewerId, string revieweeName, string taskTitle, int rating, string title, string body)
    {
        try
        {
            if (rating is < 1 or > 5)
                return (false, "Choose a rating from 1 to 5 stars.");
            if (string.IsNullOrWhiteSpace(taskTitle))
                return (false, "The task title is required.");
            var reviewee = Db.Scalar("SELECT id FROM users WHERE CONCAT(first_name, ' ', last_name) = @n LIMIT 1", ("@n", revieweeName.Trim()));
            if (reviewee is null)
                return (false, "That member could not be found.");
            var revieweeId = Convert.ToInt32(reviewee);
            if (revieweeId == reviewerId)
                return (false, "You cannot review yourself.");

            var participant = Db.Query(
                "SELECT t.poster_id, r.doer_id FROM tasks t JOIN task_requests r ON r.task_id = t.id " +
                "WHERE t.title = @t AND t.status = 'Completed' AND r.status = 'accepted' AND " +
                "(t.poster_id = @u OR r.doer_id = @u) LIMIT 1",
                r => (PosterId: r.Int("poster_id"), DoerId: r.Int("doer_id")),
                ("@t", taskTitle.Trim()), ("@u", reviewerId)).FirstOrDefault();
            if (participant.PosterId == 0)
                return (false, "Reviews are available only to the poster and accepted performer after completion.");

            var expectedReviewee = participant.PosterId == reviewerId ? participant.DoerId : participant.PosterId;
            if (revieweeId != expectedReviewee)
                return (false, "You can only review the other participant in this completed task.");

            var inserted = Db.NonQuery(
                "INSERT IGNORE INTO reviews (reviewer_id, reviewee_id, task_title, rating, title, body) VALUES (@a, @b, @t, @r, @ti, @bo)",
                ("@a", reviewerId), ("@b", revieweeId), ("@t", taskTitle), ("@r", rating), ("@ti", title.Trim()), ("@bo", body.Trim()));
            if (inserted == 0)
                return (false, "You have already reviewed this participant for this task. Each participant may submit one review per completed task.");

            Notify(revieweeId, "New review received", $"You received a {rating}-star review for “{taskTitle}”.", "tasks", "View reviews");
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>Reviews a member received (received = true) or wrote (received = false).</summary>
    public static List<ReviewRow> LoadReviews(int userId, bool received) =>
        Db.Query(
            $"""
            SELECT r.rating, r.task_title, r.title, r.body, r.created_at, u.first_name, u.last_name
            FROM reviews r JOIN users u ON u.id = r.{(received ? "reviewer_id" : "reviewee_id")}
            WHERE r.{(received ? "reviewee_id" : "reviewer_id")} = @u ORDER BY r.created_at DESC
            """,
            r =>
            {
                var first = r.Str("first_name");
                var last = r.Str("last_name");
                return new ReviewRow($"{first} {last}".Trim(), Initials(first, last), r.Int("rating"), r.Date("created_at"),
                    r.Str("task_title"), r.Str("title"), r.Str("body"));
            },
            ("@u", userId));
}
