// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public static class OracleExceptionFactory
    {
        public static OracleException CreateOracleException(int number)
        {
            var errorCtors = typeof(OracleError)
                .GetTypeInfo()
                .DeclaredConstructors;

            var error = (OracleError)errorCtors.First(c => c.GetParameters().Length == 4)
                .Invoke(new object[] { number, "dataSrc", "procedure", "errMsg" });

            var errors = (OracleErrorCollection)typeof(OracleErrorCollection)
                .GetTypeInfo()
                .DeclaredConstructors
                .Single()
                .Invoke(null);

            typeof(OracleErrorCollection).GetRuntimeMethods().Single(m => m.Name == "Add").Invoke(errors, new object[] { error });

            var exceptionCtors = typeof(OracleException)
                .GetTypeInfo()
                .DeclaredConstructors;

            return (OracleException)exceptionCtors.First(c => c.GetParameters().Length == 4)
                .Invoke(new object[] { number, "dataSrc", "procedure", "errMsg" });
        }
    }
}
