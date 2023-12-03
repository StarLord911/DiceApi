using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос для получение пополнений.
    /// </summary>
    public class GetPaymentsRequest
    {
        public PaginationRequest Pagination { get; set; }

        [Description("2022-11-21")]
        public string? StartDate { get; set; }

        [Description("2022-11-21")]
        public string? EndDate { get; set; }

        public int? PaymentSystemId { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }
    }
}
