# A minimalistic example of a SimConnect on a .NET core web site

To create your own SimProject make sure of:

1. Install the FS2020 SDK (we assume you have it installed in "C:\MSFS SDK")
2. Your project must be a 64 bit assembly
3. You must have a post build event which copies the DLL and config:
```
xcopy "C:\MSFS SDK\SimConnect SDK\lib\SimConnect.dll" "$(TargetDir)" /y
xcopy "C:\MSFS SDK\Samples\SimvarWatcher\SimConnect.cfg" "$(TargetDir)" /y
```

A couple of tricks:
* An hidden windows is created by the MessagePumpWindow to get the messages
* A "standard" WIN32 message loop is handled by MessagePumpWindow and called by the main function after starting the .net core web application in a separated thread
Without those two points, Simconnect will not work.