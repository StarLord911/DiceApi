using DiceApi.Data.Payments;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPaymentRequisitesRepository
    {
        /// <summary>
        /// Добавление реквезитов
        /// </summary>
        Task AddPaymentRequisite(PaymentRequisite paymentRequisite);

        /// <summary>
        /// Получение реквезитов по id юзера
        /// </summary>
        Task<PaymentRequisite> GetPaymentRequisiteByUserId(long userId);
    }
}