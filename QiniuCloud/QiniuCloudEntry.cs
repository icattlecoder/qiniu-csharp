using System;

namespace Qiniu.Cloud
{
	public class QiniuCloudEntry
	{
		private string domain;
		private string bucket;
		private string key;
		private string url;

		public string Url {
			get {
				return url;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Qiniu.QiniuCloudEntry"/> class.
		/// </summary>
		/// <param name="bucket">Bucket.</param>
		/// <param name="key">Key.</param>
		/// <param name="domain">Domain.</param>
		public QiniuCloudEntry (string bucket,string key,string domain=null)
		{
			this.bucket = bucket;
			this.key = key;
			if (string.IsNullOrEmpty (domain)) {
				this.domain = "qiniudn.com";
			}else{
				this.domain = domain;
			}
			this.url = string.Format ("{0}:{1}", bucket, key);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Qiniu.QiniuCloudEntry"/> class.
		/// </summary>
		/// <param name="url">URL.</param>
		public QiniuCloudEntry(string url){

		}

	}
}

