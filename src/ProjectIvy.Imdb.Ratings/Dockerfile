FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY . .
RUN dotnet build "ProjectIvy.Imdb.Ratings.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjectIvy.Imdb.Ratings.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectIvy.Imdb.Ratings.dll"]