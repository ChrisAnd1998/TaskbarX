Imports System.Runtime.InteropServices
Imports System.Threading
Imports FalconX4.VisualEffects.Animations.Effects
Imports FalconX4.VisualEffects.Animators
Imports FalconX4.VisualEffects.Easing

Namespace VisualEffects

    Public Class Animator2

        Public Event Animated As EventHandler(Of AnimationStatus)

        <DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
        End Function

        Public Shared SWP_NOSIZE As UInt32 = 1
        Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
        Public Shared SWP_NOACTIVATE As UInt32 = 16
        Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
        Public Shared SWP_NOZORDER As UInt32 = 4

        Public Shared IsAnimated As Boolean = True

        Public Shared Function Animate(ByVal control As Control, ByVal iEffect As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal delay As Integer, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1) As AnimationStatus
            '  Try
            If Not XLocationEffect2.SecondTaskbarOldPosition = 0 Then
                If Taskbar.Horizontal = True Then
                    SetWindowPos(XLocationEffect2.SecondTaskbarPtr, IntPtr.Zero, XLocationEffect2.SecondTaskbarOldPosition, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                Else
                    SetWindowPos(XLocationEffect2.SecondTaskbarPtr, IntPtr.Zero, 0, XLocationEffect2.SecondTaskbarOldPosition, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
                End If
            End If

            IsAnimated = False
            'used to calculate animation frame based on how much time has effectively passed
            Dim stopwatch = New Stopwatch()

            'used to cancel animation
            Dim cancelTokenSource = New CancellationTokenSource()

            'used to access animation progress
            Dim animationStatus = New AnimationStatus(cancelTokenSource, stopwatch) With {
                .IsCompleted = False
            }

            'This timer allows delayed start. Control's state checks and evaluations are delayed too.
            Dim tempVar As New Timers.Timer

            Dim originalValue As Integer

            If AnimationControl.Animation = "None" Then
                originalValue = XLocationEffect2.SecondTaskbarPosition
            Else
                originalValue = XLocationEffect2.SecondTaskbarOldPosition
            End If

            'is there anything to do here?

            If originalValue = valueToReach Then
                animationStatus.IsCompleted = True

            End If

            'upper bound check
            Dim maxVal As Integer = iEffect.GetMaximumValue(control)
            If valueToReach > maxVal Then
                Dim msg As String = String.Format("Value must be lesser than the maximum allowed. " & "Max: {0}, provided value: {1}", maxVal, valueToReach)

                Throw New ArgumentException(msg, "valueToReach")
            End If

            'lower bound check
            Dim minVal As Integer = iEffect.GetMinimumValue(control)
            If valueToReach < iEffect.GetMinimumValue(control) Then
                Dim msg As String = String.Format("Value must be greater than the minimum allowed. " & "Min: {0}, provided value: {1}", minVal, valueToReach)

                Throw New ArgumentException(msg, "valueToReach")
            End If

            Dim reversed As Boolean = False
            Dim performedLoops As Integer = 0

            Dim actualValueChange As Integer = Math.Abs(originalValue - valueToReach)

            Dim animationTimer As New System.Timers.Timer With {
                .Interval = 0.1
            }

            'main animation timer tick
            AddHandler animationTimer.Elapsed, Sub(o, e2)
                                                   'cancellation support
                                                   If cancelTokenSource.Token.IsCancellationRequested Then
                                                       animationStatus.IsCompleted = True
                                                       animationTimer.Stop()
                                                       stopwatch.Stop()

                                                       Return
                                                   End If

                                                   'main logic
                                                   Dim increasing As Boolean = originalValue < valueToReach

                                                   Dim minValue As Integer = Math.Min(originalValue, valueToReach)
                                                   Dim maxValue As Integer = Math.Abs(valueToReach - originalValue)
                                                   Dim newValue As Integer = CInt(Math.Truncate(easing(stopwatch.ElapsedMilliseconds, minValue, maxValue, duration)))

                                                   ' Console.WriteLine(newValue)

                                                   If Not increasing Then
                                                       newValue = (originalValue + valueToReach) - newValue - 1
                                                   End If

                                                   'control.BeginInvoke(New MethodInvoker(Sub()
                                                   If Taskbar.Horizontal = True Then
                                                       iEffect.SetValueX(control, originalValue, valueToReach, newValue)
                                                   ElseIf Taskbar.Horizontal = False Then
                                                       iEffect.SetValueY(control, originalValue, valueToReach, newValue)
                                                   End If

                                                   Dim timeout As Boolean = stopwatch.ElapsedMilliseconds >= duration
                                                   If timeout Then
                                                       If reverse AndAlso (Not reversed OrElse loops <= 0 OrElse performedLoops < loops) Then
                                                           reversed = Not reversed
                                                           If reversed Then
                                                               performedLoops += 1
                                                           End If

                                                           Dim initialValue As Integer = originalValue
                                                           Dim finalValue As Integer = valueToReach

                                                           valueToReach = If(valueToReach = finalValue, initialValue, finalValue)
                                                           originalValue = If(valueToReach = finalValue, initialValue, finalValue)

                                                           stopwatch.Restart()
                                                           animationTimer.Start()
                                                       Else
                                                           animationStatus.IsCompleted = True
                                                           animationTimer.Stop()
                                                           ' stopwatch.Stop()
                                                           IsAnimated = True

                                                           'RaiseEvent Animated(control, animationStatus)
                                                       End If
                                                   End If
                                                   'End Sub))
                                               End Sub

            'start
            stopwatch.Start()
            animationTimer.Start()

            Return animationStatus
            '   Catch
            '   End Try

        End Function

    End Class

End Namespace