using CustomerApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// CustomerProvider'ý projenin her yerinde kullanýlabilir hale getiriyoruz.
// AddScoped: Her bir web isteði için yeni bir CustomerProvider nesnesi oluþturulur.

    // ESKÝ HALÝ:
// builder.Services.AddScoped<CustomerProvider>();

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
