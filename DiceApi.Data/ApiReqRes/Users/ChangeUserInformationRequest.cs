using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос на изменение данных юзера
    /// </summary>
    public class ChangeUserInformationRequest
    {
        public long UserId { get; set; }

        public string NewName { get; set; }
        
        public string NewPassword { get; set; }
    }
}
