using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Portal.Controllers
{
    public class CreateController : Controller
    {
        private readonly ILogger<CreateController> _logger;
        private readonly AlMohasebDBContext _context;
        readonly string UserPassword = string.Empty;
        private readonly IConfiguration _configuration;

        public CreateController(ILogger<CreateController> logger, AlMohasebDBContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            string? FromAppSettings = _configuration.GetSection("AppSettings:UserPassword").Value;
            if (FromAppSettings != null)
            {
                UserPassword = FromAppSettings;
            }
            else
            {
                UserPassword = "Qwert@123";
            }
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Names()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Names(NameVM VM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingCivilNumber = await _context.MosbName.AnyAsync(x => x.CivilNumber == VM.CivilNumber);
                    if (existingCivilNumber)
                    {
                        ModelState.AddModelError("CivilNumber", "هذا الرقم المدني موجود بالفعل");
                        return View();
                    }

                    var existingPhone = await _context.MosbName.AnyAsync(x => x.PhoneNumber == VM.Phone);
                    if (existingPhone)
                    {
                        ModelState.AddModelError("Phone", "هذا الرقم موجود بالفعل");
                        return View();
                    }

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    var newMosbName = new MosbName
                    {
                        Name = VM.Name,
                        PhoneNumber = VM.Phone,
                        CivilNumber = VM.CivilNumber,
                        RegisterDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        SubscriptionAmount = VM.SubscriptionAmount
                    };

                     await _context.MosbName.AddAsync(newMosbName);

                    var saveChangesResult = await _context.SaveChangesAsync();
                    if (saveChangesResult == 0)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "حدث خطأ عام");
                    }

                    await transaction.CommitAsync();
                    TempData["AddStatus"] = "Success";
                    return RedirectToAction("Names", "List");
                }
                else
                {
                    TempData["AddStatus"] = "Error";
                    ModelState.AddModelError("", "حدث خطأ عام");
                    return View();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ActionResult> CreateReason(string Reason, double Amount)
        {
            var reason = new MosbReasons
            {
                Name = Reason,
                Amount = Amount,
                Date = DateTime.Now.ToString("yyyy-MM-dd")
            };
            await _context.MosbReasons.AddAsync(reason);
            int add = await _context.SaveChangesAsync();
            if (add == 0)
            {
                return BadRequest();
            }
            return Ok(add);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteReason(long Id, string Password)
        {
            if (Password != UserPassword)
            {
                return Ok(2);
            }
            var reason = await _context.MosbReasons.FindAsync(Id);
            if (reason == null)
            {
                return NotFound();
            }
            _context.MosbReasons.Remove(reason);
            int delete = await _context.SaveChangesAsync();
            if (delete == 0)
            {
                return BadRequest(3);
            }
            return Ok(delete);
        }


        public IActionResult ReceivePayments(long? ReasonId = null)
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ReceivePayments(ReceivePaymentsVM VM)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    using var transaction = await _context.Database.BeginTransactionAsync();

                    var newReceivePayments = new MosbReceivePayments
                    {
                        NameId = VM.NameId,
                        Date = VM.Date,
                        Amount = VM.Amount,
                        Description = VM.Description,
                    };

                    var AddedRePayment = await _context.MosbReceivePayments.AddAsync(newReceivePayments);
                    var saveChangesResult = await _context.SaveChangesAsync();
                    if (saveChangesResult == 0)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "حدث خطأ عام");
                    }

                    await transaction.CommitAsync();
                    TempData["AddStatus"] = "Success";
                    return RedirectToAction("ReceivePayments", "List");
                }
                else
                {
                    TempData["AddStatus"] = "Error";
                    ModelState.AddModelError("", "حدث خطأ عام");
                    return View();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        #region SpendMoney
        public IActionResult SpendMoney()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SpendMoney(SpendMoneyVM VM)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    var newSpendMoney = new MosbSpendMoney
                    {
                        PersonId = VM.NameId,
                        Date = VM.Date,
                        Amount = VM.Amount,
                        Description = VM.Description,
                        IsForReason = false.GetHashCode(),
                        IsPaid = true.GetHashCode(),
                    };

                    var AddedSpendMoney = await _context.MosbSpendMoney.AddAsync(newSpendMoney);

                    var saveChangesResult = await _context.SaveChangesAsync();
                    if (saveChangesResult == 0)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "حدث خطأ عام");
                    }

                    await transaction.CommitAsync();
                    TempData["AddStatus"] = "Success";
                    return RedirectToAction("SpendMoney", "List");
                }
                else
                {
                    TempData["AddStatus"] = "Error";
                    ModelState.AddModelError("", "حدث خطأ عام");
                    return View();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion SpendMoney

        #region SpendMoneyForReason
        public async Task<IActionResult> SpendMoneyForReason(long? ReasonId = null, string? YearMonth = null)

        {
            if (ReasonId == null || YearMonth == null)
            {
                return View(new SpendMoneyForReasonVM());
            }
            // YearMonth format Y-M (23-01) parse to DateTime
            var YearMonthDate = DateTime.Parse(YearMonth);

            var spendMoneyForReason = await _context.MosbSpendMoney.Include(x => x.Person)
                .Where(x => x.ReasonsId == ReasonId 
                && x.IsForReason == true.GetHashCode()
                && x.Date == YearMonthDate.ToString("yyyy-MM-dd")
                )
                .ToListAsync();

            var VM = new SpendMoneyForReasonVM
            {
                ReasonId = ReasonId.Value,
                YearMonth = YearMonth,
                IsSelected = true,
                AmountSubscribed = await _context.MosbReasons.Where(x => x.Id == ReasonId).Select(x => x.Amount).FirstOrDefaultAsync(),
                AllPersonInSystem = new List<ShowSpendMoney>(),
                SpendMoneySubmitedAmount = new List<ShowSpendMoney>()
            };

            if (spendMoneyForReason.Count > 0)
            {
                VM.SpendMoneySubmitedAmount = spendMoneyForReason
                    .Select(x => new ShowSpendMoney
                    {
                        SpendMoneyId = x.Id,
                        PersonId = x.Person.Id,
                        PersonName = x.Person.Name,
                        Amount = Math.Round(x.Amount, 2)
                    })
                    .ToList();
                VM.IsHasRecodes = true;

            }
            else
            {
                VM.AllPersonInSystem  = await _context.MosbName
                    .Select(x => new ShowSpendMoney
                    {
                        PersonId = x.Id,
                        PersonName = x.Name,
                        Amount = 0
                    })
                    .ToListAsync();
            }

            return View(VM);
        }
        [HttpPost]
        public async Task<IActionResult> ModifySpendMoneyForReasonAjax(SpendMoneyForReasonRecord record)
        {
            var YearMonthDate = DateTime.Parse(record.MonthYear);

            var spendMoneyForReason = await _context.MosbSpendMoney.Include(x => x.Person)
                .Where(x => x.ReasonsId == record.ReasonId && x.Date == YearMonthDate.ToString("yyyy-MM-dd") && x.IsForReason == true.GetHashCode())
                .ToListAsync();

            bool ModifyStatus = false;
            if (spendMoneyForReason.Count > 0)
            {
                ModifyStatus = await EditRecordOnSpendMoneyForReason(record);
            }
            else
            {
                List<SpendMoneyForReasonDTO> DTO = record.PersonIdAmount.Select(x => new SpendMoneyForReasonDTO
                {
                    PersonId = x.PersonId,
                    ReasonsId = record.ReasonId,
                    Date = YearMonthDate.ToString("yyyy-MM-dd"),
                    Amount = x.IsPaid ? x.Amount : 0,
                    IsPaid = x.IsPaid
                }).ToList();
                ModifyStatus = await NewRecordOnSpendMoneyForReason(DTO);
            }
            if (ModifyStatus)
            {
                return Json(new
                {
                    status = true,
                    message = "تم أجراء التعديل بنجاح"
                });
            }
            else
            {
                return Json(new
                {
                    status = false,
                    message = "حدث خطأ أثناء تعديل البيانات"
                });
            }


        }
        public async Task<bool> NewRecordOnSpendMoneyForReason(List<SpendMoneyForReasonDTO> DTO)
        {
            try
            {
                _context.Database.BeginTransaction();
                foreach (var item in DTO)
                {
                    item.Amount = Math.Round(item.Amount, 2);
                    var newRecord = new MosbSpendMoney
                    {
                        PersonId = item.PersonId,
                        ReasonsId = item.ReasonsId,
                        Date = item.Date,
                        Amount = Math.Round(item.Amount, 2),
                        IsPaid = item.IsPaid.GetHashCode(),
                        IsForReason = true.GetHashCode(),
                        Description = string.Concat("تم صرف مبلغ ", Math.Round(item.Amount, 2), " لـ ", _context.MosbName.Where(x => x.Id == item.PersonId).Select(x => x.Name).FirstOrDefault(), " لسبب ", _context.MosbReasons.Where(x => x.Id == item.ReasonsId).Select(x => x.Name).FirstOrDefault(), " بتاريخ ", item.Date),
                    };
                    await _context.MosbSpendMoney.AddAsync(newRecord);
                    
                }
                var status = await _context.SaveChangesAsync();
                if (status == 0)
                {
                    _context.Database.RollbackTransaction();
                    return false;
                }
                await _context.Database.CommitTransactionAsync();
                return true;

            }
            catch
            {
                _context.Database.RollbackTransaction();
                return false;
            }
        }
        public async Task<bool> EditRecordOnSpendMoneyForReason(SpendMoneyForReasonRecord record)
        {
            try
            {
                _context.Database.BeginTransaction();
                foreach (var item in record.PersonIdAmount)
                {
                    var YearMonthDate = DateTime.Parse(record.MonthYear);

                    var recordToEdit = await _context.MosbSpendMoney
                        .Where(x => x.ReasonsId == record.ReasonId && x.PersonId == item.PersonId && x.Date == YearMonthDate.ToString("yyyy-MM-dd") && x.IsForReason == true.GetHashCode())
                        .FirstOrDefaultAsync();

                    if (recordToEdit == null)
                    {
                        return false;
                    }

                    recordToEdit.Amount = item.IsPaid ? item.Amount : 0;
                    recordToEdit.IsPaid = item.IsPaid.GetHashCode();

                    _context.MosbSpendMoney.Update(recordToEdit);
                    await _context.SaveChangesAsync();
                }

                await _context.Database.CommitTransactionAsync();
                return true;

            }
            catch
            {
                _context.Database.RollbackTransaction();
                return false;
            }


        }


        #endregion SpendMoneyForReason


        #region MonthlyReceivePayments
        public async Task<IActionResult> MonthlyReceivePayments(string? YearMonth = null)
        {
            if (YearMonth == null)
            {
                return View(new MonthlyReceivePaymentsVM());
            }
            // YearMonth format Y-M (23-01) parse to DateTime
            var YearMonthDate = DateTime.Parse(YearMonth);

            var monthlyReceivePayments = await _context.MosbReceivePayments.Include(x => x.Name)
                .Where(x => x.Date == YearMonthDate.ToString("yyyy-MM-dd") && x.IsMonthly == true.GetHashCode()) 
                .ToListAsync();

            var VM = new MonthlyReceivePaymentsVM
            {
                YearMonth = YearMonth,
                IsSelected = true,
                IsHasRecodes = false
            };

            if (monthlyReceivePayments.Any())
            {
                VM.SumitedMonthlyPayments = monthlyReceivePayments
                    .Select(x => new ShowMonthlyReceivePayments
                    {
                        MonthlyReceivePaymentsId = x.Id,
                        Amount = Math.Round(x.Amount, 2),
                        PersonName = x.Name.Name,
                        PersonId = x.NameId,
                        IsPaid = x.IsPaid == 1 ? true : false
                    }).ToList();
                VM.IsHasRecodes = true;
            }
            else
            {
                VM.SumitedMonthlyPayments = await _context.MosbName
                    .Select(x => new ShowMonthlyReceivePayments
                    {
                        PersonId = x.Id,
                        PersonName = x.Name,
                        Amount = Math.Round(x.SubscriptionAmount, 2),
                        IsPaid = false
                    })
                    .ToListAsync();
                VM.IsHasRecodes = false;
            }

            return View(VM);
        }
        public async Task<IActionResult> ModifyMonthlyReceiveRecordAjax(MonthlyReceiveRecord record)
        {
            if (record is null)
            {
                return Json(new
                {
                    status = false,
                    message = "حدث خطأ أثناء تعديل البيانات"
                });
            }
            // YearMonth format Y-M ( 2023-01) parse to DateTime
            var YearMonthDate = DateTime.Parse(record.YearMonth);

            var monthlyReceivePayments = await _context.MosbReceivePayments.Include(x => x.Name)
                .Where(x => x.Date == YearMonthDate.ToString("yyyy-MM-dd") && x.IsMonthly == true.GetHashCode())
                .ToListAsync();

            bool ModifyStatus = false;

            if (monthlyReceivePayments.Any())
            {
                ModifyStatus = await EditRecordOnMonthlyReceivePayments(record);
            }
            else
            {
                var DTO = record.MonthlyPaidData.Select(x => new PersonIdAmount
                {
                    PersonId = x.PersonId,
                    Amount = x.IsPaid ? Math.Round(x.Amount, 2) : 0,
                    IsPaid = x.IsPaid
                }).ToList();

                ModifyStatus = await NewRecordOnMonthlyReceivePayments(DTO, YearMonthDate.ToString("yyyy-MM-dd"));
            }
            if (ModifyStatus)
            {
                return Json(new
                {
                    status = true,
                    message = "تم أجراء التعديل بنجاح"
                });
            }
            else
            {
                return Json(new
                {
                    status = false,
                    message = "حدث خطأ أثناء تعديل البيانات"
                });
            }

        }
        private async Task<bool> NewRecordOnMonthlyReceivePayments(List<PersonIdAmount> DTO, string YearMonthDate)
        {
            try
            {
                _context.Database.BeginTransaction();
                foreach (var item in DTO)
                {
                    var newRecord = new MosbReceivePayments
                    {
                        NameId = item.PersonId,
                        Date = YearMonthDate,
                        Amount = Math.Round(item.Amount, 2),
                        IsPaid = item.IsPaid.GetHashCode(),
                        IsMonthly = true.GetHashCode(),
                        Description = item.IsPaid ? "تم دفع مبلغ الاشتراك الشهري" : "لم يتم دفع مبلغ الاشتراك الشهري من قبل الشخص"
                    };
                    await _context.MosbReceivePayments.AddAsync(newRecord);
                }
                int status = await _context.SaveChangesAsync();
                if (status <= 0)
                {
                    return false;
                }
                await _context.Database.CommitTransactionAsync();
                return true;

            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                return false;
            }

        }
        private async Task<bool> EditRecordOnMonthlyReceivePayments(MonthlyReceiveRecord record)
        {
            try
            {
                _context.Database.BeginTransaction();
                foreach (var item in record.MonthlyPaidData)
                {
                    var YearMonthDate = DateTime.Parse(record.YearMonth);

                    var recordToEdit = await _context.MosbReceivePayments
                        .Where(x => x.NameId == item.PersonId && x.Date == YearMonthDate.ToString("yyyy-MM-dd") && x.IsMonthly == true.GetHashCode())
                        .SingleAsync();

                    if (recordToEdit == null)
                    {
                        return false;
                    }

                    recordToEdit.Amount = item.IsPaid ? Math.Round(item.Amount, 2) : 0;
                    recordToEdit.IsPaid = item.IsPaid.GetHashCode();

                    _context.MosbReceivePayments.Update(recordToEdit);
                    await _context.SaveChangesAsync();
                }

                await _context.Database.CommitTransactionAsync();
                return true;

            }
            catch
            {
                _context.Database.RollbackTransaction();
                return false;
            }
        }
        #endregion MonthlyReceivePayments
    }
}