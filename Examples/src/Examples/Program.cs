using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Gr.Im.Aws.Sns.Fanout.Publisher;
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

            services.AddAwsPublisherTopic(c => c
                .AddMessage<MessageTeste1>(m => m
                    .MessageName(_ => $"message-{_.Codigo}")
                    .GroupId(_ => _.Codigo.ToString())
                    .AddAttribute("data", _ => _.Data.ToString("o"))
                )
            );

            while (true)
            {
                Console.Write("Digite o codigo: ");
                var codigo = int.Parse(Console.ReadLine());

                Console.WriteLine("Envidando mensagem");

                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    await serviceProvider.GetRequiredService<Publisher<MessageTeste1>>().SendAsync(new MessageTeste1
                    {
                        Codigo = codigo,
                        Data = DateTime.Now,
                    });
                }
            }
        }
    }

    public class MessageTeste1
    {
        public int Codigo { get; set; }
        public DateTime Data { get; set; }
    }
}
