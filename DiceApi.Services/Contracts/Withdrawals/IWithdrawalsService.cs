using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Payment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IWithdrawalsService
    {
        Task<CreateWithdrawalResponce> CreateWithdrawalRequest(CreateWithdrawalRequest createWithdrawalRequest);

        Task<Withdrawal> GetById(long id);

        Task<List<Withdrawal>> GetAll();

        Task<List<Withdrawal>> GetAllByUserId(long userId);

        Task<List<Withdrawal>> GetAllActiveByUserId(long userId);

        Task<List<Withdrawal>> GetAllConfirmedAsync();

        Task СonfirmWithdrawal(long id);

        Task DeactivateWithdrawal(long id);

        Task<WithdrawalStats> GetWithdrawalStats();

        Task<PaginatedList<Withdrawal>> GetPaginatedWithdrawals(GetPaymentWithdrawalsRequest request);

        Task<decimal> GetWithdrawalWaitSum();

        Task UpdateStatus(long id, WithdrawalStatus status);

        Task UpdateStatusWithFkValetId(long id, WithdrawalStatus status);

        Task<long> GetWithdrawalIdByFkWaletId(long id);

        Task UpdateTryCount(long id, int count);

    }
}