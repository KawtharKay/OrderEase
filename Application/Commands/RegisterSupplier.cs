using Application.Common.Dtos;
using Application.Constants;
using Application.Repositories;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Commands
{
    public class RegisterSupplier
    {
        public record RegisterSupplierCommand(Guid UserId, string Name, string Email, string PhoneNumber, string Address) : IRequest<Result<RegisterSupplierResponse>>;

        public class RegisterSupplierValidator : AbstractValidator<RegisterSupplierCommand>
        {
            public RegisterSupplierValidator()
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

        public class RegisterSupplierHandler(ISupplierRepository supplierRepository, IRoleRepository roleRepository, IUserRepository userRepository,IUnitOfWork unitOfWork) 
            : IRequestHandler<RegisterSupplierCommand, Result<RegisterSupplierResponse>>
        {
            public async Task<Result<RegisterSupplierResponse>> Handle(RegisterSupplierCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);
                if (user == null) return Result<RegisterSupplierResponse>.Failure("User not found");

                var supplierExists = await supplierRepository.GetByUserIdAsync(request.UserId);
                if (supplierExists != null) return Result<RegisterSupplierResponse>.Failure("User is already registered as a supplier");

                var role = await roleRepository.GetAsync(AppRoles.Supplier);
                if (role == null) return Result<RegisterSupplierResponse>.Failure("Supplier role not found");

                var userRole = new UserRole
                {
                    UserId = request.UserId,
                    RoleId = role.Id
                };
                await userRepository.AssignRoleAsync(userRole);

                var supplier = new Supplier
                {
                    UserId = request.UserId,
                    Name = request.Name,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    CreatedBy = request.Email,
                    DateCreated = DateTime.UtcNow
                };
                await supplierRepository.AddAsync(supplier);
                await unitOfWork.SaveAsync();

                return Result<RegisterSupplierResponse>.Success(supplier.Adapt<RegisterSupplierResponse>(), "Registration successful! Please click on the link in your email to verify your account.");
            }
        }

        public record RegisterSupplierResponse(Guid Id);
    }
}