using CustomerApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// CustomerProvider'� projenin her yerinde kullan�labilir hale getiriyoruz.
// AddScoped: Her bir web iste�i i�in yeni bir CustomerProvider nesnesi olu�turulur.

    // ESK� HAL�:
// builder.Services.AddScoped<CustomerProvider>();

    // YEN� HAL�:
// "Birisi ICustomerProvider isterse, ona CustomerProvider s�n�f�n�n bir �rne�ini ver"
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
