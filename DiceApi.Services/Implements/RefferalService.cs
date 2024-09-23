using DiceApi.Common;
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

            if (dateRange == null)
            {
                dateRange = user.RegistrationDate.Date;
            }
            else
            {
                dateRange = dateRange.Value.Date;
            }

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

            if (refferals.Count > 0)
            {
                result.RefferalStatasInfo.AverageIncomePerPlayer = ((paymentSum / 100) * user.ReferalPercent) / refferals.Count;
            }
            else
            {
                result.RefferalStatasInfo.AverageIncomePerPlayer = 0;
            }

            result.DashBoardInformation = GetDashBoardInformation(request, allPayments, user, refferals);

            return result;
        }

        private List<DashBoardInformation> GetDashBoardInformation(GetRefferalStatsByUserIdRequest request, List<Payment> payments, User owner, List<User> refferals)
        {
            var result = new List<DashBoardInformation>();
            var dateRange = request.DateRange == DateRange.AllTime
                ? owner.RegistrationDate.Date
                : GetDateRange(request.DateRange).Value.Date;

            List<DateTime> dateList = Enumerable.Range(0, (DateTime.UtcNow.Date - dateRange).Days + 1)
            .Select(offset => dateRange.AddDays(offset))
            .ToList();

            if (request.DateRange == DateRange.Day)
            {
                dateList.Add(DateTime.Now.GetMSKDateTime());
            }

            if (request.DateRange == DateRange.Day)
            {
                result.Add(new DashBoardInformation());
            }
            
            foreach (var date in dateList)
            {
                var info = new DashBoardInformation();

                var dayPayments = payments.Where(p => p.CreatedAt.Day == date.Day && p.CreatedAt.Month == date.Month);

                info.Date = date;
                info.RegistrationCount = refferals.Count(r => r.RegistrationDate.Day == date.Day && r.RegistrationDate.Month == date.Month);
                info.DepositsCount = dayPayments.Count();
                info.Income = RevShareHelper.GetRevShareIncome(dayPayments.Sum(p => p.Amount), owner.ReferalPercent);
                info.DepositsSum = dayPayments.Sum(p => p.Amount);

                result.Add(info);
            }

            if (request.DateRange == DateRange.Day)
            {
                result.Add(new DashBoardInformation());
            }

            return result;
        }


        public GetReferalStatsResponce GetEmptyData(User user, DateTime? dateRange)
        {
            var result = new GetReferalStatsResponce();

            result.RevSharePercent = user.ReferalPercent;
            result.Ballance = user.Ballance;
            result.Income = 0;

            result.RefferalStatasInfo.RegistrationCount = 0;
            result.RefferalStatasInfo.FirstDeposits = 0;
            result.RefferalStatasInfo.PaymentsSum = 0;
            result.RefferalStatasInfo.PaymentCount = 0;
            result.RefferalStatasInfo.AverageIncomePerPlayer = 0;

            result.DashBoardInformation = new List<DashBoardInformation>();


            List<DateTime> dateList = Enumerable.Range(0, (DateTime.UtcNow - dateRange).Value.Days + 1)
            .Select(offset => dateRange.Value.AddDays(offset))
            .ToList();

            foreach (var date in dateList)
            {
                var info = new DashBoardInformation();

                info.Date = date;
                info.RegistrationCount = 0;
                info.DepositsCount = 0;
                info.Income = 0;
                info.DepositsSum = 0;

                result.DashBoardInformation.Add(info);
            }

            return result;


        }

        public DateTime? GetDateRange(DateRange dateRange)
        {
            if (dateRange == DateRange.Day)
            {
                return DateTime.UtcNow.GetMSKDateTime();
            }
            if (dateRange == DateRange.Week)
            {
                return DateTime.UtcNow.AddDays(-7).GetMSKDateTime();
            }

            if (dateRange == DateRange.Month)
            {
                return DateTime.UtcNow.AddDays(-30).GetMSKDateTime();
            }

            return null;
        }
    }
}
