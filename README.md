wymagania:
- dotnet ef (restart rider after install)

nuget:
- Microsoft.EntityFrameworkCore.Design (8.0.5)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.5)

zgodność z wersją .NET (8.0.5)

appsettings.json connection string:

  "ConnectionStrings": {
    "Default": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=apbd"
  }


cd do folderu projektu

dotnet ef dbcontext scaffold "Name=ConnectionStrings:Default" Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Models
