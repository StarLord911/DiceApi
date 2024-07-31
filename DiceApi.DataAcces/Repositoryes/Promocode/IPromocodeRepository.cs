using DiceApi.Data;
using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    /// <summary>
    /// Репозиторий для работы с промокодами
    /// </summary>
    public interface IPromocodeRepository 
    {
        Task<List<Promocode>> GetRefferalPromocodesByUserId(long userId);

        Task CreatePromocode(Promocode promocode);

        Task<bool> IsPromocodeContains(string promocode);

        Task<Promocode> GetPromocode(string promocode);

        Task DiactivatePromocode(string promocode);

        Task<List<Promocode>> GetAllPromocodes();

        Task<List<Promocode>> GetPromocodeByLike(string name);

        Task<int> GetActiveRefferalPromocodeCount(long userId);
    }
}
