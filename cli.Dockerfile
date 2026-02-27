FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish "./api/LightNovelCore.Cli/LightNovelCore.Cli.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "LightNovelCore.Cli.dll"]
