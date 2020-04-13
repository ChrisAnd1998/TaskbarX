Imports System.Runtime.InteropServices
Imports System.Windows.Automation

Public Class TaskbarStyle

#Region "Declarations"

    <DllImport("user32.dll")>
    Private Shared Function SetWindowCompositionAttribute(ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
    End Function

    Public Shared WM_DWMCOLORIZATIONCOLORCHANGED As Integer = &H320
    Public Shared WM_DWMCOMPOSITIONCHANGED As Integer = &H31E
    Public Shared WM_THEMECHANGED As Integer = &H31A

    Friend Structure WindowCompositionAttributeData
        Public Attribute As WindowCompositionAttribute
        Public Data As IntPtr
        Public SizeOfData As Integer
    End Structure

    Friend Enum WindowCompositionAttribute
        WCA_ACCENT_POLICY = 19
    End Enum

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

#End Region

    Public Shared Sub TaskbarStyler()
        Try
            Dim desktops As AutomationElement = AutomationElement.RootElement
            Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
            Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, condition)
            Dim accent = New AccentPolicy()
            Dim accentStructSize = Marshal.SizeOf(accent)

            'Select accent based on settings
            If Settings.TaskbarStyle = 1 Then
                accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARANT
            End If

            If Settings.TaskbarStyle = 3 Then
                accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND
            End If

            If Settings.TaskbarStyle = 2 Then
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
            End If

            'Save accent data
            Dim accentPtr = Marshal.AllocHGlobal(accentStructSize)
            Marshal.StructureToPtr(accent, accentPtr, False)
            Dim data = New WindowCompositionAttributeData With {
                .Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                .SizeOfData = accentStructSize,
                .Data = accentPtr
            }

            Dim trays As New ArrayList

            'Put all TrayWnds into an ArrayList
            For Each trayWnd As AutomationElement In lists
                trays.Add(trayWnd.Current.NativeWindowHandle.ToString)
            Next

            'Set taskbar style for all TrayWnds each 14 millisecond
            Do
                For Each tray As String In trays
                    Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
                    SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)
                Next
                System.Threading.Thread.Sleep(14)
            Loop
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

End Class