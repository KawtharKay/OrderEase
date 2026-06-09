using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CustomerRepository(OrderEaseDbContext context) : ICustomerRepository
    {
        public async Task AddAsync(Customer customer)
        {
            await context.Customers.AddAsync(customer);
        }

        public async Task<Customer?> GetAsync(Guid id)
        {
            return await context.Customers.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Customer?> GetByUserIdAsync(Guid userId)
        {
            return await context.Customers.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<Customer?> GetAsync(string email)
        {
            return await context.Customers.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<ICollection<Customer>> GetAllAsync()
        {
            return await context.Customers.Include(x => x.Orders).ToListAsync();
        }

        public void Update(Customer customer)
        {
            context.Customers.Update(customer);
        }
    }
}