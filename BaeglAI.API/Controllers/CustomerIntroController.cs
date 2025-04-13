// using System.Text.Json.Serialization;
// using Microsoft.AspNetCore.Mvc;

// [ApiController]
// [Route("api/[controller]")]
// public class CustomerIntroController : ControllerBase
// {
//     private readonly IRepository<Customer> _customerRepository;
//     private readonly IConfiguration _config;

//     public CustomerIntroController(
//         IRepository<Customer> customerRepository,
//         IConfiguration config)
//     {
//         _customerRepository = customerRepository;
//         _config = config;
//     }

//     // [HttpGet]
//     // public async Task<IActionResult> GetCustomerName([FromQuery] string customerPhone)
//     // {
//     //     if (string.IsNullOrWhiteSpace(customerPhone))
//     //         return BadRequest(new { message = "Telefon numarası gerekli." });

//     //     var normalizedPhone = NormalizePhone(customerPhone);
//     //     var storeId = _config["Vapi:DefaultStoreId"];

//     //     var customers = await _customerRepository.GetWhereAsync(storeId, x => x.PhoneNumber == normalizedPhone);
//     //     var customer = customers.FirstOrDefault();

//     //     return Ok(new
//     //     {
//     //         customerName = customer?.FirstName
//     //     });
//     // }

//     private string NormalizePhone(string phone)
//     {
//         return "+" + new string(phone.Where(char.IsDigit).ToArray());
//     }

// [HttpPost]
// [Route("api/[controller]")]
// public async Task<IActionResult> GetCustomerName([FromBody] VapiToolRequest request)
// {
//     if (string.IsNullOrWhiteSpace(request?.Arguments?.customerPhone))
//         return BadRequest(new { message = "customerPhone is required." });

//     var normalizedPhone = NormalizePhone(request.Arguments.customerPhone);
//     var storeId = _config["Vapi:DefaultStoreId"];

//     var customers = await _customerRepository.GetWhereAsync(storeId, x => x.PhoneNumber == normalizedPhone);
//     var customer = customers.FirstOrDefault();

//     return Ok(new
//     {
//         results = new[]
//         {
//             new
//             {
//                 toolCallId = request.toolCallId,
//                 result = new
//                 {
//                     customerName = customer?.FirstName ?? "Unknown"
//                 }
//             }
//         }
//     });
// }


// }
// /// <summary>
// /// DTO
// /// </summary>
// public class VapiToolRequest
// {
//     public string toolCallId { get; set; }

//     [JsonPropertyName("arguments")]
//     public VapiToolArguments Arguments { get; set; }
// }

// public class VapiToolArguments
// {
//     public string customerPhone { get; set; }
// }


using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CustomerIntroController : ControllerBase
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IConfiguration _config;

    public CustomerIntroController(
        IRepository<Customer> customerRepository,
        IConfiguration config)
    {
        _customerRepository = customerRepository;
        _config = config;
    }

    private string NormalizePhone(string phone)
    {
        return "+" + new string(phone.Where(char.IsDigit).ToArray());
    }

    [HttpPost]
    public async Task<IActionResult> GetCustomerName([FromBody] VapiToolRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Arguments?.customerPhone))
            return BadRequest(new { message = "customerPhone is required." });

        var normalizedPhone = NormalizePhone(request.Arguments.customerPhone);
        var storeId = _config["Vapi:DefaultStoreId"];

        var customers = await _customerRepository.GetWhereAsync(storeId, x => x.PhoneNumber == normalizedPhone);
        var customer = customers.FirstOrDefault();

        return Ok(new
        {
            results = new[]
            {
                new
                {
                    toolCallId = request.toolCallId,
                    result = new
                    {
                        customerName = customer?.FirstName ?? "Unknown"
                    }
                }
            }
        });
    }
}

/// <summary>
/// DTO
/// </summary>
public class VapiToolRequest
{
    public string toolCallId { get; set; }

    [JsonPropertyName("arguments")]
    public VapiToolArguments Arguments { get; set; }
}

public class VapiToolArguments
{
    public string customerPhone { get; set; }
}
