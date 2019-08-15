using System;
using System.Security.Cryptography;
using System.Text;

namespace Unified.Encryption.Hash {
	/// <summary>Class used to generate and check hashes.</summary>
	public class Hash {
		/// <summary></summary>
		public Hash() { }
		
		#region Hash Choices
		/// <summary>The wanted hash function.</summary>
		public enum HashType :int {
			/// <summary>MD5 Hashing</summary>
			MD5,
			/// <summary>SHA1 Hashing</summary>
			SHA1,
			/// <summary>SHA256 Hashing</summary>
			SHA256,
			/// <summary>SHA384 Hashing</summary>
			SHA384,
			/// <summary>SHA512 Hashing</summary>
			SHA512
		} /* HashType */
		#endregion
		
		#region Public Methods
		/// <summary>Generates the hash of a text.</summary>
		/// <param name="strPlain">The text of which to generate a hash of.</param>
		/// <param name="hshType">The hash function to use.</param>
		/// <returns>The hash as a hexadecimal string.</returns>
		public static string GetHash(string strPlain, HashType hshType) {
			string strRet;
			switch (hshType) {
				case HashType.MD5: strRet = GetMD5(strPlain);	break;
				case HashType.SHA1: strRet = GetSHA1(strPlain);	break;
				case HashType.SHA256: strRet = GetSHA256(strPlain); break;
				case HashType.SHA384: strRet = GetSHA384(strPlain); break;
				case HashType.SHA512: strRet = GetSHA512(strPlain); break;
				default: strRet = "Invalid HashType"; break;
			}
			return strRet;
		} /* GetHash */
		
		/// <summary>Checks a text with a hash.</summary>
		/// <param name="strOriginal">The text to compare the hash against.</param>
		/// <param name="strHash">The hash to compare against.</param>
		/// <param name="hshType">The type of hash.</param>
		/// <returns>True if the hash validates, false otherwise.</returns>
		public static bool CheckHash(string strOriginal, string strHash, HashType hshType) {
			var strOrigHash = GetHash(strOriginal, hshType);
			return (strOrigHash == strHash);
		} /* CheckHash */
		#endregion
		
		#region Hashers
		private static string GetMD5(string strPlain) {
			var UE = new UnicodeEncoding();
			byte[] HashValue, MessageBytes = UE.GetBytes(strPlain);
			MD5 md5 = new MD5CryptoServiceProvider();
			var strHex = "";
			
			HashValue = md5.ComputeHash(MessageBytes);
			foreach(var b in HashValue) {
				strHex += String.Format("{0:x2}", b);
			}
			return strHex;
		} /* GetMD5 */
		
		private static string GetSHA1(string strPlain) {
			var UE = new UnicodeEncoding();
			byte[] HashValue, MessageBytes = UE.GetBytes(strPlain);
			var SHhash = new SHA1Managed();
			var strHex = "";

			HashValue = SHhash.ComputeHash(MessageBytes);
			foreach(var b in HashValue) {
				strHex += String.Format("{0:x2}", b);
			}
			return strHex;
		} /* GetSHA1 */
		
		private static string GetSHA256(string strPlain) {
			var UE = new UnicodeEncoding();
			byte[] HashValue, MessageBytes = UE.GetBytes(strPlain);
			var SHhash = new SHA256Managed();
			var strHex = "";

			HashValue = SHhash.ComputeHash(MessageBytes);
			foreach(var b in HashValue) {
				strHex += String.Format("{0:x2}", b);
			}
			return strHex;
		} /* GetSHA256 */
		
		private static string GetSHA384(string strPlain) {
			var UE = new UnicodeEncoding();
			byte[] HashValue, MessageBytes = UE.GetBytes(strPlain);
			var SHhash = new SHA384Managed();
			var strHex = "";

			HashValue = SHhash.ComputeHash(MessageBytes);
			foreach(var b in HashValue) {
				strHex += String.Format("{0:x2}", b);
			}
			return strHex;
		} /* GetSHA384 */
		
		private static string GetSHA512(string strPlain) {
			var UE = new UnicodeEncoding();
			byte[] HashValue, MessageBytes = UE.GetBytes(strPlain);
			var SHhash = new SHA512Managed();
			var strHex = "";

			HashValue = SHhash.ComputeHash(MessageBytes);
			foreach(var b in HashValue) {
				strHex += String.Format("{0:x2}", b);
			}
			return strHex;
		} /* GetSHA512 */
		#endregion
	} /* Hash */
} /* Hash */