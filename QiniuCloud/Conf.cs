using System;

namespace Qiniu
{
	public class Conf
	{
		public static string USER_AGENT = "qiniu csharp-sdk v6.0.0";

		private static string accessKEY="";

		/// <summary>
		/// Gets the ACCESSKE.
		/// </summary>
		/// <value>The ACCESSKE.</value>
		public static string ACCESS_KEY {
			get {
				return Conf.accessKEY;
			}
		}

		private static string secretKEY="";

		/// <summary>
		/// Gets the SECRETKE.
		/// </summary>
		/// <value>The SECRETKE.</value>
		public static string SECRET_KEY {
			get {
				return Conf.secretKEY;
			}
		}

		public static void Init(){
			Conf.accessKEY = System.Configuration.ConfigurationManager.AppSettings ["ACCESS_KEY"];
			if(string.IsNullOrEmpty(ACCESS_KEY)){
				throw new ArgumentNullException ("ACCESS_KEY");
			}
			Conf.secretKEY = System.Configuration.ConfigurationManager.AppSettings ["SECRET_KEY"];
			if (string.IsNullOrEmpty (SECRET_KEY)) {
				throw new ArgumentNullException ("SECRET_KEY");
			}
		}
	}
}

