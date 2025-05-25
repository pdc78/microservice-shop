using BasketService.Application.Interfaces;
using BasketService.Application.Services;
using BasketService.Domain.Entities;
using NSubstitute;

namespace BasketService.Application.Tests.Services
{
    [TestFixture]
    public class CartServiceTests
    {
        private IBasketRepository _repository = null!;
        private CartService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repository = Substitute.For<IBasketRepository>();
            _service = new CartService(_repository);
        }

        [Test]
        public async Task AddItemAsync_ShouldAddNewItem_WhenBasketDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var item = new BasketItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Quantity = 1,
                UnitPrice = 10.0m
            };

            _repository.GetByUserIdAsync(userId).Returns((Basket?)null);

            // Act
            var result = await _service.AddItemAsync(userId, item);

            // Assert
            await _repository.Received(1).SaveAsync(Arg.Is<Basket>(b =>
                b.UserId == userId &&
                b.Items.Count == 1 &&
                b.Items[0].ProductId == item.ProductId));

            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddItemAsync_ShouldUpdateItem_WhenBasketAlreadyExists()
        {
            // Arrange
            var userId = "user1";
            var productId = Guid.NewGuid();

            var existingBasket = new Basket
            {
                UserId = userId,
                Items = new List<BasketItem>
                {
                    new BasketItem
                    {
                        ProductId = productId,
                        ProductName = "Existing Product",
                        Quantity = 2,
                        UnitPrice = 5.0m
                    }
                }
            };

            var newItem = new BasketItem
            {
                ProductId = productId,
                ProductName = "Existing Product",
                Quantity = 3,
                UnitPrice = 5.0m
            };

            _repository.GetByUserIdAsync(userId).Returns(existingBasket);

            // Act
            var result = await _service.AddItemAsync(userId, newItem);

            // Assert
            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items[0].Quantity, Is.EqualTo(5));
            await _repository.Received(1).SaveAsync(existingBasket);
        }

        [Test]
        public async Task GetBasketAsync_ShouldReturnExistingBasket()
        {
            // Arrange
            var userId = "user1";
            var basket = new Basket { UserId = userId };
            _repository.GetByUserIdAsync(userId).Returns(basket);

            // Act
            var result = await _service.GetBasketAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.UserId, Is.EqualTo(userId));
        }

        [Test]
        public async Task GetBasketAsync_ShouldReturnNewBasket_WhenNoneExists()
        {
            // Arrange
            var userId = "user2";
            _repository.GetByUserIdAsync(userId).Returns((Basket?)null);

            // Act
            var result = await _service.GetBasketAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.UserId, Is.EqualTo(userId));
        }

        [Test]
        public async Task RemoveItemAsync_ShouldRemoveItem_WhenBasketAndItemExist()
        {
            // Arrange
            var userId = "user1";
            var productId = Guid.NewGuid();
            var basket = new Basket
            {
                UserId = userId,
                Items = new List<BasketItem>
                {
                    new BasketItem
                    {
                        ProductId = productId,
                        ProductName = "ToRemove",
                        Quantity = 1,
                        UnitPrice = 2.0m
                    }
                }
            };

            _repository.GetByUserIdAsync(userId).Returns(basket);

            // Act
            await _service.RemoveItemAsync(userId, productId);

            // Assert
            Assert.That(basket.Items, Is.Empty);
            await _repository.Received(1).SaveAsync(basket);
        }

        [Test]
        public async Task RemoveItemAsync_ShouldDoNothing_WhenBasketDoesNotExist()
        {
            // Arrange
            var userId = "nonexistent";
            _repository.GetByUserIdAsync(userId).Returns((Basket?)null);

            // Act
            await _service.RemoveItemAsync(userId, Guid.NewGuid());

            // Assert
            await _repository.DidNotReceive().SaveAsync(Arg.Any<Basket>());
        }

        [Test]
        public async Task DeleteBasketAsync_ShouldCallRepositoryDelete()
        {
            // Arrange
            var userId = "user1";

            // Act
            await _service.DeleteBasketAsync(userId);

            // Assert
            await _repository.Received(1).DeleteAsync(userId);
        }
    }
}
