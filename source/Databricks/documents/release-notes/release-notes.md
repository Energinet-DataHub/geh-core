# Databricks Release Notes

## Version 2.1.0
- Added support for `SqlStatementParameter` in `ExecuteAsync(string sqlStatement, List<SqlStatementParameter> parameters)`, to prevent SQL Injection.
- Marked `ExecuteAsync(string sqlStatement)` as obsolete. please use `ExecuteAsync(string, List<SqlStatementParameter>)` instead.

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
