Option Strict On

Public Delegate Function EasingDelegate(ByVal currentTime As Double, ByVal minValue As Double, ByVal maxValue As Double, ByVal duration As Double) As Double

Public Class Easings

    Public Shared Function Linear(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Return maxHeight * currentTime / duration + minHeight

    End Function

    Public Shared Function ExpoEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime <> duration Then
            Return maxHeight * (-Math.Pow(2.0, -10.0 * currentTime / duration) + 1.0) + minHeight
        End If
        Return minHeight + maxHeight
    End Function

    Public Shared Function ExpoEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime <> 0.0 Then
            Return maxHeight * Math.Pow(2.0, 10.0 * (currentTime / duration - 1.0)) + minHeight
        End If
        Return minHeight
    End Function

    Public Shared Function ExpoEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime = 0.0 Then
            Return minHeight
        End If
        If currentTime = duration Then
            Return minHeight + maxHeight
        End If
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Return maxHeight / 2.0 * Math.Pow(2.0, 10.0 * (currentTime - 1.0)) + minHeight
        End If
        Dim num2 As Double = maxHeight / 2.0
        Dim x As Double = 2.0
        Dim num3 As Double = -10.0
        Dim num4 As Double = currentTime - 1.0
        Return num2 * (-Math.Pow(x, num3 * num4) + 2.0) + minHeight
    End Function

    Public Shared Function ExpoEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.ExpoEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.ExpoEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function CircEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = 1.0
        Dim num2 As Double = currentTime / duration - 1.0
        currentTime = num2
        Return maxHeight * Math.Sqrt(num - num2 * currentTime) + minHeight
    End Function

    Public Shared Function CircEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = 1.0
        Dim num2 As Double = currentTime / duration
        currentTime = num2
        Dim sqrt As Double = Math.Sqrt(num - num2 * currentTime)
        If Double.IsNaN(sqrt) Then
            sqrt = 0.0
        End If
        Return -maxHeight * (sqrt - 1.0) + minHeight
    End Function

    Public Shared Function CircEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Return -maxHeight / 2.0 * (Math.Sqrt(1.0 - currentTime * currentTime) - 1.0) + minHeight
        End If
        Dim num2 As Double = maxHeight / 2.0
        Dim num3 As Double = 1.0
        Dim num4 As Double = currentTime - 2.0
        currentTime = num4
        Return num2 * (Math.Sqrt(num3 - num4 * currentTime) + 1.0) + minHeight
    End Function

    Public Shared Function CircEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.CircEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.CircEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function QuadEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = -maxHeight
        Dim num2 As Double = currentTime / duration
        currentTime = num2
        Return num * num2 * (currentTime - 2.0) + minHeight
    End Function

    Public Shared Function QuadEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        Return maxHeight * num * currentTime + minHeight
    End Function

    Public Shared Function QuadEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Return maxHeight / 2.0 * currentTime * currentTime + minHeight
        End If
        Dim num2 As Double = -maxHeight / 2.0
        Dim num3 As Double = currentTime - 1.0
        currentTime = num3
        Return num2 * (num3 * (currentTime - 2.0) - 1.0) + minHeight
    End Function

    Public Shared Function QuadEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.QuadEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.QuadEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function SineEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Return maxHeight * Math.Sin(currentTime / duration * 1.5707963267948966) + minHeight
    End Function

    Public Shared Function SineEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Return -maxHeight * Math.Cos(currentTime / duration * 1.5707963267948966) + maxHeight + minHeight
    End Function

    Public Shared Function SineEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Return maxHeight / 2.0 * Math.Sin(3.1415926535897931 * currentTime / 2.0) + minHeight
        End If
        Dim num2 As Double = -maxHeight / 2.0
        Dim num3 As Double = 3.1415926535897931
        Dim num4 As Double = currentTime - 1.0
        Return num2 * (Math.Cos(num3 * num4 / 2.0) - 2.0) + minHeight
    End Function

    Public Shared Function SineEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.SineEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.SineEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function CubicEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration - 1.0
        currentTime = num
        Return maxHeight * (num * currentTime * currentTime + 1.0) + minHeight
    End Function

    Public Shared Function CubicEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        Return maxHeight * num * currentTime * currentTime + minHeight
    End Function

    Public Shared Function CubicEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Return maxHeight / 2.0 * currentTime * currentTime * currentTime + minHeight
        End If
        Dim num2 As Double = maxHeight / 2.0
        Dim num3 As Double = currentTime - 2.0
        currentTime = num3
        Return num2 * (num3 * currentTime * currentTime + 2.0) + minHeight
    End Function

    Public Shared Function CubicEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.CubicEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.CubicEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function QuartEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = -maxHeight
        Dim num2 As Double = currentTime / duration - 1.0
        currentTime = num2
        Return num * (num2 * currentTime * currentTime * currentTime - 1.0) + minHeight
    End Function

    Public Shared Function QuartEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        Return maxHeight * num * currentTime * currentTime * currentTime + minHeight
    End Function

    Public Shared Function QuartEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Return maxHeight / 2.0 * currentTime * currentTime * currentTime * currentTime + minHeight
        End If
        Dim num2 As Double = -maxHeight / 2.0
        Dim num3 As Double = currentTime - 2.0
        currentTime = num3
        Return num2 * (num3 * currentTime * currentTime * currentTime - 2.0) + minHeight
    End Function

    Public Shared Function QuartEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.QuartEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.QuartEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function QuintEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration - 1.0
        currentTime = num
        Return maxHeight * (num * currentTime * currentTime * currentTime * currentTime + 1.0) + minHeight
    End Function

    Public Shared Function QuintEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        Return maxHeight * num * currentTime * currentTime * currentTime * currentTime + minHeight
    End Function

    Public Shared Function QuintEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Return maxHeight / 2.0 * currentTime * currentTime * currentTime * currentTime * currentTime + minHeight
        End If
        Dim num2 As Double = maxHeight / 2.0
        Dim num3 As Double = currentTime - 2.0
        currentTime = num3
        Return num2 * (num3 * currentTime * currentTime * currentTime * currentTime + 2.0) + minHeight
    End Function

    Public Shared Function QuintEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.QuintEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.QuintEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function ElasticEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        If num = 1.0 Then
            Return minHeight + maxHeight
        End If
        Dim p As Double = duration * 0.3
        Dim s As Double = p / 4.0
        Return maxHeight * Math.Pow(2.0, -10.0 * currentTime) * Math.Sin((currentTime * duration - s) * 6.2831853071795862 / p) + maxHeight + minHeight
    End Function

    Public Shared Function ElasticEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        If num = 1.0 Then
            Return minHeight + maxHeight
        End If
        Dim p As Double = duration * 0.3
        Dim s As Double = p / 4.0
        Dim x As Double = 2.0
        Dim num2 As Double = 10.0
        Dim num3 As Double = currentTime - 1.0
        currentTime = num3
        Return -(maxHeight * Math.Pow(x, num2 * num3) * Math.Sin((currentTime * duration - s) * 6.2831853071795862 / p)) + minHeight
    End Function

    Public Shared Function ElasticEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num = 2.0 Then
            Return minHeight + maxHeight
        End If
        Dim p As Double = duration * 0.44999999999999996
        Dim s As Double = p / 4.0
        If currentTime < 1.0 Then
            Dim num2 As Double = -0.5
            Dim x As Double = 2.0
            Dim num3 As Double = 10.0
            Dim num4 As Double = currentTime - 1.0
            currentTime = num4
            Return num2 * (maxHeight * Math.Pow(x, num3 * num4) * Math.Sin((currentTime * duration - s) * 6.2831853071795862 / p)) + minHeight
        End If
        Dim x2 As Double = 2.0
        Dim num5 As Double = -10.0
        Dim num6 As Double = currentTime - 1.0
        currentTime = num6
        Return maxHeight * Math.Pow(x2, num5 * num6) * Math.Sin((currentTime * duration - s) * 6.2831853071795862 / p) * 0.5 + maxHeight + minHeight
    End Function

    Public Shared Function ElasticEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.ElasticEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.ElasticEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function BounceEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        If num < 0.36363636363636365 Then
            Return maxHeight * (7.5625 * currentTime * currentTime) + minHeight
        End If
        If currentTime < 0.72727272727272729 Then
            Dim num2 As Double = 7.5625
            Dim num3 As Double = currentTime - 0.54545454545454541
            currentTime = num3
            Return maxHeight * (num2 * num3 * currentTime + 0.75) + minHeight
        End If
        If currentTime < 0.90909090909090906 Then
            Dim num4 As Double = 7.5625
            Dim num5 As Double = currentTime - 0.81818181818181823
            currentTime = num5
            Return maxHeight * (num4 * num5 * currentTime + 0.9375) + minHeight
        End If
        Dim num6 As Double = 7.5625
        Dim num7 As Double = currentTime - 0.95454545454545459
        currentTime = num7
        Return maxHeight * (num6 * num7 * currentTime + 0.984375) + minHeight
    End Function

    Public Shared Function BounceEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Return maxHeight - Easings.BounceEaseOut(duration - currentTime, 0.0, maxHeight, duration) + minHeight
    End Function

    Public Shared Function BounceEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.BounceEaseIn(currentTime * 2.0, 0.0, maxHeight, duration) * 0.5 + minHeight
        End If
        Return Easings.BounceEaseOut(currentTime * 2.0 - duration, 0.0, maxHeight, duration) * 0.5 + maxHeight * 0.5 + minHeight
    End Function

    Public Shared Function BounceEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.BounceEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.BounceEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

    Public Shared Function BackEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration - 1.0
        currentTime = num
        Return maxHeight * (num * currentTime * (2.70158 * currentTime + 1.70158) + 1.0) + minHeight
    End Function

    Public Shared Function BackEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim num As Double = currentTime / duration
        currentTime = num
        Return maxHeight * num * currentTime * (2.70158 * currentTime - 1.70158) + minHeight
    End Function

    Public Shared Function BackEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        Dim s As Double = 1.70158
        Dim num As Double = currentTime / (duration / 2.0)
        currentTime = num
        If num < 1.0 Then
            Dim num2 As Double = maxHeight / 2.0
            Dim num3 As Double = currentTime * currentTime
            Dim num4 As Double = s * 1.525
            s = num4
            Return num2 * (num3 * ((num4 + 1.0) * currentTime - s)) + minHeight
        End If
        Dim num5 As Double = maxHeight / 2.0
        Dim num6 As Double = currentTime - 2.0
        currentTime = num6
        Dim num7 As Double = num6 * currentTime
        Dim num8 As Double = s * 1.525
        s = num8
        Return num5 * (num7 * ((num8 + 1.0) * currentTime + s) + 2.0) + minHeight
    End Function

    Public Shared Function BackEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
        If currentTime < duration / 2.0 Then
            Return Easings.BackEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
        End If
        Return Easings.BackEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
    End Function

End Class