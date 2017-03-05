namespace ServerWithAbstraction
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using NServiceBus;
    using ProductMaster.FundShareClass.Handlers;
    using Schroders.Bus.Core;
    using Schroders.Bus.Core.Contracts;
    using Schroders.Bus.NServiceBus;
    using Schroders.Bus.NServiceBus.Helpers;

    public class Program
    {
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
                            "ProductMaster.FundShareClass.Publisher", new[]
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
                            "ProductMaster.FundShareClass.Commands", ep => { }
                        },
                        {
                            "ProductMaster.FundShareClass.Publisher", ep => { }                          
                        }
                    });

            return container;
        }

        public static async Task AsyncMain()
        {
            Console.Title = "Schroders.EventHandling";
            // Abstracted start
            var container = StartIndependentBus();

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
    }
}