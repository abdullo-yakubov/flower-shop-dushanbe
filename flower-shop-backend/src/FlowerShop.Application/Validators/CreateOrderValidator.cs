using FluentValidation;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Domain.Constants;

namespace FlowerShop.Application.Validators;

internal static class ValidationRules
{
    public const string PhonePattern = @"^\+992\d{9}$";
    public static readonly string[] PaymentMethods = ["dc", "alif", "cash"];

    public static bool IsValidDeliveryDate(string? value) =>
        DateOnly.TryParse(value, out var date) &&
        date >= DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5)) && // Asia/Dushanbe is UTC+5, no DST
        date <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60));
}

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItem>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.SizeId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, OrderConstants.MaxLineQuantity);
        RuleFor(x => x.Packaging).Must(p => p is "basket" or "no_basket").WithMessage("Недопустимый тип упаковки");
        RuleFor(x => x.StemCount).GreaterThan(0).When(x => x.StemCount.HasValue);
    }
}

public class CreateOrderAddonValidator : AbstractValidator<CreateOrderAddon>
{
    public CreateOrderAddonValidator()
    {
        RuleFor(x => x.AddonId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, OrderConstants.MaxLineQuantity);
        RuleFor(x => x.Inscription).MaximumLength(100);
    }
}

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Заказ должен содержать минимум 1 товар");
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
        RuleForEach(x => x.Addons).SetValidator(new CreateOrderAddonValidator());
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DeliveryDate)
            .Must(ValidationRules.IsValidDeliveryDate)
            .WithMessage("Дата доставки должна быть не раньше сегодняшней и не позже чем через 60 дней");
        RuleFor(x => x.DeliveryTime)
            .Must(t => OrderConstants.NormalizeTimeSlot(t) is not null)
            .WithMessage("Недопустимый временной слот");
        RuleFor(x => x.PaymentMethod)
            .Must(p => ValidationRules.PaymentMethods.Contains(p))
            .WithMessage("Недопустимый способ оплаты");
        RuleFor(x => x.CustomerName).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.CustomerPhone).Matches(ValidationRules.PhonePattern).WithMessage("Неверный формат телефона");
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(100).When(x => x.IsGift)
            .WithMessage("Укажите имя получателя");
        RuleFor(x => x.RecipientPhone).Matches(ValidationRules.PhonePattern).When(x => x.IsGift)
            .WithMessage("Неверный формат телефона получателя");
        RuleFor(x => x.CardText).MaximumLength(200);
        RuleFor(x => x.PromoCode).MaximumLength(30);
        RuleFor(x => x.RedeemPoints).GreaterThanOrEqualTo(0);
    }
}

public class QuoteOrderValidator : AbstractValidator<QuoteOrderRequest>
{
    public QuoteOrderValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Заказ должен содержать минимум 1 товар");
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
        RuleForEach(x => x.Addons).SetValidator(new CreateOrderAddonValidator());
        RuleFor(x => x.DeliveryDate).Must(ValidationRules.IsValidDeliveryDate).WithMessage("Недопустимая дата доставки");
        RuleFor(x => x.PromoCode).MaximumLength(30);
        RuleFor(x => x.RedeemPoints).GreaterThanOrEqualTo(0);
    }
}

public class SendOtpValidator : AbstractValidator<DTOs.Auth.SendOtpRequest>
{
    public SendOtpValidator()
    {
        RuleFor(x => x.Phone).NotEmpty().Matches(ValidationRules.PhonePattern).WithMessage("Неверный формат телефона");
    }
}
