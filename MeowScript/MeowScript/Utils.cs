using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace MeowScript
{
	public static class Utils
	{
        public static string ResourceDirectory => 
            Utils.Frankenpath(AssemblyDirectory, "Resources");

		public static string AssemblyDirectory
		{
			get
			{
                return new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName; 
                
                //Jebaited

				//string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				//UriBuilder uri = new UriBuilder(codeBase);
				//string path = Uri.UnescapeDataString(uri.Path);
				//return Path.GetDirectoryName(path);
			}
		}

		public static string RemoveExtension(string filePath, string extension)
		{
			if (filePath.ToUpper().EndsWith(extension.ToUpper()))
			{
				return filePath.Substring(0, filePath.Length - extension.Length);
			}
			return filePath;
		}

		public static string Frankenpath(params string[] pathParts)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < pathParts.Length; i++)
			{
				sb.Append(pathParts[i].Trim('\\'));
				if (i < pathParts.Length - 1)
				{
					sb.Append('\\');
				}
			}
			return sb.ToString();
		}
	}
}
