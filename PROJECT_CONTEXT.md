# PROJECT_CONTEXT : StockFlow

> Colle ce fichier au début de chaque nouvelle conversation avec Claude.
> Mets-le à jour à la fin de chaque session (2 minutes).
> Dernière mise à jour : 2026-10-30

## Objectif
API de gestion de stock et de commandes en .NET, pour renforcer mon profil .NET / DBA junior.
Projet personnel, présenté dans un CV .NET séparé. Mon PFE MERN reste inchangé (je ne le présente pas comme un projet .NET).

## Environnement
- Windows, projet dans C:\Users\arrafi\Desktop\StockFlow (une seule copie du projet)
- .NET 10 (net10.0), EF Core 10.0.14, dotnet-ef 10.x
- Éditeur : Visual Studio 2022 (terminal PowerShell / Git Bash pour Git et dotnet)
- SQL Server 2022 dans Docker (conteneur `stockflow-db`, port 1433), base `StockFlowDb`
- Client SQL : SSMS (connexion `localhost,1433`, user `sa`, cocher "Trust server certificate"). Dans SSMS toujours faire `USE StockFlowDb;`
- Documentation/test de l'API : Scalar (`/scalar/v1`), basé sur OpenAPI natif .NET 10
- Lancer l'API : `dotnet run --project src/StockFlow.API --launch-profile https` (depuis le terminal, plus fiable que le débogueur de Visual Studio)

