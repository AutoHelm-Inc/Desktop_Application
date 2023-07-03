# Desktop_Application


## Insalling Swift on Windows
### Install Windows Package Manager
[Installer](https://docs.microsoft.com/windows/package-manager/) or [Microsoft Store](https://www.microsoft.com/en-us/p/app-installer/9nblggh4nns1)

### Install Visual Studio
Run these commands
```
winget install Git.Git
winget install Python.Python.3.10

curl -sOL https://aka.ms/vs/16/release/vs_community.exe
start /w vs_community.exe --passive --wait --norestart --nocache ^
  --installPath "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community" ^
  --add Microsoft.VisualStudio.Component.Windows10SDK.19041 ^
  --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64
del /q vs_community.exe
```

Open up `Visual Studio Installer` to modify the file and install
- MSVC v142 - VS 2019 C++ x64/x86 build tools (Latest)
- Windows 10 SDK (10.0.17763.0)
- C++ CMake tools for Windows

You Should be Running the commands from the `X64 Native Tools Command Prompt for VS2019` as Administrator.

With that being said, you can install the official Swift VSCode extension [here](https://marketplace.visualstudio.com/items?itemName=sswg.swift-lang).
## Install CMAKE
Download CMake from [here](https://cmake.org/download/) and **CHECK ADD TO PATH** in the installer.
## Install Swift
Download the Windows version [here](https://www.swift.org/download/)
### How to Run Swift

#### Hardcore Mode (recommended)

Run the run script, by typing `run.bat` 

**or**

We want to follow these commands:
1. `swift build`
2. `mt -nologo -manifest Sources\Application\Application.exe.manifest -outputresource:.build\x86_64-unknown-windows-msvc\debug\Application.exe
`
1. `copy Sources\Application\Info.plist .build\x86_64-unknown-windows-msvc\debug\`
2. `swift run` or any other debugging command


**NOTE:** if console stays hung, hit `enter`

#### Simple Way
In the project directory run, `swiftc [file_name]`
The key word `swiftc` is the compiler function, that will compile and create a `.exe` to run

#### Useless Way
Swift has it's own interpreter that is similar to python, however, its buggy on Windows. We should never need to use this. `swift`. There is also `swift repl` which is like coding on terminal in Python.