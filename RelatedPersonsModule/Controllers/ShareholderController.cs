using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;


namespace RelatedPersonsModule.Controllers
{
    [Authorize]
    public class ShareholderController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public ShareholderController(IDB _db, ILogger<HomeController> logger)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult Shareholders()
        {
            var model = new MainDataViewModel();
            model.RelatedPersons = db.GetRelatedPersons(1, 0, 0, 0);
            return View(model);
        }

        [HttpPost]
        public IActionResult NewShareholder(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.RelatedPerson);
            db.AddNewShareholderShare(model.RelatedPerson.PinCode, model.ShareholderShare.Shares);
            return RedirectToAction("Shareholders", "Shareholder");
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
            model.ShareholderShare = db.GetShareholderSharesById(IdForEdit);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditShareholder(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            db.UpdateShareholdersSharesById(model.RelatedPerson.Id, model.ShareholderShare.Shares);
            return RedirectToAction("Shareholders", "Shareholder");
        }
    }


}
