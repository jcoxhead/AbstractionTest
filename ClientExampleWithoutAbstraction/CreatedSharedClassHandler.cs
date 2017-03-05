namespace ClientExampleWithoutAbstraction
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Schroders.Bus.Core;

    public class CreatedSharedClassHandler : IHandleMessages<BusEvent>
    {
        static ILog log = LogManager.GetLogger<BusEvent>();

        public Task Handle(BusEvent message, IMessageHandlerContext context)
        {
            log.Info($"Handling: order for bus Event");
            return Task.CompletedTask;
        }
    }
}
