using Mediator;
namespace CSharpEssentials.Mediator;

public interface ILoggableRequest : IMessage;

public interface IRequestLoggable : ILoggableRequest;

public interface IResponseLoggable : ILoggableRequest;

public interface IRequestResponseLoggable : IRequestLoggable, IResponseLoggable;
