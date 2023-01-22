using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace StudioDrydock.AppStoreConnect.Api
{
    public partial class AppStoreClient
    {
        readonly Uri baseUri = new Uri("https://api.appstoreconnect.apple.com");
        HttpClient client;

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

        async Task SendAsync(HttpRequestMessage request)
        {
            Trace.TraceInformation($"{request.Method} {request.RequestUri}");
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Status code {response.StatusCode}");
        }

        async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            Trace.TraceInformation($"{request.Method} {request.RequestUri}");
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Status code {response.StatusCode}");

            string responseText = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<T>(responseText);
            if (responseObject == null)
                throw new Exception($"Deserialization failed");

            return responseObject;
        }
    }
}