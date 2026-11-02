namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>A notification with one handler per library.</summary>
public sealed record Notification1 : global::Mediator.INotification, global::MediatR.INotification;
