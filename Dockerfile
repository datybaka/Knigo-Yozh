FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KnigoYozh.API/KnigoYozh.API.csproj", "KnigoYozh.API/"]
# Копируем остальные проекты, если нужно для сборки
COPY ["KnigoYozh.Domain/KnigoYozh.Domain.csproj", "KnigoYozh.Domain/"]
COPY ["KnigoYozh.Application/KnigoYozh.Application.csproj", "KnigoYozh.Application/"]
COPY ["KnigoYozh.Infrastructure/KnigoYozh.Infrastructure.csproj", "KnigoYozh.Infrastructure/"]

RUN dotnet restore "KnigoYozh.API/KnigoYozh.API.csproj"
COPY . .
WORKDIR "/src/KnigoYozh.API"
RUN dotnet build "KnigoYozh.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KnigoYozh.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KnigoYozh.API.dll"]