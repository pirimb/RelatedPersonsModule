using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;


namespace RelatedPersonsModule.Controllers
{
    [Authorize]
    public class AuditController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public AuditController(IDB _db, ILogger<HomeController> logger)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult Audits()
        {
            var model = new MainDataViewModel();
            model.RelatedPersons = db.GetRelatedPersons(0,0,1,0);
            return View(model);
        }

        [HttpPost]
        public IActionResult NewAudit(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.RelatedPerson);
            return RedirectToAction("Audits", "Audit");
        }

        [HttpGet]
        public IActionResult NewAudit()
        {
            MainDataViewModel model = new MainDataViewModel();
            return View(model);
        }
        public IActionResult EditAudit(int IdForEdit)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditAudit(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            return RedirectToAction("Audits", "Audit");
        }
    }


}
