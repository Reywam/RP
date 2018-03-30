using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace TextRankCalc
{
    class Program
    {        
        static void SendIdToQueue(string id, string exchange, IModel channel)
        {            
            channel.ExchangeDeclare(exchange, "direct");
            string message = "TextRankTask:" + id;
            Console.WriteLine("SENDED " + message);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchange,
                                    routingKey: "text-rank-task",
                                    basicProperties: null,
                                    body: body);            
        }

        static void Main(string[] args)
        {
            const string inputExchange = "backend-api";
            const string outputExchange = "text-rank-tasks";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {                  
                channel.ExchangeDeclare(exchange: inputExchange, type: "fanout");                
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                exchange: inputExchange,
                                routingKey: "");
            
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);                    
                    
                    Console.WriteLine("RECEIVED " + message);

                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 2 && msgArgs[0] == "TextCreated")
                    {
                        string id = msgArgs[1];
                        id = id.Replace("text_", "");
                        SendIdToQueue(id, outputExchange, channel);
                    }
                };
                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);
                Console.ReadLine();
            }
        }
    }
}