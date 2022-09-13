using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var OrderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (OrderFromDb != null)
            {
                OrderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    OrderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePayment(int id, string sessionId, string paymentIntentId)
        {
            var OrderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (OrderFromDb != null)
            {
                OrderFromDb.PaymentDate = DateTime.Now;
                OrderFromDb.SessionId = sessionId;
                OrderFromDb.PaymentIntentId = paymentIntentId;
                
            }
        }
    }
}
