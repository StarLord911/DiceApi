using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers
{
    [Route("api/Referal")]
    [ApiController]
    public class ReferalController : ControllerBase
    {
        public ReferalController()
        {

        }

        // POST api/<ReferalController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ReferalController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ReferalController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
