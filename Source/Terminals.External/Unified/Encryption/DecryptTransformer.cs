using System;
using System.Security.Cryptography;

namespace Unified.Encryption
{
    public class DecryptTransformer
    {
        private readonly EncryptionAlgorithm algorithmId;

        private byte[] initVec;

        internal byte[] IV { set { initVec = value; } }

        internal DecryptTransformer(EncryptionAlgorithm deCryptId)
        {
            this.algorithmId = deCryptId;
        }

        internal ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
        {
            ICryptoTransform iCryptoTransform;

            switch (this.algorithmId)
            {
                case EncryptionAlgorithm.Des:
                    DES dES = new DESCryptoServiceProvider();
                    dES.Mode = CipherMode.CBC;
                    dES.Key = bytesKey;
                    dES.IV = initVec;
                    iCryptoTransform = dES.CreateDecryptor();
                    break;

                case EncryptionAlgorithm.TripleDes:
                    TripleDES tripleDES = new TripleDESCryptoServiceProvider();
                    tripleDES.Mode = CipherMode.CBC;
                    iCryptoTransform = tripleDES.CreateDecryptor(bytesKey, initVec);
                    break;

                case EncryptionAlgorithm.Rc2:
                    RC2 rC2 = new RC2CryptoServiceProvider();
                    rC2.Mode = CipherMode.CBC;
                    iCryptoTransform = rC2.CreateDecryptor(bytesKey, initVec);
                    break;

                case EncryptionAlgorithm.Rijndael:
                    Rijndael rijndael = new RijndaelManaged();
                    rijndael.Mode = CipherMode.CBC;
                    iCryptoTransform = rijndael.CreateDecryptor(bytesKey, initVec);
                    break;

                default:
                    throw new CryptographicException(String.Concat("Algorithm ID \'", this.algorithmId,
                        "\' not supported."));
            }

            return iCryptoTransform;
        }
    }
}