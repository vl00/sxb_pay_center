#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["iSchool.FinanceCenter.Api/iSchool.FinanceCenter.Api.csproj", "iSchool.FinanceCenter.Api/"]
RUN dotnet restore "iSchool.FinanceCenter.Api/iSchool.FinanceCenter.Api.csproj"
COPY . .
WORKDIR "/src/iSchool.FinanceCenter.Api"
RUN dotnet build "iSchool.FinanceCenter.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "iSchool.FinanceCenter.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "iSchool.FinanceCenter.Api.dll"]