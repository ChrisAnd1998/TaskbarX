Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace VisualEffects.Easing

    Public Delegate Function EasingDelegate(ByVal currentTime As Double, ByVal minValue As Double, ByVal maxValue As Double, ByVal duration As Double) As Double

    Public Class EasingFunctions

        Public Shared Function Linear(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Return maxHeight * currentTime / duration + minHeight
        End Function

        ' Token: 0x06000015 RID: 21 RVA: 0x00002190 File Offset: 0x00000390
        Public Shared Function ExpoEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime <> duration Then
                Return maxHeight * (-Math.Pow(2.0, -10.0 * currentTime / duration) + 1.0) + minHeight
            End If
            Return minHeight + maxHeight
        End Function

        ' Token: 0x06000016 RID: 22 RVA: 0x000021C4 File Offset: 0x000003C4
        Public Shared Function ExpoEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime <> 0.0 Then
                Return maxHeight * Math.Pow(2.0, 10.0 * (currentTime / duration - 1.0)) + minHeight
            End If
            Return minHeight
        End Function

        ' Token: 0x06000017 RID: 23 RVA: 0x00002200 File Offset: 0x00000400
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
            currentTime = num4
            Return num2 * (-Math.Pow(x, num3 * num4) + 2.0) + minHeight
        End Function

        ' Token: 0x06000018 RID: 24 RVA: 0x000022B0 File Offset: 0x000004B0
        Public Shared Function ExpoEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.ExpoEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.ExpoEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x06000019 RID: 25 RVA: 0x00002314 File Offset: 0x00000514
        Public Shared Function CircEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = 1.0
            Dim num2 As Double = currentTime / duration - 1.0
            currentTime = num2
            Return maxHeight * Math.Sqrt(num - num2 * currentTime) + minHeight
        End Function

        ' Token: 0x0600001A RID: 26 RVA: 0x0000233C File Offset: 0x0000053C
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

        ' Token: 0x0600001B RID: 27 RVA: 0x00002384 File Offset: 0x00000584
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

        ' Token: 0x0600001C RID: 28 RVA: 0x00002410 File Offset: 0x00000610
        Public Shared Function CircEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.CircEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.CircEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x0600001D RID: 29 RVA: 0x00002474 File Offset: 0x00000674
        Public Shared Function QuadEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = -maxHeight
            Dim num2 As Double = currentTime / duration
            currentTime = num2
            Return num * num2 * (currentTime - 2.0) + minHeight
        End Function

        ' Token: 0x0600001E RID: 30 RVA: 0x0000248D File Offset: 0x0000068D
        Public Shared Function QuadEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration
            currentTime = num
            Return maxHeight * num * currentTime + minHeight
        End Function

        ' Token: 0x0600001F RID: 31 RVA: 0x0000249C File Offset: 0x0000069C
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

        ' Token: 0x06000020 RID: 32 RVA: 0x0000250C File Offset: 0x0000070C
        Public Shared Function QuadEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.QuadEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.QuadEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x06000021 RID: 33 RVA: 0x00002570 File Offset: 0x00000770
        Public Shared Function SineEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Return maxHeight * Math.Sin(currentTime / duration * 1.5707963267948966) + minHeight
        End Function

        ' Token: 0x06000022 RID: 34 RVA: 0x00002588 File Offset: 0x00000788
        Public Shared Function SineEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Return -maxHeight * Math.Cos(currentTime / duration * 1.5707963267948966) + maxHeight + minHeight
        End Function

        ' Token: 0x06000023 RID: 35 RVA: 0x000025A4 File Offset: 0x000007A4
        Public Shared Function SineEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / (duration / 2.0)
            currentTime = num
            If num < 1.0 Then
                Return maxHeight / 2.0 * Math.Sin(3.1415926535897931 * currentTime / 2.0) + minHeight
            End If
            Dim num2 As Double = -maxHeight / 2.0
            Dim num3 As Double = 3.1415926535897931
            Dim num4 As Double = currentTime - 1.0
            currentTime = num4
            Return num2 * (Math.Cos(num3 * num4 / 2.0) - 2.0) + minHeight
        End Function

        ' Token: 0x06000024 RID: 36 RVA: 0x00002638 File Offset: 0x00000838
        Public Shared Function SineEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.SineEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.SineEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x06000025 RID: 37 RVA: 0x0000269C File Offset: 0x0000089C
        Public Shared Function CubicEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration - 1.0
            currentTime = num
            Return maxHeight * (num * currentTime * currentTime + 1.0) + minHeight
        End Function

        ' Token: 0x06000026 RID: 38 RVA: 0x000026C0 File Offset: 0x000008C0
        Public Shared Function CubicEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration
            currentTime = num
            Return maxHeight * num * currentTime * currentTime + minHeight
        End Function

        ' Token: 0x06000027 RID: 39 RVA: 0x000026D0 File Offset: 0x000008D0
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

        ' Token: 0x06000028 RID: 40 RVA: 0x00002738 File Offset: 0x00000938
        Public Shared Function CubicEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.CubicEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.CubicEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x06000029 RID: 41 RVA: 0x0000279C File Offset: 0x0000099C
        Public Shared Function QuartEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = -maxHeight
            Dim num2 As Double = currentTime / duration - 1.0
            currentTime = num2
            Return num * (num2 * currentTime * currentTime * currentTime - 1.0) + minHeight
        End Function

        ' Token: 0x0600002A RID: 42 RVA: 0x000027C3 File Offset: 0x000009C3
        Public Shared Function QuartEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration
            currentTime = num
            Return maxHeight * num * currentTime * currentTime * currentTime + minHeight
        End Function

        ' Token: 0x0600002B RID: 43 RVA: 0x000027D8 File Offset: 0x000009D8
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

        ' Token: 0x0600002C RID: 44 RVA: 0x00002844 File Offset: 0x00000A44
        Public Shared Function QuartEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.QuartEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.QuartEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x0600002D RID: 45 RVA: 0x000028A8 File Offset: 0x00000AA8
        Public Shared Function QuintEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration - 1.0
            currentTime = num
            Return maxHeight * (num * currentTime * currentTime * currentTime * currentTime + 1.0) + minHeight
        End Function

        ' Token: 0x0600002E RID: 46 RVA: 0x000028D0 File Offset: 0x00000AD0
        Public Shared Function QuintEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration
            currentTime = num
            Return maxHeight * num * currentTime * currentTime * currentTime * currentTime + minHeight
        End Function

        ' Token: 0x0600002F RID: 47 RVA: 0x000028E4 File Offset: 0x00000AE4
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

        ' Token: 0x06000030 RID: 48 RVA: 0x00002954 File Offset: 0x00000B54
        Public Shared Function QuintEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.QuintEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.QuintEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x06000031 RID: 49 RVA: 0x000029B8 File Offset: 0x00000BB8
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

        ' Token: 0x06000032 RID: 50 RVA: 0x00002A28 File Offset: 0x00000C28
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

        ' Token: 0x06000033 RID: 51 RVA: 0x00002AA4 File Offset: 0x00000CA4
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

        ' Token: 0x06000034 RID: 52 RVA: 0x00002B90 File Offset: 0x00000D90
        Public Shared Function ElasticEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.ElasticEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.ElasticEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x06000035 RID: 53 RVA: 0x00002BF4 File Offset: 0x00000DF4
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

        ' Token: 0x06000036 RID: 54 RVA: 0x00002CB6 File Offset: 0x00000EB6
        Public Shared Function BounceEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Return maxHeight - EasingFunctions.BounceEaseOut(duration - currentTime, 0.0, maxHeight, duration) + minHeight
        End Function

        ' Token: 0x06000037 RID: 55 RVA: 0x00002CD0 File Offset: 0x00000ED0
        Public Shared Function BounceEaseInOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.BounceEaseIn(currentTime * 2.0, 0.0, maxHeight, duration) * 0.5 + minHeight
            End If
            Return EasingFunctions.BounceEaseOut(currentTime * 2.0 - duration, 0.0, maxHeight, duration) * 0.5 + maxHeight * 0.5 + minHeight
        End Function

        ' Token: 0x06000038 RID: 56 RVA: 0x00002D48 File Offset: 0x00000F48
        Public Shared Function BounceEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.BounceEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.BounceEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

        ' Token: 0x06000039 RID: 57 RVA: 0x00002DAC File Offset: 0x00000FAC
        Public Shared Function BackEaseOut(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration - 1.0
            currentTime = num
            Return maxHeight * (num * currentTime * (2.70158 * currentTime + 1.70158) + 1.0) + minHeight
        End Function

        ' Token: 0x0600003A RID: 58 RVA: 0x00002DE4 File Offset: 0x00000FE4
        Public Shared Function BackEaseIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            Dim num As Double = currentTime / duration
            currentTime = num
            Return maxHeight * num * currentTime * (2.70158 * currentTime - 1.70158) + minHeight
        End Function

        ' Token: 0x0600003B RID: 59 RVA: 0x00002E08 File Offset: 0x00001008
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

        ' Token: 0x0600003C RID: 60 RVA: 0x00002EAC File Offset: 0x000010AC
        Public Shared Function BackEaseOutIn(currentTime As Double, minHeight As Double, maxHeight As Double, duration As Double) As Double
            If currentTime < duration / 2.0 Then
                Return EasingFunctions.BackEaseOut(currentTime * 2.0, minHeight, maxHeight / 2.0, duration)
            End If
            Return EasingFunctions.BackEaseIn(currentTime * 2.0 - duration, minHeight + maxHeight / 2.0, maxHeight / 2.0, duration)
        End Function

    End Class

End Namespace