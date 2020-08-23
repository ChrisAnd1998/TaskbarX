Option Strict On

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text


Public Class TaskbarStyle

    Public Delegate Function CallBack(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean

    Public Declare Function EnumWindows Lib "user32" (ByVal Adress As CallBack, ByVal y As Integer) As Integer
    Public Shared ActiveWindows As New System.Collections.ObjectModel.Collection(Of IntPtr)

    Public Shared Function GetActiveWindows() As ObjectModel.Collection(Of IntPtr)
        windowHandles.Clear()
        EnumWindows(AddressOf Enumerator, 0)
        Return ActiveWindows
    End Function

    Public Shared Function Enumerator2(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Try

            Dim style As Integer = Win32.GetWindowLong(hwnd, Win32.GWL_STYLE)

            If (style And Win32.WS_MAXIMIZE) = Win32.WS_MAXIMIZE Then
                If Not (style And Win32.WS_POPUP) = Win32.WS_POPUP Then
                    windowHandles2.Add(hwnd)
                End If
            End If
        Catch ex As Exception

        End Try
        Return True
    End Function

    Public Shared Function Enumerator(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Dim sClassName As New StringBuilder("", 256)
        Call Win32.GetClassName(hwnd, sClassName, 256)
        If sClassName.ToString = "Shell_TrayWnd" Or sClassName.ToString = "Shell_SecondaryTrayWnd" Then
            windowHandles.Add(hwnd)
        End If
        Return True
    End Function

    Public Shared Sub Ttt()
        Do
            windowHandles2.Clear()

            EnumWindows(AddressOf Enumerator2, 0)
            System.Threading.Thread.Sleep(100)
        Loop
    End Sub

    Public Shared windowHandles As ArrayList = New ArrayList()
    Public Shared windowHandles2 As ArrayList = New ArrayList()

    Public Shared Sub TaskbarStyler()
        Try

            GetActiveWindows()

            If Settings.DefaultTaskbarStyleOnWinMax = 1 Then
                '  Dim t2 As Thread = New Thread(AddressOf Ttt)
                '   t2.Start()
            End If

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
                accent.GradientColor = 0
            End If

            'Save accent data
            Dim accentPtr = Marshal.AllocHGlobal(accentStructSize)
            Marshal.StructureToPtr(accent, accentPtr, False)


            Dim data = New Win32.WindowCompositionAttributeData
            data.Attribute = Win32.WindowCompositionAttribute.WCA_ACCENT_POLICY
            data.SizeOfData = accentStructSize
            data.Data = accentPtr


            Dim trays As New ArrayList

            'Put all TrayWnds into an ArrayList
            For Each trayWnd As IntPtr In windowHandles
                'Console.WriteLine(trayWnd)
                trays.Add(trayWnd)
            Next

            'Set taskbar style for all TrayWnds each 14 millisecond
            For Each tray As IntPtr In trays
                Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
                Win32.SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)
            Next



            Do

                Try

                    For Each tray As IntPtr In trays
                        Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
                        Win32.SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)

                        'Win32.SetWindowCompositionTransition(CType(trayptr, IntPtr), data)


                        ' If Settings.DefaultTaskbarStyleOnWinMax = 1 Then
                        ' im cc As Integer = 0
                        ' Dim oldcc As Integer
                        ' cc = windowHandles2.Count
                        ' If windowHandles2.Count = 0 Then
                        ' Win32.SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)
                        ' Else
                        ' For Each activewindow In windowHandles2
                        ' Console.WriteLine(activewindow)
                        ' Dim curmonx As Screen = Screen.FromHandle(activewindow)
                        ' Dim curmontbx As Screen = Screen.FromHandle(tray)
                        ' If cc <= oldcc Then
                        ' If Not cc = oldcc Then
                        ' If Not curmontbx.DeviceName = curmonx.DeviceName Then
                        ' Win32.SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)
                        ' End If
                        ' End If
                        ' End If
                        ' If curmontbx.DeviceName = curmonx.DeviceName Then
                        ' Win32.SendMessage(trayptr, Win32.WM_DWMCOMPOSITIONCHANGED, True, 0)
                        ' End If
                        ' Next
                        ' End If
                        ' oldcc = cc
                        ' Else
                        ' Win32.SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)
                        ' End If

                    Next

                    System.Threading.Thread.Sleep(5)
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