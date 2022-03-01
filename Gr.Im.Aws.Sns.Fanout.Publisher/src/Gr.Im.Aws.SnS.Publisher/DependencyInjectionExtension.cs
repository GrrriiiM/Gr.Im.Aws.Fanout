using System;
using System.Collections.Generic;
using System.Text.Json;
using Gr.Im.Aws.Sns.Fanout.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddAwsPublisherTopic(this IServiceCollection services, Action<AddAwsPublisherConfig> config = null)
        {
            var _config = new AddAwsPublisherConfig(services);
            config?.Invoke(_config);

            services.AddSingleton<TopicConfiguration>(_ => 
            {
                var configurationRoot  = _.GetRequiredService<IConfigurationRoot>();

                return new TopicConfiguration(
                    configurationRoot.GetSection("AWS:SNS:TopicArn").Value,
                    _config.jsonSerializerOptions
                );
            });
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

            public AddAwsPublisherConfig AddMessage<TMessage>(Action<AddAwsPublisherMessageConfig<TMessage>> config = null) where TMessage : class
            {

                _services.AddSingleton<MessageConfiguration<TMessage>>(sp =>
                {
                    var topicConfiguration = sp.GetRequiredService<TopicConfiguration>();

                    var c = new AddAwsPublisherMessageConfig<TMessage>();
                    config?.Invoke(c);

                    return new MessageConfiguration<TMessage>(
                        c.messageName,
                        c.jsonSerializerOptions ?? topicConfiguration.JsonSerializerOptions,
                        c.groupId,
                        topicConfiguration,
                        c.attributes
                    );
                });

                _services.AddScoped<Publisher<TMessage>>();

                return this;
            }
        }

        public class AddAwsPublisherMessageConfig<TMessage>
        {
            internal Func<TMessage, string> messageName;
            public AddAwsPublisherMessageConfig<TMessage> MessageName(Func<TMessage, string> messageName)
            {
                this.messageName = messageName;
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
            
            internal Func<TMessage, string> groupId;
            public AddAwsPublisherMessageConfig<TMessage> GroupId(Func<TMessage, string> groupId)
            {
                this.groupId = groupId;
                return this;
            }

            internal Dictionary<string, Func<TMessage, string>> attributes = new Dictionary<string, Func<TMessage, string>>();
            public AddAwsPublisherMessageConfig<TMessage> AddAttribute(string key, Func<TMessage, string> value)
            {
                this.attributes.Add(key, value);
                return this;
            }
        }
        
    }

    
}