## Stack
- C#, ASP.NET Core Web API (.NET 10), contrôleurs
- Entity Framework Core + SQL Server (Docker)
- Clean Architecture : Domain, Application, Infrastructure, API
- Validation par DataAnnotations sur les DTO (pas de FluentValidation ni AutoMapper pour l'instant, mappage fait à la main)
- Gestion d'erreurs globale (IExceptionHandler + ProblemDetails)
- À venir : JWT + rôles (Admin, Magasinier), xUnit

## Structure de la solution
```
StockFlow/
  .env (local, ignoré par Git) / .env.example / docker-compose.yml
  PROJECT_CONTEXT.md / StockFlow.slnx
  src/
    StockFlow.Domain          (aucune dépendance)
      Common/BaseEntity, Entities/(Category, Product, StockMovement), Enums/StockMovementType,
      Exceptions/(DomainException, NotFoundException)
    StockFlow.Application     (dépend de Domain + EF Core 10.0.14)
      Common/(IAppDbContext, PagedResult), Categories/(DTOs, ICategoryService, CategoryService),
      Products/(DTOs + ProductQuery, IProductService, ProductService)
    StockFlow.Infrastructure  (dépend de Application)
      Persistence/(AppDbContext, AppDbContextFactory, Configurations/, Migrations/)
    StockFlow.API             (dépend de Application + Infrastructure)
      Controllers/(CategoriesController, ProductsController), Middleware/GlobalExceptionHandler, Program.cs
  tests/
    StockFlow.Tests
```

## Règles de travail
- Git : une branche par fonctionnalité, Pull Request, merge dans main, puis `git checkout main` + `git pull`.
- Commits : Conventional Commits (feat, fix, chore, docs, test, perf, ci, build). Un commit = une idée.
- `dotnet build` dans le terminal avant chaque commit ; `git status` avant chaque `git add`.
- Jamais de secrets dans Git (user-secrets, `.env`, variable d'environnement `STOCKFLOW_DB` pour les migrations).
- Je travaille un jalon par session, étape par étape, et je colle les résultats/erreurs à Claude.

## Jalons
- [x] 1. Base : solution, entités, SQL Server, première migration (terminé)
- [x] 2. API : CRUD catégories et produits, pagination/recherche/filtre/tri, gestion globale des erreurs, Scalar (terminé)
- [ ] 3. Authentification JWT, 2 rôles  <- PROCHAIN
- [ ] 4. Logique métier : commandes achat/vente, stock en transaction, anti-stock négatif (RowVersion)
- [ ] 5. SQL avancé : 2 procédures, 2 vues, index avec mesures avant/après, backup/restore
- [ ] 6. (Bonus) Docker complet, CI GitHub Actions, déploiement, README final
- [ ] 7. (Bonus) Petit frontend Angular

### Détail du jalon 2 (terminé)
- [x] Scalar + endpoint `/health/db` (injection de AppDbContext vérifiée)
- [x] CRUD Categories (`/api/v1/Categories`)
- [x] Gestion globale des erreurs : NotFoundException -> 404, DomainException -> 409, validation -> 400, autre -> 500
- [x] CRUD Products (`/api/v1/Products`) avec `page`, `pageSize` (1-100), `search`, `categoryId`, `lowStockOnly`, `sortBy` (name|price|stock), `descending`
- [ ] Tag v0.2.0 (à faire après le merge de feature/crud-products : `git tag -a v0.2.0 -m "API: categories and products CRUD"` puis `git push origin v0.2.0`)
- À vérifier : tag v0.1.0 posé aussi

## Décisions prises
- Pas de frontend au début : l'API est testée via Scalar/Postman.
- Pas d'audit, dashboard, Testcontainers ni CD pour l'instant.
- Règles de stock dans l'entité Product : `QuantityInStock` a un `private set`, modifié uniquement par `IncreaseStock` / `DecreaseStock` (lève `DomainException` si stock insuffisant). Un produit est créé avec un stock de 0 ; le stock ne change que par les réceptions d'achat et les ventes (jalon 4).
- `Product.RowVersion` (byte[]) configuré avec `IsRowVersion()`, pour la concurrence au jalon 4.
- Index unique sur `Product.Sku` (SKU enregistré en majuscules), prix en `HasPrecision(18,2)`, `DeleteBehavior.Restrict` sur les relations.
- Les services lèvent des exceptions (NotFoundException / DomainException) ; les controllers n'ont pas de try/catch.
- Les lectures utilisent `AsNoTracking()` + `Select` vers DTO (pas d'entités exposées).
- Chaîne de connexion de l'API : user-secrets du projet StockFlow.API (clé `ConnectionStrings:DefaultConnection`).
- Migrations : `AppDbContextFactory` lit la variable d'environnement `STOCKFLOW_DB`. À définir dans le terminal avant chaque `dotnet ef` :
  - PowerShell : `$env:STOCKFLOW_DB = "Server=localhost,1433;Database=StockFlowDb;User Id=sa;Password=...;TrustServerCertificate=True"`
  - `dotnet ef migrations add Nom -p src/StockFlow.Infrastructure -s src/StockFlow.API -o Persistence/Migrations` puis `dotnet ef database update -p src/StockFlow.Infrastructure -s src/StockFlow.API`
- CV : le projet est présenté comme "Projet personnel" (jamais comme PFE). Les chiffres (tables, ms avant/après index, nombre de lignes) ne sont ajoutés qu'après mesure réelle, au jalon 5. Les fonctionnalités non terminées sont marquées "(en cours)".

## Pièges rencontrés (à relire en cas de bug étrange)
- Fichier créé par Visual Studio = `internal class` par défaut : toujours écrire `public` (erreurs CS0060 / CS0051 / CS0122).
- Visual Studio peut écraser un fichier modifié depuis un autre éditeur ou le terminal (ex. NotFoundException.cs, .csproj). Fermer l'onglet sans enregistrer, recharger, ou modifier le fichier avec le Bloc-notes puis recharger.
- Visual Studio peut supprimer `using Microsoft.EntityFrameworkCore;` à l'enregistrement quand il ne voit pas le package (Code Cleanup on Save). Désactiver : Outils > Options > Éditeur de texte > Code Cleanup > "Run Code Cleanup profile on Save".
- Version du package EF Core : toujours exacte (10.0.14), jamais flottante (10.0.*), et identique entre Application et Infrastructure.
- En cas d'erreurs de Visual Studio que le terminal n'a pas : fermer Visual Studio, supprimer `bin`, `obj` et `.vs`, `dotnet restore`, rouvrir.
- Le débogueur de Visual Studio s'arrête sur les exceptions gérées par le gestionnaire global : cliquer Continuer, ou lancer sans débogueur (Ctrl + F5 / terminal).
- Docker Desktop : si "WSL distro exited", quitter Docker, `wsl --shutdown`, `wsl --update`, redémarrer le PC. Ne jamais cliquer sur "Reset to factory defaults" / "Purge data" (supprime la base).
- Le profil `https` de launchSettings.json contient `"launchBrowser": true` et `"launchUrl": "scalar/v1"`.

## Points à surveiller
- `dotnet ef` utilisant le démarrage de l'API échouait avec "Unable to resolve service for type DbContextOptions<AppDbContext>", alors que `Program.cs` contient `AddDbContext`. Contourné avec `AppDbContextFactory`. L'injection fonctionne à l'exécution de l'API (vérifiée avec `/health/db` et les CRUD).

## État actuel
- Jalons terminés : 1 et 2 (branche feature/crud-products à merger si pas déjà fait)
- Prochaine étape : jalon 3 (authentification). Plan : (3.1) entité User + enum UserRole + migration, (3.2) hachage BCrypt, (3.3) génération JWT (clé dans user-secrets), (3.4) endpoints login / register (register réservé Admin), (3.5) JwtBearer + `[Authorize]` + policies par rôle (Admin, Magasinier), (3.6) bouton Authorize dans Scalar, seed de l'admin en développement, (3.7) tag v0.3.0

## Notes / ce que j'ai appris
- `internal` vs `public` : une classe `internal` n'est visible que dans son projet.
- Le contexte EF est créé par `dotnet ef` soit via l'application hôte, soit via une `IDesignTimeDbContextFactory`.
- Les migrations EF créent du code C# qui est ensuite exécuté sur la base (`database update`).
- Les secrets ne vont jamais dans Git : `.env`, user-secrets, variables d'environnement.
- Pagination fiable : toujours un ordre stable (`ThenBy(p => p.Id)`) avant `Skip`/`Take`.
- Un DTO ne doit jamais exposer directement l'entité ; la projection `Select` ne charge que les colonnes utiles.