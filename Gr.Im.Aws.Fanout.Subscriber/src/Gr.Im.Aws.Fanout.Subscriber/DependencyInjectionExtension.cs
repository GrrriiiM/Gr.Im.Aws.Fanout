using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Gr.Im.Aws.Fanout.Subscriber;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddAwsFanoutSubscriber(this IServiceCollection services, Action<AddAwsSubscriberConfig> config = null)
        {
            var _config = new AddAwsSubscriberConfig(services);
            
            config?.Invoke(_config);

            services.AddSingleton<SubscriberConfiguration>(_ => 
            {
                

                var configurationRoot  = _.GetRequiredService<IConfiguration>();

                return new SubscriberConfiguration(
                    configurationRoot.GetSection("AWS:SQS:QueueUrl").Value,
                    _config.receivers.ToDictionary(k => k.MessageName, v => v),
                    _config.jsonSerializerOptions
                );
            });

            services.AddScoped<Subscriber>();
            
            return services;
        }

        public static IServiceCollection AddAwsFanoutSubscriberWorker(this IServiceCollection services)
        {
            services.AddHostedService<SubscriberBackgroundService>();
            return services;
        }

        public class AddAwsSubscriberConfig
        {
            readonly IServiceCollection _services;
            internal List<ReceiverConfiguration> receivers = new List<ReceiverConfiguration>();

            public AddAwsSubscriberConfig(IServiceCollection services)
            {
                _services = services;
            }


            internal JsonSerializerOptions jsonSerializerOptions;
            public AddAwsSubscriberConfig JsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
            {
                this.jsonSerializerOptions = jsonSerializerOptions;
                return this;
            }

            public AddAwsSubscriberConfig AddReceiver<TReceiver>(Action<AddAwsReceiverConfig<TReceiver>> config = null) where TReceiver : class
            {
                var _config = new AddAwsReceiverConfig<TReceiver>();
                config?.Invoke(_config);

                var receiverConfiguration = new ReceiverConfiguration(
                    _config.messageName ?? typeof(TReceiver).Name,
                    typeof(TReceiver),
                    _config.jsonSerializerOptions ?? this.jsonSerializerOptions
                );

                receivers.Add(receiverConfiguration);

                _services.AddScoped<TReceiver>();

                return this;
            }
        }

        public class AddAwsReceiverConfig<TMessage>
        {
            internal string messageName;

            public AddAwsReceiverConfig<TMessage> MessageName(string messageName)
            {
                this.messageName = messageName;
                return this;
            }


            internal JsonSerializerOptions jsonSerializerOptions;
            public AddAwsReceiverConfig<TMessage> JsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
            {
                this.jsonSerializerOptions = jsonSerializerOptions;
                return this;
            }
        }
    }
}
