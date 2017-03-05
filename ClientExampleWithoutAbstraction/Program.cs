namespace ClientExampleWithoutAbstraction
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using ProductMaster.FundShareClass.DataContracts.DomainModel;
    using Schroders.Bus.Core;

    class Program
    {
        static ILog logger = LogManager.GetLogger<Program>();

        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            IEndpointInstance endpointCommandQueueInstance = await InitialiseCommandQueue();
            IEndpointInstance endPointSubscription = await InitialiseSubscriptionQueue();

            await RunLoop(endpointCommandQueueInstance);

            await endpointCommandQueueInstance.Stop()
                .ConfigureAwait(false);
            //await endpointPublisherInstance.Stop()
            //.ConfigureAwait(false);
        }

        private static async Task<IEndpointInstance> InitialiseCommandQueue()
        {
            var endpointConfiguration = new EndpointConfiguration("ProductMaster.FundShareClass.Commands");

            var transport = endpointConfiguration.UseTransport<MsmqTransport>();

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.SendOnly();
            endpointConfiguration.Conventions().DefiningCommandsAs(t => t == typeof(BusMessage));
            endpointConfiguration.Conventions().DefiningMessagesAs(t => t == typeof(BusMessage));

            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            return endpointInstance;
        }

        private static async Task<IEndpointInstance> InitialiseSubscriptionQueue()
        {
            //var endpointConfiguration = new EndpointConfiguration("Schroders.ProductMaster.Publisher");

            //var transport = endpointConfiguration.UseTransport<MsmqTransport>();

            //endpointConfiguration.UseSerialization<JsonSerializer>();
            //endpointConfiguration.UsePersistence<InMemoryPersistence>();
            //endpointConfiguration.SendFailedMessagesTo("error");
            //endpointConfiguration.Conventions().DefiningEventsAs(t => t == typeof(BusEvent));
            //endpointConfiguration.EnableInstallers();

            //var endpointInstance = await Endpoint.Start(endpointConfiguration)
            //    .ConfigureAwait(false);

            var endpointConfiguration = new EndpointConfiguration("ProductMaster.FundShareClass.Events");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            var transport = endpointConfiguration.UseTransport<MsmqTransport>();
            var routing = transport.Routing();
            routing.RegisterPublisher(
                eventType: typeof(BusEvent),
                publisherEndpoint: "ProductMaster.FundShareClass.Publisher");

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.Conventions().DefiningEventsAs(t => t == typeof(BusEvent));

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            return endpointInstance;
        }

        static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            while (true)
            {
                logger.Info("Press 'P' to place an order, or 'Q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();

                var writeShareClass = new WriteShareClass
                {
                    ShareClassId = Guid.NewGuid().ToString(),
                    AccOrDist = 'A',
                    Class = "ABC",
                    Currency = "EU",
                    Name = "Test Share Class",
                    Priips = true,
                    ShareClassCode = "AB",
                    SystemSource = "R"
                };

                var x = (Object)writeShareClass;

                switch (key.Key)
                {
                    case ConsoleKey.P:
                        // Instantiate the command
                        var busMessage = new BusMessage
                        {
                            TopicName = "AddUpdateShareClass",
                            Payload = "Tset Message"
                            // Payload = x
                        };

                        await endpointInstance.Send("ProductMaster.FundShareClass.Commands", busMessage).ConfigureAwait(false);
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        logger.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }
    }
}