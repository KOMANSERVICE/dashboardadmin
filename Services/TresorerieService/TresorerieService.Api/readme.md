## Configuration de la migration

# Installation de ef
dotnet tool install --global dotnet-ef

# commande de migration
dotnet ef migrations add InitialCreate --project Services/TresorerieService/TresorerieService.Infrastructure --startup-project Services/TresorerieService/TresorerieService.Api --output-dir Data/Migrations