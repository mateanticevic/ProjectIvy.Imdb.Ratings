FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal  AS build
WORKDIR /src
COPY . .
WORKDIR "/src/src/ProjectIvy.Imdb.Ratings"
RUN dotnet build "ProjectIvy.Imdb.Ratings.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjectIvy.Imdb.Ratings.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectIvy.Imdb.Ratings.dll"]