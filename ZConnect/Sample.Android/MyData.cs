using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.MyAndroid
{
	class MyData
	{
		public static string GetFileTitle(string isbn, string jan)
		{
			return String.Format("isbn_{0}_jan_{1}", isbn, jan);
		}
		public static string GetDatFilePath(string genreName, string isbn, string jan)
		{
			return GetDatDirectory(genreName) + "/" + GetFileTitle(isbn, jan) + ".dat";
		}
 		public static string GetImgFilePath(string genreName, string isbn, string jan) {
			return GetImgDirectory(genreName) + "/" + GetFileTitle(isbn, jan) + ".jpg";
		}
		public static string GetDatDirectory(string genreName)
		{
			return Constant.APP_NAME + "/Dat/" + genreName;
		}
		public static string GetImgDirectory(string genreName)
		{
			return Constant.APP_NAME + "/Img/" + genreName;
		}
	}
}
