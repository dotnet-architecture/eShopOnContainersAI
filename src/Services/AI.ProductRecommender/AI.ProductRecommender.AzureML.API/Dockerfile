ARG configuration

FROM microsoft/aspnetcore:2.0.3 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
ARG configuration
WORKDIR /src
COPY . .
RUN dotnet restore -nowarn:msb3202,nu1503
WORKDIR /src/src/Services/AI.ProductRecommender/AI.ProductRecommender.AzureML.API
RUN dotnet build -c ${configuration} -o /app

FROM build AS publish
ARG configuration
RUN dotnet publish --no-restore -c ${configuration} -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ProductRecommender.AzureML.API.dll"]
