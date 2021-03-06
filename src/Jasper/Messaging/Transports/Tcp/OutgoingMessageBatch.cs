using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports.Tcp
{
    public class OutgoingMessageBatch
    {
        public OutgoingMessageBatch(Uri destination, IEnumerable<Envelope> messages)
        {
            Destination = destination;
            var messagesList = new List<Envelope>();
            messagesList.AddRange(messages);
            Messages = messagesList;

            foreach (var message in messages)
            {
                message.Destination = destination;
            }

            Data = Envelope.Serialize(Messages);
        }

        public byte[] Data { get; set; }

        public Uri Destination { get; }

        public IList<Envelope> Messages { get; }

        public override string ToString()
        {
            return $"Outgoing batch to {Destination} with {Messages.Count} messages";
        }

        public static OutgoingMessageBatch ForPing(Uri destination)
        {
            var envelope = Envelope.ForPing();
            envelope.Destination = destination;

            return new OutgoingMessageBatch(destination, new[]{envelope})
            {
                IsPing = true
            };
        }

        public bool IsPing { get; private set; }
    }
}
