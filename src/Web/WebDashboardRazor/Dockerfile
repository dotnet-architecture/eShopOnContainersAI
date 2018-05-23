ARG configuration

FROM microsoft/aspnetcore:2.0.5 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0.5-2.1.4 AS build
ARG configuration
WORKDIR /src
COPY . .
RUN dotnet restore -nowarn:msb3202,nu1503
WORKDIR /src/src/Web/WebDashboardRazor
RUN dotnet build -c ${configuration} -o /app

FROM build AS publish
ARG configuration
RUN dotnet publish --no-restore -c ${configuration} -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebDashboardRazor.dll"]
