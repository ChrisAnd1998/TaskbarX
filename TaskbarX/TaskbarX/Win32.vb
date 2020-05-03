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

    Public Delegate Function CallBack(ByVal hwnd As Integer, ByVal lParam As Integer) As Boolean

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

    Public Shared WM_USER = 1024
    Public Shared TB_ENABLEBUTTON = (WM_USER + 1)
    Public Shared TB_CHECKBUTTON = (WM_USER + 2)
    Public Shared TB_PRESSBUTTON = (WM_USER + 3)
    Public Shared TB_HIDEBUTTON = (WM_USER + 4)
    Public Shared TB_INDETERMINATE = (WM_USER + 5)
    Public Shared TB_MARKBUTTON = (WM_USER + 6)
    Public Shared TB_ISBUTTONENABLED = (WM_USER + 9)
    Public Shared TB_ISBUTTONCHECKED = (WM_USER + 10)
    Public Shared TB_ISBUTTONPRESSED = (WM_USER + 11)
    Public Shared TB_ISBUTTONHIDDEN = (WM_USER + 12)
    Public Shared TB_ISBUTTONINDETERMINATE = (WM_USER + 13)
    Public Shared TB_ISBUTTONHIGHLIGHTED = (WM_USER + 14)
    Public Shared TB_SETSTATE = (WM_USER + 17)
    Public Shared TB_GETSTATE = (WM_USER + 18)
    Public Shared TB_ADDBITMAP = (WM_USER + 19)
    Public Shared TB_ADDBUTTONSA = (WM_USER + 20)
    Public Shared TB_INSERTBUTTONA = (WM_USER + 21)
    Public Shared TB_ADDBUTTONS = (WM_USER + 20)
    Public Shared TB_INSERTBUTTON = (WM_USER + 21)
    Public Shared TB_DELETEBUTTON = (WM_USER + 22)
    Public Shared TB_GETBUTTON = (WM_USER + 23)
    Public Shared TB_BUTTONCOUNT = (WM_USER + 24)
    Public Shared TB_COMMANDTOINDEX = (WM_USER + 25)
    Public Shared TB_SAVERESTOREA = (WM_USER + 26)
    Public Shared TB_CUSTOMIZE = (WM_USER + 27)
    Public Shared TB_ADDSTRINGA = (WM_USER + 28)
    Public Shared TB_GETITEMRECT = (WM_USER + 29)
    Public Shared TB_BUTTONSTRUCTSIZE = (WM_USER + 30)
    Public Shared TB_SETBUTTONSIZE = (WM_USER + 31)
    Public Shared TB_SETBITMAPSIZE = (WM_USER + 32)
    Public Shared TB_AUTOSIZE = (WM_USER + 33)
    Public Shared TB_GETTOOLTIPS = (WM_USER + 35)
    Public Shared TB_SETTOOLTIPS = (WM_USER + 36)
    Public Shared TB_SETPARENT = (WM_USER + 37)
    Public Shared TB_SETROWS = (WM_USER + 39)
    Public Shared TB_GETROWS = (WM_USER + 40)
    Public Shared TB_GETBITMAPFLAGS = (WM_USER + 41)
    Public Shared TB_SETCMDID = (WM_USER + 42)
    Public Shared TB_CHANGEBITMAP = (WM_USER + 43)
    Public Shared TB_GETBITMAP = (WM_USER + 44)
    Public Shared TB_GETBUTTONTEXTA = (WM_USER + 45)
    Public Shared TB_GETBUTTONTEXTW = (WM_USER + 75)
    Public Shared TB_REPLACEBITMAP = (WM_USER + 46)
    Public Shared TB_SETINDENT = (WM_USER + 47)
    Public Shared TB_SETIMAGELIST = (WM_USER + 48)
    Public Shared TB_GETIMAGELIST = (WM_USER + 49)
    Public Shared TB_LOADIMAGES = (WM_USER + 50)
    Public Shared TB_GETRECT = (WM_USER + 51)
    Public Shared TB_SETHOTIMAGELIST = (WM_USER + 52)
    Public Shared TB_GETHOTIMAGELIST = (WM_USER + 53)
    Public Shared TB_SETDISABLEDIMAGELIST = (WM_USER + 54)
    Public Shared TB_GETDISABLEDIMAGELIST = (WM_USER + 55)
    Public Shared TB_SETSTYLE = (WM_USER + 56)
    Public Shared TB_GETSTYLE = (WM_USER + 57)
    Public Shared TB_GETBUTTONSIZE = (WM_USER + 58)
    Public Shared TB_SETBUTTONWIDTH = (WM_USER + 59)
    Public Shared TB_SETMAXTEXTROWS = (WM_USER + 60)
    Public Shared TB_GETTEXTROWS = (WM_USER + 61)
    Public Shared TB_GETOBJECT = (WM_USER + 62)
    Public Shared TB_GETBUTTONINFOW = (WM_USER + 63)
    Public Shared TB_SETBUTTONINFOW = (WM_USER + 64)
    Public Shared TB_GETBUTTONINFOA = (WM_USER + 65)
    Public Shared TB_SETBUTTONINFOA = (WM_USER + 66)
    Public Shared TB_INSERTBUTTONW = (WM_USER + 67)
    Public Shared TB_ADDBUTTONSW = (WM_USER + 68)
    Public Shared TB_HITTEST = (WM_USER + 69)
    Public Shared TB_SETDRAWTEXTFLAGS = (WM_USER + 70)
    Public Shared TB_GETHOTITEM = (WM_USER + 71)
    Public Shared TB_SETHOTITEM = (WM_USER + 72)
    Public Shared TB_SETANCHORHIGHLIGHT = (WM_USER + 73)
    Public Shared TB_GETANCHORHIGHLIGHT = (WM_USER + 74)
    Public Shared TB_SAVERESTOREW = (WM_USER + 76)
    Public Shared TB_ADDSTRINGW = (WM_USER + 77)
    Public Shared TB_MAPACCELERATORA = (WM_USER + 78)
    Public Shared TB_GETINSERTMARK = (WM_USER + 79)
    Public Shared TB_SETINSERTMARK = (WM_USER + 80)
    Public Shared TB_INSERTMARKHITTEST = (WM_USER + 81)
    Public Shared TB_MOVEBUTTON = (WM_USER + 82)
    Public Shared TB_GETMAXSIZE = (WM_USER + 83)
    Public Shared TB_SETEXTENDEDSTYLE = (WM_USER + 84)
    Public Shared TB_GETEXTENDEDSTYLE = (WM_USER + 85)
    Public Shared TB_GETPADDING = (WM_USER + 86)
    Public Shared TB_SETPADDING = (WM_USER + 87)
    Public Shared TB_SETINSERTMARKCOLOR = (WM_USER + 88)
    Public Shared TB_GETINSERTMARKCOLOR = (WM_USER + 89)

End Class