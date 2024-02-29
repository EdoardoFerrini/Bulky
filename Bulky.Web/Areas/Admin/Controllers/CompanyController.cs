using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Company> companyList = _unitOfWork.Company.GetAll();

            return View(companyList);
        }
        public IActionResult Upsert(int id)
        {
           
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company company= _unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }

        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                
                if(company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }
               
                _unitOfWork.Save();
                TempData["success"] = "The company has been created succesfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "";
            return View(company);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyDelete = _unitOfWork.Company.Get(u=> u.Id == id);
            if(companyDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
 

            _unitOfWork.Company.Remove(companyDelete);
            _unitOfWork.Save();

            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { success = true, message = "Deleted succesfully" });
        }
        #endregion
    }
}
