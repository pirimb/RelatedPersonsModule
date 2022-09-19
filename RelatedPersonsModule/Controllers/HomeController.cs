using ClosedXML.Excel;
using Fingers10.ExcelExport.ActionResults;
using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace RelatedPersonsModule.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static IDB db;

        public HomeController(IDB _db, ILogger<HomeController> logger)
        {
            db = _db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new MainDataViewModel();
            model.RelatedPersons = db.GetRelatedPersons(0, 0, 0, 0);
            return View(model);
        }

        [HttpPost]
        public IActionResult NewRelatedPerson(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.RelatedPerson);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult NewRelatedPerson()
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelativeTypes = new List<RelativeType>();
            return View(model);
        }

        public IActionResult EditRelatedPerson(int IdForEdit)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditRelatedPerson(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            return RedirectToAction("index", "Home");
        }
        public IActionResult EditRelativeConnection(int IdForEdit, int mainId)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.MainId = mainId;
            model.RelatedPerson = db.GetRelatedPersonsById(IdForEdit);
            model.RelativeTypes = db.GetRelativeTypes();
            model.Relatives = db.GetRelativeTypeById(mainId, IdForEdit);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditRelativeConnection(MainDataViewModel model)
        {
            db.UpdateRelatedPerson(model.RelatedPerson);
            db.UpdateRelationship(model.MainId, model.RelatedPerson.Id, model.Relatives.RelType);
            model.RelatedPerson = db.GetRelatedPersonsById(model.MainId);
            model.RelativeConnections = db.GetRelativeConnectionsById(model.MainId);
            model.RelativeTypes = db.GetRelativeTypes();
            return View("Relatives", model);
        }

        public IActionResult Relatives(int IdForRelatives)
        {
            var model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForRelatives);
            model.RelativeConnections = db.GetRelativeConnectionsById(IdForRelatives);
            model.MainId = IdForRelatives;
            model.RelativeTypes = db.GetRelativeTypes();
            return View(model);
        }
        [HttpPost]
        public IActionResult NewRelative(MainDataViewModel model)
        {
            db.AddNewRelatedPerson(model.Relative);
            db.AddNewRelationship(model.RelatedPerson.Id, model.Relative.PinCode, model.Relatives.RelType);
            model.RelativeTypes = db.GetRelativeTypes();
            model.RelatedPerson = db.GetRelatedPersonsById(model.RelatedPerson.Id);
            model.RelativeConnections = db.GetRelativeConnectionsById(model.RelatedPerson.Id);
            model.MainId = model.RelatedPerson.Id;
            model.Relative = new RelatedPerson();
            return View("Relatives", model);
        }

        public IActionResult DeleteRelativeConnection(MainDataViewModel model)
        {
            model.Relatives = db.GetRelativeTypeById(model.MainId, model.RelativeId);
            db.DeleteRelativeConnection(model.MainId, model.RelativeId, model.Relatives.RelType);
            model.RelatedPerson = db.GetRelatedPersonsById(model.MainId);
            model.RelativeConnections = db.GetRelativeConnectionsById(model.MainId);
            model.RelativeTypes = db.GetRelativeTypes();
            return View("Relatives", model);
        }

        public IActionResult Departments()
        {
            MainDataViewModel model = new MainDataViewModel();
            model.Departments = db.GetDepartments();
            return View(model);
        }


        public IActionResult NewDepartment(MainDataViewModel model)
        {
            db.AddNewDepartment(model.NewDepartment.Name);
            return RedirectToAction("Departments", "Home");
        }

        public IActionResult EditDepartment(int depId)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.Department = db.GetDepartmentById(depId);
            return View(model);
        }
        [HttpPost]
        public IActionResult EditDepartment(MainDataViewModel model)
        {
            db.UpdateDepartment(model.Department.Id, model.Department.Name);
            return RedirectToAction("Departments", "Home");
        }


        public IActionResult Divisions()
        {
            MainDataViewModel model = new MainDataViewModel();
            model.Divisions = db.GetDivisions();
            return View(model);
        }
        public IActionResult NewDivision(MainDataViewModel model)
        {
            db.AddNewDivision(model.Division.Name);
            return RedirectToAction("Divisions", "Home");
        }

        public IActionResult EditDivision(int divId)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.Division = db.GetDivisionById(divId);
            return View(model);
        }
        [HttpPost]
        public IActionResult EditDivision(MainDataViewModel model)
        {
            db.UpdateDivision(model.Division.Id, model.Division.Name);
            return RedirectToAction("Divisions", "Home");
        }

        public IActionResult Committees()
        {
            MainDataViewModel model = new MainDataViewModel();
            model.Committees = db.GetCommittees();
            return View(model);
        }

        public IActionResult NewCommittee(MainDataViewModel model)
        {
            db.AddNewCommittee(model.Committee.Name);
            return RedirectToAction("Committees", "Home");
        }

        public IActionResult EditCommittee(int depId)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.Committee = db.GetCommitteeById(depId);
            return View(model);
        }
        [HttpPost]
        public IActionResult EditCommittee(MainDataViewModel model)
        {
            db.UpdateCommittee(model.Committee.Id, model.Committee.Name);
            return RedirectToAction("Committees", "Home");
        }


        public IActionResult EditPersonCommittee(int mainId, int ComitteeId)
        {
            MainDataViewModel model = new MainDataViewModel();
            model.MainId = mainId;
            model.ComitteeId = ComitteeId;
            model.RelatedPerson = db.GetRelatedPersonsById(mainId);
            model.Committees = db.GetCommittees();
            return View(model);
        }

        [HttpPost]
        public IActionResult EditPersonCommittee(MainDataViewModel model)
        {
            db.UpdatePersonCommittee(model.MainId, model.PersonCommittee.ComId, model.PersonCommittee.Position, model.ComitteeId);
            model.RelatedPerson = db.GetRelatedPersonsById(model.MainId);
            model.PersonCommittees = db.GetPersonCommitteesById(model.MainId);
            model.Committees = db.GetCommittees();
            return View("PersonCommittees", model);
        }

        public IActionResult PersonCommittees(int IdForCommittees)
        {
            var model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(IdForCommittees);
            model.PersonCommittees = db.GetPersonCommitteesById(IdForCommittees);
            model.MainId = IdForCommittees;
            model.Committees = db.GetCommittees();
            return View(model);
        }
        [HttpPost]
        public IActionResult NewPersonCommittee(MainDataViewModel model)
        {
            db.AddNewPersonCommittee(model.MainId, model.PersonCommittee.ComId, model.PersonCommittee.Position);
            model.Committees = db.GetCommittees();
            model.RelatedPerson = db.GetRelatedPersonsById(model.MainId);
            model.PersonCommittees = db.GetPersonCommitteesById(model.MainId);
            return View("PersonCommittees", model);
        }

        public IActionResult DeletePersonCommittee(int IdForDelete, int mainId)
        {
            db.DeletePersonCommittee(mainId, IdForDelete);
            var model = new MainDataViewModel();
            model.RelatedPerson = db.GetRelatedPersonsById(mainId);
            model.PersonCommittees = db.GetPersonCommitteesById(mainId);
            model.MainId = mainId;
            model.Committees = db.GetCommittees();
            return View("PersonCommittees", model);
        }

        [HttpGet]
        public int CheckByPinCode(string pinCode)
        {
            var result = db.CheckExistingByPinCode(pinCode);
            return result;
        }

        public string GetDbName()
        {
            string name = db.GetDbName();
            return name;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public string GetUserNameFromSession()
        {
            var obj = new LoginModel();
            try
            {
                var str = HttpContext.Session.GetString("user");

                obj = JsonSerializer.Deserialize<LoginModel>(str);
            }
            catch (Exception e)
            {
                RedirectToAction("Logout", "Account");
            }
            return obj.UserName;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult Edxcel()
        {

            var result = db.GetRelatedPersons(0, 0, 0, 0);
            return new ExcelResult<RelatedPerson>(result, "AidiyyatiShexsler", "AidiyyatiShexsler");
        }
        public IActionResult Excel()
        {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[9] { new DataColumn("Ad"),
                                        new DataColumn("Soyad"),
                                        new DataColumn("Ata adı"),
                                        new DataColumn("Qeydiyyat nömrəsi"),
                                        new DataColumn("Pin kod"),
                                        new DataColumn("Səhmdar"),
                                        new DataColumn("Audit"),
                                        new DataColumn("Müşahidə şurası"),
                                        new DataColumn("Tutduğu vəzifə") });

            var persons = db.GetRelatedPersons(0,0,0,0);

            foreach (var person in persons)
            {
                dt.Rows.Add(person.Name, person.SurName, person.Patron, person.Cif, person.PinCode, person.ShareholderWord, person.AuditWord, person.SupervisorWord, person.PositionWord);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AidiyyatiShexsler.xlsx");
                }
            }
        }

    }
}