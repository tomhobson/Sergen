FROM mcr.microsoft.com/dotnet/core/aspnet:5.0-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-stretch AS build
WORKDIR /src
COPY ["Sergen.Main/Sergen.Main.csproj", "Sergen.Main/"]
RUN dotnet restore "Sergen.Main/Sergen.Main.csproj"
COPY . .
WORKDIR "/src/Sergen"
RUN dotnet build "Sergen.Main.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Sergen.Main.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Sergen.Main.dll"]