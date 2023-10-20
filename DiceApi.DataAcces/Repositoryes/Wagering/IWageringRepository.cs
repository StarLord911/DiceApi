using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IWageringRepository
    {
        Task AddWearing(Wagering wagering);

        Task<Wagering> GetActiveWageringByUserId(long userId);

        Task UpdateWagering(long userId, long addWagerSub);

        Task UpdatePlayed(long userId, long addPlayedSub);

        Task DeactivateWagering(int wagerId);

        Task ActivateWagering(int wagerId);
    }
}
