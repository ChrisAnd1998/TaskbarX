Option Strict On

Imports System.Text
Imports System.Threading

Public Class Main

    Public Shared noty As New NotifyIcon

    Public Shared Sub Main()
        Try
            'Kill every other running instance of TaskbarX
            Try
                For Each prog As Process In Process.GetProcessesByName("TaskbarX")
                    If Not prog.Id = Process.GetCurrentProcess.Id Then
                        prog.Kill()
                    End If
                Next
            Catch
            End Try

            'Set default settings
            Settings.TaskbarStyle = 0
            Settings.PrimaryTaskbarOffset = 0
            Settings.SecondaryTaskbarOffset = 0
            Settings.CenterPrimaryOnly = 0
            Settings.CenterSecondaryOnly = 0
            Settings.AnimationStyle = "cubiceaseinout"
            Settings.AnimationSpeed = 300
            Settings.LoopRefreshRate = 400
            Settings.CenterInBetween = 0
            Settings.DontCenterTaskbar = 0
            Settings.FixToolbarsOnTrayChange = 1
            Settings.OnBatteryAnimationStyle = "cubiceaseinout"
            Settings.OnBatteryLoopRefreshRate = 400

            'Read the arguments for the settings
            Dim arguments() As String = Environment.GetCommandLineArgs
            For Each argument In arguments
                Dim val() As String = Split(argument, "=")
                If argument.Contains("-stop") Then
                    noty.Visible = False
                    TaskbarCenter.RevertToZero()
                    ResetTaskbarStyle()
                    End
                End If
                If argument.Contains("-console") Then
                    Win32.AllocConsole()
                    Settings.ConsoleEnabled = 1
                End If
                If argument.Contains("-tbs=") Then
                    Settings.TaskbarStyle = CInt(val(1))
                End If

                If argument.Contains("-color") Then
                    Dim colorval As String = val(1)
                    Dim colorsep = colorval.Split(CType(";", Char()))

                    Settings.TaskbarStyleRed = CInt(colorsep(0))
                    Settings.TaskbarStyleGreen = CInt(colorsep(1))
                    Settings.TaskbarStyleBlue = CInt(colorsep(2))
                    Settings.TaskbarStyleAlpha = CInt(colorsep(3))
                End If

                If argument.Contains("-ptbo=") Then
                    Settings.PrimaryTaskbarOffset = CInt(val(1))
                End If
                If argument.Contains("-stbo=") Then
                    Settings.SecondaryTaskbarOffset = CInt(val(1))
                End If
                If argument.Contains("-cpo=") Then
                    Settings.CenterPrimaryOnly = CInt(val(1))
                End If
                If argument.Contains("-cso=") Then
                    Settings.CenterSecondaryOnly = CInt(val(1))
                End If
                If argument.Contains("-as=") Then
                    Settings.AnimationStyle = CType(val(1), String)
                End If
                If argument.Contains("-asp=") Then
                    Settings.AnimationSpeed = CInt(val(1))
                End If
                If argument.Contains("-lr=") Then
                    Settings.LoopRefreshRate = CInt(val(1))
                End If
                If argument.Contains("-cib=") Then
                    Settings.CenterInBetween = CInt(val(1))
                End If
                If argument.Contains("-obas=") Then
                    Settings.OnBatteryAnimationStyle = CType(val(1), String)
                End If
                If argument.Contains("-oblr=") Then
                    Settings.OnBatteryLoopRefreshRate = CInt(val(1))
                End If
                If argument.Contains("-ftotc=") Then
                    Settings.FixToolbarsOnTrayChange = CInt(val(1))
                End If
                If argument.Contains("-sr=") Then
                    Settings.SkipResolution = CInt(val(1))
                End If
                If argument.Contains("-dtbsowm=") Then
                    Settings.DefaultTaskbarStyleOnWinMax = CInt(val(1))
                End If
                If argument.Contains("-cfsa=") Then
                    Settings.CheckFullscreenApp = CInt(val(1))
                End If
                If argument.Contains("-dct=") Then
                    Settings.DontCenterTaskbar = CInt(val(1))
                End If
                If argument.Contains("-hps=") Then
                    Settings.HidePrimaryStartButton = CInt(val(1))
                End If
                If argument.Contains("-hss=") Then
                    Settings.HideSecondaryStartButton = CInt(val(1))
                End If
                If argument.Contains("-hpt=") Then
                    Settings.HidePrimaryNotifyWnd = CInt(val(1))
                End If
                If argument.Contains("-hst=") Then
                    Settings.HideSecondaryNotifyWnd = CInt(val(1))
                End If
                If argument.Contains("-sti=") Then
                    Settings.ShowTrayIcon = CInt(val(1))
                End If
                If argument.Contains("-tbsom=") Then
                    Settings.TaskbarStyleOnMax = CInt(val(1))
                End If
                If argument.Contains("-stsb=") Then
                    Settings.StickyStartButton = CInt(val(1))
                End If
            Next

            'If animation speed is lower than 1 then make it 1. Otherwise it will give an error.
            If Settings.AnimationSpeed <= 1 Then
                Settings.AnimationSpeed = 1
            End If

            'Makes the animations run smoother
            Dim currentProcess As Process = Process.GetCurrentProcess
            currentProcess.PriorityClass = ProcessPriorityClass.Idle

            'Prevent wrong position calculations
            Win32.SetProcessDpiAwareness(Win32.PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware)

            'Wait for Shell_TrayWnd
            Dim Handle As IntPtr
            Do
                Console.WriteLine("Waiting for Shell_TrayWnd")
                Handle = Nothing
                Thread.Sleep(250)
                Dim Shell_TrayWnd = Win32.FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
                Dim TrayNotifyWnd = Win32.FindWindowEx(Shell_TrayWnd, CType(0, IntPtr), "TrayNotifyWnd", Nothing)
                Dim ReBarWindow32 = Win32.FindWindowEx(Shell_TrayWnd, CType(0, IntPtr), "ReBarWindow32", Nothing)
                Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
                Dim MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)
                Handle = MSTaskListWClass
                'Lock the Taskbar
                '  Win32.PostMessage(Shell_TrayWnd, CUInt(&H111), CType(424, IntPtr), CType(vbNullString, IntPtr))
            Loop Until Not Handle = Nothing

            'Just empty startup memory before starting
            ClearMemory()

            'Reset the taskbar style...
            ResetTaskbarStyle()

            If Settings.ShowTrayIcon = 1 Then
                noty.Text = "TaskbarX (L = Restart) (M = Config) (R = Stop)"
                noty.Icon = My.Resources.icon
                noty.Visible = True
            End If

            AddHandler noty.MouseClick, AddressOf MnuRef_Click

            'Start the TaskbarCenterer
            If Not Settings.DontCenterTaskbar = 1 Then
                Dim t1 As Thread = New Thread(AddressOf TaskbarCenter.TaskbarCenterer)
                t1.Start()
            End If

            'Start the TaskbarStyler if enabled
            If Settings.TaskbarStyle = 1 Or Settings.TaskbarStyle = 2 Or Settings.TaskbarStyle = 3 Or Settings.TaskbarStyle = 4 Or Settings.TaskbarStyle = 5 Then
                Dim t2 As Thread = New Thread(AddressOf TaskbarStyle.TaskbarStyler)
                t2.Start()
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Public Shared Sub MnuRef_Click(sender As Object, e As MouseEventArgs)

        If (e.Button = MouseButtons.Left) Then
            noty.Visible = False
            Application.Restart()
        ElseIf (e.Button = MouseButtons.Right) Then
            noty.Visible = False
            TaskbarCenter.RevertToZero()
            ResetTaskbarStyle()
            End
        ElseIf (e.Button = MouseButtons.Middle) Then
            If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
                Try
                    Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                        .WindowStyle = ProcessWindowStyle.Hidden,
                        .FileName = "cmd.exe",
                        .Arguments = " /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarXGUI"
                    }
                    Process.Start(processInfo)
                Catch
                End Try
            Else
                Try
                    System.Diagnostics.Process.Start("TaskbarX Configurator.exe")
                Catch
                End Try
            End If
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

    Public Shared windowHandles As ArrayList = New ArrayList()

    Public Shared Sub ResetTaskbarStyle()
        GetActiveWindows()

        Dim trays As New ArrayList
        For Each trayWnd As IntPtr In windowHandles
            Console.WriteLine(trayWnd)
            trays.Add(trayWnd)
        Next

        For Each tray As IntPtr In trays
            Dim trayptr As IntPtr = tray
            Win32.SendMessage(trayptr, Win32.WM_THEMECHANGED, True, 0)
            Win32.SendMessage(trayptr, Win32.WM_DWMCOLORIZATIONCOLORCHANGED, True, 0)
            Win32.SendMessage(trayptr, Win32.WM_DWMCOMPOSITIONCHANGED, True, 0)
        Next
    End Sub

    Public Shared Sub RestartExplorer()
        For Each MyProcess In Process.GetProcessesByName("explorer")
            MyProcess.Kill()
        Next
    End Sub

    Public Shared Function ClearMemory() As Int32
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()
        Return Win32.SetProcessWorkingSetSize(Diagnostics.Process.GetCurrentProcess.Handle, -1, -1)
    End Function

#End Region

End Class