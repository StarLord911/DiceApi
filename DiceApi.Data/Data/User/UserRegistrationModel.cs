using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Модель для регистраций юзера.
    /// </summary>
    public class UserRegistrationModel
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public long? OwnerId { get; set; }

        public string IpAddres { get; set; }
    }
}
