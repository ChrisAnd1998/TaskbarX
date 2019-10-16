Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports FalconX3.VisualEffects.Animations.Effects

Namespace VisualEffects

    ''' <summary>
    ''' By implementing this interface you define what property of your control
    ''' is manipulated and the way you manipulate it.
    ''' </summary>
    Public Interface IEffect
        ReadOnly Property Interaction() As EffectInteractions

        Function GetCurrentValue(ByVal control As Control) As Integer

        Sub SetValue(ByVal control As Control, ByVal originalValue As Integer, ByVal valueToReach As Integer, ByVal newValue As Integer)

        Function GetMinimumValue(ByVal control As Control) As Integer

        Function GetMaximumValue(ByVal control As Control) As Integer

    End Interface

End Namespace