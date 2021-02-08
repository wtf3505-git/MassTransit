namespace MassTransit.Containers.Tests.FutureTests
{
    using System;
    using System.Threading.Tasks;
    using ExtensionsDependencyInjectionIntegration;
    using FutureScenarios.Consumers;
    using FutureScenarios.Contracts;
    using FutureScenarios.Futures;
    using FutureScenarios.Services;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    [TestFixture]
    public class ComboFuture_Specs :
        FutureTestFixture
    {
        [Test]
        public async Task Should_complete()
        {
            var orderId = NewId.NextGuid();
            var orderLineId = NewId.NextGuid();

            var startedAt = DateTime.UtcNow;

            var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<OrderCombo>>();

            Response<ComboCompleted> response = await client.GetResponse<ComboCompleted>(new
            {
                OrderId = orderId,
                OrderLineId = orderLineId,
                Number = 5
            }, timeout: RequestTimeout.After(s: 5));

            Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
            Assert.That(response.Message.OrderLineId, Is.EqualTo(orderLineId));
            Assert.That(response.Message.Created, Is.GreaterThan(startedAt));
            Assert.That(response.Message.Completed, Is.GreaterThan(response.Message.Created));

            Assert.That(response.Message.Description, Contains.Substring("Fries"));
            Assert.That(response.Message.Description, Contains.Substring("Shake"));
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<IShakeMachine, ShakeMachine>();
            collection.AddSingleton<IFryer, Fryer>();
        }

        protected override void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
            configurator.AddConsumer<PourShakeConsumer>();
            configurator.AddConsumer<CookFryConsumer>();

            configurator.AddFuture<FryFuture>();
            configurator.AddFuture<ShakeFuture>();
            configurator.AddFuture<ComboFuture>();
        }
    }
}
