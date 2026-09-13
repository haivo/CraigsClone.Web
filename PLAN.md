# CraigsClone — Project Plan

A Craigslist-style classifieds site built as a learning / portfolio project.

## 1. Decisions (confirmed)

| Topic | Decision |
|---|---|
| Purpose | Learning / portfolio. Clarity over scale. |
| Stack | C# / ASP.NET Core **MVC** on .NET 10 (SDK 10.0.400 installed) |
| Database | **PostgreSQL 17** in Docker (Docker 29.7 + Compose v5.5 installed), accessed via EF Core + Npgsql |
| Auth | **None.** Anyone can post, edit, or delete any listing. |
| v1 scope | Listings (create / read / update / delete), categories, cities, keyword search with filters, pagination |
| Location | `C:\Data\sources\CraigsClone` |

Explicitly **out of scope for v1**: user accounts, image uploads, buyer–seller messaging, payments, moderation. See §8 for a stretch list.

## 2. What the site does

Mirrors the Craigslist flow:

1. **Home page** — pick a city, see category groups (for sale, housing, jobs, services, community, gigs) with their sub-categories.
2. **Browse page** — `/{city}/{category}` lists ads newest-first, with a search box and filters (keyword, min/max price, sort). Paginated.
3. **Listing page** — title, price, posted date, description, contact email, city / neighborhood.
4. **Post page** — form to create an ad. Redirects to the new listing.
5. **Edit / delete** — open to everyone (per decision above); delete asks for confirmation.

## 3. Data model

```
City       Id, Name, Slug (unique)
Category   Id, Name, Slug (unique), Group (enum: ForSale, Housing, Jobs, Services, Community, Gigs)
Listing    Id, Title (<=120), Description (text), Price (decimal?, null = "not listed"),
           CityId -> City, CategoryId -> Category, Neighborhood (<=80, optional),
           ContactEmail, CreatedAt (UTC), UpdatedAt (UTC)
```

- Indexes: `Listing(CityId, CategoryId, CreatedAt DESC)` for browse. A full-text index on title/description is a stretch item (§8).
- Cities and categories are **seeded** on startup (a handful of cities, ~20 categories across the 6 groups), never edited through the UI.
- Prices stored as `numeric(10,2)`.

## 4. Routes

| Method | Route | Controller.Action | Purpose |
|---|---|---|---|
| GET | `/` | Home.Index | City picker + category groups |
| GET | `/{citySlug}` | Home.City | Category groups for one city |
| GET | `/{citySlug}/{categorySlug}` | Listings.Index | Browse + search + filters + paging |
| GET | `/listing/{id}` | Listings.Details | Single ad |
| GET / POST | `/post` | Listings.Create | New ad form / submit |
| GET / POST | `/listing/{id}/edit` | Listings.Edit | Edit form / submit |
| POST | `/listing/{id}/delete` | Listings.Delete | Delete (anti-forgery token, confirm dialog) |
| GET | `/search?q=&city=&category=&min=&max=&sort=&page=` | Listings.Search | Cross-category search |

Query-string filters on browse/search: `q`, `min`, `max`, `sort` (`newest` | `price_asc` | `price_desc`), `page` (20 per page).

## 5. Project structure

```
CraigsClone/
├─ CraigsClone.slnx              # .NET 10 default solution format
├─ docker-compose.yml            # postgres:17, volume, port 5432
├─ .env.example                  # POSTGRES_* values
├─ src/CraigsClone.Web/
│  ├─ Program.cs                 # DI, EF, routing, seed on startup
│  ├─ appsettings.json           # ConnectionStrings:Default
│  ├─ Controllers/  Home, Listings
│  ├─ Data/         AppDbContext.cs, DbSeeder.cs, Migrations/
│  ├─ Models/       City, Category, Listing, CategoryGroup (enum)
│  ├─ ViewModels/   ListingIndexVm, ListingFormVm, SearchFilterVm, PagedResult<T>
│  ├─ Services/     IListingService + ListingService (query building, paging)
│  ├─ Views/        Home/, Listings/, Shared/_Layout, _Pager, _ListingRow
│  └─ wwwroot/      site.css (deliberately plain, Craigslist-ish), minimal JS
└─ tests/CraigsClone.Tests/      xUnit: ListingService filter/paging tests, validation tests
```

