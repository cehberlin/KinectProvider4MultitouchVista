KinectProvider4MultitouchVista
==============================

This is a Kinect provider for the virtual touch driver from multitouch vista detecting touch with a virtual touch plane and not with finger or hand gestures.
You can even use a virtual touch-plane absolute to the sensor position or relative to the user body.

Use
---
- Ensure prerequisites
- Build the library or use provided binaries! See below for building
- Install driver from the binary package depending on your Windows version (e.g. Run "Driver\x64\Install driver.cmd" from a cmd with administrator rights)
- Activate the driver by deactivating and activating once the Device "Universal Software HID device" in your Windows device manager
- Start Multitouch.Service.Console.exe
- Start Multitouch.Configuration.WPF.exe and enable KinectProvider provider
- [Configure KinectProvider inside its configuration window, configuration is updated during runtime of the provider every few seconds]
- [The configuration further allows you to configure the mode (relative to user or absolute to kinect touch plane), as well as the used offsets]
- [You are now able to test the input, but be aware, no touch input is processed, you can just play with the cursors. Filled circle is a click, Unfilled just for position]
- Start Multitouch.Driver.Console.exe for enabling touch processing through multitouchvista


Build
-----

Prerequisites:

Libraries:
- openni-win32-1.5.4.0-dev

Driver:
- nite-win32-1.5.2.21-dev

Probably newer versions of libs and drivers, but these are the versions I used.

Build together with Multitouchvista:
- After cloning the repository copy the folder into "Main Source\InputProviders" of the MultiTouchVista source tree
- Add the project KinectProviderBuildInsideMultitouchvista.csproj.csproj to solution Multitouch.InputProviders.sln]
- Build project and all multitouchvista solutions

Build standalone:
- Clone the repository and download my build of multitouchvista (at time of writting MultiTouchVista_bin_69631.zip)
- Use for building the solution KinectProviderBuildStandAlone.sln with KinectProviderStandAlone.csproj
- This solution is using included dlls from the multitouchvista framework which are located in the folder libStandAloneBuild
	- Multitouch.Contracts.dll and System.ComponentModel.Composition
- Moreover this solution builds to "bin\[Debug|Release]\KinectProvider"
- After building just copy the created directory "KinectProvider" from "bin\Debug" or "bin\Release" to the AddIns directory of your downloaded MultiTouchVista binaries 
  (In fact I was not able to get it running with MultiTouchVista_-_second_release_-_refresh_2.zip - It seems like there is something different to the svn version 69631 I used to create this)
- It is may necessary to replace the provided and included Multitouch.Contracts.dll after a newer release of MultiTouchVista

Other build and development instructions:
- If you want to use the library with Windows 8, especially with the new startscreen, you should may edit the CursorFactory.cs for using a different cursor. 
This cursor will work on the new start screen, but is looking less nice and will only work in a one screen setup and needs more performance. 
You should first try the default implementation.

Licence:
--------

Copyright 2012 Christopher-Eyk Hrabia

KinectProvider4MultitouchVista is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 2 of the License, or (at your option) any later version.

KinectProvider4MultitouchVista is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with KinectProvider4MultitouchVista. If not, see http://www.gnu.org/licenses/.
