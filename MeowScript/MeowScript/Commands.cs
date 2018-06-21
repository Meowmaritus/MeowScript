using MeowDSIO.DataTypes.LUAGNL;
using MeowDSIO.DataTypes.LUAINFO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MeowScript
{
	public static class Commands
	{
		private static INI ConfigIni;

		private static string ConfigININame => Utils.Frankenpath(Utils.AssemblyDirectory, "\\MeowScript_Config.ini");

		private static string DarkSoulsDataPath => ConfigIni["General", "DarkSoulsDataPath"];

		static Commands()
		{
			if (!File.Exists(ConfigININame))
			{
				using (Stream stream = typeof(Commands).Assembly.GetManifestResourceStream($"{nameof(MeowScript)}.MeowScript_Config.ini"))
				{
					using (FileStream fs = File.OpenWrite(ConfigININame))
					{
						stream.CopyTo(fs);
					}
				}
			}
			string iniString = File.ReadAllText(ConfigININame);
			ConfigIni = INI.Parse(iniString);
		}

		public static bool Build(string[] args)
		{
			if (args.Length == 0)
			{
				return false;
			}
			ValueTuple<int, string, string, bool, bool> luainfoEntry;
			foreach (string inputFile in args)
			{
				string inputText = File.ReadAllText(inputFile);
				string[] inputLines = inputText.Split('\n');
				var cur_PackageDestinations = new List<(string, string)>();
				var cur_LUAINFO = new List<(int, string, string, bool, bool)>();
                var cur_ExtraGnlEntries = new Dictionary<string, List<string>>();

                var specialTokens = inputLines
                    .Select(x => x.Trim())
                    .Where(x => x.StartsWith("--@"));

				foreach (string item in specialTokens)
				{
					int colonIndex = item.IndexOf(':');
					if (colonIndex == -1)
					{
						throw new Exception($"Invalid special token comment in lua script: \"{item}\"");
					}
					string tokenName = item.Substring("--@".Length, colonIndex - "--@".Length).ToLower();
					string[] tokenParams = item
                        .Substring(colonIndex + 1)
                        .Split(',')
					    .Select(x => x.Trim())
                        .ToArray();

					if (tokenName == "package")
					{
						cur_PackageDestinations.Add((Utils.RemoveExtension(tokenParams[0], ".luabnd"), Utils.RemoveExtension(tokenParams[1], ".lua")));
					}
					else if (tokenName == "battle_goal")
					{
						cur_LUAINFO.Add((int.Parse(tokenParams[0]), tokenParams[1], null, true, false));
					}
					else if (tokenName == "logic_goal")
					{
						cur_LUAINFO.Add((int.Parse(tokenParams[0]), tokenParams[1], tokenParams[2], false, true));
					}
                    else if (tokenName == "misc_goal")
                    {
                        cur_LUAINFO.Add((int.Parse(tokenParams[0]), tokenParams[1], null, false, false));
                    }
                    else if (tokenName == "gnl_entry")
                    {
                        if (!cur_ExtraGnlEntries.ContainsKey(tokenParams[0]))
                        {
                            cur_ExtraGnlEntries.Add(tokenParams[0], new List<string>());
                        }
                        cur_ExtraGnlEntries[tokenParams[0]].Add(tokenParams[1]);
                    }
                }
                foreach (var extraGnlEntry in cur_ExtraGnlEntries)
                {
                    LUABND luabnd = LUABND.Load(DarkSoulsDataPath, extraGnlEntry.Key);
                    foreach (var gnlEntry in extraGnlEntry.Value)
                    {
                        if (!luabnd.GlobalVariableNames.Any((StringRef x) => x.Value == gnlEntry))
                        {
                            luabnd.GlobalVariableNames.Add(gnlEntry);
                        }
                    }
                    LUABND.Save(luabnd, DarkSoulsDataPath, extraGnlEntry.Key);
                }
				foreach (var packageDest in cur_PackageDestinations)
				{
					LUABND luabnd = LUABND.Load(DarkSoulsDataPath, packageDest.Item1);
					List<string> compilationErrors = new List<string>();
					List<string> luaGnlCheckErrors = new List<string>();
					List<string> luaGnlEntries = new List<string>();
					luabnd.AddOrUpdateScript(packageDest.Item2, inputText, compilationErrors, luaGnlCheckErrors, luaGnlEntries);
					if (compilationErrors.Count > 0)
					{
						Console.WriteLine("COMPILE ERRORS:");
						foreach (string item3 in compilationErrors)
						{
							Console.Error.WriteLine(item3);
						}
						throw new Exception();
					}
					if (luaGnlCheckErrors.Count > 0)
					{
						Console.WriteLine("GNL CHECK ERRORS:");
						foreach (string item4 in luaGnlCheckErrors)
						{
							Console.Error.WriteLine(item4);
						}
						throw new Exception();
					}
					foreach (var gnlEntry in luaGnlEntries)
					{
						if (!luabnd.GlobalVariableNames.Any((StringRef x) => x.Value == gnlEntry))
						{
							luabnd.GlobalVariableNames.Add(gnlEntry);
						}
					}
					foreach (var infoEntry in cur_LUAINFO)
					{
						luainfoEntry = infoEntry;
						if (!luabnd.Goals.Any((Goal x) => x.ID == luainfoEntry.Item1))
						{
							luabnd.Goals.Add(new Goal
							{
								ID = luainfoEntry.Item1,
								IsBattleInterrupt = luainfoEntry.Item4,
								IsLogicInterrupt = luainfoEntry.Item5,
								Name = luainfoEntry.Item2,
								LogicInterruptName = luainfoEntry.Item3
							});
						}
					}
					LUABND.Save(luabnd, DarkSoulsDataPath, packageDest.Item1);
				}
			}
			return true;
		}
	}
}
