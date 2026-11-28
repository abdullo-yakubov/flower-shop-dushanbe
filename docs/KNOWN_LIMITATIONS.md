# Known limitations

- SMS uses a mock provider; Eskiz is unimplemented and fails explicitly. No real OTP delivery was verified.
- A mock frontend mode exists. It does not establish that backend checkout works.
- Backend and frontend unit tests pass, but PostgreSQL order/stock integration and browser checkout have not been verified.
- Real payment integration is not established from the audited source.
- NuGet vulnerability feed returned `NU1900` in the sandbox.
- Real screenshots, Lighthouse, load tests and deployment checks are pending.
