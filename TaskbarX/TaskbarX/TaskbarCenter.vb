Imports TaskbarX.VisualEffects
Imports TaskbarX.VisualEffects.Easing
Imports System.Threading
Imports Microsoft.Win32
Imports System.Text

Public Class TaskbarCenter

#Region "Values"

    Public Shared ScreensChanged As Boolean

    Public Shared TaskbarCount As Integer

    Public Shared windowHandles As ArrayList = New ArrayList()

    Public Shared childLeft As Integer
    Public Shared childTop As Integer
    Public Shared childWidth As Integer
    Public Shared childHeight As Integer

    Public Shared childLeft2 As Integer
    Public Shared childTop2 As Integer
    Public Shared childWidth2 As Integer
    Public Shared childHeight2 As Integer

    Public Shared childLeft3 As Integer
    Public Shared childTop3 As Integer
    Public Shared childWidth3 As Integer
    Public Shared childHeight3 As Integer

    Public Shared trayfixed As Boolean
    Public Shared setposhwnd As IntPtr
    Public Shared setpospos As Integer
    Public Shared setposori As String

    Public Shared UserPref As New Microsoft.Win32.UserPreferenceChangedEventHandler(AddressOf HandlePrefChange)

#End Region

    Public Shared Sub TaskbarCenterer()
        RevertToZero()

        AddHandler SystemEvents.DisplaySettingsChanged, AddressOf DPChange
        AddHandler SystemEvents.UserPreferenceChanged, UserPref

        'Start the Looper
        Dim t1 As Thread = New Thread(AddressOf Looper)
        t1.Start()

        'Start the TrayLoopFix
        If Settings.FixToolbarsOnTrayChange = 1 Then
            Dim t2 As Thread = New Thread(AddressOf TrayLoopFix)
            t2.Start()
        End If

    End Sub

