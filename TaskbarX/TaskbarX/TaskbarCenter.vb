Option Strict On


Imports System.Threading
Imports Microsoft.Win32
Imports System.Text
Imports Accessibility


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
        'AddHandler SystemEvents.UserPreferenceChanged, UserPref

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

    Public Shared Sub SetPos()
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

    Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer)
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
            Call Win32.GetClassName(CType(Taskbar, IntPtr), sClassName, 256)

            Dim MSTaskListWClass As IntPtr

            If sClassName.ToString = "Shell_TrayWnd" Then
                Dim ReBarWindow32 = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "ReBarWindow32", Nothing)
                Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
                MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)
            End If

            If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                Dim WorkerW = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "WorkerW", Nothing)
                MSTaskListWClass = Win32.FindWindowEx(WorkerW, CType(0, IntPtr), "MSTaskListWClass", Nothing)
            End If

            ' Console.WriteLine(MSTaskListWClass)

            Taskbars.Add(MSTaskListWClass)
        Next

        For Each TaskList In Taskbars
            Win32.SendMessage(Win32.GetParent(Win32.GetParent(CType(TaskList, IntPtr))), 11, True, 0)
            Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
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
                Call Win32.GetClassName(CType(Taskbar, IntPtr), sClassName, 256)

                Dim MSTaskListWClass As IntPtr

                If sClassName.ToString = "Shell_TrayWnd" Then
                    Dim ReBarWindow32 = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "ReBarWindow32", Nothing)
                    Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)
                End If

                If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                    Dim WorkerW = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "WorkerW", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(WorkerW, CType(0, IntPtr), "MSTaskListWClass", Nothing)
                End If

                If MSTaskListWClass = Nothing Then
                    MessageBox.Show("TaskbarX: Could not find the handle of the taskbar. Your current OS may not be supported.")
                    End
                End If

                Taskbars.Add(MSTaskListWClass)
            Next

            Dim TaskObject = New List(Of Accessibility.IAccessible)()
            For Each TaskList In Taskbars
                Dim accessiblex As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(CType(TaskList, IntPtr))
                TaskObject.Add(accessiblex)
            Next

            Dim TaskObjects As List(Of IAccessible) = TaskObject

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

                    Dim i As Integer = 0

                    For Each TaskList In TaskObjects
                        '  Do While i < CInt(TaskObjects.Count)
                        Dim children() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(CType(TaskList, IAccessible))

                        GetLocation(CType(TaskList, IAccessible), 0)

                        Dim tH = childHeight
                        Dim tW = childWidth

                        For Each childx As Accessibility.IAccessible In children
                            If CInt(childx.accRole(0)) = 22 Then
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
                            ' Continue For
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

                        i += 1
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
            Dim Shell_TrayWnd = Win32.FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
            Dim TrayNotifyWnd = Win32.FindWindowEx(Shell_TrayWnd, CType(0, IntPtr), "TrayNotifyWnd", Nothing)
            Dim ReBarWindow32 = Win32.FindWindowEx(Shell_TrayWnd, CType(0, IntPtr), "ReBarWindow32", Nothing)
            Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
            Dim MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)

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
                            setpospos = CInt(pos)
                            setposori = TrayOrientation

                            Dim t1 As Thread = New Thread(AddressOf SetPos)
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
                Call Win32.GetClassName(CType(Taskbar, IntPtr), sClassName, 256)

                Dim MSTaskListWClass As IntPtr

                If sClassName.ToString = "Shell_TrayWnd" Then
                    Dim ReBarWindow32 = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "ReBarWindow32", Nothing)
                    Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)
                End If

                If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                    Dim WorkerW = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "WorkerW", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(WorkerW, CType(0, IntPtr), "MSTaskListWClass", Nothing)
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
                Call Win32.GetClassName(CType(TaskList, IntPtr), sClassName, 256)

                Dim ChildFirstcL As Integer
                Dim ChildFirstcT As Integer
                Dim ChildFirstcW As Integer
                Dim ChildFirstcH As Integer

                Dim ChildLastcL As Integer
                Dim ChildLastcT As Integer
                Dim ChildLastcW As Integer
                Dim ChildLastcH As Integer

                Dim accessible As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(CType(TaskList, IntPtr))
                Dim children() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(accessible)

                GetLocation2(accessible, 0)

                Dim TaskListcL As Integer = childLeft2
                Dim TaskListcT As Integer = childTop2
                Dim TaskListcW As Integer = childWidth2
                Dim TaskListcH As Integer = childHeight2

                For Each childx As Accessibility.IAccessible In children

                    If CInt(childx.accRole(0)) = 22 Then

                        Dim children2() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(childx)
                        Dim Count As Integer = 0
                        Count = 0

                        For Each ccc As Accessibility.IAccessible In children2
                            Try
                                If CInt(childx.accRole(ccc)) = &H2B Or CInt(childx.accRole(ccc)) = &H39 Then 'push button (0x2B) | menu button (0x39)
                                    Count += 1
                                End If
                            Catch
                                Count += 1
                            End Try

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

                Dim RebarHandle = Win32.GetParent(CType(TaskList, IntPtr))
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

                Dim TrayWndHandle = Win32.GetParent(Win32.GetParent(CType(TaskList, IntPtr)))

                Dim TrayWndClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256)

                'Check if TrayWnd = wrong. if it is, correct it (This will be the primary taskbar which should be Shell_TrayWnd)
                If TrayWndClassName.ToString = "ReBarWindow32" Then
                    Win32.SendMessage(TrayWndHandle, 11, False, 0)
                    TrayWndHandle = Win32.GetParent(Win32.GetParent(Win32.GetParent(CType(TaskList, IntPtr))))

                    Dim TrayNotify = Win32.FindWindowEx(TrayWndHandle, CType(0, IntPtr), "TrayNotifyWnd", Nothing)
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
                        Position = CInt(CInt((TrayWndWidth / 2) - (TaskbarWidth / 2) - TaskbarLeft).ToString.Replace("-", "")) + Settings.PrimaryTaskbarOffset
                    End If
                Else
                    Position = CInt(CInt((TrayWndWidth / 2) - (TaskbarWidth / 2) - TaskbarLeft).ToString.Replace("-", "")) + Settings.SecondaryTaskbarOffset
                End If

                'Trigger the animator
                If SystemInformation.PowerStatus.PowerLineStatus = PowerLineStatus.Offline Then

