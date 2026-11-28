# Security

The API requires a configured JWT secret of at least 32 characters. Treat `.env`, customer contacts and order records as private. Mock SMS now logs only that delivery was simulated; it does not log a phone number or one-time code. The Eskiz provider fails explicitly because delivery is not implemented.

Before a commercial launch, verify OTP delivery, object-level authorization, order idempotency and stock races with a test PostgreSQL database. NuGet vulnerability feed access failed with `NU1900` here, so dependency status is unverified.
