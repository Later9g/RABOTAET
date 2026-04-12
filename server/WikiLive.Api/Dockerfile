FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY server/WikiLive.Api/WikiLive.Api.csproj server/WikiLive.Api/
RUN dotnet restore server/WikiLive.Api/WikiLive.Api.csproj

COPY . .
WORKDIR /src/server/WikiLive.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "WikiLive.Api.dll"]
