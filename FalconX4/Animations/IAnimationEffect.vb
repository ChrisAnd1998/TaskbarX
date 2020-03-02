Imports FalconX4.VisualEffects.Animations.Effects

Namespace VisualEffects

    Public Interface IEffect

        Sub SetValueX(ByVal originalValue As Double, ByVal valueToReach As Double, ByVal newValue As Double)

        Sub SetValueY(ByVal originalValue As Double, ByVal valueToReach As Double, ByVal newValue As Double)

    End Interface

End Namespace