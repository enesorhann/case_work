# 1) React + TypeScript arayüzünü derle (çıktı: /src/wwwroot)
FROM node:22-alpine AS frontend
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
# "Örnek CSV ile dene" düğmesi ödev CSV'sini derleme sırasında arayüze gömer
COPY data/urunler_duzeltilmis_test.csv /src/data/
RUN npm run build

# 2) ASP.NET Core API'yi derle
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MiniDpp.csproj ./
RUN dotnet restore MiniDpp.csproj

COPY . ./
COPY --from=frontend /src/wwwroot ./wwwroot
RUN dotnet publish MiniDpp.csproj -c Release -o /app/publish --no-restore

# 3) Çalışma imajı
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MiniDpp.dll"]
