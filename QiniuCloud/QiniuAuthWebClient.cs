using System;
using System.Text;
using System.Net;
using Qiniu.Cloud.Auth;
namespace Qiniu.Cloud
{
	/// <summary>
	/// Qiniu auth web client.
	/// </summary>
	public class QiniuAuthWebClient
	{
		/// <summary>
		/// The wc.
		/// </summary>
		private WebClient wc = new WebClient();

		/// <summary>
		/// The mac.
		/// </summary>
		private Mac mac;

		/// <summary>
		/// Initializes a new instance of the <see cref="Qiniu.Cloud.QiniuAuthWebClient"/> class.
		/// </summary>
		/// <param name="mac">Mac.</param>
		public QiniuAuthWebClient (Mac mac=null)
		{
			this.mac = mac == null ? new Mac (): mac;
			wc = new WebClient ();
			wc.Headers.Add ("User-Agent", Conf.USER_AGENT);
		}

		/// <summary>
		/// Uploads the data.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="path">Path.</param>
		/// <param name="data">Data.</param>
		public string UploadData(Uri path, byte[] data){
			string auth = mac.SignRequest (path.PathAndQuery, data);
			wc.Headers.Add ("Authorization", "QBox " + auth);
			byte[] rawBytes = wc.UploadData (path,data);
			return Encoding.UTF8.GetString (rawBytes);
		}

		/// <summary>
		/// Get the specified path.
		/// </summary>
		/// <param name="path">Path.</param>
		public string Get(Uri path){
			string auth = mac.SignRequest (path.PathAndQuery, null);
			wc.Headers.Add ("Authorization", "QBox " + auth);
			try{
				return wc.UploadString (path,string.Empty);
			}catch(WebException e){
				throw e;
			}

		}

		/// <summary>
		/// Uploads the string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="path">Path.</param>
		/// <param name="body">Body.</param>
		public string UploadString(Uri path,string body ){
			string auth = mac.SignRequest (path.PathAndQuery, Encoding.UTF8.GetBytes(body));
			wc.Headers.Add ("Authorization", "QBox " + auth);
			return wc.UploadString (path, body);
		}
	}
}

