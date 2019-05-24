using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Terminals.Security
{
    /// <summary>
    ///     Security encryption/decryption logic for stored passwords after version 2.0.
    ///     This version resolves previous version problems.
    ///     Used Algorithm and length of key and salt are identical with previous version.
    ///     Passwords are stored as text combined of initialization vector + encrypted password.
    ///     Master password key and stored password keys are based on random initialization vector.
    ///     There is no stored master password key hash.
    ///     To create new master password key:
    ///     1. call <see cref="CalculateStoredMasterPasswordKey" /> method
    ///     2. call <see cref="CalculateMasterPasswordKey(string,string)" /> to get new master password key
    ///     Links:
    ///     <seealso cref="http://msdn.microsoft.com/en-us/magazine/cc163913.aspx" />
    ///     <seealso
    ///         cref="http://blogs.msdn.com/b/shawnfa/archive/2004/04/14/generating-a-key-from-a-password.aspx?Redirected=true" />
    ///     <seealso cref="http://msdn.microsoft.com/en-us/library/system.security.cryptography.rfc2898derivebytes.aspx" />
    /// </summary>
    internal class PasswordFunctions2
    {
        /// <summary>
        ///     Recommended minimal value is greater than 1000, we simply use randomly more than 2000
        /// </summary>
        private const int ITERATION_COUNT = 2121;

        /// <summary>
        ///     Stronger, than in previous version
        /// </summary>
        private const int KEY_LENGTH = 32;

        /// <summary>
        ///     Identical with previous version (16B).
        /// </summary>
        private const int IV_LENGTH = PasswordFunctions.IV_LENGTH;

        private static readonly RandomNumberGenerator saltGenerator = RandomNumberGenerator.Create();

        /// <summary>
        ///     Ensures if, password entered by user is valid against storedPassword.
        /// </summary>
        /// <param name="password">Password entered by user</param>
        /// <param name="storedMasterPassword">
        ///     Validation key, encrypted by masterPassword key,
        ///     which should be identical to key, which we are able to generate from entered password
        /// </param>
        internal static bool MasterPasswordIsValid(string password, string storedMasterPassword)
        {
            // first part is master password key
            var keyParts = SplitEncryptedPassword(storedMasterPassword);
            // from the second part again first part is salt, second is not encrypted validation key
            var validationParts = SplitEncryptedPassword(keyParts.Item2);

            // empty password check
            if (string.IsNullOrEmpty(password) && validationParts.Item1.Length == 0)
                return true;

            var validationKey = CalculateMasterPasswordKey(password, validationParts.Item1);
            return validationKey.SequenceEqual(validationParts.Item2);
        }

        /// <summary>
        ///     Replacement of v1 passwords <see cref="PasswordFunctions.ComputeMasterPasswordHash" />
        /// </summary>
        internal static string CalculateStoredMasterPasswordKey(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            var validationKeySalt = CreateRandomKeySalt();
            var vaidationKey = CalculateMasterPasswordKey(password, validationKeySalt);
            var validationKeyWithSalt = ConcatenatePasswordParts(validationKeySalt, vaidationKey);
            var masterKeySalt = CreateRandomKeySalt();
            return ConcatenatePasswordPartsToText(masterKeySalt, validationKeyWithSalt);
        }

        /// <summary>
        ///     Calculates new master password key using key salt used for stored master password.
        ///     Doesn't validate, if the stored master password is valid. Use only this overload on real data.
        ///     For newly entered passwords, should be used always after <see cref="CalculateStoredMasterPasswordKey" /> method.
        /// </summary>
        /// <param name="password">master password for which the key has to be generated</param>
        /// <param name="storedMasterPassword">
        ///     Not empty stored random key derived from master password,
        ///     contains two key salt
        /// </param>
        internal static string CalculateMasterPasswordKey(string password, string storedMasterPassword)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;
            var keySalt = SplitEncryptedPassword(storedMasterPassword).Item1;
            return CalculateMasterPasswordKeyText(password, keySalt);
        }

        private static string CalculateMasterPasswordKeyText(string password, byte[] keySalt)
        {
            var key = CalculateMasterPasswordKey(password, keySalt);
            return Convert.ToBase64String(key);
        }

        private static byte[] CalculateMasterPasswordKey(string password, byte[] keySalt)
        {
            var rfbk2 = new Rfc2898DeriveBytes(password, keySalt, ITERATION_COUNT);
            return rfbk2.GetBytes(KEY_LENGTH);
        }

        internal static string EncryptPassword(string password, string keyMaterial)
        {
            try
            {
                return TryEncryptPassword(password, keyMaterial);
            }
            catch (Exception exception)
            {
                Logging.Error("Error Encrypting Password", exception);
                return string.Empty;
            }
        }

        private static string TryEncryptPassword(string password, string keyMaterial)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            if (string.IsNullOrEmpty(keyMaterial))
                return EncryptByEmptyKeyMaterial(password);

            return EncryptPasswordByRandomSalt(password, keyMaterial);
        }

        private static string EncryptPasswordByRandomSalt(string password, string keyMaterial)
        {
            var initializationVector = CreateRandomKeySalt();
            var passwordKey = Convert.FromBase64String(keyMaterial);
            return EncryptPassword(password, passwordKey, initializationVector);
        }

        private static string EncryptByEmptyKeyMaterial(string password)
        {
            var initializationVector = CreateRandomKeySalt();
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            var protectedPassword =
                ProtectedData.Protect(passwordBytes, initializationVector, DataProtectionScope.CurrentUser);
            return ConcatenatePasswordPartsToText(initializationVector, protectedPassword);
        }

        private static string EncryptPassword(string password, byte[] passwordKey, byte[] initializationVector)
        {
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            var encryptedPassword = PasswordFunctions.EncryptByKey(passwordBytes, initializationVector, passwordKey);
            return ConcatenatePasswordPartsToText(initializationVector, encryptedPassword);
        }

        internal static string DecryptPassword(string encryptedPassword, string keyMaterial)
        {
            try
            {
                return TryDecryptPassword(encryptedPassword, keyMaterial);
            }
            catch (Exception exception)
            {
                Logging.Error("Error Decrypting Password", exception);
                return string.Empty;
            }
        }

        private static string TryDecryptPassword(string encryptedPassword, string keyMaterial)
        {
            if (string.IsNullOrEmpty(encryptedPassword) || encryptedPassword.Length < IV_LENGTH)
                return string.Empty;

            if (string.IsNullOrEmpty(keyMaterial))
                return DecryptByEmptyKeyMaterial(encryptedPassword);

            var passwordKey = Convert.FromBase64String(keyMaterial);
            return DecryptPassword(encryptedPassword, passwordKey);
        }

        private static string DecryptByEmptyKeyMaterial(string encryptedPassword)
        {
            var passwordParts = SplitEncryptedPassword(encryptedPassword);
            var plaintext = ProtectedData.Unprotect(passwordParts.Item2, passwordParts.Item1,
                DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(plaintext);
        }

        private static string DecryptPassword(string encryptedPassword, byte[] passwordKey)
        {
            var passwordParts = SplitEncryptedPassword(encryptedPassword);
            var decrypted = PasswordFunctions.DecryptByKey(passwordParts.Item2, passwordParts.Item1, passwordKey);
            return Encoding.Unicode.GetString(decrypted);
        }

        private static string ConcatenatePasswordPartsToText(byte[] initializationVector, byte[] encryptedPassword)
        {
            var encryptedResult = ConcatenatePasswordParts(initializationVector, encryptedPassword);
            return Convert.ToBase64String(encryptedResult);
        }

        private static byte[] ConcatenatePasswordParts(byte[] initializationVector, byte[] encryptedPassword)
        {
            var encryptedResult = new byte[initializationVector.Length + encryptedPassword.Length];
            initializationVector.CopyTo(encryptedResult, 0);
            encryptedPassword.CopyTo(encryptedResult, initializationVector.Length);
            return encryptedResult;
        }

        /// <summary>
        ///     Extracts initialization vector as Item1 and password bytes as Item2 from encryptedPassword.
        /// </summary>
        private static Tuple<byte[], byte[]> SplitEncryptedPassword(string encryptedPassword)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedPassword);
            return SplitEncryptedPassword(encryptedBytes);
        }

        private static Tuple<byte[], byte[]> SplitEncryptedPassword(byte[] encryptedBytes)
        {
            var initializationVector = encryptedBytes.Take(IV_LENGTH).ToArray();
            var passwordPart = encryptedBytes.Skip(IV_LENGTH).ToArray();
            return new Tuple<byte[], byte[]>(initializationVector, passwordPart);
        }

        private static byte[] CreateRandomKeySalt()
        {
            var initializationVector = new byte[IV_LENGTH];
            saltGenerator.GetBytes(initializationVector);
            return initializationVector;
        }
    }
}