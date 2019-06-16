# FalconX
FalconX is a tool written in VB.NET that centers your taskbar icons with animations.

FalconX uses the UIAutomation API (https://docs.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-overview)
to get the width of your taskbar and uses SetWindowPos(https://www.pinvoke.net/default.aspx/user32.setwindowpos) to move the taskbar.

It uses Dot Net Transitions (https://github.com/UweKeim/dot-net-transitions) for the animations.


**Examples**

If you want to move 3 taskbars or more you can try this.

```vb.net
  Dim desktop As AutomationElement = AutomationElement.RootElement
        Dim tasklisty As AutomationElement = Nothing
        Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New   PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
        Dim lists As AutomationElementCollection = desktop.FindAll(TreeScope.Children, condition)
        Dim NumberItem = 0
        For Each trayWnd As AutomationElement In lists
            NumberItem = NumberItem + 1
            Dim children As New Dictionary(Of AutomationElement, AutomationElement)()
            Dim icons As New List(Of AutomationElement)()
            icons.Clear()
            children.Clear()
            Dim tasklist As AutomationElement = trayWnd.FindFirst(TreeScope.Descendants, New      PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
            tasklisty = tasklist
            icons.Add(trayWnd)
            children.Add(trayWnd, tasklist)
            Dim tasklists As AutomationElement = children(trayWnd)

            If NumberItem = 1 Then
                tasklistPtr = tasklists.Current.NativeWindowHandle
                tasklistWidth = trayWnd.Current.BoundingRectangle.Width
                tasklistHeight = trayWnd.Current.BoundingRectangle.Height
            End If


            If NumberItem = 2 Then
                tasklistPtr2 = tasklists.Current.NativeWindowHandle
                tasklistWidth2 = trayWnd.Current.BoundingRectangle.Width
                tasklistHeight2 = trayWnd.Current.BoundingRectangle.Height
                tasklistLeft2 = tasklists.Current.BoundingRectangle.Left
            End If


            If NumberItem = 3 Then
                tasklistPtr3 = tasklists.Current.NativeWindowHandle
                tasklistWidth3 = trayWnd.Current.BoundingRectangle.Width
                tasklistHeight3 = trayWnd.Current.BoundingRectangle.Height
                tasklistLeft3 = tasklists.Current.BoundingRectangle.Left
            End If
        Next
```
