namespace TaskTap;

/// <summary>Shared payment context passed between the payment history, record, record-payment and review pages.</summary>
public sealed record PaymentRecordDetails(string Task, string Reference, string Method, string Status, decimal Amount, DateTime Date, string Category, string Barangay, string Person, string Description);
