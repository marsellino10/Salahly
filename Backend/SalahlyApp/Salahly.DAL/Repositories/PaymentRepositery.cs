using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Repositories
{
    public class PaymentRepositery : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepositery(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Payment?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }
    }
}
