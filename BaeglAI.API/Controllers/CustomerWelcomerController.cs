using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CustomerWelcomerController : ControllerBase
{
    private readonly ICustomerWelcomerService _customerWelcomerService;

    public CustomerWelcomerController(ICustomerWelcomerService customerWelcomerService)
    {
        _customerWelcomerService = customerWelcomerService;
    }

    // 🔍 Çağrının structured data'larını ön izlemek için
    [HttpGet("preview-order/{callId}")]
    public async Task<IActionResult> PreviewOrder(string callId)
    {
        try
        {
            var dto = await _customerWelcomerService.BuildOrderFromVapiCall(callId);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Veri alınamadı", error = ex.Message });
        }
    }

    // 💾 Çağrıdan gelen veriyi veritabanına kaydetmek için
    [HttpPost("save-order/{callId}")]
    public async Task<IActionResult> SaveOrder(string callId)
    {
        try
        {
            var order = await _customerWelcomerService.SaveOrderFromCall(callId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Sipariş kaydedilemedi", error = ex.Message });
        }
    }


}

