using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace CatalogService.Tests.Repositories
{
    public class BasketRepositoryTests
    {
        private BasketDbContext _dbContext;
        private BasketRepository _repository;
        private ILogger<BasketRepository> _logger;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<BasketDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB for each test
                .Options;

            _dbContext = new BasketDbContext(options);
            _dbContext.Database.EnsureCreated();

            _logger = Substitute.For<ILogger<BasketRepository>>();
            _repository = new BasketRepository(_logger, _dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }

        [Test]
        public async Task SaveAsync_Should_Add_New_Basket_When_It_Does_Not_Exist()
        {
            var productId = Guid.NewGuid();
            var basket = new Basket
            {
                UserId = "user1",
                Items = new List<BasketItem>
                {
                    new BasketItem { ProductId = productId, ProductName="prod name", Quantity = 2 }
                }
            };

            await _repository.SaveAsync(basket);

            var saved = await _repository.GetByUserIdAsync("user1");

            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.Items, Is.Not.Null);
            Assert.That(saved.Items.Count, Is.EqualTo(1));
            Assert.That(saved.Items.First().ProductId, Is.EqualTo(productId));
        }

        [Test]
        public async Task SaveAsync_Should_Update_Items_When_Basket_Exists()
        {
            var initialProductId = Guid.NewGuid();
            var updatedProductId = Guid.NewGuid();

            var existingBasket = new Basket
            {
                UserId = "user2",
                Items = new List<BasketItem>
        {
            new BasketItem
            {
                ProductId = initialProductId,
                ProductName = "prod name",
                Quantity = 1,
                UnitPrice = 5
            }
        }
            };

            _dbContext.Baskets.Add(existingBasket);
            await _dbContext.SaveChangesAsync();

            var updatedBasket = new Basket
            {
                UserId = "user2",
                Items = new List<BasketItem>
                {
            new BasketItem
                    {
                        ProductId = updatedProductId,
                        ProductName = "new prod name",
                        Quantity = 3,
                        UnitPrice = 10
                    }
                }
            };

            await _repository.SaveAsync(updatedBasket);

            var saved = await _repository.GetByUserIdAsync("user2");

            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.Items.Count, Is.EqualTo(1));
            Assert.That(saved.Items.First().ProductId, Is.EqualTo(updatedProductId));
            Assert.That(saved.Items.First().Quantity, Is.EqualTo(3));
        }

        [Test]
        public async Task DeleteAsync_Should_Remove_Existing_Basket()
        {
            var productId = Guid.NewGuid();
            var basket = new Basket
            {
                UserId = "user3",
                Items = new List<BasketItem>
                {
                    new BasketItem { ProductId = productId, ProductName = "Test Product", Quantity = 1 }
                }
            };

            _dbContext.Baskets.Add(basket);
            await _dbContext.SaveChangesAsync();

            await _repository.DeleteAsync("user3");

            var deleted = await _repository.GetByUserIdAsync("user3");

            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task GetByUserIdAsync_Should_Return_Null_When_Not_Found()
        {
            var result = await _repository.GetByUserIdAsync("unknown_user");

            Assert.That(result, Is.Null);
        }
    }
}
