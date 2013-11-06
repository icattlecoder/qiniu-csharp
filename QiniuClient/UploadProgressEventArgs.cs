using System;

namespace Qiniu
{
	/// <summary>
	/// Upload progress event arguments.
	/// </summary>
	public class UploadProgressEventArgs
	{
		private long totalBytes;

		/// <summary>
		/// Gets the total bytes.
		/// </summary>
		/// <value>The total bytes.</value>
		public long TotalBytes {
			get {
				return totalBytes;
			}
		}

		private long bytesSent;

		/// <summary>
		/// Gets the bytes sent.
		/// </summary>
		/// <value>The bytes sent.</value>
		public long BytesSent {
			get {
				return bytesSent;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Qiniu.UploadProgressEventArgs"/> class.
		/// </summary>
		/// <param name="total">Total.</param>
		/// <param name="send">Send.</param>
		public UploadProgressEventArgs (long total,long sent)
		{
			this.totalBytes = total;
			this.bytesSent = sent;
		}
	}
}

