using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        //Get
        public IActionResult Upsert(int? id) 
        {
            if (id == 0 || id == null)
            {
                Company company = new Company();
                return View(company);
            }
            else
            {
                Company company = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                    _unitOfWork.Company.Add(company);
                else
                    _unitOfWork.Company.Update(company);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(company);
        }

        #region APICALLS

        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }

        public IActionResult Delete(int? id)
        {
            var companyObj = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
            if (companyObj == null)
                return Json(new { success = false, message = "Error While Deleting Company" });
            else
            {
                _unitOfWork.Company.Remove(companyObj);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Company Deleted Successfully" });
            }
        }

        #endregion
    }
}
