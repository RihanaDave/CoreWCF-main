﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using CoreWCF.Channels;

namespace CoreWCF.Dispatcher
{
    public interface IInstanceContextProvider
    {
        InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel);
        void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel);
        bool IsIdle(InstanceContext instanceContext);
        void NotifyIdle(Action<InstanceContext> callback, InstanceContext instanceContext);
    }


    internal abstract class InstanceContextProviderBase : IInstanceContextProvider
    {
        public DispatchRuntime DispatchRuntime { get; }

        internal InstanceContextProviderBase(DispatchRuntime dispatchRuntime)
        {
            DispatchRuntime = dispatchRuntime;
        }

        internal static bool IsProviderSingleton(IInstanceContextProvider provider)
        {
            return (provider is SingletonInstanceContextProvider);
        }

        internal static bool IsProviderSessionful(IInstanceContextProvider provider)
        {
            return (provider is PerSessionInstanceContextProvider);
        }

        internal static IInstanceContextProvider GetProviderForMode(InstanceContextMode instanceMode, DispatchRuntime runtime)
        {
            switch (instanceMode)
            {
                case InstanceContextMode.PerCall:
                    return new PerCallInstanceContextProvider(runtime);
                case InstanceContextMode.PerSession:
                    return new PerSessionInstanceContextProvider(runtime);
                case InstanceContextMode.Single:
                    return new SingletonInstanceContextProvider(runtime);
                default:
                    DiagnosticUtility.FailFast("InstanceContextProviderBase.GetProviderForMode: default");
                    return null;
            }
        }

        internal static bool IsProviderPerCall(IInstanceContextProvider provider)
        {
            return (provider is PerCallInstanceContextProvider);
        }

        internal ServiceChannel GetServiceChannelFromProxy(IContextChannel channel)
        {
            if (!(channel is ServiceChannel serviceChannel))
            {
                serviceChannel = ServiceChannelFactory.GetServiceChannel(channel);
            }
            return serviceChannel;
        }

        #region IInstanceContextProvider Members

        public virtual InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public virtual void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public virtual bool IsIdle(InstanceContext instanceContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public virtual void NotifyIdle(Action<InstanceContext> callback, InstanceContext instanceContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        #endregion
    }
}
