using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var categoryList = _unitOfWork.Category.GetAll();
            return View(categoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The name cannot be the same as display order");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "The Category has been created succesfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "";
            return View(category);
        }

        public IActionResult Edit(int categoryId)
        {
            Category category = _unitOfWork.Category.Get(u => u.Id == categoryId);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The name cannot be the same as display order");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "The Category has been edited succesfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "";
            return View(category);
        }

        public IActionResult Delete(int categoryId)
        {
            Category category = _unitOfWork.Category.Get(u => u.Id == categoryId);
            return View(category);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePost(int categoryId)
        {
            Category category = _unitOfWork.Category.Get(u => u.Id == categoryId);
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "The Category has been deleted succesfully.";
            return RedirectToAction("Index");
        }
    }
}
