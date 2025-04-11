using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CustomerOrderCollectorController : ControllerBase
{
    private readonly ICustomerOrderCollectorService _collectorService;

    public CustomerOrderCollectorController(ICustomerOrderCollectorService collectorService)
    {
        _collectorService = collectorService;
    }

    [HttpPost("save-order-and-customer/{callId}")]
    public async Task<IActionResult> SaveOrderAndCustomer(string callId)
    {
        try
        {
            var (order, customer) = await _collectorService.SaveOrderAndCustomerFromCall(callId);
            return Ok(new { order, customer });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Sipariş ve müşteri kaydedilemedi", error = ex.Message });
        }
    }
}
