## Configuration de la migration

# Installation de ef
dotnet tool install --global dotnet-ef

# commande de migration
dotnet ef migrations add InitialCreate --project BackendAdmin/BackendAdmin.Infrastructure --startup-project BackendAdmin/BackendAdmin.Api --output-dir Data/Migrations

# Remove migration
dotnet ef migrations remove --project BackendAdmin/BackendAdmin.Infrastructure --startup-project BackendAdmin/BackendAdmin.Api

docker compose exec BackendAdmin.api dotnet ef migrations remove  --project BackendAdmin/BackendAdmin.Infrastructure   --startup-project BackendAdmin/BackendAdmin.Api
