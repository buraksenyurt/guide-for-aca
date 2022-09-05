# Uygulamalı Clean Architecture - 101 _(Ubuntu Üstünde .Net 6 Odaklı)_

Applied Clean Architecture. Uygulamalı clean architecture eğitimlerinde kullanılmak üzere oluşturduğum repo. Basit ve hafifsiklet bir projenin temel enstrümanları ile uçtan uca hazırlanmasına yardımcı olan anlatımı içermektedir. Materyal Ubuntu 20.04 sistemini baz almaktadır. Örnek senaryo bir şarkı sözü yazarının bestelerini yönetmek üzerine kurgulanmıştır. Eğitimin süresi 6 dersle sınırlıdır bu nedenle Redis Cache, Serilog, JWT Token ile Authentication entegrasyonları ile proje bazlı Unit Test'ler eklenmemiştir. Referans olarak önceki sürümlerde geliştirilmiş olan [scaling-robot](https://github.com/buraksenyurt/scaling-robot) projesine bakılabilir.

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
mkdir Services
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
dotnet ef migrations add InitialCreate --startup-project ../../presentation/BalladMngr.WebApi
dotnet ef database update --startup-project ../../presentation/BalladMngr.WebApi

cd ..
cd ..

# Vue Tarafının Oluşturulması
sudo npm install -g @vue/cli

cd presentation
npm install -g @vue/cli

# Presentation klasörü altındayken vue projesi oluşturulur
vue create web-app

# > Manually select features
# İzleyen kısımda yapılacak seçimler (Choose Vue Version, Babel, Router, Vuex, Linter/Formatter)
# Version olarak 2.x seçilir ki 3.x ile denemek lazım
# Use history mode for router sorusuna Y(Yes) cevabı verilir
# Ardından ESLint + Prettier seçilir
# List on save seçili bırakılır
# In dedicated config files seçili bırakılır

# Material Design yaklaşımına uygun olarak vuetify kütüphanesi eklendi
# librarian-app klasöründeyken aşağıdaki komut çalıştırılır
vue add vuetify

# Default(recommended) ile ilerlenir
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

## 02 - MediatR Paketinin Eklenmesi ve CQRS Hazırlıkları

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
dotnet add package Microsoft.Extensions.Options.ConfigurationExtensions

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

Serüven boyunca ihtiyacımız olacak bazı temel türleri ekleyelim. 

Olası özel exception tipleri...

SongNotFoundException.cs

```csharp
using System;

namespace BalladMngr.Application.Common.Exceptions
{
    public class SongNotFoundException
        : Exception
    {
        public SongNotFoundException(int songId)
          : base($"{songId} nolu kitap envanterde bulunamadı") { }
    }
}
```

SendEmailException.cs

```csharp
using System;

namespace BalladMngr.Application.Common.Exceptions
{
    public class SendEmailException
        : Exception
    {
        public SendEmailException(string message)
            : base(message)
        {
        }
    }
}
```

ValidationException.cs

```csharp
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BalladMngr.Application.Common.Exceptions
{
    /*
     * 
     * ValidationBehavior tipi tarafından kullanılan Exception türevidir.
     * 
     */
    public class ValidationException
        :Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        public ValidationException()
        {
            Errors = new Dictionary<string, string[]>();
        }
        public ValidationException(IEnumerable<ValidationFailure> errors)
            :this()
        {
            var errorGroups= errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

            foreach (var e in errorGroups)
            {
                Errors.Add(e.Key, e.ToArray());
            }
        }

    }
}
```

Yine serüven boyunca özel olarak MediatR tarafına entegre edeceğimiz bazı davranışlar var. Loglama, hata yakalama gibi. Bunları da Behaviors klasörüne sırasıyla ekleyelim.

ValidationBehavior.cs

```csharp
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Common.Behaviors
{
    /*
     * Bir Command çalışıp ekleme, güncelleme gibi işlemler yapıldığı sırada devreye giren doğrulama fonksiyonunu sağlıyoruz.
     * 
     * Eğer talebin geldiği komuta bir Validator uygulanmışsa buradaki süreç çalışıp doğrulama ihlalleri toplanacak.
     * Eğer ihlaller varsa bunu da ValidationException isimli bizim yazdığımı bir nesnede toplayıp sisteme exception basacağız.
     * Doğal olarak talep işlenmeyecek.
     */
    public class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var errors = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (errors.Count != 0)
                throw new ValidationException(errors);

            return await next();
        }
    }
}
```

LoggingBehavior.cs

```csharp
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Common.Behaviors
{
    /*
     * MediatR işleyişinde mesajlar arasına girmek mümkün.
     * Burada mesaj işlenmeden önce araya girip basitçe log atmaktayız.
     */
    public class LoggingBehavior<TRequest>
        : IRequestPreProcessor<TRequest>
    {
        private readonly ILogger<TRequest> _logger;
        public LoggingBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
        }
        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation($"Talep geldi {request}", requestName);
        }
    }
}
```

PerformanceBehvaior.cs

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Common.Behaviors
{
    /*
     * Taleplerin ne kadar sürede işlendiğini tespit etmek ve belli bir değerin altına indiğinde uyarı logu basmak için bu uyarlama oldukça kullanışlı.
     * LoggingBehavior'un uyguladığı arayüzden farklı bir arayüz uygulandığına dikkat etmek lazım.
     * Handle metodunda mesajı hat üzerindeki bir sonraki noktaya taşıdığımız next fonksiyonunun önüne ve sonrasına zamanlayıcı çağrıları koyduk.
     * Sonrasında cevaplama süresinin 250 milisaniye altında olup olmadığına bakıp uyarı logu basıyoruz.
     */
    public class PerformanceBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : MediatR.IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;
        private readonly Stopwatch _hgwells;
        public PerformanceBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
            _hgwells = new Stopwatch();
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _hgwells.Start();
            var response = await next();
            _hgwells.Stop();

            var responseDuration = _hgwells.ElapsedMilliseconds;
            if (responseDuration < 250)
                return response;

            _logger.LogWarning($"{typeof(TRequest).Name} normal çalışma süresini aştı. {responseDuration}. İçerik {request}");
            return response;

        }
    }
}
```

son olarak UnhandledExceptionBehavior.cs

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Common.Behaviors
{
    /*
     * Pipeline arayüzünden türeyen bu behavior tipi ile Query ve Command'lerin işleyişi sırasında oluşan exception durumlarını loglamaktayız.
     * Yakalanan Exception akışta ele alınmamış bir exception ise burada yakalamamız kolay olacaktır.
     */
    public class UnhandledExceptionBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : MediatR.IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;
        public UnhandledExceptionBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception excp)
            {
                _logger.LogError(excp, "Talep geldi ama bir exception oluştu");
                throw;
            }
        }
    }
}
```

