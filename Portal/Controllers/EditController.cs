using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.ComponentModel.Design;
using System.Xml.Linq;

namespace Portal.Controllers
{
    [Authorize]
    public class EditController : Controller
    {
        private readonly AlMohasebDBContext _dbContext;

        public EditController(AlMohasebDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> EditPerson(long id)
        {
            var name = await _dbContext.MosbName
                .FirstOrDefaultAsync(n => n.Id == id);

            if (name == null)
            {
                return NotFound();
            }

            EditPersonVM VM = new()
            {
                NameId = name.Id,
                Name = name.Name,
                CivilNumber = name.CivilNumber,
                Phone = name.PhoneNumber,
                SubscriptionAmount = name.SubscriptionAmount,
            };

            ViewData["Id"] = id;

            return View(VM);
        }

        [HttpPost]
        public async Task<IActionResult> EditPerson(EditPersonVM VM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(VM);
                }
                var name = await _dbContext
                    .MosbName
                    .FirstOrDefaultAsync(n => n.Id == VM.NameId);

                if (name == null)
                {
                    return NotFound();
                }

                // Update name properties
                if (name.Name != VM.Name) name.Name = VM.Name;
                if (name.CivilNumber != VM.CivilNumber) name.CivilNumber = VM.CivilNumber;
                if (name.PhoneNumber != VM.Phone) name.PhoneNumber = VM.Phone;
                if (name.SubscriptionAmount != VM.SubscriptionAmount) name.SubscriptionAmount = VM.SubscriptionAmount;

                _dbContext.Update(name);

                var saveStatus = await _dbContext.SaveChangesAsync();

                return RedirectToAction("Names", "List", new { update = saveStatus == 1 });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> EditReceivePayments(long id)
        {
            var receivePayment = await _dbContext.MosbReceivePayments
                .FirstOrDefaultAsync(n => n.Id == id);

            if (receivePayment == null)
            {
                return NotFound();
            }

            EditReceivePaymentsVM VM = new()
            {
                ReceivePaymentId = receivePayment.Id,
                Amount = receivePayment.Amount,
                Date = receivePayment.Date,
                Description = receivePayment.Description,
            };
            ViewBag.Date = DateTime.Parse(receivePayment.Date).ToString("yyyy-MM-dd");
            ViewBag.SelectedName = receivePayment.NameId;
            ViewBag.Id = id;

            return View(VM);
        }

        [HttpPost]
        public async Task<IActionResult> EditReceivePayments(EditReceivePaymentsVM VM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(VM);
                }

                var receivePayment = await _dbContext
                    .MosbReceivePayments
                    .FirstOrDefaultAsync(n => n.Id == VM.ReceivePaymentId);

                if (receivePayment == null)
                {
                    return NotFound();
                }

                // Update receivePayment properties
                if (receivePayment.NameId != VM.NameId) receivePayment.NameId = VM.NameId;
                receivePayment.IsPaid = VM.Amount == 0 ? false.GetHashCode() : true.GetHashCode();
                if (receivePayment.Amount != VM.Amount)
                {
                    receivePayment.Amount = VM.Amount;
                    receivePayment.ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                if (receivePayment.Date != VM.Date) receivePayment.Date = VM.Date;
                if (receivePayment.Description != VM.Description) receivePayment.Description = VM.Description;

                _dbContext.Update(receivePayment);

                var saveStatus = await _dbContext.SaveChangesAsync();
                return RedirectToAction("ReceivePayments", "List", new { update = saveStatus == 1 });
            }
            catch
            {
                throw;
            }
        }

        public async Task<IActionResult> EditSpendMoney(long id)
        {
            var spendMoney = await _dbContext.MosbSpendMoney
                .FirstOrDefaultAsync(n => n.Id == id);

            if (spendMoney == null)
            {
                return NotFound();
            }

            EditSpendMoneyVM VM = new()
            {
                SpendMoneyId = spendMoney.Id,
                NameId = spendMoney.PersonId,
                Amount = spendMoney.Amount,
                Date = spendMoney.Date,
                Description = spendMoney.Description,
            };
            ViewBag.Date = DateTime.Parse(spendMoney.Date).ToString("yyyy-MM-dd");
            ViewBag.SelectedName = spendMoney.PersonId;
            ViewBag.Id = id;

            return View(VM);
        }

