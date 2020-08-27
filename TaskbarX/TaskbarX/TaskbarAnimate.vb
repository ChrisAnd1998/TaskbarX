Public Class TaskbarAnimate

    Public Shared current As New ArrayList

    Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer)
        Try

            If valueToReach = oldpos Or CInt((valueToReach - oldpos).ToString.Replace("-", "")) <= 10 Then
                'Prevent Wiggling (if the new position has a difference of 10 or lower then there is no reason to move)
                Exit Sub
            End If

            For Each tt In current
                If CType(tt, IntPtr) = hwnd Then
                    'If hwnd is already getting animated then hwnd is in this arraylist and exit the animator because it's uneeded.
                    Exit Sub
                End If
            Next

            current.Add(hwnd)

            Dim sw As New Stopwatch
            Dim originalValue As Integer = oldpos
            Dim elapsed As New Integer
            Dim minValue As Integer = Math.Min(originalValue, valueToReach)
            Dim maxValue As Integer = Math.Abs(valueToReach - originalValue)
            Dim increasing As Boolean = originalValue < valueToReach

            elapsed = 0
            sw.Start()

            While Not elapsed >= duration
                elapsed = CInt(sw.ElapsedMilliseconds)

                Dim newValue As Integer = CInt(Math.Truncate(easing(elapsed, minValue, maxValue, duration)))

                If Not increasing Then
                    newValue = (originalValue + valueToReach) - newValue
                End If

                If orient = "H" Then
                    Win32.SetWindowPos(hwnd, IntPtr.Zero, newValue, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                Else
                    Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, newValue, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_ASYNCWINDOWPOS Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
                End If
            End While

            sw.Stop()
            current.Remove(hwnd)

            Main.ClearMemory()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

End Class