Şimdi Mappings klasörüne IMapFrom.cs arayüzü eklenir.

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

namespace BalladMngr.Application.Common.Interfaces
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
using BalladMngr.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

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

Şarkı bilgilerini çekmek için kullanılacak query nesneleri de GetSongs klasörü altına eklenecekler. 

Ama öncesinde ilgili Dto nesnesi de eklemeli.

SongDto.cs

```csharp
SongDto.cs

```csharp
using AutoMapper;
using BalladMngr.Application.Common.Mappings;
using BalladMngr.Domain.Entities;

namespace BalladMngr.Application.Dtos.Songs
{
    /*
     * Bu aslında bir Data Transfer Object tipi.
     * 
     * Controller tarafından gelen istekleri ele alan MediatR nesneleri doğrudan Song tipi ile değil de,
     * Daha az özelliği içeren SongDto tipi ile çalışıyor. Son kullanıcıya da bu içerik verilecek.
     * 
     * Genelde Entity tipleri dolaşıma girdiğinde tüm özelliklerini sunmak istemediğimiz senaryolara dahil olabilir.
     * ViewModel'in bu durumlarda tüm entity nesnesi ile çalışmak yerine gerçekten ihtiyaç duyduğu özellikleri barındıran bir nesne ile çalışması doğru yaklaşımdır.
     * 
     * Bu DTO IMapFrom<T> arayüzünü kullanır. Bu kısım aslında AutoMapper'ın ilgilendiği arayüzdür.
     * IMapFrom içindeki Mapping metodu burada ele alınmış ve Language özelliği için ekstra bir işlem eklenmiştir.
     */
    public class SongDto
        : IMapFrom<Song>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Lyrics { get; set; }
        public int Language { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Song, SongDto>()
                .ForMember(dest => dest.Language
                    , opt => opt.MapFrom(source => (int)source.Language)
                    );
        }
    }
}
```

SongsViewModel.cs

```csharp

using BalladMngr.Application.Dtos.Songs;
using System.Collections.Generic;

