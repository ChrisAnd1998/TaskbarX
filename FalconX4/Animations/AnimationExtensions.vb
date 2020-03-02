Imports FalconX4.VisualEffects.Easing

Namespace VisualEffects
    Public Module AnimationExtensions

        <System.Runtime.CompilerServices.Extension>
        Public Sub Animate(ByVal iAnimation As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Double, ByVal duration As Double, ByVal delay As Double, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1)
            Animator.Animate(iAnimation, easing, valueToReach, duration, delay, reverse, loops)
        End Sub

        <System.Runtime.CompilerServices.Extension>
        Public Sub Animate2(ByVal iAnimation As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Double, ByVal duration As Double, ByVal delay As Double, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1)
            Animator2.Animate(iAnimation, easing, valueToReach, duration, delay, reverse, loops)
        End Sub

        <System.Runtime.CompilerServices.Extension>
        Public Sub Animate3(ByVal iAnimation As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Double, ByVal duration As Double, ByVal delay As Double, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1)
            Animator3.Animate(iAnimation, easing, valueToReach, duration, delay, reverse, loops)
        End Sub

    End Module
End Namespace