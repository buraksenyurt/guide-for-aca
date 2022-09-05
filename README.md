# Uygulamalı Clean Architecture - 101

Applied Clean Architecture. Uygulamalı clean architecture eğitimlerinde kullanılmak üzere oluşturduğum repo. Basit ve hafifsiklet bir projenin temel enstrümanları ile uçtan uca hazırlanmasına yardımcı olan anlatımı içermektedir. Materyal Ubuntu 20.04 sistemini baz almaktadır. Örnek senaryo bir şarkı sözü yazarının bestelerini yönetmek üzerine kurgulanmıştır.

## Kontrol

İlk etapta sistem dotnet yüklü mü değil mi kontrol etmek lazım. Yoksa [şu adrese gidip sistemimize kurabiliriz](https://dotnet.microsoft.com/en-us/download) Ardından aşağıdaki terminal komutu ile dotnet sdk'in yüklü olup olmadığına bakabiliriz.

```shell
dotnet --version
```

## 00 - Çözüm İskeletinin Oluşturulması

Aşağıdaki komutlar ile işe başlanır.

```shell
## Backend tarafının inşası

mkdir BalladMngr
cd BalladMngr
dotnet new sln

mkdir src
cd src

mkdir core infrastructure presentation

cd core
dotnet new classlib -f net6.0 --name BalladMngr.Domain
dotnet new classlib -f net6.0 --name BalladMngr.Application

cd BalladMngr.Application
dotnet add reference ../BalladMngr.Domain/BalladMngr.Domain.csproj

cd ..
cd ..
cd infrastructure

dotnet new classlib -f net6.0 --name BalladMngr.Data
dotnet new classlib -f net6.0 --name BalladMngr.Shared

cd BalladMngr.Data
dotnet add reference ../../core/BalladMngr.Domain/BalladMngr.Domain.csproj
dotnet add reference ../../core/BalladMngr.Application/BalladMngr.Application.csproj

cd ..
cd BalladMngr.Shared
dotnet add reference ../../core/BalladMngr.Application/BalladMngr.Application.csproj

cd ..
cd ..
cd presentation

dotnet new webapi --name BalladMngr.WebApi

cd BalladMngr.WebApi

dotnet add reference ../../core/BalladMngr.Application/BalladMngr.Application.csproj
dotnet add reference ../../infrastructure/BalladMngr.Data/BalladMngr.Data.csproj
dotnet add reference ../../infrastructure/BalladMngr.Shared/BalladMngr.Shared.csproj

cd ..
cd ..
cd ..

dotnet sln add src/core/BalladMngr.Domain/BalladMngr.Domain.csproj
dotnet sln add src/core/BalladMngr.Application/BalladMngr.Application.csproj
dotnet sln add src/infrastructure/BalladMngr.Data/BalladMngr.Data.csproj
dotnet sln add src/infrastructure/BalladMngr.Shared/BalladMngr.Shared.csproj
dotnet sln add src/presentation/BalladMngr.WebApi/BalladMngr.WebApi.csproj

cd src
cd core
cd BalladMngr.Domain
mkdir Entities Enums Settings
cd ..
cd ..

# Entity Framework için gerekli hazırlıklar

dotnet tool install --global dotnet-ef

cd presentation
cd BalladMngr.WebApi

dotnet add package Microsoft.EntityFrameworkCore.Design

cd ..
cd ..
cd infrastructure
cd BalladMngr.Data
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

mkdir Contexts

# Sqlite veri tabanı için migration operasyonları (Gerektiğinde Kullanılacak)
dotnet ef migrations add InitialCreate --startup-project ../../presentation/Librarian.WebApi
dotnet ef database update --startup-project ../../presentation/Librarian.WebApi

cd ..
cd ..

# Vue Tarafının Oluşturulması
sudo npm install -g @vue/cli

cd presentation
# Presentation klasörü altındayken vue projesi oluşturulur
vue create ballad-mngr-app

cd ballad-mngr-app
# ballad-mngr-app klasöründeyken aşağıdaki komut çalıştırılır
vue add vuetify

# Aşağıdaki komut ile vue tarafının çalıştığından emin olunur
npm run serve
```

## 01 - Domain Projesine İlgili Tiplerin Eklenmesi

Enum sabitleri,

Status.cs

```csharp
namespace BalladMngr.Domain.Enums
{
    //Şarkının güncel durumunu işaret eder.
    public enum Status
    {
        Draft,
        Completed
    }
}
```

Language.cs

```csharp
namespace BalladMngr.Domain.Enums
{
    // Şarkının hangi dilde olduğunu tutar.
    // Bestecimiz birden fazla dil biliyor :P
    public enum Language
    {
        English,
        Turkish,
        Spanish
    }
}
```

ve Entity veri yapıları

Song.cs

```csharp
using BalladMngr.Domain.Enums;

namespace BalladMngr.Domain.Entities
{
     // Beste ile ilgili temel bilgileri içerir.
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Lyrics { get; set; }
        public Status Status { get; set; }
        public Language Language { get; set; }
    }
}
```

User.cs

```csharp
namespace BalladMngr.Domain.Entities
{
    // İleride authentication gibi noktalarda kullanacağımız kullanıcı içindir.
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
```

Mail gönderim servisi içinde MailSettings.cs

```csharp
namespace BalladMngr.Domain.Settings
{
    /*
     * Shared projesine eklenecek EmailService, mail gönderme işini üstlenen bir servis.
     * Mail gönderimi sırasında kimden gittiği, Smtp ayarları gibi bilgileri appSettings.json içeriğinden alsak hiç fena olmaz.
     * Bu nedenle Domain altına appSettings altındaki MailSettings sekmesini işaret edecek bu sınıf eklendi.
     */
    public class MailSettings
    {
        public string From { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        public string DisplayName { get; set; }
    }
}
```

## 02 - MediatoR Paketinin Eklenmesi ve CQRS Hazırlıkları

```shell
cd core
cd BalladMngr.Application
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package Microsoft.EntityFrameworkCore

mkdir Common
cd Common
mkdir Behaviors Exceptions Interfaces Mappings

cd ..
mkdir Dtos
cd Dtos
mkdir Songs Email User
cd ..

mkdir Songs
cd Songs
mkdir Commands
cd Commands
mkdir CreateSong UpdateSong DeleteSong
cd ..

mkdir Queries
cd Queries
mkdir ExportSongs GetSongs
```

İlk olarak Mappings klasörüne IMapFrom.cs arayüzü eklenir.

```csharp
using AutoMapper;

namespace BalladMngr.Application.Common.Mappings
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
    }
}
```

Aynı klasöre MappingProfile.cs eklenir.

```csharp
using AutoMapper;
using System;
using System.Linq;
using System.Reflection;

namespace BalladMngr.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var types = assembly.GetExportedTypes()
              .Where(
                        t => t.GetInterfaces()
                        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>))
                    )
              .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var methodInfo = type.GetMethod("Mapping") ?? type.GetInterface("IMapFrom`1").GetMethod("Mapping");
                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}
