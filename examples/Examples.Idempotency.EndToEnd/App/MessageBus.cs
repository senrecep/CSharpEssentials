using System.Threading.Channels;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// An in-process stand-in for an at-least-once broker: a message can be delivered more than once, and the consumer
/// acknowledges every delivery with its outcome.
/// </summary>
public sealed class MessageBus
{
    private readonly Channel<PaymentCaptured> _messages = Channel.CreateUnbounded<PaymentCaptured>();
    private readonly Channel<MessageDelivery> _acknowledgements = Channel.CreateUnbounded<MessageDelivery>();

    public ChannelReader<PaymentCaptured> Messages => _messages.Reader;

    public ChannelReader<MessageDelivery> Acknowledgements => _acknowledgements.Reader;

    public void Publish(PaymentCaptured message) => _messages.Writer.TryWrite(message);

    public void Acknowledge(MessageDelivery delivery) => _acknowledgements.Writer.TryWrite(delivery);
}
