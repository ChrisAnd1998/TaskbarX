Imports System.Runtime.InteropServices
Imports TaskbarX.VisualEffects.Easing
Imports System.Timers
Imports System.Threading

Namespace VisualEffects

    Public Class TaskbarAnimate
        Public Shared IsAnimated As Boolean = True
        Public Shared AnimatedTaskbars As Integer

        Public Shared Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal iEffect As Effect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer)
            Try

                If Not oldpos = 0 Then
                    If valueToReach = oldpos Or (valueToReach - oldpos).ToString.Replace("-", "") <= 10 Then
                        'Prevent Wiggling (if the new position has a difference of 10 or lower then there is no reason to move)
                        Exit Sub
                    End If

                    ' If AnimatedTaskbars >= (TaskbarCenter.TaskbarCount) Then
                    If valueToReach = oldpos Or (valueToReach - oldpos).ToString.Replace("-", "") >= 300 Then
                        'Prevent Big Swing (if the New position has a difference of 300 Or higher then there Is no reason to move)
                        Exit Sub
                    End If
                    ' End If

                End If

                IsAnimated = False

                Dim originalValue As Integer = oldpos
                Dim elapsed As New Integer
                Dim minValue As Integer = Math.Min(originalValue, valueToReach)
                Dim maxValue As Integer = Math.Abs(valueToReach - originalValue)
                Dim increasing As Boolean = originalValue < valueToReach

                elapsed = 0

                While Not elapsed >= duration
                    Thread.Sleep(3)
                    elapsed = elapsed + 3

                    Dim newValue As Integer = CInt(Math.Truncate(easing(elapsed, minValue, maxValue, duration)))

                    If Not increasing Then
                        newValue = (originalValue + valueToReach) - newValue
                    End If

                    If orient = "H" Then
                        'Pass new Horizontal animation position
                        iEffect.SetValueX(hwnd, newValue)
                    Else
                        'Pass new Vertical animation position
                        iEffect.SetValueY(hwnd, newValue)
                    End If

                End While

                ' AnimatedTaskbars = AnimatedTaskbars + 1

                IsAnimated = True
                Main.ClearMemory()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Sub

    End Class

End Namespace