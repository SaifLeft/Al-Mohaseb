using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
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
                .Include(x => x.PersonReasonMapping)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (name == null)
            {
                return NotFound();
            }

            EditPersonVM VM = new()
            {
                Id = name.Id,
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
                VM.Id = long.Parse(Request.Form["Id"]);
                if (!ModelState.IsValid)
                {
                    return View(VM);
                }
                var name = await _dbContext.MosbName.Include(x => x.PersonReasonMapping).FirstOrDefaultAsync(n => n.Id == VM.Id);

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
                ViewBag.UpdateStatus = saveStatus == 0
                                        ? "Error" : "Success";

                return RedirectToAction("Names", "List");
            }
            catch (Exception ex)
            {
                ViewBag.UpdateStatus = "Error: " + ex.Message;
                return View(VM);
            }
        }

        public async Task<IActionResult> EditReceivePayments(long id)
        {
            var receivePayment = await _dbContext.MosbReceivePayments
                .Include(x => x.ReceivePaymentsReasonsMapping)
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
            ViewBag.selectedReasonsList = receivePayment.ReceivePaymentsReasonsMapping.Select(x => x.ReasonsId).ToList();
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
                    .Include(x => x.ReceivePaymentsReasonsMapping)
                    .FirstOrDefaultAsync(n => n.Id == VM.ReceivePaymentId);

                if (receivePayment == null)
                {
                    return NotFound();
                }

                // Update receivePayment properties
                if (receivePayment.NameId != VM.NameId) receivePayment.NameId = VM.NameId;
                if (receivePayment.Amount != VM.Amount) receivePayment.Amount = VM.Amount;
                if (receivePayment.Date != VM.Date) receivePayment.Date = VM.Date;
                if (receivePayment.Description != VM.Description) receivePayment.Description = VM.Description;

                _dbContext.Update(receivePayment);

                var saveStatus = await _dbContext.SaveChangesAsync();
                ViewBag.UpdateStatus = saveStatus == 0
                                        ? "Error" : "Success";
                return RedirectToAction("ReceivePayments", "List");
            }
            catch (Exception ex)
            {
                ViewBag.UpdateStatus = "Error: " + ex.Message;
                return View(VM);
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

                using var transaction = _dbContext.Database.BeginTransaction();

                var spendMoney = await _dbContext.MosbSpendMoney.FirstOrDefaultAsync(n => n.Id == VM.SpendMoneyId);

                if (spendMoney == null)
                {
                    return RedirectToAction("NotFound", "Auth");
                }

                // Update spendMoney properties
                if (spendMoney.PersonId != VM.NameId) spendMoney.PersonId = VM.NameId;
                if (spendMoney.Amount != VM.Amount) spendMoney.Amount = VM.Amount;
                if (spendMoney.Date != VM.Date) spendMoney.Date = VM.Date;
                if (spendMoney.Description != VM.Description) spendMoney.Description = VM.Description;

                _dbContext.Update(spendMoney);

                var Status = await _dbContext.SaveChangesAsync();
                if (Status == 0)
                {
                    ViewData["UpdateStatus"] = "Error";
                }
                transaction.Commit();
                ViewData["UpdateStatus"] = "Success";

                return RedirectToAction("SpendMoney", "List");
            }
            catch (Exception ex)
            {
                ViewData["UpdateStatus"] = "Error";
                return View(VM);
            }
        }

    }
}
