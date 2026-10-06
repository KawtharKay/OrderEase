using Application.Common.Dtos;
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

        public async Task<IEnumerable<Customer>> GetAllWithBalancesAsync()
        {
            return await context.Customers
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new Customer
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    OutstandingBalance = x.OutstandingBalance
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerBalanceSummary>> GetAllWithBalanceSummaryAsync()
        {
            return await context.Customers
                .Where(x => !x.IsDeleted && x.User.IsVerified)
                .Select(x => new CustomerBalanceSummary
                {
                    CustomerId = x.Id,
                    CustomerName = x.Name,
                    CustomerEmail = x.Email,
                    TotalBilled = x.Orders
                        .Where(o => !o.IsDeleted)
                        .Sum(o => (decimal?)o.TotalPrice) ?? 0,
                    TotalPaid = (x.Orders
                        .Where(o => !o.IsDeleted)
                        .Sum(o => (decimal?)o.WalletAmountUsed) ?? 0)
                        +
                        (x.Orders
                        .Where(o => !o.IsDeleted)
                        .SelectMany(o => o.Payments.Where(p => p.IsConfirmed && !p.IsDeleted))
                        .Sum(p => (decimal?)p.AmountPaid) ?? 0),
                    OutstandingBalance = x.OutstandingBalance
                })
                .OrderBy(x => x.CustomerName)
                .ToListAsync();
        }

        public void Update(Customer customer)
        {
            context.Customers.Update(customer);
        }
    }
}