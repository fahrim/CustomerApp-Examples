using CustomerApp;
using CustomerApp.Validation; // <-- Ekledik
using FluentValidation; // <-- Ekledik


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ESK� KODLARI S�L:
// builder.Services.AddControllers().AddFluentValidation(...);
// builder.Services.AddScoped<IValidator<Customer>, CustomerValidator>();

// YEN� KOD (Otomatik Tarama):
// FluentValidation'a validator'lar�n nerede oldu�unu s�yl�yoruz.
// Bu metot, CustomerValidator ile ayn� assembly (proje) i�indeki
// t�m validator s�n�flar�n� bulur ve DI i�in otomatik olarak kaydeder.
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCustomerCommandValidator>();

// CustomerProvider'� projenin her yerinde kullan�labilir hale getiriyoruz.
// AddScoped: Her bir web iste�i i�in yeni bir CustomerProvider nesnesi olu�turulur.

// EN ESK� HAL� (no interface):
//builder.Services.AddScoped<CustomerProvider>();

// ESK� HAL�: (no Interface Segregation Principle)
//builder.Services.AddScoped<ICustomerProvider, CustomerProvider>();

// YEN� HAL�:
// "Birisi ICustomerProvider(Query/Command)[CQRS] isterse, ona CustomerProvider s�n�f�n�n bir �rne�ini ver"
// Art�k aray�zler ayr�ld� ve aray�zlerin tek bir sorumlulu�u var
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
