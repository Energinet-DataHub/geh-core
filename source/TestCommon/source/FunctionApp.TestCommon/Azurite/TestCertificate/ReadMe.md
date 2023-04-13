# Test Certificate

The test certificate `azurite-cert.pfx` was generated using `dotnet dev-certs`.

When using this certificate together with Azurite the uri used from clients must use `localhost` instead of `127.0.0.1`.

See https://github.com/Azure/Azurite/blob/main/README.md#generate-pfx-certificate