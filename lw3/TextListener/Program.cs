using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.RegularExpressions;

namespace TextListener
{
    class Program
    {
        private static string GetValueById(string id) 
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            IDatabase db = redis.GetDatabase();
                        
            string value = null;
            value = db.StringGet(id);
            return value;
        }

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {   
                while(true)
                {             
                    channel.QueueDeclare(queue: "backend-api",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);
                
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);                        
                        string id = Regex.Split(message, ":")[1];
                        string val = GetValueById(id);
                        Console.WriteLine(id + " : " + val);
                    };
                    channel.BasicConsume(queue: "backend-api",
                                        autoAck: true,
                                        consumer: consumer);                            
                }
            }
        }
    }
}