#Region "Commands"

    Public Shared Function GetActiveWindows() As ObjectModel.Collection(Of IntPtr)
        windowHandles.Clear()
        Win32.EnumWindows(AddressOf Enumerator, 0)
        Return Win32.ActiveWindows
    End Function

    Public Shared Function Enumerator(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Dim sClassName As New StringBuilder("", 256)
        Call Win32.GetClassName(hwnd, sClassName, 256)
        If sClassName.ToString = "Shell_TrayWnd" Or sClassName.ToString = "Shell_SecondaryTrayWnd" Then
            windowHandles.Add(hwnd)
        End If
        Return True
    End Function

    Public Shared Function GetLocation(ByVal acc As Accessibility.IAccessible, ByVal idChild As Integer) As Integer
        acc.accLocation(childLeft, childTop, childWidth, childHeight, idChild)
        Return Nothing
    End Function

    Public Shared Function GetLocation2(ByVal acc As Accessibility.IAccessible, ByVal idChild As Integer) As Integer
        acc.accLocation(childLeft2, childTop2, childWidth2, childHeight2, idChild)
        Return Nothing
    End Function

    Public Shared Function GetLocation3(ByVal acc As Accessibility.IAccessible, ByVal idChild As Integer) As Integer
        acc.accLocation(childLeft3, childTop3, childWidth3, childHeight3, idChild)
        Return Nothing
    End Function

    Public Shared Sub setpos()
        If setposori = "H" Then
            Do
                Win32.SetWindowPos(setposhwnd, IntPtr.Zero, setpospos, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
            Loop Until trayfixed = True
        Else
            Do
                Win32.SetWindowPos(setposhwnd, IntPtr.Zero, 0, setpospos, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
            Loop Until trayfixed = True
        End If
    End Sub

    Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal easing As EasingDelegate, ByVal valueToReach As Double, ByVal duration As Double)
        Try
            Dim t1 As Thread = New Thread(Sub() TaskbarAnimate.Animate(hwnd, oldpos, orient, easing, valueToReach, duration))
            t1.Start()
        Catch ex As Exception

            Console.WriteLine("@Animation Call | " & ex.Message)
        End Try
    End Sub

    Public Shared Sub RevertToZero()
        'Put all taskbars back to default position
        GetActiveWindows()

        For Each prog As Process In Process.GetProcesses
            If prog.ProcessName = "AcrylicPanel" Then
                prog.Kill()
            End If
        Next

        Dim Taskbars As New ArrayList

        For Each Taskbar In windowHandles
            Dim sClassName As New StringBuilder("", 256)
            Call Win32.GetClassName(Taskbar, sClassName, 256)

            Dim MSTaskListWClass As IntPtr

            If sClassName.ToString = "Shell_TrayWnd" Then
                Dim ReBarWindow32 = Win32.FindWindowEx(Taskbar, 0, "ReBarWindow32", Nothing)
                Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, 0, "MSTaskSwWClass", Nothing)
                MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, 0, "MSTaskListWClass", Nothing)
            End If

            If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                Dim WorkerW = Win32.FindWindowEx(Taskbar, 0, "WorkerW", Nothing)
                MSTaskListWClass = Win32.FindWindowEx(WorkerW, 0, "MSTaskListWClass", Nothing)
            End If

            ' Console.WriteLine(MSTaskListWClass)

            Taskbars.Add(MSTaskListWClass)
        Next

        For Each TaskList In Taskbars
            Win32.SendMessage(Win32.GetParent(Win32.GetParent(TaskList)), 11, True, 0)
            Win32.SetWindowPos(TaskList, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
        Next
    End Sub

#End Region

#Region "Events"

    Public Shared Sub HandlePrefChange(ByVal sender As Object, ByVal e As Microsoft.Win32.UserPreferenceChangedEventArgs)
        Console.WriteLine(e.Category)
        If e.Category = Microsoft.Win32.UserPreferenceCategory.General Then

            Console.WriteLine()
            Thread.Sleep(1000)
            'Wait for Shell_TrayWnd
            Dim Handle As IntPtr
            Do
                Console.WriteLine("Waiting for Shell_TrayWnd")
                Handle = Nothing
                Thread.Sleep(250)
                Handle = Win32.FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
            Loop Until Not Handle = Nothing

            Application.Restart()

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
            Handle = Win32.FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
        Loop Until Not Handle = Nothing

        Application.Restart()
    End Sub

#End Region

#Region "Looper"

    Public Shared Sub Looper()
        Try
            'This loop will check if the taskbar changes and requires a move
            GetActiveWindows()

            Dim Taskbars As New ArrayList

            'Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
            For Each Taskbar In windowHandles
                Dim sClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(Taskbar, sClassName, 256)

                Dim MSTaskListWClass As IntPtr

                If sClassName.ToString = "Shell_TrayWnd" Then
                    Dim ReBarWindow32 = Win32.FindWindowEx(Taskbar, 0, "ReBarWindow32", Nothing)
                    Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, 0, "MSTaskSwWClass", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, 0, "MSTaskListWClass", Nothing)
                End If

                If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                    Dim WorkerW = Win32.FindWindowEx(Taskbar, 0, "WorkerW", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(WorkerW, 0, "MSTaskListWClass", Nothing)
                End If

                If MSTaskListWClass = Nothing Then
                    MessageBox.Show("TaskbarX: Could not find the handle of the taskbar. Your current OS may not be supported.")
                    End
                End If

                Taskbars.Add(MSTaskListWClass)
            Next

            Dim TaskObject = New List(Of Accessibility.IAccessible)()
            For Each TaskList In Taskbars
                Dim accessiblex As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TaskList)
                TaskObject.Add(accessiblex)
            Next

            'Start the endless loop
            Do
                Try

                    Dim results As String = Nothing
                    Dim oldresults As String

                    If Not Settings.SkipResolution = 0 Then
                        If Screen.PrimaryScreen.Bounds.Width = Settings.SkipResolution Then
                            RevertToZero()
                            Exit Do
                        End If
                    End If

                    If Settings.CheckFullscreenApp = 1 Then
                        Dim activewindow = Win32.GetForegroundWindow()
                        Dim curmonx As Screen = Screen.FromHandle(activewindow)
                        Dim activewindowsize As New Win32.RECT
                        Win32.GetWindowRect(activewindow, activewindowsize)

                        If activewindowsize.Top = curmonx.Bounds.Top And activewindowsize.Bottom = curmonx.Bounds.Bottom And activewindowsize.Left = curmonx.Bounds.Left And activewindowsize.Right = curmonx.Bounds.Right Then
                            Console.WriteLine("Fullscreen App detected " & activewindowsize.Bottom & "," & activewindowsize.Top & "," & activewindowsize.Left & "," & activewindowsize.Right)
                            Settings.Pause = True
                            Do
                                System.Threading.Thread.Sleep(500)
                                activewindow = Win32.GetForegroundWindow()
                                Win32.GetWindowRect(activewindow, activewindowsize)
                                System.Threading.Thread.Sleep(500)

                            Loop While activewindowsize.Top = curmonx.Bounds.Top And activewindowsize.Bottom = curmonx.Bounds.Bottom And activewindowsize.Left = curmonx.Bounds.Left And activewindowsize.Right = curmonx.Bounds.Right
                            Console.WriteLine("Fullscreen App deactivated")
                            Settings.Pause = False
                        End If
                    End If

                    'Go through each taskbar and result in a unique string containing the current state
                    For Each TaskList In TaskObject

                        Dim children() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(TaskList)

                        GetLocation(TaskList, 0)

                        Dim tH = childHeight
                        Dim tW = childWidth

                        For Each childx As Accessibility.IAccessible In children
                            If childx.accRole(0) = 22 Then
                                Dim children2() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(childx)
                                GetLocation(childx, children2.Count)
                                Continue For
                            End If
                        Next

                        Dim cL = childLeft
                        Dim cT = childTop
                        Dim cW = childWidth
                        Dim cH = childHeight

                        Try
                            Dim testiferror = cL
                        Catch ex As Exception
                            'Current taskbar is empty go to next taskbar.
                            Continue For
                        End Try

                        Dim Orientation As String
                        Dim TaskbarCount As Integer
                        Dim TrayWndSize As Integer

                        'Get current taskbar orientation (H = Horizontal | V = Vertical)
                        If tH >= 200 Then
                            Orientation = "V"
                        Else
                            Orientation = "H"
                        End If

                        'Console.WriteLine(Orientation)

                        'Get the end position of the last icon in the taskbar
                        If Orientation = "H" Then
                            TaskbarCount = cL + cW
                        Else
                            TaskbarCount = cT + cH
                        End If

                        'Gets the width of the whole taskbars placeholder
                        If Orientation = "H" Then
                            TrayWndSize = tW
                        Else
                            TrayWndSize = tH
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
                        Dim t3 As Thread = New Thread(AddressOf PositionCalculator)
                        t3.Start()

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
                    If ex.ToString.Contains("NullReference") Or ex.ToString.Contains("Missing method") Then
                        Dim Handle As IntPtr
                        Do
                            Handle = Nothing
                            System.Threading.Thread.Sleep(250)
                            Handle = Win32.FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
                        Loop Until Not Handle = Nothing
                        Application.Restart()
                    End If

                End Try
            Loop
        Catch ex As Exception
            MessageBox.Show("@Looper2 | " & ex.Message)
        End Try
    End Sub

