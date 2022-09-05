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

Email gönderim servisi ve authentication tarafı için Dtos altına gerekli Dto veri yapılarını da ekleyelim.

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

