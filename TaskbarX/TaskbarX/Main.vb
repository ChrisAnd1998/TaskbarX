Option Strict On

Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Reflection
Imports System.Text

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
                    ResetTaskbarStyle()
                    End
                End If
                If argument.Contains("-tbs=") Then
                    Settings.TaskbarStyle = CInt(val(1))
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
                Dim Shell_TrayWnd = Win32.FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))
                Dim TrayNotifyWnd = Win32.FindWindowEx(Shell_TrayWnd, CType(0, IntPtr), "TrayNotifyWnd", Nothing)
                Dim ReBarWindow32 = Win32.FindWindowEx(Shell_TrayWnd, CType(0, IntPtr), "ReBarWindow32", Nothing)
                Dim MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, CType(0, IntPtr), "MSTaskSwWClass", Nothing)
                Dim MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, CType(0, IntPtr), "MSTaskListWClass", Nothing)
                Handle = MSTaskListWClass
                Console.WriteLine("Current Handle = " & Handle.ToString)
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

    Public Delegate Function CallBack(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean

    Public Declare Function EnumWindows Lib "user32" (ByVal Adress As CallBack, ByVal y As Integer) As Integer
    Public Shared ActiveWindows As New System.Collections.ObjectModel.Collection(Of IntPtr)

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function GetClassName(ByVal hWnd As System.IntPtr, ByVal lpClassName As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    Public Shared Function GetActiveWindows() As ObjectModel.Collection(Of IntPtr)
        windowHandles.Clear()
        EnumWindows(AddressOf Enumerator, 0)
        Return ActiveWindows
    End Function

    Public Shared Function Enumerator(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Dim sClassName As New StringBuilder("", 256)
        Call GetClassName(hwnd, sClassName, 256)
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
        RefreshWindowsExplorer()
        For Each tray As IntPtr In trays
            Dim trayptr As IntPtr = tray
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
            Dim itemNameInfo As PropertyInfo = itemType.GetProperty("Name")
            If (itemNameInfo <> Nothing) Then
                Dim itemName As String = CType(itemType.InvokeMember("Name", BindingFlags.GetProperty, Nothing, item, Nothing), String)
                If (itemName = "Shell_TrayWnd") Then
                    itemType.InvokeMember("Refresh", BindingFlags.InvokeMethod, Nothing, item, Nothing)
                End If
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