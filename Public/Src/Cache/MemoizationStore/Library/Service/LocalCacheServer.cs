﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using BuildXL.Cache.ContentStore.Interfaces.FileSystem;
using BuildXL.Cache.ContentStore.Interfaces.Logging;
using BuildXL.Cache.ContentStore.Interfaces.Results;
using BuildXL.Cache.ContentStore.Interfaces.Stores;
using BuildXL.Cache.ContentStore.Service;
using BuildXL.Cache.ContentStore.Tracing;
using BuildXL.Cache.ContentStore.Tracing.Internal;
using BuildXL.Cache.MemoizationStore.Interfaces.Caches;
using BuildXL.Cache.MemoizationStore.Interfaces.Sessions;
using BuildXL.Cache.MemoizationStore.Sessions.Grpc;
using Grpc.Core;

namespace BuildXL.Cache.MemoizationStore.Service
{
    /// <summary>
    /// IPC interface to a file system memoization store.
    /// </summary>
    public class LocalCacheServer : LocalContentServerBase<ICache, ICacheSession>
    {
        private readonly GrpcCacheServer _grpcCacheServer;

        /// <inheritdoc />
        protected override Tracer Tracer { get; } = new Tracer(nameof(LocalCacheServer));

        /// <nodoc />
        public LocalCacheServer(
            IAbsFileSystem fileSystem,
            ILogger logger,
            string scenario,
            Func<AbsolutePath, ICache> cacheFactory,
            LocalServerConfiguration localContentServerConfiguration,
            Capabilities capabilities = Capabilities.All)
        : base(logger, fileSystem, scenario, cacheFactory, localContentServerConfiguration)
        {
            // This must agree with the base class' StoresByName to avoid "missing content store" errors from Grpc, and
            // to make sure everything is initialized properly when we expect it to.
            var storesByNameAsContentStore = StoresByName.ToDictionary(kvp => kvp.Key, kvp =>
            {
                var store = kvp.Value;
                if (store is IContentStore contentStore)
                {
                    return contentStore;
                }

                throw new ArgumentException(
                    $"Severe cache misconfiguration: {nameof(cacheFactory)} must generate instances that are " +
                    $"IContentStore. Instead, it generated {store.GetType()}.",
                    nameof(cacheFactory));
            });

            _grpcCacheServer = new GrpcCacheServer(logger, capabilities, this, storesByNameAsContentStore, localContentServerConfiguration);
        }

        /// <inheritdoc />
        protected override ServerServiceDefinition[] BindServices() => _grpcCacheServer.Bind();

        /// <inheritdoc />
        protected override Task<GetStatsResult> GetStatsAsync(ICache store, OperationContext context) => store.GetStatsAsync(context);

        /// <inheritdoc />
        protected override CreateSessionResult<ICacheSession> CreateSession(ICache store, OperationContext context, string name, ImplicitPin implicitPin) => store.CreateSession(context, name, implicitPin);

        /// <inheritdoc />
        protected override async Task<BoolResult> StartupCoreAsync(OperationContext context)
        {
            await _grpcCacheServer.StartupAsync(context).ThrowIfFailure();

            return await base.StartupCoreAsync(context);
        }

        /// <inheritdoc />
        protected override async Task<BoolResult> ShutdownCoreAsync(OperationContext context)
        {
            // Tracing content server statistics at shutdown, because currently no one calls GetStats on this instance.
            Tracer.TraceStatisticsAtShutdown(context, _grpcCacheServer.Counters.ToCounterSet(), prefix: "GrpcContentServer");

            var result = await base.ShutdownCoreAsync(context);

            result &= await _grpcCacheServer.ShutdownAsync(context);

            return result;
        }
    }
}
