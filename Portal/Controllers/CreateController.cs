using Microsoft.AspNetCore.Mvc;
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
        string UserPassword = string.Empty;
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
        public IActionResult Names()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Names(PersonVM VM)
        {
            if (ModelState.IsValid)
            {
                MosbName Add = new MosbName();
                Add.Name = VM.Name;
                Add.PhoneNumber = VM.Phone;
                Add.ReasonsId = VM.ReasonId;
                Add.CivilNumber = VM.CivilNumber;
                Add.RegisterDate = DateTime.Now.ToString("yyyy-MM-dd");
               await _context.MosbName.AddAsync(Add);
               var status = await _context.SaveChangesAsync();
                return RedirectToAction("Names", status);
            }
            else
            {
                ModelState.AddModelError("", "حدث خطأ عام");
            }
            return View();


        }
        public async Task<ActionResult> CreateReason(string Reason)
        {
            var reason = new MosbReasons { Name = Reason };
            _context.MosbReasons.AddAsync(reason);
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