namespace BalladMngr.Application.Songs.Queries.GetSongs
{
    /*
     * Şarkı listesinin tamamını çeken Query'nin çalıştığı ViewModel nesnesi
     * 
     */
    public class SongsViewModel
    {
        public IList<SongDto> SongList { get; set; }
    }
}
```

ve GetSongsQuery.cs

```csharp
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Application.Dtos.Songs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Queries.GetSongs
{
    /*
     * Query ile tüm şarkı listesinin çekilmesi sürecini ele alıyoruz.
     * 
     * Talebe karşılık SongsViewModel nesnesi dönülüyor ki o da içinde SongDto tipinden bir liste barındırmakta.
     * 
     */
    public class GetSongsQuery
        : IRequest<SongsViewModel>
    {
    }

    public class GetSongsQueryHandler
        : IRequestHandler<GetSongsQuery, SongsViewModel>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetSongsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<SongsViewModel> Handle(GetSongsQuery request, CancellationToken cancellationToken)
        {
            SongsViewModel Songs;

            Songs = new SongsViewModel
            {
                SongList = await _context
                .Songs
                .ProjectTo<SongDto>(_mapper.ConfigurationProvider)
                .OrderBy(b => b.Title)
                .ToListAsync(cancellationToken)
            }; // normal EF üstünden repository'ye gidip veriyi çekiyor

            return Songs;
        }
    }
}
```

Email gönderim servisi ve authentication tarafı için Dtos klasörü altına gerekli Dto veri yapılarını da ekleyelim.

EmailDto.cs

```csharp
namespace BalladMngr.Application.Dtos.Email
{
    public class EmailDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
```

AuthenticationRequest.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace BalladMngr.Application.Dtos.User
{
    /*
     * Authentication aşamasında gelen talebin içinde Username ve Password bilgisinin taşınmasını sağlayan sınıfımız
     */
    public class AuthenticationRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
```

AuthenticationResponse.cs

```csharp
namespace BalladMngr.Application.Dtos.User
{
    /*
     * Doğrulama başarılı olduğunda dönen response içeriğini temsil eden sınıf.
     * 
     * Aslında Claim sete ait bilgileri taşıdığını ifade edebiliriz.
     * Önemli detaylardan birisi Password alanının olmayışı ama üretilecek JWT token için Token isimli bir özellik kullanılmasıdır.
     */
    public class AuthenticationResponse
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public AuthenticationResponse(Domain.Entities.User user, string token)
        {
            UserId = user.UserId;
            Name = user.Name;
            LastName = user.LastName;
            Username = user.Username;
            Email = user.Email;
            Token = token;
        }
    }
}
```

Email gönderim servisi ve doğrulama hizmeti için gerekli Dto nesneleri hazır. Artık dışarıya açılacak sözleşmeleri tanımlayabiliriz.

IEmailService.cs

```csharp
using BalladMngr.Application.Dtos.Email;
using System.Threading.Tasks;

namespace BalladMngr.Application.Common.Interfaces
{
    /*
     * Email gönderim işlemini tanımlayan servis sözleşmesi.
     * 
     * Sistemde buna benzer işlevsel fonksiyonlar içeren servisleri birer arayüz ile tanımlıyoruz.
     * Böylece bağımlılıkları kolayca çözümletebiliriz. Dependency Injection uygulamak kolay olacaktır.
     * Bu servisler Application katmanında toplanıyorlar. 
     * Interfaces isimli bir klasör içerisinde konuşlandırmak oldukça mantıklı.
     * 
     */
    public interface IEmailService
    {
        Task SendAsync(EmailDto mailDto);
    }
}
```

IUserService.cs

```csharp
using BalladMngr.Application.Dtos.User;
using BalladMngr.Domain.Entities;

namespace BalladMngr.Application.Common.Interfaces
{
    /*
     * Kullanıcı servisi response içeriği ile gelen bilgilere göre doğrulama işini üstlenen
     * ve id üstünden kullanıcı bilgisi döndüren fonksiyonları tarifleyen bir sözleşme sunuyor.
     */
    public interface IUserService
    {
        AuthenticationResponse Authenticate(AuthenticationRequest model);
        User GetById(int userId);
    }
}
```

Tekrar CQRS tarafına dönelim ve Create, Update, Delete için gerekli komut sınıflarını oluşturalım.

CreateSongCommand.cs

```csharp
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Entities;
using BalladMngr.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Commands.CreateSong
{
    /*
     * Şarkı listesine yeni bir beste ekleme işini üstlenen MediatR tipleri.
     * Komut şarkı için gerekli parametreleri alırken geriye insert sonrası oluşan bir int değer(kuvvetle muhtemel primary key id değeri) dönüyor.
     * Handler sınıfı IApplicationDbContext üstünden gelen Entity Context nesnesini kullanarak besteyi repository'ye ekliyor.
     * 
     */
    public class CreateSongCommand
        : IRequest<int>
    {
        public string Title { get; set; }
        public Language Language{ get; set; }
        public string Lyrics { get; set; }
    }

    public class CreateSongCommandHandler
        : IRequestHandler<CreateSongCommand, int>
    {
        private readonly IApplicationDbContext _context;
        public CreateSongCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> Handle(CreateSongCommand request, CancellationToken cancellationToken)
        {
            var s = new Song
            {
                Title = request.Title,
                Lyrics=request.Lyrics,
                Language=request.Language,
                Status=Status.Draft
            };
            _context.Songs.Add(s);
            await _context.SaveChangesAsync(cancellationToken);

            return s.Id;
        }
    }
}
```

Bir şarkı eklenirken doğrulama işlemlerini de işletebiliriz. Bunun için,

CreateSongCommandValidator.cs

```csharp
using FluentValidation;
using BalladMngr.Application.Common.Interfaces;

namespace BalladMngr.Application.Songs.Commands.CreateSong
{
    /*
     * Şarkı bilgilerinin güncellendiği Command nesnesi için hazırlanmış doğrulama tipi. 
     * Esasında CreateSongCommandValidator ile neredeyse aynı.
     */
    public class CreateSongCommandValidator : AbstractValidator<CreateSongCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateSongCommandValidator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(v => v.Title)
              .NotEmpty().WithMessage("Şarkının başlığı olmalı!")
              .MaximumLength(100).WithMessage("Bu şarkının adı çok uzun!");

            RuleFor(v => v.Lyrics)
              .NotEmpty().WithMessage("Birkaç mısra da olsa sözler olmalı!")
              .MinimumLength(50).WithMessage("Bence en az 50 karakterden oluşan bir metin olmalı!")
              .MaximumLength(1000).WithMessage("Ne çok söz varmış. Şarkıyı kısaltalım.");
        }
    }
}
```

DeleteSongCommand.cs

```csharp
using BalladMngr.Application.Common.Exceptions;
using BalladMngr.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Commands.DeleteSong
{
    /*
     * beste silme işini ele aldığımı Command ve Handler tipleri.
     * 
     * Bir besteyi silmek için talebin içinde Id bilgisinin olması yeterli. Command buna göre düzenlendi.
     * Silme operasyonunu ele alan Handler tipimiz yine ilgili kitabı envanterde arıyor.
     * Eğer bulamazsa SongNotFoundException fırlatılıyor.
     * 
     */
    public class DeleteSongCommand
        : IRequest
    {
        public int SongId { get; set; }
    }

    public class DeleteSongCommandHandler
        : IRequestHandler<DeleteSongCommand>
    {
        private readonly IApplicationDbContext _context;
        public DeleteSongCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteSongCommand request, CancellationToken cancellationToken)
        {
            var b = await _context.Songs
              .Where(l => l.Id == request.SongId)
              .SingleOrDefaultAsync(cancellationToken);

            if (b == null)
            {
                throw new SongNotFoundException(request.SongId);
            }

            _context.Songs.Remove(b);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
```

ve güncelleme komutu için UpdateSongCommand.cs

```csharp
using BalladMngr.Application.Common.Exceptions;
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Songs.Commands.UpdateSong
{
    /*
     * Bir şarkının bilgilerini güncellemek isteyebilirim. Adı, içeriği, durumu vesaire.
     * Topluca bunların güncellemesini ele alan Command'ın beklediği özellikler aşağıdaki gibidir.
     * 
     * Handler sınıfı da bu Command'i kullanarak repository üzerinde gerekli güncellemeleri yapar. 
     * Id ile gelen kitap bilgisi bulunamazsa ortama SongNotFoundException isimli bizim yazdığımız bir istisna tipi fırlatılır.
     * 
     */
    public partial class UpdateSongCommand : IRequest
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string Lyrics { get; set; }
        public Language Language { get; set; }
        public Status Status { get; set; }
    }

    public class UpdateSongCommandHandler : IRequestHandler<UpdateSongCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateSongCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateSongCommand request, CancellationToken cancellationToken)
        {
            var s = await _context.Songs.FindAsync(request.SongId);
            if (s == null)
            {
                throw new SongNotFoundException(request.SongId);
            }
            s.Title = request.Title;
            s.Lyrics = request.Lyrics;
            s.Language = request.Language;
            s.Status = request.Status;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
```

Güncelleme işlemi sırasında da doğrulama operasyonu işletebiliriz. Bunun için

UpdateSongCommandValidator.cs

```csharp
using FluentValidation;
using BalladMngr.Application.Common.Interfaces;

namespace BalladMngr.Application.Songs.Commands.UpdateSong
{
    /*
     * Şarkı bilgilerinin güncellendiği Command nesnesi için hazırlanmış doğrulama tipi. 
     * Esasında CreateSongCommandValidator ile neredeyse aynı.
     */
    public class UpdateSongCommandValidator : AbstractValidator<UpdateSongCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateSongCommandValidator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(v => v.Title)
              .NotEmpty().WithMessage("Şarkının başlığı olmalı!")
              .MaximumLength(100).WithMessage("Bu şarkının adı çok uzun!");

            RuleFor(v => v.Lyrics)
              .NotEmpty().WithMessage("Birkaç mısra da olsa sözler olmalı!")
              .MinimumLength(50).WithMessage("Bence en az 50 karakterden oluşan bir metin olmalı!")
              .MaximumLength(1000).WithMessage("Ne çok söz varmış. Şarkıyı kısaltalım.");
        }
    }
}
```

## 03 - DI Mekanizması ve Shared Geliştirmeleri

DI ayarlamaları için Application projesine DependencyInjection.cs eklenir.

DependencyInjection.cs

```csharp
using FluentValidation;
using BalladMngr.Application.Common.Behaviors;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace BalladMngr.Application
{
    /*
     * Uygulama katmanının içindeki servisleri kullanan tarafa(örneğin Web API) bildirmek için kullandığımız bir sınıf olarak düşünebiliriz.
     * IServiceCollection zaten .Net'in dahili DI servislerine ulaşmakta önemli bir aracı.
     * AutoMapper, MediatR, Validation ve Behavior'ları servisler koleksiyonuna ekleyen bir metot.
     * 
     * Bu sınıfı büyük ihtimalle Web API projesinin Startup'ındaki ConfigureServices metodunda kullanacağız. Böylece Web API çalışma zamanına
     * gerekli DI bağımlılıkları otomatik olarak yüklenmiş olacak.
     */
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehavior<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));

            return services;
        }
    }
}
```

Csv ve email işlemleri için bazı yardımcı paketlerden yararlanacağız. Application tarafında interface olarak tanımlanan bu sözleşmelerinin uygulayıcı Infrastructure katmanındaki Shared isimli kütüphane. Bu nedenle Shared isimli projeye aşağıdaki paketleri eklemeliyiz.

```shell
dotnet add package CsvHelper
dotnet add package MailKit
dotnet add package MimeKit
```

Şimdi Shared projesinin Services klasörüne CsvBuilder ve EmailService sınıfları eklenir.

CsvBuilder.cs

```csharp
using CsvHelper;
using BalladMngr.Application.Songs.Queries.ExportSongs;
using BalladMngr.Application.Common.Interfaces;
using System.Globalization;

namespace BalladMngr.Shared.Services
{
    /*
     * Şarkı kaytılarını CSV dosyada geriye veren fonksiyonelliği içeren sınıfımız.
     * Application içerisinde tanımladığımız servis sözleşmesini uyguladığına dikkat edelim.
     * Yani servisin sözleşmesi Core katmanındaki Application projesinde, uyarlaması Infrastructure içerisindeki Shared projesinde.
     */
    public class CsvBuilder
        : ICsvBuilder
    {
        public byte[] BuildFile(IEnumerable<SongRecord> songRecords)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                {
                    using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csvWriter.WriteRecords(songRecords);
                }
                return ms.ToArray();
            }
        }
    }
}
```

EmailService.cs

```csharp
using BalladMngr.Application.Common.Exceptions;
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Application.Dtos.Email;
using BalladMngr.Domain.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BalladMngr.Shared.Services
{
    /*
     * Mail gönderme işini yapan asıl sınıf.
     * Tahmin edileceği üzere tüm servis sözleşmelerinin olduğu Core katmanındaki Application projesindeki arayüzü kullanıyor.
     * Loglama ve Email sunucu ayarlarını almak için gerekli bağımlılıklar Constructor üzerinden enjekte edilmekte.
     * SendAsync metodu EmailDto isimli Data Transfer Object üstünden gelen bilgilerdeki kişiye mail olarak gönderiyor.
     * Mail gönderimi sırasında bir istisna olması oldukça olası. Sonuçta bir SMTP sunucum bile yok. 
     * Bunu hata loglarında veya exception tablosunda ayrıca anlayabilmek için SendMailException isimli bir istisna tipi kullanılıyor.
     */
    public class EmailService
        : IEmailService
    {
        private ILogger<EmailService> _logger { get; }
        private MailSettings _mailSettings { get; }

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailDto request)
        {
            try
            {
                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(request.From ?? _mailSettings.From)
                };
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                var builder = new BodyBuilder { HtmlBody = request.Body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw new SendEmailException(ex.Message);
            }
        }
    }
}
```

Sırada Shader kütüphanesi için gereki DI mekanizmasının entegrasyonu var. Bunun için projeye DependencyInjection sınıfını ekliyoruz.

DependencyInjection.cs

```csharp
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Settings;
using BalladMngr.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BalladMngr.Shared
{
    /*
     * 
     * Çağıran ortamdaki DI servislerine Shared tarafından gelen bağımlılıkları yüklememizi kolaylaştıran sınıf.
     * BalladMngr.Application'daki DependencyInjection sınıfı ile aynı teoriyi kullanıyor ve yine
     * kuvvetle muhtemel buradaki servisleri tüketecek olan Web API projesindeki Startup sınıfındaki ConfigureServices içinden çağırılacak
     */
    public static class DependencyInjection
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ICsvBuilder, CsvBuilder>();

            return services;
        }
    }
}
```

## 04 - Data Tarafının Organizasyonu ve EF Migration İşleri

İlk olarak Context tiplerini eklemeliyiz.

BalladMngrDbContext.cs

```csharp
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalladMngr.Data.Contexts
{
    /*
     * DbContext nesnesi.
     * IApplicationDbContext türetmesine de dikkat edelim. Core.Application katmanındaki sözleşmeyi kullanıyoruz.
     * Bu DI servislerine Entity Context nesnesini eklerken işimize yarayacak.
     * 
     */
    public class BalladMngrDbContext
        : DbContext, IApplicationDbContext
    {
        public BalladMngrDbContext(DbContextOptions<BalladMngrDbContext> options)
            : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }
    }
}
```

Örnek verilerin girilmesini sağlayacak birde Seed sınıfı ekleyelim.

BalladMngrDbContextSeed.cs

```csharp
using BalladMngr.Domain.Entities;
using BalladMngr.Domain.Enums;

namespace BalladMngr.Data.Contexts
{
    /*
     * Seed operasyonu için kullanılan statik sınıfımız.
     * Eğer hiç şarkı yoksa birkaç tane ekleyecek.
     */
    public static class BalladMngrDbContextSeed
    {
        public static async Task SeedDataAsync(BalladMngrDbContext context)
        {
            if (!context.Songs.Any())
            {
                await context.Songs.AddAsync(new Song
                {
                    Title = "The Wall",
                    Lyrics = @"We don't need no education
                                We don't need no thought control
                                No dark sarcasm in the classroom
                                Teacher, leave them kids alone",
                    Language = Language.English,
                    Status = Status.Draft
                });
                await context.Songs.AddAsync(new Song
                {
                    Title = "Mazeretim Var Asabiyim Ben",
                    Lyrics = @"Gülmüyor yüzüm hayat zor oldu
                                Güller susuz kurudu soldu
                                Tövbe ettim gene bozuldu
                                Yüreğim yanar
                                Mazeretim var; asabiyim ben
                                Mazeretim var; asabiyim ben",
                    Language = Language.Turkish,
                    Status = Status.Completed
                });
                await context.Songs.AddAsync(new Song
                {
                    Title = "Dönence",
                    Lyrics = @"Simsiyah gecenin koynundayım yapayalnız
                                Uzaklarda bir yerlerde güneşler doğuyor
                                Biliyorum
                                Dönence",
                    Language = Language.Turkish,
                    Status = Status.Draft
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
```

Diğer katmanlarda olduğu gibi burada da bağımlılıkları DI mekanizması ile çalışma zamanı için çözümleyeceğiz.

DependencyInjection.cs

```csharp
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BalladMngr.Data
{
    /*
     * Çalışma zamanı DI servislerini Entity Framework DbContext türevimizi eklemek için kullanılan sınıf.
     * 
     */
    public static class DependencyInjection
    {
        public static IServiceCollection AddData(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<BalladMngrDbContext>(
                options => options.UseSqlite(configuration.GetConnectionString("BalladMngrDbConnection")) // SQLite veri tabanı bağlantı bilgisi konfigurasyon üstünden gelecek. Web API' nin appSettings.json dosyasından
                );

            // services.AddDbContext<BalladMngrDbContext>(
            //     options => options.UseSqlServer(configuration.GetConnectionString("BalladMngrDbConnection"))
            //     );

            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<BalladMngrDbContext>());
            return services;
        }
    }
}
```

## 05 - Web API Projesi için Hazırlıklar

Controller tarafında gerekli sınıfları ekleyelim.

SongsController.cs

```csharp
using BalladMngr.Application.Songs.Commands.CreateSong;
using BalladMngr.Application.Songs.Commands.DeleteSong;
using BalladMngr.Application.Songs.Commands.UpdateSong;
using BalladMngr.Application.Songs.Queries.GetSongs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BalladMngr.WebApi.Controllers
{
    /*
     * SongsController, şarkı ekleme, silme, güncelleme ve şarkı listesini ViewModel ölçütünde çekme işlemlerini üstleniyor.
     * 
     * Constructor üstünden MediatR modülünün enjekte edildiğini görebiliyoruz.
     * 
     * Önceki versiyonuna göre en büyük fark elbetteki metotlarda Query/Command nesnelerinin kullanılması.
     * 
     * Ayrıca fonksiyon içeriklerine bakıldığında yapılan tek şey Send ile ilgili komutu MeditaR'a yönlendirmek. O gerisini halleder
     * 
     */
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SongsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<SongsViewModel>> Get()
        {
            return await _mediator.Send(new GetSongsQuery());
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateSongCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteSongCommand { SongId = id });
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateSongCommand command)
        {
            if (id != command.SongId)
                return BadRequest();
            await _mediator.Send(command);

            return NoContent();
        }
    }
}
```

ExportController.cs (CSV Export tarafı için)

```csharp
using BalladMngr.Application.Songs.Queries.ExportSongs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BalladMngr.WebApi.Controllers
{
    /*
     * Şarkı listesini CSV formatında export etmemizi sağlayan Controller.
     * 
     * SongsController'da olduğu gibi MediatR kullanıyor ve CSV çıktısı için ilgili Query komutunu işletiyor.
     */
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController 
        : ControllerBase
    {
        private readonly IMediator _mediator;
        public ExportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<FileResult> Get()
        {
            var model = await _mediator.Send(new ExportSongsQuery());
            return File(model.Content, model.ContentType, model.FileName);
        }
    }
}
```

UserController.cs (Auth tarafı için)

```csharp
using BalladMngr.Application.Common.Interfaces;
using BalladMngr.Application.Dtos.User;
using Microsoft.AspNetCore.Mvc;

namespace BalladMngr.WebApi.Controllers
{
    /*
     * Kullanıcı doğrulama işini üstlenen controller tipidir.
     * Authenticat fonksiyonu /auth talebi ile çalışır ve Body içeriği ile gelen model nesnesindeki kullanıcı adı şifre üstünden UserService'e gidilir.
     * Geçerli bir kullanıcı ise tamam ama değilse HTTP 400 Bad Request hatası basarız.
     */
    [ApiController]
    [Route("api/[controller]")]
    public class UserController
        : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) => _userService = userService;

        [HttpPost("auth")]
        public IActionResult Authenticate([FromBody] AuthenticationRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new
                {
                    message = "Kullanıcı adın ya da şifren hatalı!"
                });

            return Ok(response);
        }
    }
}
```

program.cs içeriği aşağıdaki gibi düzenlenir.

```csharp
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
```

appsettings.json'da aşağıdaki gibi düzenlenir.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "BalladMngrDbConnection": "Data Source=BalladMngrDatabase.sqlite3"
  },
  "MailSettings": {
    "From": "admin@blabal.bla",
    "SmtpHost": "localhost",
    "SmtpPort": "",
    "SmtpPass": "",
    "DisplayName": "Administrator"
  },
  "AllowedHosts": "*"
}
```

Api uygulamasını test etmeden önce Sqlite tarafı için gerekli migration işlemlerini başlatmak gerekecektir. Bunun için Infrastructure katmanındaki Data projesi klasöründeyken aşağıdaki komutlar çalıştırılır.

```shell
dotnet ef migrations add InitialCreate --startup-project ../../presentation/BalladMngr.WebApi
dotnet ef database update --startup-project ../../presentation/BalladMngr.WebApi
```

Bu işlemler sonucunda Data projesinde Migration klasörü, WebApi projesinde de sqlite3 uzantılı veri tabanı dosyası oluşur. Kontrol için bir Sqlite görüntüleme eklentisinden yararlanılabilir. Ayrıca web api projesi çalıştırıldıktan sonra https://localhost:7008/swagger/index.html adresine gidilerek ilgili testler yapılabilir.

## 06 - Vue Tarafındaki Geliştirmeler

```bash
# Servis haberleşmesi için axios kullanacağız bu nedenle ilgili paketi install etmeliyiz
# Bunu tabii ki ballad-mngr-app klasörü altında çalıştırmalıyız.
sudo npm i axios

