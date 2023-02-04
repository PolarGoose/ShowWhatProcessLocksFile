# ShowWhatProcessLocksFile
A utility to discover what processes lock a specific file or folder.

# Screenshots
## Context menu
<img src="doc/ContextMenu.png" width="70%" height="70%"/>

## Application
<img src="doc/Screenshot.png" width="70%" height="70%"/>

# System requirements
* Windows 10 or higher (it can also work on Windows 8 if you install [.Net Framework 4.6.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net462-web-installer))
* The user should be allowed to run applications as an Administrator.

# How it works
The application uses [Handle2](https://github.com/PolarGoose/Handle2) to get information about locking processes.

# How to use
* Download `ShowWhatProcessLocksFile.msi.zip` from the latest [release](https://github.com/PolarGoose/ShowWhatProcessLocksFile/releases).
* Run the installer. The installer will install this programm to the `%AppData%\ShowWhatProcessLocksFile` folder and add a `Show what locks this file` Windows File Explorer context menu element.
* Use `Show what locks this file` File Explorer's context menu to select a file or folder
* To terminate a process, select it and open a context menu by clicking right mouse button
* If you want to uninstall the program, use `Control Panel\Programs\Programs and Features`, uninstaller will remove an integration with the context menu and all files which were installed.

# How to build
* To work with the codebase, use `Visual Studio 2022` with a plugin [HeatWave for VS2022](https://marketplace.visualstudio.com/items?itemName=FireGiant.FireGiantHeatWaveDev17).
* To build a release, run `.github\workflows\build.ps1` (`git.exe` should be in your PATH)
