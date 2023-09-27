# Databricks Release Notes

## Version 3.0.0

- See [Version 2.0.0 release notes](./version_3_0_0.md)
- Replaced `ExecuteAsync(string)` with `ExecuteAsync(string, List<SqlStatementParameter>)` for the possibility to use Parameter Markers in SQL statements.
    - Optionally, it is possible to call the method with an empty list as parameters and it would work as before. (Still recommended to use Parameter Markers in queries to avoid SQL injection)

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
