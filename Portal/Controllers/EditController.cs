using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models.ViewModels;
using System.Xml.Linq;

namespace Portal.Controllers
{
    public class EditController : Controller
    {
        //cotaxt 
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
                Phone = name.PhoneNumber.Value,
                ReasonsList = name.PersonReasonMapping.Select(x => x.ReasonsId).ToList()
            };

            ViewData["SelectedReasonsList"] = name.PersonReasonMapping.Select(x => x.ReasonsId).ToList();
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

                using var transaction = _dbContext.Database.BeginTransaction();
                var name = await _dbContext.MosbName.Include(x=>x.PersonReasonMapping).FirstOrDefaultAsync(n => n.Id == VM.Id);

                if (name == null)
                {
                    return NotFound();
                }

                // Update name properties
                if (name.Name != VM.Name) name.Name = VM.Name;
                if (name.CivilNumber != VM.CivilNumber) name.CivilNumber = VM.CivilNumber;
                if (name.PhoneNumber != VM.Phone) name.PhoneNumber = VM.Phone;

                _dbContext.Update(name);

                await _dbContext.SaveChangesAsync();

                // Handle reason mappings
                var newSelectedReasonsList = VM.ReasonsList;
                var oldSelectedReasonsList = name.PersonReasonMapping.Select(x => x.ReasonsId).ToList();

                var addedReasonsList = newSelectedReasonsList.Except(oldSelectedReasonsList).ToList();
                var removedReasonsList = oldSelectedReasonsList.Except(newSelectedReasonsList).ToList();

                foreach (var reasonId in addedReasonsList)
                {
                    _dbContext.PersonReasonMapping.Add(new PersonReasonMapping
                    {
                        ReasonsId = reasonId,
                        NameId = VM.Id
                    });
                }

                await _dbContext.SaveChangesAsync();

                foreach (var reasonId in removedReasonsList)
                {
                    var reason = await _dbContext.PersonReasonMapping.FirstOrDefaultAsync(x => x.ReasonsId == reasonId && x.NameId == VM.Id);
                    if (reason != null)
                    {
                        _dbContext.PersonReasonMapping.Remove(reason);
                    }
                }

                await _dbContext.SaveChangesAsync();

                transaction.Commit();
                ViewBag.UpdateStatus = "Success";

