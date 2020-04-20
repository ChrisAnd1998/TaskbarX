Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Reflection
Imports System.Windows.Automation

Public Class Main

#Region "Declarations"

    <DllImport("SHCore.dll", SetLastError:=True)>
    Private Shared Function SetProcessDpiAwareness(ByVal awareness As PROCESS_DPI_AWARENESS) As Boolean
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=Runtime.InteropServices.CharSet.Auto)>
    Private Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function SetProcessWorkingSetSize(ByVal hProcess As IntPtr, ByVal dwMinimumWorkingSetSize As Int32, ByVal dwMaximumWorkingSetSize As Int32) As Int32
    End Function

    Private Enum PROCESS_DPI_AWARENESS
        Process_DPI_Unaware = 0
        Process_System_DPI_Aware = 1
        Process_Per_Monitor_DPI_Aware = 2
    End Enum

    Public Shared WM_DWMCOLORIZATIONCOLORCHANGED As Integer = &H320
    Public Shared WM_DWMCOMPOSITIONCHANGED As Integer = &H31E
    Public Shared WM_THEMECHANGED As Integer = &H31A

    Public Const WM_SETREDRAW As Integer = 11

    Public Shared Shell_TrayWnd As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr)))

#End Region

    Public Shared Sub Main()
        Try
            'Kill every other running instance of FalconX
            Try
                For Each prog As Process In Process.GetProcesses
                    If prog.ProcessName = "TaskbarX" Then
                        If Not prog.Id = Process.GetCurrentProcess.Id Then
                            prog.Kill()
                        End If
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
            Settings.AnimationSpeed = 100
            Settings.LoopRefreshRate = 400
            Settings.CenterInBetween = 0
            Settings.FixToolbarsOnTrayChange = 1
            Settings.OnBatteryAnimationStyle = "cubiceaseinout"
            Settings.OnBatteryLoopRefreshRate = 400

            'Read the arguments for the settings
            Dim arguments() As String = Environment.GetCommandLineArgs
            For Each argument In arguments
                Dim val() As String = Split(argument, "=")
                If argument.Contains("-stop") Then
                    TaskbarCenter.RevertToZero()
                    RefreshWindowsExplorer()
                    End
                End If
                If argument.Contains("-taskbarstyle") Then
                    Settings.TaskbarStyle = val(1)
                End If
                If argument.Contains("-primarytaskbaroffset") Then
                    Settings.PrimaryTaskbarOffset = val(1)
                End If
                If argument.Contains("-secondarytaskbaroffset") Then
                    Settings.SecondaryTaskbarOffset = val(1)
                End If
                If argument.Contains("-centerprimaryonly") Then
                    Settings.CenterPrimaryOnly = val(1)
                End If
                If argument.Contains("-centersecondaryonly") Then
                    Settings.CenterSecondaryOnly = val(1)
                End If
                If argument.Contains("-animationstyle") Then
                    Settings.AnimationStyle = val(1)
                End If
                If argument.Contains("-animationspeed") Then
                    Settings.AnimationSpeed = val(1)
                End If
                If argument.Contains("-looprefreshrate") Then
                    Settings.LoopRefreshRate = val(1)
                End If
                If argument.Contains("-centerinbetween") Then
                    Settings.CenterInBetween = val(1)
                End If
                If argument.Contains("-onbatteryanimationstyle") Then
                    Settings.OnBatteryAnimationStyle = val(1)
                End If
                If argument.Contains("-onbatterylooprefreshrate") Then
                    Settings.OnBatteryLoopRefreshRate = val(1)
                End If
                If argument.Contains("-fixtoolbarsontraychange") Then
                    Settings.FixToolbarsOnTrayChange = val(1)
                End If
            Next

            If Settings.AnimationSpeed <= 1 Then
                Settings.AnimationSpeed = 1
            End If

            'Makes the animations run smoother
            Dim currentProcess As Process = Process.GetCurrentProcess
            currentProcess.PriorityClass = ProcessPriorityClass.High

            'Prevent wrong position calculations
            SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware)

            'Wait for Shell_TrayWnd
            Dim Handle As IntPtr
            Do
                Console.WriteLine("Waiting for Shell_TrayWnd")
                Handle = Nothing
                Thread.Sleep(250)
                Handle = FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
            Loop Until Not Handle = Nothing

            'Just empty startup memory before starting
            ClearMemory()

            'Reset the taskbar style...
            ResetTaskbarStyle()

            'Start the TaskbarCenterer
            Dim t1 As Thread = New Thread(AddressOf TaskbarCenter.TaskbarCenterer)
            t1.Start()

            'Start the TaskbarStyler if enabled
            If Settings.TaskbarStyle = 1 Or Settings.TaskbarStyle = 2 Or Settings.TaskbarStyle = 3 Then
                Dim t2 As Thread = New Thread(AddressOf TaskbarStyle.TaskbarStyler)
                t2.Start()
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

#Region "Commands"

    Public Shared Sub ResetTaskbarStyle()
        Dim Progman As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Progman", CType(0, IntPtr)))
        Dim desktops As AutomationElement = AutomationElement.RootElement
        Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
        Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, condition)
        Dim trays As New ArrayList
        For Each trayWnd As AutomationElement In lists
            trays.Add(trayWnd.Current.NativeWindowHandle.ToString)
        Next
        RefreshWindowsExplorer()
        For Each tray As String In trays
            Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
            SendMessage(trayptr, WM_THEMECHANGED, True, 0)
            SendMessage(trayptr, WM_DWMCOLORIZATIONCOLORCHANGED, True, 0)
            SendMessage(trayptr, WM_DWMCOMPOSITIONCHANGED, True, 0)
        Next
    End Sub

    Public Shared Sub RefreshWindowsExplorer()
        Dim CLSID_ShellApplication As Guid = New Guid("13709620-C279-11CE-A49E-444553540000")
        Dim shellApplicationType As Type = Type.GetTypeFromCLSID(CLSID_ShellApplication, True)
        Dim shellApplication As Object = Activator.CreateInstance(shellApplicationType)
        Dim windows As Object = shellApplicationType.InvokeMember("Windows", BindingFlags.InvokeMethod, Nothing, shellApplication, New Object(-1) {})
        Dim windowsType As Type = windows.GetType
        Dim count As Object = windowsType.InvokeMember("Count", BindingFlags.GetProperty, Nothing, windows, Nothing)
        Dim i As Integer = 0
        Do While (i < CType(count, Integer))
            Dim item As Object = windowsType.InvokeMember("Item", BindingFlags.InvokeMethod, Nothing, windows, New Object() {i})
            Dim itemType As Type = item.GetType
            Dim itemName As String = CType(itemType.InvokeMember("Name", BindingFlags.GetProperty, Nothing, item, Nothing), String)
            If (itemName = "Shell_TrayWnd") Then
                itemType.InvokeMember("Refresh", BindingFlags.InvokeMethod, Nothing, item, Nothing)
            End If
            i = (i + 1)
        Loop
    End Sub

    Public Shared Function ClearMemory() As Int32
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()
        Return SetProcessWorkingSetSize(Diagnostics.Process.GetCurrentProcess.Handle, -1, -1)
    End Function

#End Region

End Class