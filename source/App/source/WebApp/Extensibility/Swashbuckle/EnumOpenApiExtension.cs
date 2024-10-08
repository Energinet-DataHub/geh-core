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

using System.Text.Json;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Energinet.DataHub.Core.App.WebApp.Extensibility.Swashbuckle;

/// <summary>
/// OpenAPI extension for adding enum names to the OpenAPI schema.
/// This will enable nswag to generate the enum names in the client. Without using JsonStringEnumConverter,
/// enabling having both the enum value and the name in the client.
/// </summary>
public class EnumOpenApiExtension : IOpenApiExtension
{
    private readonly SchemaFilterContext _context;

    public EnumOpenApiExtension(SchemaFilterContext context)
    {
        _context = context;
    }

    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        string[] enums = Enum.GetNames(_context.Type);
        JsonSerializerOptions options = new() { WriteIndented = true };
        string value = JsonSerializer.Serialize(enums, options);
        writer.WriteRaw(value);
    }
}