sudo npm i vuelidate
```

vuelidate kullanımı için plugins klasörüne vuelidate.js eklenmelidir.

```js
import Vue from "vue";
import Vuelidate from "vuelidate";
Vue.use(Vuelidate);
```

Doğrulama opsiyonları için src klasörü altına validators isimli bir klasör açıp index.js dosyasını ekleyelim.

```js
import {
    required,
    minLength,
    maxLength
} from "vuelidate/lib/validators";

export default {
    addSong: {
        title: {
            required, maxLength: maxLength(50)
        },
        lyrics: {
            required, minLength: minLength(50), maxLength: maxLength(1000)
        },
    }
};
```

Temel komponentleri ekleyelim. Bileşenleri components klasörü altına ekleyeceğiz.

AddSongForm.vue

```vue
<template>
    <v-row justify="center">
        <v-dialog v-model="dialog" persistent max-width="600px">
            <template v-slot:activator="{ on, attrs }">
                <v-btn style="margin-top: 1rem"
                       rounded
                       color="light-blue"
                       dark
                       v-bind="attrs"
                       v-on="on">
                    <v-icon left>mdi-plus</v-icon>
                    Koleksiyona yeni bir beste ekle.
                </v-btn>
            </template>
            <v-card>
                <form @submit.prevent="
            addSongAction(body);
            body = {};
          ">
                    <v-card-title>
                        <span class="headline">Şarkı Bilgileri</span>
                    </v-card-title>
                    <v-card-text>
                        <v-container>
                            <v-row>
                                <v-col cols="12" sm="6">
                                    <v-text-field label="Adı"
                                                  v-model="body.title"
                                                  @input="$v.body.title.$touch()"
                                                  @blur="$v.body.title.$touch()"
                                                  :error-messages="titleErrors"
                                                  required></v-text-field>
                                </v-col>
                                <v-col cols="12">
                                    <v-textarea label="Sözleri"
                                                v-model="body.lyrics"
                                                @input="$v.body.lyrics.$touch()"
                                                @blur="$v.body.lyrics.$touch()"
                                                :error-messages="lyricsErrors"
                                                required></v-textarea>
                                </v-col>

                                <v-col cols="12" sm="6">
                                    <v-select v-model="body.language"
                                              :items="languages"
                                              item-text="name"
                                              item-value="id"
                                              label="Dili"
                                              persistent-hint
                                              return-object
                                              single-line>
                                    </v-select>
                                </v-col>
                            </v-row>
                        </v-container>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer></v-spacer>
                        <v-btn color="blue darken-1" text @click="dialog = false">
                            Close
                        </v-btn>
                        <v-btn color="blue darken-1"
                               text
                               @click="dialog = false"
                               :disabled="$v.$anyError"
                               type="submit">
                            Save
                        </v-btn>
                    </v-card-actions>
                </form>
            </v-card>
        </v-dialog>
    </v-row>
