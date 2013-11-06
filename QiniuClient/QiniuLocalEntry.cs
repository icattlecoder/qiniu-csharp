using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Qiniu.Common;

namespace Qiniu
{
	public delegate void UploadProgressing (object sender, UploadProgressEventArgs e);
	public delegate void UploadFinished (object sender, UploadFinishedArgs e);
	public delegate void UploadFailed (object sender, UploadFailedEventArgs e);

	/// <summary>
	/// Qiniu local entry.
	/// </summary>
	public class QiniuLocalEntry
	{
		private struct upInfo{
			public string upUrl;
			public byte[] data; 
		}

		private string filePath;
		private Stream streamReader;
		private bool uping = false;
		private long fileSize = 0;
		private long bytesSent = 0;
		private FileInfo finfo;

		#region settings
		private const int BLOCKBITS = 22;
		private const int BLOCKMASK = (1 << BLOCKBITS) - 1;
		private static int BLOCKSIZE = 4 * 1024 * 1024;
		private const string UNDEFINED_KEY = "?";
		private const int CHUNKSIZE = 1<<18;
		private const int RETRYTIMES = 3;
		#endregion


		public QiniuLocalEntry (string localFilePath)
		{
			this.filePath = localFilePath;
			if (string.IsNullOrEmpty (localFilePath)) {
				throw new ArgumentException ("localFilePath shoud be not null or empty");
			}
			this.finfo = new FileInfo (localFilePath);
			this.fileSize = finfo.Length;
		}

		public QiniuLocalEntry (Stream reader)
		{
			if (reader == null || !reader.CanRead) {
				throw new ArgumentException("stream reader shoud be not null and readable");  
			}
			this.streamReader = reader;

		}

		private static int BLOCK_COUNT(long fsize){
			return (int)((fsize + BLOCKMASK) >> BLOCKBITS);
		}

		#region readUp
	
		private upInfo readyUp(string upToken,Stream stream, string saveKey ,string mimeType = null, bool crc32 = false, NameValueCollection xVars = null,bool isFile=true){

			if (uping) {
				throw new Exception ("upload busying");
			}
			if(string.IsNullOrEmpty(upToken)){
				throw new Exception("bad upToken");
			}


			StringBuilder urlBuilder = new StringBuilder ();

			urlBuilder.Append (QiniuHosts.UP_HOST);

			//Fsize

			urlBuilder.AppendFormat ("/put/{0}",stream.Length);

			//key
			if (string.IsNullOrEmpty (saveKey)) {
				urlBuilder.AppendFormat ("/key/{0}", saveKey.ToBase64URLSafe ());
			}

			//mimetype
			if (!string.IsNullOrEmpty (mimeType)) {
				urlBuilder.AppendFormat("/mimeType/{0}",mimeType);
			}

			byte[] buffer = new byte[stream.Length];
			int n = stream.Read(buffer,0,(int)stream.Length);

			//crc32
			if (crc32) {
				urlBuilder.AppendFormat ("/crc32/{0}",CRC32.CheckSumBytes (buffer));
			}

			//x-Vars
			if (xVars != null && xVars.Count > 0) {
				foreach (string key in xVars.AllKeys) {
					if (!key.StartsWith ("x:")) {
						throw new Exception ("user var name must begin with \"x:\"");
					}
					urlBuilder.Append (string.Format ("/{0}/{1}", key, xVars [key].ToBase64URLSafe()));
				}
			}


			string url = urlBuilder.ToString();

			return new upInfo{upUrl=url,data=buffer};

		}
		#endregion

