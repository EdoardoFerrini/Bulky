using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace Bulky.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
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
        [HttpPost]
        [Authorize(Roles = Sd.Role_Admin + "," + Sd.Role_Employee)]
        public IActionResult StartProcessing() 
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, Sd.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = Sd.Role_Admin + "," + Sd.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u=> u.Id == OrderVm.OrderHeader.Id);
            
            orderHeader.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVm.OrderHeader.Carrier;
            orderHeader.OrderStatus = OrderVm.OrderHeader.OrderStatus;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus == Sd.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = Sd.Role_Admin + "," + Sd.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u=> u.Id == OrderVm.OrderHeader.Id);

            if(orderHeader.PaymentStatus == Sd.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Sd.StatusCancelled, Sd.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Sd.StatusCancelled, Sd.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Order cancelled successfully";
            
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id});
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
