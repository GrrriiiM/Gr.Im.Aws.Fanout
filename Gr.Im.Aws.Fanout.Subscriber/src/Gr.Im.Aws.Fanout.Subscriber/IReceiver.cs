using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gr.Im.Aws.Fanout.Subscriber
{
    public interface IReceiver<TMessage> : IReceiver
    {
        Task Received(TMessage message, CancellationToken cancellationToken);
    }

    public interface IReceiver
    {
        Type MessageType { get; }
        Task Received(object message, CancellationToken cancellationToken);
    }

    public abstract class Receiver<TMessage> : IReceiver<TMessage>
    {
        public Type MessageType => typeof(TMessage);

        public abstract Task Received(TMessage message, CancellationToken cancellationToken);

        public Task Received(object message, CancellationToken cancellationToken)
        {
            return Received((TMessage)message, cancellationToken);
        }
    }

}