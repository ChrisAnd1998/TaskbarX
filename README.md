# FalconX
FalconX is a tool written in VB.NET that centers your taskbar icons with animations.

FalconX uses the UIAutomation API (https://docs.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-overview)
to get the width of your taskbar and uses SetWindowPos(https://www.pinvoke.net/default.aspx/user32.setwindowpos) to move the taskbar.

It uses Dot Net Transitions (https://github.com/UweKeim/dot-net-transitions) for the animations.




**Examples**

If you want to get the width's and hwnd from 3 taskbars or more you can try this.

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

SetWindowPos

```vb.net
Private Const SWP_NOSIZE As Integer = &H1
    Private Const SWP_NOZORDER As Integer = &H4
    Private Const SWP_SHOWWINDOW As Integer = &H40
    Private Const SWP_ASYNCWINDOWPOS As Integer = &H4000
    Private Const SWP_NOSENDCHANGING As Integer = &H400
    Private Const SWP_NOACTIVATE As Integer = &H10


    Private Declare Auto Function SetWindowPos Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As Integer) As Boolean
    
    Sub MoveTaskbar()
    SetWindowPos(tasklistPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOZORDER Or SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE)
    End Sub
```

**Video**

[![Alt text](https://img.youtube.com/vi/H07adcIXg7s/0.jpg)](https://www.youtube.com/watch?v=H07adcIXg7s)
