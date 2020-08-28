Option Strict On

Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

Public Class TaskbarStyle

    Public Delegate Function CallBack(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean

    Public Declare Function EnumWindows Lib "user32" (ByVal Adress As CallBack, ByVal y As Integer) As Integer
    Public Shared ActiveWindows As New System.Collections.ObjectModel.Collection(Of IntPtr)

    Public Shared Function GetActiveWindows() As ObjectModel.Collection(Of IntPtr)
        windowHandles.Clear()
        EnumWindows(AddressOf Enumerator, 0)
        Return ActiveWindows
    End Function

    Public Shared windowHandles As ArrayList = New ArrayList()
    Public Shared windowHandles2 As ArrayList = New ArrayList()
    Public Shared trays As ArrayList = New ArrayList()
    Public Shared traysbackup As ArrayList = New ArrayList()
    Public Shared normalwindows As ArrayList = New ArrayList()
    Public Shared resetted As ArrayList = New ArrayList()

    Public Shared Function Enumerator(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Dim sClassName As New StringBuilder("", 256)
        Call Win32.GetClassName(hwnd, sClassName, 256)
        If sClassName.ToString = "Shell_TrayWnd" Or sClassName.ToString = "Shell_SecondaryTrayWnd" Then
            windowHandles.Add(hwnd)
        End If
        Return True
    End Function

    Public Shared Function Enumerator2(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Try

            Dim intRet As Integer
            Dim wpTemp As Win32.WINDOWPLACEMENT

            wpTemp.Length = System.Runtime.InteropServices.Marshal.SizeOf(wpTemp)
            intRet = CInt(Win32.GetWindowPlacement(hwnd, wpTemp))
            Dim style As Integer = Win32.GetWindowLong(hwnd, Win32.GWL_STYLE)

            If (style And Win32.WS_VISIBLE) = Win32.WS_VISIBLE Then
                If wpTemp.showCmd = 1 Then
                    normalwindows.Remove(hwnd)
                    normalwindows.Add(hwnd)
                ElseIf wpTemp.showCmd = 2 Then
                    normalwindows.Remove(hwnd)
                    normalwindows.Add(hwnd)
                ElseIf wpTemp.showCmd = 3 Then
                    windowHandles2.Remove(hwnd)
                    windowHandles2.Add(hwnd)
                End If
            End If
        Catch ex As Exception
        End Try
        Return True
    End Function

    Public Shared Sub Tbsm()
        Do

            Dim windowsold As Integer
            Dim windowsnew As Integer
            windowsold = windowHandles2.Count

            windowHandles2.Clear()
            System.Threading.Thread.Sleep(250)
            EnumWindows(AddressOf Enumerator2, 0)

            windowsnew = windowHandles2.Count

            If Not windowsnew = windowsold Then
                For Each tray As IntPtr In traysbackup
                    For Each normalwindow As IntPtr In normalwindows
                        Dim curmonx As Screen = Screen.FromHandle(normalwindow)
                        Dim curmontbx As Screen = Screen.FromHandle(tray)
                        If curmonx.DeviceName = curmontbx.DeviceName Then
                            trays.Remove(tray)
                            trays.Add(tray)
                        End If
                    Next
                Next

                For Each tray As IntPtr In traysbackup
                    For Each maxedwindow As IntPtr In windowHandles2
                        Dim curmonx As Screen = Screen.FromHandle(maxedwindow)
                        Dim curmontbx As Screen = Screen.FromHandle(tray)
                        If curmonx.DeviceName = curmontbx.DeviceName Then
                            trays.Remove(tray)
                            Win32.PostMessage(tray, &H31E, CType(&H1, IntPtr), CType(&H0, IntPtr))
                        End If
                    Next
                Next
            End If

        Loop
    End Sub

    Public Shared Sub TaskbarStyler()
        Try

            GetActiveWindows()

            Dim accent = New Win32.AccentPolicy()
            Dim accentStructSize = Marshal.SizeOf(accent)

            'Select accent based on settings
            If Settings.TaskbarStyle = 1 Then
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_TRANSPARANT
            End If

            If Settings.TaskbarStyle = 2 Then
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_BLURBEHIND
            End If

            If Settings.TaskbarStyle = 3 Then
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND
            End If

            If Settings.TaskbarStyle = 4 Then
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT
            End If

            If Settings.TaskbarStyle = 5 Then
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_GRADIENT
            End If

            accent.AccentFlags = 2 'enable colorize
            accent.GradientColor = BitConverter.ToInt32(New Byte() {CByte(Settings.TaskbarStyleRed), CByte(Settings.TaskbarStyleGreen), CByte(Settings.TaskbarStyleBlue), CByte(Settings.TaskbarStyleAlpha * 2.55)}, 0)

            'Save accent data
            Dim accentPtr = Marshal.AllocHGlobal(accentStructSize)
            Marshal.StructureToPtr(accent, accentPtr, False)

            Dim data = New Win32.WindowCompositionAttributeData
            data.Attribute = Win32.WindowCompositionAttribute.WCA_ACCENT_POLICY
            data.SizeOfData = accentStructSize
            data.Data = accentPtr

            'Put all TrayWnds into an ArrayList
            For Each trayWnd As IntPtr In windowHandles
                trays.Add(trayWnd)
                traysbackup.Add(trayWnd)
            Next

            If Settings.DefaultTaskbarStyleOnWinMax = 1 Then
                Dim t2 As Thread = New Thread(AddressOf Tbsm)
                t2.Start()
            End If

            'Set taskbar style for all TrayWnds each 14 millisecond
            For Each tray As IntPtr In trays
                Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
                Win32.SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)
            Next

            Do
                Try

                    For Each tray As IntPtr In trays
                        Win32.SetWindowCompositionAttribute(tray, data)

                    Next
                    System.Threading.Thread.Sleep(10)
                Catch
                End Try
            Loop
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Public Shared childLeft As Integer
    Public Shared childTop As Integer
    Public Shared childWidth As Integer
    Public Shared childHeight As Integer

    Public Shared Function GetLocation(ByVal acc As Accessibility.IAccessible, ByVal idChild As Integer) As Integer
        acc.accLocation(childLeft, childTop, childWidth, childHeight, idChild)
        Return Nothing
    End Function

End Class