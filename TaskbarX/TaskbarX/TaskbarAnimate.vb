
Imports System.Threading


Public Class TaskbarAnimate
        Public Shared IsAnimated As Boolean = True
        Public Shared AnimatedTaskbars As Integer

        Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer)
            Try

                If Not oldpos <= 100 Then

                    If valueToReach = oldpos Or CInt((valueToReach - oldpos).ToString.Replace("-", "")) <= 10 Then
                        'Prevent Wiggling (if the new position has a difference of 10 or lower then there is no reason to move)
                        Exit Sub
                    End If

                    ' If AnimatedTaskbars >= (TaskbarCenter.TaskbarCount) Then
                    If valueToReach = oldpos Or CInt((valueToReach - oldpos).ToString.Replace("-", "")) >= 300 Then
                        'Prevent Big Swing (if the New position has a difference of 300 Or higher then there Is no reason to move)
                        Exit Sub
                        ' End If
                    End If

                End If

                IsAnimated = False

            Dim sw As New Stopwatch
            Dim originalValue As Integer = oldpos
            Dim elapsed As New Integer
            Dim minValue As Integer = Math.Min(originalValue, valueToReach)
            Dim maxValue As Integer = Math.Abs(valueToReach - originalValue)
            Dim increasing As Boolean = originalValue < valueToReach

            'Dim sleepvalue = Settings.AnimationSpeed / 144

            elapsed = 0
            sw.Start()

            While Not elapsed >= duration

                ' Thread.Sleep(3)
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



            ' AnimatedTaskbars = AnimatedTaskbars + 1
            sw.Stop()
            IsAnimated = True
                Main.ClearMemory()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Sub

    End Class