                return RedirectToAction("Names", "List");
            }
            catch (Exception ex)
            {
                ViewBag.UpdateStatus = "Error: " + ex.Message;
                return View(VM);
            }
        }

        //EditReceivePayments
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
                Id = receivePayment.Id,
                Amount = receivePayment.Amount,
                Date = receivePayment.Date
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
                VM.Id = long.Parse(Request.Form["Id"]);
                if (!ModelState.IsValid)
                {
                    return View(VM);
                }

                using var transaction = _dbContext.Database.BeginTransaction();
                var receivePayment = await _dbContext.MosbReceivePayments.Include(x => x.ReceivePaymentsReasonsMapping).FirstOrDefaultAsync(n => n.Id == VM.Id);

                if (receivePayment == null)
                {
                    return NotFound();
                }

                // Update receivePayment properties
                if (receivePayment.NameId != VM.NameId) receivePayment.NameId = VM.NameId;
                if (receivePayment.Amount != VM.Amount) receivePayment.Amount = VM.Amount;
                if (receivePayment.Date != VM.Date) receivePayment.Date = VM.Date;

                _dbContext.Update(receivePayment);

                await _dbContext.SaveChangesAsync();

                // Handle reason mappings
                var newSelectedReasonsList = VM.ReasonsList;
                var oldSelectedReasonsList = receivePayment.ReceivePaymentsReasonsMapping.Select(x => x.ReasonsId).ToList();

                var addedReasonsList = newSelectedReasonsList.Except(oldSelectedReasonsList).ToList();
                var removedReasonsList = oldSelectedReasonsList.Except(newSelectedReasonsList).ToList();

                foreach (var reasonId in addedReasonsList)
                {
                    _dbContext.ReceivePaymentsReasonsMapping.Add(new ReceivePaymentsReasonsMapping
                    {
                        ReasonsId = reasonId,
                        ReceivePaymentsId = VM.Id
                    });
                }

                await _dbContext.SaveChangesAsync();

                foreach (var reasonId in removedReasonsList)
                {
                    var reason = await _dbContext.ReceivePaymentsReasonsMapping.FirstOrDefaultAsync(x => x.ReasonsId == reasonId && x.ReceivePaymentsId == VM.Id);
                    if (reason != null)
                    {
                        _dbContext.ReceivePaymentsReasonsMapping.Remove(reason);
                    }
                }

                await _dbContext.SaveChangesAsync();

                transaction.Commit();
                ViewBag.UpdateStatus = "Success";

                return RedirectToAction("ReceivePayments", "List");
            }
            catch (Exception ex)
            {
                ViewBag.UpdateStatus = "Error: " + ex.Message;
                return View(VM);
            }
        }

        //EditSpendMoney
        public async Task<IActionResult> EditSpendMoney(long id)
        {
            var spendMoney = await _dbContext.MosbSpendMoney
                .Include(x => x.ReasonsSpendMoneyMapping)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (spendMoney == null)
            {
                return NotFound();
            }

            EditSpendMoneyVM VM = new()
            {
                NameId = spendMoney.Id,
                Amount = spendMoney.Amount,
                Date = spendMoney.Date
            };
            ViewBag.Date = DateTime.Parse(spendMoney.Date).ToString("yyyy-MM-dd");
            ViewBag.SelectedName = spendMoney.NameId;
            ViewBag.selectedReasonsList = spendMoney.ReasonsSpendMoneyMapping.Select(x => x.ReasonsId).ToList();
            ViewBag.Id = id;

            return View(VM);
        }

        [HttpPost]
        public async Task<IActionResult> EditSpendMoney(EditSpendMoneyVM VM)
        {
            try
            {
                VM.NameId = long.Parse(Request.Form["Id"]);
                if (!ModelState.IsValid)
                {
                    return View(VM);
                }

                using var transaction = _dbContext.Database.BeginTransaction();
                var spendMoney = await _dbContext.MosbSpendMoney.Include(x => x.ReasonsSpendMoneyMapping).FirstOrDefaultAsync(n => n.Id == VM.NameId);

                if (spendMoney == null)
                {
                    return NotFound();
                }

                // Update spendMoney properties
                if (spendMoney.NameId != VM.NameId) spendMoney.NameId = VM.NameId;
                if (spendMoney.Amount != VM.Amount) spendMoney.Amount = VM.Amount;
                if (spendMoney.Date != VM.Date) spendMoney.Date = VM.Date;

                _dbContext.Update(spendMoney);

                await _dbContext.SaveChangesAsync();

                // Handle reason mappings
                var newSelectedReasonsList = VM.ReasonsList;
                var oldSelectedReasonsList = spendMoney.ReasonsSpendMoneyMapping.Select(x => x.ReasonsId).ToList();

                var addedReasonsList = newSelectedReasonsList.Except(oldSelectedReasonsList).ToList();
                var removedReasonsList = oldSelectedReasonsList.Except(newSelectedReasonsList).ToList();

                foreach (var reasonId in addedReasonsList)
                {
                    _dbContext.ReasonsSpendMoneyMapping.Add(new ReasonsSpendMoneyMapping
                    {
                        ReasonsId = reasonId,
                        SpendMoneyId = VM.NameId
                    });
                }

                await _dbContext.SaveChangesAsync();

                foreach (var reasonId in removedReasonsList)
                {
                    var reason = await _dbContext.ReasonsSpendMoneyMapping.FirstOrDefaultAsync(x => x.ReasonsId == reasonId && x.SpendMoneyId == VM.NameId);
                    if (reason != null)
                    {
                        _dbContext.ReasonsSpendMoneyMapping.Remove(reason);
                    }
                }

                await _dbContext.SaveChangesAsync();

                transaction.Commit();
                ViewBag.UpdateStatus = "Success";

                return RedirectToAction("SpendMoney", "List");
            }
            catch (Exception ex)
            {
                ViewBag.UpdateStatus = "Error: " + ex.Message;
                return View(VM);
            }
        }

    }
}