#End Region

#Region "TrayLoopFix"

    Public Shared Sub TrayLoopFix()

        Try
            Dim Shell_TrayWnd = Win32.FindWindowByClass("Shell_TrayWnd", 0)
            Dim TrayNotifyWnd = Win32.FindWindowEx(Shell_TrayWnd, 0, "TrayNotifyWnd", Nothing)
            Dim ReBarWindow32 = Win32.FindWindowEx(Shell_TrayWnd, 0, "ReBarWindow32", Nothing)
            Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, 0, "MSTaskSwWClass", Nothing)
            Dim MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, 0, "MSTaskListWClass", Nothing)

            If MSTaskListWClass = Nothing Then
                MessageBox.Show("TaskbarX: Could not find the handle of the taskbar. Your current OS may not be supported.")
                End
            End If

            Dim accessible As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(MSTaskListWClass)

            Dim accessible2 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TrayNotifyWnd)

            Dim accessible3 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(ReBarWindow32)
            GetLocation3(accessible3, 0)
            Dim ReBarcL = childLeft3
            Dim ReBarcT = childTop3
            Dim ReBarcW = childWidth3
            Dim ReBarcH = childHeight3

            Dim SWP_NOSIZE As UInt32 = 1
            Dim SWP_ASYNCWINDOWPOS As UInt32 = 16384
            Dim SWP_NOACTIVATE As UInt32 = 16
            Dim SWP_NOSENDCHANGING As UInt32 = 1024
            Dim SWP_NOZORDER As UInt32 = 4

            Do

                GetLocation3(accessible2, 0)
                Dim TrayNotifycL = childLeft3
                Dim TrayNotifycT = childTop3
                Dim TrayNotifycW = childWidth3
                Dim TrayNotifycH = childHeight3

                GetLocation3(accessible, 0)
                Dim TaskListcL = childLeft3
                Dim TaskListcT = childTop3
                Dim TaskListcW = childWidth3
                Dim TaskListcH = childHeight3

                Win32.SendMessage(ReBarWindow32, 11, False, 0)
                Win32.SendMessage(Win32.GetParent(Shell_TrayWnd), 11, False, 0)

                Dim TrayNotifyWidth As Integer = 0
                Dim OldTrayNotifyWidth As Integer
                Dim TrayOrientation As String

                'If the TrayNotifyWnd updates then refresh the taskbar
                If TaskListcH >= 200 Then
                    TrayOrientation = "V"
                Else
                    TrayOrientation = "H"
                End If

                TrayNotifyWidth = TrayNotifycW

                If Not TrayNotifyWidth = OldTrayNotifyWidth Then
                    If Not OldTrayNotifyWidth = 0 Then
                        If Not TaskListcL = 0 Then
                            If TrayNotifycL = 3 Then
                                '
                                Exit Sub
                            End If
                            '  Dim offset = CInt(TaskList.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                            Dim pos = (TaskListcL - ReBarcL - 2).ToString.Replace("-", "")

                            trayfixed = False

                            setposhwnd = MSTaskListWClass
                            setpospos = pos
                            setposori = TrayOrientation

                            Dim t1 As Thread = New Thread(AddressOf setpos)
                            t1.Start()

                            Thread.Sleep(5)
                            Win32.SendMessage(ReBarWindow32, 11, True, 0)
                            Thread.Sleep(5)
                            Win32.SendMessage(ReBarWindow32, 11, False, 0)
                            Thread.Sleep(5)
                            trayfixed = True

                        End If
                    End If
                End If

                OldTrayNotifyWidth = TrayNotifyWidth

                Thread.Sleep(400)

            Loop
        Catch ex As Exception
            Console.WriteLine("@TrayLoopFix | " & ex.Message)
        End Try
    End Sub

