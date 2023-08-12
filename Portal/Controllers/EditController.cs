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
                .Include(x => x.MosbPersonReasonMapping)
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
                ReasonsList = name.MosbPersonReasonMapping.Select(x => x.ReasonsId).ToList()
            };

            ViewBag.selectedReasonsList = name.MosbPersonReasonMapping.Select(x => x.ReasonsId).ToList();
            ViewBag.Id = id;

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
                var name = await _dbContext.MosbName.Include(x=>x.MosbPersonReasonMapping).FirstOrDefaultAsync(n => n.Id == VM.Id);

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
                var oldSelectedReasonsList = name.MosbPersonReasonMapping.Select(x => x.ReasonsId).ToList();

                var addedReasonsList = newSelectedReasonsList.Except(oldSelectedReasonsList).ToList();
                var removedReasonsList = oldSelectedReasonsList.Except(newSelectedReasonsList).ToList();

                foreach (var reasonId in addedReasonsList)
                {
                    _dbContext.MosbPersonReasonMapping.Add(new MosbPersonReasonMapping
                    {
                        ReasonsId = reasonId,
                        NameId = VM.Id
                    });
                }

                await _dbContext.SaveChangesAsync();

                foreach (var reasonId in removedReasonsList)
                {
                    var reason = await _dbContext.MosbPersonReasonMapping.FirstOrDefaultAsync(x => x.ReasonsId == reasonId && x.NameId == VM.Id);
                    if (reason != null)
                    {
                        _dbContext.MosbPersonReasonMapping.Remove(reason);
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

    }
}
