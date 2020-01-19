Imports FalconX4.VisualEffects
Imports FalconX4.VisualEffects.Animations.Effects
Imports FalconX4.VisualEffects.Easing

Public Class AnimationControl
    Public Shared AnimationSelection As String
    Public Shared AnimationSpeed As Integer
    Public Shared AnimationSpeed2 As Integer
    Public Shared TaskbarRefresh As Boolean
    Public Shared Animation As String

    Shared Speed As Integer
    Shared Position As Integer

    Public Shared Sub animateTaskbar()

        Try

            Do
                System.Threading.Thread.Sleep(10) : Application.DoEvents()
            Loop Until Animator.IsAnimated = True

            Console.WriteLine("Taskbar1 || " & VisualEffects.Animations.Effects.XLocationEffect.FirstTaskbarPosition)

            Animation = AnimationSelection.ToString
            Position = VisualEffects.Animations.Effects.XLocationEffect.FirstTaskbarPosition
            Speed = AnimationSpeed

            If TaskbarRefresh = True Then
                TaskbarRefresh = False
                Console.WriteLine("Refresh Taskbar!")
                Taskbar.RefreshWindowsExplorer()
            End If

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
        Animate(New XLocationEffect(), AddressOf EasingFunctions.Linear, Position, 100, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aLinear()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.Linear, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aExpoEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCircEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuadEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aSineEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aCubicEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuartEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aQuintEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aElasticEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBounceEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseInOut()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseInOut, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

    Shared Sub aBackEaseOutIn()
        Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseOutIn, Position, Speed, 0)
        Taskbar.Updating = False
    End Sub

End Class