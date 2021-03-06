using System;
using System.Text;

namespace Qiniu.Common
{

	public static class Base64UrlSafe
	{
		public static string Encode (string text)
		{
			if (String.IsNullOrEmpty (text))
				return "";
			byte[] bs = Encoding.UTF8.GetBytes (text);
			string encodedStr = Convert.ToBase64String (bs);
			encodedStr = encodedStr.Replace ('+', '-').Replace ('/', '_');
			return encodedStr;
		}

		/// <summary>
		/// string扩展方法，生成base64UrlSafe
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string ToBase64URLSafe (this string text)
		{
			return Encode (text);
		}

		public static string ToBase64UrlSafe(this byte[] bs){
			return Encode (bs);
		}

		public static string Encode (byte[] bs)
		{
			if (bs == null || bs.Length == 0)
				return "";
			string encodedStr = Convert.ToBase64String (bs);
			encodedStr = encodedStr.Replace ('+', '-').Replace ('/', '_');
			return encodedStr;
		}
	}
}