		// put 
		private void put(string upToken, string saveKey = null, string mimeType = null, bool crc32 = false, NameValueCollection xVars = null){
			upInfo info = readyUp(upToken,this.streamReader,saveKey,mimeType,crc32,xVars);
			WebClient client = new WebClient ();
			client.Headers.Add("Authorization","UpToken "+ upToken);
			client.UploadProgressChanged += (o, e) => {
				onProgressing(o,new UploadProgressEventArgs(fileSize,e.BytesSent));
			};
			client.UploadDataCompleted +=(o,e)=>{
				if(!e.Cancelled){
					string jsonStr = Encoding.UTF8.GetString(e.Result);
					Dictionary<string,string> res = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
					onFinished(this,new UploadFinishedArgs(res));
				}else{
					//TO-DO
				}
			};
			try{
				uping = true;
				client.UploadDataAsync(new Uri(info.upUrl),"POST",info.data);
			}catch(Exception e){
				onFailed(this,new UploadFailedEventArgs(e.ToString()));
			}
		}

		public void UploadFile (string upToken, string saveKey = null, string mimeType = null, bool crc32 = false, NameValueCollection xVars = null)
		{
			if (string.IsNullOrEmpty (filePath)) {
				throw new Exception ("file path is empty or null");
			}

			FileInfo finfo = new FileInfo (filePath);

			if (!finfo.Exists) {
				throw new Exception (string.Format ("file {0} doesn't exist", filePath));
			}


			using(FileStream fstream = File.OpenRead(filePath)){
				this.streamReader = fstream;
				UploadStream(upToken,saveKey,mimeType,crc32,xVars);
			}
		}

		public void UploadStream (string upToken, string saveKey = null, string mimeType = null, bool crc32 = false, NameValueCollection xVars = null)
		{
			if(streamReader!=null&&streamReader.CanRead&&streamReader.Length>0){
				if (streamReader.Length < 1 >> 22) {
					put (upToken, saveKey, mimeType, crc32, xVars);
				} else {
					ResumbleUp (upToken, saveKey, mimeType, crc32, xVars);
				}
			}
		}

		private void AsynProgressChangedHandler(object sender,UploadProgressChangedEventArgs args){
			this.bytesSent += args.BytesSent;
			onProgressing(this, new UploadProgressEventArgs (fileSize, this.bytesSent));
		}

		private BlkputRet mkblk(string upToken,byte[] blockdata,int blkidx){

			int block_size = blockdata.Length;

			string url = string.Format ("{0}/mkblk/{1}", QiniuHosts.UP_HOST, block_size);
			using(WebClient wc = new WebClient ())
			{
//				wc.UploadProgressChanged += new UploadProgressChangedEventHandler (AsynProgressChangedHandler);
				wc.Headers.Add ("Authorization", "UpToken " + upToken);
			//first chunk length
				int fcl = CHUNKSIZE < block_size ? CHUNKSIZE : block_size;
			//first chunk
			byte[] fc = new byte[fcl];
			Array.Copy (blockdata, 0, fc, 0, fcl);
			BlkputRet putRet = bput (wc, url, fc);
				this.bytesSent += fcl;
				onProgressing(this,new UploadProgressEventArgs(this.fileSize,this.bytesSent));

			while (putRet.offset < block_size) {
				url = string.Format ("{0}/bput/{1}/{2}", putRet.host, putRet.ctx, putRet.offset);
				//chunk length
				int cl = (CHUNKSIZE < (block_size - putRet.offset)) ? CHUNKSIZE : (int)(block_size - putRet.offset);
				byte[] chunk = new byte[cl];
				Array.Copy (blockdata, putRet.offset, chunk, 0, cl);
				putRet = bput (wc,url,chunk);
					this.bytesSent += cl;
					onProgressing (this, new UploadProgressEventArgs (this.fileSize, this.bytesSent));
			}
			return putRet;
			}
		}	
		

		private BlkputRet bput( WebClient wc,string url ,byte[] bdata ){

			for(int i=0;i<RETRYTIMES;i++){
				try{
					byte[] rawBytes = wc.UploadData (url, bdata);
					BlkputRet putRet = JsonConvert.DeserializeObject<BlkputRet>(Encoding.UTF8.GetString(rawBytes));
					if (CRC32.CheckSumBytes (bdata) != putRet.crc32) {
						if(i==(RETRYTIMES-1)){
							throw new Exception ("crc32 check error");
						}
					}else{
						return putRet;
					}
				}catch(Exception e){
					if(i==(RETRYTIMES-1)){
						throw e;
					}
				}
			}
			return null;
		}

