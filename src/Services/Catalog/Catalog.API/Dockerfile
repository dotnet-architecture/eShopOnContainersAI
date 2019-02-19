ARG configuration

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

RUN echo "Build configuration: ${configuration}"
FROM microsoft/dotnet:2.2-sdk AS build
ARG configuration
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/Catalog/Catalog.API
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build --no-restore -c ${configuration} -o /app

FROM build AS publish
ARG configuration
RUN echo "Build configuration: ${configuration}"
RUN dotnet publish --no-restore -c ${configuration} -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