```

Şimdi CQRS tarafını tamamlayalım.

ExportSongs klasörüne SongRecord.cs eklenir.

```csharp
using BalladMngr.Application.Common.Mappings;
using BalladMngr.Domain.Entities;
using BalladMngr.Domain.Enums;

namespace BalladMngr.Application.Songs.Queries.ExportSongs
{
    /*
     * CSV içerisine hangi şarkı bilgilerini tutacağımızı belirten veri yapısı.
     * 
     * Bir nesne dönüşümü söz konusu olduğundan IMapFrom<T> uyarlaması var.
     * 
     */
    public class SongRecord
        :IMapFrom<Song>
    {
        public string Title { get; set; }
        public Status Status { get; set; }
    }
}
```

Ardından Interfaces klasörüne IApplicationDbContext vs ICsvBuilder.cs sınıfları eklenir.

IApplicationDbContext.cs

```csharp
using BalladMngr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Librarian.Application.Common.Interfaces
{
    /*
     * Entity Framework Core context nesnesine ait servis sözleşmemiz.
     * Onu da DI mekanizmasına kolayca dahil edip bağımlılığı çözümletebiliriz.
     * Yani infrastructe katmanındaki Data kütüphanesindeki BalladMngrDbContext'i bu arayüz üstünden sisteme entegre edebileceğiz.
     */
    public interface IApplicationDbContext
    {
        public DbSet<Song> Songs { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
```

ICsvBuilder.cs

```csharp
using BalladMngr.Application.Songs.Queries.ExportSongs;
using System.Collections.Generic;

namespace BalladMngr.Application.Common.Interfaces
{
    /*
     * CSV dosya çıktısı üreten servisin sözleşmesi
     * IEmailService tarafında olduğu gibi bu hizmeti de DI mekanizmasına kolayca dahil edebileceğiz.
     */
    public interface ICsvBuilder
    {
        byte[] BuildFile(IEnumerable<SongRecord> songRecords);
    }
}
```

Aynı yere ExportSongsQuery.cs ve ExportSongsViewModel.cs dosyaları eklenir.

ExportSongsViewModel.cs

```csharp
namespace BalladMngr.Application.Songs.Queries.ExportSongs
{
    /*
     * Şarkıların bir çıktısını CSV olarak verdiğimizde kullanılan ViewModel nesnemiz.
     * Bunu ExportSongsQuery ve Handler tipi kullanmakta.
     * Dosyanın adını, içeriğin tipini ve byte[] cinsinden içeriği tutuyor.
     * Belki byte[] yerine 64 bit encode edilmiş string içerik de verebiliriz. 
     */
    public class ExportSongsViewModel
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
```

ExportSongsQuery.cs

```csharp
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Librarian.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using BalladMngr.Application.Common.Interfaces;

namespace BalladMngr.Application.Songs.Queries.ExportSongs
{
    /*
     * 
     * Query nesnemiz.
     * Talebe karşılık bir ViewModel döndürüleceğini tanımlıyor.
     * 
     */
    public class ExportSongsQuery
        : IRequest<ExportSongsViewModel>
    {
    }

    /*
    * Handler tipimiz.
    * Gelen sorguya ele alıp uygun ViewModel nesnesinin döndürülmesi sağlanıyor.
    * 
    * Kullanması için gereken bağımlılıkları Constructor Injection ile içeriye alıyoruz.
    * Buna göre CSV üretici, AutoMapper nesne dönüştürücü ve EF Core DbContext servislerini içeriye alıyoruz.
    * 
    * Şarkı listesini çektiğimiz LINQ sorgusunda ProjectTo metodunu nasıl kullandığımız dikkat edelim.
    * Listenin SongRecord tipinden nesnelere dönüştürülmesi noktasında AutoMapper'ın çalışma zamanı sorumlusu kimse o kullanılıyor olacak.
    * Nitekim SongRecord tipinin IMapFrom<Song> tipini uyguladığını düşünecek olursak çalışma zamanı Song üstünden gelen özelliklerden eş düşenleri, SongRecord karşılıklarına alacak.
    */
    public class ExportSongsQueryHandler : IRequestHandler<ExportSongsQuery, ExportSongsViewModel>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICsvBuilder _csvBuilder;
        public ExportSongsQueryHandler(IApplicationDbContext context, IMapper mapper, ICsvBuilder csvBuilder)
        {
            _context = context;
            _mapper = mapper;
            _csvBuilder = csvBuilder;
        }
        public async Task<ExportSongsViewModel> Handle(ExportSongsQuery request, CancellationToken cancellationToken)
        {
            var viewModel = new ExportSongsViewModel();

            var list = await _context.Songs
                .ProjectTo<SongRecord>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            viewModel.FileName = "Songs.csv";
            viewModel.ContentType = "text/csv";
            viewModel.Content = _csvBuilder.BuildFile(list);

            return await Task.FromResult(viewModel);
        }
    }
}
```

Interfaces klasörüne aşağıdaki arayüzler eklenir

IEmailService.cs

```csharp

```

IUserService.cs

```csharp

```

