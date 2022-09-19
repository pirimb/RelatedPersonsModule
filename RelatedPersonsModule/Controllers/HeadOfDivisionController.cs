using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;


namespace RelatedPersonsModule.Controllers
{
    [Authorize]
    public class HeadOfDivisionController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public HeadOfDivisionController(IDB _db, ILogger<HomeController> logger)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult HeadOfDivisions()
        {
            var model = new MainDataViewModel();
            model.RelatedPersons = db.GetRelatedPersons(0, 0, 0, 1);
            model.Divisions = db.GetDivisions();
            return View(model);
        }

        [HttpPost]
        public IActionResult NewHeadOfDivision(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.RelatedPerson);
            db.AddNewHeadOfDivById(model.RelatedPerson.PinCode, model.Division.Id);
            return RedirectToAction("HeadOfDivisions", "HeadOfDivision");
        }

        [HttpGet]
        public IActionResult NewHeadOfDivision()
        {
            MainDataViewModel model = new MainDataViewModel();
            model.Divisions = db.GetDivisions();
            return View(model);
        }
        public IActionResult EditHeadOfDivision(int IdForEdit)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            model.Division = db.GetHeadOfDivDepById(IdForEdit);
            model.Divisions = db.GetDivisions();
            return View(model);
        }

        [HttpPost]
        public IActionResult EditHeadOfDivision(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            db.UpdateHeadOfDivByİd(model.RelatedPerson.Id, model.Division.Id);
            return RedirectToAction("HeadOfDivisions", "HeadOfDivision");
        }
    }


}
