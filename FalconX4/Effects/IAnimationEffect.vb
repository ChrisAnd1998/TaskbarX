Imports FalconX4.VisualEffects.Animations.Effects

Namespace VisualEffects

    Public Interface IEffect
        ReadOnly Property Interaction() As EffectInteractions

        Function GetCurrentValue(ByVal control As Control) As Integer

        Sub SetValue(ByVal control As Control, ByVal originalValue As Integer, ByVal valueToReach As Integer, ByVal newValue As Integer)

        Function GetMinimumValue(ByVal control As Control) As Integer

        Function GetMaximumValue(ByVal control As Control) As Integer

    End Interface

End Namespace