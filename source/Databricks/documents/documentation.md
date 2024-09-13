# Databricks Documentation

## SQL Statement Execution

This project contains a client for the Databricks SQL Statement Execution API. The client is a wrapper around the Databricks REST API. The client is used to execute SQL statements on a Databricks cluster.

The implementation uses solely `disposition=EXTERNAL_LINKS` despite that inlining is simpler. This is because inlining has a limit of 16MB, which isn't sufficient for all use cases.

### Usage

Install `Energinet.DataHub.Core.Databricks.SqlStatementExecution` package.

Example of how to setup the Databricks Sql Statement Execution in `startup.cs`.

```c#
private static void AddDatabricksSqlStatementExecution(IServiceCollection services, IConfiguration configuration)
{   
    services.Configure<DatabricksSqlStatementOptions>(
                configuration.GetSection(DatabricksSqlStatementOptions.DatabricksOptions));
    
    services.AddDatabricksSqlStatementExecution();
}
```

#### DatabricksSqlWarehouseQueryExecutor

`DatabricksSqlWarehouseQueryExecutor` is built around streaming of data from Databricks SQL Warehouse.

A query is created by extending `DatabricksStatement` and implementing the mandatory method `GetSqlStatement`. To query all persons a minimal implementation would look like this:

```c#
public class QueryAllPersons : DatabricksStatement
{
    protected internal override string GetSqlStatement()
    {
        return "SELECT name, date FROM persons";
    }
}
```

If you want to add a filter to a query this must be implemented with parameters.

```c#
public class QueryPersons : DatabricksStatement
{
    private readonly string _name;
    private readonly DateTime _date;

    public QueryPersons(string name, DateTime date)
    {
        _name = name;
        _date = date;
    }

    protected internal override string GetSqlStatement()
    {
        return "SELECT name, date FROM persons where name = :my_name AND date = :my_date";
    }

    protected internal override IReadOnlyCollection<QueryParameter> GetParameters()
    {
        return new[]
        {
            QueryParameter.Create("my_name", _name),
            QueryParameter.Create("my_date", _date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "Z"),
        };
    }
}
```

A query can then be sent to Databricks SQL Warehouse with the `DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync` method. It is possible to set the streaming format. If no format is set, then ApacheArrow is used. The methods returns a dynamic object for each record that is read from Databricks SQL Warehouse.

```c#
var query = new QueryPersons(name: "Sheldon Cooper", date: new DateTime(1980, 2, 26));
var records = _warehouse.ExecuteStatementAsync(query); // _warehouse is an instance of DatabricksSqlWarehouseQueryExecutor

var allSheldons = new List<Person>();
await foreach (var record in records)
    allSheldons.Add(new Person(record.name, record.date));
```

A query can contain arrays. When used with Apache Arrow the array is encoded as object[]. If the format is JsonArray the array is encoded as Json string array.

```c#
var statement = DatabricksStatement.FromRawSql(
            @"SELECT a, b FROM VALUES
                ('one', array(0, 1)),
                ('two', array(2, 3)) AS data(a, b);").Build();

var result = client.ExecuteStatementAsync(statement, Format.ApacheArrow);
var row = await result.FirstAsync();

// Apache arrow
var values = ((object[])row.b).OfType<int>();

// JsonArray
var values = JsonConvert.DeserializeObject<string[]>((string)row.b).Select(int.Parse);
```

#### Controlling the Number of Concurrently Downloaded Chunks

To control the number of chunks being downloaded concurrently when using the `DatabricksSqlWarehouseQueryExecutor`, you can use the `QueryOptions` class. The `QueryOptions` class allows you to customize the behavior of the query execution, including the parallel download of chunks.

By default, the parallel download is disabled. To enable parallel downloading and specify the number of concurrent chunks, you can use the `WithParallelDownload` method of the `QueryOptions` class. This method takes an optional parameter `maxParallelChunks` that specifies the maximum number of chunks to be downloaded concurrently.

Here's an example of how to use the `WithParallelDownload` method to control the number of concurrently downloaded chunks:

```c#
var query = new QueryPersons(name: "Sheldon Cooper", date: new DateTime(1980, 2, 26));
var options = QueryOptions.Default.WithParallelDownload(maxParallelChunks: 5); // Set the maximum number of concurrent chunks to 5
var records = _warehouse.ExecuteStatementAsync(query, options); // _warehouse is an instance of DatabricksSqlWarehouseQueryExecutor
```

In the example above, the `maxParallelChunks` parameter is set to 5, which means that up to 5 chunks will be downloaded concurrently. You can adjust this value based on your specific requirements and the capabilities of your system.

