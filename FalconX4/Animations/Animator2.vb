Imports System.Runtime.InteropServices

Imports FalconX4.VisualEffects.Animations.Effects

Imports FalconX4.VisualEffects.Easing

Namespace VisualEffects

    Public Class Animator2

        <DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
        End Function

        Public Shared SWP_NOSIZE As UInt32 = 1
        Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
        Public Shared SWP_NOACTIVATE As UInt32 = 16
        Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
        Public Shared SWP_NOZORDER As UInt32 = 4

        Public Shared IsAnimated As Boolean = True

        Public Shared Sub Animate(ByVal iEffect As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal delay As Integer, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1)
            Try

                If Not XLocationEffect2.SecondTaskbarOldPosition = 0 Then
                    If Taskbar.Horizontal = True Then
                        SetWindowPos(XLocationEffect2.SecondTaskbarPtr, IntPtr.Zero, XLocationEffect2.SecondTaskbarOldPosition, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                    Else
                        SetWindowPos(XLocationEffect2.SecondTaskbarPtr, IntPtr.Zero, 0, XLocationEffect2.SecondTaskbarOldPosition, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                    End If
                End If

                If Not XLocationEffect2.SecondTaskbarOldPosition = 0 Then
                    If XLocationEffect2.SecondTaskbarPosition = XLocationEffect2.SecondTaskbarOldPosition Or XLocationEffect2.SecondTaskbarPosition = XLocationEffect2.SecondTaskbarOldPosition + 1 Or XLocationEffect2.SecondTaskbarPosition = XLocationEffect2.SecondTaskbarOldPosition - 1 Or XLocationEffect2.SecondTaskbarPosition = XLocationEffect2.SecondTaskbarOldPosition - 2 Or XLocationEffect2.SecondTaskbarPosition = XLocationEffect2.SecondTaskbarOldPosition + 2 Then
                        Console.WriteLine("Animator 2 | Aborted difference too small")
                        Exit Sub
                    End If
                End If

                IsAnimated = False

                Dim stopwatch = New Stopwatch()

                Dim tempVar As New Timers.Timer

                Dim originalValue As Integer

                If AnimationControl.Animation = "None" Then
                    originalValue = XLocationEffect2.SecondTaskbarPosition
                Else
                    originalValue = XLocationEffect2.SecondTaskbarOldPosition
                End If

                Dim animationTimer As New System.Timers.Timer With {
                    .Interval = 1
                }

                Console.WriteLine("Animator 2 | From " & originalValue & " To " & valueToReach)

                AddHandler animationTimer.Elapsed, Sub(o, e2)

                                                       Dim increasing As Boolean = originalValue < valueToReach

                                                       Dim minValue As Integer = Math.Min(originalValue, valueToReach)
                                                       Dim maxValue As Integer = Math.Abs(valueToReach - originalValue)
                                                       Dim newValue As Integer = CInt(Math.Truncate(easing(stopwatch.ElapsedMilliseconds, minValue, maxValue, duration)))

                                                       If Not increasing Then
                                                           newValue = (originalValue + valueToReach) - newValue
                                                       End If

                                                       If Taskbar.Horizontal = True Then
                                                           iEffect.SetValueX(originalValue, valueToReach, newValue)
                                                       ElseIf Taskbar.Horizontal = False Then
                                                           iEffect.SetValueY(originalValue, valueToReach, newValue)
                                                       End If

                                                       Dim timeout As Boolean = stopwatch.ElapsedMilliseconds >= duration
                                                       If timeout Then

                                                           animationTimer.Stop()

                                                           IsAnimated = True
                                                           Console.WriteLine("Animator 2 | Ended on " & newValue)

                                                           If Taskbar.Horizontal = True Then
                                                               SetWindowPos(XLocationEffect2.SecondTaskbarPtr, IntPtr.Zero, valueToReach, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                                           Else
                                                               SetWindowPos(XLocationEffect2.SecondTaskbarPtr, IntPtr.Zero, 0, valueToReach, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                                                           End If
                                                           Console.WriteLine("Animator 2 | Position Fixed on " & valueToReach)

                                                       End If

                                                   End Sub

                stopwatch.Start()
                animationTimer.Start()
            Catch
            End Try
            Console.WriteLine("Animator 2 | Fully closed")

        End Sub

    End Class

End Namespace