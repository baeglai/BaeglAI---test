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

    // [HttpPost]
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

[HttpPost]
public async Task<IActionResult> GetCustomerName([FromBody] VapiMessageWrapper request)
{
    var toolCall = request.message?.toolCallList?.FirstOrDefault();

    if (toolCall == null)
        return BadRequest(new { message = "Tool call missing." });

    var arguments = toolCall.arguments;

    // VAPI bazen "arguments.customerPhone", bazen "arguments_customerPhone" şeklinde yollar
    arguments.TryGetValue("customerPhone", out var phoneFromDotNotation);
    arguments.TryGetValue("arguments_customerPhone", out var phoneFromFlatKey);

    var customerPhone = phoneFromDotNotation ?? phoneFromFlatKey;

    if (string.IsNullOrWhiteSpace(customerPhone))
        return BadRequest(new { message = "customerPhone is required." });

    var normalizedPhone = NormalizePhone(customerPhone);
    var storeId = _config["Vapi:DefaultStoreId"];

    var customers = await _customerRepository.GetWhereAsync(storeId, x => x.PhoneNumber == normalizedPhone);
    var customer = customers.FirstOrDefault();

    var name = customer?.FirstName ?? "Unknown";

    return Ok(new
    {
        results = new[]
        {
            new
            {
                toolCallId = toolCall.id,
                result = new { customerName = name }
            }
        }
    });
}



}


public class VapiMessageWrapper
{
    public VapiMessage message { get; set; }
}

public class VapiMessage
{
    public List<VapiToolCall> toolCallList { get; set; }
    public string type { get; set; }
    public long timestamp { get; set; }
}

public class VapiToolCall
{
    public string id { get; set; }
    public string name { get; set; }
    public Dictionary<string, string> arguments { get; set; }
}

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
