using System;
using System.IO;
using System.Security.Cryptography;

namespace Unified.Encryption
{
  public class Decryptor
  {
    private DecryptTransformer transformer;

    private byte[] initVec;


    public byte[] IV
    {

      set
      {
        initVec = value;
      }
    }

    public Decryptor(EncryptionAlgorithm algId)
    {
      transformer = new DecryptTransformer(algId);
    }

    public byte[] Decrypt(byte[] bytesData, byte[] bytesKey)
    {
      var memoryStream = new MemoryStream();
      transformer.IV = initVec;
      var iCryptoTransform = transformer.GetCryptoServiceProvider(bytesKey);
      var cryptoStream = new CryptoStream(memoryStream, iCryptoTransform, CryptoStreamMode.Write);
      try
      {
        cryptoStream.Write(bytesData, 0, (int)bytesData.Length);
        cryptoStream.FlushFinalBlock();
        cryptoStream.Close();
        var bs = memoryStream.ToArray();
        return bs;
      }
      catch (Exception e)
      {
        throw new Exception(String.Concat("Error while writing encrypted data to the stream: \n", e.Message));
      }
    }
  }

}
