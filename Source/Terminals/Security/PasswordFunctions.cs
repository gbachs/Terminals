using System;
using System.Security.Cryptography;
using System.Text;
using Unified.Encryption;
using Unified.Encryption.Hash;

namespace Terminals.Security
{
    /// <summary>
    ///     Security encryption/decryption logic for stored passwords till version 2.0.
    ///     Use only for upgrades from previous version.
    ///     Implementation problems:
    ///     - master password hash is stored in config file
    ///     - rfbk2 wasn't used to make the key stronger
    ///     - key generated from identical master password is always the same
    /// </summary>
    internal static class PasswordFunctions
    {
        private const int KEY_LENGTH = 24;

        internal const int IV_LENGTH = 16;

        private const EncryptionAlgorithm ENCRYPTION_ALGORITHM = EncryptionAlgorithm.Rijndael;

        internal static bool MasterPasswordIsValid(string password, string storedPassword)
        {
            var hashToCheck = string.Empty;
            if (!string.IsNullOrEmpty(password))
                hashToCheck = ComputeMasterPasswordHash(password);
            return hashToCheck == storedPassword;
        }

        internal static string CalculateMasterPasswordKey(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;
            var hashToCheck = ComputeMasterPasswordHash(password);
            return ComputeMasterPasswordHash(password + hashToCheck);
        }

        internal static string ComputeMasterPasswordHash(string password)
        {
            return Hash.GetHash(password, Hash.HashType.SHA512);
        }

        internal static string DecryptPassword(string encryptedPassword, string keyMaterial)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedPassword))
                    return encryptedPassword;

                if (keyMaterial == string.Empty)
                    return DecryptByEmptyKey(encryptedPassword);

                return DecryptByKey(encryptedPassword, keyMaterial);
            }
            catch (Exception e)
            {
                Logging.Error("Error Decrypting Password", e);
                return string.Empty;
            }
        }

        private static string DecryptByKey(string encryptedPassword, string keyMaterial)
        {
            var initializationVector = GetInitializationVector(keyMaterial);
            var passwordKey = GetPasswordKey(keyMaterial);
            var passwordBytes = Convert.FromBase64String(encryptedPassword);
            var data = DecryptByKey(passwordBytes, initializationVector, passwordKey);

            if (data != null && data.Length > 0)
                return Encoding.Default.GetString(data);

            return string.Empty;
        }

        internal static byte[] DecryptByKey(byte[] encryptedPassword, byte[] initializationVector, byte[] passwordKey)
        {
            var dec = new Decryptor(ENCRYPTION_ALGORITHM);
            dec.IV = initializationVector;
            return dec.Decrypt(encryptedPassword, passwordKey);
        }

        private static string DecryptByEmptyKey(string encryptedPassword)
        {
            var cyphertext = Convert.FromBase64String(encryptedPassword);
            var entropy = Encoding.UTF8.GetBytes(string.Empty);
            var plaintext = ProtectedData.Unprotect(cyphertext, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plaintext);
        }

        internal static string EncryptPassword(string decryptedPassword, string keyMaterial)
        {
            try
            {
                if (string.IsNullOrEmpty(decryptedPassword))
                    return decryptedPassword;

                if (keyMaterial == string.Empty)
                    return EncryptByEmptyKey(decryptedPassword);

                return EncryptByKey(decryptedPassword, keyMaterial);
            }
            catch (Exception ec)
            {
                Logging.Error("Error Encrypting Password", ec);
                return string.Empty;
            }
        }

        private static string EncryptByKey(string decryptedPassword, string keyMaterial)
        {
            var initializationVector = GetInitializationVector(keyMaterial);
            var passwordKey = GetPasswordKey(keyMaterial);
            var passwordBytes = Encoding.Default.GetBytes(decryptedPassword);
            var data = EncryptByKey(passwordBytes, initializationVector, passwordKey);

            if (data != null && data.Length > 0)
                return Convert.ToBase64String(data);

            return string.Empty;
        }

        internal static byte[] EncryptByKey(byte[] decryptedPassword, byte[] initializationVector, byte[] passwordKey)
        {
            var enc = new Encryptor(ENCRYPTION_ALGORITHM);
            enc.IV = initializationVector;
            return enc.Encrypt(decryptedPassword, passwordKey);
        }

        private static string EncryptByEmptyKey(string decryptedPassword)
        {
            var plaintext = Encoding.UTF8.GetBytes(decryptedPassword);
            var entropy = Encoding.UTF8.GetBytes(string.Empty);
            var cyphertext = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(cyphertext);
        }

        private static byte[] GetInitializationVector(string keyMaterial)
        {
            var keyPart = keyMaterial.Substring(keyMaterial.Length - IV_LENGTH);
            return Encoding.Default.GetBytes(keyPart);
        }

        private static byte[] GetPasswordKey(string keyMaterial)
        {
            var keyChars = keyMaterial.Substring(0, KEY_LENGTH);
            return Encoding.Default.GetBytes(keyChars);
        }
    }
}