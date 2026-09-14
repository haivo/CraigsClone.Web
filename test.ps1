# Run the test suite, or one category of it.
#   .\test.ps1              everything
#   .\test.ps1 unit         plain C#, no Docker needed, a few seconds
#   .\test.ps1 integration  real Postgres in Docker (Testcontainers)
#   .\test.ps1 e2e          real app + Postgres + headless Chromium (Playwright)
param(
    [ValidateSet("unit", "integration", "e2e", "all")]
    [string] $what = "all"
)

$filter = switch ($what) {
    "unit"        { "Category=Unit" }
    "integration" { "Category=Integration" }
    "e2e"         { "Category=E2E" }
    default       { $null }
}

# A lingering build server sometimes holds obj/ files on Windows; shutting it down avoids MSB3021/MSB3027.
dotnet build-server shutdown *> $null

if ($filter) {
    dotnet test --nologo --filter $filter
} else {
    dotnet test --nologo
}
exit $LASTEXITCODE
