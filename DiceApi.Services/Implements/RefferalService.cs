using DiceApi.Data;
using DiceApi.Data.Data.User.Api.Requests;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class RefferalService : IRefferalService
    {
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;

        public RefferalService(IUserService userService, IPaymentService paymentService)
        {
            _userService = userService;

            _paymentService = paymentService;

        }

        public async Task<GetReferalStatsResponce> GetReferalStats(GetRefferalStatsByUserIdRequest request)
        {
            var result = new GetReferalStatsResponce();

            var user = _userService.GetById(request.UserId);
            var dateRange = GetDateRange(request.DateRange);

            var refferals = await _userService.GetRefferalsByUserId(request.UserId);

            var firstDeposits = 0;
            var paymentCount = 0;
            decimal paymentSum = 0;

            var allPayments = new List<Payment>();

            foreach (var refferal in refferals)
            {
                var payments = (await _paymentService.GetPaymentsByUserId(refferal.Id)).Where(p => p.Status == PaymentStatus.Payed && p.CreatedAt >= dateRange);

                if (payments.Any() && payments.All(p => p.CreatedAt >= dateRange && p.CreatedAt <= DateTime.UtcNow))
                {
                    firstDeposits++;
                }

                allPayments.AddRange(payments.ToList());

                paymentCount += payments.Count();
                paymentSum += payments.Sum(p => p.Amount);
            }

            result.RevSharePercent = user.ReferalPercent;
            result.Ballance = user.Ballance;
            result.Income = RevShareHelper.GetRevShareIncome(paymentSum, user.ReferalPercent);
            
            result.RefferalStatasInfo.RegistrationCount = refferals.Count(r => r.RegistrationDate >= dateRange);
            result.RefferalStatasInfo.FirstDeposits = firstDeposits;
            result.RefferalStatasInfo.PaymentsSum = paymentSum;
            result.RefferalStatasInfo.PaymentCount = paymentCount;
            result.RefferalStatasInfo.AverageIncomePerPlayer = (paymentSum / 2) / refferals.Count;

            result.DashBoardInformation = GetDashBoardInformation(request, allPayments, user, refferals);

            return result;
        }

        private List<DashBoardInformation> GetDashBoardInformation(GetRefferalStatsByUserIdRequest request, List<Payment> payments, User owner, List<User> refferals)
        {
            var result = new List<DashBoardInformation>();
            var dateRange = request.DateRange == DateRange.AllTime
                ?  DateTime.UtcNow.AddDays(-30)
                : GetDateRange(request.DateRange);

            List<DateTime> dateList = Enumerable.Range(0, (DateTime.UtcNow - dateRange).Days + 1)
            .Select(offset => dateRange.AddDays(offset))
            .ToList();

            foreach (var date in dateList)
            {
                var info = new DashBoardInformation();

                var dayPayments = payments.Where(p => p.CreatedAt.Day == date.Day && p.CreatedAt.Month == date.Month);

                info.Date = date;
                info.RegistrationCount = refferals.Count(r => r.RegistrationDate.Day == date.Day && r.RegistrationDate.Month == date.Month);
                info.DepositsCount = dayPayments.Count();
                info.Income = RevShareHelper.GetRevShareIncome(dayPayments.Sum(p => p.Amount), owner.ReferalPercent);
                info.DepositsSum = dayPayments.Sum(p => p.Amount);

            }



            return result;
        }


        public DateTime GetDateRange(DateRange dateRange)
        {
            if (dateRange == DateRange.Day)
            {
                return DateTime.UtcNow.AddDays(-1);
            }
            if (dateRange == DateRange.Week)
            {
                return DateTime.UtcNow.AddDays(-7);
            }

            if (dateRange == DateRange.Month)
            {
                return DateTime.UtcNow.AddDays(-30);
            }

            return DateTime.MinValue;
        }
    }
}
