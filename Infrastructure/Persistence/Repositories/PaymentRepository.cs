using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class PaymentRepository(OrderEaseDbContext context) : IPaymentRepository
    {
        public async Task AddAsync(Payment payment)
        {
            await context.Payments.AddAsync(payment);
        }

        public async Task<Payment?> GetAsync(Guid id)
        {
            return await context.Payments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Payment?> GetByPaystackReferenceAsync(string reference)
        {
            return await context.Payments.FirstOrDefaultAsync(x => x.PaystackReference == reference);
        }

        public async Task<ICollection<Payment>> GetAllByCustomerIdAsync(Guid customerId)
        {
            return await context.Payments
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public async Task<ICollection<Payment>> GetAllAsync()
        {
            return await context.Payments
                .Include(x => x.Customer)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public void Update(Payment payment)
        {
            context.Payments.Update(payment);
        }
    }
}