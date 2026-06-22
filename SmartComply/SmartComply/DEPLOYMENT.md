# SmartComply Deployment Guide

## Development Deployment

### Prerequisites
- .NET 9.0 SDK
- SQL Server LocalDB or SQL Server Express
- Visual Studio 2022 or VS Code

### Quick Start
1. Run the setup script:
   ```bash
   # Windows
   setup.bat
   
   # Cross-platform
   pwsh setup.ps1
   ```

2. Or manually:
   ```bash
   dotnet restore
   dotnet build
   dotnet ef database update
   dotnet run
   ```

## Production Deployment

### 1. Server Requirements
- Windows Server 2019+ or Linux (Ubuntu 20.04+)
- .NET 9.0 Runtime
- SQL Server 2019+ or Azure SQL Database
- IIS (Windows) or Nginx/Apache (Linux)

### 2. Database Setup

#### SQL Server
```sql
-- Create database
CREATE DATABASE SmartComply;
GO

-- Create login and user
CREATE LOGIN SmartComplyUser WITH PASSWORD = 'YourSecurePassword123!';
GO

USE SmartComply;
GO

CREATE USER SmartComplyUser FOR LOGIN SmartComplyUser;
GO

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER SmartComplyUser;
ALTER ROLE db_datawriter ADD MEMBER SmartComplyUser;
ALTER ROLE db_ddladmin ADD MEMBER SmartComplyUser;
GO
```

#### Azure SQL Database
```bash
# Set connection string in production
"Server=tcp:yourserver.database.windows.net,1433;Database=SmartComply;User ID=username;Password=password;Encrypt=True;Connection Timeout=30;"
```

### 3. Application Configuration

#### Production appsettings.json
```json
{
  "ConnectionStrings": {
    "Database": "Server=your-sql-server;Database=SmartComply;User Id=SmartComplyUser;Password=YourSecurePassword123!;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com",
  "Environment": "Production"
}
```

### 4. Build for Production
```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Or with specific runtime
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### 5. Database Migration in Production
```bash
# Update database schema
dotnet ef database update --connection "YourProductionConnectionString"

# Or use SQL scripts
dotnet ef migrations script > migration.sql
```

### 6. IIS Deployment (Windows)

#### Install ASP.NET Core Module
1. Download and install [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Restart IIS: `iisreset`

#### Create IIS Site
```xml
<!-- web.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\SmartComply.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="InProcess" />
    </system.webServer>
  </location>
</configuration>
```

### 7. Linux Deployment (Ubuntu)

#### Install .NET Runtime
```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0
```

#### Create Systemd Service
```ini
# /etc/systemd/system/smartcomply.service
[Unit]
Description=SmartComply ASP.NET Core Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/smartcomply/SmartComply.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=smartcomply
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

#### Start Service
```bash
sudo systemctl enable smartcomply.service
sudo systemctl start smartcomply.service
sudo systemctl status smartcomply.service
```

#### Configure Nginx
```nginx
# /etc/nginx/sites-available/smartcomply
server {
    listen        80;
    server_name   yourdomain.com;
    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/smartcomply /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## Docker Deployment

### 1. Create Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SmartComply.csproj", "."]
RUN dotnet restore "SmartComply.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "SmartComply.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartComply.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartComply.dll"]
```

### 2. Create docker-compose.yml
```yaml
version: '3.8'

services:
  smartcomply:
    build: .
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Database=Server=sqlserver;Database=SmartComply;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - sqlserver
    networks:
      - smartcomply-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourPassword123!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - smartcomply-network

volumes:
  sqlserver_data:

networks:
  smartcomply-network:
    driver: bridge
```

### 3. Deploy with Docker
```bash
# Build and run
docker-compose up -d

# Check logs
docker-compose logs smartcomply

# Update database
docker-compose exec smartcomply dotnet ef database update
```

## Azure App Service Deployment

### 1. Prepare for Azure
```bash
# Install Azure CLI
az login

# Create resource group
az group create --name SmartComplyRG --location "East US"

# Create App Service plan
az appservice plan create --name SmartComplyPlan --resource-group SmartComplyRG --sku B1

# Create web app
az webapp create --resource-group SmartComplyRG --plan SmartComplyPlan --name smartcomply-app --runtime "DOTNETCORE|9.0"
```

### 2. Configure Connection String
```bash
az webapp config connection-string set --resource-group SmartComplyRG --name smartcomply-app --connection-string-type SQLAzure --settings Database="Server=tcp:yourserver.database.windows.net,1433;Database=SmartComply;User ID=username;Password=password;Encrypt=True;"
```

### 3. Deploy
```bash
# Publish and deploy
dotnet publish -c Release
cd bin/Release/net9.0/publish
zip -r smartcomply.zip .
az webapp deployment source config-zip --resource-group SmartComplyRG --name smartcomply-app --src smartcomply.zip
```

## Security Considerations

### 1. Environment Variables
```bash
# Set sensitive configuration via environment variables
export ConnectionStrings__Database="YourConnectionString"
export JwtSettings__SecretKey="YourSecretKey"
```

### 2. HTTPS Configuration
```csharp
// In Program.cs
app.UseHttpsRedirection();
app.UseHsts();
```

### 3. Security Headers
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

## Monitoring and Logging

### 1. Application Insights (Azure)
```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### 2. Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

### 3. Structured Logging
```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.AddApplicationInsights();
});
```

## Backup and Recovery

### 1. Database Backup
```sql
-- Full backup
BACKUP DATABASE SmartComply 
TO DISK = 'C:\Backups\SmartComply_Full.bak'

-- Automated backups
EXEC sp_add_job @job_name = 'SmartComply_Backup'
```

### 2. Application Backup
```bash
# Backup published application
tar -czf smartcomply-backup-$(date +%Y%m%d).tar.gz /var/www/smartcomply/
```

## Performance Optimization

### 1. Database Optimization
- Add indexes on frequently queried columns
- Use connection pooling
- Implement read replicas for reporting

### 2. Application Optimization
- Enable response compression
- Use output caching
- Implement CDN for static files

### 3. Monitoring
- Set up performance counters
- Monitor memory usage
- Track response times

This deployment guide covers the most common deployment scenarios. Choose the approach that best fits your infrastructure and requirements.
