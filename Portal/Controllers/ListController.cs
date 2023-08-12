using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models.ViewModels;

namespace Portal.Controllers
{
    public class ListController : Controller
    {
        private readonly AlMohasebDBContext _context;

        public ListController(AlMohasebDBContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Names()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetUserDetails()
        {

            int totalRecord = 0;
            int filterRecord = 0;

            var data = _context.MosbName
                .Include(n => n.MosbPersonReasonMapping)
                .AsQueryable();

            

            totalRecord = await data.CountAsync();
            filterRecord = totalRecord;

            var phoneList = await data
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    phone = p.PhoneNumber,
                    civilnumber = p.CivilNumber,
                    date = p.RegisterDate,
                    reasons = p.MosbPersonReasonMapping.Select(x => x.Reasons.Name).ToList()
                })
                .ToListAsync();

            var returnObj = new
            {
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                Data = phoneList
            };

            return Ok(returnObj);
        }
        public IActionResult Reasons()
        {
            var reasons = _context.MosbReasons.ToList();
            var VM = new ReasonVM
            {
                Reasons = reasons.Select(r => (r.Id, r.Name)).ToList()
            };
            return View(VM);
        }
        [HttpGet]
        public async Task<List<MosbReasons>> GetReasons()
        {
            return await _context.MosbReasons.ToListAsync();
        }

        public IActionResult ReceivePayments()
        {
            return View();
        }

        public IActionResult SpendMoney()
        {
            return View();
        }

    }
}
