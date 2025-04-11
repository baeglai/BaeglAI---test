// using Microsoft.AspNetCore.Mvc;
// using BaeglAI.Business.Services;

// namespace BaeglAI.API.Controllers;

// [ApiController]
// [Route("api/stores/{storeId}/orders")]
// public class OrderController : ControllerBase
// {
//     private readonly OrderService _orderService;

//     public OrderController(OrderService orderService)
//     {
//         _orderService = orderService;
//     }

//     [HttpGet]
//     public async Task<IActionResult> GetAll(string storeId)
//     {
//         var orders = await _orderService.GetAllAsync(storeId);
//         return Ok(orders);
//     }

//     [HttpGet("{orderId}")]
//     public async Task<IActionResult> GetById(string storeId, string orderId)
//     {
//         var order = await _orderService.GetByIdAsync(storeId, orderId);
//         return order != null ? Ok(order) : NotFound();
//     }

//     [HttpPost]
//     public async Task<IActionResult> Create(string storeId, [FromBody] CreateOrderDto dto)
//     {
//         var success = await _orderService.CreateAsync(storeId, dto);
//         return success ? Ok(new { message = "Order created." }) : BadRequest();
//     }

//     [HttpPut("{orderId}")]
//     public async Task<IActionResult> Update(string storeId, string orderId, [FromBody] Order order)
//     {
//         var success = await _orderService.UpdateAsync(storeId, orderId, order);
//         return success ? Ok(new { message = "Order updated." }) : BadRequest();
//     }

//     [HttpDelete("{orderId}")]
//     public async Task<IActionResult> Delete(string storeId, string orderId)
//     {
//         var success = await _orderService.DeleteAsync(storeId, orderId);
//         return success ? Ok(new { message = "Order deleted." }) : NotFound();
//     }

//     [HttpGet("by-customer/{customerId}")]
//     public async Task<IActionResult> GetByCustomerId(string storeId, string customerId)
//     {
//         var orders = await _orderService.GetByCustomerIdAsync(storeId, customerId);
//         return Ok(orders);
//     }
// }
