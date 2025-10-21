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
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCustomerCommandValidator>();

// CustomerProvider'ý projenin her yerinde kullanýlabilir hale getiriyoruz.
// AddScoped: Her bir web isteði için yeni bir CustomerProvider nesnesi oluþturulur.

// EN ESKÝ HALÝ (no interface):
//builder.Services.AddScoped<CustomerProvider>();

// ESKÝ HALÝ: (no Interface Segregation Principle)
//builder.Services.AddScoped<ICustomerProvider, CustomerProvider>();

// YENÝ HALÝ:
// "Birisi ICustomerProvider(Query/Command)[CQRS] isterse, ona CustomerProvider sýnýfýnýn bir örneðini ver"
// Artýk arayüzler ayrýldý ve arayüzlerin tek bir sorumluluðu var
builder.Services.AddScoped<ICustomerQueryProvider, CustomerProvider>();
builder.Services.AddScoped<ICustomerCommandProvider, CustomerProvider>();


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
