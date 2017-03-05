namespace ClientExampleWithAbstraction
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using NServiceBus;
    using NServiceBus.Logging;
    using ProductMaster.FundShareClass.DataContracts.DomainModel;
    using ProductMaster.FundShareClass.Handlers;
    using Schroders.Bus.Core;
    using Schroders.Bus.Core.Contracts;
    using Schroders.Bus.NServiceBus;
    using Schroders.Bus.NServiceBus.Helpers;

    public class Program
    {
        static ILog logger = LogManager.GetLogger<Program>();

        public static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        public static IContainer StartIndependentBus()
        {
            var configurationBuilder = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<AddUpdateShareClassHandler>().As<IBusHandler>();

            containerBuilder.RegisterType<DefaultBusHandlerProvider>().As<IBusHandlerProvider>();
            containerBuilder.RegisterType<NServiceBus>().As<Schroders.Bus.Core.Contracts.IBus>();
            containerBuilder.RegisterType<CreatedShareClassHandler>().As<IBusHandler>();
            containerBuilder.RegisterType<EndpointInstanceProvider>().As<IEndpointInstanceProvider>().SingleInstance();

            var container = containerBuilder.Build();

            NServiceBusEndpointHosting.StartConfiguredEndpoints(
                configuration,
                container,
                new Dictionary<string, string[]>
                    {
                        {
                            "ProductMaster.FundShareClass.Commands", new[]
                                                                    {
                                                                        "AddUpdateShareClass"
                                                                    }
                        },
                        {
                            "ProductMaster.FundShareClass.Events", new[]
                                                                    {
                                                                        "ShareClassCreated"
                                                                    }
                        }
                    },
                allEndpointConfiguration =>
                {
                    var directoryToScan = AppDomain.CurrentDomain.BaseDirectory;
                    var localFiles = Directory.GetFiles(directoryToScan);
                    var filteredFiles = localFiles.Select(Path.GetFileName).Where(fn => fn != "NServiceBus.Core.dll");

                    allEndpointConfiguration.ExcludeAssemblies(filteredFiles.ToArray());

                    allEndpointConfiguration.Conventions().DefiningMessagesAs(t => t == typeof(BusMessage));
                    allEndpointConfiguration.Conventions().DefiningCommandsAs(t => t == typeof(BusMessage));
                    allEndpointConfiguration.Conventions().DefiningEventsAs(t => t == typeof(BusEvent));
                },
                new Dictionary<string, Action<EndpointConfiguration>>
                    {
                        {
                            "ProductMaster.FundShareClass.Commands", ep => { ep.SendOnly(); }
                        },
                        {
                            "ProductMaster.FundShareClass.Events", ep =>
                            {
                                  var transport = ep.UseTransport<MsmqTransport>();
                                    var routing = transport.Routing();
                                    routing.RegisterPublisher(typeof(BusEvent), "ProductMaster.FundShareClass.Publisher");
                            }
                        }
                    });

            return container;
        }

        public static async Task AsyncMain()
        {
            Console.Title = "Schroders.EventHandling";
            // Abstracted start
            var container = StartIndependentBus();

            var endPoint = container.Resolve<IEndpointInstanceProvider>();
            var endPointInstance = endPoint.Get("AddUpdateShareClass");

            await RunLoop(endPointInstance.Instance);

            try
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            finally
            {
                container.Dispose();
            }
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