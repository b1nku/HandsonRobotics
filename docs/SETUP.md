# Setup

## Prerequisites

| Package | Version |
|---|---|
| Meta XR Core SDK | 85.0.0 |
| Meta XR Interaction SDK OVR | 85.0.0 |
| Meta XR MR Utility Kit | 85.0.0 |
| com.unity.xr.openxr | 1.16.1 |
| com.unity.xr.meta-openxr | 2.4.0 |
| com.unity.robotics.ros-tcp-connector | git (Unity-Robotics-Hub) |
| com.unity.robotics.urdf-importer | git (Unity-Robotics-Hub) |
| com.unity.render-pipelines.universal | 17.4.0 |

Open `Unity_Projects/Virtual_Reality/Hands-on Robotics/` in **Unity 6000.4.0f1**. Build target: **Android**.

Do not hand-edit files under `Assets/RosMessages/`: regenerate via **Robotics > Generate ROS Messages** in the Unity menu if the ROS interface changes.

---

## ROS (Docker)

ROS 1 Noetic has no native Arch packages. The `docker/` directory contains a Docker setup with:

- `roscore`
- `ros-tcp-endpoint` (Unity bridge, port 10000)
- `niryo_one_msgs` message definitions
- `niryo_moveit` custom messages (`NiryoMoveitJoints`, `MoverService`)
- MoveIt full install with `niryo_one_moveit_config`
- `mover.py`: custom single-trajectory planner using `set_position_target` (orientation-free, avoids TIMED_OUT)

**Build and start:**

```bash
cd docker
docker compose up --build
```

Services: `roscore`, `ros-tcp-endpoint`, `moveit` (demo.launch, no RViz), `niryo-moveit` (mover.py service).

**On-device (Quest Pro over Wi-Fi):** set the ROS IP via the tablet ROS Config view before the session. The `ROSConfigurator` component stores the IP in PlayerPrefs; changes take effect on the next app launch. The port defaults to 10000.

**Attach a shell to the running ROS environment:**

```bash
docker compose exec ros-tcp-endpoint bash
```

---

## Linux: Meta XR Simulator Patch

Meta does not ship a Linux binary for the XR Simulator (automatically triggered by Core SDK 85.0.0). Apply this patch before opening the project, then **do not click any simulator install prompts**.

1. Install Core SDK 85.0.0 via Package Manager.
2. Locate `Installer.cs` at:
   ```
   Library/PackageCache/com.meta.xr.sdk.core@<hash>/Editor/MetaXRSimulator/Installer.cs
   ```
3. Replace lines 81-88 with:

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

4. In case of DllNotFoundException libdl -- on modern glibc (Arch and CachyOS) libdl was merged into libc. Create a symlink:

```bash
sudo ln -s /usr/lib/libdl.so.2 /usr/lib/libdl.so
```

---

## Linux: NDK Broken Symlinks

Unity 6000.4.0f1's bundled NDK (r27c) ships with tool symlinks that point to a nested `android-ndk-r27c/` subdirectory that does not exist in the extracted layout on Linux. This causes Android builds to fail with "No such file or directory" errors for `clang`, `clang++`, `llvm-strip`, and potentially other tools.

The root fix creates a symlink that makes the missing subdirectory resolve to the NDK root itself, fixing all dangling tool symlinks in one step:

```bash
ln -sf . "/home/$USER/Unity/Hub/Editor/6000.4.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/NDK/android-ndk-r27c"
```

---

## Linux: Swap

Unity's IL2CPP Android build spawns many parallel shader compiler workers and can exhaust available RAM, causing the OOM killer to terminate Unity or -- with no swap and Wayland -- destabilize the display server entirely.

Add a swapfile before building:

```bash
sudo dd if=/dev/zero of=/swapfile bs=1G count=16 status=progress
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

Also add to `/etc/sysctl.conf`:

```
vm.swappiness=10
vm.vfs_cache_pressure=50
```

Close Steam, ALVR, Docker, and the browser before building. The build takes roughly 70 seconds on a clean run after the IL2CPP compile cache is warm.
