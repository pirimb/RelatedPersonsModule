using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;


namespace RelatedPersonsModule.Controllers
{
    [Authorize]
    public class SupervisorController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public SupervisorController(IDB _db, ILogger<HomeController> logger)
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

        [HttpPost]
        public IActionResult NewSupervisor(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.RelatedPerson);
            db.AddNewSupervisorPosition(model.RelatedPerson.PinCode, model.SupervisorPosition.Id);
            return RedirectToAction("Supervisors", "Supervisor");
        }

        [HttpGet]
        public IActionResult NewSupervisor()
        {
            MainDataViewModel model = new MainDataViewModel();
            return View(model);
        }
        public IActionResult EditSupervisor(int IdForEdit)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            model.SupervisorPosition = db.GetSupervisorPositionById(IdForEdit);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditSupervisor(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            db.UpdateSupervisorPositionById(model.RelatedPerson.Id, model.SupervisorPosition.Id);
            return RedirectToAction("Supervisors", "Supervisor");
        }
    }


}
