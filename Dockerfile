# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy only the project file first (better cache)
COPY TimeTrackerApi.csproj ./
RUN dotnet restore TimeTrackerApi.csproj

# now copy the rest of the source
COPY . .

# publish the specific web project (avoid ambiguity with the .sln)
RUN dotnet publish TimeTrackerApi.csproj -c Release -o /app /p:UseAppHost=false

# ---- run stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Render forwards traffic to a port your app exposes; default is 10000
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

# If your DLL name changes, update this
ENTRYPOINT ["dotnet","TimeTrackerApi.dll"]
