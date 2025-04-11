# Temel image (Docker Hub) seçilir
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Çalışma dizinini belirleriz
WORKDIR /app

# Proje dosyalarını kopyalarız
COPY . . 

# .NET projemizi build ederiz
RUN dotnet publish BaeglAI.API/BaeglAI.API.csproj -c Release -o /app/publish

# Uygulamanın çalışacağı son image'ı belirleriz
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

# Dışarıdan gelen isteklerin yönlendirileceği portu açarız
EXPOSE 8080

# Publish edilen dosyaları kopyalarız
COPY --from=build /app/publish . 

# Uygulamayı çalıştıran komut
ENTRYPOINT ["dotnet", "BaeglAI.API.dll"]
