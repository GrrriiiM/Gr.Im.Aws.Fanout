using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Gr.Im.Aws.Fanout.Publisher
{
    public class MessageConfiguration
    {
        public MessageConfiguration(Type messageType, Func<object, string> messageName, JsonSerializerOptions jsonSerializerOptions, Func<object, string> groupId, IReadOnlyDictionary<string, Func<object, string>> attributes)
        {
            MessageType = messageType;
            MessageName = messageName;
            JsonSerializerOptions = jsonSerializerOptions;
            GroupId = groupId;
            Attributes = attributes;
        }

        public Type MessageType { get; private set; }
        public Func<object, string> MessageName { get; private set; }
        public JsonSerializerOptions JsonSerializerOptions { get; private set; }
        public Func<object, string> GroupId { get; private set; }
        public TopicConfiguration Topic { get; private set; }
        public IReadOnlyDictionary<string, Func<object, string>> Attributes { get; private set; }

    }
}