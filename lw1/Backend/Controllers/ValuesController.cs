using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        static readonly ConcurrentDictionary<string, string> _data = new ConcurrentDictionary<string, string>();

        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            string value = null;
            _data.TryGetValue(id, out value);
            return value;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromForm]string value)
        {
            var id = Guid.NewGuid().ToString();
            Console.WriteLine(id);
            _data[id] = value;
            return id;
        }
    }
}
