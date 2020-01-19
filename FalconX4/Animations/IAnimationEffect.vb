Imports FalconX4.VisualEffects.Animations.Effects

Namespace VisualEffects

    Public Interface IEffect

        Sub SetValueX(ByVal originalValue As Integer, ByVal valueToReach As Integer, ByVal newValue As Integer)

        Sub SetValueY(ByVal originalValue As Integer, ByVal valueToReach As Integer, ByVal newValue As Integer)

    End Interface

End Namespace