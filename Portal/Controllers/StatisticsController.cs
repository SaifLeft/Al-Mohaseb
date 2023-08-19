using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Portal.Controllers
{
    public class StatisticsController : Controller
    {
        private AlMohasebDBContext _context;

        public StatisticsController(AlMohasebDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? FirstYear = 0, int? SecondYear = 0)
        {
            var VM = new StatisticsVM();
            var allNames = await _context.MosbName.ToListAsync();
            var allReceivePayments = await _context.MosbReceivePayments.ToListAsync();
            var allReasons = await _context.MosbReasons.ToListAsync();
            var allSpendMoney = await _context.MosbSpendMoney.ToListAsync();


            if (FirstYear == 0 && SecondYear == 0)
            {
                FirstYear = DateTime.Now.Year - 1;
                SecondYear = DateTime.Now.Year ;
            }
            else if (FirstYear == 0 && SecondYear != 0)
            {
                FirstYear = SecondYear - 1;
                SecondYear = FirstYear ;
            }
            else if (FirstYear != 0 && SecondYear == 0)
            {
                SecondYear = FirstYear - 1;
            }
            else
            {
                VM.YearSelected = true;
                VM.FirstYear = FirstYear;
                VM.SecondYear = SecondYear;
            }


            //prepere data for analytics Bar Chart 
            VM.NamesCount = allNames.Count();
            VM.ReceivePaymentsAmountCount = allReceivePayments.Select(allReceivePayments => allReceivePayments.Amount).Sum();
            VM.SpendMoneyAmountCount = allSpendMoney.Select(allSpendMoney => allSpendMoney.Amount).Sum();


            List<string> MonthInEn = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                MonthInEn.Add(new DateTime(2023, i, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("ar-KW")));
            }

            VM.MonthInEn = MonthInEn;


            /*{
                                Year: '2019',
                                AmountBerMonth: [5, 7, 12, 17, 9, 17, 26, 21, 10, 12, 18, 4]
                            }*/
            VM.ReceivePaymentsBerYear = GetYearlyData(allReceivePayments, "Date", "Amount", FirstYear.Value, SecondYear.Value);
            VM.SpendMoneyBerYear = GetYearlyData(allSpendMoney, "Date", "Amount", FirstYear.Value, SecondYear.Value);

            VM.FromFirstYearToLastYearInReceivePayments = GetListOfYears(allReceivePayments);
            VM.FromFirstYearToLastYearInSpendMoney = GetListOfYears(allSpendMoney);

            return View(VM);
        }
        private List<int> GetListOfYears<T>(List<T> Data)
        {
            var FirstYear = Data.Select(Data => DateTime.Parse(Data.GetType().GetProperty("Date").GetValue(Data).ToString()).Year).Min();
            var SecondYear = Data.Select(Data => DateTime.Parse(Data.GetType().GetProperty("Date").GetValue(Data).ToString()).Year).Max();

            List<int> FromFirstYearToLastYear = new List<int>();
            for (int i = FirstYear; i <= SecondYear; i++)
            {
                FromFirstYearToLastYear.Add(i);
            }

            return FromFirstYearToLastYear;

        }
        private List<BerYear> GetYearlyData<T>(List<T> data, string datePropertyName, string amountPropertyName, int FirstYear,int SecondYear)
        {
            var yearlyData = new List<BerYear>();

            for (int year = FirstYear; year <= SecondYear; year++)
            {
                var berYear = new BerYear
                {
                    name = year,
                    Data = GetMonthlyAmountData<T>(data, datePropertyName, amountPropertyName, year)
                };

                yearlyData.Add(berYear);
            }

            return yearlyData;
        }
        private List<double> GetMonthlyAmountData<T>(List<T> data, string datePropertyName, string amountPropertyName, int year)
        {
            List<double> yearlyData = new List<double>();
            for (int month = 1; month <= 12; month++)
            {
                //AmountBerMonth: [TotalAmountInJun, TotalAmountInFeb, 12, 17, 9, 17, 26, 21, 10, 12, 18, 4]
                double AmountInMonth =data.Where(x=> DateTime.Parse(x.GetType().GetProperty(datePropertyName).GetValue(x).ToString()).Year == year && DateTime.Parse(x.GetType().GetProperty(datePropertyName).GetValue(x).ToString()).Month == month).Select(x=> double.Parse(x.GetType().GetProperty(amountPropertyName).GetValue(x).ToString())).Sum();
                yearlyData.Add(AmountInMonth);
            }

            return yearlyData;
        }

       


    }
}
