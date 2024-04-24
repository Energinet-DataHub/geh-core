# Databricks Release Notes

## Version 10.0.2

- No functional change

## Version 10.0.1

- No functional change

## Version 10.0.0

Bump to .NET 8.0 for TargetFramework.

## Version 9.0.2

Cancel statement if token cancellation is requested.

When the request is canceled the cluster is notified to also cancel execution of that statement.

With this change the communication with the job cluster is using the [**Asynchronous mode**](https://docs.databricks.com/api/azure/workspace/statementexecution/executestatement#wait_timeout).

## Version 9.0.1

Added try-catch block to health checks to prevent both logging exceptions and indicate unhealthy state.

## Version 9.0.0

The `DatabricksSqlStatementOptions.TimeoutInSeconds` is removed. It didn't represent the total timeout for a request,
but rather constituted a part of the overall time out where the rest of the timeout was hardcoded.
So it was misleading. It has been replaced by the usage of cancellation tokens.

There are no changes to the jobs API.

## Version 8.2.2

- No functional change

## Version 8.2.1

- No functional change

## Version 8.2.0

- Add support for ListArray in Apache Arrow format
- Update documentation with usage guide for Apache Arrow format and JsonArray format for arrays

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
