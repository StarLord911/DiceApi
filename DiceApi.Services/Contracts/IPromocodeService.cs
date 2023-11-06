using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IPromocodeService
    {
        Task<Promocode> CreatePromocode(CreatePromocodeRequest request);

        Task<ActivatePromocodeResponce> ActivetePromocode(ActivatePromocodeRequest request);
    }
}
