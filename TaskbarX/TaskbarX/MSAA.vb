Option Strict On

Imports System.Runtime.InteropServices
Imports System.Text
Imports Accessibility

Public Class MSAA

    <DllImport("oleacc.dll")>
    Public Shared Function WindowFromAccessibleObject(ByVal pacc As IAccessible, ByRef phwnd As IntPtr) As UInteger

    End Function

    <DllImport("oleacc.dll")>
    Public Shared Function AccessibleChildren(ByVal paccContainer As IAccessible, ByVal iChildStart As Integer, ByVal cChildren As Integer, <[Out]()> ByVal rgvarChildren() As Object, ByRef pcObtained As Integer) As UInteger

    End Function

    Private Declare Function AccessibleObjectFromWindow Lib "oleacc" (ByVal Hwnd As Int32, ByVal dwId As Int32, ByRef riid As Guid, <MarshalAs(UnmanagedType.IUnknown)> ByRef ppvObject As Object) As Int32

    <DllImport("oleacc.dll")>
    Public Shared Function GetStateText(ByVal dwStateBit As UInteger, <Out> ByVal lpszStateBit As StringBuilder, ByVal cchStateBitMax As UInteger) As UInteger
    End Function

    Public Shared guidAccessible As New Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}")

    Public Shared Function GetAccessibleChildren(ByVal objAccessible As IAccessible) As IAccessible()
        Dim childCount As Integer
        Try
            childCount = objAccessible.accChildCount
        Catch ex As Exception
            childCount = 0
        End Try

        Dim accObjects = New IAccessible((childCount) - 1) {}
        Dim count As Integer = 0

        ''Console.WriteLine(count)

        If (childCount <> 0) Then

            AccessibleChildren(objAccessible, 0, childCount, accObjects, count)
        End If

        Return accObjects
    End Function

    Public Shared Function GetAccessibleObjectFromHandle(ByVal hwnd As IntPtr) As IAccessible
        Dim accObject As Object = New Object
        Dim objAccessible As IAccessible = Nothing
        If (hwnd <> CType(0, IntPtr)) Then
            AccessibleObjectFromWindow(CInt(CType(hwnd, IntPtr)), 0, guidAccessible, accObject)
            objAccessible = CType(accObject, IAccessible)
        End If

        Return objAccessible
    End Function

    Public Shared Function GetStateTextFunc(ByVal stateID As UInteger) As String
        Dim maxLength As UInteger = 1024
        Dim focusableStateText = New StringBuilder(CInt(maxLength))
        Dim sizeableStateText = New StringBuilder(CInt(maxLength))
        Dim moveableStateText = New StringBuilder(CInt(maxLength))
        Dim invisibleStateText = New StringBuilder(CInt(maxLength))
        Dim pressedStateText = New StringBuilder(CInt(maxLength))
        Dim hasPopupStateText = New StringBuilder(CInt(maxLength))

        If stateID = (MSAAStateConstants.STATE_SYSTEM_INVISIBLE Or MSAAStateConstants.STATE_SYSTEM_FOCUSABLE Or MSAAStateConstants.STATE_SYSTEM_HASPOPUP) Then
            GetStateText(MSAAStateConstants.STATE_SYSTEM_INVISIBLE, invisibleStateText, maxLength)
            GetStateText(MSAAStateConstants.STATE_SYSTEM_FOCUSABLE, focusableStateText, maxLength)
            GetStateText(MSAAStateConstants.STATE_SYSTEM_HASPOPUP, hasPopupStateText, maxLength)

            Return invisibleStateText.ToString & "," & focusableStateText.ToString & "," & hasPopupStateText.ToString
        End If

        If stateID = (MSAAStateConstants.STATE_SYSTEM_PRESSED Or MSAAStateConstants.STATE_SYSTEM_INVISIBLE Or MSAAStateConstants.STATE_SYSTEM_FOCUSABLE) Then
            GetStateText(MSAAStateConstants.STATE_SYSTEM_PRESSED, pressedStateText, maxLength)
            GetStateText(MSAAStateConstants.STATE_SYSTEM_INVISIBLE, invisibleStateText, maxLength)
            GetStateText(MSAAStateConstants.STATE_SYSTEM_PRESSED, focusableStateText, maxLength)

            Return pressedStateText.ToString & "," & focusableStateText.ToString & "," & focusableStateText.ToString
        End If

        If stateID = (MSAAStateConstants.STATE_SYSTEM_FOCUSABLE Or MSAAStateConstants.STATE_SYSTEM_HASPOPUP) Then
            GetStateText(MSAAStateConstants.STATE_SYSTEM_FOCUSABLE, focusableStateText, maxLength)
            GetStateText(MSAAStateConstants.STATE_SYSTEM_HASPOPUP, hasPopupStateText, maxLength)

            Return focusableStateText.ToString & "," & hasPopupStateText.ToString
        End If

        If stateID = (MSAAStateConstants.STATE_SYSTEM_FOCUSABLE) Then
            GetStateText(MSAAStateConstants.STATE_SYSTEM_FOCUSABLE, focusableStateText, maxLength)
            Return focusableStateText.ToString
        End If

        Dim stateText = New StringBuilder(CInt(maxLength))
        GetStateText(stateID, stateText, maxLength)
        Return stateText.ToString()
    End Function

