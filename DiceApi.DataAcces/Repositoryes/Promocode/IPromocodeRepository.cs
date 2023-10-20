using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IPromocodeRepository 
    {
        Task CreatePromocode(Promocode promocode);

        Task<bool> IsPromocodeContains(string promocode);

        Task<Promocode> GetPromocode(string promocode);

        Task DiactivatePromocode(string promocode);

    }
}
