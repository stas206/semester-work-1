FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore MiniHttpServer.sln
RUN dotnet publish MiniHttpServer/MiniHttpServer.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish ./
RUN mkdir -p static/assets/images/uploads
EXPOSE 8888
ENTRYPOINT ["dotnet", "MiniHttpServer.dll"]