</template>

<script>
    import { mapActions } from "vuex";
    import validators from "@/validators";
    export default {
        name: "AddSongForm",
        data: () => ({
            body: {
                title: "",
                lyrics: "",
                language: 1
            },
            dialog: false,
            languages: [
                { id: 0, name: "İngilizce" }
                , { id: 1, name: "Türkçe" }
                , { id: 2, name: "İspanyolca" }
            ]
        }),
        methods: {
            ...mapActions("SongModule", ["addSongAction"]),
        },
        computed: {
            titleErrors() {
                const errors = [];
                if (!this.$v.body.title.$dirty)
                    return errors;
                !this.$v.body.title.required && errors.push("Lütfen şarkının adını yaz.");
                !this.$v.body.title.maxLength && errors.push("En fazla 50 karakter.");
                return errors;
            },
            lyricsErrors() {
                const errors = [];
                if (!this.$v.body.lyrics.$dirty) return errors;
                !this.$v.body.lyrics.required && errors.push("Lütfen şarkı sözlerini gir.");
                !this.$v.body.lyrics.minLength && errors.push("En az 50 karakter olsun");
                !this.$v.body.lyrics.maxLength && errors.push("En fazla 1000 karakter.");
                return errors;
            },
        },
        validations: {
            body: {
                title: validators.addSong.title,
                lyrics: validators.addSong.lyrics,
            }
        }
    };
