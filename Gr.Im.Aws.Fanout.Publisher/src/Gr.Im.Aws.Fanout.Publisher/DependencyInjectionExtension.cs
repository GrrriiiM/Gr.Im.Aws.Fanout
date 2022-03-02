using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Gr.Im.Aws.Fanout.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddAwsFanoutPublisher(this IServiceCollection services, Action<AddAwsPublisherConfig> config = null)
        {
            var _config = new AddAwsPublisherConfig(services);
            config?.Invoke(_config);

            services.AddSingleton<TopicConfiguration>(_ => 
            {
                var configurationRoot  = _.GetRequiredService<IConfiguration>();

                return new TopicConfiguration(
                    configurationRoot.GetSection("AWS:SNS:TopicArn").Value,
                    _config.jsonSerializerOptions,
                    _config.messages.ToDictionary(m => m.MessageType.Name, m => m)
                );
            });

            services.AddScoped<Publisher>();

            return services;
        }

        public class AddAwsPublisherConfig
        {
            readonly IServiceCollection _services;

            public AddAwsPublisherConfig(IServiceCollection services)
            {
                _services = services;
            }


            internal JsonSerializerOptions jsonSerializerOptions;
            public AddAwsPublisherConfig JsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
            {
                this.jsonSerializerOptions = jsonSerializerOptions;
                return this;
            }

            internal List<MessageConfiguration> messages = new List<MessageConfiguration>(); 
            public AddAwsPublisherConfig AddMessage<TMessage>(Action<AddAwsPublisherMessageConfig<TMessage>> config = null) where TMessage : class
            {
                var _config = new AddAwsPublisherMessageConfig<TMessage>();
                config?.Invoke(_config);

                messages.Add(new MessageConfiguration(
                    _config.messageType,
                    _config.messageName,
                    _config.jsonSerializerOptions,
                    _config.groupId,
                    _config.attributes
                ));

                return this;
            }
        }

        public class AddAwsPublisherMessageConfig<TMessage>
        {
            internal Type messageType;

            public AddAwsPublisherMessageConfig()
            {
                messageType = typeof(TMessage);
            }

            internal Func<object, string> messageName;
            public AddAwsPublisherMessageConfig<TMessage> MessageName(Func<TMessage, string> messageName)
            {
                this.messageName = _ => messageName((TMessage)_);
                return this;
            }

            public AddAwsPublisherMessageConfig<TMessage> MessageName(string messageName)
            {
                this.messageName = _ => messageName;
                return this;
            }


            internal JsonSerializerOptions jsonSerializerOptions;
            public AddAwsPublisherMessageConfig<TMessage> JsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
            {
                this.jsonSerializerOptions = jsonSerializerOptions;
                return this;
            }
            
            internal Func<object, string> groupId;
            public AddAwsPublisherMessageConfig<TMessage> GroupId(Func<TMessage, string> groupId)
            {
                this.groupId = _ => groupId((TMessage)_);
                return this;
            }

            internal Dictionary<string, Func<object, string>> attributes = new Dictionary<string, Func<object, string>>();
            public AddAwsPublisherMessageConfig<TMessage> AddAttribute(string key, Func<TMessage, string> value)
            {
                this.attributes.Add(key, _ => value((TMessage)_));
                return this;
            }
        }
        
    }

    
}