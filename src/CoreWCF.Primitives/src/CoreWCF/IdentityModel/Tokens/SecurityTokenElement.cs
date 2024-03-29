// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Xml;

namespace CoreWCF.IdentityModel.Tokens
{
    /// <summary>
    /// This class represents a number elements found in a <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken"/> which represent security tokens.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe.
    /// </remarks>
    public class SecurityTokenElement
    {
        private SecurityToken _securityToken;
        private readonly SecurityTokenHandlerCollection _securityTokenHandlers;
        private ReadOnlyCollection<ClaimsIdentity> _subject;

        /// <summary>
        /// Creates an instance of this object using a <see cref="SecurityToken"/> object.
        /// </summary>
        /// <param name="securityToken">The security token this object represents.</param>
        /// <remarks>
        /// <see cref="GetIdentities"/> is not supported by this object if this constructor is used unless
        /// <see cref="ValidateToken"/> is overriden.
        /// If the securityToken passed in is a <see cref="GenericXmlSecurityToken"/> then SecurityTokenXml will 
        /// be set to the value found in <see cref="GenericXmlSecurityToken"/>
        /// </remarks>
        public SecurityTokenElement(SecurityToken securityToken)
        {
            if (securityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(securityToken));
            }

            if (securityToken is GenericXmlSecurityToken xmlToken)
            {
                SecurityTokenXml = xmlToken.TokenXml;
            }

            _securityToken = securityToken;
        }

        /// <summary>
        /// Creates an instance of this object using XML representation of the security token.
        /// </summary>
        /// <param name="securityTokenXml">The <see cref="XmlElement"/> representation of the security token.</param>
        /// <param name="securityTokenHandlers">The collection of <see cref="SecurityTokenHandler"/> objects that may 
        /// be used to read and validate the security token this object represents.</param>
        public SecurityTokenElement(XmlElement securityTokenXml, SecurityTokenHandlerCollection securityTokenHandlers)
        {
            if (securityTokenXml == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(securityTokenXml));
            }

            if (securityTokenHandlers == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(securityTokenHandlers));
            }

            SecurityTokenXml = securityTokenXml;
            _securityTokenHandlers = securityTokenHandlers;
        }

        /// <summary>
        /// Gets the XML representation of the token.
        /// </summary>
        /// <remarks>This property will be null unless this object was constructed using
        /// <see cref="SecurityTokenElement(XmlElement, SecurityTokenHandlerCollection)"/>.
        /// </remarks>
        public XmlElement SecurityTokenXml { get; }

        /// <summary>
        /// Gets the security token this object represents.
        /// </summary>
        /// <remarks>
        /// If this object was not constructed directly with a <see cref="SecurityToken"/> using
        /// <see cref="SecurityTokenElement(SecurityToken)"/>, <see cref="ReadSecurityToken"/>
        /// will be called for this value.
        /// </remarks>
        /// <returns>The <see cref="SecurityToken"/> this object represents</returns>
        public SecurityToken GetSecurityToken()
        {
            if (_securityToken == null)
            {
                _securityToken = ReadSecurityToken(SecurityTokenXml, _securityTokenHandlers);
            }

            return _securityToken;
        }

        /// <summary>
        /// Gets the collection of <see cref="ClaimsIdentity"/> contained in the token.
        /// <seealso cref="ValidateToken"/>
        /// </summary>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        public ReadOnlyCollection<ClaimsIdentity> GetIdentities()
        {
            if (_subject == null)
            {
                _subject = ValidateToken(SecurityTokenXml, _securityTokenHandlers);
            }

            return _subject;
        }

        /// <summary>
        /// Creates the identities for the represented by the <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="securityTokenXml">The <see cref="XmlElement"/> representation of the security token.</param>
        /// <param name="securityTokenHandlers">The collection of <see cref="SecurityTokenHandler"/> objects that may 
        /// be used to read and validate the security token this object represents.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="InvalidOperationException">If either parameter 'securityTokenXml' or 'securityTokenHandlers' are null.</exception>
        protected virtual ReadOnlyCollection<ClaimsIdentity> ValidateToken(XmlElement securityTokenXml, SecurityTokenHandlerCollection securityTokenHandlers)
        {
            if (securityTokenXml == null || securityTokenHandlers == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.Format(SR.ID4052)));
            }

            SecurityToken securityToken = GetSecurityToken();
            return securityTokenHandlers.ValidateToken(securityToken);
        }

        /// <summary>
        /// Reads a <see cref="SecurityToken"/> from the provided XML representation.
        /// </summary>
        /// <param name="securityTokenXml">The XML representation of the security token.</param>
        /// <param name="securityTokenHandlers">The <see cref="SecurityTokenHandlerCollection"/> used to
        /// read the token.</param>
        /// <returns>A <see cref="SecurityToken"/>.</returns>
        protected virtual SecurityToken ReadSecurityToken(XmlElement securityTokenXml,
                                                           SecurityTokenHandlerCollection securityTokenHandlers)
        {
            XmlReader reader = new XmlNodeReader(securityTokenXml);
            reader.MoveToContent();
            SecurityToken securityToken = securityTokenHandlers.ReadToken(reader);
            if (securityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.Format(SR.ID4051, securityTokenXml, reader.LocalName, reader.NamespaceURI)));
            }

            return securityToken;
        }
    }
}
