# Локальный запуск без Docker (PostgreSQL должен работать на localhost:5432)

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Flower Shop - local start" -ForegroundColor Cyan
Write-Host ""
Write-Host "Запуск API на http://localhost:5000 ..."
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\flower-shop-backend\src\FlowerShop.API'; dotnet run"

Start-Sleep -Seconds 3

Write-Host "Запуск Frontend на http://localhost:5173 ..."
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\flower-shop'; npm run dev -- --host localhost --port 5173"

Write-Host ""
Write-Host "Готово! Откройте:" -ForegroundColor Green
Write-Host "  Frontend: http://localhost:5173"
Write-Host "  API:      http://localhost:5000"
Write-Host ""
