namespace ServerWithAbstraction
{
    using Schroders.Bus.Core;
    using Schroders.Bus.Core.Contracts;
    using Schroders.Bus.NServiceBus;

    public class NServiceBusMessageHandler : BaseNServiceBusHandler<BusMessage>
    {
        public NServiceBusMessageHandler(IBusHandlerProvider busHandlerProvider, IBus bus)
            : base(
                busHandlerProvider,
                bus,
                nbusMessage => nbusMessage,
                message => message)
        {
        }
    }

    public class NServiceBusEventHandler : BaseNServiceBusHandler<BusEvent>
    {
        public NServiceBusEventHandler(IBusHandlerProvider busHandlerProvider, IBus bus)
            : base(
                busHandlerProvider,
                bus,
                nbusMessage => new BusMessage { TopicName = nbusMessage.TopicName, Payload = nbusMessage.Payload },
                message => new BusEvent { TopicName = message.TopicName, Payload = message.Payload })
        {
        }
    }
}

