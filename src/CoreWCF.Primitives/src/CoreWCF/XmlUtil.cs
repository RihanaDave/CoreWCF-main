﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using CoreWCF.IdentityModel.Selectors;
using CoreWCF.IdentityModel.Tokens;
using CoreWCF.Runtime;

namespace CoreWCF
{
    internal static class XmlUtil
    {
        public const string XmlNs = "http://www.w3.org/XML/1998/namespace";
        public const string XmlNsNs = "http://www.w3.org/2000/xmlns/";
        public const string XmlSerializerSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XmlSerializerSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

        public static XmlQualifiedName GetXsiType(XmlReader reader)
        {
            string xsiType = reader.GetAttribute("type", XmlSchema.InstanceNamespace);
            reader.MoveToElement();

            if (string.IsNullOrEmpty(xsiType))
            {
                return null;
            }

            return ResolveQName(reader, xsiType);
        }

        public static bool EqualsQName(XmlQualifiedName qname, string localName, string namespaceUri)
        {
            return null != qname
                && StringComparer.Ordinal.Equals(localName, qname.Name)
                && StringComparer.Ordinal.Equals(namespaceUri, qname.Namespace);
        }

        public static bool IsNil(XmlReader reader)
        {
            string xsiNil = reader.GetAttribute("nil", XmlSchema.InstanceNamespace);
            return !string.IsNullOrEmpty(xsiNil) && XmlConvert.ToBoolean(xsiNil);
        }

        public static string NormalizeEmptyString(string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        public static XmlQualifiedName ResolveQName(XmlReader reader, string qstring)
        {
            string name = qstring;
            string prefix = String.Empty;
            string ns = null;

            int colon = qstring.IndexOf(':'); // index of char is always ordinal
            if (colon > -1)
            {
                prefix = qstring.Substring(0, colon);
                name = qstring.Substring(colon + 1, qstring.Length - (colon + 1));
            }

            ns = reader.LookupNamespace(prefix);

            return new XmlQualifiedName(name, ns);
        }

        public static void ValidateXsiType(XmlReader reader, string expectedTypeName, string expectedTypeNamespace)
        {
            ValidateXsiType(reader, expectedTypeName, expectedTypeNamespace, false);
        }

        public static void ValidateXsiType(XmlReader reader, string expectedTypeName, string expectedTypeNamespace, bool requireDeclaration)
        {
            XmlQualifiedName declaredType = GetXsiType(reader);

            if (null == declaredType)
            {
                if (requireDeclaration)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperXml(reader,
                        SR.Format(SR.ID4104, reader.LocalName, reader.NamespaceURI));
                }
            }
            else if (!(StringComparer.Ordinal.Equals(expectedTypeNamespace, declaredType.Namespace)
                && StringComparer.Ordinal.Equals(expectedTypeName, declaredType.Name)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperXml(reader,
                    SR.Format(SR.ID4102, expectedTypeName, expectedTypeNamespace, declaredType.Name, declaredType.Namespace));
            }
        }

        public static string SerializeSecurityKeyIdentifier(SecurityKeyIdentifier ski, SecurityTokenSerializer tokenSerializer)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    tokenSerializer.WriteKeyIdentifierClause(xmlWriter, ski[0]);
                }
            }

