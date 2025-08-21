# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# ---- run stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
# Render expects your app to bind on 0.0.0.0 to the PORT it provides (default 10000)
# We'll set the port via env vars in Render, so don't hardcode it here.
EXPOSE 10000
#  If your DLL name differs, change it here:
ENTRYPOINT ["dotnet","TimeTrackerApi.dll"]
