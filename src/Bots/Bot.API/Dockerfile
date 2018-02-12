FROM microsoft/aspnet
ARG source

RUN C:\Windows\System32\inetsrv\appcmd.exe set config -section:system.webServer/security/authentication/anonymousAuthentication /userName: /commit:apphost

WORKDIR /inetpub/wwwroot
COPY ${source:-obj/Docker/publish} .
