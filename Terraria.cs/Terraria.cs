using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using System;
using System.Linq;
using System.IO.Compression;

namespace WindowsGSM.Plugins
{
    public class Terraria 
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Terraria", // WindowsGSM.XXXX
            author = "Andy",
            description = "WindowsGSM plugin for supporting Terraria",
            version = "1.0",
            url = "https://github.com/Kickbut101/WindowsGSM.Terraria", // Github repository link (Best practice)
            color = "#800080" // Color Hex
        };



        // - Standard Constructor and properties

        public Terraria(ServerConfig serverData) => _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;


        // - Game server Fixed variables
        public string StartPath => @"TerrariaServer.exe"; // Game server start path
        public string FullName = "Terraria Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = null; // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()


        // - Game server default values
        public string Port = "7777"; // Default port // Need UDP and TCP
        public string QueryPort = "7777"; // Default query port
        public string Defaultmap = "DefaultWorld"; // Default World Name
        public string Maxplayers = "8"; // Default maxplayers
        public string Additional = ""; // Additional server start parameter

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public async void CreateServerCFG()
        {
          /*  //Download server.cfg
            string configPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"cfx-server-data-master\server.cfg");
            if (await Functions.Github.DownloadGameServerConfig(configPath, FullName))
            {
                string configText = File.ReadAllText(configPath);
                configText = configText.Replace("{{hostname}}", _serverData.ServerName);
                configText = configText.Replace("{{rcon_password}}", _serverData.GetRCONPassword());
                configText = configText.Replace("{{ip}}", _serverData.GetIPAddress());
                configText = configText.Replace("{{port}}", _serverData.ServerPort);
                configText = configText.Replace("{{maxplayers}}", Maxplayers);
                File.WriteAllText(configPath, configText);
            } */
        }




        public async Task<Process> Start()
        {
            string shipWorkingPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID); // c:\windowsgsm\servers\1\serverfiles\
            string terrariaEXEPath = Path.Combine(ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath)); // c:\windowsgsm\servers\1\serverfiles\terrariaserver.exe

            // Does .exe path exist?
            if (!File.Exists(terrariaEXEPath))
            {
                Error = $"{Path.GetFileName(terrariaEXEPath)} not found in ({serverData.ServerID})";
                return null;
            }

            // Prepare start parameters
            var param = new StringBuilder();
            param.Append($" -persistent_storage_root \"{shipWorkingBinPath}\"");
            param.Append($" -conf_dir serverdatafolder");
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerMap) ? string.Empty : $" -cluster \"{_serverData.ServerMap}\""); 
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerMap) ? string.Empty : $" -shard \"{_serverData.ServerMap}_Shard\"");
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -port {_serverData.ServerPort}");
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -steam_master_server_port {_serverData.ServerQueryPort}");
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerMaxPlayer) ? string.Empty : $" -players {_serverData.ServerMaxPlayer}");
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerParam) ? string.Empty : $" {_serverData.ServerParam}");

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false,
                    WorkingDirectory = shipWorkingPath,
                    FileName = terrariaEXEPath,
                    Arguments = param.ToString()
                },
                EnableRaisingEvents = true
            };


            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;

                // Start Process
                try
                {
                    p.Start();
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null; // return null if fail to start
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                return p;

            }
            // Start Process
            try
            {
                p.Start();
                return p;
            }
            catch (Exception e)
            {
                base.Error = e.Message;
                return null; // return null if fail to start
            }
        }

        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                if (p.StartInfo.RedirectStandardInput)
                {
                    p.StandardInput.WriteLine("quit");
                }
                else
                {
                    Functions.ServerConsole.SendMessageToMainWindow(p.MainWindowHandle, "quit");
                }
            });
        }

        public async Task<Process> Install()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string html = await webClient.DownloadStringTaskAsync("https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/");
                    Regex regex = new Regex(@"[0-9]{4}-[ -~][^\s]{39}");
                    var matches = regex.Matches(html);

                    if (matches.Count <= 0)
                    {
                        return null;
                    }

                    //Match 1 is the latest recommended
                    string recommended = regex.Match(html).ToString();

                    //Download server.zip and extract then delete server.zip
                    string serverPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "server");
                    Directory.CreateDirectory(serverPath);
                    string zipPath = Path.Combine(serverPath, "server.zip");
                    await webClient.DownloadFileTaskAsync($"https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/{recommended}/server.zip", zipPath);
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await FileManagement.ExtractZip(zipPath, serverPath);
                        }
                        catch
                        {
                            Error = "Path too long";
                        }
                    });
                    await Task.Run(() => File.Delete(zipPath));

                    //Create FiveM-version.txt and write the downloaded version with hash
                    File.WriteAllText(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "RedM-version.txt"), recommended);

                    //Download cfx-server-data-master and extract to folder cfx-server-data-master then delete cfx-server-data-master.zip
                    zipPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "cfx-server-data-master.zip");
                    await webClient.DownloadFileTaskAsync("https://github.com/citizenfx/cfx-server-data/archive/master.zip", zipPath);
                    await Task.Run(() => FileManagement.ExtractZip(zipPath, Functions.ServerPath.GetServersServerFiles(_serverData.ServerID)));
                    await Task.Run(() => File.Delete(zipPath));
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Process> Update()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string remoteBuild = await GetRemoteBuild();

                    //Download server.zip and extract then delete server.zip
                    string serverPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "server");
                    await Task.Run(() =>
                    {
                        try
                        {
                            Directory.Delete(serverPath, true);
                        }
                        catch
                        {
                            //ignore
                        }
                    });

                    if (Directory.Exists(serverPath))
                    {
                        Error = $"Unable to delete server folder. Path: {serverPath}";
                        return null;
                    }

                    Directory.CreateDirectory(serverPath);
                    string zipPath = Path.Combine(serverPath, "server.zip");
                    await webClient.DownloadFileTaskAsync($"https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/{remoteBuild}/server.zip", zipPath);
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await FileManagement.ExtractZip(zipPath, serverPath);
                        }
                        catch
                        {
                            Error = "Path too long";
                        }
                    });
                    await Task.Run(() => File.Delete(zipPath));

                    //Create FiveM-version.txt and write the downloaded version with hash
                    File.WriteAllText(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "RedM-version.txt"), remoteBuild);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public bool IsInstallValid()
        {
            string exeFile = @"server\FXServer.exe";
            string exePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, exeFile);

            return File.Exists(exePath);
        }

        public bool IsImportValid(string path)
        {
            string exeFile = @"server\FXServer.exe";
            string exePath = Path.Combine(path, exeFile);

            Error = $"Invalid Path! Fail to find {exeFile}";
            return File.Exists(exePath);
        }

        public string GetLocalBuild()
        {
            string versionPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "RedM-version.txt");
           // Error = $"Fail to get local build";
            return File.Exists(versionPath) ? File.ReadAllText(versionPath) : string.Empty;
        }

        public async Task<string> GetRemoteBuild()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string html = await webClient.DownloadStringTaskAsync("https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/");
                    Regex regex = new Regex(@"[0-9]{4}-[ -~][^\s]{39}");
                    var matches = regex.Matches(html);

                    return matches[0].Value;
                }
            }
            catch
            {
                //ignore
            }

            Error = $"Fail to get remote build";
            return string.Empty;
        }
    }
}