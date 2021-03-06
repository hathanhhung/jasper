﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class message_type_specific_envelope_rules
    {
        [Fact]
        public void apply_message_type_rules_from_attributes()
        {
            var settings = new MessagingSettings();
            var envelope = new Envelope
            {
                Message = new MySpecialMessage()
            };

            settings.ApplyMessageTypeSpecificRules(envelope);

            envelope.Headers["special"].ShouldBe("true");
        }

        [Fact]
        public void apply_message_rules()
        {
            var rule1 = new MessageTypeRule(t => t == typeof(MySpecialMessage), e => e.Headers.Add("rule1", "true"));
            var rule2 = new MessageTypeRule(t => t.IsInNamespace(typeof(MySpecialMessage).Namespace), e => e.Headers.Add("rule2", "true"));

            var settings = new MessagingSettings();
            settings.MessageTypeRules.Add(rule1);
            settings.MessageTypeRules.Add(rule2);

            var envelope = new Envelope
            {
                Message = new MySpecialMessage()
            };

            settings.ApplyMessageTypeSpecificRules(envelope);

            envelope.Headers["rule1"].ShouldBe("true");
            envelope.Headers["rule2"].ShouldBe("true");

        }

        [Fact]
        public async Task see_the_customizations_happen_inside_of_message_context()
        {
            var runtime = await JasperRuntime.BasicAsync();


            try
            {
                var context = runtime.Get<IMessageContext>();

                // Just to force the message context to pool up the envelope instead
                // of sending it out
                await context.EnlistInTransaction(new InMemoryEnvelopeTransaction());

                var mySpecialMessage = new MySpecialMessage();

                await context.Send("tcp://localhost:2001".ToUri(), mySpecialMessage);

                var outgoing = context.As<MessageContext>().Outstanding.Single();

                outgoing.Headers["special"].ShouldBe("true");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task customize_with_fluent_interface_against_a_specific_type()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Publish.Message<MySpecialMessage>().Customize(e => e.Headers.Add("rule", "true"));
            });

            try
            {
                var context = runtime.Get<IMessageContext>();

                // Just to force the message context to pool up the envelope instead
                // of sending it out
                await context.EnlistInTransaction(new InMemoryEnvelopeTransaction());

                var mySpecialMessage = new MySpecialMessage();

                await context.Send("tcp://localhost:2001".ToUri(), mySpecialMessage);

                var outgoing = context.As<MessageContext>().Outstanding.Single();

                outgoing.Headers["rule"].ShouldBe("true");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task customize_with_fluent_interface_against_a_type_filter()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Publish.MessagesFromNamespace(GetType().Namespace).Customize(e => e.Headers.Add("rule2", "true"));
            });

            try
            {
                var context = runtime.Get<IMessageContext>();

                // Just to force the message context to pool up the envelope instead
                // of sending it out
                await context.EnlistInTransaction(new InMemoryEnvelopeTransaction());

                var mySpecialMessage = new MySpecialMessage();

                await context.Send("tcp://localhost:2001".ToUri(), mySpecialMessage);

                var outgoing = context.As<MessageContext>().Outstanding.Single();

                outgoing.Headers["rule2"].ShouldBe("true");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }


        [Fact]
        public void deliver_by_mechanics()
        {
            var settings = new MessagingSettings();
            var envelope = new Envelope
            {
                Message = new MySpecialMessage()
            };

            settings.ApplyMessageTypeSpecificRules(envelope);

            envelope.DeliverBy.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        }


    }

    public class SpecialAttribute : ModifyEnvelopeAttribute
    {
        public override void Modify(Envelope envelope)
        {
            envelope.Headers.Add("special", "true");
        }
    }

    [Special, DeliverWithin(5)]
    public class MySpecialMessage
    {

    }

    // SAMPLE: StatusMessageSendingApp
    public class StatusMessageSendingApp : JasperRegistry
    {
        public StatusMessageSendingApp()
        {
            // Any time StatusMessage is published,
            // set the deliver within limit on the outgoing Envelope
            Publish.Message<StatusMessage>()
                .Customize(envelope => envelope.DeliverWithin(10.Seconds()));

            // Use a criteria against the message type to say
            // that all messages contained in the "MyApp.Status" namespace
            // would be published with the deliver within rule
            Publish.MessagesFromNamespace("MyApp.Status")
                .Customize(envelope => envelope.DeliverWithin(10.Seconds()));
        }
    }

    // ENDSAMPLE

    // SAMPLE: UsingDeliverWithinAttribute
    // Any message of this type should be successfully
    // delivered within 10 seconds or discarded
    [DeliverWithin(10)]
    public class StatusMessage{}
    // ENDSAMPLE
}
