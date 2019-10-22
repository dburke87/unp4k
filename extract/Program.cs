using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unp4k;
using unforge;
using System.IO;
using Microsoft.VisualBasic;

namespace extract
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0) args = new[] { @"Z:\3.7.0 LIVE.3180042\Data.p4k" };
			if (args.Length == 1) args = new[] { args[0], @"Y:\star-citizen-data" };

			Extract(args[0], args[1]);
		}

		private static void Extract(string dataFilePath, string destinationPath)
		{
			// unp4k / unforge Data.p4k
			unp4k.Program.Unpack(dataFilePath, "*.*", true, destinationPath);
			unforge.Program.Unforge(Path.Combine(destinationPath, "non-dcb", "Data"));
			unforge.Program.Unforge(Path.Combine(destinationPath, "dcb", "Data"));

			// delete bad tagdatabase file
			string badTagDatabase = Path.Combine(destinationPath, @"dcb\Data\libs\foundry\records\tagdatabase\tagdatabase.tagdatabase.xml");
			if (File.Exists(badTagDatabase))
			{
				File.Delete(badTagDatabase);
			}
			else
			{

			}

			// copy non-dcb directory into dcb directory, delete non-dcb, move dcb contents to root, then delete dcb
			CopyDir.Copy(Path.Combine(destinationPath, "non-dcb"), Path.Combine(destinationPath, "dcb"));
			Directory.Delete(Path.Combine(destinationPath, "non-dcb"), true);
			CopyDir.MoveDirectoryContents(Path.Combine(destinationPath, "dcb"), destinationPath);
			Directory.Delete(Path.Combine(destinationPath, "dcb"));

			// decompress
			string objectContainerDirectory = Path.Combine(destinationPath, @"Data\ObjectContainers\PU");
			CopyDir.ExtractDirectory(objectContainerDirectory, true, true);
		}
	}
}
