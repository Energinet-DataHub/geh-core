# Documentation

Notes regarding usage of the NuGet package `Energinet.DataHub.Core.TestCommon` that is part of the `TestCommon` bundle.

The package contains reuseable code to help implementing xUnit tests of Energinet DataHub components.

> We aim for documenting types using XML documentation comments, so be sure to also look at those.

## Types

### `Awaiter`

This utility class contains methods for awaiting conditions in tests.

### `ITestDiagnosticsLogger`

This interface, with its implementation, can be used within common test classes to write diagnostics information to output.

### `TestBase<TSut>`

This class can be used as a base class for a test class to easily get the *subject-under-test* (sut) created and exposed as a `Sut` property.

The class uses `AutoFixture` with `AutoMoqCustomization` which allows developers to customize the `Sut` in the test class constructor and thereby reusing the setup logic for tests within the test class.

The `Sut` instance is not created until the first time it is accessed.

## AutoFixture.Extensions

This namespace contains the extension method `ForConstructorOn<TTypeToConstruct>` that can be used to specify parameter values when constructing a type using `AutoFixture`.
