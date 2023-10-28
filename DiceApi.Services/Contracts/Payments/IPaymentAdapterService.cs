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
        Task<string> CreatePaymentForm(CreateOrderRequest createPaymentRequest);

        /// <summary>
        /// Получение баланса для антиминуса.
        /// </summary>
        /// <returns></returns>
        Task<double> GetCurrentBallance();

        /// <summary>
        /// Обновление баланса для антиминуса при пополнений.
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        void UpdateCurrentBallance(double sum);
    }
}
