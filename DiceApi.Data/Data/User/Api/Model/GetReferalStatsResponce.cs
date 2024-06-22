using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Ответ для модели с реферальной системой.
    /// </summary>
    public class GetReferalStatsResponce
    {
        public GetReferalStatsResponce()
        {
            RefferalStatasInfo = new RefferalStatasInfo();
            DashBoardInformation = new List<DashBoardInformation>();
        }

        public int RevSharePercent { get; set; }

        public decimal Ballance { get; set; }

        public decimal Income { get; set; }

        public RefferalStatasInfo RefferalStatasInfo { get; set; }

        public List<DashBoardInformation> DashBoardInformation { get; set; }

    }

    /// <summary>
    /// Класс содержит базовую инфу которая может колебаться по дням.
    /// </summary>
    public class RefferalStatasInfo
    {
        public int RegistrationCount { get; set; }

        public decimal AverageIncomePerPlayer { get; set; }

        public int FirstDeposits { get; set; }

        public int PaymentCount { get; set; }

        public decimal PaymentsSum { get; set; }
    }

    public class DashBoardInformation
    {
        public DateTime Date { get; set; }

        public decimal Income { get; set; }

        public int RegistrationCount { get; set; }

        public decimal DepositsSum { get; set; }

        public int DepositsCount { get; set; }

    }

    public class GetProfitStatsResponce
    {
        public decimal ToDayReferals { get; set; }

        public decimal ToMonthReferals { get; set; }

        public decimal ToAllTimeReferals { get; set; }

    }
}
