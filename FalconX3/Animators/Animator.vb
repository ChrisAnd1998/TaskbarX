Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms

Imports FalconX3.VisualEffects.Animators
Imports FalconX3.VisualEffects.Easing
Imports FalconX3.VisualEffects.Animations.Effects

Namespace VisualEffects
    Public Module Animator
        '        I decided to add no abstraction over animators and provide bare functionality.
        '         *I think nothing is better than a static method here.

        Public Event Animated As EventHandler(Of AnimationStatus)

        ''' <summary>
        ''' Animate a control property from its present value to a target one
        ''' </summary>
        ''' <param name="control">Target control</param>
        ''' <param name="iEffect">Effect to apply</param>
        ''' <param name="easing">Easing function to apply</param>
        ''' <param name="valueToReach">Target value reached when animation completes</param>
        ''' <param name="duration">Amount of time taken to reach the target value</param>
        ''' <param name="delay">Amount of delay to apply before animation starts</param>
        ''' <param name="reverse">If set to true, animation reaches target value and animates back to initial value. It takes 2*<paramref name="duration"/></param>
        ''' <param name="loops">If reverse is set to true, indicates how many loops to perform. Negatives or zero mean infinite loop</param>
        ''' <returns></returns>
        Public Function Animate(ByVal control As Control, ByVal iEffect As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal delay As Integer, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1) As AnimationStatus
            Try

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

                'is there anything to do here?
                Dim originalValue As Integer = iEffect.GetCurrentValue(control)
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
                    .Interval = 1
                }

                'because of naive interval calculation this is required
                If iEffect.Interaction = EffectInteractions.COLOR Then
                    animationTimer.Interval = 10
                End If

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

                                                       If Not increasing Then
                                                           newValue = (originalValue + valueToReach) - newValue - 1
                                                       End If

                                                       control.BeginInvoke(New MethodInvoker(Sub()
                                                                                                 iEffect.SetValue(control, originalValue, valueToReach, newValue)

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
                                                                                                         stopwatch.Stop()
                                                                                                         animationTimer.Dispose()
                                                                                                         tempVar.Dispose()

                                                                                                         RaiseEvent Animated(control, animationStatus)
                                                                                                     End If
                                                                                                 End If
                                                                                             End Sub))
                                                   End Sub

                'start
                stopwatch.Start()
                animationTimer.Start()

                Return animationStatus
            Catch
            End Try
            Return Nothing
        End Function

    End Module
End Namespace