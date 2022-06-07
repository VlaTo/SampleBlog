using System.Security.Cryptography;

namespace SampleBlog.Identity.Authorization.Configuration;

internal partial class SigningKeysLoader
{
    private class RSAKeyParameters
    {
        public string D { get; set; }
        public string DP { get; set; }
        public string DQ { get; set; }
        public string E { get; set; }
        public string IQ { get; set; }
        public string M { get; set; }
        public string P { get; set; }
        public string Q { get; set; }

        public static RSAKeyParameters Create()
        {
            using (var rsa = RSA.Create())
            {
                if (rsa is RSACryptoServiceProvider && rsa.KeySize < 2048)
                {
                    rsa.KeySize = 2048;

                    if (rsa.KeySize < 2048)
                    {
                        throw new InvalidOperationException("We can't generate an RSA key with at least 2048 bits. Key generation is not supported in this system.");
                    }
                }

                return GetParameters(rsa);
            }
        }

        public static RSAKeyParameters GetParameters(RSA key)
        {
            var result = new RSAKeyParameters();
            var rawParameters = key.ExportParameters(includePrivateParameters: true);

            if (rawParameters.D != null)
            {
                result.D = Convert.ToBase64String(rawParameters.D);
            }

            if (rawParameters.DP != null)
            {
                result.DP = Convert.ToBase64String(rawParameters.DP);
            }

            if (rawParameters.DQ != null)
            {
                result.DQ = Convert.ToBase64String(rawParameters.DQ);
            }

            if (rawParameters.Exponent != null)
            {
                result.E = Convert.ToBase64String(rawParameters.Exponent);
            }

            if (rawParameters.InverseQ != null)
            {
                result.IQ = Convert.ToBase64String(rawParameters.InverseQ);
            }

            if (rawParameters.Modulus != null)
            {
                result.M = Convert.ToBase64String(rawParameters.Modulus);
            }

            if (rawParameters.P != null)
            {
                result.P = Convert.ToBase64String(rawParameters.P);
            }

            if (rawParameters.Q != null)
            {
                result.Q = Convert.ToBase64String(rawParameters.Q);
            }

            return result;
        }

        public RSA GetRSA()
        {
            var parameters = new RSAParameters();
            if (D != null)
            {
                parameters.D = Convert.FromBase64String(D);
            }

            if (DP != null)
            {
                parameters.DP = Convert.FromBase64String(DP);
            }

            if (DQ != null)
            {
                parameters.DQ = Convert.FromBase64String(DQ);
            }

            if (E != null)
            {
                parameters.Exponent = Convert.FromBase64String(E);
            }

            if (IQ != null)
            {
                parameters.InverseQ = Convert.FromBase64String(IQ);
            }

            if (M != null)
            {
                parameters.Modulus = Convert.FromBase64String(M);
            }

            if (P != null)
            {
                parameters.P = Convert.FromBase64String(P);
            }

            if (Q != null)
            {
                parameters.Q = Convert.FromBase64String(Q);
            }

            var rsa = RSA.Create();
            rsa.ImportParameters(parameters);

            return rsa;
        }
    }
}