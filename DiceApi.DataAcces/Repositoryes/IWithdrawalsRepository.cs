using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
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

        Task<List<Withdrawal>> GetAllActive();

        Task<List<Withdrawal>> GetAllConfirmedAsync();

        Task<List<Withdrawal>> GetAll();

        Task<Withdrawal> GetById(long id);

        Task<List<Withdrawal>> GetAllByUserId(long userId);

        Task DeactivateWithdrawal(long id);

        Task<List<Withdrawal>> GetByUserIdAll(long userId);

        Task<PaginatedList<Withdrawal>> GetPaginatedWithdrawals(GetPaymentWithdrawalsRequest request);

        Task<decimal> GetWithdrawalWaitSum();

        Task UpdateStatus(long id, WithdrawalStatus status);

        Task UpdateFkWaletId(long id, long waletId);

        Task UpdateStatusWithFkValetId(long id, WithdrawalStatus status);

        Task<long> GetWithdrawalIdByFkWaletId(long id);
    }
}
