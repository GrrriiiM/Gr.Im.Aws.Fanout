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

namespace Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfigurationRoot>(configuration);

            services.AddAWSService<IAmazonSimpleNotificationService>(configuration.GetAWSOptions("AWS"));
            services.AddAWSService<IAmazonSQS>(configuration.GetAWSOptions("AWS"));

            services.AddAwsFanoutPublisher(c => c
                .AddMessage<MessageTeste123>(m => m
                    .MessageName(_ => $"message-{_.Codigo}")
                    .GroupId(_ => _.Codigo.ToString())
                    .AddAttribute("data", _ => _.Data.ToString("o"))
                )
            );

            services.AddAwsFanoutSubscriber(c => c
                .AddReceiver<TesteReceiver>(r => r.
                    MessageName("message-123")
                )
                .AddReceiver<TesteReceiver>(r => r.
                    MessageName("message-321")
                )
            );

            while (true)
            {
                Console.Write("Digite o codigo: ");
                var codigo = int.Parse(Console.ReadLine());

                Console.Write("Digite o texto: ");
                var texto = Console.ReadLine();

                Console.WriteLine("Enviando mensagem");

                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    await serviceProvider.GetRequiredService<Publisher<MessageTeste123>>().SendAsync(new MessageTeste123
                    {
                        Codigo = codigo,
                        Data = DateTime.Now,
                        Texto = texto
                    });
                    await serviceProvider.GetRequiredService<Subscriber>().ReceiveAsync();
                }
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
