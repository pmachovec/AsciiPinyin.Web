FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN dotnet dev-certs https
WORKDIR /src
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY Main/Server/AsciiPinyin.Web.Server.csproj Main/Server/
COPY Main/Client/AsciiPinyin.Web.Client.csproj Main/Client/
COPY Main/Shared/AsciiPinyin.Web.Shared.csproj Main/Shared/
RUN dotnet restore Main/Server/AsciiPinyin.Web.Server.csproj
COPY . .
WORKDIR /src/Main/Server
RUN dotnet build AsciiPinyin.Web.Server.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish AsciiPinyin.Web.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AsciiPinyin.Web.Server.dll"]

LABEL org.opencontainers.image.description "AsciiPinyin.Web - in development"
