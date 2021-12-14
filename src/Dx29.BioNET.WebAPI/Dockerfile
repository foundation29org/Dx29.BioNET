#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Dx29.BioNET.WebAPI/Dx29.BioNET.WebAPI.csproj", "Dx29.BioNET.WebAPI/"]
COPY ["Dx29.BioNET/Dx29.BioNET.csproj", "Dx29.BioNET/"]
COPY ["Dx29.BioEntity/Dx29.BioEntity.csproj", "Dx29.BioEntity/"]
COPY ["Dx29/Dx29.csproj", "Dx29/"]
RUN dotnet restore "Dx29.BioNET.WebAPI/Dx29.BioNET.WebAPI.csproj"
COPY . .
WORKDIR "/src/Dx29.BioNET.WebAPI"
RUN dotnet build "Dx29.BioNET.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dx29.BioNET.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dx29.BioNET.WebAPI.dll"]