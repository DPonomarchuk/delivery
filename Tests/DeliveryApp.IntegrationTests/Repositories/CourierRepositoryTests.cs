using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class CourierRepositoryShould : IAsyncLifetime
{
    /// <summary>
    ///     Настройка Postgres из библиотеки TestContainers
    /// </summary>
    /// <remarks>По сути это Docker контейнер с Postgres</remarks>
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:14")
        .WithDatabase("courier")
        .WithUsername("username")
        .WithPassword("secret")
        .WithCleanUp(true)
        .Build();

    private ApplicationDbContext _context;
    private IMediator _mediator;

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <remarks>Вызывается один раз перед всеми тестами в рамках этого класса</remarks>
    public CourierRepositoryShould()
    {
        _mediator = Substitute.For<IMediator>();
        _mediator.Publish(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
    }

    /// <summary>
    ///     Инициализируем окружение
    /// </summary>
    /// <remarks>Вызывается перед каждым тестом</remarks>
    public async Task InitializeAsync()
    {
        //Стартуем БД (библиотека TestContainers запускает Docker контейнер с Postgres)
        await _postgreSqlContainer.StartAsync();

        //Накатываем миграции и справочники
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
            .Options;
        _context = new ApplicationDbContext(contextOptions);
        _context.Database.Migrate();
    }

    /// <summary>
    ///     Уничтожаем окружение
    /// </summary>
    /// <remarks>Вызывается после каждого теста</remarks>
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task CanAddCourier()
    {
        //Arrange
        var courier = Courier.Create("Антон", 1, Location.Create(1, 1).Value).Value;

        //Act
        var courierRepository = new CourierRepository(_context);
        var unitOfWork = new UnitOfWork(_context, _mediator);

        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var getCourierResult = await courierRepository.GetAsync(courier.Id);
        getCourierResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierResult.Value;
        courier.Should().BeEquivalentTo(courierFromDb);
    }

    [Fact]
    public async Task CanUpdateCourierWithAddingNewStoragePlace()
    {
        //Arrange
        var courier = Courier.Create("Эльдар", 1, Location.Create(1, 1).Value).Value;

        var courierRepository = new CourierRepository(_context);
        var unitOfWork = new UnitOfWork(_context, _mediator);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        //Act
        var courierAddStoragePlace = courier.AddStoragePlace("Корзина", 13);
        courierAddStoragePlace.IsSuccess.Should().BeTrue();
        courierRepository.Update(courier);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var getCourierResult = await courierRepository.GetAsync(courier.Id);
        getCourierResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierResult.Value;
        courierFromDb.StoragePlaces.Should().HaveCount(2);
        courierFromDb.StoragePlaces.ElementAt(1).Should().BeEquivalentTo(new {
            Name = "Корзина",
            TotalVolume = 13
        },  options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task CanGetById()
    {
        //Arrange
        var courier = Courier.Create("Иван", 1, Location.Create(1, 1).Value).Value;

        //Act
        var courierRepository = new CourierRepository(_context);
        var unitOfWork = new UnitOfWork(_context, _mediator);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var getCourierResult = await courierRepository.GetAsync(courier.Id);
        getCourierResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierResult.Value;
        courier.Should().BeEquivalentTo(courierFromDb);
    }

    [Fact]
    public async Task CanGetAllFree()
    {
        //Arrange
        var courierRepository = new CourierRepository(_context);
        var courier = Courier.Create("Иван", 1, Location.Create(1, 1).Value).Value;
        var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 5).Value;
        courier.TakeOrder(order);
        await courierRepository.AddAsync(courier);

        var courier2 = Courier.Create("Антон", 1, Location.Create(2, 2).Value).Value;
        var order2 = Order.Create(Guid.NewGuid(), Location.Create(2, 2).Value, 5).Value;
        courier2.TakeOrder(order2);
        await courierRepository.AddAsync(courier2);

        var courier3 = Courier.Create("Илья", 1, Location.Create(3, 3).Value).Value;
        await courierRepository.AddAsync(courier3);

        var courier4 = Courier.Create("Себастьян", 1, Location.Create(4, 4).Value).Value;
        await courierRepository.AddAsync(courier4);

        var unitOfWork = new UnitOfWork(_context, _mediator);
        await unitOfWork.SaveChangesAsync();

        //Act
        var freeCouriersResult = await courierRepository.GetAllFree();

        // Assert
        freeCouriersResult.Should().NotBeEmpty();
        freeCouriersResult.Should().HaveCount(2);
        freeCouriersResult.Should().Contain(courier3);
        freeCouriersResult.Should().Contain(courier4);
    }
}
