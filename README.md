# ShowWhatProcessLocksFile

A simple clone of [PowerToys File Locksmith](https://learn.microsoft.com/en-us/windows/powertoys/file-locksmith) utility to discover what processes lock a specific file or folder.

## Advantages over File Locksmith
* Much faster
* Lightweight
* Supports older versions of Windows
* Doesn't require admin rights to be installed

## Screenshots

### Context menu

<img src="doc/ContextMenu.png" width="70%" height="70%"/>

### Application

<img src="doc/Screenshot.png" width="70%" height="70%"/>

## System requirements

* Windows 7 x64 or higher (you might need to install `.Net Framework 4.7.2`)

## How to use

* Download `ShowWhatProcessLocksFile.msi.zip` from the latest [release](https://github.com/PolarGoose/ShowWhatProcessLocksFile/releases).
* Run the installer. The installer will install this program to the `%AppData%\ShowWhatProcessLocksFile` folder and add a `Show what locks this file` Windows File Explorer context menu element.
* Use `Show what locks this file` File Explorer's context menu to select a file or folder
* To terminate a process, select it and open a context menu by clicking the right mouse button
* If you want to uninstall the program, use `Control Panel\Programs\Programs and Features`. Uninstaller will remove an integration with the context menu and all installed files.

## How to build

* To work with the codebase, use `Visual Studio 2022` with a plugin [HeatWave for VS2022](https://marketplace.visualstudio.com/items?itemName=FireGiant.FireGiantHeatWaveDev17).
* To build a release, run `.\build.ps1` (`git.exe` should be in your PATH)
