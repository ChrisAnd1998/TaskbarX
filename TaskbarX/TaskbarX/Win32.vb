Option Strict On

Imports System.Runtime.InteropServices

Public Class Win32

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function SetParent(ByVal hWndChild As IntPtr, ByVal hWndNewParent As IntPtr) As IntPtr
    End Function

    Public Shared Function MakeLParam(ByVal LoWord As Integer, ByVal HiWord As Integer) As IntPtr
        Return New IntPtr((HiWord << 16) Or (LoWord And &HFFFF))
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
    End Function

    Public Delegate Function CallBack(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean

    Public Declare Function EnumWindows Lib "user32" (ByVal Adress As CallBack, ByVal y As Integer) As Integer
    Public Shared ActiveWindows As New System.Collections.ObjectModel.Collection(Of IntPtr)

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Public Shared Function GetClassName(ByVal hWnd As System.IntPtr, ByVal lpClassName As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    <DllImport("User32.dll")>
    Public Shared Function EnumChildWindows _
     (ByVal WindowHandle As IntPtr, ByVal Callback As EnumWindowProcess,
     ByVal lParam As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SetWindowCompositionAttribute(ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function FindWindowEx(ByVal parentHandle As IntPtr, ByVal childAfter As IntPtr, ByVal lclassName As String, ByVal windowTitle As String) As IntPtr
    End Function

    <DllImport("user32.dll", ExactSpelling:=True, CharSet:=CharSet.Auto)>
    Public Shared Function GetParent(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
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

    <Flags()>
    Public Enum RedrawWindowFlags As UInteger

        ''' <summary>
        ''' Invalidates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
        ''' You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_INVALIDATE invalidates the entire window.
        ''' </summary>
        Invalidate = &H1

        ''' <summary>Causes the OS to post a WM_PAINT message to the window regardless of whether a portion of the window is invalid.</summary>
        InternalPaint = &H2

        ''' <summary>
        ''' Causes the window to receive a WM_ERASEBKGND message when the window is repainted.
        ''' Specify this value in combination with the RDW_INVALIDATE value; otherwise, RDW_ERASE has no effect.
        ''' </summary>
        [Erase] = &H4

        ''' <summary>
        ''' Validates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
        ''' You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_VALIDATE validates the entire window.
        ''' This value does not affect internal WM_PAINT messages.
        ''' </summary>
        Validate = &H8

        NoInternalPaint = &H10

        ''' <summary>Suppresses any pending WM_ERASEBKGND messages.</summary>
        NoErase = &H20

        ''' <summary>Excludes child windows, if any, from the repainting operation.</summary>
        NoChildren = &H40

        ''' <summary>Includes child windows, if any, in the repainting operation.</summary>
        AllChildren = &H80

        ''' <summary>Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND and WM_PAINT messages before the RedrawWindow returns, if necessary.</summary>
        UpdateNow = &H100

        ''' <summary>
        ''' Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND messages before RedrawWindow returns, if necessary.
        ''' The affected windows receive WM_PAINT messages at the ordinary time.
        ''' </summary>
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

    Public Shared WM_DWMCOLORIZATIONCOLORCHANGED As Integer = &H320
    Public Shared WM_DWMCOMPOSITIONCHANGED As Integer = &H31E
    Public Shared WM_THEMECHANGED As Integer = &H31A

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

        '  WS_POPUPWINDOW = WS_POPUP Or WS_BORDER Or WS_SYSMENU
    End Enum

    Public Delegate Function EnumWindowProcess(ByVal Handle As IntPtr, ByVal Parameter As IntPtr) As Boolean

    Public Shared SWP_NOSIZE As UInt32 = 1
    Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
    Public Shared SWP_NOACTIVATE As UInt32 = 16
    Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
    Public Shared SWP_NOZORDER As UInt32 = 4



End Class