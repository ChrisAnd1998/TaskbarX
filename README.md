# FalconX

FalconX automate's your taskbar position to the center.

## Features

- 42 different Animations including "none"
- 0% CPU Usage at Idle (Very optimized and lightweight looping)
- Change animation speed
- Change custom offset position based on center
- Center between startbutton, search, taskview etc... and left tray icons, clock etc...
- All taskbar settings supported
- Vertical taskbar supported
- Multiple monitors supported max (3)
- Change taskbar style to Transparent, Blur and Acrylic

## Preview

[![Alt text](https://img.youtube.com/vi/diPj9AKCpb4/0.jpg)](https://www.youtube.com/watch?v=diPj9AKCpb4) 

## Downloads

- FREE | Simple .zip package : https://chrisandriessen.nl/web/FalconX.html  
- PAID | Windows Store : https://www.microsoft.com/store/productid/9PCMZ6BXK8GH

## References

- VisualEffects  | https://www.codeproject.com/Articles/827808/Control-Animation-in-Winforms  
- UIAComWrapperX | https://github.com/technoscavenger/UIAComWrapperX

## Screenshots

<p float="left">
  <img src="https://chrisandriessen.nl/web/img/p1.jpg" width="49%" />
  <img src="https://chrisandriessen.nl/web/img/p2.jpg" width="49%" /> 
</p>

<p float="left">
  <img src="https://chrisandriessen.nl/web/img/p3.jpg" width="32%" />
  <img src="https://chrisandriessen.nl/web/img/p4.jpg" width="32%" /> 
  <img src="https://chrisandriessen.nl/web/img/p5.jpg" width="32%" />
</p>



## Changelog

```
FalconX Build 10.11.2019 1.3.1.0 November
* FIXED | "Shadow buttons" - shows pointer cursor instead of normal (Issue opened by Martin5001)
* FIXED | Taskbar styling options not working until refresh (Issue opened by Martin5001)
* FIXED | Using tray when Settings opened freezes Settings window (Issue opened by Martin5001)
* FIXED | Update check error (Issue opened by Martin5001)
* ADDED | Update check improvements (Issue opened by Martin5001)

FalconX Build 31.10.2019 1.3.0.0 October
* Added right click tray menu.
* Added (check for updates).
* Little UI Change.
* Fixed animation triggering twice.

FalconX Build 27.10.2019 1.2.9.0 October
* Added option to set a custom position for secondary taskbars separately. 
* Added Taskbar Style (Blur).
* Added Taskbar Style (Acrylic) <-- not fully stable yet.
* Config now saves in Appdata instead of your Documents folder.

FalconX Build 20.10.2019 1.2.8.0 October
* FalconX rewritten to clean up and optimize all code.

FalconX Build 18.10.2019 1.2.7.0 October
* Better Taskbar Transparancy performance.

FalconX Build 16.10.2019 1.2.6.0 October
* Bug fixes and small performance improvements.
* Merged VisualEffects into FalconX

FalconX Build 13.10.2019 1.2.5.0 October
* Added Expirimental Taskbar Transparancy.
* Bug fixes for Multi Monitor support.

FalconX Build 13.10.2019 1.2.4.0 October
* FalconX now uses its own config file.
# To prevent user.config in appdata errors.

FalconX Build 13.10.2019 1.2.3.0 October
* Secondary Taskbar now checks its own left offset.
# So you can have a search box on your main taskbar and button on the secondary taskbar

FalconX Build 12.10.2019 1.2.2.0 October
* Multi Monitor Support. (up to 3 displays).
# Enable with "Center on Multiple Monitors" option.

FalconX Build 11.10.2019 1.2.1.0 October
* And another stability update.

FalconX Build 10.10.2019 1.2.0.0 October
* Slight change to the VisualEffects to make them even smoother.
* Bug fixes and optimizations

FalconX Build 09.10.2019 1.1.9.0 October
- Removed .Net Transitions.
* Added VisualEffects (www.easings.net)

FalconX Build 21.09.2019 1.1.8.0 August
* .Net Transitions fully integrated.
* Animation Preview

FalconX Build 20.09.2019 1.1.7.0 August
* Vertical taskbar support.
* Automatically restarts when Explorer restarts.
* Added option to change Reaction speed.
* Added simple reset settings button.

FalconX Build 25.08.2019 1.1.6.0 August
* Fixed start-up issues in UWP mode.

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



## Tags
center taskbar, center taskbar icons, center taskbar icons windows 10, center taskbar windows, windows center taskbar, windows center taskbar icons, center taskbar icons, windows 10 center taskbar icons, falcon10, falcon taskbar, taskbar, taskbar icons, taskbar buttons
