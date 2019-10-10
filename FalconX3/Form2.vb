Imports VisualEffects
Imports VisualEffects.Animations.Effects
Imports VisualEffects.Easing

Public Class Form2


    Dim IsMoving As Boolean

    Private Sub Panel1_Move(sender As Object, e As EventArgs) Handles Panel1.Move
        Form1.MovetoPos()
    End Sub

    Sub AnimatorMove()

        Dim position As Integer
        position = Form1.TaskbarNewPos

        Dim speed As Integer = Form1.NumericUpDown1.Value

        Do Until IsMoving = False
            System.Threading.Thread.Sleep(10) : Application.DoEvents()
        Loop

        IsMoving = True

        If Form1.ComboBox1.Text = "" Then
            Panel1.Left = position
        End If

        If Form1.ComboBox1.Text = "None" Then
            Panel1.Left = position
        End If

        If Form1.ComboBox1.Text = "Linear" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.Linear, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ExpoEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ExpoEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ExpoEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ExpoEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ExpoEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CircEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CircEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CircEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CircEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CircEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuadEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuadEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuadEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuadEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuadEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "SineEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "SineEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "SineEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "SineEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.SineEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CubicEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CubicEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CubicEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "CubicEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.CubicEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuartEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuartEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuartEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuartEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuartEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuintEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuintEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuintEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "QuintEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.QuintEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ElasticEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ElasticEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ElasticEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "ElasticEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.ElasticEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BounceEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BounceEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BounceEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BounceEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BounceEaseOutIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BackEaseOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BackEaseIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseIn, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BackEaseInOut" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseInOut, position, speed, 0)
        End If

        If Form1.ComboBox1.Text = "BackEaseOutIn" Then
            Panel1.Animate(New XLocationEffect(), AddressOf EasingFunctions.BackEaseOutIn, position, speed, 0)
        End If

    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Animator.Animated, AddressOf IsAnimated
    End Sub

    Sub IsAnimated()
        IsMoving = False
    End Sub

End Class