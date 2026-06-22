# SmartComply Troubleshooting Guide

## Common Issues and Solutions

### 1. Build Errors

#### Error: "The type or namespace name could not be found"
**Solution:**
```bash
dotnet clean
dotnet restore
dotnet build
```

#### Error: "RZ1031: Tag helper directive"
This usually means Razor views have syntax errors. Check:
- Missing `@model` declarations
- Incorrect property names in views
- Missing using statements

### 2. Database Issues

#### Error: "Cannot open database"
**Solution:**
1. Ensure SQL Server LocalDB is installed
2. Check connection string in `appsettings.json`
3. Try creating the database manually:
```bash
dotnet ef database update
```

#### Error: "A network-related or instance-specific error"
**Solution:**
1. Start SQL Server LocalDB:
```bash
sqllocaldb start mssqllocaldb
```
2. Check if LocalDB is running:
```bash
sqllocaldb info
```

#### Error: "Login failed for user"
**Solution:**
- Use Windows Authentication (Trusted_Connection=true)
- Or create SQL Server login and update connection string

### 3. Migration Issues

#### Error: "Unable to create an object of type 'ApplicationDbContext'"
**Solution:**
1. Check that the connection string is in `appsettings.json`
2. Ensure the project builds successfully
3. Try:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Error: "The migration has already been applied"
**Solution:**
```bash
# Check migration status
dotnet ef migrations list

# Remove last migration if needed
dotnet ef migrations remove

# Or force update
dotnet ef database update --force
```

### 4. Runtime Issues

#### Error: "Process cannot access the file because it is being used by another process"
**Solution:**
1. Find the running process:
```bash
tasklist | findstr SmartComply
```
2. Kill the process:
```bash
taskkill /PID <ProcessID> /F
```

#### Error: "Port already in use"
**Solution:**
1. Use different ports:
```bash
dotnet run --urls "https://localhost:5002;http://localhost:5003"
```
2. Or kill the process using the port:
```bash
netstat -ano | findstr :5001
taskkill /PID <ProcessID> /F
```

### 5. Authentication Issues

#### Error: "Unable to login with default credentials"
**Solution:**
1. Ensure the database seeder ran successfully
2. Check the `AspNetUsers` table for seeded users
3. Manually run the seeder:
```csharp
// In Program.cs, ensure the seeder is called
await AuditDataSeeder.SeedAsync(app.Services);
```

#### Error: "Access Denied"
**Solution:**
1. Check user roles in `AspNetUserRoles` table
2. Verify `[Authorize]` attributes on controllers
3. Ensure user is assigned correct roles

### 6. Performance Issues

#### Slow database queries
**Solution:**
1. Check Entity Framework query logs
2. Add indexes to frequently queried columns
3. Use async/await for database operations
4. Implement pagination for large datasets

#### High memory usage
**Solution:**
1. Dispose DbContext properly
2. Use `AsNoTracking()` for read-only queries
3. Implement proper caching strategies

### 7. Development Environment Issues

#### Error: "dotnet command not found"
**Solution:**
1. Install .NET 9.0 SDK
2. Restart command prompt/terminal
3. Verify installation:
```bash
dotnet --version
```

#### Error: "Entity Framework tools not found"
**Solution:**
```bash
dotnet tool install --global dotnet-ef
```

### 8. View/Razor Issues

#### Error: "The name 'model' does not exist in the current context"
**Solution:**
1. Add `@model YourModelType` at the top of the view
2. Check that the model type is correct
3. Ensure the controller passes the correct model

#### Error: "Tag helper not found"
**Solution:**
1. Check `_ViewImports.cshtml` for tag helper directives
2. Ensure proper using statements
3. Verify tag helper assembly references

## Logging and Debugging

### Enable detailed logging
In `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Debug Entity Framework queries
In `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging(); // Only in development
    options.LogTo(Console.WriteLine);
});
```

## Getting Help

1. **Check the logs** in the console output
2. **Search the error message** online
3. **Check Entity Framework documentation** for database issues
4. **Review ASP.NET Core documentation** for framework issues
5. **Create an issue** in the project repository with:
   - Error message
   - Steps to reproduce
   - Environment details (.NET version, OS, etc.)

## Useful Commands

```bash
# Clean and rebuild
dotnet clean && dotnet restore && dotnet build

# Database operations
dotnet ef migrations list
dotnet ef database drop
dotnet ef database update
dotnet ef migrations add <name>

# Run with specific environment
dotnet run --environment Development
dotnet run --environment Production

# Check running processes
tasklist | findstr dotnet
netstat -ano | findstr :5001

# Package management
dotnet add package <PackageName>
dotnet remove package <PackageName>
dotnet list package
```
