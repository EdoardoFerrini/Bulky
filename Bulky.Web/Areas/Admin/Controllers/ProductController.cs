using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Category");

            return View(productList);
        }
        public IActionResult Upsert(int id)
        {
            ProductVm productVm = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
            };
            if (id == null || id == 0)
            {
                productVm.Product = new();
            }
            else
            {
                productVm.Product = _unitOfWork.Product.Get(u => u.Id == id);
            }

            return View(productVm);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVm productVm, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    if (!string.IsNullOrEmpty(productVm.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVm.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVm.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if(productVm.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVm.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVm.Product);
                }
               
                _unitOfWork.Save();
                TempData["success"] = "The Product has been created succesfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "";
            return View(productVm.Product);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return Json(new { data = productList });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productDelete = _unitOfWork.Product.Get(u=> u.Id == id);
            if(productDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productDelete.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(productDelete);
            _unitOfWork.Save();

            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { success = true, message = "Deleted succesfully" });
        }
        #endregion
    }
}
