using System;
using System.Collections.Generic;

namespace Qiniu
{
	public class  UploadFinishedArgs:EventArgs{

		private Dictionary<string,string> result;

		public Dictionary<string, string> Result {
			get {
				return result;
			}
		}

		public UploadFinishedArgs(Dictionary<string,string>  result){
			this.result = result;
		}

	}
}

