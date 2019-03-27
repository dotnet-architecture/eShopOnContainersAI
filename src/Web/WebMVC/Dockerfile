FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/Web/WebMVC
RUN dotnet restore -nowarn:msb3202,nu1503

FROM build AS publish
ARG configuration
RUN dotnet publish --no-restore -c release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebMVC.dll"]
