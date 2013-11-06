
namespace QiniuWebDemo
{
	using System;
	using System.Web;
	using System.Web.UI;
	using Qiniu.Cloud;

	public class Token : System.Web.IHttpHandler
	{
		public bool IsReusable {
			get {
				return false;
			}
		}

		public void ProcessRequest (HttpContext context)
		{
			PutPolicy policy = new PutPolicy ("wangming");
			string token = policy.Token ();
			context.Response.Write (token);

//			context.Response.Write ("iN7NgwM31j4-BZacMjPrOQBs34UG1maYCAQmhdCV:IBVwHNHjPo5Zc1AdiZP7j2YeHt8=:eyJzY29wZSI6InF0ZXN0YnVja2V0OmhlbGxveHh4eCIsImRlYWRsaW5lIjoxMzgzMzEyMDE1fQ==");
		}
	}
}

