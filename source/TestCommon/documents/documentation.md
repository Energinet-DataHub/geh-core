# Documentation

Notes regarding usage of the NuGet package bundle `TestCommon`.

The bundle contains the following packages:

* [Energinet.DataHub.Core.FunctionApp.TestCommon](#functionapp.testcommon)
* [Energinet.DataHub.Core.TestCommon](#testcommon)

> We aim for documenting types using XML documentation comments, so be sure to also look at those.

## FunctionApp.TestCommon

The package contains reuseable code to help implementing xUnit tests of Energinet DataHub Azure Functions.

*TODO: Mention usage of the `functionapphost.settings.json`.*

*TODO: Mention example of usage can be seen usage in geh-charges repository.*

## TestCommon

The package contains reuseable code to help implementing xUnit tests of Energinet DataHub components.

### Types

#### `Awaiter`

This utility class contains methods for awaiting conditions in tests.

#### `ITestDiagnosticsLogger`

This interface, with its implementation, can be used within common test classes to write diagnostics information to output.

#### `TestBase<TSut>`

This class can be used as a base class for a test class to easily get the *subject-under-test* (sut) created and exposed as a `Sut` property.

The class uses `AutoFixture` with `AutoMoqCustomization` which allows developers to customize the `Sut` in the test class constructor and thereby reusing the setup logic for tests within the test class.

The `Sut` instance is not created until the first time it is accessed.

### AutoFixture.Extensions

This namespace contains the extension method `ForConstructorOn<TTypeToConstruct>` that can be used to specify parameter values when constructing a type using AutoFixture.
