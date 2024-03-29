// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace CoreWCF.IdentityModel.Tokens
{
    /// <summary>
    /// This class defines the encrypting credentials which can be used to 
    /// encrypt the proof key. It is very similar to SigningCredentials class defined 
    /// in System.IdentityModel.dll
    /// </summary>
    public class EncryptingCredentials
    {
        private string _algorithm;
        private SecurityKey _key;
        private SecurityKeyIdentifier _keyIdentifier;

        /// <summary>
        /// Constructor for easy subclassing.
        /// </summary>
        public EncryptingCredentials()
        {
        }

        /// <summary>
        /// Constructs an EncryptingCredentials with a security key, a security key identifier and
        /// the encryption algorithm.
        /// </summary>
        /// <param name="key">A security key for encryption.</param>
        /// <param name="keyIdentifier">A security key identifier for the encryption key.</param>
        /// <param name="algorithm">The encryption algorithm.</param>
        /// <exception cref="ArgumentNullException">When key is null.</exception>
        /// <exception cref="ArgumentNullException">When key identifier is null.</exception>
        /// <exception cref="ArgumentNullException">When algorithm is null.</exception>
        public EncryptingCredentials(SecurityKey key, SecurityKeyIdentifier keyIdentifier, string algorithm)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(key));
            }

            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(keyIdentifier));
            }

            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(algorithm));
            }

            //
            // It is possible that keyIdentifier is pointing to a token which 
            // is not capable of doing the given algorithm, we have no way verify 
            // that at this level.
            //
            _algorithm = algorithm;
            _key = key;
            _keyIdentifier = keyIdentifier;
        }

        /// <summary>
        /// Gets or sets the encryption algorithm.
        /// </summary>
        public string Algorithm
        {
            get
            {
                return _algorithm;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(value));
                }

                _algorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets the encryption key material.
        /// </summary>
        public SecurityKey SecurityKey
        {
            get
            {
                return _key;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(value));
                }

                _key = value;
            }
        }

        /// <summary>
        /// Gets or sets the SecurityKeyIdentifier that identifies the encrypting credential.
        /// </summary>
        public SecurityKeyIdentifier SecurityKeyIdentifier
        {
            get
            {
                return _keyIdentifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(value));
                }

                _keyIdentifier = value;
            }
        }
    }
}
