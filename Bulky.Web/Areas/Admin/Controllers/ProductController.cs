﻿using Bulky.DataAccess.Data;
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
        private readonly ApplicationDbContext _db;
        public ProductController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
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
                Product= new Product()
            };
            if (productId != null || productId != 1)
            {

                productVm.Product = _unitOfWork.Product.Get(u => u.Id == productId);
            }

            return View(productVm);
        }
        [HttpPost]
        public IActionResult Create(Product product)
        {
            //if (product.Name == product.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("Name", "The name cannot be the same as display order");
            //}
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(product);
                _unitOfWork.Save();
                TempData["success"] = "The Product has been created succesfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "";
            return View(product);
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