		private void mkfile(string upToken,string saveKey,long fsize,string mimeType,ref BlkputRet[] upRets,NameValueCollection xVars)
		{

			string mkfileHost = string.Empty;
			StringBuilder sb = new StringBuilder ();
			for (int i=0; i<upRets.Length; i++) {
				sb.Append (upRets [i].ctx);
				if (i != (upRets.Length - 1)) {
					sb.Append (',');
				} else {
					mkfileHost = upRets [i].host;
				}
			}

			StringBuilder urlBuilder = new StringBuilder ();

			//Fsize
			urlBuilder.AppendFormat ("{0}/mkfile/{1}/key/{2}", QiniuHosts.UP_HOST, fsize, saveKey.ToBase64URLSafe());

			//mimeType
			if (!string.IsNullOrEmpty (mimeType)) {
				urlBuilder.AppendFormat ("/mimeType/{0}", mimeType.ToBase64URLSafe ());
			}

			//x-Vars
			if (xVars!=null&& xVars.Count > 0) {
				foreach (string key in xVars.Keys) {
					if (!key.StartsWith ("x:")) {
						throw new Exception ("user var name must begin with \"x:\"");
					}
					urlBuilder.AppendFormat (string.Format ("/{0}/{1}", key, xVars [key].ToBase64URLSafe ()));
				}
			}
			byte[] data = Encoding.ASCII.GetBytes (sb.ToString ());

			string url = urlBuilder.ToString ();
			WebClient wc = new WebClient ();
			wc.Headers.Add ("Authorization", "UpToken " + upToken);
			try{
				string resp =wc.UploadString (url, sb.ToString ());
				Dictionary<string,string> res = JsonConvert.DeserializeObject<Dictionary<string, string>> (resp);
				onFinished (this, new UploadFinishedArgs (res));
			}catch(Exception e){
				onFailed(this,new UploadFailedEventArgs(e.ToString()));
			}
		}	

		private void ResumbleUp(string upToken, string saveKey = null, string mimeType = null, bool crc32 = false, NameValueCollection xVars = null)
		{
			//streamReader has checked
			long fsize = streamReader.Length;

			int block_cnt = BLOCK_COUNT (fsize);

//			int chunks = fsize / CHUNKSIZE;

			BlkputRet []resumbleUpRets = new BlkputRet[block_cnt];

			Parallel.For (0, block_cnt, (i) => {
				Console.WriteLine("thread id={0}",i);
				int blocksize = (i + 1) *BLOCKSIZE  > fsize ? (int)(fsize - i * BLOCKSIZE) : BLOCKSIZE;
				byte[] buf = new byte[blocksize];
				lock (streamReader) {
					streamReader.Seek (i * BLOCKBITS, SeekOrigin.Begin);
					streamReader.Read (buf, 0, blocksize);
				}
				resumbleUpRets [i] = mkblk (upToken, buf, i);
			});
			mkfile(upToken,saveKey,fsize,mimeType,ref resumbleUpRets,xVars);
		}	

		public event UploadProgressing uploadProgressing;
		public event UploadFinished uploadFinished;
		public event UploadFailed uploadFailed;

		protected void onProgressing (object sender,UploadProgressEventArgs  e)
		{
			if(uploadProgressing != null) { uploadProgressing (sender, e);}
		}

		protected void onFinished (object sender, UploadFinishedArgs e)
		{
			uping = false;
			if(uploadFinished != null ){ uploadFinished (sender, e);}
		}

		protected void onFailed (object sender, UploadFailedEventArgs e)
		{
			uping = false;
			if(uploadFailed != null){  uploadFailed (sender, e);}
		}
	}
}

