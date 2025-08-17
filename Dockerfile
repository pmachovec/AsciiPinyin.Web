FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN dotnet dev-certs https

WORKDIR /src

COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY Main/Server/AsciiPinyin.Web.Server.csproj Main/Server/
COPY Main/Client/AsciiPinyin.Web.Client.csproj Main/Client/
COPY Main/Shared/AsciiPinyin.Web.Shared.csproj Main/Shared/

COPY Test/Server/AsciiPinyin.Web.Server.Test.csproj Test/Server/
COPY Test/Client/AsciiPinyin.Web.Client.Test.csproj Test/Client/
COPY Test/Shared/AsciiPinyin.Web.Shared.Test.csproj Test/Shared/

RUN dotnet restore Main/Server/AsciiPinyin.Web.Server.csproj

RUN for proj in Test/**/*.csproj; do \
        dotnet restore $proj || exit 1; \
    done

COPY . .

RUN dotnet build --no-restore Main/Server/AsciiPinyin.Web.Server.csproj -c Release -o /app/build

RUN for proj in Test/**/*.csproj; do \
        dotnet test --no-restore --verbosity normal $proj || exit 1; \
    done


FROM build AS publish
RUN dotnet publish --no-restore Main/Server/AsciiPinyin.Web.Server.csproj -c Release -o /app/publish /p:UseAppHost=false /p:PublishTrimmed=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AsciiPinyin.Web.Server.dll"]

LABEL org.opencontainers.image.description "AsciiPinyin.Web - in development"
