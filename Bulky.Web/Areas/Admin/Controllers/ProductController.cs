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
            var productList = _unitOfWork.Product.GetAll();

            return View(productList);
        }
        public IActionResult Upsert(int productId)
        {
            ProductVm productVm = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
            };
            if (productId == null || productId == 0)
            {
                productVm.Product = new();
            }
            else
            {
                productVm.Product = _unitOfWork.Product.Get(u => u.Id == productId);
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

        public IActionResult Edit(int ProductId)
        {
            Product Product = _unitOfWork.Product.Get(u => u.Id == ProductId);
            return View(Product);
        }

        [HttpPost]
        public IActionResult Edit(Product Product)
        {
            //if (Product. == Product.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("Name", "The name cannot be the same as display order");
            //}
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(Product);
                _unitOfWork.Save();
                TempData["success"] = "The Product has been edited succesfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "";
            return View(Product);
        }

        public IActionResult Delete(int ProductId)
        {
            Product Product = _unitOfWork.Product.Get(u => u.Id == ProductId);
            return View(Product);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePost(int ProductId)
        {
            Product Product = _unitOfWork.Product.Get(u => u.Id == ProductId);
            _unitOfWork.Product.Remove(Product);
            _unitOfWork.Save();
            TempData["success"] = "The Product has been deleted succesfully.";
            return RedirectToAction("Index");
        }
    }
}
