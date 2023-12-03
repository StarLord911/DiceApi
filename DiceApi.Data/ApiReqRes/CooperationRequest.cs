using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос на заявку о сотрудничистве.
    /// </summary>
    public class CooperationRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
