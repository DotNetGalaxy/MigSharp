﻿using NUnit.Framework;

namespace MigSharp.SqlServerCe.NUnit
{
    [TestFixture, Category("SqlServerCe3.5")]
    public class SqlServerCe35IntegrationTests : SqlServerCeIntegrationTestsBase
    {
        protected override DbPlatform DbPlatform { get { return DbPlatform.SqlServerCe35; } }

        protected override string CeVersion { get { return "3.5"; } }
    }
}