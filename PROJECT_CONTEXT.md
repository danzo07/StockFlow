# PROJECT_CONTEXT : StockFlow

> Colle ce fichier au début de chaque nouvelle conversation avec Claude.
> Mets-le à jour à la fin de chaque session (2 minutes).

## Objectif
API de gestion de stock et de commandes en .NET, pour renforcer mon profil .NET / DBA junior.
Projet personnel, présenté dans un CV .NET séparé. Mon PFE MERN reste inchangé.

## Stack
- C#, ASP.NET Core Web API (.NET LTS récent)
- Entity Framework Core + SQL Server (Docker)
- Clean Architecture : Domain, Application, Infrastructure, API
- JWT + rôles (Admin, Magasinier)
- Swagger pour tester, xUnit pour les tests

## Règles de travail
- Git : une branche par fonctionnalité, Pull Request, merge dans main.
- Commits : Conventional Commits (feat, fix, chore, docs, test, perf, ci, build).
- Un commit = une idée, le projet compile à chaque commit.
- Jamais de secrets dans Git (user-secrets ou variables d'environnement).
- Je travaille un jalon par session, étape par étape, et je colle les résultats/erreurs à Claude.

## Jalons
- [ ] 1. Base : solution, entités, SQL Server, première migration
- [ ] 2. API : CRUD produits et catégories, Swagger
- [ ] 3. Authentification JWT, 2 rôles
- [ ] 4. Logique métier : commandes achat/vente, stock en transaction, anti-stock négatif
- [ ] 5. SQL avancé : 2 procédures, 2 vues, index avec mesures avant/après, backup/restore
- [ ] 6. (Bonus) Docker complet, CI GitHub Actions, déploiement, README final
- [ ] 7. (Bonus) Petit frontend Angular

## Décisions prises
- Pas de frontend au début : Swagger suffit.
- Pas d'audit, dashboard, Testcontainers ni CD pour l'instant.

## État actuel
- Jalon en cours : 1
- Dernière étape terminée : aucune
- Prochaine étape : créer le repo GitHub et la solution

## Problèmes en cours
(aucun)

## Notes / ce que j'ai appris
(à remplir : utile pour les entretiens)
