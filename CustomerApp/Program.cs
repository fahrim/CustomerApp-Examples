using CustomerApp;
using CustomerApp.Validation; // <-- Ekledik
using FluentValidation; // <-- Ekledik


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ESKÝ KODLARI SÝL:
// builder.Services.AddControllers().AddFluentValidation(...);
// builder.Services.AddScoped<IValidator<Customer>, CustomerValidator>();

// YENÝ KOD (Otomatik Tarama):
// FluentValidation'a validator'larýn nerede olduðunu söylüyoruz.
// Bu metot, CustomerValidator ile ayný assembly (proje) içindeki
// tüm validator sýnýflarýný bulur ve DI için otomatik olarak kaydeder.
builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

// CustomerProvider'ý projenin her yerinde kullanýlabilir hale getiriyoruz.
// AddScoped: Her bir web isteði için yeni bir CustomerProvider nesnesi oluþturulur.

// ESKÝ HALÝ:
 builder.Services.AddScoped<CustomerProvider>();

// YENÝ HALÝ:
// "Birisi ICustomerProvider isterse, ona CustomerProvider sýnýfýnýn bir örneðini ver"
builder.Services.AddScoped<ICustomerProvider, CustomerProvider>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
