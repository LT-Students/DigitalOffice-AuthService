#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 9818
EXPOSE 9817

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Services/AuthenticationService/AuthenticationService.csproj", "Services/AuthenticationService/"]
COPY ["Shared/Kernel/Kernel.csproj", "Shared/Kernel/"]
RUN dotnet restore "Services/AuthenticationService/AuthenticationService.csproj"
COPY . .
WORKDIR "/src/Services/AuthenticationService"
RUN dotnet build "AuthenticationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthenticationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LT.DigitalOffice.AuthenticationService.dll"]