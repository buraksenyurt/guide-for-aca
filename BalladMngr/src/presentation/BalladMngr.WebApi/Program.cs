using BalladMngr.Application;
using BalladMngr.Data;
using BalladMngr.Data.Contexts;
using BalladMngr.Shared;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*
* Web API'nin çalışma zamanının ihtiyaç duyacağı Application,Data(Entity Framework context'ini alacak) ve Shared servislerini 
* aşağıdaki metotlar yardımıyla ekliyoruz.
* 
* İlgili servisleri burada da açık bir şekilde ekleyebilirdik ancak yapmadık. 
* Bu sayede o kütüphanelerin servislerinin DI koleksiyonuna eklenme işini buradan soyutlamış olduk.
* Orada servislerde bir değişiklik olursa buraya gelip bir şeyler yapmamıza gerek kalmayacak.
* 
*/
builder.Services.AddApplication(configuration);
builder.Services.AddData(configuration);
builder.Services.AddShared(configuration);

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BalladMngrDbContext>();
    await BalladMngrDbContextSeed.SeedDataAsync(context);
}

app.Run();
