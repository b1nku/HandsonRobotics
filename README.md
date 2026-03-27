# HandsonRobotics

Existing research indicates that the Virtual Reality platform has considerable potential in lowering the cognitive load of robotics developers and operators.  

This project will explore developing tools for robotics developers in Virtual Reality based on interaction design principles, such that it does not contribute to cognitive overhead.

Unity ver.: 6000.4.0f1  
VR Platform: Meta Quest  
For ROS-enabled robotics  

## Prerequisites
Meta XR Interaction SDK 85.0.0  
Meta XR Core SDK 85.0.0

## Linux Developers BEWARE !
Meta in their infinite wisdom have not compiled a version of their Meta XR simulator for linux (automatically installed by their core SDK package) -> manual fix is needed
Do not click any prompts to install the XR simulator after patching:
1) Install Core SDK 85.0.0
2) Locate Installer.cs at Library/PackageCache/com.meta.xr.sdk.core@f8b4cfb2789f/Editor/MetaXRSimulator/
3) Update lines 81-88:
```csharp
#if UNITY_EDITOR_WIN
            var downloadedInstallerPath =
                            Path.Combine(XRSimConstants.DownloadFolderPath, $"meta_xr_simulator.msi");
#elif UNITY_EDITOR_OSX
            var downloadedInstallerPath =
                            Path.Combine(XRSimConstants.DownloadFolderPath, $"meta_xr_simulator.dmg");
#else
	    var downloadedInstallerPath = string.Empty;
	    return false; // XR Simulator not supported on linux lmao get rekt n00b
#endif
            if (!await DownloadInstaller(downloadedInstallerPath, downloadUrl, errorMessage =>
                {
                    onError?.Invoke(errorMessage);
                }))
            {
                return false;
            }
```