Keep query logic in `ListingService` so it can be unit-tested without controllers. Use view models for every form; never bind directly to entities.

## 6. Milestones

Each milestone ends with the app running and something visible in the browser.

**M0 — Scaffold (about 30 min)**
- `dotnet new sln`, `dotnet new mvc -f net10.0`, `dotnet new xunit`, add both to the solution.
- Add packages: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`.
- `dotnet tool install -g dotnet-ef` (not currently installed).
- `docker-compose.yml` with `postgres:17`; `docker compose up -d`.
- Done when: `dotnet run` shows the default MVC page and the DB container is healthy.

**M1 — Data layer**
- Entities, `AppDbContext`, Fluent config (slugs unique, price precision, indexes).
- `DbSeeder` for cities + categories, run at startup (idempotent).
- `dotnet ef migrations add Initial` then `dotnet ef database update`.
- Done when: tables exist in Postgres with seed rows.

**M2 — Browse (read-only)**
- Home page (city picker, category groups), city page, category browse page, listing details.
- Seed ~30 sample listings in dev only so pages are not empty.
- Done when: you can click from home to city to category to a listing.

**M3 — Post / edit / delete**
- `ListingFormVm` with DataAnnotations validation (title required, email format, price >= 0).
- Create, Edit, Delete actions with anti-forgery tokens; delete uses a confirm dialog.
- Done when: an ad posted via the form appears in its category and can be edited and removed.

**M4 — Search, filters, pagination**
- `ListingService.Query(filter)` builds an `IQueryable`: `ILIKE` on title/description, price range, sort, `Skip/Take`.
- `_Pager` partial; filters preserved across pages via query string.
- `/search` route for cross-category search within a city.
- Done when: search + filters + paging all work together on a category page.

**M5 — Polish**
- Layout, Craigslist-flavoured minimal CSS, friendly 404 / error pages.
- Server-side validation messages, `TempData` success banners.
- Unit tests for `ListingService` (filters, sort, paging edge cases) and validation.
- README with setup steps.
- Done when: tests pass and a stranger can clone, `docker compose up`, `dotnet run`, and use it.

## 7. Key commands

```powershell
# one-time
dotnet tool install -g dotnet-ef
docker compose up -d

# scaffold
dotnet new sln -n CraigsClone
dotnet new mvc -n CraigsClone.Web -o src/CraigsClone.Web -f net10.0
dotnet new xunit -n CraigsClone.Tests -o tests/CraigsClone.Tests -f net10.0
dotnet sln add src/CraigsClone.Web tests/CraigsClone.Tests
dotnet add src/CraigsClone.Web package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/CraigsClone.Web package Microsoft.EntityFrameworkCore.Design
dotnet add tests/CraigsClone.Tests reference src/CraigsClone.Web

# migrations (run from src/CraigsClone.Web)
dotnet ef migrations add Initial
dotnet ef database update

# run
dotnet run --project src/CraigsClone.Web
dotnet test
```

Connection string (dev): `Host=localhost;Port=5432;Database=craigsclone;Username=postgres;Password=postgres`, overridable via the `ConnectionStrings__Default` environment variable.

## 8. Stretch ideas (after v1)

Ordered by how naturally they extend v1:

1. **Secret edit link** — random token per listing so only the poster can edit/delete. Smallest step toward ownership without accounts.
2. **Listing expiry** — `ExpiresAt` (e.g. 30 days) and a filter that hides expired ads.
3. **Postgres full-text search** — `tsvector` column + GIN index replacing `ILIKE`.
4. **Image uploads** — up to 4 photos per listing, stored on disk under `wwwroot/uploads`.
5. **Flag for review** — "prohibited / spam" flags with a count; hide at a threshold.
6. **User accounts** — ASP.NET Core Identity, "my listings" page.
7. **Buyer–seller contact** — anonymized email relay via an SMTP provider.

## 9. Risks / notes

- **No auth means anyone can edit or delete anything.** Acceptable for a learning project on localhost; do not deploy publicly as-is.
- Keep search on `ILIKE` until data volume actually hurts; full-text search is an easy swap later because all queries live in one service.
- `dotnet-ef` must match the EF Core major version pulled in by Npgsql; install it after adding packages and check `dotnet ef --version`.
- No local `psql` is installed; inspect the DB with `docker compose exec db psql -U postgres craigsclone` or a GUI like DBeaver.
