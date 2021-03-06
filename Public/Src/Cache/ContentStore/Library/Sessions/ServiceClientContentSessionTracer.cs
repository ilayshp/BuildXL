// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BuildXL.Cache.ContentStore.Stats;
using BuildXL.Cache.ContentStore.Tracing;

namespace BuildXL.Cache.ContentStore.Sessions
{
    /// <summary>
    /// Content tracer for the service client.
    /// </summary>
    public class ServiceClientContentSessionTracer : ContentSessionTracer
    {
        private const string ClientWaitForServerName = "ClientWaitForServer";
        private readonly Counter _clientWaitForServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClientContentSessionTracer"/> class.
        /// </summary>
        public ServiceClientContentSessionTracer(string name)
            : base(name, useTracerName: true)
        {
            Counters.Add(_clientWaitForServer = new Counter(ClientWaitForServerName));
        }

        /// <summary>
        /// Tracks client/server waiting time.
        /// </summary>
        public void TrackClientWaitForServerTicks(long delayTicks)
        {
            _clientWaitForServer.Add(delayTicks / TimeSpan.TicksPerMillisecond);
        }
    }
}
