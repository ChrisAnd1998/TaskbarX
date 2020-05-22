Option Strict On

Imports System.Runtime.InteropServices
Imports Accessibility

Public Class MSAA

    <DllImport("oleacc.dll")>
    Public Shared Function WindowFromAccessibleObject(ByVal pacc As IAccessible, ByRef phwnd As IntPtr) As UInteger

    End Function

    Declare Function AccessibleChildren Lib "oleacc.dll" (ByVal paccContainer As IAccessible, ByVal iChildStart As Integer, ByVal cChildren As Integer, <[Out]()> ByVal rgvarChildren() As Object, ByRef pcObtained As Integer) As UInteger

    Private Declare Function AccessibleObjectFromWindow Lib "oleacc" (ByVal Hwnd As Int32,
    ByVal dwId As Int32,
    ByRef riid As Guid,
    <MarshalAs(UnmanagedType.IUnknown)> ByRef ppvObject As Object) As Int32
    Declare Function GetForegroundWindow Lib "user32" () As Int32
    Public Shared guidAccessible As New Guid("{00020400-0000-0000-C000-000000000046}")

    Public Shared Function GetAccessibleChildren(ByVal objAccessible As IAccessible) As IAccessible()
        Dim childCount As Integer = 0
        Try
            childCount = objAccessible.accChildCount
        Catch ex As Exception
            childCount = 0
        End Try

        Dim accObjects = New IAccessible((childCount) - 1) {}
        Dim count As Integer = 0
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

End Class