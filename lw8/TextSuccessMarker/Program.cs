using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace TextSuccessMarker
{
    class Program
    {
        static void SendMsgToQueue(string id, string status, string exchange, IModel channel)
        {            
            channel.ExchangeDeclare(exchange, "fanout");
            string message = "TextSuccessMarked:" + id + ":" + status;
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchange,
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);            
        }


        static void Main(string[] args)
        {
            const float minSuccessValue = 0.5f;
            const string inputExchange = "text-rank-calc";
            const string outputExchange = "text-success-marker";

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

                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 3 && msgArgs[0] == "TextRankCalculated")
                    {
                        Console.WriteLine("RECEIVED: " + message);
                        string id = msgArgs[1];
                        float rank = float.Parse(msgArgs[2]);                        
                        if(rank > minSuccessValue)
                        {
                            SendMsgToQueue(msgArgs[1], "true", outputExchange, channel);
                        } 
                        else
                        {
                            SendMsgToQueue(msgArgs[1], "false", outputExchange, channel);
                        }
                    } 
                };
                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);

                Console.WriteLine("Listening messages.");
                Console.ReadLine();
            }
        }
    }
}