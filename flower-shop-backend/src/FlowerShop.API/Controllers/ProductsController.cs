using FlowerShop.Application.DTOs.Common;
using FlowerShop.Application.DTOs.Products;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using FlowerShop.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController(AppDbContext db, ProductRepository products) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductsQuery q, CancellationToken ct)
    {
        var flowerTypesList = q.FlowerTypes?.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var colorsList = q.Colors?.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var (items, total) = await products.GetPagedAsync(
            q.Q, q.Occasion, flowerTypesList, colorsList,
            q.PriceMin, q.PriceMax, q.DeliverToday, q.Freshness, q.Kind, q.Packaging,
            q.Sort, q.Page, Math.Min(q.PageSize, 100), ct);

        var dtos = items.Select(p => MapCard(p, q.Lang));
        return Ok(PagedResponse<ProductCardDto>.Ok(dtos, q.Page, q.PageSize, total));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetProduct(string slug, [FromQuery] string lang = "ru", CancellationToken ct = default)
    {
        var product = await products.GetBySlugAsync(slug, ct);
        if (product is null) return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Товар не найден"));
        return Ok(ApiResponse<ProductCardDto>.Ok(MapCard(product, lang)));
    }

    [HttpGet("{slug}/reviews")]
    public async Task<IActionResult> GetReviews(string slug, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Slug == slug, ct);
        if (product is null) return NotFound();

        var query = db.Reviews.Include(r => r.Photos).Include(r => r.User)
            .Where(r => r.ProductId == product.Id && r.IsApproved && !r.IsHidden);
        var total = await query.CountAsync(ct);
        var reviews = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var dtos = reviews.Select(r => new ReviewDto(
            r.Id, r.AuthorName, r.User?.AvatarUrl, r.Rating, r.Text,
            r.Photos.Select(p => p.Url), r.HelpfulCount, r.AdminReply,
            r.CreatedAt.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"))
        ));

        return Ok(PagedResponse<ReviewDto>.Ok(dtos, page, pageSize, total));
    }

    [Authorize]
    [HttpPost("{slug}/reviews")]
    public async Task<IActionResult> CreateReview(string slug, [FromBody] CreateReviewRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Slug == slug, ct);
        if (product is null) return NotFound();
        var user = await db.Users.FindAsync([userId], ct);

        var review = new Domain.Entities.Review
        {
            ProductId = product.Id,
            UserId = userId,
            AuthorName = user?.Name ?? "Аноним",
            Rating = req.Rating,
            Text = req.Text,
            IsApproved = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);

        return Created(string.Empty, new { success = true, data = new { id = review.Id } });
    }

    private static ProductCardDto MapCard(Domain.Entities.Product p, string lang) => new(
        p.Id, p.Slug,
        lang == "en" ? p.NameEn : lang == "tg" ? p.NameTg : p.NameRu,
        lang == "en" ? p.CompositionEn : lang == "tg" ? p.CompositionTg : p.CompositionRu,
        p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url),
        p.Price, p.OldPrice, p.Discount, p.Rating, p.ReviewCount,
        p.Occasions.Select(o => o.Occasion.ToString().ToLower()),
        p.FlowerTypes.Select(f => f.FlowerType.ToString().ToLower()),
        p.Colors.Select(c => c.Color),
        p.IsNew, p.IsHit, p.InStock, p.DeliverToday,
        p.Kind.ToString().ToLower(),
        p.Freshness.ToString().ToLower(),
        p.PackagingOptions.Select(pk => pk.Packaging == Packaging.Basket ? "basket" : "no_basket"),
        p.Sizes.OrderBy(s => s.SortOrder).Select(s => new ProductSizeDto(s.Id, s.Label.ToString(), s.Price, s.OldPrice)),
        p.StemPrice, p.StemOptions
    );
}
