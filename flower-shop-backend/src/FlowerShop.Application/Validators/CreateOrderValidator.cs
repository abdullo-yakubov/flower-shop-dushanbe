using FluentValidation;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Domain.Constants;

namespace FlowerShop.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Заказ должен содержать минимум 1 товар");
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DeliveryDate)
            .NotEmpty()
            .Must(d => DateOnly.TryParse(d, out var date) && date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Дата доставки не может быть в прошлом");
        RuleFor(x => x.DeliveryTime)
            .Must(t => OrderConstants.TimeSlots.Contains(t))
            .WithMessage("Недопустимый временной слот");
        RuleFor(x => x.PaymentMethod).Must(p => new[] { "dc", "alif", "cash" }.Contains(p))
            .WithMessage("Недопустимый способ оплаты");
        RuleFor(x => x.CustomerName).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.CustomerPhone).Matches(@"^\+992\d{9}$").WithMessage("Неверный формат телефона");
        RuleFor(x => x.CardText).MaximumLength(200);
    }
}

public class SendOtpValidator : AbstractValidator<DTOs.Auth.SendOtpRequest>
{
    public SendOtpValidator()
    {
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^\+992\d{9}$").WithMessage("Неверный формат телефона");
    }
}
