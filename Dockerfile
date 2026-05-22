# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["NuGet.config", "./"]
COPY ["src/ProjectTaskManager.API/ProjectTaskManager.API.csproj", "src/ProjectTaskManager.API/"]
COPY ["src/ProjectTaskManager.Application/ProjectTaskManager.Application.csproj", "src/ProjectTaskManager.Application/"]
COPY ["src/ProjectTaskManager.Infrastructure/ProjectTaskManager.Infrastructure.csproj", "src/ProjectTaskManager.Infrastructure/"]
COPY ["src/ProjectTaskManager.Domain/ProjectTaskManager.Domain.csproj", "src/ProjectTaskManager.Domain/"]

RUN dotnet restore "src/ProjectTaskManager.API/ProjectTaskManager.API.csproj"

COPY src/ src/
WORKDIR /src/src/ProjectTaskManager.API
RUN dotnet publish "ProjectTaskManager.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjectTaskManager.API.dll"]
