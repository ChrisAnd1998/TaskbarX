Imports System.Text

Public Class BuildString
    Private Declare Function PostMessage Lib "user32.dll" Alias "PostMessageA" (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer

    Public Event StringOK(ByVal Result As String)

    Private hwnd As Integer = 0
    Private wMsg As Integer = 0
    Private wParam As Integer = 0
    Private lParam As String = ""
    Private tempA(-1) As Byte
    Private enc As Encoding = Encoding.UTF8

    Public Property Encode() As Encoding
        Get
            Return enc
        End Get
        Set(ByVal value As Encoding)
            enc = value
        End Set
    End Property

    Public Sub BuildString(ByVal b As IntPtr)
        If b <> 0 Then
            'build temp array
            Dim tempB(tempA.Length) As Byte
            tempA.CopyTo(tempB, 0)
            tempB(tempA.Length) = b
            ReDim tempA(tempB.Length - 1)
            tempB.CopyTo(tempA, 0)
        Else
            'decode byte array to string
            Dim s As String
            If enc Is Encoding.UTF8 Then
                s = Encoding.UTF8.GetString(tempA)
            ElseIf enc Is Encoding.Unicode Then
                s = Encoding.Unicode.GetString(tempA)
            ElseIf enc Is Encoding.ASCII Then
                s = Encoding.ASCII.GetString(tempA)
            Else
                s = Encoding.Default.GetString(tempA)
            End If

            'send out result string via event
            RaiseEvent StringOK(s)
            ReDim tempA(-1)
        End If
    End Sub

    Public Sub PostString(ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As String)
        Me.hwnd = hwnd
        Me.wMsg = wMsg
        Me.wParam = wParam
        Me.lParam = lParam
        'create a new thread to post window message
        Dim t As Threading.Thread
        t = New Threading.Thread(AddressOf SendString)
        t.Start()
    End Sub

    Private Sub SendString()
        'create byte array
        Dim ba() As Byte

        'encode string to byte array
        If enc Is Encoding.UTF8 Then
            ba = Encoding.UTF8.GetBytes(lParam)
        ElseIf enc Is Encoding.Unicode Then
            ba = Encoding.Unicode.GetBytes(lParam)
        ElseIf enc Is Encoding.ASCII Then
            ba = Encoding.ASCII.GetBytes(lParam)
        Else
            ba = Encoding.Default.GetBytes(lParam)
        End If

        Dim i As Integer
        For i = 0 To ba.Length - 1
            'start post message
            PostMessage(hwnd, wMsg, wParam, ba(i))
        Next
        'post a terminator message to destination window
        PostMessage(hwnd, wMsg, wParam, 0)
    End Sub

End Class