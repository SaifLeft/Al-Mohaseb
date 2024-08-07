﻿using System;
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
        public async Task<IActionResult> Names(bool? add, bool? update)
        {
            var VM = new NamesVM();
            VM.AddStatus = add;
            VM.UpdateStatus = update;
            return View(VM);
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
                    date = DateTime.Parse(p.RegisterDate).ToString("yyyy-MM-dd"),
                    amount = p.SubscriptionAmount
                })
                .OrderByDescending(x => x.id)
                .ToListAsync();

            var returnObj = new
            {
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                Data = phoneList
            };

            return Ok(returnObj);
        }
        public IActionResult Reasons(bool? add, bool? update, bool? delete)
        {
            var reasons = _context.MosbReasons.ToList();
            var VM = new ReasonVM
            {
                Reasons = reasons.Select(r => new ReasonsTable
                {
                    Id = r.Id,
                    Name = r.Name,
                    Date = r.Date,
                    Amount = r.Amount
                }).OrderByDescending(r => r.Id).ToList()
            };
            VM.AddStatus = add;
            VM.UpdateStatus = update;
            VM.DeleteStatus = delete;
            return View(VM);
        }


        [HttpGet]
        public async Task<List<MosbName>> GetNames()
        {
            return await _context.MosbName.ToListAsync();
        }

        public async Task<IActionResult> ReceivePayments(bool? add, bool? update)
        {
            ReceivePaymentsVM VM = new();
            VM.Names = await _context.MosbName
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToListAsync();

            VM.AddStatus = add;
            VM.UpdateStatus = update;

            return View(VM);
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
                .Include(n => n.Name)
                .Include(n => n.SpendMoneyTarget)
                .Include(n => n.SpendMoneyTarget.Person)
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
                    spendMoneyTargetId = p.SpendMoneyTargetId,
                    name = p.Name.Name,
                    amount = p.Amount,
                    date = DateTime.Parse(p.Date).ToString("yyyy-MM-dd"),
                    typeDetails = GetTypeDetails(p),
                    isTransaction = p.IsTransaction,
                    text = GeneratePaymentSummary(p)
                })
                .OrderByDescending(x => x.id)
                .ToListAsync();

            var returnObj = new
            {
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                Data = phoneList
            };

            return Ok(returnObj);
        }


        private static string GetTypeDetails(MosbReceivePayments p)
        {
            if (p.IsMonthly == true.GetHashCode())
                return @"<span class='badge rounded-pill bg-info'>اشتراك شهري</span>";
            else if (p.IsMonthly == false.GetHashCode())
            {
                if (p.IsTransaction == true.GetHashCode()) return @"<span class='badge rounded-pill bg-warning'>تحويل</span>";
                else return @"<span class='badge rounded-pill bg-secondary'>ايداع شخصي</span>";
            }
            else return "اخرى";
        }

        private static string GeneratePaymentSummary(MosbReceivePayments p)
        {
            string TransFormat = "تم إيداع المبلغ الى حساب: {0} بتحويله من حساب: {1} وذلك لـ{2}";
            string EditTransFormat = "تم تعديل التحويل مبلغ: {0} ريال الذي حصل بتاريخ:{1} من:{2} الى:{3} وذلك بسبب:{4} المبلغ الجديد:{5} ريال التعديل حصل بتاريخ:{6} ";

            if (p.IsTransaction == true.GetHashCode())
            {
                if (p.Amount == p.OriginalAmount)
                {
                    return string.Format(TransFormat, p.Name.Name, p.SpendMoneyTarget.Person.Name, p.Description);
                }
                else
                {
                    return string.Format(EditTransFormat, p.OriginalAmount, p.CreatedDate, p.Name.Name, p.SpendMoneyTarget.Person.Name, p.Description, p.Amount, p.ModifiedDate);
                }
            }
            else
            {
                return p.Description;
            }

        }



        #region SpendMoney Table
        public async Task<IActionResult> SpendMoney(bool? add, bool? update)
        {
            SpendMoneyVM VM = new SpendMoneyVM();
            VM.NamesList = await _context.MosbName
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToListAsync();
            // Get the years from the database and add them to the list of years in the view model group by the date year 
            VM.YearList = await _context.MosbSpendMoney
                .Select(x => new SelectListItem { Text = x.Date.Substring(0, 4), Value = x.Date.Substring(0, 4) })
                .Distinct()
                .ToListAsync();

            VM.AddStatus = add;
            VM.UpdateStatus = update;

            return View(VM);
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

            var data = _context.MosbSpendMoney
                        .Where(x => x.IsPaid == true.GetHashCode())
                        .Include(n => n.Person)
                        .Include(n => n.ReceivePaymentsTarget)
                        .Include(n => n.ReceivePaymentsTarget.Name)
                        .Include(n => n.Reasons)
                        .AsQueryable();
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
                    receivePaymentsTargetId = p.ReceivePaymentsTargetId,
                    name = p.Person.Name,
                    amount = p.Amount,
                    date = p.Date,
                    typeDetails = GetTypeDetails(p),
                    isTransaction = p.IsTransaction,
                    text = GenerateSpendMoneySummaryV1(p)

                })
                .OrderByDescending(x => x.id)
                .ToListAsync();



            var returnObj = new
            {
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                Data = phoneList
            };

            return Ok(returnObj);
        }
        private static string GetTypeDetails(MosbSpendMoney p)
        {
            if (p.IsForReason == true.GetHashCode())
                return @"<span class='badge rounded-pill bg-info'>صرف لسبب</span>";
            else if (p.IsForReason == false.GetHashCode())
            {
                if (p.IsTransaction == true.GetHashCode()) return @"<span class='badge rounded-pill bg-warning'>تحويل</span>";
                else return @"<span class='badge rounded-pill bg-secondary'>صرف شخصي</span>";
            }
            else return "اخرى";
        }
        public static string GenerateSpendMoneySummaryV1(MosbSpendMoney p)
        {
            bool isModefiled = p.Amount != p.OriginalAmount;
            string TransFormat = "تم سحب المبلغ من حساب: {0} بتحويله الى حساب: {1} وذلك لـ{2}";
            string EditTransFormat = "تم تعديل التحويل مبلغ: {0} ريال الذي حصل بتاريخ:{1} من:{2} الى:{3} وذلك بسبب:{4} المبلغ الجديد:{5} ريال التعديل حصل بتاريخ:{6} ";

            if (p.IsTransaction == true.GetHashCode())
            {
                if (isModefiled)
                {
                    return string.Format(EditTransFormat, p.OriginalAmount, p.CreatedDate, p.Person.Name, p.ReceivePaymentsTarget.Name.Name, p.Description, p.Amount, p.ModifiedDate);
                }
                else
                {
                    return string.Format(TransFormat, p.Person.Name, p.ReceivePaymentsTarget.Name.Name, p.Description);
                }
            }
            else
            {
                return p.Description;
            }
        }


        #endregion SpendMoney Table



    }
}
