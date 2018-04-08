using System;
using System.Collections.Generic;
using StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace VowelConsCounter
{
    class Program
    {
        private static void InitializeVowels(HashSet<char> vowels) 
        {            
            vowels.Add('a');
            vowels.Add('e');
            vowels.Add('i');
            vowels.Add('o');
            vowels.Add('u');
            vowels.Add('y');
        }

        private static void InitializeConsonants(HashSet<char> consonants) 
        {
            consonants.Add('b'); consonants.Add('c');consonants.Add('d');
            consonants.Add('f'); consonants.Add('g');consonants.Add('h');
            consonants.Add('j'); consonants.Add('k');consonants.Add('l');
            consonants.Add('m'); consonants.Add('n');consonants.Add('p');
            consonants.Add('q'); consonants.Add('r');consonants.Add('s');
            consonants.Add('t'); consonants.Add('v');consonants.Add('w');
            consonants.Add('x');consonants.Add('z');
        }

        static int CalculateHash(string value)
        {
            int hash = 0;

            for(int i = 0; i < value.Length; i++)
            {
                if(Char.IsLetter(value[i])) 
                {
                    hash++;
                }
            }

            return hash % 10;
        }

        private static void SendDataToQueue(VowelConsCounted data, IModel channel)
        {                    
            channel.ExchangeDeclare("vowel-cons-counter", "direct");
            string message = "VowelConsCounted:" + data.Id + ":" + data.Vowels + ":" + data.Cons;
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "vowel-cons-counter",
                                    routingKey: "vowel-cons-task",
                                    basicProperties: null,
                                    body: body);            
        }

        private static string GetTextById(string id) 
        {            
            Console.WriteLine(id);
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            int dbNum = CalculateHash(id);
            IDatabase db = redis.GetDatabase(dbNum);
                        
            Console.WriteLine("From db #" + dbNum);

            string value = null;            
            value = db.StringGet("text_" + id);
            return value;
        }

        private static VowelConsCounted CalculateVowelsCons(string text, HashSet<char> vowels, HashSet<char> consonants) 
        {
            float vowelsCount = 0;
            float consonantsCount = 0;
            for(int i = 0; i < text.Length; i++)
            {
                if(vowels.Contains(text[i]))
                {
                    vowelsCount++;
                }
                else if(consonants.Contains(text[i]))
                {
                    consonantsCount++;
                }
            }
            return new VowelConsCounted("", vowelsCount.ToString(), consonantsCount.ToString());
        }

        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsCounter");
            HashSet<char> vowels = new HashSet<char>();
            HashSet<char> consonants = new HashSet<char>();
            InitializeConsonants(consonants);
            InitializeVowels(vowels);

            // Получаем сообщения из директа text-ranc-tasks
            const string inputExchange = "text-rank-tasks";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: inputExchange, 
                                        type: "direct");                
                var queueName = "count-task";
                channel.QueueDeclare(queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.QueueBind(queue: queueName,
                                exchange: inputExchange,
                                routingKey: "text-rank-task");
            
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 2 && msgArgs[0] == "TextRankTask")
                    {                        
                        string id = msgArgs[1];                        
                        string text = GetTextById(id);                        
                        id = "text_" + id;                        

                        VowelConsCounted result = CalculateVowelsCons(text, vowels, consonants);                        
                        result.Id = id;                     
                        // Дальше посылаем данные в другой компонент                        
                        SendDataToQueue(result, channel);
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