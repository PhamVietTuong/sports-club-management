using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = UserRole.Admin)]
public class AdminPaymentsController : ControllerBase
{
    private readonly PaymentRepository _payments;

    public AdminPaymentsController(PaymentRepository payments) => _payments = payments;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> List() =>
        Ok((await _payments.FindAllAsync()).Select(PaymentDto.From));

    /// <summary>Revenue overview — all-time total plus a per-month breakdown.</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueDto>> Revenue()
    {
        var monthly = await _payments.MonthlyRevenueAsync();
        var total = await _payments.TotalRevenueAsync();
        return new RevenueDto(total, monthly.Sum(m => m.Count), monthly.Select(MonthlyRevenueDto.From));
    }
}
