using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedAddonsAsync(db);
        await SeedPromoCodesAsync(db);
        await SeedDeliveryZonesAsync(db);
        await SeedHolidayPricingAsync(db);
        await SeedSettingsAsync(db);
        await SeedProductsAsync(db);
        await db.SaveChangesAsync();
    }

    private static async Task SeedAddonsAsync(AppDbContext db)
    {
        if (await db.Addons.AnyAsync()) return;

        db.Addons.AddRange(
            new Addon { Type = "chocolate", NameRu = "Шоколад", NameEn = "Chocolate", Price = 350, SortOrder = 1, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new Addon { Type = "teddy", NameRu = "Мягкая игрушка", NameEn = "Teddy Bear", Price = 850, SortOrder = 2, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new Addon { Type = "choco_strawberry", NameRu = "Клубника в шоколаде", NameEn = "Chocolate Strawberry", Price = 650, SortOrder = 3, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new Addon { Type = "bento_cake", NameRu = "Бенто-торт", NameEn = "Bento Cake", Price = 1200, HasInscription = true, SortOrder = 4, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
    }

    private static async Task SeedPromoCodesAsync(AppDbContext db)
    {
        if (await db.PromoCodes.AnyAsync()) return;

        db.PromoCodes.AddRange(
            new PromoCode { Code = "WELCOME20", LabelRu = "Скидка 20% на первый заказ", DiscountPercent = 20, ValidFrom = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new PromoCode { Code = "WELCOME15", LabelRu = "Приветственная скидка 15%", DiscountPercent = 15, ValidFrom = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new PromoCode { Code = "FLOWER10", LabelRu = "Скидка 10%", DiscountPercent = 10, ValidFrom = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new PromoCode { Code = "SPRING20", LabelRu = "Весенняя скидка 20%", DiscountPercent = 20, MinOrder = 500, ValidFrom = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
    }

    private static async Task SeedDeliveryZonesAsync(AppDbContext db)
    {
        if (await db.DeliveryZones.AnyAsync()) return;

        db.DeliveryZones.AddRange(
            new DeliveryZone { NameRu = "Центр Душанбе", Fee = 30, MinHours = 1, MaxHours = 2, SortOrder = 1, CreatedAt = DateTimeOffset.UtcNow },
            new DeliveryZone { NameRu = "Север (Сино, Бохтар)", Fee = 40, MinHours = 2, MaxHours = 3, SortOrder = 2, CreatedAt = DateTimeOffset.UtcNow },
            new DeliveryZone { NameRu = "Юг (Исмоили Сомони)", Fee = 40, MinHours = 2, MaxHours = 3, SortOrder = 3, CreatedAt = DateTimeOffset.UtcNow },
            new DeliveryZone { NameRu = "Пригород", Fee = 60, MinHours = 3, MaxHours = 5, SortOrder = 4, CreatedAt = DateTimeOffset.UtcNow }
        );
    }

    private static async Task SeedHolidayPricingAsync(AppDbContext db)
    {
        if (await db.HolidayPricings.AnyAsync()) return;

        db.HolidayPricings.AddRange(
            new HolidayPricing { NameRu = "День Святого Валентина", Month = 2, Day = 14, MarkupPercent = 30 },
            new HolidayPricing { NameRu = "Международный женский день", Month = 3, Day = 8, MarkupPercent = 30 },
            new HolidayPricing { NameRu = "Навруз", Month = 3, Day = 20, MarkupPercent = 30 },
            new HolidayPricing { NameRu = "Навруз", Month = 3, Day = 21, MarkupPercent = 30 },
            new HolidayPricing { NameRu = "Новый год", Month = 12, Day = 31, MarkupPercent = 30 }
        );
    }

    private static async Task SeedSettingsAsync(AppDbContext db)
    {
        if (await db.Settings.AnyAsync()) return;

        var settings = new[]
        {
            ("shop_name", "Flower Shop", "Название магазина"),
            ("shop_phone", "+992004048769", "Основной телефон"),
            ("shop_telegram", "@yakubov_111", "Telegram"),
            ("shop_instagram", "@yakubov.abdullo_", "Instagram"),
            ("shop_open_time", "06:00", "Время открытия"),
            ("shop_close_time", "00:00", "Время закрытия"),
            ("default_delivery_fee", "40", "Стоимость доставки по умолчанию"),
            ("free_delivery_threshold", "1500", "Порог бесплатной доставки"),
            ("loyalty_earn_divisor", "20", "1 балл за X сомони"),
            ("loyalty_min_redeem", "50", "Минимум баллов для списания"),
            ("loyalty_max_redeem_pct", "30", "Максимум % от заказа для списания"),
            ("loyalty_silver_threshold", "300", "Баллов для тира Silver"),
            ("loyalty_gold_threshold", "1000", "Баллов для тира Gold"),
            ("otp_expire_minutes", "10", "Время жизни OTP-кода"),
            ("otp_max_attempts", "3", "Максимум попыток ввода OTP"),
            ("otp_max_send_per_hour", "5", "Максимум отправок OTP за час")
        };

        foreach (var (key, value, desc) in settings)
            db.Settings.Add(new Setting { Key = key, Value = value, Description = desc, UpdatedAt = DateTimeOffset.UtcNow });
    }

    private static async Task SeedProductsAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        // 1. Красные розы "Страсть"
        var redRoses = AddProduct(db, "krasnye-rozy-25", "Красные розы «Страсть»", "Red Roses «Passion»",
            "25 роз сорта Фридом, гипсофила, атласная лента",
            "25 Freedom roses, baby's breath, satin ribbon",
            800, 960, 17, isHit: true, deliverToday: true, sortOrder: 1);
        AddImages(db, redRoses,
            "https://images.unsplash.com/photo-1548142813-c348350df52b?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1455582916367-25f75bfc6710?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, redRoses, (SizeLabel.S, 480, 0), (SizeLabel.M, 800, 960), (SizeLabel.L, 1220, 0));
        AddOccasions(db, redRoses, Occasion.Birthday, Occasion.Wedding, Occasion.Romantic);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = redRoses.Id, FlowerType = FlowerType.Rose });
        db.ProductColors.Add(new ProductColor { ProductId = redRoses.Id, Color = "red" });
        AddPackaging(db, redRoses, Packaging.Basket, Packaging.NoBasket);

        // 2. Белые лилии "Чистота"
        var whiteLilies = AddProduct(db, "belye-lilii-7", "Белые лилии «Чистота»", "White Lilies «Purity»",
            "7 лилий Oriental, декоративная зелень, крафт-бумага",
            "7 Oriental lilies, decorative greens, kraft paper",
            580, 0, 0, isNew: true, deliverToday: false, sortOrder: 2);
        AddImages(db, whiteLilies,
            "https://images.unsplash.com/photo-1562887009-900cfe5451b4?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1519378058457-4c29a0a2efac?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, whiteLilies, (SizeLabel.S, 380, 0), (SizeLabel.M, 580, 0), (SizeLabel.L, 830, 0));
        AddOccasions(db, whiteLilies, Occasion.Birthday, Occasion.Mom, Occasion.Other);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = whiteLilies.Id, FlowerType = FlowerType.Mixed });
        db.ProductColors.Add(new ProductColor { ProductId = whiteLilies.Id, Color = "white" });
        AddPackaging(db, whiteLilies, Packaging.NoBasket);

        // 3. Розовые пионы "Нежность"
        var pinkPeonies = AddProduct(db, "rozovye-piony-15", "Розовые пионы «Нежность»", "Pink Peonies «Tenderness»",
            "15 пионов сорта Сара Бернар, листья эвкалипта",
            "15 Sarah Bernhardt peonies, eucalyptus leaves",
            1120, 1440, 22, isHit: true, deliverToday: false, sortOrder: 3);
        AddImages(db, pinkPeonies,
            "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1561181286-d3fee7d55364?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, pinkPeonies, (SizeLabel.S, 700, 0), (SizeLabel.M, 1120, 1440), (SizeLabel.L, 1660, 0));
        AddOccasions(db, pinkPeonies, Occasion.Birthday, Occasion.Wedding, Occasion.Mom);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = pinkPeonies.Id, FlowerType = FlowerType.Peony });
        db.ProductColors.Add(new ProductColor { ProductId = pinkPeonies.Id, Color = "pink" });
        AddPackaging(db, pinkPeonies, Packaging.Basket, Packaging.NoBasket);

        // 4. Подсолнухи "Солнышко"
        var sunflowers = AddProduct(db, "podsolnukhi-11", "Подсолнухи «Солнышко»", "Sunflowers «Sunshine»",
            "11 подсолнухов, зелень, джутовая упаковка",
            "11 sunflowers, greenery, jute packaging",
            350, 0, 0, isHit: false, deliverToday: true, sortOrder: 4);
        AddImages(db, sunflowers,
            "https://images.unsplash.com/photo-1468327768560-75b778cbb551?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1508700929628-666bc8bd84ea?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, sunflowers, (SizeLabel.S, 220, 0), (SizeLabel.M, 350, 0), (SizeLabel.L, 540, 0));
        AddOccasions(db, sunflowers, Occasion.Birthday, Occasion.Other);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = sunflowers.Id, FlowerType = FlowerType.Mixed });
        db.ProductColors.Add(new ProductColor { ProductId = sunflowers.Id, Color = "yellow" });
        AddPackaging(db, sunflowers, Packaging.NoBasket);

        // 5. Орхидеи "Изысканность"
        var orchids = AddProduct(db, "orkhidei-belye-3", "Орхидеи «Изысканность»", "Orchids «Elegance»",
            "3 ветки белых орхидей Фаленопсис, декоративный мох",
            "3 Phalaenopsis white orchid branches, decorative moss",
            900, 0, 0, isNew: true, deliverToday: false, sortOrder: 5);
        AddImages(db, orchids,
            "https://images.unsplash.com/photo-1566598932249-3b4cbf7ee65e?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1550159930-40066082a4fc?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, orchids, (SizeLabel.S, 580, 0), (SizeLabel.M, 900, 0), (SizeLabel.L, 1340, 0));
        AddOccasions(db, orchids, Occasion.Birthday, Occasion.Wedding, Occasion.Other);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = orchids.Id, FlowerType = FlowerType.Orchid });
        db.ProductColors.Add(new ProductColor { ProductId = orchids.Id, Color = "white" });
        AddPackaging(db, orchids, Packaging.Basket);

        // 6. Луговой микс "Полевые"
        var wildMix = AddProduct(db, "lugovoy-miks-25", "Луговой микс «Полевые»", "Wildflower Mix «Meadow»",
            "25 стеблей: ромашки, лаванда, эхинацея, зелень",
            "25 stems: chamomile, lavender, echinacea, greenery",
            480, 580, 17, deliverToday: true, sortOrder: 6);
        AddImages(db, wildMix,
            "https://images.unsplash.com/photo-1490750967868-88df5691cc23?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1462275646964-a0e3386b89fa?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, wildMix, (SizeLabel.S, 290, 0), (SizeLabel.M, 480, 580), (SizeLabel.L, 700, 0));
        AddOccasions(db, wildMix, Occasion.Birthday, Occasion.Other);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = wildMix.Id, FlowerType = FlowerType.Mixed });
        db.ProductColors.Add(new ProductColor { ProductId = wildMix.Id, Color = "purple" });
        AddPackaging(db, wildMix, Packaging.NoBasket);

        // 7. Герберы "Яркость"
        var gerbers = AddProduct(db, "gerbery-oranzhevye-15", "Герберы «Яркость»", "Gerberas «Brightness»",
            "15 оранжевых гербер, итальянская зелень, упаковка",
            "15 orange gerberas, Italian greens, packaging",
            420, 0, 0, deliverToday: true, sortOrder: 7);
        AddImages(db, gerbers,
            "https://images.unsplash.com/photo-1591261730799-ee4e6c2d16d7?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1520763185298-1b434c919102?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, gerbers, (SizeLabel.S, 260, 0), (SizeLabel.M, 420, 0), (SizeLabel.L, 640, 0));
        AddOccasions(db, gerbers, Occasion.Birthday, Occasion.Other);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = gerbers.Id, FlowerType = FlowerType.Mixed });
        db.ProductColors.Add(new ProductColor { ProductId = gerbers.Id, Color = "orange" });
        AddPackaging(db, gerbers, Packaging.NoBasket);

        // 8. Свадебный букет "Невеста"
        var wedding = AddProduct(db, "svadebnyy-buket-nevesta", "Свадебный букет «Невеста»", "Wedding Bouquet «Bride»",
            "21 белая роза, эустома, ранункулюс, атласная лента",
            "21 white roses, eustoma, ranunculus, satin ribbon",
            1440, 1760, 18, isHit: true, deliverToday: false, sortOrder: 8);
        AddImages(db, wedding,
            "https://images.unsplash.com/photo-1522441815192-d9f04eb0615c?w=600&h=800&fit=crop&auto=format&q=80",
            "https://images.unsplash.com/photo-1519657337289-077653f724ed?w=600&h=800&fit=crop&auto=format&q=80");
        AddSizes(db, wedding, (SizeLabel.S, 900, 0), (SizeLabel.M, 1440, 1760), (SizeLabel.L, 2180, 0));
        AddOccasions(db, wedding, Occasion.Wedding, Occasion.Birthday);
        db.ProductFlowerTypes.Add(new ProductFlowerType { ProductId = wedding.Id, FlowerType = FlowerType.Rose });
        db.ProductColors.Add(new ProductColor { ProductId = wedding.Id, Color = "white" });
        AddPackaging(db, wedding, Packaging.Basket, Packaging.NoBasket);
    }

    private static Product AddProduct(AppDbContext db, string slug, string nameRu, string nameEn,
        string compositionRu, string compositionEn,
        int price, int oldPrice = 0, int discount = 0,
        bool isNew = false, bool isHit = false, bool deliverToday = true, int sortOrder = 0)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            NameRu = nameRu,
            NameEn = nameEn,
            CompositionRu = compositionRu,
            CompositionEn = compositionEn,
            Price = price,
            OldPrice = oldPrice > 0 ? oldPrice : null,
            Discount = discount > 0 ? discount : null,
            Rating = Math.Round((decimal)(4.5 + new Random().NextDouble() * 0.5), 1),
            ReviewCount = new Random().Next(20, 120),
            IsNew = isNew,
            IsHit = isHit,
            InStock = true,
            IsActive = true,
            DeliverToday = deliverToday,
            Kind = ProductKind.Bouquet,
            Freshness = Freshness.Live,
            SortOrder = sortOrder,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Products.Add(product);
        return product;
    }

    private static void AddImages(AppDbContext db, Product product, params string[] urls)
    {
        for (int i = 0; i < urls.Length; i++)
            db.ProductImages.Add(new ProductImage { ProductId = product.Id, Url = urls[i], SortOrder = i + 1 });
    }

    private static void AddSizes(AppDbContext db, Product product, params (SizeLabel Label, int Price, int OldPrice)[] sizes)
    {
        for (int i = 0; i < sizes.Length; i++)
            db.ProductSizes.Add(new ProductSize
            {
                ProductId = product.Id,
                Label = sizes[i].Label,
                Price = sizes[i].Price,
                OldPrice = sizes[i].OldPrice > 0 ? sizes[i].OldPrice : null,
                SortOrder = i + 1
            });
    }

    private static void AddOccasions(AppDbContext db, Product product, params Occasion[] occasions)
    {
        foreach (var o in occasions)
            db.ProductOccasions.Add(new ProductOccasion { ProductId = product.Id, Occasion = o });
    }

    private static void AddPackaging(AppDbContext db, Product product, params Packaging[] packagings)
    {
        foreach (var p in packagings)
            db.ProductPackagings.Add(new ProductPackaging { ProductId = product.Id, Packaging = p });
    }
}
