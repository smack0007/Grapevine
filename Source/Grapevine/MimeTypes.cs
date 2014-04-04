using System;
using System.IO;

namespace Grapevine
{
	public static class MimeTypes
	{
		public static string GetMimeTypeForExtension(string extension)
		{
			switch (extension)
			{
				case ".css": return "text/css";
				case ".html": return "text/html";
				case ".js": return "application/x-javascript";
				case ".json": return "application/json";
			}

			return "application/octet-stream";
		}

		public static string GetMimeType(string fileName)
		{
			return GetMimeTypeForExtension(Path.GetExtension(fileName));
		}
	}
}
