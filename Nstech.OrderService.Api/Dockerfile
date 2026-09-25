FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Nstech.OrderService.Api/Nstech.OrderService.Api.csproj", "Nstech.OrderService.Api/"]
COPY ["Nstech.OrderService.Application/Nstech.OrderService.Application.csproj", "Nstech.OrderService.Application/"]
COPY ["Nstech.OrderService.Domain/Nstech.OrderService.Domain.csproj", "Nstech.OrderService.Domain/"]
COPY ["Nstech.OrderService.Infrastructure/Nstech.OrderService.Infrastructure.csproj", "Nstech.OrderService.Infrastructure/"]
RUN dotnet restore "Nstech.OrderService.Api/Nstech.OrderService.Api.csproj"
COPY . .
WORKDIR "/src/Nstech.OrderService.Api"
RUN dotnet build "Nstech.OrderService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nstech.OrderService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nstech.OrderService.Api.dll"]
