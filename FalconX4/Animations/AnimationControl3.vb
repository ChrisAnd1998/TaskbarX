Imports FalconX4.VisualEffects
Imports FalconX4.VisualEffects.Animations.Effects
Imports FalconX4.VisualEffects.Easing

Public Class AnimationControl3
    Public Shared AnimationSelection As String
    Public Shared AnimationSpeed As Integer
    Public Shared AnimationSpeed2 As Integer
    Public Shared TaskbarRefresh As Boolean
    Public Shared Animation As String
    Shared Speed As Integer
    Shared Position As Integer

    Shared Sub animateTaskbar3()
        Try
            Do
                System.Threading.Thread.Sleep(10) : Application.DoEvents()
            Loop Until Animator3.IsAnimated = True
            Console.WriteLine("Taskbar3 || " & VisualEffects.Animations.Effects.XLocationEffect3.ThirdTaskbarPosition)
            Animation = AnimationSelection.ToString
            Position = VisualEffects.Animations.Effects.XLocationEffect3.ThirdTaskbarPosition
            Speed = AnimationSpeed
            If Animation = "None" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aNone)
                t1.Start()
            End If
            If Animation = "Linear" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aLinear)
                t1.Start()
            End If
            If Animation = "ExpoEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aExpoEaseOut)
                t1.Start()
            End If
            If Animation = "ExpoEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aExpoEaseIn)
                t1.Start()
            End If
            If Animation = "ExpoEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aExpoEaseInOut)
                t1.Start()
            End If
            If Animation = "ExpoEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aExpoEaseOutIn)
                t1.Start()
            End If
            If Animation = "CircEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCircEaseOut)
                t1.Start()
            End If
            If Animation = "CircEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCircEaseIn)
                t1.Start()
            End If
            If Animation = "CircEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCircEaseInOut)
                t1.Start()
            End If
            If Animation = "CircEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCircEaseOutIn)
                t1.Start()
            End If
            If Animation = "QuadEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuadEaseOut)
                t1.Start()
            End If
            If Animation = "QuadEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuadEaseIn)
                t1.Start()
            End If
            If Animation = "QuadEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuadEaseInOut)
                t1.Start()
            End If
            If Animation = "QuadEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuadEaseOutIn)
                t1.Start()
            End If
            If Animation = "SineEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aSineEaseOut)
                t1.Start()
            End If
            If Animation = "SineEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aSineEaseIn)
                t1.Start()
            End If
            If Animation = "SineEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aSineEaseInOut)
                t1.Start()
            End If
            If Animation = "SineEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aSineEaseOutIn)
                t1.Start()
            End If
            If Animation = "CubicEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCubicEaseOut)
                t1.Start()
            End If
            If Animation = "CubicEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCubicEaseIn)
                t1.Start()
            End If
            If Animation = "CubicEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCubicEaseInOut)
                t1.Start()
            End If
            If Animation = "CubicEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aCubicEaseOutIn)
                t1.Start()
            End If
            If Animation = "QuartEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuartEaseOut)
                t1.Start()
            End If
            If Animation = "QuartEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuartEaseIn)
                t1.Start()
            End If
            If Animation = "QuartEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuartEaseInOut)
                t1.Start()
            End If
            If Animation = "QuartEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuartEaseOutIn)
                t1.Start()
            End If
            If Animation = "QuintEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuintEaseOut)
                t1.Start()
            End If
            If Animation = "QuintEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuintEaseIn)
                t1.Start()
            End If
            If Animation = "QuintEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuintEaseInOut)
                t1.Start()
            End If
            If Animation = "QuintEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aQuintEaseOutIn)
                t1.Start()
            End If
            If Animation = "ElasticEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aElasticEaseOut)
                t1.Start()
            End If
            If Animation = "ElasticEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aElasticEaseIn)
                t1.Start()
            End If
            If Animation = "ElasticEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aElasticEaseInOut)
                t1.Start()
            End If
            If Animation = "ElasticEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aElasticEaseOutIn)
                t1.Start()
            End If
            If Animation = "BounceEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBounceEaseOut)
                t1.Start()
            End If
            If Animation = "BounceEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBounceEaseIn)
                t1.Start()
            End If
            If Animation = "BounceEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBounceEaseInOut)
                t1.Start()
            End If
            If Animation = "BounceEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBounceEaseOutIn)
                t1.Start()
            End If
            If Animation = "BackEaseOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBackEaseOut)
                t1.Start()
            End If
            If Animation = "BackEaseIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBackEaseIn)
                t1.Start()
            End If
            If Animation = "BackEaseInOut" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBackEaseInOut)
                t1.Start()
            End If
            If Animation = "BackEaseOutIn" Then
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf aBackEaseOutIn)
                t1.Start()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Shared Sub aNone()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.Linear, Position, 100, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aLinear()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.Linear, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ExpoEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ExpoEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ExpoEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ExpoEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CircEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CircEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CircEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CircEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuadEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuadEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuadEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuadEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.SineEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.SineEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.SineEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.SineEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CubicEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CubicEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CubicEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.CubicEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuartEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuartEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuartEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuartEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuintEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuintEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuintEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.QuintEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ElasticEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ElasticEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ElasticEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.ElasticEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BounceEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BounceEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BounceEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BounceEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BackEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BackEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseInOut()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BackEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseOutIn()
        Animate3(New XLocationEffect3(), AddressOf EasingFunctions.BackEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

End Class