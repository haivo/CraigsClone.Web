# Before you start

A few things in PLAN.md would cause trouble later. Each one is already folded into a task, so you don't need to fix anything now. This page just explains *why* some tasks look different from the plan.

**1. The default MVC routing would fight with our URLs.**
The project template adds a rule that turns `/austin/furniture` into "controller Austin, action Furniture". Our own rule says it means "city austin, category furniture". Two rules for one URL is a bug waiting to happen. So in task M0.7 we remove the template rule and only use routes we write ourselves.

**2. Search tests need a real Postgres.**
Our keyword search uses `ILIKE`, which only exists in Postgres. The fake "in-memory" database that people often use for tests doesn't support it. So the tests (task M5.4) start a small throwaway Postgres in Docker using a library called Testcontainers.

**3. The database sets itself up when the app starts.**
Instead of you running `dotnet ef database update` by hand, the app applies migrations and seed data on startup in Development (task M1.6). Then "clone, docker compose up, dotnet run" is all anyone needs.

**4. Dates must be UTC.**
Postgres via Npgsql refuses local-time `DateTime` values. Always use `DateTime.UtcNow`, and only set dates inside the service (task M3.2).

**5. Anti-forgery is turned on globally.**
One line in Program.cs (task M3.3) protects every POST form, so you can't forget it on one action.

**6. Price has an upper limit.**
The database column allows at most 99,999,999.99. The form checks that too (task M3.1) so the database never throws.

**7. Sample listings are separate from real seed data.**
Cities and categories are always seeded (task M1.4). Fake sample ads are only seeded in Development (task M2.6).

**8. Some words are reserved in URLs.**
Because `/{citySlug}` matches almost anything, no city may be named `post`, `search`, `listing`, or `error`. The seed list in task M1.4 avoids these.

**9. Strip the template extras.**
The template ships Bootstrap, jQuery, and a Privacy page. We don't want them, so task M0.7 deletes them early.