#Region "Animation Trigger On Battery"

                    If Settings.CenterPrimaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_TrayWnd" Then
                            If Orientation = "H" Then
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If
                                DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListcL, RebarcL, "H", Position)
                            Else
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListcT, RebarcT, "V", Position)
                                End If
                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListcL, RebarcL, "H", Position)
                                End If
                            Else
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListcT, RebarcT, "V", Position)
                                End If
                            End If
                        End If
                    Else
                        If Orientation = "H" Then
                            If Settings.OnBatteryAnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListcL, RebarcL, "H", Position)
                            End If
                        Else
                            If Settings.OnBatteryAnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListcT, RebarcT, "V", Position)
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
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListcL, RebarcL, "H", Position)
                                End If
                            Else
                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListcT, RebarcT, "V", Position)
                                End If
                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListcL, RebarcL, "H", Position)
                                End If
                            Else
                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListcT, RebarcT, "V", Position)
                                End If
                            End If
                        End If
                    Else
                        If Orientation = "H" Then
                            If Settings.AnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListcL, RebarcL, "H", Position)
                            End If
                        Else
                            If Settings.AnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListcT, RebarcT, "V", Position)
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

    Private Shared Sub DaAnimator(animationStyle As String, taskList As IntPtr, taskListc As Integer, rebarc As Integer, orient As String, position As Integer)
        If animationStyle = "linear" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.Linear, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "expoeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "expoeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "expoeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "expoeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "circeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "circeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "circeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "circeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quadeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quadeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quadeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quadeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "sineeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "sineeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "sineeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "sineeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "cubiceaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "cubiceasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "cubiceaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "cubiceaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quarteaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quarteasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quarteaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quarteaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quinteaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quinteasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quinteaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "quinteaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "elasticeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "elasticeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "elasticeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "elasticeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "bounceeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "bounceeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "bounceeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "bounceeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseOutIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "backeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "backeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseIn, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "backeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseInOut, position, Settings.AnimationSpeed)
        ElseIf animationStyle = "backeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseOutIn, position, Settings.AnimationSpeed)
        End If
    End Sub

#End Region

End Class