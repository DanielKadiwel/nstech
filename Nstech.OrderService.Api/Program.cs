using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nstech.OrderService.Application.Commands;
using Nstech.OrderService.Infrastructure;
using Nstech.OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Order Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Insira o token JWT neste formato: Bearer SEU_TOKEN_AQUI",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Infrastructure (EF Core, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Application (MediatR)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"] ?? "SuperSecretKeyForTestingPurposeOnly123456789!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "Nstech",
            ValidAudience = jwtSettings["Audience"] ?? "NstechApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply Auto Migrations & Seed Products
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var initialProducts = new[]
    {
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Notebook Gamer Dell G15", 4500.00m, 10),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Mouse Sem Fio Logitech MX Master 3S", 150.00m, 50),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Monitor UltraWide LG 29 IPS", 1299.90m, 15),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Teclado Mecanico Keychron K2", 650.00m, 25),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Headset Gamer HyperX Cloud II", 480.00m, 30),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("66666666-6666-6666-6666-666666666666"), "Cadeira Gamer Ergonomica Noblechairs", 2100.00m, 5),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("77777777-7777-7777-7777-777777777777"), "Smartphone Samsung Galaxy S23", 3899.00m, 12),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("88888888-8888-8888-8888-888888888888"), "Webcam Full HD Logitech C920", 399.90m, 20),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("99999999-9999-9999-9999-999999999999"), "SSD NVMe M.2 1TB Kingston", 450.00m, 40),
        new Nstech.OrderService.Domain.Entities.Product(Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"), "Processador Intel Core i7 13700K", 2499.00m, 8)
    };

    bool hasChanges = false;
    foreach (var product in initialProducts)
    {
        if (!dbContext.Products.Any(p => p.Id == product.Id))
        {
            dbContext.Products.Add(product);
            hasChanges = true;
        }
    }

    if (hasChanges)
    {
        dbContext.SaveChanges();
    }

    if (!dbContext.Orders.Any())
    {
        var idProperty = typeof(Nstech.OrderService.Domain.Entities.Order).GetProperty(nameof(Nstech.OrderService.Domain.Entities.Order.Id));

        var sampleOrder1 = new Nstech.OrderService.Domain.Entities.Order(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "BRL");
        idProperty?.SetValue(sampleOrder1, Guid.Parse("10000000-0000-0000-0000-000000000001"));
        sampleOrder1.AddItem(Guid.Parse("11111111-1111-1111-1111-111111111111"), 4500.00m, 1);
        sampleOrder1.AddItem(Guid.Parse("22222222-2222-2222-2222-222222222222"), 150.00m, 2);
        sampleOrder1.Place();

        var sampleOrder2 = new Nstech.OrderService.Domain.Entities.Order(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "BRL");
        idProperty?.SetValue(sampleOrder2, Guid.Parse("20000000-0000-0000-0000-000000000002"));
        sampleOrder2.AddItem(Guid.Parse("33333333-3333-3333-3333-333333333333"), 1299.90m, 1);
        sampleOrder2.AddItem(Guid.Parse("44444444-4444-4444-4444-444444444444"), 650.00m, 1);
        sampleOrder2.Place();
        sampleOrder2.Confirm();

        dbContext.Orders.AddRange(sampleOrder1, sampleOrder2);
        dbContext.SaveChanges();
    }
}

app.Run();
