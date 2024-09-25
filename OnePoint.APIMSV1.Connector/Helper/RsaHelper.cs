using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.OpenSsl;

namespace OnePoint.APIMSV1.Connector.Helper
{
    public class RsaHelper
    {
        public RsaHelper(string privateKey)
        {
            _privateKey = GetPrivateKeyFromPemFile(privateKey);
        }
        private readonly RSACryptoServiceProvider _privateKey;

        private RSACryptoServiceProvider GetPrivateKeyFromPemFile(string pemFilePath)
        {
            string pemContents;
            using (StreamReader reader = new StreamReader(pemFilePath))
            {
                pemContents = reader.ReadToEnd();
            }
            using (TextReader privateKeyTextReader = new StringReader(pemContents))
            {
                AsymmetricCipherKeyPair readKeyPair = (AsymmetricCipherKeyPair)new PemReader(privateKeyTextReader).ReadObject();

                RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)readKeyPair.Private);
                RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
                csp.ImportParameters(rsaParams);
                return csp;
            }
        }

        public string Sign(byte[] bytes)
        {
            var hashBytes = _privateKey.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
