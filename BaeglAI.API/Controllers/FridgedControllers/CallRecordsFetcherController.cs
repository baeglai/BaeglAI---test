// using System.Text.Json;
// using BaeglAI.Business.Services;
// using Microsoft.AspNetCore.Mvc;

// public class CallRecordsFetcherController : ControllerBase
// {
//     private readonly ICallRecordsFetcher _callRecordsFetcher;
//     private readonly OrderService _orderService;

//     public CallRecordsFetcherController(ICallRecordsFetcher callRecordsFetcher, OrderService orderService)
//     {
//         _callRecordsFetcher = callRecordsFetcher;
//         _orderService = orderService;
//     }

//     [HttpGet("get-structured-data/{callId}")]
//     public async Task<IActionResult> GetStructuredData(string callId)
//     {
//         var data = await _callRecordsFetcher.StructuredDataFetch(callId);
//         return Ok(data);
//     }

//     [HttpPost("save-structured-data/{storeId}/{callId}")]
//     public async Task<IActionResult> SaveStructuredDataToFirestore(string storeId, string callId)
//     {
//         var jsonElement = await _callRecordsFetcher.StructuredDataFetch(callId);

//         // Deserialization için ayarlar
//         var options = new JsonSerializerOptions
//         {
//             PropertyNameCaseInsensitive = true
//         };

//         VapiStructuredOrderDto vapiDto;

//         try
//         {
//             vapiDto = JsonSerializer.Deserialize<VapiStructuredOrderDto>(jsonElement.GetRawText(), options);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine("❌ JSON deserialize hatası: " + ex.Message);
//             Console.WriteLine("📦 Gelen veri: " + jsonElement.GetRawText());
//             return BadRequest("JSON parse edilemedi.");
//         }

//         if (vapiDto == null || vapiDto.Items == null || !vapiDto.Items.Any())
//         {
//             Console.WriteLine("⚠️ Yapı geçersiz: boş items listesi");
//             return BadRequest("Sipariş verisi geçersiz.");
//         }

//         var createOrderDto = new CreateOrderDto
//         {
//             CustomerId = vapiDto.CustomerName ?? "unknown",
//             Items = vapiDto.Items.Select(i => new OrderItemDto
//             {
//                 Name = i.Name,
//                 Quantity = i.Quantity,
//                 UnitPrice = 0 // henüz bilinmiyor
//             }).ToList(),
//             Note = vapiDto.Transcript
//         };

//         var success = await _orderService.CreateAsync(storeId, createOrderDto);
//         return success
//             ? Ok(new { message = "Sipariş başarıyla Firestore'a kaydedildi." })
//             : BadRequest("Sipariş kaydedilemedi.");
//     }

    
// }