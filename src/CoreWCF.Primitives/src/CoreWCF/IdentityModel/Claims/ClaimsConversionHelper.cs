// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace CoreWCF.Security.Claims
{
    internal static class ClaimsConversionHelper
    {
        public static ClaimsIdentity CreateClaimsIdentityFromClaimSet(CoreWCF.IdentityModel.Claims.ClaimSet claimset, string authenticationType)
        {
            if (claimset == null)
            {
                throw new ArgumentNullException(nameof(claimset));
            }

            string issuer = null;
            if (claimset.Issuer == null)
            {
                issuer = ClaimsIdentity.DefaultIssuer;
            }
            else
            {
                foreach (CoreWCF.IdentityModel.Claims.Claim claim in claimset.Issuer.FindClaims(CoreWCF.IdentityModel.Claims.ClaimTypes.Name, CoreWCF.IdentityModel.Claims.Rights.Identity))
                {
                    if ((claim != null) && (claim.Resource is string))
                    {
                        issuer = claim.Resource as string;
                        break;
                    }
                }
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(authenticationType);

            for (int i = 0; i < claimset.Count; ++i)
            {
                //
                // Only capture possesses property claims
                //
                if (String.Equals(claimset[i].Right, CoreWCF.IdentityModel.Claims.Rights.PossessProperty, StringComparison.Ordinal))
                {
                    claimsIdentity.AddClaim(CreateClaimFromWcfClaim(claimset[i], issuer));
                }
            }

            return claimsIdentity;
        }

        public static ClaimsIdentity CreateClaimsIdentityFromClaimSet(CoreWCF.IdentityModel.Claims.ClaimSet claimset)
        {
            return CreateClaimsIdentityFromClaimSet(claimset, null);
        }

        public static System.Security.Claims.Claim CreateClaimFromWcfClaim(CoreWCF.IdentityModel.Claims.Claim wcfClaim)
        {
            return CreateClaimFromWcfClaim(wcfClaim, null);
        }

        public static System.Security.Claims.Claim CreateClaimFromWcfClaim(CoreWCF.IdentityModel.Claims.Claim wcfClaim, string issuer)
        {
            string claimType = null;
            string value = null;
            string valueType = ClaimValueTypes.String;
            string originalIssuer = issuer;
            string samlNameIdentifierFormat = null;
            string samlNameIdentifierNameQualifier = null;

            if (wcfClaim == null)
            {
                throw new ArgumentNullException(nameof(wcfClaim));
            }

            if (wcfClaim.Resource == null)
            {
                throw new InvalidOperationException();
            }

            if (string.IsNullOrEmpty(issuer))
            {
                issuer = ClaimsIdentity.DefaultIssuer;
            }

            if (wcfClaim.Resource is string)
            {
                AssignClaimFromStringResourceSysClaim(wcfClaim, out claimType, out value);
            }
            else
            {
                AssignClaimFromSysClaim(wcfClaim, out claimType, out value, out valueType, out samlNameIdentifierFormat, out samlNameIdentifierNameQualifier);
            }

            if (value == null)
            {
                throw new InvalidOperationException();
            }

            System.Security.Claims.Claim newClaim = new System.Security.Claims.Claim(claimType, value, valueType, issuer, originalIssuer);
            newClaim.Properties[ClaimProperties.SamlNameIdentifierFormat] = samlNameIdentifierFormat;
            newClaim.Properties[ClaimProperties.SamlNameIdentifierNameQualifier] = samlNameIdentifierNameQualifier;
            return newClaim;
        }

        private static void AssignClaimFromStringResourceSysClaim(CoreWCF.IdentityModel.Claims.Claim claim, out string claimType, out string claimValue)
        {
            claimType = claim.ClaimType;
            claimValue = (string)claim.Resource;

            if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.Sid))
            {
                if (claim.Right == CoreWCF.IdentityModel.Claims.Rights.Identity)
                {
                    claimType = ClaimTypes.PrimarySid;
                }
                else
                {
                    claimType = ClaimTypes.GroupSid;
                }
            }
        }

        private static void AssignClaimFromSysClaim(CoreWCF.IdentityModel.Claims.Claim claim, out string _type, out string _value, out string _valueType, out string samlNameIdentifierFormat, out string samlNameIdentifierNameQualifier)
        {
            samlNameIdentifierFormat = null;
            samlNameIdentifierNameQualifier = null;
            _type = null;
            _value = null;
            _valueType = null;

            if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.Sid) && claim.Resource is SecurityIdentifier)
            {
                if (claim.Right == CoreWCF.IdentityModel.Claims.Rights.Identity)
                {
                    _type = ClaimTypes.PrimarySid;
                }
                else
                {
                    _type = ClaimTypes.GroupSid;
                }
                _value = ((SecurityIdentifier)claim.Resource).Value;
            }
            else if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.Email) && claim.Resource is MailAddress)
            {
                _type = claim.ClaimType;
                _value = ((MailAddress)claim.Resource).Address;
            }
            else if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.Thumbprint) && claim.Resource is byte[])
            {
                _type = claim.ClaimType;
                _value = Convert.ToBase64String(((byte[])claim.Resource));
                _valueType = ClaimValueTypes.Base64Binary;
            }
            else if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.Hash) && claim.Resource is byte[])
            {
                _type = claim.ClaimType;
                _value = Convert.ToBase64String(((byte[])claim.Resource));
                _valueType = ClaimValueTypes.Base64Binary;
            }
            else if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.X500DistinguishedName) && claim.Resource is X500DistinguishedName)
            {
                _type = claim.ClaimType;
                _value = ((X500DistinguishedName)claim.Resource).Name;
                _valueType = ClaimValueTypes.X500Name;
            }
            else if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.Uri) && claim.Resource is Uri)
            {
                _type = claim.ClaimType;
                _value = ((Uri)claim.Resource).ToString();
            }
            else if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.Rsa) && claim.Resource is RSA)
            {
                _type = claim.ClaimType;
                _value = ((RSA)claim.Resource).ToXmlString(false);
                _valueType = ClaimValueTypes.RsaKeyValue;
            }
            else if (StringComparer.Ordinal.Equals(claim.ClaimType, ClaimTypes.DenyOnlySid) && claim.Resource is SecurityIdentifier)
            {
                _type = claim.ClaimType;
                _value = ((SecurityIdentifier)claim.Resource).Value;
            }
        }
    }
}

