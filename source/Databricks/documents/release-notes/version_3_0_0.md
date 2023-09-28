# Version 3.0.0 Release Notes

## New Features

### SQL Statement Execution using Parameter Markers

The method `ExecuteAsync(string)` has been replaced with `ExecuteAsync(string, List<SqlStatementParameter>?)`.
The new method supports [Parameter Markers](https://docs.databricks.com/en/sql/language-manual/sql-ref-parameter-marker.html) in for form of `:parameter_name` in the SQL statement. The parameters are passed in a list of `SqlStatementParameter` objects.

By using Parameter Markers in the SQL statement, the developer can prevent SQL injections.

## How to Upgrade

1. Update your NuGet package to version 3.0.0.
2. Modify your code to use the new `ExecuteAsync(string, List<SqlStatementParameter>)` by modifying your query to use Parameter Markers, and provide a list of `SqlStatementParameter` corresponding to the parameter markers.
   1. Optionally, the developer can use the method as before, only with a string. (Still recommended to use Parameter Markers in queries to avoid SQL injection)

## Example usage

```c#
[HttpGet]
public async Task<IActionResult> GetAsync()
{
    var sqlQuery = "SELECT * FROM my_table WHERE name = :my_name AND date = :my_date";
    var parameters = new List<SqlStatementParameter>
    {
        SqlStatementParameter.Create("my_name", "Sheldon Cooper"),
        SqlStatementParameter.Create("my_date", "26-02-1980", "DATE"),
    };
    var resultList = new List<TestModel>();

    await foreach (var row in _databricksSqlStatementClient.ExecuteAsync(sqlQuery, parameters)) {
        var testModel = new TestModel(row["my_name"], row["my_date"]);
        resultList.Add(testModel)
    }

    return Ok(resultList);
}
```

Notice, if a type is given in the `SqlStatementParameter` Create method, Databricks SQL Statement Execution API will perform type checking on the parameter value. Otherwise, it will be treated as a string. See [Databricks documentation](https://docs.databricks.com/api/workspace/statementexecution/executestatement) for more information.
