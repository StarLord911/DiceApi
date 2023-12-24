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
        Task AddWearing(Wager wagering);

        Task<Wager> GetActiveWageringByUserId(long userId);

        Task UpdateWagering(long userId, decimal addWagerSub);

        Task UpdatePlayed(long userId, decimal addPlayedSub);

        Task DeactivateWagering(int wagerId);

        Task ActivateWagering(int wagerId);
    }
}
