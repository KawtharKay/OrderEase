using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ReturnRequestRepository(OrderEaseDbContext context) : IReturnRequestRepository
    {
        public async Task AddAsync(ReturnRequest returnRequest)
        {
            await context.ReturnRequests.AddAsync(returnRequest);
        }

        public async Task<ReturnRequest?> GetAsync(Guid id)
        {
            return await context.ReturnRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<ICollection<ReturnRequest>> GetAllByCustomerIdAsync(Guid customerId)
        {
            return await context.ReturnRequests
                .Where(x => x.CustomerId == customerId)
                .Include(x => x.ReturnRequestItems)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public async Task<ICollection<ReturnRequest>> GetAllAsync()
        {
            return await context.ReturnRequests
                .Include(x => x.Customer)
                .Include(x => x.Category)
                .Include(x => x.ReturnRequestItems)
                    .ThenInclude(x => x.Item)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public void Update(ReturnRequest returnRequest)
        {
            context.ReturnRequests.Update(returnRequest);
        }
    }
}