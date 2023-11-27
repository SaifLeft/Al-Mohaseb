using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace Portal.Controllers
{
    public class StatisticsController : Controller
    {
        private AlMohasebDBContext _context;

        #region Index
        public StatisticsController(AlMohasebDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? RPFirstYear = 0, int? RPSecondYear = 0, int? SMFirstYear = 0, int? SMSecondYear = 0, int? AddCountYear = 0)
        {
            var VM = new StatisticsVM();
            var allNames = await _context.MosbName.ToListAsync();
            var allReceivePayments = await _context.MosbReceivePayments.ToListAsync();
            var allReasons = await _context.MosbReasons.ToListAsync();
            var allSpendMoney = await _context.MosbSpendMoney.ToListAsync();


            if (RPFirstYear == 0 && RPSecondYear == 0)
            {
                RPFirstYear = DateTime.Now.Year - 1;
                RPSecondYear = DateTime.Now.Year;
            }
            else if (RPFirstYear == 0 && RPSecondYear != 0)
            {
                RPFirstYear = RPSecondYear - 1;
                RPSecondYear = RPFirstYear;
            }
            else if (RPFirstYear != 0 && RPSecondYear == 0)
            {
                RPSecondYear = RPFirstYear - 1;
            }
            else
            {
                VM.RPYearSelected = true;
                VM.RPFirstYear = RPFirstYear;
                VM.RPSecondYear = RPSecondYear;
            }

            if (SMFirstYear == 0 && SMSecondYear == 0)
            {
                SMFirstYear = DateTime.Now.Year - 1;
                SMSecondYear = DateTime.Now.Year;
            }
            else if (SMFirstYear == 0 && SMSecondYear != 0)
            {
                SMFirstYear = SMSecondYear - 1;
                SMSecondYear = SMFirstYear;
            }
            else if (SMFirstYear != 0 && SMSecondYear == 0)
            {
                SMSecondYear = SMFirstYear - 1;
            }
            else
            {
                VM.SMYearSelected = true;
                VM.SMFirstYear = SMFirstYear;
                VM.SMSecondYear = SMSecondYear;
            }


            //prepare data for analytics Bar Chart 
            VM.NamesCount = allNames.Count();
            VM.ReceivePaymentsAmountCount = allReceivePayments.Select(allReceivePayments => allReceivePayments.Amount).Sum();
            VM.SpendMoneyAmountCount = allSpendMoney.Select(allSpendMoney => allSpendMoney.Amount).Sum();




            //get Names that been added in certain year.
            if (AddCountYear != 0)
            {
                VM.AddCountYear = AddCountYear;
            }
            VM.NamesCountInCertainYear = GetNameCountInCertainYear(allNames, AddCountYear);
            VM.ReasonsCountInCertainYear = GetReasonCountInCertainYear(allReasons, AddCountYear);




            List<string> MonthInAr = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                MonthInAr.Add(new DateTime(2023, i, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("ar-KW")));
            }

            VM.MonthInEn = MonthInAr;


            VM.ReceivePaymentsBerYear = GetYearlyData(allReceivePayments, "Date", "Amount", RPFirstYear.Value, RPSecondYear.Value);

            VM.FromFirstYearToLastYearInReceivePayments = GetListOfYears(allReceivePayments);
            VM.FromFirstYearToLastYearInSpendMoney = GetListOfYears(allSpendMoney);


            VM.SpendMoneyBerYear = GetYearlyData(allSpendMoney, "Date", "Amount", RPFirstYear.Value, RPSecondYear.Value);




            return View(VM);
        }

        #region أحصائيات الأضافات السنوية

        private int GetNameCountInCertainYear(List<MosbName> allNames, int? addCountYear)
        {
            //if addCountYear is 0 then return all names count on carrnt year
            if (addCountYear == 0)
            {
                return allNames.Where(allNames => DateTime.Parse(allNames.RegisterDate).Year == DateTime.Now.Year).Count();
            }
            else
            {
                return allNames.Where(allNames => DateTime.Parse(allNames.RegisterDate).Year == addCountYear).Count();
            }
        }
        private int GetReasonCountInCertainYear(List<MosbReasons> allReasons, int? addCountYear)
        {
            //if addCountYear is 0 then return all names count on carrnt year
            if (addCountYear == 0)
            {
                return allReasons.Where(allReasons => DateTime.Parse(allReasons.Date).Year == DateTime.Now.Year).Count();
            }
            else
            {
                return allReasons.Where(allReasons => DateTime.Parse(allReasons.Date).Year == addCountYear).Count();
            }
        }
        #endregion أحصائيات الأضافات السنوية

        private List<int> GetListOfYears<T>(List<T> Data)
        {
            if (Data.Count == 0)
            {
                return new List<int>();
            }
            var FirstYear = Data.Select(Data => DateTime.Parse(Data.GetType().GetProperty("Date").GetValue(Data).ToString()).Year).Min();
            var SecondYear = Data.Select(Data => DateTime.Parse(Data.GetType().GetProperty("Date").GetValue(Data).ToString()).Year).Max();

            List<int> FromFirstYearToLastYear = new List<int>();
            for (int i = FirstYear; i <= SecondYear; i++)
            {
                FromFirstYearToLastYear.Add(i);
            }

            return FromFirstYearToLastYear;

        }
        private List<BerYear> GetYearlyData<T>(List<T> data, string datePropertyName, string amountPropertyName, int FirstYear, int SecondYear)
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
                double AmountInMonth = data.Where(x => DateTime.Parse(x.GetType().GetProperty(datePropertyName).GetValue(x).ToString()).Year == year && DateTime.Parse(x.GetType().GetProperty(datePropertyName).GetValue(x).ToString()).Month == month).Select(x => double.Parse(x.GetType().GetProperty(amountPropertyName).GetValue(x).ToString())).Sum();
                yearlyData.Add(AmountInMonth);
            }

            return yearlyData;
        }

        #endregion Index


        #region Person

        public async Task<IActionResult> Person(int? PersonId = null)
        {
            PersonVM VM = new();

            if (PersonId != null)
            {
                VM.FromQuery = true;

                MosbName? PersonDetails = await _context.MosbName
                    .Include(x => x.MosbReceivePayments)
                    .ThenInclude(x => x.ReceivePaymentsReasonsMapping)
                    .Include(x => x.MosbSpendMoney)
                    .Include(x => x.PersonReasonMapping)
                    .ThenInclude(x => x.Reasons)
                    .FirstOrDefaultAsync(x => x.Id == PersonId);

                VM.FromQearyDetails = PersonDetails;
                VM.PersonId = PersonId.Value;
            }

            // Rest of your code

            return View(VM); // or however you want to return the result
        }


        #endregion Person


        #region Reason

        public async Task<IActionResult> Reason(int? ReasonId = null)
        {
            ReasonVM VM = new();

            if (ReasonId != null)
            {
                VM.FromQuery = true;

                MosbReasons? ReasonDetails = await _context.MosbReasons
                    .Include(x => x.PersonReasonMapping)
                    .ThenInclude(x => x.Name)
                    .Include(x => x.ReceivePaymentsReasonsMapping)
                    .ThenInclude(x => x.ReceivePayments)
                    .FirstOrDefaultAsync(x => x.Id == ReasonId);

                VM.FromQearyDetails = ReasonDetails;
                VM.ReasonId = ReasonId.Value;
            }

            // Rest of your code

            return View(VM); // or however you want to return the result
        }

        #endregion Reason


        #region Semple
        public async Task<IActionResult> Semple(long? NameId, int? Year)
        {
            try
            {
                var VM = new SempleVM();

                var ReceivePayments = await _context.MosbReceivePayments
                    .Include(x => x.Name)
                    .Include(x => x.ReceivePaymentsReasonsMapping)
                    .ToListAsync();

                var SpendMoney = await _context.MosbSpendMoney
                    .Include(x => x.Person)
                    .Include(x => x.Reasons)
                    .ToListAsync();

                var Names = await _context.MosbName.ToListAsync();

                VM.AllReceivePaymentsAmount = Math.Round(ReceivePayments.Sum(x => x.Amount),3);
                VM.AllSpendMoneyAmount = Math.Round(SpendMoney.Sum(x => x.Amount), 3);
                VM.GeneralBalance = VM.AllReceivePaymentsAmount - VM.AllSpendMoneyAmount;


                VM.NamesList = Names.Select(x => new SelectListModel { Text = x.Name, Value = x.Id, IsSelected = x.Id == NameId }).ToList();

                VM.PersonalBalanceIsAvailable = false;

                if (NameId != null)
                {
                    VM.PersonalBalanceIsAvailable = true;
                    VM.PersonalReceivePayment = Math.Round(ReceivePayments.Where(x => x.NameId == NameId).Sum(x => x.Amount),3);
                    VM.PersonalSpendMoney = Math.Round(SpendMoney.Where(x => x.PersonId == NameId).Sum(x => x.Amount), 3);
                    VM.PersonalTotalAmount = VM.PersonalReceivePayment - VM.PersonalSpendMoney;
                }


                List<SelectListModel> AllYears = new();
                AddYearsToListModel(ReceivePayments, ref AllYears, x => DateTime.Parse(x.Date).Year, Year);
                AddYearsToListModel(SpendMoney, ref AllYears, x => DateTime.Parse(x.Date).Year, Year);
                VM.YearsList = AllYears.GroupBy(x => x.Value).Select(x => x.First()).ToList();

                VM.YearIsAvailable = false;
                if (Year != null)
                {
                    var YearlyDateTime = new DateTime(Year.Value, 1, 1);
                    VM.SelectedYear = Year.Value;
                    VM.YearIsAvailable = true;
                    var SpendMoneyDate = SpendMoney.Where(x => DateTime.Parse(x.Date).Year == YearlyDateTime.Year).ToList();
                    VM.SpendMoneyYearlyData = SpendMoney.Sum(x => x.Amount);
                    VM.ReceivePaymentsYearlyData = ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == YearlyDateTime.Year).Sum(x => x.Amount);
                    VM.TotalAmountYearlyData = VM.ReceivePaymentsYearlyData - VM.SpendMoneyYearlyData;
                }


                VM.PersonsBalance = Names.Select(x => new ShowPersonsTotal
                {
                    PhoneNumber = x.PhoneNumber,
                    Name = x.Name,
                    ReceivePayment = Math.Round(x.MosbReceivePayments.Sum(x => x.Amount), 3),
                    SpendMoney = Math.Round(x.MosbSpendMoney.Sum(x => x.Amount), 3),
                    TotalAmount = Math.Round((x.MosbReceivePayments.Sum(x => x.Amount) - x.MosbSpendMoney.Sum(x => x.Amount)), 3)
                }).ToList();

                return View(VM);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AddYearsToListModel<T>(List<T> data, ref List<SelectListModel> yearsList, Func<T, int> yearExtractor, int? selectedYear)
        {
            yearsList.AddRange(data.Select(x => new SelectListModel
            {
                Text = yearExtractor(x).ToString(),
                Value = yearExtractor(x),
                IsSelected = selectedYear != null && yearExtractor(x) == selectedYear
            }));
        }


        #endregion Semple




    }
}
