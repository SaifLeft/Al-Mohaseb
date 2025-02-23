using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Globalization;

namespace Portal.Controllers
{
    [Authorize]
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
            List<double> yearlyData = new();
            for (int month = 1; month <= 12; month++)
            {
                //AmountBerMonth: [TotalAmountInJun, TotalAmountInFeb, 12, 17, 9, 17, 26, 21, 10, 12, 18, 4]
                double AmountInMonth = data.Where(x => DateTime.Parse(x.GetType().GetProperty(datePropertyName).GetValue(x).ToString()).Year == year && DateTime.Parse(x.GetType().GetProperty(datePropertyName).GetValue(x).ToString()).Month == month).Select(x => double.Parse(x.GetType().GetProperty(amountPropertyName).GetValue(x).ToString())).Sum();
                yearlyData.Add(AmountInMonth);
            }

            return yearlyData;
        }

        #endregion Index



        #region GeneralBalance
        public async Task<IActionResult> GeneralBalance(int? Year)
        {
            var VM = new GeneralBalanceVM();
            Year = Year == 0000 ? null : Year;
            var ReceivePayments = await _context.MosbReceivePayments
                   .Include(x => x.Name)
                   .ToListAsync();

            var SpendMoney = await _context.MosbSpendMoney
                .Include(x => x.Person)
                .Include(x => x.Reasons)
                .ToListAsync();


            int? year = Year.HasValue ? Year.Value : null;
            List<SelectListModel> AllYears = new();
            AddYearsToListModel(ReceivePayments, ref AllYears, x => DateTime.Parse(x.Date).Year, year);
            AddYearsToListModel(SpendMoney, ref AllYears, x => DateTime.Parse(x.Date).Year, year);
            VM.YearsList = AllYears.GroupBy(x => x.Value).Select(x => x.First()).ToList();


            if (Year.HasValue)
            {
                ReceivePayments = ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == Year).ToList();
                SpendMoney = SpendMoney.Where(x => DateTime.Parse(x.Date).Year == Year).ToList();
            }


            VM.AllReceivePaymentsAmount = Math.Round(ReceivePayments.Sum(x => x.Amount), 4);
            VM.AllSpendMoneyAmount = Math.Round(SpendMoney.Sum(x => x.Amount), 4);
            VM.GeneralBalance = Math.Round(VM.AllReceivePaymentsAmount - VM.AllSpendMoneyAmount, 4);
            VM.Zakat = Math.Round((VM.AllReceivePaymentsAmount - VM.AllSpendMoneyAmount) * 0.025, 4);





            return View(VM);
        }
        #endregion GeneralBalance


        #region PersonalBalance
        public async Task<IActionResult> PersonalBalance(long? NameId, int? Year)
        {
            PersonalBalanceVM VM = new();

            Year = Year == 0000 ? null : Year;
            var ReceivePayments = await _context.MosbReceivePayments
                   .Include(x => x.Name)
                   .ToListAsync();

            var SpendMoney = await _context.MosbSpendMoney
                .Include(x => x.Person)
                .Include(x => x.Reasons)
                .ToListAsync();

            var Names = await _context
                .MosbName
                .ToListAsync();



            int? year = Year.HasValue ? Year : null;

            List<SelectListModel> AllYears = new();
            AddYearsToListModel(ReceivePayments, ref AllYears, x => DateTime.Parse(x.Date).Year, year);
            AddYearsToListModel(SpendMoney, ref AllYears, x => DateTime.Parse(x.Date).Year, year);
            VM.YearsList = AllYears.GroupBy(x => x.Value).Select(x => x.First()).ToList();


            if (Year.HasValue)
            {
                ReceivePayments = ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == Year).ToList();
                SpendMoney = SpendMoney.Where(x => DateTime.Parse(x.Date).Year == Year).ToList();
            }


            VM.NamesList = Names.Select(x => new SelectListModel { Text = x.Name, Value = x.Id, IsSelected = x.Id == NameId }).ToList();


            VM.NamesList = Names.Select(x => new SelectListModel { Text = x.Name, Value = x.Id, IsSelected = x.Id == NameId }).ToList();

            VM.PersonalBalanceIsAvailable = false;

            if (NameId.HasValue)
            {
                VM.PersonalBalanceIsAvailable = true;
                VM.PersonalReceivePayment = Math.Round(ReceivePayments.Where(x => x.NameId == NameId && (Year == null || DateTime.Parse(x.Date).Year == Year)).Sum(x => x.Amount), 4);
                VM.PersonalSpendMoney = Math.Round(SpendMoney.Where(x => x.PersonId == NameId
                && (Year == null || DateTime.Parse(x.Date).Year == Year)).Sum(x => x.Amount), 4);
                VM.PersonalTotalAmount = Math.Round(VM.PersonalReceivePayment - VM.PersonalSpendMoney, 4);
                VM.Zakat = Math.Round((VM.PersonalReceivePayment - VM.PersonalSpendMoney) * 0.025, 4);
                VM.Name = Names.FirstOrDefault(x => x.Id == NameId)?.Name;
                VM.Year = Year;

            }


            return View(VM);
        }


        #endregion PersonalBalance

        #region Semple
        public async Task<IActionResult> Semple(SempleSearchModel model)
        {
            try
            {
                var VM = new SempleVM();

                var ReceivePayments = await _context.MosbReceivePayments
                    .Include(x => x.Name)
                    .ToListAsync();

                var SpendMoney = await _context.MosbSpendMoney
                    .Include(x => x.Person)
                    .Include(x => x.Reasons)
                    .ToListAsync();


                var Names = await _context.MosbName.ToListAsync();

                VM.AllReceivePaymentsAmount = Math.Round(ReceivePayments.Sum(x => x.Amount), 4);
                VM.AllSpendMoneyAmount = Math.Round(SpendMoney.Sum(x => x.Amount), 4);
                VM.GeneralBalance = Math.Round(VM.AllReceivePaymentsAmount - VM.AllSpendMoneyAmount, 4);



                int? year = model.StatisticsYear.HasValue ? model.StatisticsYear.Value : model.PersonalYear;
                List<SelectListModel> AllYears = new();
                AddYearsToListModel(ReceivePayments, ref AllYears, x => DateTime.Parse(x.Date).Year, year);
                AddYearsToListModel(SpendMoney, ref AllYears, x => DateTime.Parse(x.Date).Year, year);
                VM.YearsList = AllYears.GroupBy(x => x.Value).Select(x => x.First()).ToList();

                VM.YearIsAvailable = false;
                if (model.StatisticsYear.HasValue)
                {
                    var YearlyDateTime = new DateTime(model.StatisticsYear.Value, 1, 1);
                    VM.SelectedYear = model.StatisticsYear.Value;
                    VM.YearIsAvailable = true;
                    VM.SpendMoneyYearlyData = Math.Round(SpendMoney.Sum(x => x.Amount), 4);
                    VM.ReceivePaymentsYearlyData = Math.Round(ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == YearlyDateTime.Year).Sum(x => x.Amount), 3);
                    VM.TotalAmountYearlyData = Math.Round(VM.ReceivePaymentsYearlyData - VM.SpendMoneyYearlyData, 4);
                }


                VM.PersonsBalance = Names.Select(x => new ShowPersonsTotal
                {
                    PhoneNumber = x.PhoneNumber,
                    Name = x.Name,
                    ReceivePayment = Math.Round(x.MosbReceivePayments.Sum(x => x.Amount), 4),
                    SpendMoney = Math.Round(x.MosbSpendMoney.Sum(x => x.Amount), 4),
                    TotalAmount = Math.Round((x.MosbReceivePayments.Sum(x => x.Amount) - x.MosbSpendMoney.Sum(x => x.Amount)), 4),
                    LastActivity = GetLastActivity(ReceivePayments, SpendMoney, x.Id)
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
        private static (string? LastReceivePayment, string? LastSpendMoney) GetLastActivity(List<MosbReceivePayments> ReceivePayments, List<MosbSpendMoney> SpendMoney, long NameId)
        {
            var LastReceivePayment = ReceivePayments.Where(x => x.NameId == NameId).OrderByDescending(x => x.Date).FirstOrDefault()?.Date;
            var LastSpendMoney = SpendMoney.Where(x => x.PersonId == NameId).OrderByDescending(x => x.Date).FirstOrDefault()?.Date;
            // 2022-2 foramt
            LastReceivePayment = LastReceivePayment?.Substring(0, LastReceivePayment.Length - 3);
            LastSpendMoney = LastSpendMoney?.Substring(0, LastSpendMoney.Length - 3);



            return (LastReceivePayment, LastSpendMoney);
        }

        #endregion Semple


        public IActionResult DawnloadDB()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "AlMohaseb-ERD.db");
            // copy as backup in 'DB Backup' folder
            var backupPath = Path.Combine(Directory.GetCurrentDirectory(), "DB Backup", $"AlMohaseb-ERD-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.db");
            System.IO.File.Copy(path, backupPath);
            // download the backup db file
            return PhysicalFile(backupPath, "application/octet-stream", Path.GetFileName(backupPath));
        }

        public async Task<IActionResult> DawnloadAllDataAsPDF()
        {
            DawnloadAllDataAsPDFVM VM = new();

            VM.Reasons = await _context.MosbReasons.Select(r => new ReasonsTable
            {
                Id = r.Id,
                Name = r.Name,
                Date = r.Date,
                Amount = r.Amount
            }).OrderByDescending(r => r.Id).ToListAsync();

            var ReceivePayments = await _context.MosbReceivePayments
                    .Include(x => x.Name)
                    .ToListAsync();

            var SpendMoney = await _context.MosbSpendMoney
                .Include(x => x.Person)
                .Include(x => x.Reasons)
                .ToListAsync();

            var Names = await _context.MosbName.ToListAsync();

            VM.AllReceivePaymentsAmount = Math.Round(ReceivePayments.Sum(x => x.Amount), 4);
            VM.AllSpendMoneyAmount = Math.Round(SpendMoney.Sum(x => x.Amount), 4);
            VM.GeneralBalance = Math.Round(VM.AllReceivePaymentsAmount - VM.AllSpendMoneyAmount, 4);


            List<SelectListModel> AllYears = new();
            AddYearsToListModel(ReceivePayments, ref AllYears, x => DateTime.Parse(x.Date).Year, null);
            AddYearsToListModel(SpendMoney, ref AllYears, x => DateTime.Parse(x.Date).Year, null);
            var YearsList = AllYears.GroupBy(x => x.Value).Select(x => x.First()).ToList();

            VM.AllBalanceForEveryYear = YearsList.Select(year => new YearlyBalanceDTO
            {
                Year = year.Text,
                ReceivePayments = Math.Round(ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount), 4),
                SpendMoney = Math.Round(SpendMoney.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount), 4),
                TotalAmount = Math.Round(ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount) - SpendMoney.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount), 4)
            }).ToList();

            VM.PersonsBalance = Names.Select(x => new ShowPersonsTotal
            {
                PhoneNumber = x.PhoneNumber,
                Name = x.Name,
                ReceivePayment = Math.Round(x.MosbReceivePayments.Sum(x => x.Amount), 4),
                SpendMoney = Math.Round(x.MosbSpendMoney.Sum(x => x.Amount), 4),
                TotalAmount = Math.Round((x.MosbReceivePayments.Sum(x => x.Amount) - x.MosbSpendMoney.Sum(x => x.Amount)), 4),
                LastActivity = GetLastActivity(ReceivePayments, SpendMoney, x.Id)
            }).ToList();

            return View(VM);
        }

        // using CloseXML package
        public async Task<IActionResult> DownloadAllDataAsExcel()
        {
            var Reasons = await _context.MosbReasons.Select(r => new ReasonsTable
            {
                Id = r.Id,
                Name = r.Name,
                Date = r.Date,
                Amount = r.Amount
            }).OrderByDescending(r => r.Id).ToListAsync();

            var ReceivePayments = await _context.MosbReceivePayments
                    .Include(x => x.Name)
                    .Where(x => x.IsPaid == true.GetHashCode())
                    .ToListAsync();

            var SpendMoney = await _context.MosbSpendMoney
                .Include(x => x.Person)
                .Include(x => x.Reasons)
                .Where(x => x.IsPaid == true.GetHashCode())
                .ToListAsync();

            var Names = await _context.MosbName.ToListAsync();

            var AllReceivePaymentsAmount = Math.Round(ReceivePayments.Sum(x => x.Amount), 4);
            var AllSpendMoneyAmount = Math.Round(SpendMoney.Sum(x => x.Amount), 4);
            var GeneralBalance = Math.Round(AllReceivePaymentsAmount - AllSpendMoneyAmount, 4);


            List<SelectListModel> AllYears = new();
            AddYearsToListModel(ReceivePayments, ref AllYears, x => DateTime.Parse(x.Date).Year, null);
            AddYearsToListModel(SpendMoney, ref AllYears, x => DateTime.Parse(x.Date).Year, null);
            var YearsList = AllYears.GroupBy(x => x.Value).Select(x => x.First()).ToList();

            var AllBalanceForEveryYear = YearsList.Select(year => new YearlyBalanceDTO
            {
                Year = year.Text,
                ReceivePayments = Math.Round(ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount), 4),
                SpendMoney = Math.Round(SpendMoney.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount), 4),
                TotalAmount = Math.Round(ReceivePayments.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount) - SpendMoney.Where(x => DateTime.Parse(x.Date).Year == year.Value).Sum(x => x.Amount), 4)
            }).ToList();

            var PersonsBalance = Names.Select(x => new ShowPersonsTotal
            {
                PhoneNumber = x.PhoneNumber,
                Name = x.Name,
                ReceivePayment = Math.Round(x.MosbReceivePayments.Sum(x => x.Amount), 4),
                SpendMoney = Math.Round(x.MosbSpendMoney.Sum(x => x.Amount), 4),
                TotalAmount = Math.Round((x.MosbReceivePayments.Sum(x => x.Amount) - x.MosbSpendMoney.Sum(x => x.Amount)), 4),
                LastActivity = GetLastActivity(ReceivePayments, SpendMoney, x.Id)
            }).ToList();





            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("الرصيد العام");

            //worksheet right to left
            worksheet.RightToLeft = true;

            worksheet.Cell("A1").Value = "الرصيد العام";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 20;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Range("A1:B1").Merge();

            worksheet.Cell("A2").Value = "الرصيد الكلي للمدفوعات";
            worksheet.Cell("B2").Value = AllReceivePaymentsAmount;

            worksheet.Cell("A3").Value = "الرصيد الكلي للمصروفات";
            worksheet.Cell("B3").Value = AllSpendMoneyAmount;

            worksheet.Cell("A4").Value = "الرصيد العام";
            worksheet.Cell("B4").Value = GeneralBalance;

            worksheet.Columns().AdjustToContents();

            var worksheet2 = workbook.Worksheets.Add("الرصيد السنوي");

            //worksheet right to left
            worksheet2.RightToLeft = true;

            worksheet2.Cell("A1").Value = "الرصيد السنوي";
            worksheet2.Cell("A1").Style.Font.Bold = true;
            worksheet2.Cell("A1").Style.Font.FontSize = 20;
            worksheet2.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet2.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet2.Range("A1:D1").Merge();

            worksheet2.Cell("A2").Value = "السنة";
            worksheet2.Cell("B2").Value = "المدفوعات";
            worksheet2.Cell("C2").Value = "المصروفات";
            worksheet2.Cell("D2").Value = "الرصيد السنوي";

            for (int i = 0; i < AllBalanceForEveryYear.Count; i++)
            {
                worksheet2.Cell(i + 3, 1).Value = AllBalanceForEveryYear[i].Year;
                worksheet2.Cell(i + 3, 2).Value = AllBalanceForEveryYear[i].ReceivePayments;
                worksheet2.Cell(i + 3, 3).Value = AllBalanceForEveryYear[i].SpendMoney;
                worksheet2.Cell(i + 3, 4).Value = AllBalanceForEveryYear[i].TotalAmount;
            }

            // create table for the data
            var table = worksheet2.Range("A2:D" + (AllBalanceForEveryYear.Count + 2)).CreateTable();
            table.ShowAutoFilter = true;
            table.Theme = XLTableTheme.TableStyleMedium2;

            worksheet2.Columns().AdjustToContents();

            var worksheet3 = workbook.Worksheets.Add("بيانات الاساسية للأعضاء");

            //worksheet right to left
            worksheet3.RightToLeft = true;

            worksheet3.Cell("A1").Value = "رصيد الأعضاء المنتسبين";
            worksheet3.Cell("A1").Style.Font.Bold = true;
            worksheet3.Cell("A1").Style.Font.FontSize = 20;
            worksheet3.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet3.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet3.Range("A1:E1").Merge();

            worksheet3.Cell("A2").Value = "الاسم";
            worksheet3.Cell("B2").Value = "رقم الهاتف";
            worksheet3.Cell("C2").Value = "المدفوعات";
            worksheet3.Cell("D2").Value = "المصروفات";
            worksheet3.Cell("E2").Value = "الرصيد الكلي";

            for (int i = 0; i < PersonsBalance.Count; i++)
            {
                worksheet3.Cell(i + 3, 1).Value = PersonsBalance[i].Name;
                worksheet3.Cell(i + 3, 2).Value = PersonsBalance[i].PhoneNumber;
                worksheet3.Cell(i + 3, 3).Value = PersonsBalance[i].ReceivePayment;
                worksheet3.Cell(i + 3, 4).Value = PersonsBalance[i].SpendMoney;
                worksheet3.Cell(i + 3, 5).Value = PersonsBalance[i].TotalAmount;
            }

            // create table for the data
            var table1 = worksheet3.Range("A2:E" + (PersonsBalance.Count + 2)).CreateTable();
            table1.ShowAutoFilter = true;
            table1.Theme = XLTableTheme.TableStyleMedium2;

            worksheet3.Columns().AdjustToContents();

            var worksheet4 = workbook.Worksheets.Add("جميع الأسباب");

            //worksheet right to left
            worksheet4.RightToLeft = true;

            worksheet4.Cell("A1").Value = "جميع الأسباب";
            worksheet4.Cell("A1").Style.Font.Bold = true;
            worksheet4.Cell("A1").Style.Font.FontSize = 20;
            worksheet4.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet4.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet4.Range("A1:D1").Merge();

            worksheet4.Cell("A2").Value = "الاسم";
            worksheet4.Cell("B2").Value = "التاريخ";
            worksheet4.Cell("C2").Value = "المبلغ";

            for (int i = 0; i < Reasons.Count; i++)
            {
                worksheet4.Cell(i + 3, 1).Value = Reasons[i].Name;
                worksheet4.Cell(i + 3, 2).Value = Reasons[i].Date;
                worksheet4.Cell(i + 3, 3).Value = Reasons[i].Amount;
            }
            // create table for the data
            var table2 = worksheet4.Range("A2:C" + (Reasons.Count + 2)).CreateTable();
            table2.ShowAutoFilter = true;
            table2.Theme = XLTableTheme.TableStyleMedium2;

            worksheet4.Columns().AdjustToContents();

            var worksheet5 = workbook.Worksheets.Add("جميع الايداعات");

            //worksheet right to left
            worksheet5.RightToLeft = true;

            worksheet5.Cell("A1").Value = "جميع الايداعات";
            worksheet5.Cell("A1").Style.Font.Bold = true;
            worksheet5.Cell("A1").Style.Font.FontSize = 20;
            worksheet5.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet5.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet5.Range("A1:C1").Merge();

            worksheet5.Cell("A2").Value = "الاسم";
            worksheet5.Cell("B2").Value = "التاريخ الايداع";
            worksheet5.Cell("C2").Value = "المبلغ الايداع";
            worksheet5.Cell("D2").Value = "السبب";

            for (int i = 0; i < ReceivePayments.Count; i++)
            {
                worksheet5.Cell(i + 3, 1).Value = ReceivePayments[i].Name.Name;
                worksheet5.Cell(i + 3, 2).Value = ReceivePayments[i].Date;
                worksheet5.Cell(i + 3, 3).Value = ReceivePayments[i].Amount;
                worksheet5.Cell(i + 3, 4).Value = ReceivePayments[i].Description;
            }
            // create table for the data
            var table3 = worksheet5.Range("A2:D" + (ReceivePayments.Count + 2)).CreateTable();
            table3.ShowAutoFilter = true;
            table3.Theme = XLTableTheme.TableStyleMedium2;

            worksheet5.Columns().AdjustToContents();

            var worksheet6 = workbook.Worksheets.Add("جميع المصروفات");

            //worksheet right to left
            worksheet6.RightToLeft = true;

            worksheet6.Cell("A1").Value = "جميع المصروفات";
            worksheet6.Cell("A1").Style.Font.Bold = true;
            worksheet6.Cell("A1").Style.Font.FontSize = 20;
            worksheet6.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet6.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet6.Range("A1:C1").Merge();

            worksheet6.Cell("A2").Value = "الاسم";
            worksheet6.Cell("B2").Value = "التاريخ الصرف";
            worksheet6.Cell("C2").Value = "مبلغ الصرف";
            worksheet6.Cell("D2").Value = "السبب";

            for (int i = 0; i < SpendMoney.Count; i++)
            {
                worksheet6.Cell(i + 3, 1).Value = SpendMoney[i].Person.Name;
                worksheet6.Cell(i + 3, 2).Value = SpendMoney[i].Date;
                worksheet6.Cell(i + 3, 3).Value = SpendMoney[i].Amount;
                worksheet6.Cell(i + 3, 4).Value = SpendMoney[i].Description;
            }
            // create table for the data
            var table4 = worksheet6.Range("A2:D" + (SpendMoney.Count + 2)).CreateTable();
            table4.ShowAutoFilter = true;
            table4.Theme = XLTableTheme.TableStyleMedium2;

            worksheet6.Columns().AdjustToContents();



            var fileTitle = $"بيانات النطام - {DateTime.Now.ToString("yyyy-MM-dd-(HH-mm-ss)")}.xlsx";
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileTitle);
            }


        }


    }
}
