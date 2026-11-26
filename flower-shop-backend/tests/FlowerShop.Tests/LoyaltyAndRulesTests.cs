using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Application.Validators;
using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Tests;

public class LoyaltyServiceTests
{
    private static Order NewOrder(TestDb t)
    {
        var order = new Order { Id = Guid.NewGuid(), OrderNumber = "#000001", UserId = t.UserId, Address = "a",
            DeliveryTime = OrderConstants.TimeSlots[0], CustomerName = "Test", CustomerPhone = "+992900000001" };
        t.Db.Orders.Add(order);
        return order;
    }

    [Fact]
    public async Task Redeem_then_earn_then_revert_restores_balance()
    {
        using var t = new TestDb();
        t.SetPoints(200);
        var loyalty = new LoyaltyService(t.Db);
        var order = NewOrder(t);

        await loyalty.RedeemPointsAsync(t.UserId, 100, order, default);
        var earned = await loyalty.EarnPointsAsync(t.UserId, 1000, order, default);
        await t.Db.SaveChangesAsync();

        var account = await t.Db.LoyaltyAccounts.SingleAsync(a => a.UserId == t.UserId);
        Assert.Equal(50, earned);
        Assert.Equal(200 - 100 + 50, account.Points);

        await loyalty.RevertOrderAsync(t.UserId, order, default);
        await t.Db.SaveChangesAsync();

        Assert.Equal(200, account.Points);
        Assert.Equal(200, account.TotalEarned);
    }

    [Fact]
    public async Task Redeem_more_than_balance_fails()
    {
        using var t = new TestDb();
        t.SetPoints(60);
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            new LoyaltyService(t.Db).RedeemPointsAsync(t.UserId, 100, NewOrder(t), default));
        Assert.Equal("VALIDATION_ERROR", ex.Code);
    }

    [Fact]
    public async Task Account_is_created_on_first_earn()
    {
        using var t = new TestDb();
        await new LoyaltyService(t.Db).EarnPointsAsync(t.UserId, 400, NewOrder(t), default);
        await t.Db.SaveChangesAsync();

        var account = await t.Db.LoyaltyAccounts.SingleAsync(a => a.UserId == t.UserId);
        Assert.Equal(20, account.Points);
    }

    [Theory]
    [InlineData(0, LoyaltyTier.Bronze)]
    [InlineData(300, LoyaltyTier.Silver)]
    [InlineData(1000, LoyaltyTier.Gold)]
    public void Tier_thresholds(int totalEarned, LoyaltyTier expected) =>
        Assert.Equal(expected, LoyaltyService.RecalcTier(totalEarned));
}

public class DomainRulesTests
{
    [Theory]
    [InlineData("09:00–12:00", "09:00–12:00")]
    [InlineData("09:00-12:00", "09:00–12:00")]
    [InlineData(" 18:00 - 21:00 ", "18:00–21:00")]
    [InlineData("07:00-08:00", null)]
    [InlineData("", null)]
    public void Time_slots_accept_hyphen_and_en_dash(string input, string? expected) =>
        Assert.Equal(expected, OrderConstants.NormalizeTimeSlot(input));

    [Theory]
    [InlineData(OrderStatus.PhotoReady, "photo_ready")]
    [InlineData(OrderStatus.CourierEnRoute, "courier_en_route")]
    [InlineData(OrderStatus.Accepted, "accepted")]
    [InlineData(LoyaltyTransactionType.AdminAdjust, "admin_adjust")]
    public void Enums_are_serialized_in_snake_case(object value, string expected) =>
        Assert.Equal(expected, value switch
        {
            OrderStatus s => s.ToApiString(),
            LoyaltyTransactionType l => l.ToApiString(),
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        });

    private static CreateOrderRequest ValidOrder() => new(
        Items: [new CreateOrderItem(Guid.NewGuid(), Guid.NewGuid())],
        Addons: null, PostcardId: null, CardText: null,
        Address: "ул. Рудаки 1", DeliveryZoneId: null,
        DeliveryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)).ToString("yyyy-MM-dd"),
        DeliveryTime: "12:00–15:00", PaymentMethod: "cash",
        CustomerName: "Абдулло", CustomerPhone: "+992900000001");

    [Fact]
    public void Valid_order_passes_validation() =>
        Assert.True(new CreateOrderValidator().Validate(ValidOrder()).IsValid);

    [Fact]
    public void Gift_requires_recipient() =>
        Assert.False(new CreateOrderValidator().Validate(ValidOrder() with { IsGift = true }).IsValid);

    [Fact]
    public void Past_delivery_date_is_rejected() =>
        Assert.False(new CreateOrderValidator().Validate(ValidOrder() with { DeliveryDate = "2020-01-01" }).IsValid);

    [Fact]
    public void Negative_quantity_is_rejected() =>
        Assert.False(new CreateOrderValidator().Validate(ValidOrder() with
        {
            Items = [new CreateOrderItem(Guid.NewGuid(), Guid.NewGuid(), -1)]
        }).IsValid);
}
