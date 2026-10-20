namespace TaskTap.Data;

public static partial class Backend
{
    public sealed record ReviewRow(string OtherName, string OtherInitials, int Rating, DateTime Date, string Task, string Title, string Body);

    /// <summary>Saves (or updates) the reviewer's review of a member for a task.</summary>
    public static (bool Success, string Error) SaveReview(int reviewerId, string revieweeName, string taskTitle, int rating, string title, string body)
    {
        try
        {
            var reviewee = Db.Scalar("SELECT id FROM users WHERE CONCAT(first_name, ' ', last_name) = @n LIMIT 1", ("@n", revieweeName.Trim()));
            if (reviewee is null)
                return (false, "That member could not be found.");
            var revieweeId = Convert.ToInt32(reviewee);
            if (revieweeId == reviewerId)
                return (false, "You cannot review yourself.");

            Db.NonQuery(
                "INSERT INTO reviews (reviewer_id, reviewee_id, task_title, rating, title, body) VALUES (@a, @b, @t, @r, @ti, @bo) " +
                "ON DUPLICATE KEY UPDATE rating = VALUES(rating), title = VALUES(title), body = VALUES(body), created_at = NOW()",
                ("@a", reviewerId), ("@b", revieweeId), ("@t", taskTitle), ("@r", rating), ("@ti", title.Trim()), ("@bo", body.Trim()));
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