</script>
```

SongListCard.vue

```Vue
<template>
  <v-skeleton-loader
    v-if="loading"
    width="500"
    max-width="600"
    height="100%"
    type="card"
  ></v-skeleton-loader>

  <v-card v-else width="500" max-width="600" height="100%">
    <v-toolbar color="pink" dark>
      <v-toolbar-title>Besteler</v-toolbar-title>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-list-item-group color="primary">
      <v-list-item v-for="song in songs" :key="song.id">
        <v-list-item-content>
          <v-list-item-title v-text="song.title"></v-list-item-title>
          <v-list-item-subtitle v-text="song.status"></v-list-item-subtitle>
        </v-list-item-content>
        <v-list-item-action>
          <v-icon @click="removeSong(song.id)"> mdi-delete-outline </v-icon>
        </v-list-item-action>
      </v-list-item>
    </v-list-item-group>
  </v-card>
</template>

<script>
import { mapActions, mapGetters } from "vuex";
export default {
  name: "SongListCard",
  computed: {
    ...mapGetters("songModule", {
      songs: "songs",
      loading: "loading",
    }),
  },
  methods: {
    ...mapActions("songModule", ["removeSongAction"]),
    removeSong(songId) {
      const confirmed = confirm("Bu besteyi silmek istediğine emin misin?");
      if (!confirmed) return;
      this.removeSongAction(songId);
    },
  },
};
</script>
```

NavigationBar.vue

```Vue
<template>
    <v-app-bar app color="primary" light>
      <div>
        <v-btn color="primary" outlined :to="{ path: '/' }">
          <span class="menu">Lobi</span>
        </v-btn>
  
        <router-link to="/about">
          <v-btn color="primary" outlined>
            <span class="menu">Bilgi</span>
          </v-btn>
        </router-link>

        <v-btn
          color="primary"
          outlined
          :to="{ path: '/dashboard' }"
        >
          <span class="menu">Besteler</span>
        </v-btn>
      </div>
    </v-app-bar>
  </template>
  
  <script>
  export default {
    name: "NavigationBar"    
  };
  </script>
  
  <style scoped>
  .menu {
    color: white;
    text-decoration: none;
  }
  </style>
