using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    /// <summary>
    /// Интерфеис предназначен для взаимодействия с платежным адаптером.
    /// </summary>
    public interface IPaymentAdapterService
    {
        /// <summary>
        /// Создание ссылки на оплату.
        /// </summary>
        /// <param name="createPaymentRequest"></param>
        /// <returns></returns>
        Task<CreatePaymentResponse> CreatePaymentForm(CreatePaymentRequest createPaymentRequest, long paymentId);


        Task<GetFreeKassaOrderByIdResponce> GetOrderByFreeKassaId(long orderId);
        /// <summary>
        /// Получение баланса для антиминуса.
        /// </summary>
        /// <returns></returns>
        Task<decimal> GetCurrentBallance();

        /// <summary>
        /// Сделать вывод.
        /// </summary>
        /// <returns></returns>
        Task<long> CreateWithdrawal(Withdrawal withdrawal);

        Task<FkWaletWithdrawalStatusResponce> GetWithdrawalStatusFkWalet(long fkWaletId);

        Task<List<FkWaletSpbWithdrawalBank>> GetSpbWithdrawalBanks();

    }
}
