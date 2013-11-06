using System;
using Newtonsoft.Json;

namespace Qiniu
{
	[JsonObject(MemberSerialization.OptIn)]
	public class BlkputRet
	{
		[JsonProperty("host")]
		public string host;
		[JsonProperty("ctx")]
		public string ctx;
		[JsonProperty("checksum")]
		public string checkSum;
		[JsonProperty("crc32")]
		public UInt32 crc32;
		[JsonProperty("offset")]
		public UInt32 offset;
	}
}
