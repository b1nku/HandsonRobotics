# Test Day Guide
---

## Pre-requisites

- Meta Quest HMD
- Windows PC (With Docker)
- A phone to use as a Wi-Fi hotspot

---

## 1. Download the APK and Sideload

Go to the [Releases page](https://github.com/b1nku/HandsonRobotics/releases) and download the latest `.apk` file.

---

## 2. Set Up the Network

Use a **phone hotspot** -- campus Wi-Fi often blocks device-to-device traffic which breaks the ROS connection.

1. Enable hotspot on your phone
2. Connect the **PC** to the hotspot
3. Connect the **Quest Pro** to the hotspot (Settings -> Wi-Fi inside the headset)
4. On the PC, find its IP address:
   - Open Command Prompt and run `ipconfig`
   - Look for the **Wi-Fi adapter** section
   - Write down the **IPv4 Address** (looks like `192.168.x.x`)

---

## 3. Start ROS on the PC

1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop/) if not already installed and make sure it is running
2. Download the repository as a zip from GitHub and extract it, or clone it if you have git
3. Open Command Prompt, navigate to the `docker` folder:
   ```
   cd path\to\HandsOnRobotics\docker
   ```
4. Start the ROS stack:
   ```
   docker compose up --build
   ```
   The first run downloads images and should take only a few minutes.
5. Wait until you see `[niryo_moveit] Service ready.` in the output.

**Windows Firewall:** if the headset cannot connect, Windows may be blocking port 10000. Allow it:
- Search "Windows Defender Firewall" -> Advanced Settings -> Inbound Rules -> New Rule -> Port -> TCP -> 10000 -> Allow

---

## 4. Configure the ROS IP in the App

The app needs to know the PC's IP address. This is saved between sessions so you only need to do this when the IP changes.

1. Launch **Hands-on Robotics** from Unknown Sources on the Quest
2. On the floating tablet, navigate to the **config** view (use the arrow buttons at the bottom navigation bar of the tablet)
3. Type in the PC's IP address from Step 3 and press **Save**
4. Close and relaunch the app -- the new IP takes effect on the next launch

---

## 5. Room Setup

1. Go to **Settings -> Physical Space -> Space Setup** on the Quest
2. Follow the prompts to scan the room, add walls - change their heights. Furniture as well :D

This only needs to be done once per room. Skip this step if the desk has been scanned previously.

---

## 6. Launch and Place

1. Launch the app
2. You will see the virtual workstation - you should be able to grab it and move it to a more convenient location.
3. Once placed it **should** lock in position

---

## Troubleshooting

| Problem | Fix |
|---|---|
| Topics all show grey / no connection | Check Docker is running and `[niryo_moveit] Service ready.` appeared. Check the IP in ROS Config matches the PC's current IP. Both devices on the same hotspot? |
| App crashes on launch | Relaunch from Unknown Sources. If it keeps crashing, reinstall the APK. |
| Docker command not found | Make sure Docker Desktop is running (check the system tray). |
| Planning always fails | The target sphere may be outside the arm's reach. Place it within the blue workspace envelope sphere. |
