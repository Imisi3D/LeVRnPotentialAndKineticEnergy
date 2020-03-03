# LeVRnPotentialAndKineticEnergy
An educational VR project developed to teach Junior Secondary School students about Potential and Kinetic Energy.

## Prerequisites
- Unity 2019.1.1f1

## Getting Started
These instructions will get a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on your Oculus Go.

## Installing
To get a copy of this project working properly on your local machine, follow these simple steps:

- Download the project folder.
- Open the project with Unity 2019.1.1f1.

To keep the git repository small in size, a lot of the large assets used are not included in the repo but are hosted somewhere else as a .unitypackage.

- Download the .unitypackage here [leVRn-Non-Communicable-Diseases UnityPackage](https://drive.google.com/file/d/1ecPvgyHXnRFar_76sW94xNIIqjv-Ntic/view?usp=sharing)
- Import the leVRn-Potential-And-Kinetic-Energy UnityPackage into the project.
- Ensure the platform is set to Android in Build Settings.
- Download the Oculus SDK used for this project [here](https://drive.google.com/file/d/1NmoFQmk7qaPrjr9PDjEzkipx2NsY7zvs/view?usp=sharing).
- Import the Oculus SDK into the project.
- Build the .apk, load it into an Oculus Go and test the experience to ensure everything works fine.


## Deployment

### On your phone
- Open the Oculus app on your phone
- Click your paired Oculus Go headset at the top
- Click More Settings > Developer Mode and then toggle Developer Mode ON
- Connect the Oculus Go to your computer
- Accept the RSA key request on the Oculus Go

### On your computer
If you are on Windows, you could check if these files have already been downloaded and installed
- [Android Platform tool](https://developer.android.com/studio/releases/platform-tools)
- [ADB drivers](https://developer.oculus.com/downloads/package/oculus-go-adb-drivers/) from Oculus website
    - Unzip the downloaded file
    - Right-click on the .inffile and select install
    - Go to Start Menu, search for CMD, right-click and open as administrator

### On your opened terminal
- Type "adb devices" and press enter
    - If you don’t see any devices listed in the output, turn off your Go, disconnect the USB, power your Go back on, reconnect the USB and then run the command again
    - If you see a device ID show up in the output with “unauthorized” behind it, put your Oculus Go on and give permission
    - If you see a device ID show up with “device” behind it, you are good to go and ready for the next step
- If your Oculus Go is still not found on the terminal
    - Go to the INSTALLEDPATH of the Android Platform tool on your computer (usually, it is found in C:\Users\YOURUSERNAME\AppData\Local\Android\sdk\platform-tools or C:\Users\YOURUSERNAME\AndroidPlatformTools\Android\sdk\platform-tools) and check if adb.exe is there
    - Copy the directory (right-click on the address bar and copy the address)
    - Go back to command prompt and type "cd TheDirectoryYouCopied" and press enter
    - Then type adb
    - Connect your Oculus Go to your computer
    - Type adb devices (your device ID should appear)
- Then type "adb install PathToYourApk\ApkName.apk" and press enter

## Contributing
Please read [CONTRIBUTING.md](https://github.com/Imisi3D/LeVRn_Non_Communicable_Diseases/blob/master/Contributing.md) for details on how best to contribute to this project.

## Authors
- **Isiaq Gbadamosi** - [Github](https://github.com/badmusishaq)
