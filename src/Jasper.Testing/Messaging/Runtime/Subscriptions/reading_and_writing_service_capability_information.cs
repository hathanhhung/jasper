﻿using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime.Subscriptions
{
    public class reading_and_writing_service_capability_information
    {
        [Fact]
        public async Task can_write_then_read()
        {
            ServiceCapabilities services;

            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);

                _.ServiceName = "AppWithSubscriptions";


                _.Subscribe.To<Message1>();
                _.Subscribe.To<Message2>();

                _.Publish.Message<Message3>();
                _.Publish.Message<Message4>();
                _.Publish.Message<Message5>();

                _.Settings.Alter<MessagingSettings>(x => x.ThrowOnValidationErrors = false);
            });

            try
            {
                services = runtime.Capabilities;

                services.Errors.Length.ShouldBeGreaterThan(0);
                services.Subscriptions.Length.ShouldBeGreaterThan(0);
                services.Published.Length.ShouldBeGreaterThan(0);
            }
            finally
            {
                await runtime.Shutdown();
            }

            services.WriteToFile("services.json");

            var services2 = ServiceCapabilities.ReadFromFile("services.json");

            services2.ShouldNotBeNull();
            services2.ServiceName.ShouldBe(services.ServiceName);

            services2.Subscriptions.Length.ShouldBe(2);
            services2.Published.Length.ShouldBe(services.Published.Length);
            services2.Errors.Length.ShouldBe(services.Errors.Length);
        }
    }
}
