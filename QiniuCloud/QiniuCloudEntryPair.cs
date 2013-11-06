using System;

namespace Qiniu.Cloud
{
	public class QiniuCloudEntryPair
	{
		private QiniuCloudEntry first;

		/// <summary>
		/// Gets the first.
		/// </summary>
		/// <value>The first.</value>
		public QiniuCloudEntry First {
			get {
				return first;
			}
		}

		private QiniuCloudEntry second;

		/// <summary>
		/// Gets the second.
		/// </summary>
		/// <value>The second.</value>
		public QiniuCloudEntry Second {
			get {
				return second;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Qiniu.QiniuCloudEntryPair"/> class.
		/// </summary>
		/// <param name="bucket">Bucket.</param>
		/// <param name="key1">Key1.</param>
		/// <param name="key2">Key2.</param>
		public QiniuCloudEntryPair (string bucket,string key1,string key2)
		{
			first = new QiniuCloudEntry (bucket, key1);
			second = new QiniuCloudEntry (bucket, key2);

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Qiniu.QiniuCloudEntryPair"/> class.
		/// </summary>
		/// <param name="bucket1">Bucket1.</param>
		/// <param name="key1">Key1.</param>
		/// <param name="bucket2">Bucket2.</param>
		/// <param name="key2">Key2.</param>
		public QiniuCloudEntryPair (string bucket1,string key1,string bucket2,string key2)
		{
			first = new QiniuCloudEntry (bucket1, key1);
			second = new QiniuCloudEntry (bucket2, key2);
		}

		public QiniuCloudEntryPair(QiniuCloudEntry first,QiniuCloudEntry second){
			this.first = first;
			this.second = second;
		}
	}
}

