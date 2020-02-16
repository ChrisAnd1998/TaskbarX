Imports System.Runtime.InteropServices

Namespace VisualEffects.Animations.Effects

    Public Class XLocationEffect2
        Implements IEffect

        <DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
        End Function

        Public SWP_NOSIZE As UInt32 = 1
        Public SWP_ASYNCWINDOWPOS As UInt32 = 16384
        Public SWP_NOACTIVATE As UInt32 = 16
        Public SWP_NOSENDCHANGING As UInt32 = 1024
        Public SWP_NOZORDER As UInt32 = 4
        Public Shared SecondTaskbarPtr As IntPtr = CType(0, IntPtr)
        Public Shared SecondTaskbarPosition As Integer = 0
        Public Shared SecondTaskbarOldPosition As Integer = 0

        Public Sub SetValuex(ByVal originalValue As Integer, ByVal valueToReach As Integer, ByVal newValue As Integer) Implements IEffect.SetValueX
            SetWindowPos(SecondTaskbarPtr, IntPtr.Zero, newValue, Taskbar.YforHTaskbar, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
        End Sub

        Public Sub SetValuey(ByVal originalValue As Integer, ByVal valueToReach As Integer, ByVal newValue As Integer) Implements IEffect.SetValueY
            SetWindowPos(SecondTaskbarPtr, IntPtr.Zero, Taskbar.XforVTaskbar, newValue, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
        End Sub

    End Class

End Namespace