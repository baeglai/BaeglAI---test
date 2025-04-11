// using Microsoft.AspNetCore.Mvc;

// [ApiController]
// [Route("api/[controller]")]
// public class CustomerIntroController : ControllerBase
// {
//     private readonly IRepository<Customer> _customerRepository;

//     public CustomerIntroController(IRepository<Customer> customerRepository)
//     {
//         _customerRepository = customerRepository;
//     }

//     [HttpGet]
//     public async Task<IActionResult> GetCustomerName([FromQuery] string storeId, [FromQuery] string phoneNumber)
//     {
//         var customers = await _customerRepository.GetWhereAsync(storeId, c => c.PhoneNumber == phoneNumber);

//         var customer = customers.FirstOrDefault();

//         return Ok(new
//         {
//             customerName = customer?.FirstName
//         });
//     }
// }


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

    [HttpGet]
    public async Task<IActionResult> GetCustomerName([FromQuery] string customerPhone)
    {
        if (string.IsNullOrWhiteSpace(customerPhone))
            return BadRequest(new { message = "Telefon numarası gerekli." });

        var normalizedPhone = NormalizePhone(customerPhone);
        var storeId = _config["Vapi:DefaultStoreId"];

        var customers = await _customerRepository.GetWhereAsync(storeId, x => x.PhoneNumber == normalizedPhone);
        var customer = customers.FirstOrDefault();

        return Ok(new
        {
            customerName = customer?.FirstName
        });
    }

    private string NormalizePhone(string phone)
    {
        return "+" + new string(phone.Where(char.IsDigit).ToArray());
    }
}
