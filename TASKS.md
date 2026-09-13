# CraigsClone — Task Checklist

One file per task in [tasks/](tasks/). Do them in order; each milestone ends with something you can see in the browser.

Read these two pages first:

- [tasks/00-before-you-start.md](tasks/00-before-you-start.md) — where the tasks differ from PLAN.md and why.
- [tasks/01-testing-approach.md](tasks/01-testing-approach.md) — what "unit", "integration", and "end-to-end" mean here. Every task has all three.

## M0 — Scaffold

- [ ] [M0.1 Git repo and README](tasks/M0.1-git-and-readme.md)
- [ ] [M0.2 Create solution and projects](tasks/M0.2-create-solution-and-projects.md)
- [ ] [M0.3 Add packages](tasks/M0.3-add-packages.md)
- [ ] [M0.4 Install dotnet-ef](tasks/M0.4-install-dotnet-ef.md)
- [ ] [M0.5 Postgres in Docker](tasks/M0.5-docker-compose-postgres.md)
- [ ] [M0.6 Connection string](tasks/M0.6-connection-string.md)
- [ ] [M0.7 Strip template, attribute routing](tasks/M0.7-strip-template.md)
- [ ] [M0.8 Test skeleton: fixtures and smoke tests](tasks/M0.8-test-skeleton.md)

**Finish line:** `dotnet run` shows a bare page; `docker compose ps` says healthy; `dotnet test` runs one green test in each category.

## M1 — Data layer

- [ ] [M1.1 Entity classes](tasks/M1.1-entities.md)
- [ ] [M1.2 AppDbContext](tasks/M1.2-dbcontext.md)
- [ ] [M1.3 Register DbContext](tasks/M1.3-register-dbcontext.md)
- [ ] [M1.4 Seed cities and categories](tasks/M1.4-seed-cities-and-categories.md)
- [ ] [M1.5 Initial migration](tasks/M1.5-initial-migration.md)
- [ ] [M1.6 Migrate and seed on startup](tasks/M1.6-migrate-and-seed-on-startup.md)

**Finish line:** tables exist in Postgres with 6 cities and 22 categories.

## M2 — Browse

- [ ] [M2.1 View models for browsing](tasks/M2.1-view-models-for-browsing.md)
- [ ] [M2.2 ListingService, read side](tasks/M2.2-listing-service-read.md)
- [ ] [M2.3 Home and city pages](tasks/M2.3-home-and-city-pages.md)
- [ ] [M2.4 Browse and details pages](tasks/M2.4-browse-and-details-pages.md)
- [ ] [M2.5 Layout](tasks/M2.5-layout.md)
- [ ] [M2.6 Sample listings for dev](tasks/M2.6-dev-sample-listings.md)

**Finish line:** click home → city → category → an ad.

## M3 — Post, edit, delete

- [ ] [M3.1 ListingFormVm with validation](tasks/M3.1-listing-form-view-model.md)
- [ ] [M3.2 ListingService, write side](tasks/M3.2-listing-service-write.md)
- [ ] [M3.3 Anti-forgery on every POST](tasks/M3.3-global-antiforgery.md)
- [ ] [M3.4 Post a new listing](tasks/M3.4-create-listing.md)
- [ ] [M3.5 Edit a listing](tasks/M3.5-edit-listing.md)
- [ ] [M3.6 Delete a listing](tasks/M3.6-delete-listing.md)
- [ ] [M3.7 "post" link remembers the city](tasks/M3.7-post-link-keeps-city.md)

**Finish line:** post an ad, see it in its category, edit it, delete it.

## M4 — Search, filters, paging

- [ ] [M4.1 SearchFilterVm and ListingSort](tasks/M4.1-search-filter-view-model.md)
- [ ] [M4.2 Query building](tasks/M4.2-query-building.md)
- [ ] [M4.3 Pager partial](tasks/M4.3-pager-partial.md)
- [ ] [M4.4 Search form partial](tasks/M4.4-search-form-partial.md)
- [ ] [M4.5 Wire filters into browse](tasks/M4.5-wire-filters-into-browse.md)
- [ ] [M4.6 Cross-category search page](tasks/M4.6-search-page.md)

**Finish line:** keyword + price + sort + page 2 all work together on one category page.

## M5 — Polish

- [ ] [M5.1 CSS](tasks/M5.1-css.md)
- [ ] [M5.2 Error pages](tasks/M5.2-error-pages.md)
- [ ] [M5.3 Success banner](tasks/M5.3-success-banner.md)
- [ ] [M5.4 Run the whole suite, fill gaps](tasks/M5.4-tests.md)
- [ ] [M5.5 README](tasks/M5.5-readme.md)
- [ ] [M5.6 Fresh-clone check](tasks/M5.6-fresh-clone-check.md)

**Finish line:** `dotnet test` passes and a stranger can run it from the README.
