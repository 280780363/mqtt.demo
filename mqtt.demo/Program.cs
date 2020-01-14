using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mqtt.server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new MqttFactory().CreateMqttServer();

            server.UseClientConnectedHandler(r =>
            {
                Console.WriteLine($"客户端已连接:{r.ClientId}");
            });


            await server
                .StartAsync(
                    new MqttServerOptionsBuilder()
                     .WithDefaultEndpointBoundIPAddress(IPAddress.Any)
                     .WithDefaultEndpointPort(5678)
                     .WithConnectionValidator(r =>
                     {
                         Console.WriteLine($"client valid success:{r.ClientId}");
                         r.ReasonCode = MqttConnectReasonCode.Success;
                     })
                     .WithSubscriptionInterceptor(context =>
                     {
                         context.AcceptSubscription = true;
                         Console.WriteLine($"subscribe:{context.ClientId}");
                     })
                     .WithApplicationMessageInterceptor(r =>
                     {
                         r.AcceptPublish = true;
                         Console.WriteLine($"reveived msg from client:{r.ClientId},msg content:{Encoding.UTF8.GetString(r.ApplicationMessage.Payload)}");
                     })
                     .Build()
                );


            while (true)
            {
                Console.WriteLine($"输入消息,exit退出");
                Console.Write("> ");
                var msg = Console.ReadLine();
                if (msg == "exit")
                    return;

                await server.PublishAsync(new MqttApplicationMessage
                {
                    Topic = "test/topic",
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce,
                    Payload = Encoding.UTF8.GetBytes(msg),
                });
                Thread.Sleep(10);
            }

            Console.ReadLine();
        }
    }
}
