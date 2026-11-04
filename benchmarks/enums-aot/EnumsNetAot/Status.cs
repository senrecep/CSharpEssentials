namespace EnumsNetAot;

public enum Status
{
    Draft = 0, Submitted = 3, InReview = 7, ChangesRequested = 10, Resubmitted = 12, Escalated = 15, OnHold = 20,
    Scheduled = 22, Assigned = 27, InProgress = 31, PendingApproval = 40, Approved = 44, Rejected = 50, Shipped = 53,
    Delivered = 61, Returned = 64, Refunded = 70, Cancelled = 75, Archived = 90, Closed = 100,
}
