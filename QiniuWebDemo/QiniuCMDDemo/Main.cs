using System;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using Qiniu.Cloud;
namespace Qiniu
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Conf.Init ();
			FileManger ();
			return;

			Console.ReadLine ();
		}

		private static void upload(){
			QiniuLocalEntry entry = new QiniuLocalEntry ("/Users/icattlecoder/Movies/Win8_Activation_XP85.rar");

			entry.uploadFinished += (o,e)=>{
				Console.WriteLine("hash={0}",e.Result["hash"]);
			};

			entry.uploadProgressing += (o, e) => {
				Console.WriteLine("uploaded {0}",(e.BytesSent*100.0f)/e.TotalBytes);
			};
			entry.uploadFailed += (o, e) => {
				Console.Write("failed:{0}",e.Msg);
			};

			NameValueCollection vals = new NameValueCollection ();
			vals.Add ("x:alskdf", "lsdjf");
			vals.Add ("x:asdf", "lsdf");
			byte[] bytes= new byte[100];
			Stream stream = new MemoryStream(bytes);
			try{

				WebClient wc=new WebClient();
				string token = wc.DownloadString(@"http://localhost:8080/Token.ashx");
				entry.UploadFile ("iN7NgwM31j4-BZacMjPrOQBs34UG1maYCAQmhdCV:IBVwHNHjPo5Zc1AdiZP7j2YeHt8=:eyJzY29wZSI6InF0ZXN0YnVja2V0OmhlbGxveHh4eCIsImRlYWRsaW5lIjoxMzgzMzEyMDE1fQ==",
				                  "helloxxxx",null,true,null);
			}catch(Exception e) {
				Console.WriteLine ("exception={0}", e.Message);
			}
		}

		private static void FileManger(){
			QiniuCloudFileManger fileManger = new QiniuCloudFileManger ();
			string res =fileManger.Stat (new QiniuCloudEntry ("wangming", "bucketMgr.md"));
			Console.WriteLine ("lien", res);
		}

	}
}
