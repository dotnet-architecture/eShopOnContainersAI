ARG configuration
ARG NODE_IMAGE=node:8.11

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk as dotnet-build
WORKDIR /src

FROM ${NODE_IMAGE} as node-build
WORKDIR /web
COPY src/Web/WebDashboardRazor .
RUN npm install -g gulp-cli
RUN npm install 
RUN gulp copy-assets

FROM dotnet-build AS build
ARG configuration
WORKDIR /src
COPY . .
WORKDIR /src/src/Web/WebDashboardRazor
RUN dotnet restore -nowarn:msb3202,nu1503
COPY --from=node-build /web/wwwroot wwwroot/
RUN dotnet build -c ${configuration} -o /app


FROM build AS publish
ARG configuration
RUN dotnet publish --no-restore -c ${configuration} -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebDashboardRazor.dll"]
