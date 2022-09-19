using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;


namespace RelatedPersonsModule.Controllers
{
    [Authorize]
    public class DirectorController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public DirectorController(IDB _db, ILogger<HomeController> logger)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult Directors()
        {
            var model = new MainDataViewModel();
            model.RelatedPersons = db.GetRelatedPersons(0,0,0,2); // 1-hOD, 2-director, 3-gen dir
            model.Departments = db.GetDepartments();
            return View(model);
        }
        [HttpPost]
        public IActionResult NewDirector(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.RelatedPerson);
            db.AddNewDirectorDep(model.RelatedPerson.PinCode, model.Department.Id); 
            return RedirectToAction("Directors", "Director");
        }

        [HttpGet]
        public IActionResult NewDirector()
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelativeTypes = new List<RelativeType>();
            model.Departments = db.GetDepartments();
            return View(model);
        }
        public IActionResult EditDirector(int IdForEdit)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            model.Department = db.GetDirectorDepById(IdForEdit);
            model.Departments = db.GetDepartments();
            return View(model);
        }

        [HttpPost]
        public IActionResult EditDirector(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            db.UpdateDirectorDep(model.RelatedPerson.Id, model.Department.Id);
            return RedirectToAction("Directors", "Director");
        }
    }


}
