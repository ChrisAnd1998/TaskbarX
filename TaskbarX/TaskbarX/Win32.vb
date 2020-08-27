Option Strict On

Imports System.Runtime.InteropServices

Public Class Win32

    <DllImport("user32.dll")>
    Public Shared Function ShowWindow(hWnd As IntPtr, <MarshalAs(UnmanagedType.I4)> nCmdShow As ShowWindowCommands) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function SetParent(ByVal hWndChild As IntPtr, ByVal hWndNewParent As IntPtr) As IntPtr
    End Function

    Public Shared Function MakeLParam(ByVal LoWord As Integer, ByVal HiWord As Integer) As IntPtr
        Return New IntPtr((HiWord << 16) Or (LoWord And &HFFFF))
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Public Shared Function GetClassName(ByVal hWnd As System.IntPtr, ByVal lpClassName As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetWindowPlacement(ByVal hWnd As IntPtr, ByRef lpwndpl As WINDOWPLACEMENT) As Boolean

    End Function

    <DllImport("User32.dll")>
    Public Shared Function EnumChildWindows _
     (ByVal WindowHandle As IntPtr, ByVal Callback As EnumWindowProcess,
     ByVal lParam As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SetWindowCompositionAttribute(ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function FindWindowEx(ByVal parentHandle As IntPtr, ByVal childAfter As IntPtr, ByVal lclassName As String, ByVal windowTitle As String) As IntPtr
    End Function

    <DllImport("user32.dll", ExactSpelling:=True, CharSet:=CharSet.Auto)>
    Public Shared Function GetParent(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function MonitorFromWindow(ByVal hwnd As IntPtr, ByVal dwFlags As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetMonitorInfo(ByVal hMonitor As IntPtr, ByRef lpmi As MONITORINFO) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function GetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As Integer) As Integer

    End Function

    <DllImport("user32.dll")>
    Public Shared Function UpdateWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function RedrawWindow(hWnd As IntPtr, lprcUpdate As IntPtr, hrgnUpdate As IntPtr, flags As RedrawWindowFlags) As Boolean
    End Function

    <DllImport("SHCore.dll", SetLastError:=True)>
    Public Shared Function SetProcessDpiAwareness(ByVal awareness As PROCESS_DPI_AWARENESS) As Boolean
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=Runtime.InteropServices.CharSet.Auto)>
    Public Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Public Shared Function SetProcessWorkingSetSize(ByVal hProcess As IntPtr, ByVal dwMinimumWorkingSetSize As Int32, ByVal dwMaximumWorkingSetSize As Int32) As Int32
    End Function

    <System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint:="SetWindowLong")>
    Public Shared Function SetWindowLong(ByVal hWnd As IntPtr, <MarshalAs(UnmanagedType.I4)> nIndex As WindowStyles, ByVal dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SetLayeredWindowAttributes(ByVal hwnd As IntPtr, ByVal crKey As UInteger, ByVal bAlpha As Byte, ByVal dwFlags As UInteger) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function PostMessage(ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=False)>
    Public Shared Function GetDesktopWindow() As IntPtr
    End Function

    Public Structure POINTAPI
        Public x As Integer
        Public y As Integer
    End Structure

    Public Shared WS_BORDER As Integer = 8388608
    Public Shared WS_DLGFRAME As Integer = 4194304
    Public Shared WS_CAPTION As Integer = WS_BORDER Or WS_DLGFRAME
    Public Shared WS_VISIBLE As Integer = 268435456

    Public Structure WINDOWPLACEMENT
        Public Length As Integer
        Public flags As Integer
        Public showCmd As Integer
        Public ptMinPosition As POINTAPI
        Public ptMaxPosition As POINTAPI
        Public rcNormalPosition As RECT
    End Structure

    Public Enum PROCESS_DPI_AWARENESS
        Process_DPI_Unaware = 0
        Process_System_DPI_Aware = 1
        Process_Per_Monitor_DPI_Aware = 2
    End Enum

    Public Shared WM_DWMCOLORIZATIONCOLORCHANGED As Integer = &H320
    Public Shared WM_DWMCOMPOSITIONCHANGED As Integer = &H31E
    Public Shared WM_THEMECHANGED As Integer = &H31A

    Public Const WM_SETREDRAW As Integer = 11

    <Flags()>
    Public Enum RedrawWindowFlags As UInteger
        Invalidate = &H1
        InternalPaint = &H2
        [Erase] = &H4
        Validate = &H8
        NoInternalPaint = &H10
        NoErase = &H20
        NoChildren = &H40
        AllChildren = &H80
        UpdateNow = &H100
        EraseNow = &H200
        Frame = &H400
        NoFrame = &H800
    End Enum

    <System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint:="SetWinEventHook")>
    Private Shared Function SetWinEventHook(
        ByVal eventMin As UInteger,
        ByVal eventMax As UInteger,
        ByVal hmodWinEventProc As IntPtr,
        ByVal lpfnWinEventProc As WinEventDelegate,
        ByVal idProcess As UInteger,
        ByVal idThread As UInteger,
        ByVal dwFlags As UInteger
    ) As IntPtr
    End Function

    Delegate Sub WinEventDelegate(
        ByVal hWinEventHook As IntPtr,
        ByVal eventType As UInteger,
        ByVal hwnd As IntPtr,
        ByVal idObject As Integer,
        ByVal idChild As Integer,
        ByVal dwEventThread As UInteger,
        ByVal dwmsEventTime As UInteger
    )

    Public Structure WindowCompositionAttributeData
        Public Attribute As WindowCompositionAttribute
        Public Data As IntPtr
        Public SizeOfData As Integer
    End Structure

    Public Enum WindowCompositionAttribute
        WCA_ACCENT_POLICY = 19
    End Enum

    Public Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    Public Structure MONITORINFO
        Public cbSize As Long
        Public rcMonitor As RECT
        Public rcWork As RECT
        Public dwFlags As Long
    End Structure

    Friend Enum AccentState
        ACCENT_DISABLED = 0
        ACCENT_ENABLE_GRADIENT = 1
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2
        ACCENT_ENABLE_BLURBEHIND = 3
        ACCENT_ENABLE_TRANSPARANT = 6
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        ACCENT_NORMAL = 150
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure AccentPolicy
        Public AccentState As AccentState
        Public AccentFlags As Integer
        Public GradientColor As Integer
        Public AnimationId As Integer
    End Structure

    Public Const GWL_STYLE = -16
    Public Const GWL_EXSTYLE = -20
    Public Const WS_MAXIMIZE = 16777216
    Public Const WS_POPUP = 2147483648
    Public Const WS_EX_LAYERED As Integer = 524288

    Public Const LWA_ALPHA As Integer = 2

    Public Const LWA_COLORKEY As Integer = 1

    Public Enum WindowStyles
        WS_BORDER = &H800000
        WS_CAPTION = &HC00000
        WS_CHILD = &H40000000
        WS_CLIPCHILDREN = &H2000000
        WS_CLIPSIBLINGS = &H4000000
        WS_DISABLED = &H8000000
        WS_DLGFRAME = &H400000
        WS_GROUP = &H20000
        WS_HSCROLL = &H100000
        WS_MAXIMIZE = &H1000000
        WS_MAXIMIZEBOX = &H10000
        WS_MINIMIZE = &H20000000
        WS_MINIMIZEBOX = &H20000
        WS_OVERLAPPED = &H0
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED Or WS_CAPTION Or WS_SYSMENU Or WS_SIZEFRAME Or WS_MINIMIZEBOX Or WS_MAXIMIZEBOX
        WS_SIZEFRAME = &H40000
        WS_SYSMENU = &H80000
        WS_TABSTOP = &H10000
        WS_VISIBLE = &H10000000
        WS_VSCROLL = &H200000
    End Enum

    Enum ShowWindowCommands As Integer
        Hide = 0
        Normal = 1
        ShowMinimized = 2
        Maximize = 3
        ShowMaximized = 3
        ShowNoActivate = 4
        Show = 5
        Minimize = 6
        ShowMinNoActive = 7
        ShowNA = 8
        Restore = 9
        ShowDefault = 10
        ForceMinimize = 11
    End Enum

    Public Delegate Function EnumWindowProcess(ByVal Handle As IntPtr, ByVal Parameter As IntPtr) As Boolean

    Public Shared SWP_NOSIZE As UInt32 = 1
    Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
    Public Shared SWP_NOACTIVATE As UInt32 = 16
    Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
    Public Shared SWP_NOZORDER As UInt32 = 4
    Public Shared WM_COMMAND As Long = &H111
    Public Shared HWND_BROADCAST As IntPtr = New IntPtr(65535)
    Public Shared WM_SETTINGCHANGE As UInteger = 26
    Public Shared SMTO_ABORTIFHUNG As Integer = 2

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function SendNotifyMessage(
     ByVal hWnd As IntPtr,
     ByVal msg As UInteger,
     ByVal wParam As UIntPtr,
     ByVal lParam As String
     ) As Boolean
    End Function

    <DllImport("shell32.dll")>
    Public Shared Sub SHChangeNotify(
    ByVal wEventID As HChangeNotifyEventID,
    ByVal uFlags As HChangeNotifyFlags,
    ByVal dwItem1 As IntPtr,
    ByVal dwItem2 As IntPtr)
    End Sub

    Public Enum HChangeNotifyEventID
        SHCNE_ALLEVENTS = &H7FFFFFFF
        SHCNE_ASSOCCHANGED = &H8000000
        SHCNE_ATTRIBUTES = &H800
        SHCNE_CREATE = &H2
        SHCNE_DELETE = &H4
        SHCNE_DRIVEADD = &H100
        SHCNE_DRIVEADDGUI = &H10000
        SHCNE_DRIVEREMOVED = &H80
        SHCNE_EXTENDED_EVENT = &H4000000
        SHCNE_FREESPACE = &H40000
        SHCNE_MEDIAINSERTED = &H20
        SHCNE_MEDIAREMOVED = &H40
        SHCNE_MKDIR = &H8
        SHCNE_NETSHARE = &H200
        SHCNE_NETUNSHARE = &H400
        SHCNE_RENAMEFOLDER = &H20000
        SHCNE_RENAMEITEM = &H1
        SHCNE_RMDIR = &H10
        SHCNE_SERVERDISCONNECT = &H4000
        SHCNE_UPDATEDIR = &H1000
        SHCNE_UPDATEIMAGE = &H8000
    End Enum

    Public Enum HChangeNotifyFlags
        SHCNF_DWORD = &H3
        SHCNF_IDLIST = &H0
        SHCNF_PATHA = &H1
        SHCNF_PATHW = &H5
        SHCNF_PRINTERA = &H2
        SHCNF_PRINTERW = &H6
        SHCNF_FLUSH = &H1000
        SHCNF_FLUSHNOWAIT = &H2000
    End Enum

End Class