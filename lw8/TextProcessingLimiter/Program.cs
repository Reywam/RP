using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace TextProcessingLimiter
{
    class Program
    {
        static void SendMsgToQueue(string id, string status, string exchange, IModel channel)
        {            
            channel.ExchangeDeclare(exchange, "direct");
            string message = "ProcessingAccepted:" + id + ":" + status;
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchange,
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);            
        }


        static void Main(string[] args)
        {
            int maxTextsCount = 2;
            const string inputExchange = "backend-api";
            const string successExchange = "text-success-marker";
            const string outputExchange = "processing-limiter";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {  
                channel.ExchangeDeclare(exchange: inputExchange, type: "fanout");
                channel.ExchangeDeclare(exchange: successExchange, type: "fanout");

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                exchange: inputExchange,
                                routingKey: "");

                var successQueue = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: successQueue,
                                exchange: successExchange,
                                routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);                    
                    
                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 3 && msgArgs[0] == "TextSuccessMarked" && msgArgs[2] == "false")
                    {
                        Console.WriteLine("Max count ++");
                        maxTextsCount++;
                    }
                    
                    if(msgArgs.Length == 2 && msgArgs[0] == "TextCreated")
                    {
                        if( maxTextsCount > 0)
                        {
                            maxTextsCount--;
                            Console.WriteLine("Max count --");
                            Console.WriteLine("RECEIVED: " + message);
                            SendMsgToQueue(msgArgs[1], "true", outputExchange, channel);
                        }
                        else
                        {
                            Console.WriteLine("LIMIT");
                            SendMsgToQueue(msgArgs[1], "false", outputExchange, channel);
                        }
                    } 
                    
                };
                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);
                channel.BasicConsume(queue: successQueue,
                                    autoAck: true,
                                    consumer: consumer);

                Console.WriteLine("Listening messages.");
                Console.ReadLine();
            }
        }
    }
}