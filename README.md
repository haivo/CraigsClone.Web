# CraigsClone

A Craigslist-style classifieds site: pick a city, browse categories, post an ad, search. Built as a learning project with ASP.NET Core MVC on .NET 10 and PostgreSQL. The full design is in [PLAN.md](PLAN.md); the work was done one task at a time, and each task is written up in [tasks/](tasks/).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/), running

Nothing else. No Node, no global tools for running it.

## Run it

```powershell
docker compose up -d
dotnet run --project src/CraigsClone.Web
```

Then open the URL printed in the console (usually http://localhost:5000). On first start the app creates the tables and seeds six cities, 22 categories, and about 35 sample ads. Nothing to run by hand.

`docker compose up` starts Postgres 16 on port **5433** (not the default 5432, so it won't collide with a Postgres you already have). The connection string in `appsettings.Development.json` points there.

## Run the tests

```powershell
.\test.ps1 unit          # plain C#, seconds, no Docker needed
.\test.ps1 integration   # real Postgres in Docker, about half a minute
.\test.ps1 e2e           # real app + Postgres + headless Chromium, about a minute
.\test.ps1               # everything, about two minutes
```

Integration and end-to-end tests start their own throwaway Postgres containers via Testcontainers, so they never touch the database you run the site against. One-time setup for the browser tests, after the first build:

```powershell
powershell tests/CraigsClone.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
```

How the three kinds of test are split, and the rules they follow, are in [tasks/01-testing-approach.md](tasks/01-testing-approach.md).

## Look at the database

There's no `psql` needed on your machine; go through the container:

```powershell
docker compose exec db psql -U postgres craigsclone
```

## Configuration

The only setting is the connection string, `ConnectionStrings:Default`. Override it without editing files by setting the environment variable `ConnectionStrings__Default` (two underscores).

**Already have a Postgres?** Skip `docker compose`. Create a database on it (`CREATE DATABASE craigsclone`) and point the connection string at it. That is how the original dev machine runs: an existing container on port 5433, no compose.

To change the compose port or credentials, copy `.env.example` to `.env` and edit it.

## Warning

There are **no user accounts**. Anyone who can reach the site can edit or delete any ad. That's deliberate for a learning project on localhost. Do not deploy this as-is.

## What's next

Ideas that extend v1 naturally, in order:

1. **Secret edit link** — a random token per ad so only the poster can edit or delete it.
2. **Listing expiry** — an `ExpiresAt` date, hide expired ads.
3. **Postgres full-text search** — a `tsvector` column and GIN index instead of `ILIKE`.
4. **Image uploads** — up to four photos per ad.
5. **Flag for review** — spam and prohibited-item flags, hide above a threshold.
6. **User accounts** — ASP.NET Core Identity and a "my listings" page.
7. **Buyer–seller contact** — anonymised email relay.