End Class

Class MSAAStateConstants

    Public Shared STATE_SYSTEM_ALERT_HIGH As UInteger = 268435456

    Public Shared STATE_SYSTEM_ALERT_LOW As UInteger = 67108864

    Public Shared STATE_SYSTEM_ALERT_MEDIUM As UInteger = 134217728

    Public Shared STATE_SYSTEM_ANIMATED As UInteger = 16384

    Public Shared STATE_SYSTEM_BUSY As UInteger = 2048

    Public Shared STATE_SYSTEM_CHECKED As UInteger = 16

    Public Shared STATE_SYSTEM_COLLAPSED As UInteger = 1024

    Public Shared STATE_SYSTEM_DEFAULT As UInteger = 256

    Public Shared STATE_SYSTEM_EXPANDED As UInteger = 512

    Public Shared STATE_SYSTEM_EXTSELECTABLE As UInteger = 33554432

    Public Shared STATE_SYSTEM_FLOATING As UInteger = 4096

    Public Shared STATE_SYSTEM_FOCUSABLE As UInteger = 1048576

    Public Shared STATE_SYSTEM_FOCUSED As UInteger = 4

    Public Shared STATE_SYSTEM_HASPOPUP As UInteger = 1073741824

    Public Shared STATE_SYSTEM_HOTTRACKED As UInteger = 128

    Public Shared STATE_SYSTEM_INVISIBLE As UInteger = 32768

    Public Shared STATE_SYSTEM_LINKED As UInteger = 4194304

    Public Shared STATE_SYSTEM_MARQUEED As UInteger = 8192

    Public Shared STATE_SYSTEM_MIXED As UInteger = 32

    Public Shared STATE_SYSTEM_MOVEABLE As UInteger = 262144

    Public Shared STATE_SYSTEM_MULTISELECTABLE As UInteger = 16777216

    Public Shared STATE_SYSTEM_NORMAL As UInteger = 0

    Public Shared STATE_SYSTEM_OFFSCREEN As UInteger = 65536

    Public Shared STATE_SYSTEM_PRESSED As UInteger = 8

    Public Shared STATE_SYSTEM_READONLY As UInteger = 64

    Public Shared STATE_SYSTEM_SELECTABLE As UInteger = 2097152

    Public Shared STATE_SYSTEM_SELECTED As UInteger = 2

    Public Shared STATE_SYSTEM_SELFVOICING As UInteger = 524288

    Public Shared STATE_SYSTEM_SIZEABLE As UInteger = 131072

    Public Shared STATE_SYSTEM_TRAVERSED As UInteger = 8388608

    Public Shared STATE_SYSTEM_UNAVAILABLE As UInteger = 1

    Public Shared STATE_SYSTEM_VALID As UInteger = 536870911
End Class