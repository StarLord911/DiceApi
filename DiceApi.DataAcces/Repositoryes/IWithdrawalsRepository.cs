using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IWithdrawalsRepository
    {
        Task AddWithdrawal(Withdrawal withdrawal);

        
    }
}
