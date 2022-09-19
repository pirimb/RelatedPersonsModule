using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;


namespace RelatedPersonsModule.Controllers
{
    [Authorize]
    public class GeneralDirectorController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public GeneralDirectorController(IDB _db, ILogger<HomeController> logger)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult GeneralDirectors()
        {
            var model = new MainDataViewModel();
            model.RelatedPersons = db.GetRelatedPersons(0,0,0,3);
            return View(model);
        }
        [HttpPost]
        public IActionResult NewGeneralDirector(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.RelatedPerson);
            foreach (var item in model.states)
            {
                db.AddNewDirectorDep(model.RelatedPerson.PinCode, item);
            }
            return RedirectToAction("GeneralDirectors", "GeneralDirector");
        }

        [HttpGet]
        public IActionResult NewGeneralDirector()
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelativeTypes = new List<RelativeType>();
            model.Departments = db.GetDepartments();
            return View(model);
        }
        public IActionResult EditGeneralDirector(int IdForEdit)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            model.states = db.GetGenDirDepsById(IdForEdit);
            model.Departments = db.GetDepartments();
            return View(model);
        }

        [HttpPost]
        public IActionResult EditGeneralDirector(MainDataViewModel model)
        {
            db.DeleteGenDirectorId(model.RelatedPerson.Id);
            db.UpdateRelatedPerson(model.RelatedPerson);
            foreach (var item in model.states)
            {
                db.AddNewDirectorDep(model.RelatedPerson.PinCode, item);
            }
            return RedirectToAction("GeneralDirectors", "GeneralDirector");
        }
    }


}
