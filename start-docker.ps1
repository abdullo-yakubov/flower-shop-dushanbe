# Запуск всего проекта через Docker
# Требуется: Docker Desktop (должен быть запущен)

Write-Host "Flower Shop - Docker start" -ForegroundColor Cyan
Write-Host ""

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker не установлен. Установите Docker Desktop: https://www.docker.com/products/docker-desktop/"
    exit 1
}

try {
    docker info *> $null
} catch {
    Write-Error "Docker Desktop не запущен. Откройте Docker Desktop и повторите."
    exit 1
}

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

Write-Host "Сборка и запуск контейнеров..." -ForegroundColor Yellow
docker compose up --build -d

if ($LASTEXITCODE -ne 0) {
    Write-Error "Ошибка запуска docker compose"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Готово! Откройте в браузере:" -ForegroundColor Green
Write-Host "  Frontend (магазин):  http://localhost:5173"
Write-Host "  API + Swagger:       http://localhost:5000/swagger (только Development)"
Write-Host "  PostgreSQL:          localhost:5432 (user: flowershop, db: flowershop)"
Write-Host ""
Write-Host "Остановить: docker compose down"
