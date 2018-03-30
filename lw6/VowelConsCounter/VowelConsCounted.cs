using System;
using System.Collections.Generic;
using StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace VowelConsCounter
{
    class VowelConsCounted
    {        
        public VowelConsCounted(string contextId, string vowelsCount, string consCount)
        {
            this.contextId = contextId;
            this.vowelsCount = vowelsCount;
            this.consCount = consCount;            
        }
        private string contextId;
        private string vowelsCount;
        private string consCount;

        public string Id
        {
            get 
            {
                return contextId;
            }
            
            set
            {
                contextId = value;
            }
        }

        public string Vowels
        {
            get 
            {
                return vowelsCount;
            }
            
            set
            {
                vowelsCount = value;
            }
        }

        public string Cons
        {
            get 
            {
                return consCount;
            }
            
            set
            {
                consCount = value;
            }
        }
    }
}
