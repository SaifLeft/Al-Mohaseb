using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Diagnostics;

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
                        RegisterDate = DateTime.Now.ToString("yyyy-MM-dd")
                    };

                    var addedMosbName = await _context.MosbName.AddAsync(newMosbName);

                    await _context.SaveChangesAsync();

                    foreach (var reasonId in VM.ReasonsList)
                    {
                        _context.PersonReasonMapping.Add(new PersonReasonMapping
                        {
                            ReasonsId = reasonId,
                            NameId = addedMosbName.Entity.Id
                        });
                    }

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

        public async Task<ActionResult> CreateReason(string Reason )
        {
            var reason = new MosbReasons { Name = Reason,Date= DateTime.Now.ToString("yyyy-MM-dd") };
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


        public IActionResult ReceivePayments(long? ReasonId=null)
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
                    };

                    var AddedRePayment = await _context.MosbReceivePayments.AddAsync(newReceivePayments);

                    await _context.SaveChangesAsync();

                    foreach (var reasonId in VM.ReasonsList)
                    {
                        _context.ReceivePaymentsReasonsMapping.Add(new ReceivePaymentsReasonsMapping
                        {
                            ReasonsId = reasonId,
                            ReceivePaymentsId = AddedRePayment.Entity.Id
                        });
                    }

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
                        NameId = VM.NameId,
                        Date = VM.Date,
                        Amount = VM.Amount,
                    };

                    var AddedSpendMoney = await _context.MosbSpendMoney.AddAsync(newSpendMoney);

                    await _context.SaveChangesAsync();

                    foreach (var reasonId in VM.ReasonsList)
                    {
                        _context.ReasonsSpendMoneyMapping.Add(new ReasonsSpendMoneyMapping
                        {
                            ReasonsId = reasonId,
                            SpendMoneyId = AddedSpendMoney.Entity.Id
                        });
                    }

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




    }
}