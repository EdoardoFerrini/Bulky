using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bulky.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            switch (status)
            {
                case "pending":
                    orderHeader = orderHeader.Where(u=> u.PaymentStatus == Sd.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
					orderHeader = orderHeader.Where(u => u.PaymentStatus == Sd.StatusInProcess);
                    break;
                case "completed":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == Sd.StatusShipped);
                    break;
                case "approved":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == Sd.PaymentStatusApproved);
                    break;

            }

            return Json(new { data = orderHeader });
		}

		#endregion
	}
}
