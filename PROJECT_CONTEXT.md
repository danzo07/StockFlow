# PROJECT_CONTEXT : StockFlow

> Colle ce fichier au début de chaque nouvelle conversation avec Claude.
> Mets-le à jour à la fin de chaque session (2 minutes).
> Dernière mise à jour : 2026-10-27

## Objectif
API de gestion de stock et de commandes en .NET, pour renforcer mon profil .NET / DBA junior.
Projet personnel, présenté dans un CV .NET séparé. Mon PFE MERN reste inchangé (je ne le présente pas comme un projet .NET).

## Environnement
- Windows, projet dans C:\Users\arrafi\desktop\StockFlow
- .NET 10 (net10.0), EF Core 10, dotnet-ef 10.x
- Éditeur : Visual Studio 2022 (terminal PowerShell / Git Bash pour Git et dotnet)
- SQL Server 2022 dans Docker (conteneur `stockflow-db`, port 1433), base `StockFlowDb`
- Client SQL : SSMS (connexion `localhost,1433`, user `sa`, cocher "Trust server certificate")

## Stack
- C#, ASP.NET Core Web API (.NET 10)
- Entity Framework Core + SQL Server (Docker)
- Clean Architecture : Domain, Application, Infrastructure, API
- JWT + rôles (Admin, Magasinier)
- xUnit pour les tests
- Documentation d'API : .NET 10 fournit seulement `AddOpenApi()` (pas d'interface Swagger UI par défaut). À ajouter au jalon 2 : Scalar ou Swashbuckle.

## Structure de la solution
```
StockFlow/
  .env (local, ignoré par Git) / .env.example / docker-compose.yml
  PROJECT_CONTEXT.md
  src/
    StockFlow.Domain          (aucune dépendance) : Common/BaseEntity, Entities/(Category, Product, StockMovement), Enums/StockMovementType, Exceptions/DomainException
    StockFlow.Application     (dépend de Domain) : vide pour l'instant
    StockFlow.Infrastructure  (dépend de Application) : Persistence/(AppDbContext, AppDbContextFactory, Configurations/, Migrations/)
    StockFlow.API             (dépend de Application + Infrastructure) : Program.cs
  tests/
    StockFlow.Tests
```

## Règles de travail
- Git : une branche par fonctionnalité, Pull Request, merge dans main.
- Commits : Conventional Commits (feat, fix, chore, docs, test, perf, ci, build).
- Un commit = une idée, le projet compile à chaque commit (`dotnet build` avant de commiter).
- Jamais de secrets dans Git (user-secrets ou variables d'environnement). `git status` avant chaque commit.
- Je travaille un jalon par session, étape par étape, et je colle les résultats/erreurs à Claude.
- En cas d'erreur de build : copier le message complet, ou la sortie `--verbose` enregistrée dans un fichier (à ne pas commiter).

## Jalons
- [x] 1. Base : solution, entités, SQL Server, première migration (terminé)
- [ ] 2. API : CRUD produits et catégories, documentation d'API  <- PROCHAIN
- [ ] 3. Authentification JWT, 2 rôles
- [ ] 4. Logique métier : commandes achat/vente, stock en transaction, anti-stock négatif (RowVersion)
- [ ] 5. SQL avancé : 2 procédures, 2 vues, index avec mesures avant/après, backup/restore
- [ ] 6. (Bonus) Docker complet, CI GitHub Actions, déploiement, README final
- [ ] 7. (Bonus) Petit frontend Angular

### Détail du jalon 1 (terminé)
- [x] 1.1 Outils vérifiés
- [x] 1.2 Repo GitHub + PROJECT_CONTEXT.md
- [x] 1.3 Solution + références + build OK
- [x] 1.4 Entités Domain
- [x] 1.5 SQL Server en Docker
- [x] 1.6 AppDbContext + configurations Fluent API
- [x] 1.7 Migration InitialCreate appliquée (tables Categories, Products, StockMovements)
- [ ] 1.8 Tag v0.1.0 (à faire si pas encore fait : `git tag -a v0.1.0 -m "Foundation: solution, domain and database"` puis `git push origin v0.1.0`)

## Décisions prises
- Pas de frontend au début : l'API est testée via l'outil de documentation/Postman.
- Pas d'audit, dashboard, Testcontainers ni CD pour l'instant.
- Règles de stock dans l'entité Product : `QuantityInStock` a un `private set`, modifié uniquement par `IncreaseStock` / `DecreaseStock` (lève `DomainException` si stock insuffisant).
- `Product.RowVersion` (byte[]) configuré avec `IsRowVersion()`, pour la concurrence au jalon 4.
- Index unique sur `Product.Sku`, prix en `HasPrecision(18,2)`, `DeleteBehavior.Restrict` sur les relations.
- Chaîne de connexion de l'API : user-secrets du projet StockFlow.API (clé `ConnectionStrings:DefaultConnection`), jamais dans un fichier commité.
- Pour les migrations : `AppDbContextFactory` (IDesignTimeDbContextFactory) lit la variable d'environnement `STOCKFLOW_DB`. À définir dans le terminal avant chaque `dotnet ef` :
  - PowerShell : `$env:STOCKFLOW_DB = "Server=localhost,1433;Database=StockFlowDb;User Id=sa;Password=...;TrustServerCertificate=True"`
  - Commandes : `dotnet ef migrations add NomMigration -p src/StockFlow.Infrastructure -s src/StockFlow.API -o Persistence/Migrations` puis `dotnet ef database update -p src/StockFlow.Infrastructure -s src/StockFlow.API`
- CV : le projet est présenté comme "Projet personnel" (jamais comme PFE). Les chiffres (tables, ms avant/après index, nombre de lignes) ne sont ajoutés qu'après mesure réelle, au jalon 5. Les fonctionnalités non terminées sont marquées "(en cours)".

## Points à surveiller
- `dotnet ef` utilisant le démarrage de l'API échouait avec "Unable to resolve service for type DbContextOptions<AppDbContext>", alors que `Program.cs` contient bien `AddDbContext`. Cause exacte non identifiée, contournée avec `AppDbContextFactory`. Au jalon 2, vérifier que l'injection de `AppDbContext` fonctionne à l'exécution de l'API (sinon, investiguer).
- Warnings NuGet NU1902/NU1903 sur `System.Security.Cryptography.Pkcs` 9.0.11 (package indirect). Essayer `dotnet add src/StockFlow.Infrastructure package System.Security.Cryptography.Pkcs` pour forcer une version corrigée.

## État actuel
- Jalon terminé : 1
- Prochaine étape : jalon 2. Plan : (2.1) documentation d'API (Scalar ou Swashbuckle) + vérifier l'injection de AppDbContext, (2.2) DTO + validation (FluentValidation) + AutoMapper, (2.3) CRUD Categories, (2.4) CRUD Products avec pagination/recherche/filtre, (2.5) gestion globale des erreurs (404, 400, 409), (2.6) tag v0.2.0

## Notes / ce que j'ai appris
- `internal` vs `public` : une classe `internal` n'est visible que dans son projet (erreur CS0060 / CS0053).
- Le contexte EF est créé par `dotnet ef` soit via l'application hôte, soit via une `IDesignTimeDbContextFactory`.
- Les migrations EF créent du code C# qui est ensuite exécuté sur la base (`database update`).
- Les secrets ne vont jamais dans Git : `.env`, user-secrets, variables d'environnement.