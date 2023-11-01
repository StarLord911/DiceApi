using DiceApi.Data;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IWithdrawalsService
    {
        Task<CreateWithdrawalResponce> CreateWithdrawalRequest(CreateWithdrawalRequest createWithdrawalRequest);
    }
}