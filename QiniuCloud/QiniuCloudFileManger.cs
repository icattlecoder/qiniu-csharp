using System;
using System.Collections.Generic;
using System.Net;
using Qiniu.Cloud.Auth;
using Qiniu.Common;
using System.Text;
namespace Qiniu.Cloud
{
	/// <summary>
	/// Qiniu cloud file manger.
	/// </summary>
	public class QiniuCloudFileManger
	{
		/// <summary>
		/// 文件管理操作
		/// </summary>
		private enum FileHandle
		{
			/// <summary>
			/// 查看
			/// </summary>
			STAT = 0,
			/// <summary>
			/// 移动move
			/// </summary>
			MOVE,
			/// <summary>
			/// 复制copy
			/// </summary>
			COPY,
			/// <summary>
			/// 删除delete
			/// </summary>
			DELETE
		}

		private static string[] OPS = new string[] { "stat", "move", "copy", "delete" };

		Mac mac;

		QiniuAuthWebClient webClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="Qiniu.Cloud.QiniuCloudFileManger"/> class.
		/// </summary>
		public QiniuCloudFileManger(Mac mac=null){
			this.mac = mac;
			this.webClient = new QiniuAuthWebClient (this.mac);
		}

		private string op1(FileHandle op,QiniuCloudEntry entry){
			string url = string.Format ("{0}/{1}/{2}",
			                            QiniuHosts.RS_HOST,
			                            OPS [(int)op],
			                            entry.Url.ToBase64URLSafe ());
			return webClient.Get (new Uri(url));
		}

		private string op2(FileHandle op,QiniuCloudEntryPair pair){
			string url = string.Format ("{0}/{1}/{2}/{3}",
			                           QiniuHosts.RS_HOST,
			                           OPS [(int)op],
			                           pair.First.Url,
			                           pair.Second.Url);
			return webClient.Get (new Uri(url));
		}

		/// <summary>
		/// 获取一元批操作http request Body
		/// </summary>
		/// <param name="opName">操作名</param>
		/// <param name="keys">操作对象keys</param>
		/// <returns>Request Body</returns>
		private string getBatchOp_1 (FileHandle op, QiniuCloudEntry[] entries)
		{
			if (entries.Length < 1)
				return string.Empty;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < entries.Length - 1; i++) {
				sb.AppendFormat ("op=/{0}/{1}&",
				                OPS [(int)op], 
				                entries [i].Url);
			}
			sb.AppendFormat ("op=/{0}/{1}", OPS [(int)op], entries [entries.Length - 1].Url);
			return batch (sb.ToString ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="opName"></param>
		/// <param name="keys"></param>
		/// <returns></returns>
		private string getBatchOp_2 (FileHandle op, QiniuCloudEntryPair[] entryPairs )
		{
			if (entryPairs.Length < 1)
				return string.Empty;
			StringBuilder sb = new StringBuilder ();
			int i = 0;
			for (; i < entryPairs.Length - 1; i++) {
				sb.AppendFormat ("op=/{0}/{1}/{2}&",
				                 OPS [(int)op], 
				                 entryPairs[i].First.Url.ToBase64URLSafe(),
				                 entryPairs[i].Second.Url.ToBase64URLSafe());
			}
			sb.AppendFormat ("op=/{0}/{1}/{2}", OPS [(int)op], entryPairs [i].First.Url.ToBase64URLSafe (),
			                 entryPairs [i].Second.Url.ToBase64URLSafe ());
			return batch (sb.ToString ());
		}

		private string batch(string body){
			return webClient.UploadString (new Uri(QiniuHosts.RS_HOST+"/batch"),body);
		}

		/// <summary>
		/// Stat the specified entry.
		/// </summary>
		/// <param name="entry">Entry.</param>
		public  string Stat(QiniuCloudEntry entry){
			return op1 (FileHandle.STAT, entry);
		}

		/// <summary>
		/// Delete the specified entry.
		/// </summary>
		/// <param name="entry">Entry.</param>
		public  void Delete(QiniuCloudEntry entry){
			op1 (FileHandle.STAT, entry);
		}

		/// <summary>
		/// Move the specified src and dest.
		/// </summary>
		/// <param name="src">Source.</param>
		/// <param name="dest">Destination.</param>
		public  void Move(QiniuCloudEntryPair entryPair){
			op2 (FileHandle.MOVE, entryPair);

		}

		/// <summary>
		/// Copy the specified src and dest.
		/// </summary>
		/// <param name="src">Source.</param>
		/// <param name="dest">Destination.</param>
		public  void Copy(QiniuCloudEntryPair entryPair){
			op2 (FileHandle.COPY, entryPair);
		}

		/// <summary>
		/// Stat the specified entry.
		/// </summary>
		/// <param name="entry">Entry.</param>
		public  void Stat(List<QiniuCloudEntry> entries){
			getBatchOp_1 (FileHandle.STAT, entries.ToArray());
		}

		/// <summary>
		/// Delete the specified entry.
		/// </summary>
		/// <param name="entry">Entry.</param>
		public  void Delete(List<QiniuCloudEntry> entries){
			getBatchOp_1 (FileHandle.DELETE, entries.ToArray());
		}

		/// <summary>
		/// Copy the specified pairs.
		/// </summary>
		/// <param name="pairs">Pairs.</param>
		public  void Copy(List<QiniuCloudEntryPair> pairs){
			getBatchOp_2 (FileHandle.COPY, pairs.ToArray ());
		}
		
		/// <summary>
		/// Copy the specified pairs.
		/// </summary>
		/// <param name="pairs">Pairs.</param>
		public  void Move(List<QiniuCloudEntryPair> pairs){
			getBatchOp_2 (FileHandle.MOVE, pairs.ToArray ());
		}

		/// <summary>
		/// List this instance.
		/// </summary>
		public  void List(){
		}
	}
}

