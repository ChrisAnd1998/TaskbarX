Imports System.Threading

Namespace VisualEffects.Animators

    Public Class AnimationStatus
        Inherits EventArgs

        Private ReadOnly _stopwatch As Stopwatch

        Public ReadOnly Property ElapsedMilliseconds() As Long
            Get
                Return _stopwatch.ElapsedMilliseconds
            End Get
        End Property

        Private privateCancellationToken As CancellationTokenSource

        Public Property CancellationToken() As CancellationTokenSource
            Get
                Return privateCancellationToken
            End Get
            Private Set(ByVal value As CancellationTokenSource)
                privateCancellationToken = value
            End Set
        End Property

        Public Property IsCompleted() As Boolean

        Public Sub New(ByVal token As CancellationTokenSource, ByVal stopwatch As Stopwatch)
            Me.CancellationToken = token
            _stopwatch = stopwatch
        End Sub

    End Class

End Namespace