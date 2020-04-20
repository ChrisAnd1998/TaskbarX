Imports System.Runtime.InteropServices
Imports TaskbarX.VisualEffects
Imports TaskbarX.VisualEffects.Animations.Effects
Imports TaskbarX.VisualEffects.Easing
Imports System.Threading
Imports System.Windows.Automation
Imports Microsoft.Win32
Imports UIA

Public Class TaskbarCenter

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", ExactSpelling:=True, CharSet:=CharSet.Auto)>
    Public Shared Function GetParent(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function SetParent(ByVal hWndChild As IntPtr, ByVal hWndNewParent As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
    End Function

    <DllImport("user32.dll")>
    Shared Function AnimateWindow(ByVal hwnd As IntPtr, ByVal time As Integer, ByVal flags As AnimateWindowFlags) As Boolean
    End Function

    <Flags()>
    Public Enum AnimateWindowFlags
        AW_HOR_POSITIVE = &H1
        AW_HOR_NEGATIVE = &H2
        AW_VER_POSITIVE = &H4
        AW_VER_NEGATIVE = &H8
        AW_CENTER = &H10
        AW_HIDE = &H10000
        AW_ACTIVATE = &H20000
        AW_SLIDE = &H40000
        AW_BLEND = &H80000
    End Enum

    Public Shared SWP_NOSIZE As UInt32 = 1
    Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
    Public Shared SWP_NOACTIVATE As UInt32 = 16
    Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
    Public Shared SWP_NOZORDER As UInt32 = 4

    Public Shared ScreensChanged As Boolean

    Public Shared TaskbarCount As Integer

    Public Shared Sub TaskbarCenterer()
        RevertToZero()

        AddHandler SystemEvents.DisplaySettingsChanged, AddressOf DPChange
        AddHandler SystemEvents.UserPreferenceChanged, AddressOf DPChange

        'Start the Looper
        Dim t1 As Thread = New Thread(AddressOf Looper)
        t1.Start()

        'Start the TrayLoopFix
        If Settings.FixToolbarsOnTrayChange = 1 Then
            Dim t2 As Thread = New Thread(AddressOf TrayLoopFix)
            t2.Start()
        End If

    End Sub

    Public Shared Sub DPChange(ByVal sender As Object, ByVal e As EventArgs)
        Console.WriteLine()
        Thread.Sleep(1000)
        'Wait for Shell_TrayWnd
        Dim Handle As IntPtr
        Do
            Console.WriteLine("Waiting for Shell_TrayWnd")
            Handle = Nothing
            Thread.Sleep(250)
            Handle = FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
        Loop Until Not Handle = Nothing

        Application.Restart()
    End Sub

    Public Shared Sub Looper()
        Try
            'This loop will check if the taskbar changes and requires a move
            Dim Root As AutomationElement = AutomationElement.RootElement
            Dim Condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
            Dim AllTrayWnds As AutomationElementCollection = Root.FindAll(TreeScope.Children, Condition)
            Dim TaskbarTreeWalker As TreeWalker = TreeWalker.ControlViewWalker

            Dim TrayWnds As New ArrayList
            Dim Taskbars As New ArrayList

            'Put all TrayWnds into an ArrayList
            For Each TrayWnd As AutomationElement In AllTrayWnds
                TrayWnds.Add(TrayWnd)
            Next

            'Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
            For Each Taskbar As AutomationElement In TrayWnds
                Dim mstasklist As AutomationElement = Taskbar.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
                Taskbars.Add(mstasklist)
            Next

            TaskbarCount = Taskbars.Count

            'Start the endless loop
            Do
                Try

                    Dim results As String = Nothing
                    Dim oldresults As String

                    'Go through each taskbar and result in a unique string containing the current state
                    For Each TaskList As AutomationElement In Taskbars

                        Dim child As AutomationElement = TaskbarTreeWalker.GetLastChild(TaskList)

                        Try
                            Dim testiferror = child.Current.BoundingRectangle.Left
                        Catch ex As Exception
                            'Current taskbar is empty go to next taskbar.
                            Continue For
                        End Try

                        Dim Orientation As String
                        Dim TaskbarCount As Integer
                        Dim TrayWndSize As Integer

                        'Get current taskbar orientation (H = Horizontal | V = Vertical)
                        If TaskList.Current.BoundingRectangle.Height >= 200 Then
                            Orientation = "V"
                        Else
                            Orientation = "H"
                        End If

                        'Get the end position of the last icon in the taskbar
                        If Orientation = "H" Then
                            TaskbarCount = CInt(child.Current.BoundingRectangle.Left + child.Current.BoundingRectangle.Width)
                        Else
                            TaskbarCount = CInt(child.Current.BoundingRectangle.Top + child.Current.BoundingRectangle.Height)
                        End If

                        'Gets the width of the whole taskbars placeholder
                        If Orientation = "H" Then
                            TrayWndSize = CInt(TaskList.Current.BoundingRectangle.Width)
                        Else
                            TrayWndSize = CInt(TaskList.Current.BoundingRectangle.Height)
                        End If

                        'Put the results into a string ready to be matched for differences with last loop
                        results = results & Orientation & TaskbarCount & TrayWndSize

                    Next

                    If Not results = oldresults Then
                        'Something has changed we can now calculate the new position for each taskbar

                        'Wait for the Animator to finish
                        Do
                            Thread.Sleep(30)
                        Loop Until TaskbarAnimate.IsAnimated = True

                        'Block multiple triggers at once (preventing the taskbar from moving halfway and then to final position)
                        Dim trigger As Integer
                        If trigger = 1 Then
                            GoTo triggerskip
                        End If
                        trigger = 1
                        Thread.Sleep(50)
                        trigger = 0

                        'Start the PositionCalculator
                        Dim t1 As Thread = New Thread(AddressOf PositionCalculator)
                        t1.Start()

                    End If

triggerskip:

                    'Save current results for next loop
                    oldresults = results

                    If SystemInformation.PowerStatus.PowerLineStatus = PowerLineStatus.Offline Then
                        Thread.Sleep(Settings.OnBatteryLoopRefreshRate)
                    Else
                        Thread.Sleep(Settings.LoopRefreshRate)
                    End If
                Catch ex As Exception
                    Console.WriteLine("@Looper1 | " & ex.Message)

                    'Lost taskbar handles restart application
                    If ex.ToString.Contains("E_ACCESSDENIED") Then
                        Dim Handle As IntPtr
                        Do
                            Handle = Nothing
                            System.Threading.Thread.Sleep(250)
                            Handle = FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
                        Loop Until Not Handle = Nothing
                        Application.Restart()
                    End If

                End Try
            Loop
        Catch ex As Exception
            Console.WriteLine("@Looper2 | " & ex.Message)
        End Try
    End Sub

    Public Shared Sub RevertToZero()
        'Put all taskbars back to default position
        Dim desktops As AutomationElement = AutomationElement.RootElement
        Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
        Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, condition)
        Dim TrayWnds As New ArrayList
        Dim Taskbars As New ArrayList

        'Put all TrayWnds into an ArrayList
        For Each TrayWnd As AutomationElement In AllTrayWnds
            TrayWnds.Add(TrayWnd)
        Next

        'Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
        For Each Taskbar As AutomationElement In TrayWnds
            Dim mstasklist As AutomationElement = Taskbar.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
            Taskbars.Add(mstasklist)
        Next

        For Each TaskList As AutomationElement In Taskbars
            SendMessage(GetParent(GetParent(TaskList.Current.NativeWindowHandle)), 11, True, 0)
            SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
        Next
    End Sub

    Public Shared Sub TrayLoopFix()

        Try
            Dim Shell_TrayWnd As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr)))
            Dim TrayNotifyWnd As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "TrayNotifyWnd"))
            Dim MSTaskListWClass As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
            Dim MSTaskSwWClass As IntPtr = GetParent(CType(MSTaskListWClass.Current.NativeWindowHandle, IntPtr))
            Dim ReBarWindow32 As AutomationElement = AutomationElement.FromHandle(GetParent(MSTaskSwWClass))

            Dim SWP_NOSIZE As UInt32 = 1
            Dim SWP_ASYNCWINDOWPOS As UInt32 = 16384
            Dim SWP_NOACTIVATE As UInt32 = 16
            Dim SWP_NOSENDCHANGING As UInt32 = 1024
            Dim SWP_NOZORDER As UInt32 = 4

            Do

                SendMessage(ReBarWindow32.Current.NativeWindowHandle, 11, False, 0)

                Dim TrayNotifyWidth As Integer = 0
                Dim OldTrayNotifyWidth As Integer
                Dim TrayOrientation As String

                'If the TrayNotifyWnd updates then refresh the taskbar
                If MSTaskListWClass.Current.BoundingRectangle.Height >= 200 Then
                    TrayOrientation = "V"
                Else
                    TrayOrientation = "H"
                End If

                TrayNotifyWidth = TrayNotifyWnd.Current.BoundingRectangle.Width

                If Not TrayNotifyWidth = OldTrayNotifyWidth Then
                    If Not OldTrayNotifyWidth = 0 Then
                        If Not MSTaskListWClass.Current.BoundingRectangle.X = 0 Then
                            If TrayNotifyWnd.Current.BoundingRectangle.Left = 3 Then
                                '
                                Exit Sub
                            End If
                            '  Dim offset = CInt(TaskList.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                            Dim pos = (MSTaskListWClass.Current.BoundingRectangle.Left - ReBarWindow32.Current.BoundingRectangle.Left - 2).ToString.Replace("-", "")
                            SendMessage(ReBarWindow32.Current.NativeWindowHandle, 11, True, 0)
                            SendMessage(ReBarWindow32.Current.NativeWindowHandle, 11, False, 0)
                            If TrayOrientation = "H" Then
                                SetWindowPos(MSTaskListWClass.Current.NativeWindowHandle, IntPtr.Zero, pos, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                            Else
                                SetWindowPos(MSTaskListWClass.Current.NativeWindowHandle, IntPtr.Zero, 0, pos, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                            End If
                        End If
                    End If
                End If

                OldTrayNotifyWidth = TrayNotifyWidth

                Thread.Sleep(Settings.LoopRefreshRate)

            Loop
        Catch ex As Exception
            ' Console.WriteLine(ex.Message)
            Console.WriteLine("@TrayLoopFix | " & ex.Message)
        End Try
    End Sub

    Public Shared Root As AutomationElement = AutomationElement.RootElement
    Public Shared Condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
    Public Shared AllTrayWnds As AutomationElementCollection = Root.FindAll(TreeScope.Children, Condition)

    Public Shared Sub PositionCalculator()
        Try
            'Calculate the new positions and pass them through to the animator

            Dim TrayWnds As New ArrayList
            Dim Taskbars As New ArrayList

            'Put all TrayWnds into an ArrayList
            For Each TrayWnd As AutomationElement In AllTrayWnds
                TrayWnds.Add(TrayWnd)
            Next

            'Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
            For Each Taskbar As AutomationElement In TrayWnds
                Dim mstasklist As AutomationElement = Taskbar.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
                Taskbars.Add(mstasklist)
            Next

            'Calculate Position for every taskbar and trigger the animator
            For Each TaskList As AutomationElement In Taskbars
                Dim TreeWalker As TreeWalker = TreeWalker.ControlViewWalker
                Dim ChildLast As AutomationElement = TreeWalker.GetLastChild(TaskList)
                Dim ChildFirst As AutomationElement = TreeWalker.GetFirstChild(TaskList)
                Dim TrayWndHandle = GetParent(GetParent(TaskList.Current.NativeWindowHandle))
                Dim RebarHandle = GetParent(TaskList.Current.NativeWindowHandle)
                Dim TrayWnd As AutomationElement = AutomationElement.FromHandle(TrayWndHandle)
                Dim RebarWnd As AutomationElement = AutomationElement.FromHandle(RebarHandle)
                Dim TrayNotify As AutomationElement = AutomationElement.FromHandle(RebarHandle)
                Dim Orientation As String
                Dim TaskbarWidth As Integer
                Dim TrayWndLeft As Integer
                Dim TrayWndWidth As Integer
                Dim RebarWndLeft As Integer
                Dim TaskbarLeft As Integer
                Dim Position As Integer
                Dim curleft As Integer
                Dim curleft2 As Integer
                Dim curwidth As Integer
                Dim curwidth2 As Integer

                'Check if TrayWnd = wrong. if it is, correct it (This will be the primary taskbar which should be Shell_TrayWnd)
                If TrayWnd.Current.ClassName = "ReBarWindow32" Then
                    SendMessage(TrayWnd.Current.NativeWindowHandle, 11, False, 0)
                    Dim hwnd = GetParent(GetParent(GetParent(TaskList.Current.NativeWindowHandle)))
                    TrayWnd = AutomationElement.FromHandle(hwnd)
                    TrayNotify = TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "TrayNotifyWnd"))
                    SendMessage(GetParent(TrayWnd.Current.NativeWindowHandle), 11, False, 0)
                End If

                'If the taskbar is still moving then wait until it's not (This will prevent unneeded calculations that trigger the animator)
                Do
                    curleft = TaskList.Current.BoundingRectangle.Left
                    Threading.Thread.Sleep(30)
                    curleft2 = TaskList.Current.BoundingRectangle.Left
                Loop Until curleft = curleft2

                'Get current taskbar orientation (H = Horizontal | V = Vertical)
                If TaskList.Current.BoundingRectangle.Height >= 200 Then
                    Orientation = "V"
                Else
                    Orientation = "H"
                End If

                'If the taskbar is still adding an icon then wait until it's not (This will prevent unneeded calculations that trigger the animator)
                Try
                    If Orientation = "H" Then
                        Do
                            curwidth = CInt((ChildFirst.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + (ChildLast.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + ChildLast.Current.BoundingRectangle.Width)
                            Threading.Thread.Sleep(30)
                            curwidth2 = CInt((ChildFirst.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + (ChildLast.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + ChildLast.Current.BoundingRectangle.Width)
                        Loop Until curleft = curleft2
                    Else
                        Do
                            curwidth = CInt((ChildFirst.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + (ChildLast.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + ChildLast.Current.BoundingRectangle.Height)
                            Threading.Thread.Sleep(30)
                            curwidth2 = CInt((ChildFirst.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + (ChildLast.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + ChildLast.Current.BoundingRectangle.Height)
                        Loop Until curleft = curleft2
                    End If
                Catch
                    'Taskbar is empty just skip
                End Try

                'Calculate the exact width of the total icons
                Try
                    If Orientation = "H" Then
                        TaskbarWidth = CInt((ChildFirst.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + (ChildLast.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + ChildLast.Current.BoundingRectangle.Width)
                    Else
                        TaskbarWidth = CInt((ChildFirst.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + (ChildLast.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + ChildLast.Current.BoundingRectangle.Height)
                    End If
                Catch
                    TaskbarWidth = 0
                    'Taskbar is empty just skip
                End Try

                'Get info needed to calculate the position
                If Orientation = "H" Then
                    TrayWndLeft = CInt(TrayWnd.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                    TrayWndWidth = CInt(TrayWnd.Current.BoundingRectangle.Width.ToString.Replace("-", ""))
                    RebarWndLeft = CInt(RebarWnd.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                    TaskbarLeft = CInt((RebarWndLeft - TrayWndLeft).ToString.Replace("-", ""))
                Else
                    TrayWndLeft = CInt(TrayWnd.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                    TrayWndWidth = CInt(TrayWnd.Current.BoundingRectangle.Height.ToString.Replace("-", ""))
                    RebarWndLeft = CInt(RebarWnd.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                    TaskbarLeft = CInt((RebarWndLeft - TrayWndLeft).ToString.Replace("-", ""))
                End If

                'Calculate new position
                If TrayWnd.Current.ClassName = "Shell_TrayWnd" Then
                    If Settings.CenterInBetween = 1 Then
                        If Orientation = "H" Then
                            Dim offset = (TrayNotify.Current.BoundingRectangle.Width / 2 - (TaskbarLeft \ 2))
                            Position = CInt((TrayWndWidth / 2 - (TaskbarWidth / 2) - TaskbarLeft - offset).ToString.Replace("-", "")) + Settings.PrimaryTaskbarOffset
                        Else
                            Dim offset = (TrayNotify.Current.BoundingRectangle.Height / 2 - (TaskbarLeft \ 2))
                            Position = CInt((TrayWndWidth / 2 - (TaskbarWidth / 2) - TaskbarLeft - offset).ToString.Replace("-", "")) + Settings.PrimaryTaskbarOffset
                        End If
                    Else
                        Position = CInt((TrayWndWidth / 2) - (TaskbarWidth / 2) - TaskbarLeft).ToString.Replace("-", "") + Settings.PrimaryTaskbarOffset
                    End If
                Else
                    Position = CInt((TrayWndWidth / 2) - (TaskbarWidth / 2) - TaskbarLeft).ToString.Replace("-", "") + Settings.SecondaryTaskbarOffset
                End If

                'Trigger the animator
                If SystemInformation.PowerStatus.PowerLineStatus = PowerLineStatus.Offline Then

#Region "Animation Trigger On Battery"

                    If Settings.CenterPrimaryOnly = 1 Then
                        If TrayWnd.Current.ClassName = "Shell_TrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, Position, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, Position, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWnd.Current.ClassName = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, Position, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, Position, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    Else
                        If Orientation = "H" Then

                            If Settings.OnBatteryAnimationStyle = "none" Then
                                SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, Position, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                            End If

                            If Settings.OnBatteryAnimationStyle = "linear" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                            End If
                        Else

                            If Settings.OnBatteryAnimationStyle = "none" Then
                                SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, Position, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                            End If

                            If Settings.OnBatteryAnimationStyle = "linear" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                        End If
                    End If

#End Region

                Else

#Region "Animation Trigger"

                    If Settings.CenterPrimaryOnly = 1 Then
                        If TrayWnd.Current.ClassName = "Shell_TrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.AnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, Position, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.AnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, Position, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWnd.Current.ClassName = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.AnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, Position, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.AnimationStyle = "none" Then
                                    SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, Position, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    Else
                        If Orientation = "H" Then

                            If Settings.AnimationStyle = "none" Then
                                SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, Position, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                            End If

                            If Settings.AnimationStyle = "linear" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Left - RebarWnd.Current.BoundingRectangle.Left), "H", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                            End If
                        Else

                            If Settings.AnimationStyle = "none" Then
                                SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, Position, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                            End If

                            If Settings.AnimationStyle = "linear" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeasein" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseinout" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseoutin" Then
                                Animate(TaskList.Current.NativeWindowHandle, (TaskList.Current.BoundingRectangle.Top - RebarWnd.Current.BoundingRectangle.Top), "V", New TaskbarMove(), AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                        End If
                    End If

#End Region

                End If

            Next
        Catch ex As Exception

            Console.WriteLine("@Calculator | " & ex.Message)

        End Try
    End Sub

    Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal iAnimation As Effect, ByVal easing As EasingDelegate, ByVal valueToReach As Double, ByVal duration As Double)
        Try
            Dim t1 As Thread = New Thread(Sub() TaskbarAnimate.Animate(hwnd, oldpos, orient, iAnimation, easing, valueToReach, duration))
            t1.Start()
        Catch ex As Exception

            Console.WriteLine("@Animation Call | " & ex.Message)
        End Try
    End Sub

End Class