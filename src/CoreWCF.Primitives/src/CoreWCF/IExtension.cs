﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CoreWCF
{
    public interface IExtension<T> where T : IExtensibleObject<T>
    {
        void Attach(T owner);
        void Detach(T owner);
    }
}