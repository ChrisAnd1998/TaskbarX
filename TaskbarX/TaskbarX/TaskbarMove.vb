Imports System.Runtime.InteropServices

Namespace VisualEffects.Animations.Effects

    Public Class TaskbarMove
        Implements Effect

        <DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
        End Function

        Public SWP_NOSIZE As UInt32 = 1
        Public SWP_ASYNCWINDOWPOS As UInt32 = 16384
        Public SWP_NOACTIVATE As UInt32 = 16
        Public SWP_NOSENDCHANGING As UInt32 = 1024
        Public SWP_NOZORDER As UInt32 = 4

        Public Sub SetValueX(ByVal hwnd As IntPtr, ByVal newValue As Double) Implements Effect.SetValueX
            'Set Horizontal position of the taskbar on each new position calculated by the animator
            SetWindowPos(hwnd, IntPtr.Zero, newValue, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
        End Sub

        Public Sub SetValueY(ByVal hwnd As IntPtr, ByVal newValue As Double) Implements Effect.SetValueY
            'Set Vertical position of the taskbar on each new position calculated by the animator
            SetWindowPos(hwnd, IntPtr.Zero, 0, newValue, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
        End Sub

    End Class

End Namespace