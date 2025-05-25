using BasketService.Domain.Entities;

namespace BasketService.Domain.Tests.Entities
{
    [TestFixture]
    public class BasketTests
    {
        [Test]
        public void AddOrUpdateItem_ShouldAddNewItem_WhenItemDoesNotExist()
        {
            // Arrange
            var basket = new Basket { UserId = "user1" };
            var newItem = new BasketItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product A",
                Quantity = 2,
                UnitPrice = 10.5m
            };

            // Act
            basket.AddOrUpdateItem(newItem);

            // Assert
            Assert.That(basket.Items.Count, Is.EqualTo(1));
            var addedItem = basket.Items.First();
            Assert.That(addedItem.ProductId, Is.EqualTo(newItem.ProductId));
            Assert.That(addedItem.Quantity, Is.EqualTo(2));
            Assert.That(addedItem.ProductName, Is.EqualTo("Product A"));
            Assert.That(addedItem.UnitPrice, Is.EqualTo(10.5m));
        }

        [Test]
        public void AddOrUpdateItem_ShouldUpdateQuantity_WhenItemAlreadyExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new Basket
            {
                UserId = "user1",
                Items = new List<BasketItem>
                {
                    new BasketItem
                    {
                        ProductId = productId,
                        ProductName = "Product A",
                        Quantity = 1,
                        UnitPrice = 10.5m
                    }
                }
            };

            var newItem = new BasketItem
            {
                ProductId = productId,
                ProductName = "Product A",
                Quantity = 3,
                UnitPrice = 10.5m
            };

            // Act
            basket.AddOrUpdateItem(newItem);

            // Assert
            Assert.That(basket.Items.Count, Is.EqualTo(1));
            Assert.That(basket.Items.First().Quantity, Is.EqualTo(4));
        }

        [Test]
        public void RemoveItem_ShouldRemoveItem_WhenItemExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var basket = new Basket
            {
                UserId = "user1",
                Items = new List<BasketItem>
                {
                    new BasketItem
                    {
                        ProductId = productId,
                        ProductName = "Product A",
                        Quantity = 2,
                        UnitPrice = 9.99m
                    }
                }
            };

            // Act
            basket.RemoveItem(productId);

            // Assert
            Assert.That(basket.Items, Is.Empty);
        }

        [Test]
        public void RemoveItem_ShouldDoNothing_WhenItemDoesNotExist()
        {
            // Arrange
            var basket = new Basket
            {
                UserId = "user1",
                Items = new List<BasketItem>() // Already empty
            };
            var nonExistingProductId = Guid.NewGuid();

            // Act
            basket.RemoveItem(nonExistingProductId);

            // Assert
            Assert.That(basket.Items, Is.Empty);
        }
    }
}
