using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Gr.Im.Aws.Fanout.Publisher
{
    public class MessageConfiguration<TMessage> where TMessage : class
    {
        public MessageConfiguration(Func<TMessage, string> messageName, JsonSerializerOptions jsonSerializerOptions, Func<TMessage, string> groupId, TopicConfiguration topic, IReadOnlyDictionary<string, Func<TMessage, string>> attributes)
        {
            MessageName = messageName;
            JsonSerializerOptions = jsonSerializerOptions;
            GroupId = groupId;
            Topic = topic;
            Attributes = attributes;
        }

        public Func<TMessage, string> MessageName { get; private set; }
        public JsonSerializerOptions JsonSerializerOptions { get; private set; }
        public Func<TMessage, string> GroupId { get; private set; }
        public TopicConfiguration Topic { get; private set; }
        public IReadOnlyDictionary<string, Func<TMessage, string>> Attributes { get; private set; }

    }
}