using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Api;
using DeliveryApp.Core.Application.Commands.AssignOrder;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Core.Application.Commands.MoveCouriers;
using DeliveryApp.Core.Application.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.Queries.GetCreatedAndAssignedOrders;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Primitives;

var builder = WebApplication.CreateBuilder(args);

// Health Checks
builder.Services.AddHealthChecks();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin(); // Не делайте так в проде!
        });
});

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
var connectionString = builder.Configuration["CONNECTION_STRING"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString,
            sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); });
        options.EnableSensitiveDataLogging();
    }
);


// UnitOfWork
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddTransient<ICourierRepository, CourierRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();

// Domain Services
builder.Services.AddTransient<IDispatchService, DispatchService>();

// Mediator
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Commands
builder.Services.AddTransient<IRequestHandler<CreateOrderCommand, UnitResult<Error>>, CreateOrderHandler>();
builder.Services.AddTransient<IRequestHandler<AssignOrderCommand, UnitResult<Error>>, AssignOrderHandler>();
builder.Services.AddTransient<IRequestHandler<MoveCouriersCommand, UnitResult<Error>>, MoveCouriersHandler>();

// Queries
builder.Services.AddTransient<IRequestHandler<GetBusyCouriersCommand, 
    Maybe<GetCouriersResponse>>, GetBusyCouriersHandler>();
builder.Services.AddTransient<IRequestHandler<GetCreatedAndAssignedOrdersCommand, 
    Maybe<GetCreatedAndAssignedOrdersResponse>>, GetCreatedAndAssignedOrdersHandler>();

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();

// Apply Migrations
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     db.Database.Migrate();
// }

app.Run();