﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CoreWCF.Configuration
{
    internal static class BasicHttpMessageCredentialTypeHelper
    {
        internal static bool IsDefined(BasicHttpMessageCredentialType value)
        {
            return (value == BasicHttpMessageCredentialType.UserName ||
                value == BasicHttpMessageCredentialType.Certificate);
        }
    }
}
