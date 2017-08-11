// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        private const string DateTimeOffsetFormatConst = "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}";

        public OracleDateTimeOffsetTypeMapping([NotNull] string storeType)
            : base(storeType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDateTimeOffsetTypeMapping(storeType);

        protected override string SqlLiteralFormatString => "'" + DateTimeOffsetFormatConst + "'";
    }
}
