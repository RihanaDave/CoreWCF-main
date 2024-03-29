﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Security;
using System.Security.Authentication;

namespace CoreWCF.Channels
{
    internal static class TransportDefaults
    {
        internal const bool ExtractGroupsForWindowsAccounts = true; //SspiSecurityTokenProvider.DefaultExtractWindowsGroupClaims;
        internal const long MaxReceivedMessageSize = 65536;
        internal const HostNameComparisonMode HostNameComparisonMode = CoreWCF.HostNameComparisonMode.Exact;
        internal const int MaxDrainSize = (int)MaxReceivedMessageSize;
        internal const long MaxBufferPoolSize = 512 * 1024;
        internal const int MaxBufferSize = (int)MaxReceivedMessageSize;
        internal const SslProtocols SslProtocols = System.Security.Authentication.SslProtocols.None; // Let the OS decide
    }

    internal static class ConnectionOrientedTransportDefaults
    {
        internal const int ConnectionBufferSize = 8192;
        internal const HostNameComparisonMode HostNameComparisonMode = CoreWCF.HostNameComparisonMode.StrongWildcard;
        internal static TimeSpan IdleTimeout { get { return TimeSpan.FromMinutes(2); } }
        internal static TimeSpan ChannelInitializationTimeout { get { return TimeSpan.FromSeconds(30); } }
        internal const int MaxContentTypeSize = 256;
        internal const int MaxOutboundConnectionsPerEndpoint = 10;
        internal static TimeSpan MaxOutputDelay { get { return TimeSpan.FromMilliseconds(200); } }

        internal const int MaxViaSize = 2048;
        internal const ProtectionLevel ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
        internal const TransferMode TransferMode = CoreWCF.TransferMode.Buffered;

        internal static int GetMaxConnections()
        {
            return GetMaxPendingConnections();
        }

        internal static int GetMaxPendingConnections()
        {
            return 12 * Environment.ProcessorCount;
        }

        internal static int GetMaxPendingAccepts()
        {
            return 2 * Environment.ProcessorCount;
        }
    }

    internal static class UnixDomainTransportDefaults
    {
        internal static int GetListenBacklog()
        {
            return 12 * Environment.ProcessorCount;
        }
    }
}
