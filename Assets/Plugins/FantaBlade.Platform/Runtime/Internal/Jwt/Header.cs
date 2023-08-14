using UnityEngine;

namespace FantaBlade.Platform.Internal.Jwt
{
    public class Header
    {
        /// <summary>
        /// Gets the signature algorithm that was used to create the signature.
        /// </summary>
        /// <remarks>If the signature algorithm is not found, null is returned.</remarks>
        public string alg;

        /// <summary>
        /// Gets the content mime type (Cty) of the token.
        /// </summary>
        /// <remarks>If the content mime type is not found, null is returned.</remarks>
        public string cty;

        /// <summary>
        /// Gets the encryption algorithm (Enc) of the token.
        /// </summary>
        /// <remarks>If the content mime type is not found, null is returned.</remarks>
        public string enc;

        /// <summary>
        /// Gets the iv of symmetric key wrap.
        /// </summary>
        public string iv;

        /// <summary>
        /// Gets the key identifier for the security key used to sign the token
        /// </summary>
        public string kid;

        /// <summary>
        /// Gets the mime type (Typ) of the token.
        /// </summary>
        /// <remarks>If the mime type is not found, null is returned.</remarks>
        public string typ;

        /// <summary>
        /// Gets the thumbprint of the certificate used to sign the token
        /// </summary>
        public string x5t;

        /// <summary>
        /// Deserializes Base64UrlEncoded JSON into a <see cref="JwtHeader"/> instance.
        /// </summary>
        /// <param name="base64UrlEncodedJsonString">Base64url encoded JSON to deserialize.</param>
        /// <returns>An instance of <see cref="Header"/>.</returns>
        /// <remarks>Use <see cref="JsonUtility.FromJson"/> to customize JSON serialization.</remarks>
        public static Header Base64UrlDeserialize(string base64UrlEncodedJsonString)
        {
            return JsonUtility.FromJson<Header>(Base64UrlEncoder.Decode(base64UrlEncodedJsonString));
        }

        /// <summary>
        /// Deserialzes JSON into a <see cref="Header"/> instance.
        /// </summary>
        /// <param name="jsonString"> The JSON to deserialize.</param>
        /// <returns>An instance of <see cref="Header"/>.</returns>
        /// <remarks>Use <see cref="JsonUtility.FromJson"/> to customize JSON serialization.</remarks>
        public static Header Deserialize(string jsonString)
        {
            return JsonUtility.FromJson<Header>(jsonString);
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