using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Qiniu.Common;

namespace Qiniu.Cloud.Auth
{
	/// <summary>
	/// 七牛消息认证(Message Authentication)
	/// </summary>
	public class Mac
	{

		private string accessKey;

		/// <summary>
		/// Gets or sets the access key.
		/// </summary>
		/// <value>The access key.</value>
		public string AccessKey {
			get { return accessKey; }
			set { accessKey = value; }
		}

		private byte[] secretKey;

		/// <summary>
		/// Gets the secret key.
		/// </summary>
		/// <value>The secret key.</value>
		public byte[] SecretKey {
			get { return secretKey; }
		}

		public Mac ()
		{
			this.accessKey = Conf.ACCESS_KEY;
			this.secretKey = Encoding.UTF8.GetBytes (Conf.SECRET_KEY);
		}

		public Mac (string access, byte[] secretKey)
		{
			this.accessKey = access;
			this.secretKey = secretKey;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private string _sign (byte[] data)
		{
			HMACSHA1 hmac = new HMACSHA1 (SecretKey);
			byte[] digest = hmac.ComputeHash (data);
			return digest.ToBase64UrlSafe ();
		}

		/// <summary>
		/// Sign
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public string Sign (byte[] b)
		{
			return string.Format ("{0}:{1}", this.accessKey, _sign (b));
		}

		/// <summary>
		/// SignWithData
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public string SignWithData (byte[] b)
		{
			string data = b.ToBase64UrlSafe ();
			return string.Format ("{0}:{1}:{2}", this.accessKey, _sign (Encoding.UTF8.GetBytes (data)), data);
		}

		/// <summary>
		/// Signs the request.
		/// </summary>
		/// <returns>The request.</returns>
		/// <param name="pathAndQuery">Path and query.</param>
		/// <param name="requestBody">Request body.</param>
		public string SignRequest(string pathAndQuery,byte[] requestBody){
			using (HMACSHA1 hmac = new HMACSHA1(secretKey)) {
				byte[] pathAndQueryBytes = Encoding.UTF8.GetBytes (pathAndQuery);
				using (MemoryStream buffer = new MemoryStream()) {
					buffer.Write (pathAndQueryBytes, 0, pathAndQueryBytes.Length);
					buffer.WriteByte ((byte)'\n');
					if (requestBody!=null&&requestBody.Length > 0) {
						buffer.Write (requestBody, 0, requestBody.Length);
					}
					byte[] digest = hmac.ComputeHash (buffer.ToArray ());
					string digestBase64 = digest.ToBase64UrlSafe();
					return this.accessKey + ":" + digestBase64;
				}
			}
		}

		/// <summary>
		/// SignRequest
		/// </summary>
		/// <param name="request"></param>
		/// <param name="body"></param>
		/// <returns></returns>
		public string SignRequest (System.Net.HttpWebRequest request, byte[] body)
		{
			Uri u = request.Address;
			using (HMACSHA1 hmac = new HMACSHA1(secretKey)) {
				string pathAndQuery = request.Address.PathAndQuery;

				byte[] pathAndQueryBytes = Encoding.UTF8.GetBytes (pathAndQuery);
				using (MemoryStream buffer = new MemoryStream()) {
					buffer.Write (pathAndQueryBytes, 0, pathAndQueryBytes.Length);
					buffer.WriteByte ((byte)'\n');
					if (body.Length > 0) {
						buffer.Write (body, 0, body.Length);
					}
					byte[] digest = hmac.ComputeHash (buffer.ToArray ());

					string digestBase64 = digest.ToBase64UrlSafe();

					return this.accessKey + ":" + digestBase64;
				}
			}
		}
	}
}

