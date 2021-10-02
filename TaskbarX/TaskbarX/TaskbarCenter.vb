Option Strict On

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Threading
Imports Accessibility
Imports Microsoft.Win32

Public Class TaskbarCenter

#Region "Values"

    Public Shared ScreensChanged As Boolean

    Public Shared TaskbarCount As Integer

    Public Shared windowHandles As ArrayList = New ArrayList()

    Public Shared trayfixed As Boolean
    Public Shared setposhwnd As IntPtr
    Public Shared setpospos As Integer
    Public Shared setposori As String

    Public Shared initposcalc As String
    Public Shared initposcalcready As Boolean

    Public Shared isanimating As Boolean

    Public Shared UserPref As New Microsoft.Win32.UserPreferenceChangedEventHandler(AddressOf HandlePrefChange)

#End Region

    Public Shared Sub TaskbarCenterer()

        RevertToZero()

        AddHandler SystemEvents.DisplaySettingsChanged, AddressOf DPChange

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

    Public Declare Function EnumWindows Lib "user32" (ByVal Adress As CallBack, ByVal y As Integer) As Integer

    Public Delegate Function CallBack(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean

    Public Shared ActiveWindows As New System.Collections.ObjectModel.Collection(Of IntPtr)

    Public Shared Function GetActiveWindows() As ObjectModel.Collection(Of IntPtr)
        windowHandles.Clear()
        EnumWindows(AddressOf Enumerator, 0)
        Return ActiveWindows
    End Function

    Public Shared Function Enumerator(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Dim sClassName As New StringBuilder("", 256)
        Call Win32.GetClassName(hwnd, sClassName, 256)
        If sClassName.ToString = "Shell_TrayWnd" Or sClassName.ToString = "Shell_SecondaryTrayWnd" Then
            windowHandles.Add(hwnd)
        End If
        Return True
    End Function

    Structure RectangleX
        Dim left As Integer
        Dim top As Integer
        Dim width As Integer
        Dim height As Integer
    End Structure

    Public Shared Function GetLocation(ByVal acc As Accessibility.IAccessible, ByVal idChild As Integer) As RectangleX
        Dim rect As New RectangleX
        If Not IsNothing(acc) Then        
            acc.accLocation(rect.left, rect.top, rect.width, rect.height, idChild)
        End If            
        Return rect
    End Function

    Public Shared Sub SetPos()
        If setposori = "H" Then
            Do
                Win32.SetWindowPos(setposhwnd, IntPtr.Zero, setpospos, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                If isanimating = True Then
                    Exit Do
                End If
            Loop Until trayfixed = True
        Else
            Do
                Win32.SetWindowPos(setposhwnd, IntPtr.Zero, 0, setpospos, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                If isanimating = True Then
                    Exit Do
                End If
            Loop Until trayfixed = True
        End If
    End Sub

    Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal isPrimary As Boolean, ByVal width As Integer)
        Try
            Dim t1 As Thread = New Thread(Sub() TaskbarAnimate.Animate(hwnd, oldpos, orient, easing, valueToReach, duration, isPrimary, width))
            t1.Start()
        Catch ex As Exception

            Console.WriteLine("@Animation Call | " & ex.Message)
        End Try
    End Sub

    Public Shared revertcycle As Boolean

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
                Dim MStart = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "Start", Nothing)
                Win32.ShowWindow(MStart, Win32.ShowWindowCommands.Show)

                Dim MTray = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "TrayNotifyWnd", Nothing)
                Win32.SetWindowLong(MTray, CType(Win32.GWL_STYLE, Win32.WindowStyles), &H56000000)
                Win32.SetWindowLong(MTray, CType(Win32.GWL_EXSTYLE, Win32.WindowStyles), &H2000)
                Win32.SendMessage(MTray, 11, True, 0)
                Win32.ShowWindow(MTray, Win32.ShowWindowCommands.Show)

                Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
                MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)
            End If

            If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                Dim WorkerW = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "WorkerW", Nothing)
                Dim SStart = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "Start", Nothing)
                Win32.ShowWindow(SStart, Win32.ShowWindowCommands.Show)
                Dim STray = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "ClockButton", Nothing)
                Win32.ShowWindow(STray, Win32.ShowWindowCommands.Show)
                MSTaskListWClass = Win32.FindWindowEx(WorkerW, CType(0, IntPtr), "MSTaskListWClass", Nothing)
            End If

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
        '' Console.WriteLine(e.Category)
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

                    If Not Settings.TotalPrimaryOpacity = Nothing Then
                        Win32.SetWindowLong(CType(Taskbar, IntPtr), CType(Win32.GWL_EXSTYLE, Win32.WindowStyles), &H80000)
                        Win32.SetLayeredWindowAttributes(CType(Taskbar, IntPtr), 0, CByte(255 / 100 * CByte(Settings.TotalPrimaryOpacity)), &H2)
                    End If

                    If Settings.HidePrimaryStartButton = 1 Then
                        Dim MStart = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "Start", Nothing)
                        Win32.ShowWindow(MStart, Win32.ShowWindowCommands.Hide)
                        Win32.SetLayeredWindowAttributes(MStart, 0, 0, &H2)
                    End If

                    If Settings.HidePrimaryNotifyWnd = 1 Then
                        Dim MTray = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "TrayNotifyWnd", Nothing)
                        Win32.ShowWindow(MTray, Win32.ShowWindowCommands.Hide)
                        Win32.SetWindowLong(MTray, CType(Win32.GWL_STYLE, Win32.WindowStyles), &H7E000000)
                        Win32.SetWindowLong(MTray, CType(Win32.GWL_EXSTYLE, Win32.WindowStyles), &H80000)
                        Win32.SendMessage(MTray, 11, False, 0)
                        Win32.SetLayeredWindowAttributes(MTray, 0, 0, &H2)
                    End If

                    Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)




                End If

                If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                    Dim WorkerW = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "WorkerW", Nothing)

                    If Not Settings.TotalSecondaryOpacity = Nothing Then
                        Win32.SetWindowLong(CType(Taskbar, IntPtr), CType(Win32.GWL_EXSTYLE, Win32.WindowStyles), &H80000)
                        Win32.SetLayeredWindowAttributes(CType(Taskbar, IntPtr), 0, CByte(255 / 100 * CByte(Settings.TotalSecondaryOpacity)), &H2)
                    End If

                    If Settings.HideSecondaryStartButton = 1 Then
                        Dim SStart = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "Start", Nothing)
                        Win32.ShowWindow(SStart, Win32.ShowWindowCommands.Hide)
                        Win32.SetLayeredWindowAttributes(SStart, 0, 0, &H2)
                    End If

                    If Settings.HideSecondaryNotifyWnd = 1 Then
                        Dim STray = Win32.FindWindowEx(CType(Taskbar, IntPtr), CType(0, IntPtr), "ClockButton", Nothing)
                        Win32.ShowWindow(STray, Win32.ShowWindowCommands.Hide)
                        Win32.SetLayeredWindowAttributes(STray, 0, 0, &H2)
                    End If

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

                    If Not Settings.SkipResolution2 = 0 Then
                        If Screen.PrimaryScreen.Bounds.Width = Settings.SkipResolution2 Then
                            RevertToZero()
                            Exit Do
                        End If
                    End If

                    If Not Settings.SkipResolution3 = 0 Then
                        If Screen.PrimaryScreen.Bounds.Width = Settings.SkipResolution3 Then
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

                            ''Main.Toaster("Fullscreen App detected... TaskbarX will now be disabled.")
                            ''  Main.ResetTaskbarStyle()
                            ''RevertToZero()

                            Settings.Pause = True
                            Do
                                System.Threading.Thread.Sleep(500)
                                activewindow = Win32.GetForegroundWindow()
                                Win32.GetWindowRect(activewindow, activewindowsize)
                                System.Threading.Thread.Sleep(500)

                            Loop While activewindowsize.Top = curmonx.Bounds.Top And activewindowsize.Bottom = curmonx.Bounds.Bottom And activewindowsize.Left = curmonx.Bounds.Left And activewindowsize.Right = curmonx.Bounds.Right
                            Console.WriteLine("Fullscreen App deactivated")

                            ''  Main.Toaster("Fullscreen App closed or minimized... TaskbarX will now be enabled.")

                            Settings.Pause = False
                        End If
                    End If

                    'Go through each taskbar and result in a unique string containing the current state

                    Dim i As Integer = 0

                    For Each TaskList In TaskObjects

                        Dim children() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(CType(TaskList, IAccessible))

                        Dim TaskListPos As RectangleX = GetLocation(CType(TaskList, IAccessible), 0)


                        Dim tH = TaskListPos.height
                        Dim tW = TaskListPos.width

                        Dim LastChildPos As RectangleX

                        For Each childx As Accessibility.IAccessible In children
                            If CInt(childx.accRole(0)) = &H16 Then '0x16 = toolbar
                                LastChildPos = GetLocation(childx, MSAA.GetAccessibleChildren(childx).Length)
                                Exit For
                            End If
                        Next

                        Dim cL = LastChildPos.left
                        Dim cT = LastChildPos.top
                        Dim cW = LastChildPos.width
                        Dim cH = LastChildPos.height

                        Try
                            Dim testiferror = cL
                        Catch ex As Exception
                            'Current taskbar is empty go to next taskbar.
                            ''Continue For
                        End Try

                        Dim Orientation As String
                        Dim TaskbarCount As Integer
                        Dim TrayWndSize As Integer

                        'Get current taskbar orientation (H = Horizontal | V = Vertical)
                        If tH >= tW Then
                            Orientation = "V"
                        Else
                            Orientation = "H"
                        End If

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

                        initposcalcready = True

                        i += 1
                    Next




                    If Not results = oldresults Then
                        'Something has changed we can now calculate the new position for each taskbar

                        initposcalcready = False
                        initposcalc = results

                        'Start the PositionCalculator
                        Dim t3 As Thread = New Thread(AddressOf InitPositionCalculator)
                        t3.Start()

                    End If

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
                        System.Threading.Thread.Sleep(1000)
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

            Dim accessible As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(MSTaskListWClass)

            Dim accessible2 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TrayNotifyWnd)

            Dim accessible3 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(MSTaskSwWClass)

            Dim SWP_NOSIZE As UInt32 = 1
            Dim SWP_ASYNCWINDOWPOS As UInt32 = 16384
            Dim SWP_NOACTIVATE As UInt32 = 16
            Dim SWP_NOSENDCHANGING As UInt32 = 1024
            Dim SWP_NOZORDER As UInt32 = 4

            Do

                Dim RebarPos As RectangleX = GetLocation(accessible3, 0)
                Dim TrayNotifyPos As RectangleX = GetLocation(accessible2, 0)
                Dim TaskListPos As RectangleX = GetLocation(accessible, 0)

                Win32.SendMessage(ReBarWindow32, 11, False, 0)
                Win32.SendMessage(Win32.GetParent(Shell_TrayWnd), 11, False, 0)

                Dim TrayNotifyWidth As Integer = 0
                Dim OldTrayNotifyWidth As Integer
                Dim TrayOrientation As String

                'If the TrayNotifyWnd updates then refresh the taskbar
                If TaskListPos.height >= TaskListPos.width Then
                    TrayOrientation = "V"
                Else
                    TrayOrientation = "H"
                End If

                TrayNotifyWidth = TrayNotifyPos.width

                If Not TrayNotifyWidth = OldTrayNotifyWidth Then
                    If Not OldTrayNotifyWidth = 0 Then
                        If Not TaskListPos.left = 0 Then
                            If TrayNotifyPos.left = 3 Then
                                '
                                Exit Sub
                            End If

                            Dim pos = Math.Abs((TaskListPos.left - RebarPos.left))

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

    Public Shared Sub InitPositionCalculator()

        Dim mm As String
        Dim mm2 As String

        mm = initposcalc

        Do
            Thread.Sleep(10)
        Loop Until initposcalcready = True

        mm2 = initposcalc

        If mm = mm2 Then
            'Start the PositionCalculator
            Dim t3 As Thread = New Thread(AddressOf PositionCalculator)
            t3.Start()
        End If
    End Sub

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

                Win32.SetWindowLong(CType(Taskbar, IntPtr), CType(Win32.GWL_EXSTYLE, Win32.WindowStyles), &H80L)

                If MSTaskListWClass = Nothing Then
                    Console.WriteLine("TaskbarX: Could not find the handle of the taskbar. Restarting...")
                    System.Threading.Thread.Sleep(1000)
                    Application.Restart()
                End If

                Taskbars.Add(MSTaskListWClass)
            Next

            'Calculate Position for every taskbar and trigger the animator
            For Each TaskList In Taskbars

                Dim sClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(CType(TaskList, IntPtr), sClassName, 256)

                Dim LastChildPos As RectangleX
                Dim TaskListPos As RectangleX

                Dim accessible As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(CType(TaskList, IntPtr))
                Dim children() As Accessibility.IAccessible = MSAA.GetAccessibleChildren(accessible)

                TaskListPos = GetLocation(accessible, 0)

                For Each childx As Accessibility.IAccessible In children
                    If CInt(childx.accRole(0)) = &H16 Then '0x16 = toolbar
                        LastChildPos = GetLocation(childx, MSAA.GetAccessibleChildren(childx).Length)
                        Exit For
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

                Dim TrayNotifyPos As RectangleX
                Dim NewsAndInterestsPos As RectangleX
                Dim NewsAndInterestsHandle As IntPtr


                Dim TrayWndHandle = Win32.GetParent(Win32.GetParent(CType(TaskList, IntPtr)))

                Dim TrayWndClassName As New StringBuilder("", 256)
                Call Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256)

                'Check if TrayWnd = wrong. if it is, correct it (This will be the primary taskbar which should be Shell_TrayWnd)
                If TrayWndClassName.ToString = "ReBarWindow32" Then
                    Win32.SendMessage(TrayWndHandle, 11, False, 0)
                    TrayWndHandle = Win32.GetParent(Win32.GetParent(Win32.GetParent(CType(TaskList, IntPtr))))

                    Dim TrayNotify = Win32.FindWindowEx(TrayWndHandle, CType(0, IntPtr), "TrayNotifyWnd", Nothing)
                    Dim accessible4 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TrayNotify)
                    TrayNotifyPos = GetLocation(accessible4, 0)


                    Dim NewsAndInterests = Win32.FindWindowEx(TrayWndHandle, CType(0, IntPtr), "DynamicContent1", Nothing)
                    NewsAndInterestsHandle = NewsAndInterests
                    Dim accessible5 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(NewsAndInterests)
                    NewsAndInterestsPos = GetLocation(accessible5, 0)




                    Win32.SendMessage(Win32.GetParent(TrayWndHandle), 11, False, 0)

                End If



                Call Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256)
                Dim accessible2 As Accessibility.IAccessible = MSAA.GetAccessibleObjectFromHandle(TrayWndHandle)

                Dim TrayWndPos As RectangleX = GetLocation(accessible2, 0)
                Dim RebarPos As RectangleX = GetLocation(accessible3, 0)

                'If the taskbar is still moving then wait until it's not (This will prevent unneeded calculations that trigger the animator)
                Do
                    curleft = TaskListPos.left
                    TaskListPos = GetLocation(accessible, 0)
                    '' TaskListcL = childLeft2
                    System.Threading.Thread.Sleep(30)
                    curleft2 = TaskListPos.left
                Loop Until curleft = curleft2

                'Get current taskbar orientation (H = Horizontal | V = Vertical)
                If TaskListPos.height >= TaskListPos.width Then
                    Orientation = "V"
                Else
                    Orientation = "H"
                End If




                If Not Settings.TaskbarRounding = 0 Then
                    Win32.SetWindowRgn(CType(TrayWndHandle, IntPtr), Win32.CreateRoundRectRgn(0, 0, TrayWndPos.width, TrayWndPos.height, Settings.TaskbarRounding, Settings.TaskbarRounding), True)
                End If




                'Calculate the exact width of the total icons
                Try
                    If Orientation = "H" Then
                        TaskbarWidth = CInt((LastChildPos.left - TaskListPos.left)) ''TaskbarTotalHeight
                    Else
                        TaskbarWidth = CInt((LastChildPos.top - TaskListPos.top))
                    End If
                Catch
                    TaskbarWidth = 0
                    'Taskbar is empty just skip
                End Try

                'Get info needed to calculate the position
                If Orientation = "H" Then
                    TrayWndLeft = Math.Abs(CInt(TrayWndPos.left))
                    TrayWndWidth = Math.Abs(CInt(TrayWndPos.width))
                    RebarWndLeft = Math.Abs(CInt(RebarPos.left))
                    TaskbarLeft = Math.Abs(CInt(RebarWndLeft - TrayWndLeft))
                Else
                    TrayWndLeft = Math.Abs(CInt(TrayWndPos.top))
                    TrayWndWidth = Math.Abs(CInt(TrayWndPos.height))
                    RebarWndLeft = Math.Abs(CInt(RebarPos.top))
                    TaskbarLeft = Math.Abs(CInt(RebarWndLeft - TrayWndLeft))
                End If

                Console.WriteLine("!" & NewsAndInterestsPos.width)

                'Calculate new position
                If TrayWndClassName.ToString = "Shell_TrayWnd" Then
                    If Settings.CenterInBetween = 1 Then
                        If Orientation = "H" Then
                            Dim offset = (TrayNotifyPos.width / 2 - (TaskbarLeft \ 2)) + NewsAndInterestsPos.width / 2
                            Position = Math.Abs(CInt((TrayWndWidth / 2 - (TaskbarWidth / 2) - TaskbarLeft - offset))) + Settings.PrimaryTaskbarOffset
                        Else
                            Dim offset = (TrayNotifyPos.height / 2 - (TaskbarLeft \ 2)) + NewsAndInterestsPos.height / 2
                            Position = Math.Abs(CInt((TrayWndWidth / 2 - (TaskbarWidth / 2) - TaskbarLeft - offset))) + Settings.PrimaryTaskbarOffset
                        End If
                    Else
                        Position = Math.Abs(CInt(CInt((TrayWndWidth / 2) - (TaskbarWidth / 2) - TaskbarLeft))) + Settings.PrimaryTaskbarOffset
                    End If
                Else
                    Position = Math.Abs(CInt(CInt((TrayWndWidth / 2) - (TaskbarWidth / 2) - TaskbarLeft))) + Settings.SecondaryTaskbarOffset
                End If

                'Trigger the animator
                If SystemInformation.PowerStatus.PowerLineStatus = PowerLineStatus.Offline Then

                    If Settings.CenterPrimaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_TrayWnd" Then
                            If Orientation = "H" Then
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                End If
                                DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListPos.left, RebarPos.left, "H", Position, True, TaskbarWidth)
                            Else
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListPos.top, RebarPos.top, "V", Position, True, TaskbarWidth)
                                End If
                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListPos.left, RebarPos.left, "H", Position, False, TaskbarWidth)
                                End If
                            Else
                                If Settings.OnBatteryAnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListPos.top, RebarPos.top, "V", Position, False, TaskbarWidth)
                                End If
                            End If
                        End If
                    Else
                        If Orientation = "H" Then
                            If Settings.OnBatteryAnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListPos.left, RebarPos.left, "H", Position, False, TaskbarWidth)
                            End If
                        Else
                            If Settings.OnBatteryAnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.OnBatteryAnimationStyle, CType(TaskList, IntPtr), TaskListPos.top, RebarPos.top, "V", Position, False, TaskbarWidth)
                            End If
                        End If
                    End If
                Else

                    If Settings.CenterPrimaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_TrayWnd" Then
                            If Orientation = "H" Then
                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListPos.left, RebarPos.left, "H", Position, True, TaskbarWidth)
                                End If
                            Else
                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListPos.top, RebarPos.top, "V", Position, True, TaskbarWidth)
                                End If
                            End If
                        End If
                    ElseIf Settings.CenterSecondaryOnly = 1 Then
                        If TrayWndClassName.ToString = "Shell_SecondaryTrayWnd" Then
                            If Orientation = "H" Then

                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListPos.left, RebarPos.left, "H", Position, False, TaskbarWidth)
                                End If
                            Else
                                If Settings.AnimationStyle = "none" Then
                                    Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                                Else
                                    DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListPos.top, RebarPos.top, "V", Position, False, TaskbarWidth)
                                End If
                            End If
                        End If
                    Else
                        If Orientation = "H" Then
                            If Settings.AnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListPos.left, RebarPos.left, "H", Position, False, TaskbarWidth)
                            End If
                        Else
                            If Settings.AnimationStyle = "none" Then
                                Win32.SetWindowPos(CType(TaskList, IntPtr), IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                            Else
                                DaAnimator(Settings.AnimationStyle, CType(TaskList, IntPtr), TaskListPos.top, RebarPos.top, "V", Position, False, TaskbarWidth)
                            End If
                        End If
                    End If

                End If

            Next
        Catch ex As Exception

            Console.WriteLine("@Calculator | " & ex.Message)

        End Try

    End Sub

    Private Shared Sub DaAnimator(animationStyle As String, taskList As IntPtr, taskListc As Integer, rebarc As Integer, orient As String, position As Integer, isprimary As Boolean, width As Integer)

        If animationStyle = "linear" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.Linear, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "expoeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "expoeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "expoeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "expoeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ExpoEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "circeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "circeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "circeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "circeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CircEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quadeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quadeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quadeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quadeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuadEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "sineeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "sineeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "sineeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "sineeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.SineEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "cubiceaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "cubiceasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "cubiceaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "cubiceaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.CubicEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quarteaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quarteasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quarteaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quarteaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuartEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quinteaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quinteasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quinteaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "quinteaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.QuintEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "elasticeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "elasticeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "elasticeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "elasticeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.ElasticEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "bounceeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "bounceeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "bounceeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "bounceeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BounceEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "backeaseout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "backeasein" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseIn, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "backeaseinout" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseInOut, position, Settings.AnimationSpeed, isprimary, width)
        ElseIf animationStyle = "backeaseoutin" Then
            Animate(CType(taskList, IntPtr), (taskListc - rebarc), orient, AddressOf Easings.BackEaseOutIn, position, Settings.AnimationSpeed, isprimary, width)
        End If
    End Sub

#End Region

End Class
