namespace ClientExampleWithAbstraction
{
    using NServiceBus.Logging;
    using Schroders.Bus.Core;
    using Schroders.Bus.Core.Contracts;

    public class CreatedShareClassHandler : IBusHandler
    {
        static ILog log = LogManager.GetLogger<BusEvent>();

        public object ShareClasses { get; private set; }

        public bool CanHandleMessage(BusMessage message)
        {
            return message?.TopicName == "ShareClassCreated";
        }

        public BusHandlerResponse HandleMessage(BusContext busContext, BusMessage busMessage)
        {
            var x = 10;
            //WriteShareClass shareClass = busMessage.Payload as WriteShareClass;
            log.Info($"Handling: order for bus Event");
            return new BusHandlerResponse();
        }
    }
}
