﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using ServiceContract;

namespace Services
{
    internal class SystemDataService : ISystemDataService
    {
        public DataSet GetDataSet() => throw new NotImplementedException();
    }
}
