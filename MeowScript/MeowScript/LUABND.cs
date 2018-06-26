using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.BND;
using MeowDSIO.DataTypes.LUAGNL;
using MeowDSIO.DataTypes.LUAINFO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MeowScript
{
	public class LUABND
	{
		public static class ID
		{
			public const int ScriptListStart = 1000;
			public const int GNL = 1000000;
			public const int INFO = 1000001;
		}

		private BNDHeader header;
		private LUAGNL GNL = null;
		private LUAINFO INFO = null;

        public static string INTERROOT => Commands.IsDarkSoulsRemastered ? 
            @"N:\FRPG\data\INTERROOT_x64" :
            @"N:\FRPG\data\INTERROOT_win32";

        public static string FRPG_SCRIPT_DIR => $@"{INTERROOT}\script\";
        public static string FRPG_AI_DIR => $@"{INTERROOT}\script\ai\out\bin\";

        public static string LUAC50_Path => Commands.IsDarkSoulsRemastered ?
            @"lua50_x64\LuaC.exe" :
            @"lua50_x86\luac50.exe";


        public List<Goal> Goals
		{
			get => INFO?.Goals;
			set
			{
				if (INFO != null)
					INFO.Goals = value;
			}
		}

		public List<StringRef> GlobalVariableNames
		{
			get => GNL?.GlobalVariableNames;
            set
			{
				if (GNL != null)
					GNL.GlobalVariableNames = value;
			}
		}

        public List<ScriptRef> Scripts { get; set; }
            = new List<ScriptRef>();

        public List<StringRef> CustomAiScriptIncludes { get; set; } 
            = new List<StringRef>();


		private static void LUAC(string inputFile, string outputFile, List<string> errorList)
		{
			string inputDir = new FileInfo(inputFile).DirectoryName;
			string outputDir = new FileInfo(outputFile).DirectoryName;
			if (!Directory.Exists(inputDir))
			{
				Directory.CreateDirectory(inputDir);
			}
			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}
			ProcessStartInfo luacProcInfo = new ProcessStartInfo();
			luacProcInfo.FileName = Utils.Frankenpath(Utils.ResourceDirectory, LUAC50_Path);
			luacProcInfo.Arguments = $"-o \"{outputFile}\" \"{inputFile}\"";
			luacProcInfo.CreateNoWindow = true;
			luacProcInfo.UseShellExecute = false;
			luacProcInfo.RedirectStandardError = true;
			luacProcInfo.RedirectStandardOutput = true;
			Process luacProc = new Process
			{
				StartInfo = luacProcInfo
			};
			luacProc.Start();
			if (!luacProc.WaitForExit(5000))
			{
				errorList.Add("LUAC process stopped responding.");
			}
			string output = luacProc.StandardOutput.ReadToEnd();
			string error = luacProc.StandardError.ReadToEnd();
			if (!string.IsNullOrWhiteSpace(error))
			{
				errorList.Add(error);
			}
		}

		private static string RUN_LUA(string argString, List<string> errorList)
		{
			ProcessStartInfo luacProcInfo = new ProcessStartInfo();
			luacProcInfo.FileName = Utils.Frankenpath(Utils.ResourceDirectory, LUAC50_Path);
			luacProcInfo.Arguments = $"{argString}";
			luacProcInfo.CreateNoWindow = true;
			luacProcInfo.UseShellExecute = false;
			luacProcInfo.RedirectStandardError = true;
			luacProcInfo.RedirectStandardOutput = true;
			Process luacProc = new Process
			{
				StartInfo = luacProcInfo
			};
			luacProc.Start();
			if (!luacProc.WaitForExit(5000))
			{
				errorList.Add("LUA process stopped responding.");
			}
			string output = luacProc.StandardOutput.ReadToEnd();
			string error = luacProc.StandardError.ReadToEnd();
			if (!string.IsNullOrWhiteSpace(error))
			{
				errorList.Add(error);
			}
			return output;
		}

		public bool? AddOrUpdateScript(string scriptShortName, byte[] bytecode)
		{
			int existingScriptIndex = Scripts.FindIndex((ScriptRef x) => x.Name.ToUpper() == scriptShortName.ToUpper());
			if (existingScriptIndex >= 0)
			{
				if (Scripts[existingScriptIndex].Bytecode.SequenceEqual(bytecode))
				{
					return null;
				}
				Scripts[existingScriptIndex].Bytecode = bytecode;
				return false;
			}
			Scripts.Add(new ScriptRef(scriptShortName, bytecode));
			return true;
		}

		public bool? AddOrUpdateScript(string scriptShortName, string sourceCode, List<string> compilationErrorList, List<string> luaGnlCheckErrorList, List<string> luaGnlList)
		{
			string tempDir = Utils.Frankenpath(Utils.AssemblyDirectory, "\\temp\\");
			string temp_input = Utils.Frankenpath(tempDir, $"{scriptShortName}.lua");
			string temp_output = Utils.Frankenpath(tempDir, $"{scriptShortName}.luac");
			string res_script_luaGnlCheck = Utils.Frankenpath(Utils.ResourceDirectory, "\\LuaGnlCheck.lua");
			string res_script_luaGnlCheck_GlobalStubs = Utils.Frankenpath(Utils.ResourceDirectory, "\\LuaGnlCheck_GlobalStubs.lua");
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}
			byte[] bytecode = null;
			try
			{
				File.WriteAllText(temp_input, sourceCode);
				luaGnlList = luaGnlList.Concat(from x in RUN_LUA($"\"{res_script_luaGnlCheck}\" \"{temp_input}\" \"{res_script_luaGnlCheck_GlobalStubs}\"", luaGnlCheckErrorList).Split('\n')
				select x.Trim()).ToList();
				LUAC(temp_input, temp_output, compilationErrorList);
				if (File.Exists(temp_output))
				{
					bytecode = File.ReadAllBytes(temp_output);
				}
				else if (compilationErrorList.Count == 0)
				{
					compilationErrorList.Add("LUAC function returned no errors and yet no outputted lua bytecode file was found in temp directory.");
				}
			}
			catch (Exception e)
			{
				compilationErrorList.Add($"Exception encountered in LUAC function: {e.Message}");
			}
			finally
			{
				if (File.Exists(temp_input))
				{
					File.Delete(temp_input);
				}
				if (File.Exists(temp_output))
				{
					File.Delete(temp_output);
				}
			}
			if (compilationErrorList.Count == 0)
			{
				return AddOrUpdateScript(scriptShortName, bytecode);
			}
			return null;
		}

		public static void Save(LUABND luabnd, string darkSoulsDataDir, string luaBndName)
		{
			BND newBnd = new BND();
            newBnd.Header = luabnd.header;
            newBnd.FilePath = Utils.Frankenpath(darkSoulsDataDir, $"\\script\\{luaBndName}.luabnd");

            if (Commands.IsDarkSoulsRemastered)
            {
                newBnd.VirtualUri = newBnd.FilePath;
                newBnd.FilePath = newBnd.FilePath + ".dcx";
            }

			int currentScriptID = 1000;
			foreach (ScriptRef script in luabnd.Scripts)
			{
				newBnd.Entries.Add(new BNDEntry(currentScriptID++, Utils.Frankenpath(FRPG_AI_DIR, script.Name + ".lua"), null, script.Bytecode));
            }
			if (luabnd.GNL != null)
			{
				string luagnlUri = Utils.Frankenpath(FRPG_SCRIPT_DIR, $"{luaBndName}.luagnl");
				newBnd.Entries.Add(new BNDEntry(1000000, luagnlUri, null, DataFile.SaveAsBytes(luabnd.GNL, luagnlUri, null)));
			}
			if (luabnd.INFO != null)
			{
				string luainfoUri = Utils.Frankenpath(FRPG_SCRIPT_DIR, $"{luaBndName}.luainfo");
				newBnd.Entries.Add(new BNDEntry(1000001, luainfoUri, null, DataFile.SaveAsBytes(luabnd.INFO, luainfoUri, null)));
			}

            if (Commands.IsDarkSoulsRemastered)
            {
                DataFile.ResaveDcx(newBnd);
            }
            else
            {
                DataFile.Resave(newBnd);
            }
		}

        private static BND GetBndFile(string darkSoulsDataDir, string luaBndName)
        {
            if (Commands.IsDarkSoulsRemastered)
            {
                return DataFile.LoadFromDcxFile<BND>(Utils.Frankenpath(darkSoulsDataDir, $"\\script\\{luaBndName}.luabnd.dcx"));
            }
            else
            {
                return DataFile.LoadFromFile<BND>(Utils.Frankenpath(darkSoulsDataDir, $"\\script\\{luaBndName}.luabnd"));
            }
        }


        public static LUABND Load(string darkSoulsDataDir, string luaBndName)
		{
			LUABND luabnd = new LUABND();
            using (BND bndFile = GetBndFile(darkSoulsDataDir, luaBndName))
            {
                luabnd.header = bndFile.Header;
                luabnd.Scripts.Clear();
                foreach (BNDEntry item in bndFile)
                {
                    if (item.ID == 1000000)
                    {
                        luabnd.GNL = item.ReadDataAs<LUAGNL>(null);
                    }
                    else if (item.ID == 1000001)
                    {
                        luabnd.INFO = item.ReadDataAs<LUAINFO>(null);
                    }
                    else
                    {
                        luabnd.Scripts.Add(new ScriptRef(item.Name.Substring(FRPG_AI_DIR.Length, item.Name.LastIndexOf('.') - FRPG_AI_DIR.Length), item.GetBytes()));
                    }
                }
            }
			return luabnd;
		}
	}
}
