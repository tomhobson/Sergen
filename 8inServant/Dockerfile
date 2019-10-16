FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["8inServant/8inServant.csproj", "8inServant/"]
RUN dotnet restore "8inServant/8inServant.csproj"
COPY . .
WORKDIR "/src/8inServant"
RUN dotnet build "8inServant.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "8inServant.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "8inServant.dll"]