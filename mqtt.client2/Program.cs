using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mqtt.client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var factory = new MqttFactory();


            //var options = new MqttClientOptionsBuilder()
            //        .WithClientId("Client2")
            //        .WithTcpServer("localhost", 5678)
            //        .Build();

            //var mqttClient = factory.CreateMqttClient();


            //var res = await mqttClient.ConnectAsync(options, CancellationToken.None);

            //await mqttClient.SubscribeAsync(new MQTTnet.Client.Subscribing.MqttClientSubscribeOptions
            //{
            //    TopicFilters =
            //    {
            //        new TopicFilter
            //        {
            //            QualityOfServiceLevel= MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce,Topic="test/topic"
            //        }
            //    }
            //}, CancellationToken.None);

            //mqttClient.ApplicationMessageReceivedHandler = new received();
            //while (true)
            //{
            //    Console.WriteLine($"输入消息,exit退出");
            //    Console.Write("> ");
            //    var msg = Console.ReadLine();
            //    if (msg == "exit")
            //        return;

            //    await mqttClient.PublishAsync(new MqttApplicationMessage
            //    {
            //        QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce,
            //        Topic = "test/topic",
            //        Payload = Encoding.UTF8.GetBytes(msg),
            //        CorrelationData = Encoding.UTF8.GetBytes(msg)
            //    }, CancellationToken.None);
            //    Thread.Sleep(10);
            //}

            // Setup and start a managed MQTT client.
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Client2")
                    .WithNoKeepAlive()
                    .WithTcpServer("localhost", 5678).Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            mqttClient.UseApplicationMessageReceivedHandler(r =>
            {
                Console.WriteLine($"reveice msg from client {r.ClientId ?? "server"},topic {r.ApplicationMessage.Topic}:content:{Encoding.UTF8.GetString(r.ApplicationMessage.Payload)}");
            });
            mqttClient.UseConnectedHandler(r =>
            {
                Console.WriteLine($"########client2已连接到服务器{r.AuthenticateResult.ReasonString}#######");
            });


            mqttClient.UseDisconnectedHandler(r =>
            {
                mqttClient.StopAsync().Wait();
                Console.WriteLine($"########连接已断开{r.Exception.Message}#######");
            });

            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("test/topic").WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce).Build());
            await mqttClient.StartAsync(options);
            while (true)
            {
                Console.WriteLine($"输入消息,exit退出");
                Console.Write("> ");
                var msg = Console.ReadLine();
                if (msg == "exit")
                    return;

                await mqttClient.PublishAsync(new ManagedMqttApplicationMessage
                {
                    Id = Guid.NewGuid(),
                    ApplicationMessage = new MqttApplicationMessage
                    {
                        QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce,
                        Topic = "test/topic",
                        Payload = Encoding.UTF8.GetBytes(msg)
                    }
                });
                Thread.Sleep(10);
            }
            // StartAsync returns immediately, as it starts a new thread using Task.Run, 
            // and so the calling thread needs to wait.
            Console.ReadLine();
        }
    }

    class received : IMqttApplicationMessageReceivedHandler
    {
        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs r)
        {
            await Task.CompletedTask;
            Console.WriteLine($"reveice msg from client {r.ClientId},topic {r.ApplicationMessage.Topic}:content:{Encoding.UTF8.GetString(r.ApplicationMessage.Payload)}");
        }
    }
}
