// Copyright 2020 Energinet DataHub A/S
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

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace RequestResponseMiddleware.Tests
{
    public static class MockJwtTokens
    {
        private static string Issuer { get; } = Guid.NewGuid().ToString();

        private static SecurityKey SecurityKey { get; }

        private static SigningCredentials SigningCredentials { get; }

        private static readonly JwtSecurityTokenHandler _sTokenHandler = new();
        private static readonly RandomNumberGenerator _sRng = RandomNumberGenerator.Create();
        private static readonly byte[] _sKey = new byte[32];

        static MockJwtTokens()
        {
            _sRng.GetBytes(_sKey);
            SecurityKey = new SymmetricSecurityKey(_sKey) { KeyId = Guid.NewGuid().ToString() };
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        }

        public static string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            return _sTokenHandler.WriteToken(new JwtSecurityToken(Issuer, null, claims, null, DateTime.UtcNow.AddMinutes(20), SigningCredentials));
        }
    }
}
