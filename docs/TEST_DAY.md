# Test Day Guide

Read this top to bottom once before starting. The whole setup takes about 15 minutes.

---

## What You Need

- Meta Quest Pro (charged)
- Windows PC (Docker Desktop installed)
- USB-C cable (for sideloading)
- A phone to use as a Wi-Fi hotspot

---

## 1. Download the APK

Go to the [Releases page](https://github.com/b1nku/HandsonRobotics/releases) and download the latest `.apk` file.

---

## 2. Sideload to the Quest Pro

**First time only:** enable Developer Mode on the headset.
1. Open the Meta app on your phone
2. Go to Menu -> Devices -> select the Quest Pro -> Developer Mode -> turn it on

**Install the APK:**
1. Install [Meta Quest Developer Hub](https://developer.oculus.com/meta-quest-developer-hub/) on the PC if not already installed
2. Connect the Quest Pro to the PC via USB-C
3. Put on the headset and accept the "Allow USB Debugging" prompt
4. In MQDH, go to Device Manager and drag the `.apk` onto the window -- or use the "Install APK" button
5. The app appears under **Unknown Sources** in the Quest library as **Hands-on Robotics**

---

## 3. Set Up the Network

Use a **phone hotspot** -- campus Wi-Fi often blocks device-to-device traffic which breaks the ROS connection.

1. Enable hotspot on your phone
2. Connect the **PC** to the hotspot
3. Connect the **Quest Pro** to the hotspot (Settings -> Wi-Fi inside the headset)
4. On the PC, find its IP address:
   - Open Command Prompt and run `ipconfig`
   - Look for the **Wi-Fi adapter** section
   - Write down the **IPv4 Address** (looks like `192.168.x.x`)

---

## 4. Start ROS on the PC

1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop/) if not already installed and make sure it is running
2. Download the repository as a zip from GitHub (green Code button -> Download ZIP) and extract it, or clone it if you have git
3. Open Command Prompt, navigate to the `docker` folder:
   ```
   cd path\to\HandsOnRobotics\docker
   ```
4. Start the ROS stack:
   ```
   docker compose up --build
   ```
   The first run downloads images and takes a few minutes. Subsequent runs are fast.
5. Wait until you see `[niryo_moveit] Service ready.` in the output. ROS is ready.

**Windows Firewall:** if the headset cannot connect, Windows may be blocking port 10000. Allow it:
- Search "Windows Defender Firewall" -> Advanced Settings -> Inbound Rules -> New Rule -> Port -> TCP -> 10000 -> Allow

---

## 5. Configure the ROS IP in the App

The app needs to know the PC's IP address. This is saved between sessions so you only need to do this when the IP changes.

1. Launch **Hands-on Robotics** from Unknown Sources on the Quest
2. On the floating tablet, navigate to the **ROS Config** view (use the arrow buttons at the top of the tablet)
3. Type in the PC's IP address from Step 3 and press **Save**
4. Close and relaunch the app -- the new IP takes effect on the next launch

---

## 6. Room Setup (First Time on a New Desk)

The app anchors the virtual robot to a physical desk using the Quest's room scan. If this desk has not been scanned before:

1. Go to **Settings -> Physical Space -> Space Setup** on the Quest
2. Follow the prompts to scan the room and desk
3. Make sure the desk is labelled as **Table** when the scan completes

This only needs to be done once per room. Skip this step if the desk has been scanned previously.

---

## 7. Launch and Place

1. Launch the app
2. You will see the virtual workstation floating in space -- **grab it with your hand and place it on the physical desk**
3. Once placed it locks in position

---

## 8. What to Test

Everything is controlled from the **floating tablet**. Grab it by the handles on the side.

| Feature | How to use |
|---|---|
| **Joint health rings** | Look at any joint on the robot arm -- rings show torque, temperature, voltage. Gaze at a ring to see the detail panel. |
| **Topic monitor** | Navigate to the Topics view on the tablet. Shows live ROS topic rates and connection status. |
| **TF frame overlay** | Toggle via the tablet. Shows the coordinate frame of each joint as colored axes. |
| **Trajectory planning** | Navigate to Planning view. Press Place Target, grab the red sphere and move it to where you want the arm to go, press Plan. The ghost arm previews the trajectory. |
| **Workspace envelope** | Appears automatically as a transparent sphere when placing the trajectory target. Shows the arm's reach limit. |
| **ROS debug panel** | Toggle via the debug button on the tablet. Shows on the wall behind the robot. |
| **Tablet physics** | Throw the tablet -- it bounces off the real walls. Not a research feature. |

**Note:** the Execute button on the Planning view does not send commands to the real robot yet. The preview and planning pipeline are fully functional.

---

## Troubleshooting

| Problem | Fix |
|---|---|
| Topics all show grey / no connection | Check Docker is running and `[niryo_moveit] Service ready.` appeared. Check the IP in ROS Config matches the PC's current IP. Both devices on the same hotspot? |
| App crashes on launch | Relaunch from Unknown Sources. If it keeps crashing, reinstall the APK. |
| Virtual robot not on the desk | Grab the workstation and re-place it. If it is very far off, re-run Space Setup on the Quest. |
| Docker command not found | Make sure Docker Desktop is running (check the system tray). |
| Planning always fails | The target sphere may be outside the arm's reach. Place it within the blue workspace envelope sphere. |
