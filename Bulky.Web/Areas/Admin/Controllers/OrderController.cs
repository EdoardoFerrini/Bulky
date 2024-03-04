using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace Bulky.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVm OrderVm { get; set; }
		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details( int orderId)
        {
            OrderVm = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVm);
        }
        [HttpPost]
        [Authorize(Roles=Sd.Role_Admin+","+Sd.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVm.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVm.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVm.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVm.OrderHeader.City;
            orderHeaderFromDb.State = OrderVm.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVm.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVm.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = OrderVm.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            if(User.IsInRole(Sd.Role_Admin) || User.IsInRole(Sd.Role_Employee))
            {
                orderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeader = _unitOfWork.OrderHeader.GetAll(u=> u.ApplicationUserId == userId, includeProperties:"ApplicationUser");
            }

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
