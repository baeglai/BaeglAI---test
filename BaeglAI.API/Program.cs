using BaeglAI.Business.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔌 HttpClient kullanımı için genel kayıt
builder.Services.AddHttpClient();

// 🔥 FirebaseContext kurulumu (aynı HttpClient kullanımı)
builder.Services.AddSingleton<FirebaseContext>(sp =>
    new FirebaseContext(
        sp.GetRequiredService<HttpClient>(),
        builder.Configuration["Firebase:ApiKey"],
        builder.Configuration["Firebase:ProjectId"]
    ));

// 🔧 Firestore bağlantıları
builder.Services.AddHttpClient<ICustomerOrderCollectorService, CustomerOrderCollectorService>();
builder.Services.AddHttpClient<ICustomerWelcomerService, CustomerWelcomerService>();
builder.Services.AddHttpClient<ICallRecordsFetcher, CallRecordsFetcher>();

builder.Services.AddScoped<IRepository<Order>>(sp =>
    new GenericRepository<Order>(
        sp.GetRequiredService<FirebaseContext>(),
        "orders"
    ));

builder.Services.AddScoped<IRepository<Customer>>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var baseUrl = builder.Configuration["Firestore:BaseUrl"];
    var collectionPath = "customers";
    return new RestFirestoreRepository<Customer>(httpClient, baseUrl, collectionPath);
});

builder.Services.AddScoped<IStoreLookupService, StoreLookupService>();

builder.Services.AddScoped<IRepository<Store>>(sp =>
    new RestFirestoreRepository<Store>(
        sp.GetRequiredService<HttpClient>(),
        builder.Configuration["Firestore:BaseUrl"],
        "stores"
    ));

// 📦 Servislerin birebir kaydı
builder.Services.AddScoped<OrderService>();

// 🌐 CORS ayarları
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// 📡 Controller & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BaeglAI API", Version = "v1" });
});

// Uygulama başlangıç
var app = builder.Build();

// Swagger için geliştirme ortamında arayüz ekleyelim
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🌍 Middleware pipeline
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Endpoints ile controllerları belirleyelim
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Uygulamayı çalıştır
app.Run("http://0.0.0.0:8080");
