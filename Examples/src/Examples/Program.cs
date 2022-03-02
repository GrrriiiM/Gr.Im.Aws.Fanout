using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Gr.Im.Aws.Fanout.Publisher;
using Gr.Im.Aws.Fanout.Subscriber;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(c => c
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false))
                .ConfigureServices((context, services) =>
                {
                    services.AddAWSService<IAmazonSimpleNotificationService>(context.Configuration.GetAWSOptions("AWS"));
                    services.AddAWSService<IAmazonSQS>(context.Configuration.GetAWSOptions("AWS"));

                    services.AddAwsFanoutPublisher(c => c
                        .AddMessage<MessageTeste123>(m => m
                            .MessageName(_ => $"mensagem-{_.Codigo}")
                            .GroupId(_ => _.Codigo.ToString())
                            .AddAttribute("data", _ => _.Data.ToString("o"))
                        )
                    );

                    services.AddAwsFanoutSubscriber(c => c
                        .AddReceiver<TesteReceiver>(r => r.
                            MessageName("mensagem-1231")
                        )
                        .AddReceiver<TesteReceiver>(r => r.
                            MessageName("mensagem-321")
                        )
                        .AddReceiver<TesteReceiver>(r => r.
                            MessageName("mensagem-1234")
                        )
                    );

                    services.AddAwsFanoutSubscriberWorker();

                    services.AddHostedService<ExamplePublishBackgroundService>();
                })
                .Build();

            await host.RunAsync();
            
        }
    }

    public class ExamplePublishBackgroundService : BackgroundService
    {
        readonly IServiceProvider _serviceProvider;

        public ExamplePublishBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                Console.Write("Digite o codigo: ");
                var codigo = int.Parse(Console.ReadLine());

                Console.Write("Digite o texto: ");
                var texto = Console.ReadLine();

                Console.WriteLine("Enviando mensagem");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    await serviceProvider.GetRequiredService<Publisher>().SendAsync<MessageTeste123>(new MessageTeste123
                    {
                        Codigo = codigo,
                        Data = DateTime.Now,
                        Texto = texto
                    });
                }

                Console.WriteLine("Mensagem enviada");
            }
        }
    }

    public class MessageTeste123
    {
        public int Codigo { get; set; }
        public DateTime Data { get; set; }
        public string Texto { get; set; }
    }



    public class TesteReceiver : Receiver<MessageTeste123>
    {
        public override Task Received(MessageTeste123 message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Recebendo mensagem 123 - Texto: {message.Texto}");
            return Task.CompletedTask;
        }
    }

}
