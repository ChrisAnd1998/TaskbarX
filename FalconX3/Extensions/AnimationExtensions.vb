Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports System.Threading

Imports FalconX3.VisualEffects.Easing
Imports FalconX3.VisualEffects.Animators

Namespace VisualEffects
    Public Module AnimationExtensions

        <System.Runtime.CompilerServices.Extension>
        Public Function Animate(ByVal control As Control, ByVal iAnimation As IEffect, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer, ByVal delay As Integer, Optional ByVal reverse As Boolean = False, Optional ByVal loops As Integer = 1) As AnimationStatus
            Return Animator.Animate(control, iAnimation, easing, valueToReach, duration, delay, reverse, loops)
        End Function

    End Module
End Namespace