# Brings back the dev-machine Postgres and pgAdmin containers after a Docker or Windows restart.
#
# Safe to run any time:
#   - containers that exist but are stopped are started
#   - containers that are gone are recreated (data comes back if the volume survived)
#   - databases are created only if missing
#   - both containers get a restart policy so this is rarely needed again
#
# Usage:  .\tools\restore-db.ps1
#
# Not for fresh clones. Those use docker-compose.yml instead (see README).

$ErrorActionPreference = 'Stop'

function Ensure-Container {
    param([string]$Name, [string[]]$RunArgs)

    $state = docker inspect $Name --format '{{.State.Status}}' 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[$Name] missing, creating..."
        docker run -d --name $Name @RunArgs | Out-Null
    }
    elseif ($state -ne 'running') {
        Write-Host "[$Name] stopped ($state), starting..."
        docker start $Name | Out-Null
    }
    else {
        Write-Host "[$Name] already running"
    }
}

# 1. Postgres 16 on host port 5433 (matches appsettings.Development.json).
Ensure-Container 'pg-simple-agent' @(
    '-e', 'POSTGRES_USER=postgres',
    '-e', 'POSTGRES_PASSWORD=postgres',
    '-p', '5433:5432',
    '-v', 'pg-simple-agent-data:/var/lib/postgresql/data',
    '--restart', 'unless-stopped',
    'postgres:16'
)

# 2. pgAdmin on http://localhost:5050 (login admin@admin.com / admin).
Ensure-Container 'pgadmin' @(
    '-e', 'PGADMIN_DEFAULT_EMAIL=admin@admin.com',
    '-e', 'PGADMIN_DEFAULT_PASSWORD=admin',
    '-p', '5050:80',
    '-v', 'pgadmin-data:/var/lib/pgadmin',
    '--restart', 'unless-stopped',
    'dpage/pgadmin4'
)

# 3. Restart policy, in case the containers pre-date this script.
docker update --restart unless-stopped pg-simple-agent pgadmin | Out-Null

# 4. Wait for Postgres to accept connections.
Write-Host 'Waiting for Postgres...'
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    docker exec pg-simple-agent pg_isready -U postgres 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 2
}
if (-not $ready) { throw 'Postgres did not become ready within 60 seconds.' }

# 5. Databases, created only if missing.
foreach ($db in 'craigsclone', 'simple_agent') {
    $exists = docker exec pg-simple-agent psql -U postgres -tAc "select 1 from pg_database where datname = '$db'"
    if ($exists -eq '1') {
        Write-Host "[db] $db exists"
    }
    else {
        Write-Host "[db] creating $db"
        docker exec pg-simple-agent psql -U postgres -c "CREATE DATABASE $db" | Out-Null
    }
}

# 6. Verify.
$one = docker exec pg-simple-agent psql -U postgres -d craigsclone -tAc 'select 1'
if ($one -ne '1') { throw 'craigsclone did not answer select 1.' }

Write-Host ''
Write-Host 'Done.'
Write-Host '  Postgres : localhost:5433  (postgres / postgres)'
Write-Host '  pgAdmin  : http://localhost:5050  (admin@admin.com / admin)'
Write-Host ''
Write-Host 'pgAdmin takes about two minutes after start before the page responds.'
Write-Host 'If pgAdmin was recreated, add the server by hand:'
Write-Host '  Host host.docker.internal, Port 5433, User postgres, Password postgres, Maintenance DB craigsclone'
