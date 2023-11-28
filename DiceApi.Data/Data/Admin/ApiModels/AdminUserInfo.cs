using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Admin
{
    /// <summary>
    /// Моделька для передачи по пагинаций
    /// </summary>
    public class AdminUserInfo
    {
        public long UserId { get; set; }

        public string Name { get; set; }

        public decimal Ballance { get; set; }
    }
}
