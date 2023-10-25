using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class AuthenticateResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public string Info { get; set; }

        public AuthenticateResponse(User user, string token)
        {
            Id = user.Id;
            Name = user.Name;
            Token = token;
        }

        public AuthenticateResponse()
        {

        }
    }
}
