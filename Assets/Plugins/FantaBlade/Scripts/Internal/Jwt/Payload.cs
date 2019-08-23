using System;
using UnityEngine;

namespace FantaBlade.Internal.Jwt
{
    public class Payload
    {
        /// <summary>
        /// Gets the 'value' of the 'actor' claim { actort, 'value' }.
        /// </summary>
        /// <remarks>If the 'actor' claim is not found, null is returned.</remarks>
        public string actort;

        /// <summary>
        /// Gets the 'value' of the 'acr' claim { acr, 'value' }.
        /// </summary>
        /// <remarks>If the 'acr' claim is not found, null is returned.</remarks>
        public string acr;

        /// <summary>
        /// Gets the 'value' of the 'amr' claim { amr, 'value' }.
        /// </summary>
        /// <remarks>If the 'amr' claim is not found, an empty enumerable is returned.</remarks>
        public string amr;

        /// <summary>
        /// Gets the 'value' of the 'auth_time' claim { auth_time, 'value' }.
        /// </summary>
        /// <remarks>If the 'auth_time' claim is not found OR could not be converted to <see cref="System.Int32"/>, null is returned.</remarks>
        public int authtime;

        /// <summary>
        /// Gets the 'value' of the 'audience' claim { aud, 'value' }.
        /// </summary>
        /// <remarks>If the 'audience' claim is not found, an empty enumerable is returned.</remarks>
        public string aud;

        /// <summary>
        /// Gets the 'value' of the 'azp' claim { azp, 'value' }.
        /// </summary>
        /// <remarks>If the 'azp' claim is not found, null is returned.</remarks>
        public string azp;

        /// <summary>
        /// Gets 'value' of the 'c_hash' claim { c_hash, 'value' }.
        /// </summary>
        /// <remarks>If the 'c_hash' claim is not found, null is returned.</remarks>
        public string chash;

        /// <summary>
        /// Gets the 'value' of the 'expiration' claim { exp, 'value' }.
        /// </summary>
        /// <remarks>If the 'expiration' claim is not found OR could not be converted to <see cref="System.Int32"/>, null is returned.</remarks>
        public int exp;

        /// <summary>
        /// Gets the 'value' of the 'JWT ID' claim { jti, 'value' }.
        /// </summary>
        /// <remarks>If the 'JWT ID' claim is not found, null is returned.</remarks>
        public string jti;

        /// <summary>
        /// Gets the 'value' of the 'Issued At' claim { iat, 'value' }.
        /// </summary>
        /// <remarks>If the 'Issued At' claim is not found OR cannot be converted to <see cref="System.Int32"/> null is returned.</remarks>
        public int? iat;

        /// <summary>
        /// Gets the 'value' of the 'issuer' claim { iss, 'value' }.
        /// </summary>
        /// <remarks>If the 'issuer' claim is not found, null is returned.</remarks>
        public string iss;

        /// <summary>
        /// Gets the 'value' of the 'expiration' claim { nbf, 'value' }.
        /// </summary>
        /// <remarks>If the 'notbefore' claim is not found OR could not be converted to <see cref="System.Int32"/>, null is returned.</remarks>
        public int nbf;

        /// <summary>
        /// Gets the 'value' of the 'nonce' claim { nonce, 'value' }.
        /// </summary>
        /// <remarks>If the 'nonce' claim is not found, null is returned.</remarks>
        public string nonce;

        /// <summary>
        /// Gets the 'value' of the 'subject' claim { sub, 'value' }.
        /// </summary>
        /// <remarks>If the 'subject' claim is not found, null is returned.</remarks>
        public string sub;

        /// <summary>
        /// Gets the 'value' of the 'notbefore' claim { nbf, 'value' } converted to a <see cref="DateTime"/> assuming 'value' is seconds since UnixEpoch (UTC 1970-01-01T0:0:0Z).
        /// </summary>
        /// <remarks>If the 'notbefore' claim is not found, then <see cref="DateTime.MinValue"/> is returned. Time is returned as UTC.</remarks>
        public DateTime ValidFrom
        {
            get { return EpochTime.DateTime(nbf); }
        }

        /// <summary>
        /// Gets the 'value' of the 'expiration' claim { exp, 'value' } converted to a <see cref="DateTime"/> assuming 'value' is seconds since UnixEpoch (UTC 1970-01-01T0:0:0Z).
        /// </summary>
        /// <remarks>If the 'expiration' claim is not found, then <see cref="DateTime.MinValue"/> is returned.</remarks>
        public DateTime ValidTo
        {
            get { return EpochTime.DateTime(exp); }
        }

        /// <summary>
        ///     用户类型，0：普通用户，1：游客
        /// </summary>
        public int type;

        /// <summary>
        /// Deserializes Base64UrlEncoded JSON into a <see cref="Payload"/> instance.
        /// </summary>
        /// <param name="base64UrlEncodedJsonString">base64url encoded JSON to deserialize.</param>
        /// <returns>An instance of <see cref="Payload"/>.</returns>
        /// <remarks>Use <see cref="JsonUtility.FromJson"/> to customize JSON serialization.</remarks>
        public static Payload Base64UrlDeserialize(string base64UrlEncodedJsonString)
        {
            return JsonUtility.FromJson<Payload>(Base64UrlEncoder.Decode(base64UrlEncodedJsonString));
        }

        /// <summary>
        /// Deserialzes JSON into a <see cref="Payload"/> instance.
        /// </summary>
        /// <param name="jsonString">The JSON to deserialize.</param>
        /// <returns>An instance of <see cref="Payload"/>.</returns>
        /// <remarks>Use <see cref="JsonUtility.FromJson"/> to customize JSON serialization.</remarks>
        public static Payload Deserialize(string jsonString)
        {
            return JsonUtility.FromJson<Payload>(jsonString);
        }

        /// <summary>
        /// Serializes this instance to JSON.
        /// </summary>
        /// <returns>This instance as JSON.</returns>
        /// <remarks>Use <see cref="JsonUtility.ToJson"/> to customize JSON serialization.</remarks>
        public virtual string SerializeToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}