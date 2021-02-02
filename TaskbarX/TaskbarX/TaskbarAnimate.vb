Option Strict On

Public Class TaskbarAnimate

    Public Shared current As New ArrayList

    Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal isPrimary As Boolean, ByVal width As Integer)


        Try

            If Math.Abs(CInt((valueToReach - oldpos))) = 0 Then
                'The difference is 0 so there is no need to trigger the animator.
                Exit Sub
            End If


            If Settings.RevertZeroBeyondTray = 1 Then
                'Prevent moving beyond Tray area.
                Dim TrayPos2 As Win32.RECT
                Win32.GetWindowRect(Win32.GetParent(hwnd), TrayPos2)
                Dim rightposition = valueToReach + width

                If orient = "H" Then
                    If rightposition >= TrayPos2.Right - TrayPos2.Left Then
                        Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                        Exit Sub
                    End If
                Else
                    If rightposition >= TrayPos2.Bottom - TrayPos2.Top Then
                        Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                        Exit Sub
                    End If
                End If
            End If


            If valueToReach = oldpos Or Math.Abs(CInt((valueToReach - oldpos))) <= 10 Then
                'Prevent Wiggling (if the new position has a difference of 10 or lower then there is no reason to move)
                Exit Sub
            End If

            For Each tt In current
                If CType(tt, IntPtr) = hwnd Then
                    'If hwnd is already getting animated then hwnd is in this arraylist and exit the animator because it's uneeded.
                    Exit Sub
                End If
            Next

            ' Console.WriteLine(CInt((valueToReach - oldpos).ToString.Replace("-", "")))

            current.Add(hwnd)

            Dim sw As New Stopwatch
            Dim originalValue As Integer = oldpos
            Dim elapsed As New Integer
            Dim minValue As Integer

            If originalValue <= valueToReach Then
                minValue = originalValue
            Else
                minValue = valueToReach
            End If

            Dim maxValue As Integer = Math.Abs(CInt((valueToReach - originalValue)))
            Dim increasing As Boolean = originalValue < valueToReach

            elapsed = 0
            sw.Start()

            If isPrimary = True Then
                TaskbarCenter.isanimating = True
            End If

            While Not elapsed >= duration

                elapsed = CInt(sw.ElapsedMilliseconds)

                Dim newValue As Integer = CInt((easing(elapsed, minValue, maxValue, duration)))

                If Not increasing Then
                    newValue = (originalValue + valueToReach) - newValue
                End If

                If orient = "H" Then
                    Win32.SetWindowPos(hwnd, IntPtr.Zero, newValue, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                Else
                    Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, newValue, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                End If
            End While

            If isPrimary = True Then
                TaskbarCenter.isanimating = False
            End If

            sw.Stop()
            current.Remove(hwnd)

            Main.ClearMemory()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

End Class