        [HttpPost]
        public async Task<IActionResult> EditSpendMoney(EditSpendMoneyVM VM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Error");
                    return View(VM);
                }

                var spendMoney = await _dbContext.MosbSpendMoney.FirstOrDefaultAsync(n => n.Id == VM.SpendMoneyId);

                if (spendMoney == null)
                {
                    return RedirectToAction("NotFound", "Auth");
                }

                // Update spendMoney properties
                if (spendMoney.PersonId != VM.NameId) spendMoney.PersonId = VM.NameId;
                spendMoney.IsPaid = VM.Amount == 0 ? false.GetHashCode() : true.GetHashCode();
                if (spendMoney.Amount != VM.Amount)
                {
                    spendMoney.Amount = VM.Amount;
                    spendMoney.ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                if (spendMoney.Date != VM.Date) spendMoney.Date = VM.Date;
                if (spendMoney.Description != VM.Description) spendMoney.Description = VM.Description;



                _dbContext.Update(spendMoney);

                var Status = await _dbContext.SaveChangesAsync();

                return RedirectToAction("SpendMoney", "List", new { update = Status == 1 });
            }
            catch
            {
                throw;
            }
        }

        public async Task<IActionResult> EditTransferMoney(long? SpendMoneyId, long? ReceivePaymentId)
        {
            if (SpendMoneyId == 0 || ReceivePaymentId == 0 || SpendMoneyId == null || ReceivePaymentId == null) return BadRequest();

            var spendMoney = await _dbContext.MosbSpendMoney
                .FirstOrDefaultAsync(n => n.Id == SpendMoneyId);

            var receivePayment = await _dbContext.MosbReceivePayments
                .FirstOrDefaultAsync(n => n.Id == ReceivePaymentId);

            ViewData["Names"] = await _dbContext.MosbName.ToListAsync();

            if (spendMoney == null || receivePayment == null) return NotFound();

            EditTransferMoneyVM VM = new()
            {
                SpendMoneyId = spendMoney.Id,
                ReceivePaymentId = receivePayment.Id,
                FromNameText = spendMoney.Person.Name,
                ToNameText = receivePayment.Name.Name,
                Amount = spendMoney.Amount,
                Date = spendMoney.Date,
                Description = spendMoney.Description,
            };

            return View(VM);
        }

        [HttpPost]
        public async Task<IActionResult> EditTransferMoneyAjax(TransferMoneyEditRequest Request)
         {
            try
            {
                ArgumentNullException.ThrowIfNull(Request);
                ArgumentNullException.ThrowIfNull(Request.ReceivePaymentId);
                ArgumentNullException.ThrowIfNull(Request.SpendMoneyId);

                var spendMoney = await _dbContext.MosbSpendMoney
                    .Include(n => n.Person)
                    .FirstOrDefaultAsync(n => n.Id == Request.SpendMoneyId);

                var receivePayment = await _dbContext.MosbReceivePayments
                    .Include(n => n.Name)
                    .FirstOrDefaultAsync(n => n.Id == Request.ReceivePaymentId);

                ArgumentNullException.ThrowIfNull(spendMoney);
                ArgumentNullException.ThrowIfNull(receivePayment);

                long FromNameId = spendMoney.Person.Id;
                long ToNameId = receivePayment.Name.Id;
                
                // Update spendMoney properties
                spendMoney.IsPaid = Request.Amount == 0 ? false.GetHashCode() : true.GetHashCode();
                if (spendMoney.Amount != Request.Amount)
                {
                    spendMoney.Amount = Request.Amount;
                    spendMoney.ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                if (spendMoney.Description != Request.Description) spendMoney.Description = Request.Description;


                // Update receivePayment properties
                receivePayment.IsPaid = Request.Amount == 0 ? false.GetHashCode() : true.GetHashCode();
                if (receivePayment.Amount != Request.Amount)
                {
                    receivePayment.Amount = Request.Amount;
                    receivePayment.ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                if (receivePayment.Description != Request.Description) receivePayment.Description = Request.Description;


                _dbContext.Update(spendMoney);
                _dbContext.Update(receivePayment);
                _dbContext.SaveChanges();

                return Json(new { status = true, message = "تم التعديل بنجاح" });
            }
            catch
            {
                return BadRequest();
            }

        }

    }
}
