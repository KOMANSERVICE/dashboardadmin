## Configuration de la migration

# Installation de ef
dotnet tool install --global dotnet-ef

# commande de migration
dotnet ef migrations add InitialCreate --project Services/MagasinService/MagasinService.Infrastructure --startup-project Services/MagasinService/MagasinService.Api --output-dir Data/Migrations

