# Setup

Use the root README. Docker requires `.env` based on `.env.example`; set unique database and JWT secrets. Local development needs Node.js 22, .NET SDK 10, and PostgreSQL. Run `npm --prefix flower-shop ci`, `npm --prefix flower-shop run dev`, and `dotnet run --project flower-shop-backend/src/FlowerShop.API` after configuring the local connection and JWT secret.

The frontend can run with mocks when `VITE_API_URL` is empty; this does not verify API checkout or real SMS. `Sms:Provider=Mock` simulates delivery. The Eskiz class currently throws until a working integration is implemented.
