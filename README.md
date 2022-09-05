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

mkdir core
mkdir infrastructure
mkdir presentation

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
mkdir Entities
mkdir Enums
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

dotnet ef migrations add InitialCreate --startup-project ..\..\presentation\Librarian.WebApi
dotnet ef database update --startup-project ..\..\presentation\Librarian.WebApi

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

```csharp

```

ve Entity veri yapıları

```csharp

```