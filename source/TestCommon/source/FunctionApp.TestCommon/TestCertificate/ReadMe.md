# Test Certificate

The test certificate `test-common-cert.pfx` was generated using `dotnet dev-certs`.

When using this certificate together with Azurite the uri used from clients must use `localhost` instead of `127.0.0.1`.

See [Generate PFX certificate](https://github.com/Azure/Azurite/blob/main/README.md#generate-pfx-certificate)

It is possible to check a certificate (eg. NotBefore/NotAfter) with command like this:
`certutil -p azurite -dump .\test-common-cert.pfx`

Use the following command to generate a new certificate:
`dotnet dev-certs https --trust -ep test-common-cert.pfx -p test-common`
