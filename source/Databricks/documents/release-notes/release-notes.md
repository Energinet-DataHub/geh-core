# Databricks Release Notes

## Version 8.1.0

- Create objects from statements as an alternative to dynamic

## Version 8.0.2

- Fix bug in databricks timestamp handling

## Version 8.0.1

- Add support for mocking

## Version 8.0.0

- Obsolete methods are removed for Databricks SQL Warehouse integration
- Add support for adhoc SQL queries

## Version 7.2.2

- Databricks integration test configuration keys in keyvault are renamed

## Version 7.2.1

- No functional change.

## Version 7.2.0

- Databricks streaming of `ApacheArrow` and `JsonArray` is added.
- The `DatabricksSqlStatementClient` is deprecated. Please use `DatabricksSqlWarehouseQueryExecutor` instead. See [documentation.md](../documentation.md) for information on how to use the new functionality.

## Version 7.1.2

- The "Internal" namespace is removed, and related code is grouped in relevant namespaces.

## Version 7.1.1

- No functional change.

## Version 7.1.0

Databricks streaming is added. See [documentation.md](../documentation.md) for information on how to use the new functionality.

## Version 7.0.1

- Remove validation from DatabricksSqlStatementApiHealthCheck as this was refactored in version 7.0.0 to be handled using options validation.

## Version 7.0.0

In this new version we:

- refactored namespace `AppSettings` to `Configuration`.
- refactored the validation of `DatabricksJobsOptions` to use validation based on data annotations.
- refactored the validation of `DatabricksSqlStatementOptions` to use validation based on data annotations.

## Version 6.0.0

- The jobs extension now registers the http client.

## Version 5.0.0

In this new version we:

- added the Databricks Jobs options to dependency injection.

## Version 4.0.0

In this new version we:

- added Health checks for the Databricks SQL Statement Execution API
- added Health checks for the Databricks Jobs API
- added `JobsApiClient`as a client for the Databricks Jobs API

See [documentation.md](../documentation.md) for information on how to setup and use the package.

## Version 3.0.0

- See [Version 3.0.0 release notes](./version_3_0_0.md)

## Version 2.0.0

- See [Version 2.0.0 release notes](./version_2_0_0.md)

## Version 1.1.0

- Configuration of HttpClient is moved to Dependency Injection

## Version 1.0.4

- No functional change.

## Version 1.0.3

- No functional change.

## Version 1.0.2

- No functional change.

## Version 1.0.1

- No functional change.

## Version 1.0.0

- Initial release.
