﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Xml;
using CoreWCF.Runtime;

namespace CoreWCF.Channels
{
    // TODO: This needed to be made public for NetTcp, investigate making it internal again
    public abstract class AddressingHeader : DictionaryHeader, IMessageHeaderWithSharedNamespace
    {
        protected AddressingHeader(AddressingVersion version)
        {
            Version = version;
        }

        internal AddressingVersion Version { get; }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedPrefix
        {
            get { return XD.AddressingDictionary.Prefix; }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedNamespace
        {
            get { return Version.DictionaryNamespace; }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get { return Version.DictionaryNamespace; }
        }
    }

    internal class ActionHeader : AddressingHeader
    {
        private const bool mustUnderstandValue = true;

        private ActionHeader(string action, AddressingVersion version)
            : base(version)
        {
            Action = action;
        }

        public string Action { get; }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.Action; }
        }

        public static ActionHeader Create(string action, AddressingVersion addressingVersion)
        {
            if (action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(action));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            return new ActionHeader(action, addressingVersion);
        }

        public static ActionHeader Create(XmlDictionaryString dictionaryAction, AddressingVersion addressingVersion)
        {
            if (dictionaryAction == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(Action));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            return new DictionaryActionHeader(dictionaryAction, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(Action);
        }

        public static string ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion addressingVersion)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.Action, addressingVersion.DictionaryNamespace), "");
            string act = reader.ReadElementContentAsString();

            if (act.Length > 0 && (act[0] <= 32 || act[act.Length - 1] <= 32))
            {
                act = XmlUtil.Trim(act);
            }

            return act;
        }

        public static ActionHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            string action = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new ActionHeader(action, version);
            }
            else
            {
                return new FullActionHeader(action, actor, mustUnderstand, relay, version);
            }
        }

        private class DictionaryActionHeader : ActionHeader
        {
            private readonly XmlDictionaryString _dictionaryAction;

            public DictionaryActionHeader(XmlDictionaryString dictionaryAction, AddressingVersion version)
                : base(dictionaryAction.Value, version)
            {
                _dictionaryAction = dictionaryAction;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(_dictionaryAction);
            }
        }

        private class FullActionHeader : ActionHeader
        {
            private readonly string _actor;
            private readonly bool _mustUnderstand;
            private readonly bool _relay;

            public FullActionHeader(string action, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(action, version)
            {
                _actor = actor;
                _mustUnderstand = mustUnderstand;
                _relay = relay;
            }

            public override string Actor
            {
                get { return _actor; }
            }

            public override bool MustUnderstand
            {
                get { return _mustUnderstand; }
            }

            public override bool Relay
            {
                get { return _relay; }
            }
        }
    }

    internal class FromHeader : AddressingHeader
    {
        private const bool mustUnderstandValue = false;

        private FromHeader(EndpointAddress from, AddressingVersion version)
            : base(version)
        {
            From = from;
        }

        public EndpointAddress From { get; }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.From; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public static FromHeader Create(EndpointAddress from, AddressingVersion addressingVersion)
        {
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(from));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            return new FromHeader(from, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            From.WriteContentsTo(Version, writer);
        }

        public static FromHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress from = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new FromHeader(from, version);
            }
            else
            {
                return new FullFromHeader(from, actor, mustUnderstand, relay, version);
            }
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion addressingVersion)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.From, addressingVersion.DictionaryNamespace), "");
            return EndpointAddress.ReadFrom(addressingVersion, reader);
        }

        private class FullFromHeader : FromHeader
        {
            private readonly string _actor;
            private readonly bool _mustUnderstand;
            private readonly bool _relay;

            public FullFromHeader(EndpointAddress from, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(from, version)
            {
                _actor = actor;
                _mustUnderstand = mustUnderstand;
                _relay = relay;
            }

            public override string Actor
            {
                get { return _actor; }
            }

            public override bool MustUnderstand
            {
                get { return _mustUnderstand; }
            }

            public override bool Relay
            {
                get { return _relay; }
            }
        }
    }

    internal class FaultToHeader : AddressingHeader
    {
        private const bool mustUnderstandValue = false;

        private FaultToHeader(EndpointAddress faultTo, AddressingVersion version)
            : base(version)
        {
            FaultTo = faultTo;
        }

        public EndpointAddress FaultTo { get; }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.FaultTo; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            FaultTo.WriteContentsTo(Version, writer);
        }

        public static FaultToHeader Create(EndpointAddress faultTo, AddressingVersion addressingVersion)
        {
            if (faultTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(faultTo));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            return new FaultToHeader(faultTo, addressingVersion);
        }

        public static FaultToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress faultTo = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new FaultToHeader(faultTo, version);
            }
            else
            {
                return new FullFaultToHeader(faultTo, actor, mustUnderstand, relay, version);
            }
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.FaultTo, version.DictionaryNamespace), "");
            return EndpointAddress.ReadFrom(version, reader);
        }

        private class FullFaultToHeader : FaultToHeader
        {
            private readonly string _actor;
            private readonly bool _mustUnderstand;
            private readonly bool _relay;

            public FullFaultToHeader(EndpointAddress faultTo, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(faultTo, version)
            {
                _actor = actor;
                _mustUnderstand = mustUnderstand;
                _relay = relay;
            }

            public override string Actor
            {
                get { return _actor; }
            }

            public override bool MustUnderstand
            {
                get { return _mustUnderstand; }
            }

            public override bool Relay
            {
                get { return _relay; }
            }
        }
    }

    // TODO: This needed to be made public for NetTcp, investigate making it internal again
    public class ToHeader : AddressingHeader
    {
        private const bool mustUnderstandValue = true;
        private static ToHeader s_anonymousToHeader10;
        private static ToHeader s_anonymousToHeader200408;

        protected ToHeader(Uri to, AddressingVersion version)
            : base(version)
        {
            To = to;
        }

        private static ToHeader AnonymousTo10
        {
            get
            {
                if (s_anonymousToHeader10 == null)
                {
                    s_anonymousToHeader10 = new AnonymousToHeader(AddressingVersion.WSAddressing10);
                }

                return s_anonymousToHeader10;
            }
        }

        static ToHeader AnonymousTo200408
        {
            get
            {
                if (s_anonymousToHeader200408 == null)
                    s_anonymousToHeader200408 = new AnonymousToHeader(AddressingVersion.WSAddressingAugust2004);
                return s_anonymousToHeader200408;
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.To; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public Uri To { get; }

        public static ToHeader Create(Uri toUri, XmlDictionaryString dictionaryTo, AddressingVersion addressingVersion)
        {
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            if (((object)toUri == (object)addressingVersion.AnonymousUri))
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    return AnonymousTo10;
                }
                else
                {
                    return AnonymousTo200408;
                }
            }
            else
            {
                return new DictionaryToHeader(toUri, dictionaryTo, addressingVersion);
            }
        }

        public static ToHeader Create(Uri to, AddressingVersion addressingVersion)
        {
            if ((object)to == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(to));
            }
            else if ((object)to == addressingVersion.AnonymousUri)
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    return AnonymousTo10;
                }
                else
                {
                    return AnonymousTo200408;
                }
            }
            else
            {
                return new ToHeader(to, addressingVersion);
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(To.AbsoluteUri);
        }

        public static Uri ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            return ReadHeaderValue(reader, version, null);
        }

        internal static Uri ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version, UriCache uriCache)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.To, version.DictionaryNamespace), "");

            string toString = reader.ReadElementContentAsString();

            if ((object)toString == (object)version.Anonymous)
            {
                return version.AnonymousUri;
            }

            if (uriCache == null)
            {
                return new Uri(toString);
            }

            return uriCache.CreateUri(toString);
        }

        internal static ToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, UriCache uriCache,
            string actor, bool mustUnderstand, bool relay)
        {
            Uri to = ReadHeaderValue(reader, version, uriCache);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                if ((object)to == (object)version.AnonymousUri)
                {
                    if (version == AddressingVersion.WSAddressing10)
                    {
                        return AnonymousTo10;
                    }
                    else
                    {
                        throw new PlatformNotSupportedException($"Unsupported addressing version {version}");
                    }
                    //return AnonymousTo200408;
                }
                else
                {
                    return new ToHeader(to, version);
                }
            }
            else
            {
                return new FullToHeader(to, actor, mustUnderstand, relay, version);
            }
        }

        private class AnonymousToHeader : ToHeader
        {
            public AnonymousToHeader(AddressingVersion version)
                : base(version.AnonymousUri, version)
            {
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(Version.DictionaryAnonymous);
            }
        }

        private class DictionaryToHeader : ToHeader
        {
            private readonly XmlDictionaryString _dictionaryTo;

            public DictionaryToHeader(Uri to, XmlDictionaryString dictionaryTo, AddressingVersion version)
                : base(to, version)
            {
                _dictionaryTo = dictionaryTo;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(_dictionaryTo);
            }
        }

        private class FullToHeader : ToHeader
        {
            private readonly string _actor;
            private readonly bool _mustUnderstand;
            private readonly bool _relay;

            public FullToHeader(Uri to, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(to, version)
            {
                _actor = actor;
                _mustUnderstand = mustUnderstand;
                _relay = relay;
            }

            public override string Actor
            {
                get { return _actor; }
            }

            public override bool MustUnderstand
            {
                get { return _mustUnderstand; }
            }

            public override bool Relay
            {
                get { return _relay; }
            }
        }
    }

    internal class ReplyToHeader : AddressingHeader
    {
        private const bool mustUnderstandValue = false;
        private static ReplyToHeader s_anonymousReplyToHeader10;
        private static ReplyToHeader s_anonymousReplyToHeader200408;

        private ReplyToHeader(EndpointAddress replyTo, AddressingVersion version)
            : base(version)
        {
            ReplyTo = replyTo;
        }

        public EndpointAddress ReplyTo { get; }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.ReplyTo; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public static ReplyToHeader AnonymousReplyTo10
        {
            get
            {
                if (s_anonymousReplyToHeader10 == null)
                {
                    s_anonymousReplyToHeader10 = new ReplyToHeader(EndpointAddress.AnonymousAddress, AddressingVersion.WSAddressing10);
                }

                return s_anonymousReplyToHeader10;
            }
        }

        public static ReplyToHeader AnonymousReplyTo200408
        {
            get
            {
                if (s_anonymousReplyToHeader200408 == null)
                    s_anonymousReplyToHeader200408 = new ReplyToHeader(EndpointAddress.AnonymousAddress, AddressingVersion.WSAddressingAugust2004);
                return s_anonymousReplyToHeader200408;
            }
        }

        public static ReplyToHeader Create(EndpointAddress replyTo, AddressingVersion addressingVersion)
        {
            if (replyTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(replyTo));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            return new ReplyToHeader(replyTo, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            ReplyTo.WriteContentsTo(Version, writer);
        }

        public static ReplyToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress replyTo = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                if ((object)replyTo == EndpointAddress.AnonymousAddress)
                {
                    if (version == AddressingVersion.WSAddressing10)
                    {
                        return AnonymousReplyTo10;
                    }
                    else
                    {
                        return AnonymousReplyTo200408;
                    }
                }
                return new ReplyToHeader(replyTo, version);
            }
            else
            {
                return new FullReplyToHeader(replyTo, actor, mustUnderstand, relay, version);
            }
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.ReplyTo, version.DictionaryNamespace), "");
            return EndpointAddress.ReadFrom(version, reader);
        }

        private class FullReplyToHeader : ReplyToHeader
        {
            private readonly string _actor;
            private readonly bool _mustUnderstand;
            private readonly bool _relay;

            public FullReplyToHeader(EndpointAddress replyTo, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(replyTo, version)
            {
                _actor = actor;
                _mustUnderstand = mustUnderstand;
                _relay = relay;
            }

            public override string Actor
            {
                get { return _actor; }
            }

            public override bool MustUnderstand
            {
                get { return _mustUnderstand; }
            }

            public override bool Relay
            {
                get { return _relay; }
            }
        }
    }

    internal class MessageIDHeader : AddressingHeader
    {
        private const bool mustUnderstandValue = false;

        private MessageIDHeader(UniqueId messageId, AddressingVersion version)
            : base(version)
        {
            MessageId = messageId;
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.MessageId; }
        }

        public UniqueId MessageId { get; }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public static MessageIDHeader Create(UniqueId messageId, AddressingVersion addressingVersion)
        {
            if (ReferenceEquals(messageId, null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(messageId));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            return new MessageIDHeader(messageId, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteValue(MessageId);
        }

        public static UniqueId ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.MessageId, version.DictionaryNamespace), "");
            return reader.ReadElementContentAsUniqueId();
        }

        public static MessageIDHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            UniqueId messageId = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new MessageIDHeader(messageId, version);
            }
            else
            {
                return new FullMessageIDHeader(messageId, actor, mustUnderstand, relay, version);
            }
        }

        private class FullMessageIDHeader : MessageIDHeader
        {
            private readonly string _actor;
            private readonly bool _mustUnderstand;
            private readonly bool _relay;

            public FullMessageIDHeader(UniqueId messageId, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(messageId, version)
            {
                _actor = actor;
                _mustUnderstand = mustUnderstand;
                _relay = relay;
            }

            public override string Actor
            {
                get { return _actor; }
            }

            public override bool MustUnderstand
            {
                get { return _mustUnderstand; }
            }

            public override bool Relay
            {
                get { return _relay; }
            }
        }
    }

    internal class RelatesToHeader : AddressingHeader
    {
        private const bool mustUnderstandValue = false;
        internal static readonly Uri ReplyRelationshipType = new Uri(Addressing10Strings.ReplyRelationship);

        private RelatesToHeader(UniqueId messageId, AddressingVersion version)
            : base(version)
        {
            UniqueId = messageId;
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.RelatesTo; }
        }

        public UniqueId UniqueId { get; }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public virtual Uri RelationshipType
        {
            get { return ReplyRelationshipType; }
        }

        public static RelatesToHeader Create(UniqueId messageId, AddressingVersion addressingVersion)
        {
            if (ReferenceEquals(messageId, null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(messageId));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            return new RelatesToHeader(messageId, addressingVersion);
        }

        public static RelatesToHeader Create(UniqueId messageId, AddressingVersion addressingVersion, Uri relationshipType)
        {
            if (ReferenceEquals(messageId, null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(messageId));
            }

            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(addressingVersion));
            }

            if (relationshipType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(relationshipType));
            }

            if (relationshipType == ReplyRelationshipType)
            {
                return new RelatesToHeader(messageId, addressingVersion);
            }
            else
            {
                return new FullRelatesToHeader(messageId, "", false, false, addressingVersion);
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteValue(UniqueId);
        }

        public static void ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version, out Uri relationshipType, out UniqueId messageId)
        {
            AddressingDictionary addressingDictionary = XD.AddressingDictionary;

            // The RelationshipType attribute has no namespace.
            relationshipType = ReplyRelationshipType;
            /*
            string relation = reader.GetAttribute(addressingDictionary.RelationshipType, addressingDictionary.Empty);
            if (relation == null)
            {
                relationshipType = ReplyRelationshipType;
            }
            else
            {
                relationshipType = new Uri(relation);
            }
            */
            Fx.Assert(reader.IsStartElement(addressingDictionary.RelatesTo, version.DictionaryNamespace), "");
            messageId = reader.ReadElementContentAsUniqueId();
        }

        public static RelatesToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            ReadHeaderValue(reader, version, out Uri relationship, out UniqueId messageId);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay && (object)relationship == (object)ReplyRelationshipType)
            {
                return new RelatesToHeader(messageId, version);
            }
            else
            {
                return new FullRelatesToHeader(messageId, actor, mustUnderstand, relay, version);
            }
        }

        private class FullRelatesToHeader : RelatesToHeader
        {
            private readonly string _actor;
            private readonly bool _mustUnderstand;
            private readonly bool _relay;
            //Uri relationship;

            public FullRelatesToHeader(UniqueId messageId, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(messageId, version)
            {
                //this.relationship = relationship;
                _actor = actor;
                _mustUnderstand = mustUnderstand;
                _relay = relay;
            }

            public override string Actor
            {
                get { return _actor; }
            }

            public override bool MustUnderstand
            {
                get { return _mustUnderstand; }
            }

            /*
            public override Uri RelationshipType
            {
                get { return relationship; }
            }
            */

            public override bool Relay
            {
                get { return _relay; }
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                /*
                if ((object)relationship != (object)ReplyRelationshipType)
                {
                    // The RelationshipType attribute has no namespace.
                    writer.WriteStartAttribute(AddressingStrings.RelationshipType, AddressingStrings.Empty);
                    writer.WriteString(relationship.AbsoluteUri);
                    writer.WriteEndAttribute();
                }
                */
                writer.WriteValue(UniqueId);
            }
        }
    }
}
