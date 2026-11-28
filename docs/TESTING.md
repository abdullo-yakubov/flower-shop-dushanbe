# Testing

Run `dotnet test flower-shop-backend/tests/FlowerShop.Tests/FlowerShop.Tests.csproj`, `npm --prefix flower-shop test -- --run`, `npm --prefix flower-shop run lint`, and `npm --prefix flower-shop run build`. In the initial audit these yielded 44 backend tests, 56 frontend tests, passing lint and passing builds. A disposable PostgreSQL checkout, browser, performance, accessibility, payment, and real SMS test were not run.
