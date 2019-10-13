Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Windows.Automation

Imports Microsoft.VisualBasic.CompilerServices

Public Class FalconX

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("user32.dll", ExactSpelling:=True, CharSet:=CharSet.Auto)>
    Public Shared Function GetParent(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function SetParent(ByVal hWndChild As IntPtr, ByVal hWndNewParent As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function SetProcessWorkingSetSize(ByVal hProcess As IntPtr, ByVal dwMinimumWorkingSetSize As Int32, ByVal dwMaximumWorkingSetSize As Int32) As Int32
    End Function

    Public SWP_NOSIZE As UInt32 = 1
    Public SWP_NOMOVE As UInt32 = 2
    Public SWP_NOZORDER As UInt32 = 4
    Public SWP_NOREDRAW As UInt32 = 8
    Public SWP_NOACTIVATE As UInt32 = 16
    Public SWP_DRAWFRAME As UInt32 = 32
    Public SWP_FRAMECHANGED As UInt32 = 32
    Public SWP_SHOWWINDOW As UInt32 = 64
    Public SWP_HIDEWINDOW As UInt32 = 128
    Public SWP_NOCOPYBITS As UInt32 = 256
    Public SWP_NOOWNERZORDER As UInt32 = 512
    Public SWP_NOREPOSITION As UInt32 = 512
    Public SWP_NOSENDCHANGING As UInt32 = 1024
    Public SWP_DEFERERASE As UInt32 = 8192
    Public SWP_ASYNCWINDOWPOS As UInt32 = 16384

    Private Const WM_SETICON = &H80

    Public Const WM_SETREDRAW As Integer = 11
    Private Const WM_PAINT = &HF
    Private Const WM_ERASEBKGND = &H14
    Private Const WM_DESTROY = &H2
    Private Const WM_ENABLE = &HA

    Public HWND_TOP As IntPtr = 0
    Public HWND_BOTTOM As IntPtr = 1
    Public HWND_TOPMOST As IntPtr = -1
    Public HWND_NOTOPMOST As IntPtr = -2

    Public Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    Public Shell_TrayWnd As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Shell_TrayWnd", 0))
    Public MSTaskListWClass As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
    Public TrayNotifyWnd As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "TrayNotifyWnd"))
    Public StartButton As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "Start"))
    Public MSTaskSwWClass = GetParent(MSTaskListWClass.Current.NativeWindowHandle)
    Public ReBarWindow32 = GetParent(MSTaskSwWClass)
    Public Desktop = GetParent(FindWindowByClass("Shell_TrayWnd", 0))

    Public DesktopPtr As IntPtr = Desktop
    Public Shell_TrayWndPtr As IntPtr = Shell_TrayWnd.Current.NativeWindowHandle
    Public MSTaskListWClassPtr As IntPtr = MSTaskListWClass.Current.NativeWindowHandle
    Public StartButtonPtr As IntPtr = StartButton.Current.NativeWindowHandle
    Public TrayNotifyWndPtr As IntPtr = TrayNotifyWnd.Current.NativeWindowHandle
    Public MSTaskSwWClassPtr As IntPtr = MSTaskSwWClass
    Public ReBarWindow32Ptr As IntPtr = ReBarWindow32

    Public TaskbarWidthFull As Integer
    Public TaskbarLeft As Integer
    Public SecondaryTaskbarLeft As Integer
    Public IsTaskbarMoving As Boolean
    Public TaskbarNewPos As Integer
    Public SecondaryTaskbarNewPos As Integer
    Public Launch As Boolean
    Public UpdateTaskbar As Boolean
    Public Horizontal As Boolean
    Public StickyStartButton As Boolean
    Public StartButtonWidth As Integer
    Public StartButtonHeight As Integer
    Public StickyTray As Boolean
    Public TrayBarWidth As Integer
    Public TrayBarHeight As Integer
    Public StartUp As Boolean

    Public SecondTaskbarPtr As IntPtr

    Public SecondTaskbarWidth As Integer
    Public SecondTaskbarPos As Integer

    Public ThirdTaskbarPtr As IntPtr

    Public ThirdTaskbarWidth As Integer
    Public ThirdTaskbarPos As Integer

    Sub RestartExplorer()
        For Each MyProcess In Process.GetProcessesByName("explorer")
            MyProcess.Kill()
        Next
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If Screen.AllScreens.LongCount = 1 Then
            CheckBox2.Enabled = False
        End If

        StartUp = True

        MainAnimator.Show()
        SecondaryAnimator2.Show()
        SecondaryAnimator3.Show()

        Try
            If Application.StartupPath.Contains("40210ChrisAndriessen") Then
                CheckBox7.Visible = False
                CheckBox7.Checked = False
                Dim strx As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
                If File.Exists(strx + "\FalconX.lnk") Then
                    Dim str As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
                    File.Delete(str + "\FalconX.lnk")
                End If

            End If
        Catch ex As Exception
            ' Console.WriteLine(ex)
        End Try

        Launch = True

        If ComboBox1.Text = Nothing Then
            ComboBox1.Text = "QuadEaseOut"
        End If

        RunAtStartUp()

        Dim CurrentProcess As Process = Process.GetCurrentProcess
        CurrentProcess.PriorityClass = ProcessPriorityClass.BelowNormal

        SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, False, 0)

        IsTaskbarMoving = False

        Dim TitlebarHeight As Integer = Me.Height - Me.ClientSize.Height - 2
        Me.Size = New Size(Me.Width, Button2.Location.Y + TitlebarHeight + Button2.Height + 14)

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf TaskbarCalculator)
        t1.Start()

    End Sub

    Sub TaskbarCalculator()
        Do

            Try

                Dim Laps As Integer
                Dim Trigger As Integer
                Dim TaskbarWidth As Integer = 0
                Dim OldTaskbarCount As Integer
                Dim TaskbarCount As Integer = 0

                Dim OldTrayWidth As Integer
                Dim TrayWidth As Integer = 0

                Dim Resolution As Integer = 0
                Dim OldResolution As Integer
                Dim tw As TreeWalker = TreeWalker.ControlViewWalker
                Dim child As AutomationElement = tw.GetLastChild(MSTaskListWClass)

                tw = Nothing

                If MSTaskListWClass.Current.BoundingRectangle.Height >= 200 Then
                    If Horizontal = True Then
                        UpdateTaskbar = True

                    End If
                    Horizontal = False
                Else
                    If Horizontal = False Then
                        UpdateTaskbar = True

                    End If
                    Horizontal = True
                End If

                If Horizontal = False Then
                    TaskbarCount = child.Current.BoundingRectangle.Top
                    Resolution = Screen.PrimaryScreen.Bounds.Height
                    TrayWidth = Resolution - TrayNotifyWnd.Current.BoundingRectangle.Height
                Else
                    TaskbarCount = child.Current.BoundingRectangle.Left
                    Resolution = Screen.PrimaryScreen.Bounds.Width
                    TrayWidth = TrayNotifyWnd.Current.BoundingRectangle.Left
                End If

                System.Threading.Thread.Sleep(NumericUpDown3.Value)
                If Not TaskbarCount = OldTaskbarCount Or Not Resolution = OldResolution Or Not TrayWidth = OldTrayWidth Or UpdateTaskbar = True Then

                    OldTaskbarCount = TaskbarCount
                    OldResolution = Resolution
                    OldTrayWidth = TrayWidth

                    UpdateTaskbar = False

                    Dim TreeWalker1 As TreeWalker = TreeWalker.ControlViewWalker
                    Dim BChildFirst1 As AutomationElement = TreeWalker1.GetFirstChild(MSTaskListWClass)
                    Dim BChildLast1 As AutomationElement = TreeWalker1.GetLastChild(MSTaskListWClass)

                    If Horizontal = True Then
                        TaskbarWidth = (BChildFirst1.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + (BChildLast1.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + BChildLast1.Current.BoundingRectangle.Width
                    Else
                        TaskbarWidth = (BChildFirst1.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + (BChildLast1.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + BChildLast1.Current.BoundingRectangle.Height

                    End If

                    TreeWalker1 = Nothing

                    ' Dim rct As RECT
                    ' GetWindowRect(ReBarWindow32Ptr, rct)

                    Dim RebarWnd As AutomationElement = AutomationElement.FromHandle(GetParent(MSTaskListWClassPtr))

                    If Horizontal = False Then
                        TaskbarLeft = RebarWnd.Current.BoundingRectangle.Top
                    Else
                        TaskbarLeft = RebarWnd.Current.BoundingRectangle.Left
                    End If

                    TaskbarWidthFull = TaskbarWidth
                    Dim TaskbarWidthHalf = TaskbarWidthFull / 2
                    Dim position As Integer

                    If Horizontal = True Then
                        If CheckBox1.Checked = True Then
                            Dim offset = (TrayNotifyWnd.Current.BoundingRectangle.Width / 2 - (TaskbarLeft \ 2))
                            position = (Screen.PrimaryScreen.Bounds.Width / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value - 4 - offset).ToString.Replace("-", "")
                        Else
                            position = (Screen.PrimaryScreen.Bounds.Width / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value - 4).ToString.Replace("-", "")
                        End If
                    Else
                        If CheckBox1.Checked = True Then
                            Dim offset = (TrayNotifyWnd.Current.BoundingRectangle.Height / 2 - (TaskbarLeft \ 2))
                            position = (Screen.PrimaryScreen.Bounds.Height / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value - 4 - offset).ToString.Replace("-", "")
                        Else
                            position = (Screen.PrimaryScreen.Bounds.Height / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value - 4).ToString.Replace("-", "")
                        End If
                    End If

                    TaskbarNewPos = position

                    If CheckBox2.Checked = True Then

                        If Screen.AllScreens.LongCount >= 1 Then

                            Dim desktops As AutomationElement = AutomationElement.RootElement
                            Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))

                            Dim ScreenCount As Integer

                            ScreenCount = 0

                            For Each trayWnd As AutomationElement In lists
                                ScreenCount = ScreenCount + 1

                                Dim tasklist As AutomationElement = trayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
                                Dim taskptr As IntPtr = tasklist.Current.NativeWindowHandle

                                If ScreenCount = 1 Then
                                    SecondTaskbarPtr = taskptr
                                    SecondTaskbarWidth = 0

                                    Dim TreeWalker2 As TreeWalker = TreeWalker.ControlViewWalker
                                    Dim BChildFirst2 As AutomationElement = TreeWalker2.GetFirstChild(MSTaskListWClass)
                                    Dim BChildLast2 As AutomationElement = TreeWalker2.GetLastChild(MSTaskListWClass)

                                    If Horizontal = True Then
                                        SecondTaskbarWidth = (BChildFirst2.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + (BChildLast2.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + BChildLast2.Current.BoundingRectangle.Width
                                    Else
                                        SecondTaskbarWidth = (BChildFirst2.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + (BChildLast2.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + BChildLast2.Current.BoundingRectangle.Height

                                    End If

                                    TreeWalker2 = Nothing

                                    Dim WorkerW As AutomationElement = AutomationElement.FromHandle(GetParent(taskptr))

                                    Dim SecondTaskbarLeft As Integer

                                    If Horizontal = False Then
                                        SecondTaskbarLeft = WorkerW.Current.BoundingRectangle.Top
                                    Else

                                        SecondTaskbarLeft = WorkerW.Current.BoundingRectangle.Left - trayWnd.Current.BoundingRectangle.Left

                                    End If

                                    Dim SecondTaskbarWidthHalf = SecondTaskbarWidth / 2

                                    If Horizontal = True Then
                                        SecondTaskbarPos = (trayWnd.Current.BoundingRectangle.Width / 2 - SecondTaskbarWidthHalf - SecondTaskbarLeft + NumericUpDown2.Value - 4).ToString.Replace("-", "")
                                    Else
                                        SecondTaskbarPos = (trayWnd.Current.BoundingRectangle.Height / 2 - SecondTaskbarWidthHalf - SecondTaskbarLeft + NumericUpDown2.Value - 4).ToString.Replace("-", "")
                                    End If

                                End If

                                If ScreenCount = 2 Then
                                    ThirdTaskbarPtr = taskptr
                                    ThirdTaskbarWidth = 0

                                    Dim TreeWalker3 As TreeWalker = TreeWalker.ControlViewWalker
                                    Dim BChildFirst3 As AutomationElement = TreeWalker3.GetFirstChild(MSTaskListWClass)
                                    Dim BChildLast3 As AutomationElement = TreeWalker3.GetLastChild(MSTaskListWClass)

                                    If Horizontal = True Then
                                        ThirdTaskbarWidth = (BChildFirst3.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + (BChildLast3.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + BChildLast3.Current.BoundingRectangle.Width
                                    Else
                                        ThirdTaskbarWidth = (BChildFirst3.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + (BChildLast3.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + BChildLast3.Current.BoundingRectangle.Height

                                    End If

                                    TreeWalker3 = Nothing

                                    Dim WorkerW As AutomationElement = AutomationElement.FromHandle(GetParent(taskptr))

                                    Dim ThirdTaskbarLeft As Integer

                                    If Horizontal = False Then

                                        ThirdTaskbarLeft = WorkerW.Current.BoundingRectangle.Top
                                    Else

                                        ThirdTaskbarLeft = WorkerW.Current.BoundingRectangle.Left - trayWnd.Current.BoundingRectangle.Left

                                    End If

                                    Dim ThirdTaskbarWidthHalf = ThirdTaskbarWidth / 2

                                    If Horizontal = True Then
                                        ThirdTaskbarPos = (trayWnd.Current.BoundingRectangle.Width / 2 - ThirdTaskbarWidthHalf - ThirdTaskbarLeft + NumericUpDown2.Value - 4).ToString.Replace("-", "")
                                    Else
                                        ThirdTaskbarPos = (trayWnd.Current.BoundingRectangle.Height / 2 - ThirdTaskbarWidthHalf - ThirdTaskbarLeft + NumericUpDown2.Value - 4).ToString.Replace("-", "")
                                    End If

                                End If

                            Next
                        End If

                    End If

                    Me.Invoke(Sub()
                                  Label1.Text = position
                              End Sub)

                End If

                Laps = Laps + 1

                If Laps = 50 Then
                    Laps = 0

                    SaveMemory()
                End If
            Catch ex As Exception

                If ex.ToString.Contains("E_ACCESSDENIED") Then

                    Dim Handle As IntPtr
                    Dim Laps2 As Integer

                    SaveMemory()

                    Do

                        Laps2 = Laps2 + 1

                        If Laps2 = 50 Then
                            Laps2 = 0
                            SaveMemory()
                        End If

                        Handle = Nothing
                        System.Threading.Thread.Sleep(250)
                        Handle = FindWindowByClass("Shell_TrayWnd", 0)

                    Loop Until Not Handle = Nothing

                    NotifyIcon1.Visible = False
                    Application.Restart()
                    End
                End If

            End Try

        Loop
    End Sub

    Private Sub Label1_TextChanged(sender As Object, e As EventArgs) Handles Label1.TextChanged

        If MainAnimator.Visible = True Then

            If CheckBox2.Checked = True Then

                Dim desktops As AutomationElement = AutomationElement.RootElement
                Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))

                Dim ScreenCount As Integer
                ScreenCount = 0
                If Screen.AllScreens.LongCount >= 1 Then

                    For Each trayWnd As AutomationElement In lists

                        ScreenCount = ScreenCount + 1

                        If ScreenCount = 1 Then
                            SecondaryAnimator2.AnimatorMove()
                        End If

                        If ScreenCount = 2 Then
                            SecondaryAnimator3.AnimatorMove()
                        End If

                    Next

                End If
            End If

            MainAnimator.AnimatorMove()

        End If

    End Sub

    Public Function SaveMemory() As Int32

        Return SetProcessWorkingSetSize(Diagnostics.Process.GetCurrentProcess.Handle, 2097152, 2097152)

    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        My.Settings.Save()

        SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, True, 0)

        SetWindowPos(MSTaskListWClassPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)
        SetWindowPos(SecondTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)
        SetWindowPos(ThirdTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)

        NotifyIcon1.Visible = False

        Me.Close()
        End
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://chrisandriessen.nl")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        My.Settings.Save()
        Me.Hide()
        Me.Opacity = 0

    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.Click

        Me.Opacity = 100
        Me.Show()

    End Sub

    Sub RestartApp()
        SetWindowPos(MSTaskListWClassPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)
        SetWindowPos(SecondTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)
        SetWindowPos(ThirdTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)

        NotifyIcon1.Visible = False
        Application.Restart()
        End
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        My.Settings.Save()
        RestartApp()

    End Sub

    Sub RunAtStartUp()

        Try

            If Application.StartupPath.Contains("40210ChrisAndriessen") Then
                Exit Sub
            End If

            Dim regKey As Microsoft.Win32.RegistryKey
            regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
            regKey.DeleteValue(Application.ProductName, False)
            regKey.Close()

            Dim strx As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"

            If File.Exists(strx + "\FalconX.lnk") Then
                Dim str As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
                File.Delete(str + "\FalconX.lnk")
            End If

            If CheckBox7.Checked = True Then
                Dim objectValue As Object = RuntimeHelpers.GetObjectValue(Interaction.CreateObject("WScript.Shell", ""))
                objectValue = RuntimeHelpers.GetObjectValue(Interaction.CreateObject("WScript.Shell", ""))
                Dim objectValue2 As Object = RuntimeHelpers.GetObjectValue(NewLateBinding.LateGet(objectValue, Nothing, "SpecialFolders", New Object() {"Startup"}, Nothing, Nothing, Nothing))
                Dim objectValue3 As Object = RuntimeHelpers.GetObjectValue(NewLateBinding.LateGet(objectValue, Nothing, "CreateShortcut", New Object() {Operators.ConcatenateObject(objectValue2, "\FalconX.lnk")}, Nothing, Nothing, Nothing))
                NewLateBinding.LateSet(objectValue3, Nothing, "TargetPath", New Object() {NewLateBinding.LateGet(objectValue, Nothing, "ExpandEnvironmentStrings", New Object() {Application.ExecutablePath}, Nothing, Nothing, Nothing)}, Nothing, Nothing)
                NewLateBinding.LateSet(objectValue3, Nothing, "WorkingDirectory", New Object() {NewLateBinding.LateGet(objectValue, Nothing, "ExpandEnvironmentStrings", New Object() {Application.StartupPath}, Nothing, Nothing, Nothing)}, Nothing, Nothing)
                NewLateBinding.LateSet(objectValue3, Nothing, "WindowStyle", New Object() {4}, Nothing, Nothing)
                NewLateBinding.LateCall(objectValue3, Nothing, "Save", New Object(-1) {}, Nothing, Nothing, Nothing, True)
            End If
        Catch ex As Exception

        End Try

        My.Settings.Save()

    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged
        My.Settings.Save()

        RunAtStartUp()

    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown

        If Launch = True Then
            Me.Hide()
            Launch = False
        End If

        Me.Focus()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        UpdateTaskbar = True
    End Sub

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        UpdateTaskbar = True
    End Sub

    Private Sub NumericUpDown2_KeyDown(sender As Object, e As KeyEventArgs) Handles NumericUpDown2.KeyDown
        UpdateTaskbar = True
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Select Case MsgBox("Are you Sure?", MsgBoxStyle.YesNo, "Reset settings...")
            Case MsgBoxResult.Yes
                ComboBox1.Text = "QuadEaseOut"
                NumericUpDown1.Value = 250
                NumericUpDown2.Value = 0
                NumericUpDown3.Value = 400
                CheckBox1.Checked = False
            Case MsgBoxResult.No

        End Select
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        My.Settings.Save()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Process.Start("https://easings.net/")
        My.Settings.Save()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Process.Start("https://github.com/ChrisAnd1998/FalconX/issues")
        My.Settings.Save()
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        My.Settings.Save()
        RestartExplorer()
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged

        If CheckBox2.Checked = False Then
            SetWindowPos(SecondTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)
            SetWindowPos(ThirdTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)

        End If

        UpdateTaskbar = True

        My.Settings.Save()

    End Sub

End Class