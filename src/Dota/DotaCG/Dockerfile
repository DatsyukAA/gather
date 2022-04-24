FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DotaCG.csproj", "./"]
RUN dotnet restore "DotaCG.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DotaCG.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotaCG.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotaCG.dll"]
