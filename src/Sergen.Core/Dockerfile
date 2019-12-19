FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-stretch AS build
WORKDIR /src
COPY ["Sergen.Master/Sergen.Master.csproj", "Sergen.Master/"]
RUN dotnet restore "Sergen.Master/Sergen.Master.csproj"
COPY . .
WORKDIR "/src/Sergen"
RUN dotnet build "Sergen.Master.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Sergen.Master.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Sergen.Master.dll"]