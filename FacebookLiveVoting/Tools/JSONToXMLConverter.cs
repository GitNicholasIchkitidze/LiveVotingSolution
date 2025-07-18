using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FacebookLiveVoting.Tools
{
	public class JSONToXMLConverter
	{
		public static string ConvertJsonToXml(string jsonString)
		{
			JObject jsonObject = JObject.Parse(jsonString);
			XNode convertedXml = JsonConvert.DeserializeXNode(jsonObject.ToString(), "Root");
			return convertedXml.ToString();
		}


	}
}

