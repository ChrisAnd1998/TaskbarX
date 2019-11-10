Imports FalconX4.VisualEffects.Animators
Imports FalconX4.VisualEffects.Easing

Namespace VisualEffects
    Public Module AnimationExtensions

        <System.Runtime.CompilerServices.Extension>
        Public Function Animate(ByVal control As Control, ByVal iAnimation As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal delay As Integer, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1) As AnimationStatus
            Return Animator.Animate(control, iAnimation, easing, valueToReach, duration, delay, reverse, loops)
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function Animate2(ByVal control As Control, ByVal iAnimation As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal delay As Integer, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1) As AnimationStatus
            Return Animator2.Animate(control, iAnimation, easing, valueToReach, duration, delay, reverse, loops)
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function Animate3(ByVal control As Control, ByVal iAnimation As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal delay As Integer, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1) As AnimationStatus
            Return Animator3.Animate(control, iAnimation, easing, valueToReach, duration, delay, reverse, loops)
        End Function

    End Module
End Namespace