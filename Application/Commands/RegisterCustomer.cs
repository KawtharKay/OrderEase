using Application.Common.Dtos;
using Application.Constants;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Commands
{
    public class RegisterCustomer
    {
        public record RegisterCustomerCommand(Guid UserId, string Name, string Email, string PhoneNumber, string Address) : IRequest<Result<RegisterCustomerResponse>>;

        public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
        {
            public RegisterCustomerValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Name is required")
                    .MaximumLength(100)
                    .WithMessage("Name should not exceed 100 characters");

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email is required")
                    .EmailAddress()
                    .WithMessage("Enter a valid email address")
                    .MaximumLength(70)
                    .WithMessage("Email should not exceed 70 characters");

                RuleFor(x => x.PhoneNumber)
                    .NotEmpty()
                    .WithMessage("Phone number is required")
                    .MaximumLength(14)
                    .WithMessage("Phone number should not exceed 14 characters");

                RuleFor(x => x.Address)
                    .NotEmpty()
                    .WithMessage("Address is required")
                    .MaximumLength(80)
                    .WithMessage("Address cannot exceed 80 characters");
            }
        }

        public class RegisterCustomerHandler(ICustomerRepository customerRepository, IRoleRepository roleRepository, IUserRepository userRepository, IUnitOfWork unitOfWork) 
            : IRequestHandler<RegisterCustomerCommand, Result<RegisterCustomerResponse>>
        {
            public async Task<Result<RegisterCustomerResponse>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);
                if (user == null) return Result<RegisterCustomerResponse>.Failure("User not found");

                var customerExists = await customerRepository.GetByUserIdAsync(request.UserId);
                if (customerExists != null) return Result<RegisterCustomerResponse>.Failure("User is already registered as a customer");

                var role = await roleRepository.GetAsync(AppRoles.Customer);
                if (role == null) return Result<RegisterCustomerResponse>.Failure("Customer role not found");

                var userRole = new UserRole
                {
                    UserId = request.UserId,
                    RoleId = role.Id
                };
                await userRepository.AssignRoleAsync(userRole);

                var customer = new Customer
                {
                    UserId = request.UserId,
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    CreatedBy = request.Email,
                    DateCreated = DateTime.UtcNow
                };
                await customerRepository.AddAsync(customer);
                await unitOfWork.SaveAsync();

                return Result<RegisterCustomerResponse>.Success(customer.Adapt<RegisterCustomerResponse>(), "Customer registered successfully");
            }
        }

        public record RegisterCustomerResponse(Guid Id);
    }
}