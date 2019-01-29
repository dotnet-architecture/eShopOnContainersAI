ARG configuration
ARG NODE_IMAGE=node:8.11

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk as dotnet-build
WORKDIR /src

FROM ${NODE_IMAGE} as node-build
WORKDIR /web
COPY src/Services/Identity/Identity.API .
RUN npm install -g bower@1.8.4
RUN bower install --allow-root

FROM dotnet-build as build
ARG configuration
WORKDIR /src/src/Services/Identity/Identity.API/wwwroot
COPY --from=node-build /web/wwwroot .
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/Identity/Identity.API
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build --no-restore -c ${configuration} -o /app

FROM build AS publish
ARG configuration
RUN dotnet publish --no-restore -c ${configuration} -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
