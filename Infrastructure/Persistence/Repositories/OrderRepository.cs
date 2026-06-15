using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderRepository(OrderEaseDbContext context) : IOrderRepository
    {
        public async Task AddAsync(Order order)
        {
            await context.Orders.AddAsync(order);
        }

        public async Task<Order?> GetAsync(Guid id)
        {
            return await context.Orders.Include(x => x.OrderItems).ThenInclude(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Order?> GetAsync(string orderNumber)
        {
            return await context.Orders.FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);
        }

        public async Task<ICollection<Order>> GetAllByCustomerIdAsync(Guid customerId)
        {
            return await context.Orders
                .Where(x => x.CustomerId == customerId)
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Item)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public async Task<ICollection<Order>> GetAllAsync()
        {
            return await context.Orders
                .Include(x => x.Customer)
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Item)
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public void Update(Order order)
        {
            context.Orders.Update(order);
        }
    }
}