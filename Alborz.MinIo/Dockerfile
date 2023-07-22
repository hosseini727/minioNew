#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

ENV HOSTNAMERABBITMQ 172.17.0.2
ENV PORTNAMERABBITMQ 5672
ENV ENDPOINTMINIO 172.17.0.1:9000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Alborz.MinIo/Alborz.MinIo.csproj", "Alborz.MinIo/"]
COPY ["Cleint/Cleint.csproj", "Cleint/"]
COPY ["FileUploader/Services.csproj", "FileUploader/"]
RUN dotnet restore "Alborz.MinIo/Alborz.MinIo.csproj"
COPY . .
WORKDIR "/src/Alborz.MinIo"
RUN dotnet build "Alborz.MinIo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Alborz.MinIo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alborz.MinIo.dll"]