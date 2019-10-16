Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms

Namespace VisualEffects.Animations.Effects

    Public Class XLocationEffect
        Implements IEffect

        Public Function GetCurrentValue(ByVal control As Control) As Integer Implements IEffect.GetCurrentValue
            Return control.Left
        End Function

        Public Sub SetValue(ByVal control As Control, ByVal originalValue As Integer, ByVal valueToReach As Integer, ByVal newValue As Integer) Implements IEffect.SetValue
            control.Left = newValue
        End Sub

        Public Function GetMinimumValue(ByVal control As Control) As Integer Implements IEffect.GetMinimumValue
            Return Int32.MinValue
        End Function

        Public Function GetMaximumValue(ByVal control As Control) As Integer Implements IEffect.GetMaximumValue
            Return Int32.MaxValue
        End Function

        Public ReadOnly Property IEffect_Interaction As EffectInteractions Implements IEffect.Interaction
            Get
                Return EffectInteractions.X
            End Get
        End Property

    End Class

End Namespace