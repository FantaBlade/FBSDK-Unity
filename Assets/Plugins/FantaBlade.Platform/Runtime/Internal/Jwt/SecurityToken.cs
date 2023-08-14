using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FantaBlade.Platform.Internal.Jwt
{
    public class SecurityToken
    {
        private Payload _payload;

        /// <summary>
        /// Initializes a new instance of <see cref="SecurityToken"/> from a string in JWS Compact serialized format.
        /// </summary>
        /// <param name="encodedString">A JSON Web Token that has been serialized in JWS Compact serialized format.</param>
        /// <exception cref="ArgumentNullException">'EncodedString' is null.</exception>
        /// <exception cref="ArgumentException">'EncodedString' contains only whitespace.</exception>
        /// <exception cref="ArgumentException">'EncodedString' is not in JWS Compact serialized format.</exception>
        /// <remarks>
        /// The contents of this <see cref="SecurityToken"/> have not been validated, the JSON Web Token is simply decoded. Validation can be accomplished using <see cref="SecurityTokenHandler.ValidateToken(String, TokenValidationParameters, out SecurityToken)"/>
        /// </remarks>
        public SecurityToken(string encodedString)
        {
            if (string.IsNullOrEmpty(encodedString) || encodedString.Trim().Length == 0)
                throw new ArgumentNullException("encodedString");

            // Set the maximum number of segments to MaxSegmentCount + 1. This controls the number of splits and allows detecting the number of segments is too large.
            // For example: "a.b.c.d.e.f.g.h" => [a], [b], [c], [d], [e], [f.g.h]. 6 segments.
            // If just MaxSegmentCount was used, then [a], [b], [c], [d], [e.f.g.h] would be returned. 5 segments.
            string[] tokenParts = encodedString.Split(new char[] {'.'}, Constants.MaxJwtSegmentCount + 1);
            if (tokenParts.Length == Constants.JwsSegmentCount)
            {
                if (!Regex.IsMatch(encodedString, Constants.JsonCompactSerializationRegex))
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, LogMessages.IDX10709,
                        encodedString));
            }
            else if (tokenParts.Length == Constants.JweSegmentCount)
            {
                if (!Regex.IsMatch(encodedString, Constants.JweCompactSerializationRegex))
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, LogMessages.IDX10709,
                        encodedString));
            }
            else
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, LogMessages.IDX10709,
                    encodedString));

            Decode(tokenParts, encodedString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityToken"/> class where the <see cref="Header"/> contains the crypto algorithms applied to the encoded <see cref="JwtHeader"/> and <see cref="JwtPayload"/>. The jwtEncodedString is the result of those operations.
        /// </summary>
        /// <param name="header">Contains JSON objects representing the cryptographic operations applied to the JWT and optionally any additional properties of the JWT</param>
        /// <param name="payload">Contains JSON objects representing the claims contained in the JWT. Each claim is a JSON object of the form { Name, Value }</param>
        /// <param name="rawHeader">base64urlencoded JwtHeader</param>
        /// <param name="rawPayload">base64urlencoded JwtPayload</param>
        /// <param name="rawSignature">base64urlencoded JwtSignature</param>
        /// <exception cref="ArgumentNullException">'header' is null.</exception>
        /// <exception cref="ArgumentNullException">'payload' is null.</exception>
        /// <exception cref="ArgumentNullException">'rawSignature' is null.</exception>
        /// <exception cref="ArgumentException">'rawHeader' or 'rawPayload' is null or whitespace.</exception>
        public SecurityToken(Header header, Payload payload, string rawHeader, string rawPayload, string rawSignature)
        {
            if (header == null)
                throw new ArgumentNullException("header");

            if (payload == null)
                throw new ArgumentNullException("payload");

            if (string.IsNullOrEmpty(rawHeader) || rawHeader.Trim().Length == 0)
                throw new ArgumentNullException("rawHeader");

            if (string.IsNullOrEmpty(rawPayload) || rawPayload.Trim().Length == 0)
                throw new ArgumentNullException("rawPayload");

            if (rawSignature == null)
                throw new ArgumentNullException("rawSignature");

            Header = header;
            Payload = payload;
            RawData = string.Concat(rawHeader, ".", rawPayload, ".", rawSignature);

            RawHeader = rawHeader;
            RawPayload = rawPayload;
            RawSignature = rawSignature;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityToken"/> class where the <see cref="Header"/> contains the crypto algorithms applied to the encoded <see cref="Header"/> and <see cref="Payload"/>. The EncodedString is the result of those operations.
        /// </summary>
        /// <param name="header">Contains JSON objects representing the cryptographic operations applied to the  and optionally any additional properties of the </param>
        /// <param name="payload">Contains JSON objects representing the claims contained in the . Each claim is a JSON object of the form { Name, Value }</param>
        /// <exception cref="ArgumentNullException">'header' is null.</exception>
        /// <exception cref="ArgumentNullException">'payload' is null.</exception>
        public SecurityToken(Header header, Payload payload)
        {
            if (header == null)
                throw new ArgumentNullException("header");

            if (payload == null)
                throw new ArgumentNullException("payload");

            Header = header;
            Payload = payload;
            RawSignature = string.Empty;
        }

        /// <summary>
        /// Gets the 'value' of the 'actor' claim { actort, 'value' }.
        /// </summary>
        /// <remarks>If the 'actor' claim is not found, null is returned.</remarks> 
        public string Actor
        {
            get
            {
                if (Payload != null)
                    return Payload.actort;
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the 'value' of 'audience' claim { aud, 'value' }.
        /// </summary>
        /// <remarks>If the 'audience' claim is not found, enumeration will be empty.</remarks>
        public string Audiences
        {
            get
            {
                if (Payload != null)
                    return Payload.aud;
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the <see cref="Header"/> associated with this instance if the token is signed.
        /// </summary>
        public Header Header { get; internal set; }

        /// <summary>
        /// Gets the 'value' of the ' ID' claim { jti, ''value' }.
        /// </summary>
        /// <remarks>If the ' ID' claim is not found, null is returned.</remarks>
        public string Id
        {
            get
            {
                if (Payload != null)
                    return Payload.jti;
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'issuer' claim { iss, 'value' }.
        /// </summary>
        /// <remarks>If the 'issuer' claim is not found, null is returned.</remarks>
        public string Issuer
        {
            get
            {
                if (Payload != null)
                    return Payload.iss;
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the <see cref="Payload"/> associated with this instance.
        /// Note that if this  is nested ( <see cref="SecurityToken.InnerToken"/> != null, this property represnts the payload of the most inner token.
        /// This property can be null if the content type of the most inner token is unrecognized, in that case
        ///  the content of the token is the string returned by PlainText property.
        /// </summary>
        public Payload Payload
        {
            get { return _payload; }
            internal set { _payload = value; }
        }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawAuthenticationTag { get; private set; }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawCiphertext { get; private set; }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawData { get; private set; }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawEncryptedKey { get; private set; }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawInitializationVector { get; private set; }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawHeader { get; internal set; }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawPayload { get; internal set; }

        /// <summary>
        /// Gets the original raw data of this instance when it was created.
        /// </summary>
        /// <remarks>The original JSON Compact serialized format passed to one of the two constructors <see cref="SecurityToken(string)"/>
        /// or <see cref="SecurityToken"/></remarks>
        public string RawSignature { get; internal set; }

        /// <summary>
        /// Gets the signature algorithm associated with this instance.
        /// </summary>
        /// <remarks>If there is a <see cref="SigningCredentials"/> associated with this instance, a value will be returned.  Null otherwise.</remarks>
        public string SignatureAlgorithm
        {
            get { return Header.alg; }
        }

        /// <summary>
        /// Gets the "value" of the 'subject' claim { sub, 'value' }.
        /// </summary>
        /// <remarks>If the 'subject' claim is not found, null is returned.</remarks>
        public string Subject
        {
            get
            {
                if (Payload != null)
                    return Payload.sub;
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'notbefore' claim { nbf, 'value' } converted to a <see cref="DateTime"/> assuming 'value' is seconds since UnixEpoch (UTC 1970-01-01T0:0:0Z).
        /// </summary>
        /// <remarks>If the 'notbefore' claim is not found, then <see cref="DateTime.MinValue"/> is returned.</remarks>
        public DateTime ValidFrom
        {
            get
            {
                if (Payload != null)
                    return Payload.ValidFrom;
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the 'value' of the 'expiration' claim { exp, 'value' } converted to a <see cref="DateTime"/> assuming 'value' is seconds since UnixEpoch (UTC 1970-01-01T0:0:0Z).
        /// </summary>
        /// <remarks>If the 'expiration' claim is not found, then <see cref="DateTime.MinValue"/> is returned.</remarks>
        public DateTime ValidTo
        {
            get
            {
                if (Payload != null)
                    return Payload.ValidTo;
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Serializes the <see cref="Header"/> and <see cref="Payload"/>
        /// </summary>
        /// <returns>A string containing the header and payload in JSON format.</returns>
        public override string ToString()
        {
            if (Payload != null)
                return Header.SerializeToJson() + "." + Payload.SerializeToJson();
            else
                return Header.SerializeToJson() + ".";
        }

        /// <summary>
        /// Decodes the string into the header, payload and signature.
        /// </summary>
        /// <param name="tokenParts">the tokenized string.</param>
        /// <param name="rawData">the original token.</param>
        internal void Decode(string[] tokenParts, string rawData)
        {
            try
            {
                Header = Header.Base64UrlDeserialize(tokenParts[0]);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.InvariantCulture, LogMessages.IDX10729, tokenParts[0], rawData), ex);
            }

            if (tokenParts.Length == Constants.JweSegmentCount)
                DecodeJwe(tokenParts);
            else
                DecodeJws(tokenParts);

            RawData = rawData;
        }

        /// <summary>
        /// Decodes the payload and signature from the JWS parts.
        /// </summary>
        /// <param name="tokenParts">Parts of the JWS including the header.</param>
        /// <remarks>Assumes Header has already been set.</remarks>
        private void DecodeJws(string[] tokenParts)
        {
            // Log if CTY is set, assume compact JWS
            if (Header.cty != null)
                Debug.Log(string.Format(CultureInfo.InvariantCulture, LogMessages.IDX10738, Header.cty));

            try
            {
                Payload = Payload.Base64UrlDeserialize(tokenParts[1]);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, LogMessages.IDX10723, tokenParts[1], RawData), ex);
            }

            RawHeader = tokenParts[0];
            RawPayload = tokenParts[1];
            RawSignature = tokenParts[2];
        }

        /// <summary>
        /// Decodes the payload and signature from the JWE parts.
        /// </summary>
        /// <param name="tokenParts">Parts of the JWE including the header.</param>
        /// <remarks>Assumes Header has already been set.</remarks>
        private void DecodeJwe(string[] tokenParts)
        {
            RawHeader = tokenParts[0];
            RawEncryptedKey = tokenParts[1];
            RawInitializationVector = tokenParts[2];
            RawCiphertext = tokenParts[3];
            RawAuthenticationTag = tokenParts[4];
        }
    }
}