@ECHO OFF
swift build
mt -nologo -manifest Sources\Application\Application.exe.manifest -outputresource:.build\x86_64-unknown-windows-msvc\debug\Application.exe
copy Sources\Application\Info.plist .build\x86_64-unknown-windows-msvc\debug\
swift run