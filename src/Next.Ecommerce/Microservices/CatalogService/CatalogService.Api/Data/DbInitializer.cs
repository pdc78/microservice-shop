﻿using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Data;

namespace CatalogService.API.Data;

public static class DbInitializer
{
    public static void Seed(CatalogDbContext context)
    {
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product { Id = Guid.NewGuid(), Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 25.99m, ImageUrl = "https://example.com/images/mouse.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "Mechanical Keyboard", Description = "RGB backlit mechanical keyboard", Price = 79.99m, ImageUrl = "https://example.com/images/keyboard.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "27\" Monitor", Description = "4K UHD monitor with HDR", Price = 299.99m, ImageUrl = "https://example.com/images/monitor.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "USB-C Hub", Description = "Multi-port USB-C hub with HDMI", Price = 39.99m, ImageUrl = "https://example.com/images/hub.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "External SSD", Description = "1TB high-speed SSD drive", Price = 119.99m, ImageUrl = "https://example.com/images/ssd.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "Webcam", Description = "HD webcam with microphone", Price = 49.99m, ImageUrl = "https://example.com/images/webcam.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "Laptop Stand", Description = "Adjustable aluminum laptop stand", Price = 34.99m, ImageUrl = "https://example.com/images/stand.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "Bluetooth Speaker", Description = "Portable waterproof speaker", Price = 59.99m, ImageUrl = "https://example.com/images/speaker.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "Smart LED Light", Description = "Wi-Fi enabled smart bulb", Price = 19.99m, ImageUrl = "https://example.com/images/light.jpg" },
                new Product { Id = Guid.NewGuid(), Name = "Gaming Chair", Description = "Ergonomic gaming chair with lumbar support", Price = 189.99m, ImageUrl = "https://example.com/images/chair.jpg" }
            );

            context.SaveChanges();
        }
    }
}