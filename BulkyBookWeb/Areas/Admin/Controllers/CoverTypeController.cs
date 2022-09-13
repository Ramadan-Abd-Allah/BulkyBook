using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> coverTypeList = _unitOfWork.CoverType.GetAll();
            return View(coverTypeList);
        } //GET
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Cover Type Created Succefully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Edit(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();

            //var CategoryFromDb = _db.Categories.Find(Id);
            var CoverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == Id);
            if (CoverTypeFromDb == null)
                return NotFound();

            return View(CoverTypeFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Cover Type Updated Succefully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }


        //GET
        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();

            var CoverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == Id);

            if (CoverTypeFromDb == null)
                return NotFound();

            return View(CoverTypeFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? Id)
        {
            var obj = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == Id);
            if (obj == null)
                return BadRequest();

            _unitOfWork.CoverType.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Cover Type Deleted Succefully";
            return RedirectToAction("Index");
        }
    }
}
