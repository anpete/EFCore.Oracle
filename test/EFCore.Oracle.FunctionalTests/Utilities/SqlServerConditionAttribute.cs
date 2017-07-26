// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [TraitDiscoverer("Microsoft.EntityFrameworkCore.Oracle.FunctionalTests.Utilities.OracleConditionTraitDiscoverer", "Microsoft.EntityFrameworkCore.Oracle.FunctionalTests")]
    public class OracleConditionAttribute : Attribute, ITestCondition, ITraitAttribute
    {
        public OracleCondition Conditions { get; set; }

        public OracleConditionAttribute(OracleCondition conditions)
        {
            Conditions = conditions;
        }

        public bool IsMet
        {
            get
            {
                var isMet = true;
                if (Conditions.HasFlag(OracleCondition.SupportsSequences))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(OracleCondition.SupportsSequences)) ?? true;
                }
                if (Conditions.HasFlag(OracleCondition.SupportsOffset))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(OracleCondition.SupportsOffset)) ?? true;
                }
                if (Conditions.HasFlag(OracleCondition.SupportsHiddenColumns))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(OracleCondition.SupportsHiddenColumns)) ?? false;
                }
                if (Conditions.HasFlag(OracleCondition.SupportsMemoryOptimized))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(OracleCondition.SupportsMemoryOptimized)) ?? false;
                }
                if (Conditions.HasFlag(OracleCondition.IsSqlAzure))
                {
                    isMet &= TestEnvironment.IsSqlAzure;
                }
                if (Conditions.HasFlag(OracleCondition.IsNotSqlAzure))
                {
                    isMet &= !TestEnvironment.IsSqlAzure;
                }
                if (Conditions.HasFlag(OracleCondition.SupportsAttach))
                {
                    var defaultConnection = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection);
                    isMet &= defaultConnection.DataSource.Contains("(localdb)")
                             || defaultConnection.UserInstance;
                }
                if (Conditions.HasFlag(OracleCondition.IsNotTeamCity))
                {
                    isMet &= !TestEnvironment.IsTeamCity;
                }
                return isMet;
            }
        }

        public string SkipReason =>
            // ReSharper disable once UseStringInterpolation
            string.Format("The test SQL Server does not meet these conditions: '{0}'",
                string.Join(", ", Enum.GetValues(typeof(OracleCondition))
                    .Cast<Enum>()
                    .Where(f => Conditions.HasFlag(f))
                    .Select(f => Enum.GetName(typeof(OracleCondition), f))));
    }

    [Flags]
    public enum OracleCondition
    {
        SupportsSequences = 1 << 0,
        SupportsOffset = 1 << 1,
        IsSqlAzure = 1 << 2,
        IsNotSqlAzure = 1 << 3,
        SupportsMemoryOptimized = 1 << 4,
        SupportsAttach = 1 << 5,
        SupportsHiddenColumns = 1 << 6,
        IsNotTeamCity = 1 << 7
    }
}
