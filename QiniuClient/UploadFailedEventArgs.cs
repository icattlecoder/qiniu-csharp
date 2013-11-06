using System;

namespace Qiniu
{
	public class  UploadFailedEventArgs:EventArgs{
		private string msg;
		/// <summary>
		/// Gets the message.
		/// </summary>
		/// <value>The message.</value>
		public string Msg {
			get {
				return msg;
			}
		}

		private int httpCode;
		public UploadFailedEventArgs(string msg){
			this.msg = msg;
		}
		public UploadFailedEventArgs(string msg,int httpCode){
			this.msg = msg;
			this.httpCode = httpCode;
		}
	}
}

