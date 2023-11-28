using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IWithdrawalsService
    {
        Task<CreateWithdrawalResponce> CreateWithdrawalRequest(CreateWithdrawalRequest createWithdrawalRequest);

        Task<List<Withdrawal>> GetAll();

        Task<List<Withdrawal>> GetAllByUserId(long userId);

        Task<List<Withdrawal>> GetAllActiveByUserId(long userId);

        Task СonfirmWithdrawal(long id);

        Task<WithdrawalStats> GetWithdrawalStats();

    }
}