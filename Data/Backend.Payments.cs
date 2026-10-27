namespace TaskTap.Data;

public static partial class Backend
{
    /// <summary>
    /// When a poster completes a task, records a pending payment for the selected performer (once), using the
    /// amount and payment method from the task. TaskTap does not move money: it keeps the record both people can update.
    /// </summary>
    public static void EnsureCompletionPayment(DemoTask task, int ownerId)
    {
        if (task.Id == 0 || ownerId == 0)
            return;

        var doer = Db.Query(
            "SELECT u.id AS doer_id, CONCAT(u.first_name, ' ', u.last_name) AS doer_name " +
            "FROM task_requests r JOIN users u ON u.id = r.doer_id WHERE r.task_id = @t AND r.status = 'accepted' LIMIT 1",
            r => (Id: r.Int("doer_id"), Name: r.Str("doer_name")), ("@t", task.Id)).FirstOrDefault();
        if (doer.Id == 0)
            return;

        var exists = Convert.ToInt32(Db.Scalar(
            "SELECT COUNT(*) FROM payments WHERE owner_id = @o AND task_title = @t AND person = @p",
            ("@o", ownerId), ("@t", task.Title), ("@p", doer.Name)) ?? 0) > 0;
        if (exists)
            return;

        const string method = "Cash";
        InsertPayment(new PaymentRecordDetails(
            task.Title, NextPaymentReference(), method, "Pending", task.Price, DateTime.Today,
            task.Category, task.Barangay, doer.Name, task.Description), ownerId);
        Notify(doer.Id, "Payment recorded", $"A {method} payment of ₱{task.Price:N0} was recorded for “{task.Title}”.", "payments", "View payments");
    }
}
