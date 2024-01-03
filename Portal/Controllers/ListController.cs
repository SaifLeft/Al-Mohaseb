using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Controllers
{
    [Authorize]

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
        public async Task<IActionResult> Names()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetUserDetails()
        {

            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");


            var data = _context.MosbName
                .Include(n => n.PersonReasonMapping)
                .AsQueryable();

            // Filter the data based on the searchValue if applicable
            if (!string.IsNullOrEmpty(searchValue.ToString()))
            {
                searchValue = searchValue.Trim();
                data = data.Where(p =>
                    p.Name.Contains(searchValue.ToString()) ||
                    p.CivilNumber.ToString().Contains(searchValue.ToString()) ||
                    p.RegisterDate.Contains(searchValue.ToString()) ||
                    p.PhoneNumber.ToString().Contains(searchValue.ToString())
                );
            }



            totalRecord = await data.CountAsync();
            filterRecord = totalRecord;

            var phoneList = await data
                .OrderBy(x => sortColumn + " " + sortColumnDirection)
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    phone = p.PhoneNumber,
                    civilnumber = p.CivilNumber,
                    date = p.RegisterDate,
                    amount = p.SubscriptionAmount
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
                Reasons = reasons.Select(r => (r.Id, r.Name, r.Amount)).ToList()
            };
            return View(VM);
        }
        [HttpGet]
        public async Task<List<MosbReasons>> GetReasons(long? ByNameId = null)
        {
            if (ByNameId != null)
            {
                var reasons = await _context.PersonReasonMapping
                    .Where(p => p.NameId == ByNameId)
                    .Select(p => p.Reasons)
                    .ToListAsync();
                return reasons;
            }
            return await _context.MosbReasons.ToListAsync();
        }


        [HttpGet]
        public async Task<List<MosbName>> GetNames()
        {
            return await _context.MosbName.ToListAsync();
        }

        public async Task<IActionResult> ReceivePayments()
        {
            ViewData["NamesList"] = await _context.MosbName
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetReceivePaymentsDetails()
        {
            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            var data = _context.MosbReceivePayments
                .Where(x => x.IsPaid == true.GetHashCode())
                .Include(n => n.ReceivePaymentsReasonsMapping)
                .AsQueryable();

            string? NameIdSearch = Request.Form["columns[1][search][value]"].FirstOrDefault().Trim();

            if (!string.IsNullOrEmpty(NameIdSearch) && long.TryParse(NameIdSearch, out long parsedId))
            {
                data = data.Where(p => p.Name.Id == parsedId);
            }

            // Filter the data based on the searchValue if applicable
            if (!string.IsNullOrEmpty(searchValue.ToString()))
            {
                searchValue = searchValue.Trim();
                data = data.Where(p =>
                    p.Name.Name.Contains(searchValue.ToString()) ||
                    p.Description.ToString().Contains(searchValue.ToString()) ||
                    p.Date.Contains(searchValue.ToString()) ||
                    p.Name.PhoneNumber.ToString().Contains(searchValue.ToString()) ||
                    p.Name.CivilNumber.ToString().Contains(searchValue.ToString())
                );
            }
            totalRecord = await data.CountAsync();
            filterRecord = totalRecord;

            var phoneList = await data
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name.Name,
                    amount = p.Amount,
                    date = p.Date,
                    description = p.Description
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
        public async Task<IActionResult> SpendMoney()
        {
            ViewData["NamesList"] = await _context.MosbName
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToListAsync();
            ViewData["YearList"] = await _context.MosbSpendMoney
                .Select(x => new SelectListItem { 
                    Text =DateTime.Parse(x.Date).Year.ToString(),
                    Value = DateTime.Parse(x.Date).Year.ToString()
                })
                .Distinct()
                .ToListAsync();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetSpendMoneyDetails()
        {
            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            var data = _context.MosbSpendMoney.Where(x => x.IsPaid == true.GetHashCode()).AsQueryable();
            totalRecord = await data.CountAsync();
            filterRecord = totalRecord;

            string? NameIdSearch = Request.Form["columns[1][search][value]"].FirstOrDefault()?.Trim();
            string? YearSearch = Request.Form["columns[2][search][value]"].FirstOrDefault()?.Trim();
            long parsedId = 0;
            int parsedYear = 0;
            bool isNameIdSearchValid = !string.IsNullOrEmpty(NameIdSearch) && long.TryParse(NameIdSearch, out parsedId);
            bool isYearSearchValid = !string.IsNullOrEmpty(YearSearch) && int.TryParse(YearSearch, out parsedYear);

            if (isNameIdSearchValid && isYearSearchValid)
            {
                data = data.Where(p => p.Person.Id == parsedId && p.Date.Contains(parsedYear.ToString()));
            }
            else if (isNameIdSearchValid)
            {
                data = data.Where(p => p.Person.Id == parsedId);
            }
            else if (isYearSearchValid)
            {
                data = data.Where(p => p.Date.Contains(parsedYear.ToString()));
            }

            // Filter the data based on the searchValue if applicable
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.Trim();
                data = data.Where(p =>
                    p.Person.Name.Contains(searchValue.ToString()) ||
                    p.Description.ToString().Contains(searchValue.ToString()) ||
                    p.Date.Contains(searchValue.ToString()) ||
                    p.Person.PhoneNumber.ToString().Contains(searchValue.ToString()) ||
                    p.Person.CivilNumber.ToString().Contains(searchValue.ToString())
                );
            }
            var phoneList = await data
                .OrderBy(x => sortColumn + " " + sortColumnDirection)
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Person.Name,
                    amount = p.Amount,
                    date = p.Date,
                    description = p.Description
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

    }
}