```

Views isimli klasör altında dashboard isimli bir klasör açalım. Ardından buraya aşağıdaki view nesnelerini ekleyelim.

SongList.vue

```vue
<template>
  <v-container>
    <div class="text-h4 mb-10">Beste Listesi</div>
    <div class="v-picker--full-width d-flex justify-center" v-if="loading">
      <v-progress-circular
        :size="70"
        :width="7"
        color="purple"
        indeterminate
      ></v-progress-circular>
    </div>

    <v-simple-table>
      <template v-slot:default>
        <thead>
          <tr>
            <th>Title</th>
            <th>Language</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="b in songs" :key="b.id">
            <td>{{ b.title }}</td>
            <td>{{ getLang(b.language) }}</td>
          </tr>
        </tbody>
      </template>
    </v-simple-table>
  </v-container>
</template>

<script>
import { mapActions, mapGetters } from "vuex";
export default {
  name: "songList",
  async mounted() {
    await this.getSongsAction();
    this.songList = this.songs.map((pl) => pl);
  },
  data() {
    return {
      songList: [],
      loading: false,
    };
  },
  methods: {
    ...mapActions("songModule", ["getSongsAction"]),
    getLang(language) {
      switch (language) {
        case 0:
          return "İngilizce";
        case 1:
          return "Türkçe";
        case 2:
          return "İspanyolca";
        default:
          return "Bilemedim";
      }
    },
  },
  computed: {
    ...mapGetters("songModule", {
      songs: "songs",
    }),
  },
};
</script>

```

DefaultContent.vue

```vue
<template>
    <div>
        <div class="text-h2 my-4">Besteler Panosu</div>
        <div class="default-content">
            <div style="margin-right: 4rem; margin-bottom: 4rem">
                <SongListCard @handleShowBooks="handleShowSongs" />
                <AddSongForm />
            </div>
        </div>
        <div v-if="showSongs">
            <AddSongForm :songId="songId" />
        </div>
    </div>
</template>

<script>
    import { mapActions } from "vuex";
    import SongListCard from "@/components/SongListCard";
    import AddSongForm from "@/components/AddSongForm";
    export default {
        name: "DefaultContent",
        components: {
            SongListCard,
            AddSongForm
        },
        methods: {
            ...mapActions("songModule", ["getSongsAction"]),
            handleShowSongs(show, id) {
                this.showSongs = show;
                this.songId = id;
            },
        },
        data: () => ({
            showSongs: false,
            bookId: 0
        }),
        mounted() {
            this.getSongsAction();
            this.showSongs = false;
        },
    };
</script>


<style scoped>
    .default-content {
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: flex-start;
    }
</style>
```

index.vue

```vue
<template>
    <v-sheet
      height="100vh"
      class="overflow-hidden"
      style="display: flex; flex-direction: row; justify-content: flex-start"
    >
      <v-navigation-drawer permanent expand-on-hover>
        <v-list class="d-flex flex-column justify-center align-center">
          <v-list-item class="px-2">
            <v-list-item-avatar>
              <v-img
                src="https://randomuser.me/api/portraits/women/27.jpg"
              ></v-img>
            </v-list-item-avatar>
          </v-list-item>
        </v-list>

        <v-divider></v-divider>
  
        <v-list nav dense>
          <router-link to="/dashboard/" class="menu link">
            <v-list-item link>
              <v-list-item-icon>
                <v-icon>mdi-view-dashboard-variant-outline</v-icon>
              </v-list-item-icon>
              <v-list-item-title>Dashboard</v-list-item-title>
            </v-list-item>
          </router-link>
          <router-link to="/dashboard/song-list" class="menu link">
            <v-list-item link>
              <v-list-item-icon>
                <v-icon>mdi-format-list-bulleted</v-icon>
              </v-list-item-icon>
              <v-list-item-title> Besteler Listesi </v-list-item-title>
            </v-list-item>
          </router-link>          
        </v-list>
      </v-navigation-drawer>
      <v-container>
        <router-view />
      </v-container>
    </v-sheet>
  </template>
  
  <script>
  
  export default {
    name: "dashboard-view",
  };
  </script>
  
  <style scoped>
  .link {
    text-decoration: none;
  }
  </style>
```

AboutView.vue' yu şöyle değiştirelim.

```vue
<template>
    <div class="about">
      <h1>Bestelerim</h1>
      <p>
        Test amaçlı bir açıklama gelir.
      </p>
    </div>
  </template>
  ```

HomeView.vue'yu da şöyle.

```vue
<template>
  <div class="home fill-height d-flex justify-center align-center">
    <h1>Canım Şarkılarım :P</h1>
  </div>
</template>

<script>
export default {
  name: "home-view",
};
</script>
```

src altına api klasörü açılır ve gerekli servisler bağlanır.

song-service.js

```js
import api from "@/api/api-config";

export async function getSongsAxios() {
  return await api.get(`Songs/`);
}
```

api-config.js

```js
import axios from "axios";
const debug = process.env.NODE_ENV !== "production";

/*
    Üretim ortamında değilse localhost'tan üretim ortamındaysak gerçek adresinden bağlanacağımız bir axios nesnesi üretiliyor
*/
const baseURL = debug
  ? "https://localhost:5001/api"
  : "https://mylibrary.somewhere/api";

let api = axios.create({ baseURL });

export default api;
```

router klasöründeki index.js'i aşağıdaki gibi değiştirelim.

```js
import Vue from "vue";
import VueRouter from "vue-router";
import Home from "@/views/Home.vue";
import SongList from "@/views/dashboard/SongList";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "Home",
    component: Home,
  },
  {
    path: "/about",
    name: "About",
    component: () =>
      import(/* webpackChunkName: "about" */ "../views/About.vue"),
    meta: {
      requiresAuth: false, // Hakkında sayfası için authorization gerekli değil.
    },
  },
  {
    path: "/dashboard",
    component: () => import("@/views/dashboard"),
    meta: {
      requiresAuth: false
    },
    children: [
      {
        path: "",
        component: () => import("@/views/dashboard/DefaultContent"),
      },
      {
        path: "song-list",
        component: SongList,
      }
    ],
  }    
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes,
});

