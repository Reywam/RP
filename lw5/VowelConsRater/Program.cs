using System;
using StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace VowelConsRater
{
    class Program
    {
        private static void SetRankInDbById(string id, float value) 
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            IDatabase db = redis.GetDatabase();
                                    
            db.StringSet(id, value);            
        }
        
        private static float CalculateRank(string vowels, string cons)
        {
            float vowelsCount = float.Parse(vowels);
            float consCount = float.Parse(cons);
            float result = vowelsCount;
            if(consCount != 0)
            {
                result = vowelsCount / consCount;
            }
            return result;
        }

        static void Main(string[] args)
        {            
            const string exchange = "vowel-cons-counter";
            const string key = "vowel-cons-task";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchange, 
                                        type: "direct");                
                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                exchange: exchange,
                                routingKey: key);
            
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 4 && msgArgs[0] == "VowelConsCounted")
                    {                
                        VowelConsCounted data = new VowelConsCounted(msgArgs[1], msgArgs[2], msgArgs[3]);
                        float rank = CalculateRank(data.Vowels, data.Cons);
                        SetRankInDbById(data.Id, rank);
                    }

                };
                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);

                Console.WriteLine("Waiting for messages");
                Console.ReadLine();
            }
        }
    }
}