using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserService _userService;

        public PaymentService(IPaymentRepository paymentRepository,
            IUserService userService)
        {
            _paymentRepository = paymentRepository;
            _userService = userService;
        }

        public async Task<long> AddPayment(Payment payment)
        {
            return await _paymentRepository.CreatePayment(payment);
        }

        public async Task ConfirmPayment(ConfirmPayment payment)
        {
            var user = _userService.GetById(payment.UserId);

            //Обновление баланса владельца акка.
            if (user.OwnerId != null && user.OwnerId.Value != 0)
            {
                var updateOwnerBallance = payment.Amount / 10;
                await _userService.UpdateUserBallance(user.OwnerId.Value, updateOwnerBallance);
            }
        }
    }
}
