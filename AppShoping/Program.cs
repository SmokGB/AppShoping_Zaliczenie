using AppShoping.ApplicationServices.Components.csvImporter;
using AppShoping.ApplicationServices.Components.csvReader;
using AppShoping.DataAccess.Data;
using AppShoping.DataAccess.Data.Entities;
using AppShoping.DataAccess.Data.Repositories;
using AppShoping.UI.App;
using AppShoping.UI.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IApp, App>();
services.AddSingleton<IUserCommunication, UserCommunication>();
services.AddSingleton<IRepository<Food>, SqlRepository<Food>>();
services.AddSingleton<IRepository<PurchaseStatistics>,SqlRepository<PurchaseStatistics>>();
services.AddSingleton<ICsvReader, CsvReader>();
services.AddSingleton<ICsvImporter, CsvImporter>();

services.AddDbContext<ShopAppDbContext>(options =>options
.UseSqlServer("Data Source=AgaiGrzes\\SQLEXPRESS;Initial Catalog=ShopAppStorage;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"));

var serviceProvider = services.BuildServiceProvider();
var app = serviceProvider.GetService<IApp>();


if (app == null)
{
     throw new InvalidOperationException("Nie udało się uzyskać instancji IApp.");
}
app.Run();