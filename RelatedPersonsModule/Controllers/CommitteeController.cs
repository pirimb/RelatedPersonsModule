using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;


namespace RelatedPersonsModule.Controllers
{
    [Authorize]
    public class CommitteeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public CommitteeController(IDB _db, ILogger<HomeController> logger)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult Supervisors()
        {
            var model = new MainDataViewModel();
            model.RelatedPersons = db.GetRelatedPersons(0,1,0,0);
            return View(model);
        }

        [HttpGet]
        public IActionResult NewShareholder()
        {
            MainDataViewModel model = new MainDataViewModel();
            return View(model);
        }

        public IActionResult EditShareholder(int IdForEdit)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditShareholder(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            return RedirectToAction("Shareholders", "Shareholder");
        }

        public IActionResult ShareholderRelatives()
        {
            var model = new MainDataViewModel();
            //model.RelatedPerson = db.GetShareholdersRelatives();
            return View(model);
        }

        public IActionResult NewShareholderRelative()
        {
            MainDataViewModel model = new MainDataViewModel();
            return View(model);
        }
    }


}
