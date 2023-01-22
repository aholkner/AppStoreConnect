using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace StudioDrydock.AppStoreConnect.Api
{
    public partial class AppStoreClient
    {
        readonly Uri baseUri = new Uri("https://api.appstoreconnect.apple.com");
        HttpClient client;

        readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            // Null fields must not be serialized in requests, as the App Store API requires
            // that certain fields are not submitted if not updating (for example, whatsNew
            // text for an initial version).
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public AppStoreClient(TextReader privateKey, string keyId, string issuerId)
        {
            string token = CreateTokenAndSign(privateKey, keyId, issuerId, "appstoreconnect-v1");

            client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // https://github.com/dersia/AppStoreConnect/blob/main/src/AppStoreConnect.Jwt/KeyUtils.cs
        static void GetPrivateKey(TextReader reader, ECDsa ecDSA)
        {
            var ecPrivateKeyParameters = (ECPrivateKeyParameters)new PemReader(reader).ReadObject();
            var q = ecPrivateKeyParameters.Parameters.G.Multiply(ecPrivateKeyParameters.D);
            var pub = new ECPublicKeyParameters(ecPrivateKeyParameters.AlgorithmName, q, ecPrivateKeyParameters.PublicKeyParamSet);
            var x = pub.Q.AffineXCoord.GetEncoded();
            var y = pub.Q.AffineYCoord.GetEncoded();
            var d = ecPrivateKeyParameters.D.ToByteArrayUnsigned();
            var msEcp = new ECParameters { Curve = ECCurve.NamedCurves.nistP256, Q = { X = x, Y = y }, D = d };
            msEcp.Validate();
            ecDSA.ImportParameters(msEcp);
        }

        // https://github.com/dersia/AppStoreConnect/blob/main/src/AppStoreConnect.Jwt/KeyUtils.cs
        string CreateTokenAndSign(TextReader reader, string kid, string issuer, string audience, TimeSpan timeout = default)
        {
            if (timeout == default)
            {
                timeout = TimeSpan.FromMinutes(20);
            }
            else if (timeout.TotalMinutes > 20)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }
            using var ecDSA = ECDsa.Create();
            GetPrivateKey(reader, ecDSA);

            var securityKey = new ECDsaSecurityKey(ecDSA) { KeyId = kid };
            var credentials = new SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.EcdsaSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.Add(timeout),
                TokenType = "JWT",
                SigningCredentials = credentials
            };

            var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
            var encodedToken = handler.CreateToken(descriptor);
            return encodedToken;
        }

        StringContent Serialize(object obj)
        {
            string text = JsonSerializer.Serialize(obj, options: jsonSerializerOptions);
            return new StringContent(text, encoding: Encoding.UTF8, mediaType: "application/json");
        }

        async Task SendAsync(HttpRequestMessage request)
        {
            Trace.TraceInformation($"{request.Method} {request.RequestUri}");
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceError(await response.Content.ReadAsStringAsync());
                throw new Exception($"Status code {response.StatusCode}");
            }
        }

        async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            Trace.TraceInformation($"{request.Method} {request.RequestUri}");
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceError(await response.Content.ReadAsStringAsync());
                throw new Exception($"Status code {response.StatusCode}");
            }

            string responseText = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<T>(responseText);
            if (responseObject == null)
            {
                Trace.TraceError(await response.Content.ReadAsStringAsync());
                throw new Exception($"Deserialization failed");
            }

            return responseObject;
        }
    }
}