using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Payment;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public async Task ConfirmReferalOwnerPayment(ConfirmPayment payment)
        {
            var user = _userService.GetById(payment.UserId);

            //Обновление баланса владельца акка.
            if (user.OwnerId != null && user.OwnerId.Value != 0)
            {
                var owner = _userService.GetById(user.OwnerId.Value);
                //добавить к полю referalSum updateOwnerBallance

                var updateOwnerBallance = (payment.Amount / 100) * owner.ReferalPercent;

                // var updateOwnerBallance + user.ReferalSum
                // UpdateUserReferalSum

                await _userService.UpdateReferalSum(user.Id, updateOwnerBallance + user.ReferalSum);

                await _userService.UpdateUserBallance(user.OwnerId.Value, owner.Ballance + updateOwnerBallance);
            }
        }

        public async Task<List<Payment>> GetAllPayedPayments()
        {
            return await _paymentRepository.GetAllPayedPayments();
        }

        public async Task<PaginatedList<Payment>> GetPaginatedPayments(GetPaymentsRequest request)
        {
            return await _paymentRepository.GetPaginatedPayments(request);
        }

        public async Task<Payment> GetPaymentsById(long paymentId)
        {
            return await _paymentRepository.GetPaymentsById(paymentId);
        }

        public async Task<List<Payment>> GetPaymentsByUserId(long userId)
        {
            return await _paymentRepository.GetPaymentsByUserId(userId);
        }

        public async Task<PaymentStats> GetPaymentStats()
        {
            var payments = await GetAllPayedPayments();

            var result = new PaymentStats();

            result.ToDay = payments.Where(r => r.CreatedAt.Date == DateTime.Today).Sum(p => p.Amount);

            result.ToWeek = payments.Where(r => IsThisWeek(r.CreatedAt)).Sum(p => p.Amount);

            result.ToMonth = payments.Where(r => IsThisMonth(r.CreatedAt)).Sum(p => p.Amount);

            result.AllDays = payments.Sum(p => p.Amount);

            return result;
        }



        public async Task UpdatePaymentStatus(long paymentId, PaymentStatus status)
        {
            await _paymentRepository.UpdatePaymentStatus(paymentId, status);
        }


        private bool IsThisMonth(DateTime dateTime)
        {
            return dateTime.Month == DateTime.Now.Month && dateTime.Year == DateTime.Now.Year;
        }

        private bool IsThisWeek(DateTime dateTime)
        {
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

            int currentWeek = calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int targetWeek = calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            return currentWeek == targetWeek;
        }
    }
}
