﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using CoreWCF.Channels;
using CoreWCF.Runtime;

namespace CoreWCF.Dispatcher
{
    internal class PrefixEndpointAddressMessageFilterTable<TFilterData> : EndpointAddressMessageFilterTable<TFilterData>
    {
        private UriPrefixTable<CandidateSet> _toHostTable;
        private UriPrefixTable<CandidateSet> _toNoHostTable;

        public PrefixEndpointAddressMessageFilterTable()
            : base()
        {
        }

        protected override void InitializeLookupTables()
        {
            _toHostTable = new UriPrefixTable<CandidateSet>();
            _toNoHostTable = new UriPrefixTable<CandidateSet>();
        }

        public override void Add(MessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(filter));
            }

            Add((PrefixEndpointAddressMessageFilter)filter, data);
        }

        public override void Add(EndpointAddressMessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(filter));
            }

            Fx.Assert("EndpointAddressMessageFilter cannot be added to PrefixEndpointAddressMessageFilterTable");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException("EndpointAddressMessageFilter cannot be added to PrefixEndpointAddressMessageFilterTable"));
        }

        public void Add(PrefixEndpointAddressMessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(filter));
            }

            filters.Add(filter, data);

            // Create the candidate
            byte[] mask = BuildMask(filter.HeaderLookup);
            Candidate can = new Candidate(filter, data, mask, filter.HeaderLookup);
            candidates.Add(filter, can);

            Uri soapToAddress = filter.Address.Uri;

            if (!TryMatchCandidateSet(soapToAddress, filter.IncludeHostNameInComparison, out CandidateSet cset))
            {
                cset = new CandidateSet();
                GetAddressTable(filter.IncludeHostNameInComparison).RegisterUri(soapToAddress, GetComparisonMode(filter.IncludeHostNameInComparison), cset);
            }
            cset._candidates.Add(can);

            IncrementQNameCount(cset, filter.Address);
        }

        private HostNameComparisonMode GetComparisonMode(bool includeHostNameInComparison)
        {
            return includeHostNameInComparison ? HostNameComparisonMode.Exact : HostNameComparisonMode.StrongWildcard;
        }

        private UriPrefixTable<CandidateSet> GetAddressTable(bool includeHostNameInComparison)
        {
            return includeHostNameInComparison ? _toHostTable : _toNoHostTable;
        }

        internal override bool TryMatchCandidateSet(Uri to, bool includeHostNameInComparison, out CandidateSet cset)
        {
            return GetAddressTable(includeHostNameInComparison).TryLookupUri(to, GetComparisonMode(includeHostNameInComparison), out cset);
        }

        protected override void ClearLookupTables()
        {
            _toHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
            _toNoHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
        }

        public override bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(filter));
            }

            if (filter is PrefixEndpointAddressMessageFilter pFilter)
            {
                return Remove(pFilter);
            }

            return false;
        }

        public override bool Remove(EndpointAddressMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(filter));
            }

            Fx.Assert("EndpointAddressMessageFilter cannot be removed from PrefixEndpointAddressMessageFilterTable");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException("EndpointAddressMessageFilter cannot be removed from PrefixEndpointAddressMessageFilterTable"));
        }

        public bool Remove(PrefixEndpointAddressMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(filter));
            }

            if (!filters.Remove(filter))
            {
                return false;
            }

            Candidate can = candidates[filter];
            Uri soapToAddress = filter.Address.Uri;

            if (TryMatchCandidateSet(soapToAddress, filter.IncludeHostNameInComparison, out CandidateSet cset))
            {
                if (cset._candidates.Count == 1)
                {
                    GetAddressTable(filter.IncludeHostNameInComparison).UnregisterUri(soapToAddress, GetComparisonMode(filter.IncludeHostNameInComparison));
                }
                else
                {
                    DecrementQNameCount(cset, filter.Address);

                    // Remove Candidate
                    cset._candidates.Remove(can);
                }
            }
            candidates.Remove(filter);

            RebuildMasks();
            return true;
        }
    }
}