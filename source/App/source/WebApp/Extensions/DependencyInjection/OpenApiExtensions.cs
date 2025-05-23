﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;
using Energinet.DataHub.Core.App.WebApp.Extensibility.Swashbuckle;
using Energinet.DataHub.Core.App.WebApp.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding Open API services to an ASP.NET Core app.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Register services necessary for enabling an ASP.NET Core app to generate
    /// Open API specifications and work with Swagger UI.
    /// Expects an XML file with C# documentation comments named like the executing assembly.
    /// The documentation is used for endpoints descriptions.
    /// </summary>
    /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection instance.</param>
    /// <param name="executingAssembly">Typically the web app assembly.</param>
    /// <param name="swaggerUITitle">The title which will be display in the swagger UI, independent of the apiVersion</param>
    /// <param name="swaggerUIDescription">The API description to be displayed in the swagger UI, independent of the apiVersion</param>
    /// <param name="useFullnameForSchemaIds"> SchemaId of types are generated from the fully qualified namespace, assembly not included</param>
    public static IServiceCollection AddSwaggerForWebApp(
        this IServiceCollection services,
        Assembly executingAssembly,
        string swaggerUITitle,
        string? swaggerUIDescription = null,
        bool useFullnameForSchemaIds = false)
    {
        ArgumentNullException.ThrowIfNull(executingAssembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(swaggerUITitle);

        var xmlFile = $"{executingAssembly.GetName().Name}.xml";
        services.AddSwaggerForWebApp(xmlFile, swaggerUITitle, swaggerUIDescription, useFullnameForSchemaIds);

        return services;
    }

    /// <summary>
    /// Register services necessary for enabling an ASP.NET Core app to generate
    /// Open API specifications and work with Swagger UI.
    /// Expects an XML file with C# documentation comments.
    /// The documentation is used for endpoints descriptions.
    /// </summary>
    /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection instance.</param>
    /// <param name="xmlCommentsFilename">Filename (with extension) of the XML file containing C# documentation comments.</param>
    /// <param name="swaggerUITitle">The title which will be display in the swagger UI, independent of the apiVersion</param>
    /// <param name="swaggerUIDescription">The API description to be displayed in the swagger UI, independent of the apiVersion</param>
    /// <param name="useFullnameForSchemaIds"> SchemaId of types are generated from the fully qualified namespace, assembly not included</param>
    public static IServiceCollection AddSwaggerForWebApp(
        this IServiceCollection services,
        string xmlCommentsFilename,
        string swaggerUITitle,
        string? swaggerUIDescription = null,
        bool useFullnameForSchemaIds = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xmlCommentsFilename);
        ArgumentException.ThrowIfNullOrWhiteSpace(swaggerUITitle);

        services.AddOptions<SwaggerUIOptions>()
            .Configure(options =>
            {
                options.Title = swaggerUITitle;
                options.Description = swaggerUIDescription;
            });
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();
            options.UseAllOfToExtendReferenceSchemas();

            if (useFullnameForSchemaIds)
            {
                options.CustomSchemaIds(x => x.FullName);
            }

            // Set the comments path for the Swagger JSON and UI.
            // See: https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio#xml-comments
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFilename);
            options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition(
                name: JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme,
                        },
                    },
                    new[] { JwtBearerDefaults.AuthenticationScheme }
                },
            });

            // Support marking endpoint return type as binary content
            options.OperationFilter<BinaryContentFilter>();
            // Adds extra schema details for an enum in the swagger.json i.e. x-enumNames (used by NSwag to generate Enums for C# client)
            options.SchemaFilter<EnumExtensionSchemaFilter>();
        });

        return services;
    }
}
