
# TaskbarX


![rsz_hero2](https://user-images.githubusercontent.com/50437199/90984468-6c5a1a00-e575-11ea-9af0-a874115e07e7.png)
  
TaskbarX gives you control over the position of your taskbar icons.
TaskbarX will give you an original Windows dock like feel. The icons will move to the center or user given position when an icon gets added or removed from the taskbar. You will be given the option to choose between a variety of different animations and change their speeds. The animations can be disabled if you don't like animations and want them to move in an instant. The center position can also be changed to bring your icons more to the left or right based on the center position. Currently all taskbar settings are supported including the vertical taskbar and unlimited taskbars.
TaskbarX has been in development since 6 may 2018. Also known as FalconX and Falcon10.
&nbsp;  
  
![aa4](https://user-images.githubusercontent.com/50437199/79305152-1c968280-7ef3-11ea-9eda-c97f61b758bd.png)
&nbsp;


## Features

- 42 different Animations including "none"
- Great performance (Very optimized and lightweight looping)
- Change animation speed
- Change custom offset position based on center
- Center between startbutton, search, taskview etc... and left tray icons, clock etc...
- All taskbar settings supported
- Vertical taskbar supported
- Unlimited monitors supported
- Change taskbar style to Transparent, Blur and Acrylic
- Change taskbar color and transparency
- Hide Start button and more...
- Taskbar segments
- Custom start button
  
  
&nbsp;



## How to use TaskbarX with Windows 11?
 
Do you want to install Windows 11 but you really like the taskbar of windows 10? Then you can actualy patch the Windows 11 Explorer to get back the old taskbar. Doing this will make TaskbarX functional again in Windows 11. https://github.com/valinet/ExplorerPatcher
  
  &nbsp;

  
## Start icon file

![start](https://user-images.githubusercontent.com/50437199/147479778-efed8dce-058e-408e-89f8-18b1bd4ecbf8.png) ![startdark](https://user-images.githubusercontent.com/50437199/147481351-0d90c00a-be6d-4ac7-bd76-d1557bd539a1.png)![startlight](https://user-images.githubusercontent.com/50437199/147481364-68a55a47-f527-4add-8ada-9f9a2a560b18.png)![startcolor](https://user-images.githubusercontent.com/50437199/147634252-ad73062f-6409-45d8-8a10-1362da4e226b.png)


Looking for a start icon file?  
**Start icon:** [start.ico](https://github.com/ChrisAnd1998/TaskbarX/releases/download/1.7.5.0/start.ico)  
**Start dark icon:** [startdark.ico](https://github.com/ChrisAnd1998/TaskbarX/releases/download/1.7.5.0/startdark.ico)  
**Start light icon:** [startlight.ico](https://github.com/ChrisAnd1998/TaskbarX/releases/download/1.7.5.0/startlight.ico)  
**Start color icon:** [startcolor.ico](https://github.com/ChrisAnd1998/TaskbarX/releases/download/1.7.5.0/startcolor.ico)  

  &nbsp;
  

## Commandline Arguments

_**-stop** will stop TaskbarX, puts all icons back to the left and resets the taskbar style.  
**-tbs=1** will make the taskbar transparent. 2 blur, 3 acrylic, 4 transparent (with gradient), 5 opaque and 0 is disabled.  
**-ptbo=0** will set the offset of the primary taskbar based on the center.  
**-stbo=0** will set the offset of the secondary taskbar based on the center.  
**-cpo=1** will only center the primary taskbar. 0 is disabled.  
**-cso=1** will only center the secondary taskbar(s). 0 is disabled.  
**-as=backeaseout** will set the animation style to BackEaseOut. "none" is instant.  
**-asp=500** will set the speed of the animation.  
**-lr=400** will set the refresh rate of the looper/taskbar checker.  
**-cib=1** will set the primary taskbar position in between start, search etc.. and the tray.  
**-ftotc=1** will update any toolbar when a tray icon gets added or removed.  
**-sr=1920** will put the icons to the left when screen width becomes 1920px.  
**-sr2=1920** will put the icons to the left when screen width becomes 1920px.  
**-sr3=1920** will put the icons to the left when screen width becomes 1920px.  
**-cfsa=1** will pause TaskbarX if a fullscreen app is running.  
**-obas=backeaseout** will set the animation style to BackEaseOut when on battery mode.  
**-oblr=400** will set the refresh rate of the looper/taskbar checker when on battery mode.  
**-dct=1** will stop TaskbarX from centering the taskbar icons.  
**-hps=1** will hide the start button on the primary monitor.  
**-hss=1** will hide the start button on the secondary monitor.  
**-hpt=1** will hide the tray area on the primary monitor.  
**-hst=1** will hide the tray area on the secondary monitor.  
**-sti=1** will show a tray icon to quickly restart and stop TaskbarX.  
**-dtbsowm=1** will revert to the default taskbar on maximized window.  
**-tpop=100** will set the total opacity of the primary taskbar.  
**-tsop=100** will set the total opacity of the secondary taskbar.  
**-rzbt=1** will revert the taskbar back to the left when the icons start overlapping with the Tray area.  
**-tbr=5** will set the rounding of the taskbar.  
**-tbsg=1** will enable taskbar segments. 0 is disabled and 1 is enabled.  
**-color=0;0;0;1** will set the color of the taskbar when using taskbar styling. RGBA._
  
> **Example:** _C:\Program Files (x86)\TaskbarX\TaskbarX.exe -tbs=1 -as=backeaseout_


  
&nbsp;

## References

- ModernWPF:  https://github.com/Kinnara/ModernWpf  
- TaskScheduler:  https://github.com/dahall/TaskScheduler  
- Emoji.Wpf:  https://github.com/samhocevar/emoji.wpf  

&nbsp;


## Downloads

- FREE | Portable .zip package : https://chrisandriessen.nl/taskbarx 
- FREE | Rainmeter Skin : https://chrisandriessen.nl/taskbarx 
- PAID | Windows Store : https://www.microsoft.com/store/productid/9PCMZ6BXK8GH
  
  
&nbsp;



## Frequently Asked Questions

**What versions of Windows does TaskbarX support?** TaskbarX only supports Windows 10. Version 1709 and lower will have issues with the "taskbar styling" option but, the centering will work fine. Windows 7 does not work and is not supported. It could be working on Windows 8 but, is not officially tested and is also not supported.  



**Whats the difference between the normal version and the store version?** The store version is exactly the same as the normal/free version. It's a donation sytem. The store version does give you the benefit of automatic updates and easy installing.  



**Does TaskbarX change my settings or registry?** No, TaskbarX is completely portable doesn't change your settings or your registry. Closing TaskbarX will stop the centering.  



**How to run TaskbarX on startup?** Once you click apply inside the Configurator a Taskschedule should be created with a delay of 3 seconds. If this doesn't work try increasing the delay to like 10 seconds or so inside the Configurator under the Taskschedule tab.  



**How do i uninstall TaskbarX?** Open the "TaskbarX Configurator" and hit the "Stop TaskbarX" button. This will stop the TaskbarX process and put your icons back to the left. Then go to the "Taskschedule" tab and hit the "Remove" button. After that you can simply remove the files where you installed TaskbarX. https://www.youtube.com/watch?v=Fj-gSF_nEyo  



**How to update TaskbarX?** Open the "TaskbarX Configurator" and hit the "Stop TaskbarX" button. This will stop the TaskbarX process and put your icons back to the left. Then overwrite the old TaskbarX files with the updated ones. https://www.youtube.com/watch?v=4dG8MlmeQsg  



**Error Windows cannot find 'shell:AppsFolder\Chris... after uninstalling Store version?** At the moment the Store has no option to remove a taskschedule. So it has to be done manually until Microsoft provides a solution. Here's how to fix it. In your startmenu search for "Task Scheduler". On the left menu click on "Task Scheduler Library". In the middle a list will appear. Right click on the "TaskbarX" or "FalconX" entry and click "Delete". https://www.youtube.com/watch?v=QtvxF1S7Dsw  


&nbsp;



## Errors

The Store version is not capable of uninstalling a taskschedule.   
If you get the error below then this explains how to fix it:

In your startmenu search for "**Task Scheduler**".  
On the left menu click on "**Task Scheduler Library**". In the middle a list will appear.  
Right click on the "**TaskbarX**" or "**FalconX**" entry and click "**Delete**".  

![Taskbar error](https://user-images.githubusercontent.com/50437199/80919928-e0757580-8d6c-11ea-9106-b0b1ff33f740.png)

_You can also use `SCHTASKS /DELETE /TN "TaskbarX PCNAMEUSERNAME"` to remove the scheduled task from the command line._

&nbsp;


## Tags
center taskbar, center taskbar icons, CenterTaskbar, center taskbar icons windows 10, center taskbar windows, windows center taskbar, windows center taskbar icons, center taskbar icons, windows 10 center taskbar icons, falcon10, falcon taskbar, taskbar, taskbar icons, taskbar buttons
