# FalconX
FalconX is a tool written in VB.NET that centers your taskbar icons with animations.


FalconX uses the UIAutomation API  
(https://docs.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-overview)  
to get the width of your taskbar and uses SetWindowPos  
(https://www.pinvoke.net/default.aspx/user32.setwindowpos)  
to move the taskbar.  

It uses Dot Net Transitions (https://github.com/UweKeim/dot-net-transitions) for the animations.  

**ScreenShot**

![FalconX](https://chrisandriessen.nl/web/img/FX3.jpg)  

**Usage at IDLE**

GPU: GeForce RTX 2080 8GB  
CPU: Intel(R) Core(TM) i7-8700K CPU @ 4.80GHz  
Memory: 16.00 GB RAM DDR4  
Current resolution: 3x 2560 x 1440, 144Hz  

![FalconX](https://chrisandriessen.nl/web/img/FX3u2.png)  



**Video**

[![Alt text](https://img.youtube.com/vi/H07adcIXg7s/0.jpg)](https://www.youtube.com/watch?v=H07adcIXg7s)  


**Full ChangeLog**

```
FalconX Build 01.08.2019 1.1.5.0 August
* FalconX should work again with third-party startmenu's
* Made form sizable for the people who could'nt see the buttons at the bottom.

FalconX Build 14.07.2019 1.1.4.0 July
* Added option to center between the left Start, search etc... buttons and the right Tray menu
* Bug fixes and optimizations

FalconX Build 14.07.2019 1.1.3.0 July
* Bug fixes and optimizations

FalconX Build 13.07.2019 1.1.2.0 July
* Icons don't flicker anymore :)
* New easier to use Settings
* Bug fixes and optimizations

FalconX Build 11.07.2019 1.1.1.0 July
* Attempt to fix flickering icons bug

FalconX Build 24.06.2019 June
* Changed normal User32.dll Declarations to System.Runtime.InteropServices
* Made it a 32bit application
# May fix some startup issues

FalconX Build 23.06.2019 June
* Automatically fixes position on DPI change within < 10 seconds
* Automatically fixes position on icon size change within < 10 seconds
* Automatically fixes position on left offset change within < 10 seconds

FalconX Build 22.06.2019 June
* Improved CPU usage to 0%/0.1% at idle
* Updates actual Memory usage for task manager
* Automatically fixes position on resolution change within < 10 seconds
* Switched to (UI Automation COM API) instead of (UI Automation .NET)

FalconX Build 18.06.2019 June
- Removed taskbar style (acrylic)
- Removed taskbar style (transparant)
- Removed taskbar style (blur)
# TranslucentTB does it better. And caused issues in FalconX.

FalconX Build 17.06.2019 June
* Added taskbar style (acrylic)

FalconX Build 16.06.2019 June
* Tried to fix Error HRESULT E_FAIL has been returned from a call to a COM component
* Added taskbar style (transparant)
* Added taskbar style (blur)

FalconX Build 15.06.2019 June
* Added extra loop thread to calculate the left offset
* Added light and dark mode icons
* Added option to set custom offset
* Tried to fix the 0xffffff error on startup (Hope it's fixed now)

FalconX Build 13.06.2019 June
* Added refresh option

FalconX Build 10.06.2019 June
* Program rewritten
* Only supports main taskbar
* Choose between animations
* Choose animation speed
* Supports small taskbar icons
* Supports taskbar labels
* Compatible with TranslucentTB
* Compatible with third party start menu's
* No-GUI


_______________________________________________________________________________ 


Program rewritten...
(june 10th 2019)
|
Project renamed to FalconX
(december 11th 2018)
|
Started project Falcon10
(june 15th 2018)
|
Finding ways to center the taskbar
(may 6th 2018) 
```
