FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-stretch AS build
WORKDIR /src
COPY ["Sergen/Sergen.csproj", "Sergen/"]
RUN dotnet restore "Sergen/Sergen.csproj"
COPY . .
WORKDIR "/src/Sergen"
RUN dotnet build "Sergen.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Sergen.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Sergen.dll"]