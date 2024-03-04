using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader entity);
        void UpdateStatus(int id, string orderstatus, string? paymentstatus = null);
        void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
    }
}
