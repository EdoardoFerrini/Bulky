using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader entity)
        {
            _db.OrderHeaders.Add(entity);
        }

		public void UpdateStatus(int id, string orderstatus, string? paymentstatus = null)
		{
			var orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.OrderStatus = orderstatus;
                if(!string.IsNullOrEmpty(paymentstatus))
                {
                    orderHeaderFromDb.PaymentStatus = paymentstatus;
                }
            }
        }

		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);

			if (!string.IsNullOrEmpty(sessionId))
			{
				orderHeaderFromDb.SessionId = sessionId;
			}
            if(!string.IsNullOrEmpty(paymentIntentId))
            {
                orderHeaderFromDb.PaymentIntentId = paymentIntentId;
                orderHeaderFromDb.PaymentDate = DateTime.Now;
            }

		}
	}
}
