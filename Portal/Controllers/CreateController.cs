using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Portal.Controllers
{
    [Authorize]
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

        #region Name
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
                        return View(VM);
                    }

                    var existingPhone = await _context.MosbName.AnyAsync(x => x.PhoneNumber == VM.Phone);
                    if (existingPhone)
                    {
                        ModelState.AddModelError("Phone", "هذا الرقم موجود بالفعل");
                        return View(VM);
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
                        return View(VM);
                    }

                    await transaction.CommitAsync();
                    return RedirectToAction("Names", "List", new { add = true });
                }
                else
                {
                    ModelState.AddModelError("", "حدث خطأ عام");
                    return View(VM);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion Name

        #region Reason
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
            return RedirectToAction("Reasons", "List", new { add = add == 1 });
        }

        [HttpPost]
        public async Task<IActionResult> EditReason(long Id, string Reason, double Amount)
        {
            var reason = await _context.MosbReasons.FindAsync(Id);
            if (reason == null)
            {
                return NotFound(0);
            }
            if (reason.Name != Reason) reason.Name = Reason;
            if (reason.Amount != Amount) reason.Amount = Amount;

            _context.MosbReasons.Update(reason);
            int edit = await _context.SaveChangesAsync();
            if (edit == 0)
            {
                return NotFound(0);
            }
            return Ok(1);
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
                return NotFound(0);
            }
            return Ok(1);
        }
    
        #endregion Reason

        #region ReceivePayments
        public IActionResult ReceivePayments(long? ReasonId = null)
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ReceivePayments(CreateReceivePaymentsVM VM)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var newReceivePayments = new MosbReceivePayments
                    {
                        NameId = VM.NameId,
                        Date = VM.Date,
                        Amount = VM.Amount,
                        Description = VM.Description,
                        IsMonthly = false.GetHashCode(),
                        IsPaid = true.GetHashCode(),
                        IsTransaction = false.GetHashCode()
                    };

                    await _context.MosbReceivePayments.AddAsync(newReceivePayments);
                    var saveChangesResult = await _context.SaveChangesAsync();
                    
                    return RedirectToAction("ReceivePayments", "List", new { add = saveChangesResult == 1 });
                }
                else
                {
                    ModelState.AddModelError("", "حدث خطأ عام");
                    return View(VM);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion ReceivePayments

        #region SpendMoney
        public IActionResult SpendMoney()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SpendMoney(CreateSpendMoneyVM VM)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var newSpendMoney = new MosbSpendMoney
                    {
                        PersonId = VM.NameId,
                        Date = VM.Date,
                        Amount = VM.Amount,
                        Description = VM.Description,
                        IsForReason = false.GetHashCode(),
                        IsPaid = true.GetHashCode(),
                        IsTransaction = false.GetHashCode()
                    };

                    await _context.MosbSpendMoney.AddAsync(newSpendMoney);
                    var saveChangesResult = await _context.SaveChangesAsync();
                    
                    return RedirectToAction("SpendMoney", "List", new { add = saveChangesResult == 1 });
                }
                else
                {
                    ModelState.AddModelError("", "حدث خطأ عام");
                    return View(VM);
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
                var VMnew = new SpendMoneyForReasonVM
                {
                    Reasons = await _context.MosbReasons.Select(x => new SelectListModel
                    {
                        Value = x.Id,
                        IsSelected = false,
                        Text = x.Name
                    }).ToListAsync(),
                    YearMonth = DateTime.Now.ToString("yyyy-MM")
                };
                return View(VMnew);
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
                Reasons = await _context.MosbReasons.Select(x => new SelectListModel
                {
                    Value = x.Id,
                    IsSelected = x.Id == ReasonId.Value ? true : false,
                    Text = x.Name
                }
                ).ToListAsync(),
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
                        Amount = Math.Round(x.Amount, 2),
                        IsPaid = x.IsPaid == true.GetHashCode() ? true : false,
                    })
                    .ToList();
                VM.AmountSubscribed = spendMoneyForReason.Select(x => x.MonthlyAmount).First();
                VM.IsHasRecodes = true;
            }
            else
            {
                VM.AllPersonInSystem = await _context.MosbName
                    .Select(x => new ShowSpendMoney
                    {
                        PersonId = x.Id,
                        PersonName = x.Name,
                        Amount = 0,
                        IsPaid = false
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
                    IsPaid = x.IsPaid,
                    MonthlyAmountRecord = record.MonthlyAmountRecord
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

                    var newAmount = item.IsPaid ? Math.Round(item.Amount, 2) : 0;
                    var name = _context.MosbName.Where(x => x.Id == item.PersonId).Select(x => x.Name).FirstOrDefault();
                    var reason = _context.MosbReasons.Where(x => x.Id == item.ReasonsId).Select(x => x.Name).FirstOrDefault();
                    var NotPaidDescription = string.Concat("الفاضل ", name, "لم يتم خصم مبلغ له لسبب ", reason, " بتاريخ ", item.Date);
                    var PaidDescription = string.Concat("تم خصم مبلغ ", newAmount, " لـ ", name, " لسبب ", reason, " بتاريخ ", item.Date);
                    var newRecord = new MosbSpendMoney
                    {
                        PersonId = item.PersonId,
                        ReasonsId = item.ReasonsId,
                        Date = item.Date,
                        Amount = newAmount,
                        IsPaid = item.IsPaid.GetHashCode(),
                        IsForReason = true.GetHashCode(),
                        MonthlyAmount = item.MonthlyAmountRecord,
                        Description = item.IsPaid ? PaidDescription : NotPaidDescription
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

                    var oldAmount = Math.Round(recordToEdit.Amount, 2);
                    var newAmount = item.IsPaid ? Math.Round(item.Amount, 2) : 0;
                    var name = _context.MosbName.Where(x => x.Id == item.PersonId).Select(x => x.Name).FirstOrDefault();
                    var reason = _context.MosbReasons.Where(x => x.Id == record.ReasonId).Select(x => x.Name).FirstOrDefault();
                    string EditDescription = string.Concat("تم تعديل مبلغ الصرف من ", oldAmount, " إلى ", newAmount, " لـ ", name, " لسبب ", reason, " بتاريخ ", record.MonthYear);
                    string ZeroAmountDescription = string.Concat("تم تعديل مبلغ الصرف من ", oldAmount, " إلى ", newAmount, " لـ ", name, " لسبب ", reason, " بتاريخ ", record.MonthYear, " وتم تعديل الحالة إلى غير مدفوع");
                    if (newAmount != oldAmount)
                    {
                        recordToEdit.Amount = newAmount;
                        recordToEdit.Description = !item.IsPaid ? ZeroAmountDescription : EditDescription;
                        recordToEdit.IsPaid = item.IsPaid.GetHashCode();
                    }
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
                        SubscriptionAmount = x.Name.SubscriptionAmount,
                        PersonName = x.Name.Name,
                        PersonId = x.NameId,
                        IsPaid = x.IsPaid == 1 ? true : false
                    }).ToList();
                VM.IsHasRecodes = true;
                //var rr  = [ "مبلغ الأشتراك", "هل دفع الأشتراك؟", "المبلغ الأشتراك", "الحالة"];
                VM.TableHeder = new List<string> { "الأسم", "مبلغ الأشتراك", "هل دفع الأشتراك؟", "المبلغ" };
                VM.TableBody = monthlyReceivePayments.Select(x =>
                new List<string> {
                    x.Name.Name,
                    Math.Round(x.Name.SubscriptionAmount, 2).ToString(),
                    x.IsPaid == 1 ? "نعم" : "لا",
                    Math.Round(x.Amount, 2).ToString()}).ToList();

            }
            else
            {
                VM.SumitedMonthlyPayments = await _context.MosbName
                    .Select(x => new ShowMonthlyReceivePayments
                    {
                        PersonId = x.Id,
                        PersonName = x.Name,
                        Amount = Math.Round(x.SubscriptionAmount, 2),
                        SubscriptionAmount = x.SubscriptionAmount,
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

        #region TransferMoney
        public IActionResult TransferMoney()
        {
            TransferMoneyVM VM = new();
            return View(VM);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTransferMoney(TransferMoneyVM VM)
        {
            try
            {
                if (VM == null)
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء تعديل البيانات");
                    return View(VM);
                }
                using var transaction = await _context.Database.BeginTransactionAsync();

                // FromPerson
                MosbName? FromPerson = await _context.MosbName.FirstAsync(x => x.Id == VM.FromNameId);
                if (FromPerson == null)
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء تعديل البيانات");
                    return View(VM);
                }
                MosbName? ToPerson = await _context.MosbName.FirstAsync(x => x.Id == VM.ToNameId);
                if (ToPerson == null)
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء تعديل البيانات");
                    return View(VM);
                }

                if (VM.FromNameId == VM.ToNameId)
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء تعديل البيانات");
                    return View(VM);
                }

                string Message = string.Empty;
                string? FromName = FromPerson?.Name;
                string? ToName = ToPerson?.Name;
                double TransferMoneyAmount = VM.Amount;

                Message = string.Concat(" تم تحويل مبلغ ",
                    TransferMoneyAmount, " ريال ",
                    " من ", FromName,
                    " إلى ", ToName,
                    " بتاريخ ", VM.Date.ToString("yyyy-MM-dd"),
                    " بسبب ", VM.description);

                MosbSpendMoney newSpend = new()
                {
                    PersonId = VM.FromNameId,
                    Date = VM.Date.ToString("yyyy-MM-dd"),
                    Amount = VM.Amount,
                    Description = Message,
                    IsForReason = false.GetHashCode(),
                    IsPaid = true.GetHashCode(),
                    IsTransaction = true.GetHashCode(),
                };
                await _context.MosbSpendMoney.AddAsync(newSpend);

                Message = string.Concat(" تم أستلام مبلغ ",
                    TransferMoneyAmount, " ريال ",
                    " من ", FromName,
                    " إلى ", ToName,
                    " بتاريخ ", VM.Date.ToString("yyyy-MM-dd"),
                    " بسبب ", VM.description);

                MosbReceivePayments newReceive = new()
                {
                    NameId = VM.ToNameId,
                    Date = VM.Date.ToString("yyyy-MM-dd"),
                    Amount = VM.Amount,
                    Description = Message,
                    IsMonthly = false.GetHashCode(),
                    IsPaid = true.GetHashCode(),
                    IsTransaction = true.GetHashCode(),
                };

                await _context.MosbReceivePayments.AddAsync(newReceive);

                int saveChangesResult = await _context.SaveChangesAsync();

                if (saveChangesResult == 0)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "حدث خطأ أثناء تعديل البيانات");
                    return View(VM);
                }

                await transaction.CommitAsync();
                return RedirectToAction("SpendMoney", "List", new { add = true });
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion TransferMoney
    }
}