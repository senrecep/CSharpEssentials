namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>A notification with five handlers per library.</summary>
public sealed record Notification5 : global::Mediator.INotification, global::MediatR.INotification;