export default router;
```

store klasörü içeriğini aşağıdaki gibi düzenleyelim.

```bash
cd store
mkdir song
cd song
touch action-types.js actions.js getters.js mutations.js services.js state.js index.js
```

action-types.js aşağıdaki gibi yazılır.

```js
export const LOADING_SONGS = "LOADING_SONGS";
export const GET_SONGS = "GET_SONGS";
export const REMOVE_SONG = "REMOVE_SONG";
export const ADD_SONG = "ADD_SONG";
```

state.js

```js
const state = {
    songs: [],
    loading: false
  };
  
  export default state;
```

mutations.js

```js
import * as actionTypes from "./action-types";

const mutations = {
    [actionTypes.GET_SONGS](state, songs) {
        state.songs = songs;
    },

    [actionTypes.LOADING_SONGS](state, value) {
        state.loading = value;
    },

    [actionTypes.REMOVE_SONG](state, id) {
        state.songs = state.songs.filter((tl) => tl.id !== id);
    },

    [actionTypes.ADD_SONG](state, newSong) {
        state.songs.unshift(newSong);
    }
};

export default mutations;
```

getters.js

```js
const getters = {
    songs: state => state.songs,
    loading: state => state.loading
};
export default getters;
```

services.js

```js
import api from "@/api/api-config";

/*
   api-config üstünden api tarafında CRUD taleplerini gönderen fonksiyonları içerir
*/
export async function getSongs() {
  return await api.get("songs");
}

export async function deleteSong(id) {
  return await api.delete("songs/" + id);
}

export async function addSong(newSong) {
    return await api.post("songs", newSong);
}
```

actions.js

```js
import * as actionTypes from "./action-types";
import { getSongs, deleteSong, addSong } from "@/store/song/services";

export async function getSongsAction({ commit }) {
    // Şarkıların yüklendiğine dair bir durum bildiriyor
    commit(actionTypes.LOADING_SONGS, true);

    try {
        // servis fonksiyonundan veri çekiliyor
        const { data } = await getSongs();
        // şarkı listesinin alınması farklı bir durum ve payload olarak da listenin kendisi bildiriliyor
        commit(actionTypes.GET_SONGS, data.songList);
    } catch (e) {
        console.log(e);
    }
    // Şarkıların yüklenme durumu sona erdiği için false ile bir durum bilgilendirilmesi yapılıyor
    commit(actionTypes.LOADING_SONGS, false);
}

// Listeden şarkı çıkarmak için kullanılan fonksiyon
export async function removeSongAction({ commit }, payload) {
    commit(actionTypes.LOADING_SONGS, true);

    try {
        await deleteSong(payload);
        commit(actionTypes.REMOVE_SONG, payload);
    } catch (e) {
        console.log(e);
    }

    commit(actionTypes.LOADING_SONGS, false);
}

// Yeni bir şarkı eklemek için kullanılan fonksiyon
export async function addSongAction({ commit }, payload) {
    var langs = {
        English: 0,
        Turkish: 1,
        Spanish: 2
    };
    switch (payload.language.id) {
        case 0:
            payload.language = langs.English;
            break;
        case 1:
            payload.language = langs.Turkish;
            break;
        case 2:
            payload.language = langs.Spanish;
            break;
        default:
            payload.language = langs.Turkish;
    }
    commit(actionTypes.LOADING_SONGS, true);

    try {
        const { data } = await addSong(payload);
        payload.id = data;
        commit(actionTypes.ADD_SONG, payload);
    } catch (e) {
        console.log(e);
    }

    commit(actionTypes.LOADING_SONGS, false);
}
```

index.js

```js
import state from "./state";
import getters from "./getters";
import mutations from "./mutations";
import * as actions from "./actions";

export default {
    namespaced: true,
    getters,
    mutations,
    actions,
    state
};
```

ve pek tabii song altındaki bileşenleri dışarıya açan index dosyası _(store altındaki)_

```js
import Vue from "vue";
import Vuex from "vuex";
import createLogger from "vuex/dist/logger";
import songModule from "./song";

Vue.use(Vuex);

const debug = process.env.NODE_ENV !== "production";
const plugins = debug ? [createLogger({})] : [];

export default new Vuex.Store({
  modules: {
    songModule,
  },
  plugins,
});
```

Bu işlemler ardından App.vue aşağıdaki gibi düzenlenebilir.

```vue
<template>
  <v-app>
    <NavigationBar /> 
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script>
import NavigationBar from "@/components/NavigationBar";
export default {
  name: "App",
  components: {
    NavigationBar,
  },
};
</script>
```

main.js içeriği de şöyle düzenlenebilir.

```js
import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store";
import vuetify from "./plugins/vuetify";
import "./plugins/vuelidate"; // Doğrulama paketini projede kullanabilmek için eklendi

Vue.config.productionTip = false;

new Vue({
  router,
  store,
  vuetify,
  render: (h) => h(App),
}).$mount("#app");
```

# 07 Web Api tarafından Vue'nun Otomatik Olarak Çalışması

Web Api ve Vue tarafının Single Page Application olarak çalışması için bir şeyler yapabiliriz. Öncelikle WebApi projesinde ilgili paketi eklemek lazım.

```bash
dotnet add package VueCliMiddleware
```

Ardından WebApi projesinde program.cs'de bazı değişiklikler gerekiyor. Program.cs'in son hali şöyle olacaktır.

```csharp
using BalladMngr.Application;
using BalladMngr.Data;
using BalladMngr.Data.Contexts;
using BalladMngr.Shared;
using VueCliMiddleware;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "../web-app/dist";
});

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

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../ballad-mngr-app";

    if (app.Environment.IsDevelopment())
    {
        spa.UseVueCli(npmScript: "serve"); // npm run server komutunu tetikler
    }
});

app.Run();
```

Şu aşamada sistemi ayağa kaldırmak için WebApi projesindeyken dotnet run komutunu verebiliriz. 

## İpuçları

Eğer SPA'dan vazgeçilirse dotnet web api ve vue uygulamaları ayrı ayrı çalıştırılabilir.

```bash
dotnet run

npm run serve
```

Vue tarafında oluşabilecek bazı formatsal hataları otomatik olarak ayıklamak için aşağıdaki komut kullanılabilir.

```bash
npm run lint -- --fix
```
