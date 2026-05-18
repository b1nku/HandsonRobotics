# Project Setup Guide

This guide covers getting the project running on a fresh machine. It covers both Linux and Windows. Mac is not a target platform and is not covered.

---

## Prerequisites

### All platforms

| Tool | Version | Notes |
|---|---|---|
| Git | Any recent | |
| Unity Hub | Latest | Used to install the correct Unity version |
| Unity | 6000.4.0f1 | Install via Unity Hub; include Android Build Support + Android SDK & NDK Tools |
| Docker | Latest | See platform sections below |

Unity 6000.4.0f1 can be installed via Unity Hub under **Installs > Add > Archive** if it does not appear in the standard list.

Android Build Support (including Android SDK & NDK Tools and OpenJDK) must be added as a module to the Unity installation. In Unity Hub: **Installs > (gear icon on your 6000.4.0f1 install) > Add Modules**.

---

## Linux

### Docker

Install Docker Engine and the Compose plugin. On Arch-based systems (CachyOS, Manjaro, etc.):

```bash
sudo pacman -S docker docker-compose
sudo systemctl enable --now docker
sudo usermod -aG docker $USER   # log out and back in after this
```

On Debian/Ubuntu-based systems, follow the official Docker Engine install guide (do not use the snap package).

### Meta XR Simulator patch

Meta does not ship a Linux binary for their XR Simulator, which the Core SDK tries to install automatically. You must patch one file before opening the project, then never click any simulator install prompts.

1. Install Meta XR Core SDK 85.0.0 via Unity Package Manager.
2. Locate the file:
   ```
   Library/PackageCache/com.meta.xr.sdk.core@<hash>/Editor/MetaXRSimulator/Installer.cs
   ```
3. Replace lines 81-88 with the following:

```csharp
#if UNITY_EDITOR_WIN
            var downloadedInstallerPath =
                            Path.Combine(XRSimConstants.DownloadFolderPath, $"meta_xr_simulator.msi");
#elif UNITY_EDITOR_OSX
            var downloadedInstallerPath =
                            Path.Combine(XRSimConstants.DownloadFolderPath, $"meta_xr_simulator.dmg");
#else
            var downloadedInstallerPath = string.Empty;
            return false;
#endif
            if (!await DownloadInstaller(downloadedInstallerPath, downloadUrl, errorMessage =>
                {
                    onError?.Invoke(errorMessage);
                }))
            {
                return false;
            }
```

This patch makes the installer silently bail out on Linux instead of throwing an error. The simulator itself is not needed for development; Quest Link or a physical device handles that.

---

## Windows

### Docker Desktop

Download and install Docker Desktop from the official Docker website. During setup:

- Enable the WSL2 backend (recommended and default on modern Windows).
- Start Docker Desktop and let it finish initializing before running any compose commands.

Docker Desktop automatically exposes container ports to `localhost`, so the same `docker compose up` command works without any changes.

### WSL2 (recommended for command-line work)

While Docker Desktop handles the container side, running Git and shell commands from WSL2 is more convenient than PowerShell for this project. Install WSL2 with Ubuntu:

```powershell
wsl --install
```

You can clone the repo into your Windows filesystem (e.g. `C:\Users\you\Projects`) and access it from WSL2 at `/mnt/c/Users/you/Projects/...`, or clone directly inside WSL2. Either works; keep all project files on one side to avoid permission and line-ending issues.

---

## Cloning the repository

```bash
git clone <repo-url>
cd "Hands-on Robotics"
```

---

## Building the ROS Docker image

The `docker/` directory contains a `Dockerfile` that builds a ROS Noetic image with:

- `roscore`
- `ros_tcp_endpoint` (the Unity-ROS bridge, port 10000)
- `niryo_one_msgs` (Niryo One message definitions)
- `niryo_moveit` (custom pick-and-place messages: `NiryoMoveitJoints`, `MoverService`)
- MoveIt msgs and planning interface

Build the image (this takes a few minutes the first time; subsequent builds use the cache):

```bash
cd docker
docker compose build
```

This command is the same on Linux and Windows.

---

## Running ROS

```bash
cd docker
docker compose up
```

You should see roscore start, followed by the tcp endpoint announcing it is listening on port 10000.

To run in the background:

```bash
docker compose up -d
```

To stop:

```bash
docker compose down
```

To open a shell inside the running ROS environment (useful for running `rostopic echo`, `rosnode list`, etc.):

```bash
docker compose exec ros-tcp-endpoint bash
```

---

## Opening the Unity project

1. Open Unity Hub.
2. Click **Add > Add project from disk**.
3. Navigate to the `Hands-on Robotics/` folder and select it.
4. Make sure the editor version shown is **6000.4.0f1**. If it shows a different version, click the version dropdown and select 6000.4.0f1.
5. Open the project. First load will take several minutes as packages are imported.

### Linux only: apply the Meta XR Simulator patch now

If you have not already applied the patch described above, do it before the editor finishes loading. The package is installed to `Library/PackageCache/` during first import.

---

## Connecting Unity to ROS

1. In the Unity editor, go to **Robotics > ROS Settings**.
2. Set **ROS IP Address** to `127.0.0.1` (for editor testing on the same machine).
3. Set **ROS Port** to `10000`.
4. Leave **Protocol** as ROS1.

The `ROSConnectionPrefab` in `Assets/Resources/` stores these settings for runtime use and is already configured to these defaults.

### On-device testing (Quest Pro over wifi)

When building and running on the headset, the device connects to your machine over the local network. Change **ROS IP Address** in ROS Settings to your machine's LAN IP address (e.g. `192.168.x.x`). The Docker port mapping exposes port 10000 on all host interfaces, so no other changes are needed.

---

## Verifying the connection

With `docker compose up` running and the Unity editor open:

1. Enter Play mode in Unity.
2. In the Docker terminal you should see a line like:
   ```
   [INFO] Connection from 127.0.0.1
   ```
3. In the ROS shell (`docker compose exec ros-tcp-endpoint bash`), run:
   ```bash
   rostopic list
   ```
   Unity-published topics will appear here once the scene is running.

---

## Rebuilding the image

If `docker/Dockerfile` changes (e.g. a new ROS package is added), rebuild with:

```bash
docker compose build --no-cache
```

Omit `--no-cache` to reuse cached layers where possible.

---

## Troubleshooting

**Unity cannot connect to ROS endpoint**
- Confirm `docker compose up` is running and shows no errors.
- Check that port 10000 is not blocked by a firewall.
- On Windows, confirm Docker Desktop is running (the tray icon should be active).

**`rostopic list` returns an error inside the container**
- The endpoint container waits for roscore via a retry loop; give it 10-15 seconds after `docker compose up` before running commands.

**Unity shows "ROS message type not found" errors**
- The message types built into the image are `niryo_one_msgs` and `niryo_moveit`. Any other custom message packages need to be added to the Dockerfile and the image rebuilt.

**Linux: editor crashes on opening Meta XR package**
- The Meta XR Simulator patch was not applied in time. Delete `Library/` to force a clean reimport, apply the patch during reimport, and do not click any simulator prompts.