Remember to handle the downloaded chunks appropriately in your code to ensure efficient processing and avoid any potential performance issues.

Usage:

```c#
// Using ApacheArrow format and download in parallel
var query = new QueryPersons(name: "Sheldon Cooper", date: new DateTime(1980, 2, 26));
var records = _warehouse.ExecuteStatementAsync(query, QueryOptions.Default.WithParallelDownload()); // _warehouse is an instance of DatabricksSqlWarehouseQueryExecutor
```

#### Adhoc queries

It's possible to create adhoc queries from `DatabricksStatement` class.

```c#
var statement = DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
              ('Zen Hui', 25),
              ('Anil B' , 18),
              ('Shone S', 16),
              ('Mike A' , 25),
              ('John A' , 18),
              ('Jack N' , 16) AS data(name, age)
              WHERE data.age = :_age;")
            .WithParameter("_age", 25)
            .Build();
```

#### ApacheArrow or JsonArray

The main difference between the two is that when using `Format.ApacheArrow` all the columns are [mapped](../source/SqlStatementExecution/Formats/IArrowArrayExtensions.cs) to a .NET type. If use are using `Format.JsonArray` all columns are returned as string.

#### Mocking

The class `DatabricksSqlWarehouseQueryExecutor` can be mocked by overwriting the `ExecuteStatementAsync` methods.

E.g.:

```c#
// using package System.Linq.Async

var mock = new Mock<DatabricksSqlWarehouseQueryExecutor>();
mock
    .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>()))
    .Returns(new dynamic[] { new { Id = 1 }, new { Id = 2 } }.ToAsyncEnumerable());

var result = mock.Object.ExecuteStatementAsync(new LimitRows(10));
var rowCount = await result.CountAsync();

rowCount.Should().Be(2);
```

#### Experimental - object creation

It's possible to create objects from the result of a query. This is done by annotation the properties of a record class with ArrowField attributes.

The creation is very limited. It only applies to records that are constructed from constructor parameters. Using the constructor order property of the ArrowField attribute to map the constructor parameters to the columns of the query.

Example usage:

```c#
public record Person(
        [property: ArrowField("name", 1)] string Name,
        [property: ArrowField("age", 2)] int Age);

// Create person objects
var statement = DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
              ('Zen Hui', 25),
              ('Anil B' , 18),
              ('Shone S', 16),
              ('Mike A' , 25),
              ('John A' , 18),
              ('Jack N' , 16) AS data(name, age)")
            .Build();


var result = client.ExecuteStatementAsync<Person>(statement);
await foreach (var person in result) 
    Console.WriteLine(person);
```

### Health checks

The package contains functionality to do health checks of the status of the Databricks Sql Statement Execution API.

Example of how to setup the Databricks Sql Statement Execution health check in `startup.cs`.

```c#
private static void AddSqlStatementApiHealthChecks(IServiceCollection serviceCollection, IConfiguration configuration)
{
    services.Configure<DatabricksSqlStatementOptions>(
                configuration.GetSection(DatabricksSqlStatementOptions.DatabricksOptions));
    
    serviceCollection.AddHealthChecks()
        .AddLiveCheck()
        .AddDatabricksSqlStatementApiHealthCheck();
}
```

## Jobs

This project contains a client for the Databricks Jobs API. The client is a wrapper around the Databricks Jobs REST API. The client is used to execute Jobs on a Databricks cluster.

### Usage

Install `Energinet.DataHub.Core.Databricks.Jobs` package.

Example of how to setup the Databricks in `startup.cs`.

```c#
private static void AddDatabricksJobs(IServiceCollection services, IConfiguration configuration)
{   
    services.AddDatabricksJobs(configuration);
}
```

Example of how to use the Jobs client.

```c#
[HttpGet]
public async Task<Job> GetAsync()
{
    var jobs = await _client.Jobs.List().ConfigureAwait(false);
    var exampleJob = jobs.Jobs.Single(j => j.Settings.Name == "ExampleJob");
    return await _client.Jobs.Get(exampleJob.JobId).ConfigureAwait(false);
}
```

### Health checks

The package contains functionality to do health checks of the status of the Databricks Jobs API.

Example of how to setup the Databricks Jobs health check in `startup.cs`.

```c#
private static void AddJobsApiHealthChecks(IServiceCollection serviceCollection)
{
    serviceCollection.AddHealthChecks()
        .AddLiveCheck()
        .AddDatabricksJobsApiHealthCheck();
}
```
