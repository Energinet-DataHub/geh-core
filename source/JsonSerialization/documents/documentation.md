# Documentation

Notes regarding usage of the NuGet package bundle `JsonSerialization`.

This package exists to make it easy for developers of Energinet DataHub 3.0 to handle serialization and deserialization of objects without the need to extend `System.Text.Json` with converters for `NodaTime.Instant` on their own.

# Getting started

The package contains a `JsonSerializer`.

`JsonSerializer` has an optional `JsonSerializerOptions` parameter, that can be injected through dependency injection if needed.

By default the `JsonSerializer` includes a converter for `NodaTime.Instant` serialization and deserialization.

# Registration

**Default options:**

```
services.AddSingleton<IJsonSerializer, JsonSerializer>();
```

**Custom options:**

```
services.AddSingleton<IJsonSerializer>(x =>
                {
                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new ConnectionStateConverter());
                    options.Converters.Add(new MeteringMethodConverter());
                    options.Converters.Add(new MeteringPointTypeConverter());
                    options.Converters.Add(new ResolutionConverter());
                    options.Converters.Add(new ProductConverter());
                    options.Converters.Add(new UnitConverter());
                    options.Converters.Add(new SettlementMethodConverter());
                    options.PropertyNamingPolicy = new CustomJsonNamingPolicy();

                    return new JsonSerializer(options);
                });
```
