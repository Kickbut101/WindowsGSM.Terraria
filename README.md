# WindowsGSM.Terraria
🧩 WindowsGSM plugin for supporting Terraria

## Requirements
[WindowsGSM](https://github.com/WindowsGSM/WindowsGSM) >= 1.21.0

.NET Framework 4.0 (or greater) installed

XNA Framework 4.0 installed (you can get [Here](https://web.archive.org/web/20201222035408if_/https://download.microsoft.com/download/A/C/2/AC2C903B-E6E8-42C2-9FD7-BEBAC362A930/xnafx40_redist.msi))

## Installation
1. Move **Terraria.cs** folder to **plugins** folder
1. Click **[RELOAD PLUGINS]** button or restart WindowsGSM

## Additional Command Line options

<ul><li><code>-config &lt;file path&gt;</code> - Specifies a configuration file to use (see <a href="#serverconfig">Server config file</a> below).</li>
<li><code>-port &lt;number&gt;</code> - Specifies the port to listen on.</li>
<li><code>-players &lt;number&gt; / -maxplayers &lt;number&gt;</code> - Sets the max number of players.</li>
<li><code>-pass &lt;password&gt; / -password &lt;password&gt;</code> - Sets the server password.</li>
<li><code>-motd &lt;text&gt;</code> - Set the server motto of the day text.</li>
<li><code>-world &lt;file path&gt;</code> - Load a world and automatically start the server.</li>
<li><code>-autocreate &lt;number&gt;</code> - Creates a world if none is found in the path specified by -world. World size is specified by: 1(small), 2(medium), and 3(large).</li>
<li><code>-banlist &lt;file path&gt;</code> - Specifies the location of the banlist. Defaults to "banlist.txt" in the working directory.</li>
<li><code>-worldname &lt;world name&gt;</code> - Sets the name of the world when using -autocreate.</li>
<li><code>-secure</code> - Adds additional cheat protection to the server.</li>
<li><code>-noupnp</code> - Disables automatic universal plug and play.</li>
<li><code>-steam</code> - Enables Steam support.</li>
<li><code>-lobby friends / -lobby private</code> - Allows only friends to join the server or sets it to private if Steam is enabled.</li>
<li><code>-ip &lt;ip address&gt;</code> - Sets the IP address for the server to listen on</li>
<li><code>-forcepriority &lt;priority&gt;</code> - Sets the process priority for this task. If this is used the "priority" setting below will be ignored.</li>
<li><code>-disableannouncementbox</code> - Disables the text announcements Announcement Box makes when pulsed from wire.</li>
<li><code>-announcementboxrange &lt;number&gt;</code> - Sets the announcement box text messaging range in pixels, -1 for serverwide announcements.</li>
<li><code>-seed &lt;seed&gt;</code> - Specifies the world seed when using -autocreate <span id="serverconfig"></span></li></ul>

### License
This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/BattlefieldDuck/WindowsGSM.ARMA3/blob/master/LICENSE) file for details

