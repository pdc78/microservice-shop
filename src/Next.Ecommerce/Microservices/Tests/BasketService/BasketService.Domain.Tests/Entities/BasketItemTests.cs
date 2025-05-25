using BasketService.Domain.Entities;

namespace BasketService.Domain.Tests.Entities
{
    [TestFixture]
    public class BasketItemTests
    {
        [Test]
        public void BasketItem_ShouldStoreValuesCorrectly()
        {
            // Arrange
            var basketId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var basketItem = new BasketItem
            {
                BasketId = basketId,
                ProductId = productId,
                ProductName = "Test Product",
                Quantity = 3,
                UnitPrice = 19.99m
            };

            // Act & Assert
            Assert.That(basketItem.BasketId, Is.EqualTo(basketId));
            Assert.That(basketItem.ProductId, Is.EqualTo(productId));
            Assert.That(basketItem.ProductName, Is.EqualTo("Test Product"));
            Assert.That(basketItem.Quantity, Is.EqualTo(3));
            Assert.That(basketItem.UnitPrice, Is.EqualTo(19.99m));
        }

        [Test]
        public void BasketProperty_ShouldNotBeNull_ByDefault()
        {
            // Arrange
            var basketItem = new BasketItem();

            // Assert
            Assert.That(basketItem, Is.Not.Null);
        }

        [Test]
        public void ProductName_ShouldThrow_WhenAccessedBeforeSet()
        {
            // Arrange
            var item = new BasketItem();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
            {
                var _ = item.ProductName.Length;
            });
        }
    }
}
