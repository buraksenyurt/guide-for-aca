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
dotnet new classlib -f netstandard2.1 --name BalladMngr.Domain
dotnet new classlib -f netstandard2.1 --name BalladMngr.Application

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
mkdir ExportBooks GetBooks
```