#End Region

#Region "PositionCalculator"

    Public Shared Sub PositionCalculator()
        Try
            'Calculate the new positions and pass them through to the animator

            Dim Taskbars As New ArrayList

            'Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
            For Each Taskbar In windowHandles
                Dim sClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(Taskbar, sClassName, 256)

                Dim MSTaskListWClass As IntPtr

                If sClassName.ToString = "Shell_TrayWnd" Then
                    Dim ReBarWindow32 = Win32.FindWindowEx(Taskbar, 0, "ReBarWindow32", Nothing)
                    Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, 0, "MSTaskSwWClass", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, 0, "MSTaskListWClass", Nothing)
                End If

                If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                    Dim WorkerW = Win32.FindWindowEx(Taskbar, 0, "WorkerW", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(WorkerW, 0, "MSTaskListWClass", Nothing)
                End If

                If MSTaskListWClass = Nothing Then
                    MessageBox.Show("TaskbarX: Could not find the handle of the taskbar. Your current OS may not be supported.")
                    End
                End If

                Taskbars.Add(MSTaskListWClass)
            Next

            'Calculate Position for every taskbar and trigger the animator
            For Each TaskList In Taskbars

                Dim sClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(TaskList, sClassName, 256)

                Dim ChildFirstcL As Integer
                Dim ChildFirstcT As Integer
                Dim ChildFirstcW As Integer
                Dim ChildFirstcH As Integer

                Dim ChildLastcL As Integer
                Dim ChildLastcT As Integer
                Dim ChildLastcW As Integer
                Dim ChildLastcH As Integer

                Dim accessible As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TaskList)
                Dim children() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(accessible)

                GetLocation2(accessible, 0)

                Dim TaskListcL As Integer = childLeft2
                Dim TaskListcT As Integer = childTop2
                Dim TaskListcW As Integer = childWidth2
                Dim TaskListcH As Integer = childHeight2

                For Each childx As Accessibility.IAccessible In children

                    If childx.accRole(0) = 22 Then

                        Dim children2() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(childx)
                        Dim Count As Integer = 0
                        Count = 0

                        For Each ccc As Accessibility.IAccessible In children2
                            ' If childx.accRole(ccc) = 43 Then
                            ' If childx.accState(ccc) = 1074790400 Or childx.accState(ccc) = 1074790408 Then
                            If Not childx.accName(ccc) = "" Then
                                Count = Count + 1
                            End If
                        Next

                        GetLocation2(childx, 0)
                        ChildFirstcL = childLeft2
                        ChildFirstcT = childTop2
                        ChildFirstcW = childWidth2
                        ChildFirstcH = childHeight2

                        GetLocation2(childx, Count)
                        ChildLastcL = childLeft2
                        ChildLastcT = childTop2
                        ChildLastcW = childWidth2
                        ChildLastcH = childHeight2

                        Continue For
                    End If

                Next

                Dim RebarHandle = Win32.GetParent(TaskList)
                Dim accessible3 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(RebarHandle)

                Dim RebarClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(RebarHandle, RebarClassName, 256)

                Dim Orientation As String
                Dim TaskbarWidth As Integer
                Dim TrayWndLeft As Integer
                Dim TrayWndWidth As Integer
                Dim RebarWndLeft As Integer
                Dim TaskbarLeft As Integer
                Dim Position As Integer
                Dim curleft As Integer
                Dim curleft2 As Integer

                Dim TrayNotifycL As Integer
                Dim TrayNotifycT As Integer
                Dim TrayNotifycW As Integer
                Dim TrayNotifycH As Integer

                Dim TrayWndHandle = Win32.GetParent(Win32.GetParent(TaskList))

                Dim TrayWndClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256)

                'Check if TrayWnd = wrong. if it is, correct it (This will be the primary taskbar which should be Shell_TrayWnd)
                If TrayWndClassName.ToString = "ReBarWindow32" Then
                    Win32.SendMessage(TrayWndHandle, 11, False, 0)
                    TrayWndHandle = Win32.GetParent(Win32.GetParent(Win32.GetParent(TaskList)))

                    Dim TrayNotify = Win32.FindWindowEx(TrayWndHandle, 0, "TrayNotifyWnd", Nothing)
                    Dim accessible4 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TrayNotify)

                    GetLocation2(accessible4, 0)
                    TrayNotifycL = childLeft2
                    TrayNotifycT = childTop2
                    TrayNotifycW = childWidth2
                    TrayNotifycH = childHeight2

                    Win32.SendMessage(Win32.GetParent(TrayWndHandle), 11, False, 0)

                End If

                Call Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256)
                Dim accessible2 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TrayWndHandle)

                GetLocation2(accessible2, 0)
                Dim TrayWndcL As Integer = childLeft2
                Dim TrayWndcT As Integer = childTop2
                Dim TrayWndcW As Integer = childWidth2
                Dim TrayWndcH As Integer = childHeight2

                GetLocation2(accessible3, 0)
                Dim RebarcL As Integer = childLeft2
                Dim RebarcT As Integer = childTop2
                Dim RebarcW As Integer = childWidth2
                Dim RebarcH As Integer = childHeight2

                'If the taskbar is still moving then wait until it's not (This will prevent unneeded calculations that trigger the animator)
                Do
                    curleft = TaskListcL
                    GetLocation2(accessible, 0)
                    TaskListcL = childLeft2
                    System.Threading.Thread.Sleep(30)
                    curleft2 = TaskListcL
                Loop Until curleft = curleft2

                'Get current taskbar orientation (H = Horizontal | V = Vertical)
                If TaskListcH >= 200 Then
                    Orientation = "V"
                Else
                    Orientation = "H"
                End If

                'Calculate the exact width of the total icons
                Try
                    If Orientation = "H" Then
                        TaskbarWidth = CInt((ChildFirstcL - TaskListcL) + (ChildLastcL - TaskListcL) + ChildLastcW)
                    Else
                        TaskbarWidth = CInt((ChildFirstcT - TaskListcT) + (ChildLastcT - TaskListcT) + ChildLastcH)
                    End If
                Catch
                    TaskbarWidth = 0
                    'Taskbar is empty just skip
                End Try

                'Get info needed to calculate the position
                If Orientation = "H" Then
                    TrayWndLeft = CInt(TrayWndcL.ToString.Replace("-", ""))
                    TrayWndWidth = CInt(TrayWndcW.ToString.Replace("-", ""))
                    RebarWndLeft = CInt(RebarcL.ToString.Replace("-", ""))
                    TaskbarLeft = CInt((RebarWndLeft - TrayWndLeft).ToString.Replace("-", ""))
                Else
                    TrayWndLeft = CInt(TrayWndcT.ToString.Replace("-", ""))
                    TrayWndWidth = CInt(TrayWndcH.ToString.Replace("-", ""))
                    RebarWndLeft = CInt(RebarcT.ToString.Replace("-", ""))
                    TaskbarLeft = CInt((RebarWndLeft - TrayWndLeft).ToString.Replace("-", ""))
                End If

                'Calculate new position
                If TrayWndClassName.ToString = "Shell_TrayWnd" Then
                    If Settings.CenterInBetween = 1 Then
                        If Orientation = "H" Then
                            Dim offset = (TrayNotifycW / 2 - (TaskbarLeft \ 2))
                            Position = CInt((TrayWndWidth / 2 - (TaskbarWidth / 2) - TaskbarLeft - offset).ToString.Replace("-", "")) + Settings.PrimaryTaskbarOffset
                        Else
                            Dim offset = (TrayNotifycH / 2 - (TaskbarLeft \ 2))
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
                        If TrayWndClassName.ToString = "Shell_TrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.OnBatteryAnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    Else
                        If Orientation = "H" Then

                            If Settings.OnBatteryAnimationStyle = "none" Then
                                Win32.SetWindowPos(TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            End If

                            If Settings.OnBatteryAnimationStyle = "linear" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                            End If
                        Else

                            If Settings.OnBatteryAnimationStyle = "none" Then
                                Win32.SetWindowPos(TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            End If

                            If Settings.OnBatteryAnimationStyle = "linear" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "circeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.OnBatteryAnimationStyle = "backeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                        End If
                    End If

#End Region

                Else

#Region "Animation Trigger"

                    If Settings.CenterPrimaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_TrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If
                            Else

                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If

                                If Settings.AnimationStyle = "linear" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "expoeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "circeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quadeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "sineeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "cubiceaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quarteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "quinteaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "elasticeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "bounceeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeasein" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseinout" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                                End If

                                If Settings.AnimationStyle = "backeaseoutin" Then
                                    Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                                End If

                            End If
                        End If
                    Else
                        If Orientation = "H" Then

                            If Settings.AnimationStyle = "none" Then
                                Win32.SetWindowPos(TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            End If

                            If Settings.AnimationStyle = "linear" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeasein" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseinout" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseoutin" Then
                                Animate(TaskList, (TaskListcL - RebarcL), "H", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
                            End If
                        Else

                            If Settings.AnimationStyle = "none" Then
                                Win32.SetWindowPos(TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            End If

                            If Settings.AnimationStyle = "linear" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.Linear, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "expoeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ExpoEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "circeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CircEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quadeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuadEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "sineeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.SineEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "cubiceaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.CubicEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quarteaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuartEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "quinteaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.QuintEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "elasticeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.ElasticEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "bounceeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BounceEaseOutIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeasein" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseIn, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseinout" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseInOut, Position, Settings.AnimationSpeed)
                            End If

                            If Settings.AnimationStyle = "backeaseoutin" Then
                                Animate(TaskList, (TaskListcT - RebarcT), "V", AddressOf Easings.BackEaseOutIn, Position, Settings.AnimationSpeed)
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

#End Region

End Class