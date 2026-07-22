using Application.Common.Settings;
using Application.Constants;
using Application.Repositories;
using Domain.Entities;
using Host.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatRWithBehaviors();
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configure EmailSettings and register EmailService
builder.Services.Configure<Application.Common.Settings.EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);
// Bind AppSettings
builder.Services.Configure<Application.Common.Settings.AppSettings>(
    builder.Configuration.GetSection("AppSettings")
);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrderEase API",
        Version = "v1",
        Description = "API for managing book orders"
    });

    // Define the Bearer security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    });

    // Add security requirement using DELEGATE (NEW in v10)
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        }
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var supplierRepository = scope.ServiceProvider.GetRequiredService<ISupplierRepository>();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    var existingSupplier = await supplierRepository.GetFirstAsync();
    if (existingSupplier is null)
    {
        const string seedEmail = "orderease111@gmail.com";
        const string seedPassword = "Pa$$word";   

        var salt = Guid.NewGuid().ToString();
        var user = new User
        {
            Email = seedEmail,
            Salt = salt,
            CreatedBy = "seed",
            IsVerified = true,
            DateCreated = DateTime.UtcNow
        };
        user.HashPassword = passwordHasher.HashPassword(user, $"{salt}{seedPassword}");

        await userRepository.AddAsync(user);
        await unitOfWork.SaveAsync();

        var supplierRole = await roleRepository.GetAsync(AppRoles.Supplier);
        if (supplierRole != null)
        {
            await userRepository.AssignRoleAsync(new UserRole { UserId = user.Id, RoleId = supplierRole.Id });
        }

        var supplier = new Supplier
        {
            UserId = user.Id,
            Name = "OrderEase (WASHO ENTERPRISE)", 
            Email = seedEmail,
            PhoneNumber = "08020502701",
            Address = "Km 27, Lagos-Abeokuta Expressway, Lagos, Nigeria",
            CreatedBy = "",
            DateCreated = DateTime.UtcNow
        };
        await supplierRepository.AddAsync(supplier);
        await unitOfWork.SaveAsync();
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderEase API v1");
    });
}
app.MapHub<NotificationHub>("/hubs/notifications");


app.Run();
