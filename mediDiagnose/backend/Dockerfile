ARG SERVICE_PROJECT

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG SERVICE_PROJECT
WORKDIR /src
COPY . .
RUN dotnet publish "src/${SERVICE_PROJECT}/${SERVICE_PROJECT}.csproj" --configuration Release --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet"]
