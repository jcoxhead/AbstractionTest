namespace ProductMaster.FundShareClass.Handlers
{
    using DataContracts.DomainModel;
    using Schroders.Bus.Core;
    using Schroders.Bus.Core.Contracts;

    public class AddUpdateShareClassHandler : IBusHandler
    {
        public object ShareClasses { get; private set; }

        public bool CanHandleMessage(BusMessage message)
        {
            return message?.TopicName == "AddUpdateShareClass";
        }

        public BusHandlerResponse HandleMessage(BusContext busContext, BusMessage busMessage)
        {
            var x = 10;
            //WriteShareClass shareClass = busMessage.Payload as WriteShareClass;
            busContext.Bus.Publish(
                 "ShareClassCreated",
                 new BusEvent
                 {
                     TopicName = "ShareClassCreated",
                     Payload = "Test"
                 });
            return new BusHandlerResponse();
        }
    }
}
