# How testing works in this project

Every task file has a **Tests** section with three parts. This page explains what each part means, so the task files can stay short.

## The three kinds

| Kind | Folder in test project | What it touches | Speed | Attribute on the test class |
|---|---|---|---|---|
| **Unit** | `Unit/` | Plain C# only. No database, no HTTP, no browser. | milliseconds | `[Trait("Category", "Unit")]` |
| **Integration** | `Integration/` | A real Postgres in Docker, and sometimes the real app over HTTP (no browser). | seconds | `[Trait("Category", "Integration")]` |
| **End-to-end (E2E)** | `E2E/` | The real app running on a real port, driven by a real browser (Playwright, Chromium). | seconds each | `[Trait("Category", "E2E")]` |

Rule of thumb:

- If you can test it with `new Foo()` and an `Assert`, it's a unit test.
- If it needs the database or an HTTP request, it's an integration test.
- If it needs clicking, typing, or seeing a page, it's an E2E test.

## The two fixtures

Both are created in task M0.8 and grow a little in M1.

**`PostgresFixture`** — starts a throwaway `postgres:16` container (same major version as the dev database, see M0.5) once per test run, applies migrations and seeds cities/categories, and gives you `CreateContext()` for a fresh `AppDbContext`. Integration tests for the service layer use it: `[Collection("postgres")]`.

**`WebAppFixture`** — starts another throwaway Postgres, then starts the real app on Kestrel with a random port pointed at that database, then launches a headless Chromium. It gives you:

- `CreateClient()` — an `HttpClient` for integration tests that make HTTP requests without a browser
- `Browser` and `BaseUrl` — for E2E tests
- `Services` — to reach `AppDbContext` inside the running app

HTTP tests and browser tests share it: `[Collection("app")]`.

Because the app is in the Development environment, it migrates and seeds on startup (M1.6) and adds sample listings (M2.6). So E2E tests always have data to click on.

## Rules that keep tests from stepping on each other

- Tests share one database per fixture. Any test that inserts listings uses a unique title, for example `$"chair {Guid.NewGuid():N}"`, and only asserts on its own rows.
- Never assert exact total counts of listings. Assert "contains mine" or "does not contain mine".
- Counts of cities (6) and categories (22) are stable and safe to assert.
- E2E tests open a new `IPage` per test and close it in `finally`.

## Running them

```powershell
dotnet test --filter Category=Unit          # fast, run constantly
dotnet test --filter Category=Integration   # needs Docker running
dotnet test --filter Category=E2E           # needs Docker + Playwright browsers
dotnet test                                 # everything
```

One-time Playwright browser install, after the first `dotnet build`:

```powershell
powershell tests/CraigsClone.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
```

## When a task says "none"

Some tasks (installing a tool, writing CSS, writing the README) have nothing meaningful to test at one of the three levels. The task file then says **none** and gives the reason in a few words, rather than inventing a test that proves nothing.
