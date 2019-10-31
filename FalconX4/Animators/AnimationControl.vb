Imports FalconX4.VisualEffects
Imports FalconX4.VisualEffects.Animations.Effects
Imports FalconX4.VisualEffects.Easing

Public Class AnimationControl
    Public Shared AnimationSelection As String
    Public Shared AnimationSpeed As Integer
    Public Shared AnimationSpeed2 As Integer
    Public Shared TaskbarRefresh As Boolean
    Public Shared Animation As String

    Public Sub AnimateTaskbar()
        Try

            Do
                System.Threading.Thread.Sleep(10) : Application.DoEvents()
            Loop Until Animator.IsAnimated = True

            Console.WriteLine("Taskbar1 || " & VisualEffects.Animations.Effects.XLocationEffect.FirstTaskbarPosition)

            Dim Speed As Integer
            Dim Position As Integer

            Animation = AnimationSelection.ToString
            Position = VisualEffects.Animations.Effects.XLocationEffect.FirstTaskbarPosition
            Speed = AnimationSpeed

            If TaskbarRefresh = True Then
                TaskbarRefresh = False
                Console.WriteLine("Refresh Taskbar!")
                Taskbar.RefreshWindowsExplorer()
            End If

            If Animation = "None" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.Linear, Position, 100, 0)
            End If

            If Animation = "Linear" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.Linear, Position, Speed, 0)
            End If

            If Animation = "ExpoEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseOut, Position, Speed, 0)
            End If

            If Animation = "ExpoEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseIn, Position, Speed, 0)
            End If

            If Animation = "ExpoEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseInOut, Position, Speed, 0)
            End If

            If Animation = "ExpoEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "CircEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseOut, Position, Speed, 0)
            End If

            If Animation = "CircEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseIn, Position, Speed, 0)
            End If

            If Animation = "CircEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseInOut, Position, Speed, 0)
            End If

            If Animation = "CircEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "QuadEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseOut, Position, Speed, 0)
            End If

            If Animation = "QuadEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseIn, Position, Speed, 0)
            End If

            If Animation = "QuadEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseInOut, Position, Speed, 0)
            End If

            If Animation = "QuadEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "SineEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseOut, Position, Speed, 0)
            End If

            If Animation = "SineEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseIn, Position, Speed, 0)
            End If

            If Animation = "SineEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseInOut, Position, Speed, 0)
            End If

            If Animation = "SineEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "CubicEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseOut, Position, Speed, 0)
            End If

            If Animation = "CubicEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseIn, Position, Speed, 0)
            End If

            If Animation = "CubicEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseInOut, Position, Speed, 0)
            End If

            If Animation = "CubicEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "QuartEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseOut, Position, Speed, 0)
            End If

            If Animation = "QuartEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseIn, Position, Speed, 0)
            End If

            If Animation = "QuartEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseInOut, Position, Speed, 0)
            End If

            If Animation = "QuartEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "QuintEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseOut, Position, Speed, 0)
            End If

            If Animation = "QuintEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseIn, Position, Speed, 0)
            End If

            If Animation = "QuintEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseInOut, Position, Speed, 0)
            End If

            If Animation = "QuintEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "ElasticEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseOut, Position, Speed, 0)
            End If

            If Animation = "ElasticEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseIn, Position, Speed, 0)
            End If

            If Animation = "ElasticEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseInOut, Position, Speed, 0)
            End If

            If Animation = "ElasticEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "BounceEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseOut, Position, Speed, 0)
            End If

            If Animation = "BounceEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseIn, Position, Speed, 0)
            End If

            If Animation = "BounceEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseInOut, Position, Speed, 0)
            End If

            If Animation = "BounceEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseOutIn, Position, Speed, 0)
            End If

            If Animation = "BackEaseOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseOut, Position, Speed, 0)
            End If

            If Animation = "BackEaseIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseIn, Position, Speed, 0)
            End If

            If Animation = "BackEaseInOut" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseInOut, Position, Speed, 0)
            End If

            If Animation = "BackEaseOutIn" Then
                Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseOutIn, Position, Speed, 0)
            End If

            Taskbar.Updating = False
        Catch ex As Exception

        End Try
    End Sub

    Private Sub AnimationControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

End Class