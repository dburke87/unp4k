﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unforge
{
    public class Program
	{
		static void Main(params String[] args)
		{
			if (args.Length == 0)
			{
				args = new String[] { @"Y:\star-citizen-data\non-dcb\Data" };
				//args = new String[] { "game.v4.dcb" };
				// args = new String[] { "wrld.xml" };
				// args = new String[] { "Data" };
				// args = new String[] { @"S:\Mods\BuildXPLOR\archive-3.0\661655\Data\game.dcb" };
			}

			if (args.Length < 1 || args.Length > 1)
			{
				Console.WriteLine("Usage: unforge.exe [infile]");
				Console.WriteLine();
				Console.WriteLine("Converts any Star Citizen binary file into an actual XML file.");
				Console.WriteLine("CryXml files (.xml) are saved as .raw in the original location.");
				Console.WriteLine("DataForge files (.dcb) are saved as .xml in the original location.");
				Console.WriteLine();
				Console.WriteLine("Can also convert all compatible files in a directory, and it's");
				Console.WriteLine("sub-directories. In that case, all CryXml files are saved in-place,");
				Console.WriteLine("and any DataForge files are saved to both .xml and extracted to");
				Console.WriteLine("the original component locations.");
				return;
			}

			Unforge(args[0]);
		}

		public static void Unforge(string directory)
		{
			if (Directory.Exists(directory))
			{
				foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
				{
					if (new String[] { "ini", "txt" }.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase)) continue;

					try
					{
						Console.WriteLine("Converting {0}", file.Replace(directory, ""));

						Smelter.Instance.Smelt(file);
					}
					catch (Exception) { }
				}
			}
			else
			{
				Smelter.Instance.Smelt(directory);
			}
		}
    }

	public class Smelter
	{
		public static Smelter Instance => new Smelter { };

		private Boolean _overwrite;

		public void Smelt(String path, Boolean overwrite = true)
		{
			this._overwrite = overwrite;

			try
			{
				if (File.Exists(path))
				{
					if (Path.GetExtension(path) == ".dcb")
					{
						using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
						{
							var legacy = new FileInfo(path).Length < 0x0e2e00;

							var df = new DataForge(br, legacy);

							df.Save(Path.ChangeExtension(path, "xml"));
						}
					}
					else
					{
						if (!_overwrite)
						{
							if (!File.Exists(Path.ChangeExtension(path, "raw")))
							{
								File.Move(path, Path.ChangeExtension(path, "raw"));
								path = Path.ChangeExtension(path, "raw");
							}
						}

						var xml = CryXmlSerializer.ReadFile(path);

						if (xml != null)
						{
							xml.Save(Path.ChangeExtension(path, "xml"));
						}
						else
						{
							Console.WriteLine("{0} already in XML format", path);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error converting {0}: {1}", path, ex.Message);
			}
		}
	}
}
