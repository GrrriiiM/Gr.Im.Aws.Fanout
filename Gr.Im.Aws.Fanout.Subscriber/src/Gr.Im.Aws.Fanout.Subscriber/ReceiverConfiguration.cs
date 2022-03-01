using System;
using System.Text.Json;

namespace Gr.Im.Aws.Fanout.Subscriber
{
    public class ReceiverConfiguration
    {
        public ReceiverConfiguration(string messageName, Type receiverType, JsonSerializerOptions jsonSerializerOptions)
        {
            MessageName = messageName;
            ReceiverType = receiverType;
            JsonSerializerOptions = jsonSerializerOptions;
        }

        public string MessageName { get; private set; }
        public Type ReceiverType { get; private set; }
        public JsonSerializerOptions JsonSerializerOptions { get; private set; }
    }
}