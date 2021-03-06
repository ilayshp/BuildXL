// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BuildXL.Cache.ContentStore.Distributed.NuCache;
using BuildXL.Cache.ContentStore.Interfaces.FileSystem;
using BuildXL.Cache.ContentStore.Interfaces.Logging;
using BuildXL.Cache.ContentStore.Interfaces.Time;
using BuildXL.Cache.ContentStore.SQLite;
using BuildXL.Cache.ContentStore.Stores;
using BuildXL.Cache.MemoizationStore.Interfaces.Stores;

namespace BuildXL.Cache.MemoizationStore.Stores
{
    /// <summary>
    ///     Marker type for children of <see cref="MemoizationStore"/>
    /// </summary>
    public abstract class MemoizationStoreConfiguration
    {
        /// <summary>
        /// Create memoization store with the current config
        /// </summary>
        public abstract IMemoizationStore CreateStore(ILogger logger, IClock clock);
    }

    /// <summary>
    ///     Grouped configuration for <see cref="RocksDbMemoizationStore"/>
    /// </summary>
    public class RocksDbMemoizationStoreConfiguration : MemoizationStoreConfiguration
    {
        /// <summary>
        /// Configuration for the internal RocksDB database
        /// </summary>
        public RocksDbContentLocationDatabaseConfiguration Database { get; set; }

        /// <inheritdoc />
        public override IMemoizationStore CreateStore(ILogger logger, IClock clock)
        {
            return new RocksDbMemoizationStore(logger, clock, this);
        }
    }

    /// <summary>
    ///     Grouped configuration for <see cref="SQLiteMemoizationStore"/>
    /// </summary>
    public class SQLiteMemoizationStoreConfiguration : MemoizationStoreConfiguration
    {
        /// <summary>
        /// Configuration for the internal SQLite database
        /// </summary>
        public SQLiteDatabaseConfiguration Database { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of rows in the database.
        /// </summary>
        public long MaxRowCount { get; set; } = 500000;

        /// <summary>
        ///     Gets or sets a value indicating whether to wait for lru to complete on shutdown.
        /// </summary>
        public bool WaitForLruOnShutdown { get; set; } = true;

        /// <summary>
        ///     Gets or sets how long to wait for the single instance mutex/lockfile before giving up.
        /// </summary>
        public int SingleInstanceTimeoutSeconds { get; set; } = ContentStoreConfiguration.DefaultSingleInstanceTimeoutSeconds;

        /// <summary>
        /// Force usage of the public constructor.
        /// </summary>
        private SQLiteMemoizationStoreConfiguration()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteMemoizationStoreConfiguration"/> class.
        /// </summary>
        public SQLiteMemoizationStoreConfiguration(AbsolutePath path)
        {
            Database = new SQLiteDatabaseConfiguration(path);
        }

        /// <inheritdoc />
        public override IMemoizationStore CreateStore(ILogger logger, IClock clock)
        {
            return new SQLiteMemoizationStore(logger, clock, this);
        }
    }
}
