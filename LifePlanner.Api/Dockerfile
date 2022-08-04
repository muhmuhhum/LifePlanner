FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LifePlanner.Api/LifePlanner.Api.csproj", "LifePlanner.Api/"]
RUN dotnet restore "LifePlanner.Api/LifePlanner.Api.csproj"
COPY . .
WORKDIR "/src/LifePlanner.Api"
RUN dotnet build "LifePlanner.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LifePlanner.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LifePlanner.Api.dll"]