            return sb.ToString();
        }

        public static bool IsValidXmlIDValue(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return false;
            }

            // The first character of the ID should be a letter, '_' or ':'
            return (((val[0] >= 'A') && (val[0] <= 'Z')) ||
                ((val[0] >= 'a') && (val[0] <= 'z')) ||
                (val[0] == '_') || (val[0] == ':'));
        }

        public static string GetXmlLangAttribute(XmlReader reader)
        {
            string xmlLang = null;
            if (reader.MoveToAttribute("lang", XmlNs))
            {
                xmlLang = reader.Value;
                reader.MoveToElement();
            }

            if (xmlLang == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.XmlLangAttributeMissing));
            }

            return xmlLang;
        }

        public static void WriteElementStringAsUniqueId(XmlDictionaryWriter writer, XmlDictionaryString localName, XmlDictionaryString ns, string id)
        {
            writer.WriteStartElement(localName, ns);
            writer.WriteValue(id);
            writer.WriteEndElement();
        }

        public static void WriteElementContentAsInt64(XmlDictionaryWriter writer, XmlDictionaryString localName, XmlDictionaryString ns, Int64 value)
        {
            writer.WriteStartElement(localName, ns);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        public static Int64 ReadElementContentAsInt64(XmlDictionaryReader reader)
        {
            reader.ReadFullStartElement();
            Int64 i = reader.ReadContentAsLong();
            reader.ReadEndElement();
            return i;
        }

        // Takes a collection of node list and returns a list of XmlElements
        // from the list (skipping past any XmlComments and CDATA nodes).
        public static List<XmlElement> GetXmlElements(XmlNodeList nodeList)
        {
            if (nodeList == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(nodeList));
            }

            List<XmlElement> xmlElements = new List<XmlElement>();
            foreach (XmlNode node in nodeList)
            {
                XmlElement tempElement = node as XmlElement;
                if (tempElement != null)
                {
                    xmlElements.Add(tempElement);
                }
            }

            return xmlElements;
        }

        public static void ReadContentAsQName(XmlReader reader, out string localName, out string ns)
        {
            ParseQName(reader, reader.ReadContentAsString(), out localName, out ns);
        }

        public static bool IsWhitespace(char ch)
        {
            return (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n');
        }

        public static string TrimEnd(string s)
        {
            int i;
            for (i = s.Length; i > 0 && IsWhitespace(s[i - 1]); i--)
            {
                ;
            }

            if (i != s.Length)
            {
                return s.Substring(0, i);
            }

            return s;
        }

        public static string TrimStart(string s)
        {
            int i;
            for (i = 0; i < s.Length && IsWhitespace(s[i]); i++)
            {
                ;
            }

            if (i != 0)
            {
                return s.Substring(i);
            }

            return s;
        }

        public static string Trim(string s)
        {
            int i;
            for (i = 0; i < s.Length && IsWhitespace(s[i]); i++)
            {
                ;
            }

            if (i >= s.Length)
            {
                return string.Empty;
            }

            int j;
            for (j = s.Length; j > 0 && IsWhitespace(s[j - 1]); j--)
            {
                ;
            }

            Fx.Assert(j > i, "Logic error in XmlUtil.Trim().");

            if (i != 0 || j != s.Length)
            {
                return s.Substring(i, j - i);
            }
            return s;
        }

        public static void ParseQName(XmlReader reader, string qname, out string localName, out string ns)
        {
            int index = qname.IndexOf(':');
            string prefix;
            if (index < 0)
            {
                prefix = "";
                localName = TrimStart(TrimEnd(qname));
            }
            else
            {
                if (index == qname.Length - 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.Format(SR.InvalidXmlQualifiedName, qname)));
                }

                prefix = TrimStart(qname.Substring(0, index));
                localName = TrimEnd(qname.Substring(index + 1));
            }
            ns = reader.LookupNamespace(prefix);
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.Format(SR.UnboundPrefixInQName, qname)));
            }
        }

        // This code was copied from XmlDictionaryReader.ReadElementContentAsDateTime which is an internal method
        public static DateTime ReadElementContentAsDateTime(this XmlDictionaryReader reader)
        {
            bool isEmptyElement = reader.IsStartElement() && reader.IsEmptyElement;
            DateTime value;

            if (isEmptyElement)
            {
                reader.Read();
                try
                {
                    value = DateTime.Parse(string.Empty, NumberFormatInfo.InvariantInfo);
                }
                catch (ArgumentException exception)
                {
                    throw new XmlException(SR.Format(SR.XmlInvalidConversion, string.Empty, "DateTime"), exception);
                }
                catch (FormatException exception)
                {
                    throw new XmlException(SR.Format(SR.XmlInvalidConversion, string.Empty, "DateTime"), exception);
                }
            }
            else
            {
                reader.ReadStartElement();
                value = reader.ReadContentAsDateTimeOffset().DateTime;
                reader.ReadEndElement();
            }

            return value;
        }
    }
}
