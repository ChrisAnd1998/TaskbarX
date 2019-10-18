Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
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

    <DllImport("user32.dll")>
    Private Shared Function SetWindowCompositionAttribute(ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    End Function

    Friend Structure WindowCompositionAttributeData
        Public Attribute As WindowCompositionAttribute
        Public Data As IntPtr
        Public SizeOfData As Integer
    End Structure

    Friend Enum WindowCompositionAttribute
        WCA_ACCENT_POLICY = 19
    End Enum

    Friend Enum AccentState
        ACCENT_DISABLED = 0
        ACCENT_ENABLE_GRADIENT = 1
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2
        ACCENT_ENABLE_BLURBEHIND = 3
        ACCENT_ENABLE_TRANSPARANT = 6
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        ACCENT_NORMAL = 150
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure AccentPolicy
        Public AccentState As AccentState
        Public AccentFlags As Integer
        Public GradientColor As Integer
        Public AnimationId As Integer
    End Structure

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

    Public Const WM_SETREDRAW As Integer = 11

    Public Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    Public Shell_TrayWnd As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr)))
    Public MSTaskListWClass As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
    Public TrayNotifyWnd As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "TrayNotifyWnd"))
    Public StartButton As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "Start"))
    Public MSTaskSwWClass As IntPtr = GetParent(CType(MSTaskListWClass.Current.NativeWindowHandle, IntPtr))
    Public ReBarWindow32 As IntPtr = GetParent(MSTaskSwWClass)
    Public Desktop As IntPtr = GetParent(FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr)))

    Public DesktopPtr As IntPtr = Desktop
    Public Shell_TrayWndPtr As IntPtr = CType(Shell_TrayWnd.Current.NativeWindowHandle, IntPtr)
    Public MSTaskListWClassPtr As IntPtr = CType(MSTaskListWClass.Current.NativeWindowHandle, IntPtr)
    Public StartButtonPtr As IntPtr = CType(StartButton.Current.NativeWindowHandle, IntPtr)
    Public TrayNotifyWndPtr As IntPtr = CType(TrayNotifyWnd.Current.NativeWindowHandle, IntPtr)
    Public MSTaskSwWClassPtr As IntPtr = MSTaskSwWClass
    Public ReBarWindow32Ptr As IntPtr = ReBarWindow32

    Dim TaskbarTreeWalker As TreeWalker = TreeWalker.ControlViewWalker

    Public TaskbarWidthFull As Integer = 0
    Public TaskbarLeft As Integer = 0
    Public SecondaryTaskbarLeft As Integer = 0
    Public IsTaskbarMoving As Boolean = False
    Public TaskbarNewPos As Integer = 0
    Public SecondaryTaskbarNewPos As Integer = 0
    Public Launch As Boolean = False
    Public UpdateTaskbar As Boolean = False
    Public Horizontal As Boolean = False
    Public StickyStartButton As Boolean = False
    Public StartButtonWidth As Integer = 0
    Public StartButtonHeight As Integer = 0
    Public StickyTray As Boolean = False
    Public TrayBarWidth As Integer = 0
    Public TrayBarHeight As Integer = 0
    Public StartUp As Boolean = False

    Public SecondTaskbarPtr As IntPtr = CType(0, IntPtr)

    Public SecondTaskbarWidth As Integer = 0
    Public SecondTaskbarPos As Integer = 0

    Public ThirdTaskbarPtr As IntPtr = CType(0, IntPtr)

    Public ThirdTaskbarWidth As Integer = 0
    Public ThirdTaskbarPos As Integer = 0

    Sub RestartExplorer()
        For Each MyProcess In Process.GetProcessesByName("explorer")
            MyProcess.Kill()
        Next
    End Sub

    Sub LoadSettings()

        Try

            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\FalconX.cfg"

            If File.Exists(path) Then

                ComboBox1.Text = System.IO.File.ReadAllLines(path)(0)
                NumericUpDown1.Value = CDec(System.IO.File.ReadAllLines(path)(1))
                NumericUpDown3.Value = CDec(System.IO.File.ReadAllLines(path)(2))
                NumericUpDown2.Value = CDec(File.ReadAllLines(path)(3))

                If System.IO.File.ReadAllLines(path)(4) = "True" Then
                    CheckBox7.Checked = True
                Else
                    CheckBox7.Checked = False
                End If

                If System.IO.File.ReadAllLines(path)(5) = "True" Then
                    CheckBox1.Checked = True
                Else
                    CheckBox1.Checked = False
                End If

                If System.IO.File.ReadAllLines(path)(6) = "True" Then
                    CheckBox2.Checked = True
                Else
                    CheckBox2.Checked = False
                End If

                If System.IO.File.ReadAllLines(path)(7) = "True" Then
                    CheckBox3.Checked = True
                Else
                    CheckBox3.Checked = False
                End If

            End If
        Catch
        End Try

    End Sub

    Sub SaveSettings()
        Try

            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\FalconX.cfg"
            Dim fs As FileStream = File.Create(path)

            Dim Animation = ComboBox1.Text
            Dim Speed = NumericUpDown1.Value
            Dim Rate = NumericUpDown3.Value
            Dim Offset = NumericUpDown2.Value
            Dim RunAtStartUp As Boolean
            Dim Transparant As Boolean
            Dim CBT As Boolean
            Dim CMM As Boolean

            If CheckBox7.Checked = False Then
                RunAtStartUp = False
            Else
                RunAtStartUp = True
            End If

            If CheckBox3.Checked = False Then
                Transparant = False
            Else
                Transparant = True
            End If

            If CheckBox1.Checked = False Then
                CBT = False
            Else
                CBT = True
            End If

            If CheckBox2.Checked = False Then
                CMM = False
            Else
                CMM = True
            End If

            ' Add text to the file.
            Dim info As Byte() = New UTF8Encoding(True).GetBytes(Animation.ToString & Environment.NewLine & Speed.ToString & Environment.NewLine & Rate.ToString & Environment.NewLine & Offset.ToString & Environment.NewLine & RunAtStartUp.ToString & Environment.NewLine & CBT.ToString & Environment.NewLine & CMM.ToString & Environment.NewLine & Transparant.ToString)

            fs.Write(info, 0, info.Length)
            fs.Close()
        Catch
        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try

            Try
                Dim strx2 As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\Chris_Andriessen"

                If Directory.Exists(strx2) Then
                    Directory.Delete(strx2, True)
                End If
            Catch
            End Try

            LoadSettings()

            System.Threading.Thread.Sleep(500) : Application.DoEvents()

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
                    Button8.Visible = False
                    Dim strx As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
                    If File.Exists(strx + "\FalconX.lnk") Then
                        Dim str As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
                        File.Delete(str + "\FalconX.lnk")
                    End If

                End If
            Catch

            End Try

            Launch = True

            If ComboBox1.Text = Nothing Then
                ComboBox1.Text = "QuadEaseOut"
            End If

            RunAtStartUp()

            Dim CurrentProcess As Process = Process.GetCurrentProcess
            CurrentProcess.PriorityClass = ProcessPriorityClass.Normal

            SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, False, 0)

            IsTaskbarMoving = False

            Dim TitlebarHeight As Integer = Me.Height - Me.ClientSize.Height - 2
            Me.Size = New Size(Me.Width, Button2.Location.Y + TitlebarHeight + Button2.Height + 14)

            Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf TaskbarCalculator)
            t1.Start()
        Catch
        End Try

    End Sub

    Sub TaskbarCalculator()

        Do

            Try

                ''#############################################################################

                Dim Laps As Integer

                Dim TaskbarWidth As Integer = 0
                Dim OldTaskbarCount As Integer
                Dim TaskbarCount As Integer = 0

                Dim OldTrayWidth As Integer
                Dim TrayWidth As Integer = 0

                Dim Resolution As Integer = 0
                Dim OldResolution As Integer

                Dim TaskbarTreeWalker As TreeWalker = TreeWalker.ControlViewWalker
                Dim child As AutomationElement = TaskbarTreeWalker.GetLastChild(MSTaskListWClass)

                System.Threading.Thread.Sleep(CInt(NumericUpDown3.Value / 2))

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
                    TaskbarCount = CInt(child.Current.BoundingRectangle.Top)
                    Resolution = Screen.PrimaryScreen.Bounds.Height

                    If CheckBox1.Checked = True Then
                        TrayWidth = CInt(Resolution - TrayNotifyWnd.Current.BoundingRectangle.Height)
                    End If
                Else
                    TaskbarCount = CInt(child.Current.BoundingRectangle.Left)
                    Resolution = Screen.PrimaryScreen.Bounds.Width

                    If CheckBox1.Checked = True Then
                        TrayWidth = CInt(TrayNotifyWnd.Current.BoundingRectangle.Left)
                    End If

                End If

                ''#############################################################################

                System.Threading.Thread.Sleep(CInt(NumericUpDown3.Value / 2))
                If Not TaskbarCount = OldTaskbarCount Or Not Resolution = OldResolution Or Not TrayWidth = OldTrayWidth Or UpdateTaskbar = True Then

                    Console.WriteLine("Taskbar Calculate")

                    OldTaskbarCount = TaskbarCount
                    OldResolution = Resolution
                    OldTrayWidth = TrayWidth

                    UpdateTaskbar = False

                    Dim TreeWalker1 As TreeWalker = TreeWalker.ControlViewWalker
                    Dim BChildFirst1 As AutomationElement = TreeWalker1.GetFirstChild(MSTaskListWClass)
                    Dim BChildLast1 As AutomationElement = TreeWalker1.GetLastChild(MSTaskListWClass)

                    If Horizontal = True Then
                        TaskbarWidth = CInt((BChildFirst1.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + (BChildLast1.Current.BoundingRectangle.Left - MSTaskListWClass.Current.BoundingRectangle.Left) + BChildLast1.Current.BoundingRectangle.Width)
                    Else
                        TaskbarWidth = CInt((BChildFirst1.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + (BChildLast1.Current.BoundingRectangle.Top - MSTaskListWClass.Current.BoundingRectangle.Top) + BChildLast1.Current.BoundingRectangle.Height)

                    End If

                    TreeWalker1 = Nothing

                    Dim RebarWnd As AutomationElement = AutomationElement.FromHandle(GetParent(MSTaskListWClassPtr))

                    If Horizontal = False Then
                        TaskbarLeft = CInt(RebarWnd.Current.BoundingRectangle.Top)
                    Else
                        TaskbarLeft = CInt(RebarWnd.Current.BoundingRectangle.Left)
                    End If

                    TaskbarWidthFull = TaskbarWidth
                    Dim TaskbarWidthHalf = TaskbarWidthFull / 2
                    Dim position As Integer

                    If Horizontal = True Then
                        If CheckBox1.Checked = True Then
                            Dim offset = (TrayNotifyWnd.Current.BoundingRectangle.Width / 2 - (TaskbarLeft \ 2))
                            position = CInt((Screen.PrimaryScreen.Bounds.Width / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value - offset).ToString.Replace("-", ""))
                        Else
                            position = CInt((Screen.PrimaryScreen.Bounds.Width / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value).ToString.Replace("-", ""))
                        End If
                    Else
                        If CheckBox1.Checked = True Then
                            Dim offset = (TrayNotifyWnd.Current.BoundingRectangle.Height / 2 - (TaskbarLeft \ 2))
                            position = CInt((Screen.PrimaryScreen.Bounds.Height / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value - offset).ToString.Replace("-", ""))
                        Else
                            position = CInt((Screen.PrimaryScreen.Bounds.Height / 2 - TaskbarWidthHalf - TaskbarLeft + NumericUpDown2.Value).ToString.Replace("-", ""))
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
                                ScreenCount += 1

                                Dim tasklist As AutomationElement = trayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
                                Dim taskptr As IntPtr = CType(tasklist.Current.NativeWindowHandle, IntPtr)

                                If ScreenCount = 1 Then
                                    SecondTaskbarPtr = taskptr
                                    SecondTaskbarWidth = 0

                                    Dim TreeWalker2 As TreeWalker = TreeWalker.ControlViewWalker
                                    Dim BChildFirst2 As AutomationElement = TreeWalker2.GetFirstChild(tasklist)
                                    Dim BChildLast2 As AutomationElement = TreeWalker2.GetLastChild(tasklist)

                                    If Horizontal = True Then
                                        SecondTaskbarWidth = CInt((BChildFirst2.Current.BoundingRectangle.Left - tasklist.Current.BoundingRectangle.Left) + (BChildLast2.Current.BoundingRectangle.Left - tasklist.Current.BoundingRectangle.Left) + BChildLast2.Current.BoundingRectangle.Width)
                                    Else
                                        SecondTaskbarWidth = CInt((BChildFirst2.Current.BoundingRectangle.Top - tasklist.Current.BoundingRectangle.Top) + (BChildLast2.Current.BoundingRectangle.Top - tasklist.Current.BoundingRectangle.Top) + BChildLast2.Current.BoundingRectangle.Height)

                                    End If

                                    TreeWalker2 = Nothing

                                    Dim WorkerW As AutomationElement = AutomationElement.FromHandle(GetParent(taskptr))

                                    Dim SecondTaskbarLeft As Integer

                                    If Horizontal = False Then
                                        SecondTaskbarLeft = CInt(WorkerW.Current.BoundingRectangle.Top)
                                    Else

                                        SecondTaskbarLeft = CInt(WorkerW.Current.BoundingRectangle.Left - trayWnd.Current.BoundingRectangle.Left)

                                    End If

                                    Dim SecondTaskbarWidthHalf = SecondTaskbarWidth / 2

                                    If Horizontal = True Then
                                        SecondTaskbarPos = CInt((trayWnd.Current.BoundingRectangle.Width / 2 - SecondTaskbarWidthHalf - SecondTaskbarLeft + NumericUpDown2.Value).ToString.Replace("-", ""))
                                    Else
                                        SecondTaskbarPos = CInt((trayWnd.Current.BoundingRectangle.Height / 2 - SecondTaskbarWidthHalf - SecondTaskbarLeft + NumericUpDown2.Value).ToString.Replace("-", ""))
                                    End If

                                End If

                                If ScreenCount = 2 Then
                                    ThirdTaskbarPtr = taskptr
                                    ThirdTaskbarWidth = 0

                                    Dim TreeWalker3 As TreeWalker = TreeWalker.ControlViewWalker
                                    Dim BChildFirst3 As AutomationElement = TreeWalker3.GetFirstChild(tasklist)
                                    Dim BChildLast3 As AutomationElement = TreeWalker3.GetLastChild(tasklist)

                                    If Horizontal = True Then
                                        ThirdTaskbarWidth = CInt((BChildFirst3.Current.BoundingRectangle.Left - tasklist.Current.BoundingRectangle.Left) + (BChildLast3.Current.BoundingRectangle.Left - tasklist.Current.BoundingRectangle.Left) + BChildLast3.Current.BoundingRectangle.Width)
                                    Else
                                        ThirdTaskbarWidth = CInt((BChildFirst3.Current.BoundingRectangle.Top - tasklist.Current.BoundingRectangle.Top) + (BChildLast3.Current.BoundingRectangle.Top - tasklist.Current.BoundingRectangle.Top) + BChildLast3.Current.BoundingRectangle.Height)

                                    End If

                                    TreeWalker3 = Nothing

                                    Dim WorkerW As AutomationElement = AutomationElement.FromHandle(GetParent(taskptr))

                                    Dim ThirdTaskbarLeft As Integer

                                    If Horizontal = False Then

                                        ThirdTaskbarLeft = CInt(WorkerW.Current.BoundingRectangle.Top)
                                    Else

                                        ThirdTaskbarLeft = CInt(WorkerW.Current.BoundingRectangle.Left - trayWnd.Current.BoundingRectangle.Left)

                                    End If

                                    Dim ThirdTaskbarWidthHalf = ThirdTaskbarWidth / 2

                                    If Horizontal = True Then
                                        ThirdTaskbarPos = CInt((trayWnd.Current.BoundingRectangle.Width / 2 - ThirdTaskbarWidthHalf - ThirdTaskbarLeft + NumericUpDown2.Value).ToString.Replace("-", ""))
                                    Else
                                        ThirdTaskbarPos = CInt((trayWnd.Current.BoundingRectangle.Height / 2 - ThirdTaskbarWidthHalf - ThirdTaskbarLeft + NumericUpDown2.Value).ToString.Replace("-", ""))
                                    End If

                                End If

                            Next
                        End If

                    End If

                    Me.Invoke(Sub()

                                  Label1.Text = CType(position, String)

                              End Sub)

                End If

                Laps += 1

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

                        Laps2 += 1

                        If Laps2 = 50 Then
                            Laps2 = 0
                            SaveMemory()
                        End If

                        Handle = Nothing
                        System.Threading.Thread.Sleep(250)
                        Handle = FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))

                    Loop Until Not Handle = Nothing

                    NotifyIcon1.Visible = False
                    Application.Restart()
                    End
                End If

            End Try

        Loop
    End Sub

    Private Sub Label1_TextChanged(sender As Object, e As EventArgs) Handles Label1.TextChanged

        Try

            Console.WriteLine("Taskbar Update")
            ' Sub AnimatorMove()
            If MainAnimator.Visible = True Then

                If CheckBox2.Checked = True Then

                    Dim desktops As AutomationElement = AutomationElement.RootElement
                    Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))

                    Dim ScreenCount As Integer
                    ScreenCount = 0
                    If Screen.AllScreens.LongCount >= 1 Then

                        For Each trayWnd As AutomationElement In lists

                            ScreenCount += 1

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
        Catch
        End Try
    End Sub

    Public Function SaveMemory() As Int32

        Return SetProcessWorkingSetSize(Diagnostics.Process.GetCurrentProcess.Handle, 2097152, 2097152)

    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SaveSettings()

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

        SaveSettings()

        Me.Hide()
        Me.Opacity = 0

    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As EventArgs) Handles NotifyIcon1.Click

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
        SaveSettings()
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
                NewLateBinding.LateCall(objectValue3, Nothing, "Save", Array.Empty(Of Object)(), Nothing, Nothing, Nothing, True)
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged

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
                NumericUpDown1.Value = 450
                NumericUpDown2.Value = 0
                NumericUpDown3.Value = 500
                CheckBox1.Checked = False
                CheckBox2.Checked = False
                CheckBox7.Checked = False
            Case MsgBoxResult.No

        End Select

        SaveSettings()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Process.Start("https://easings.net/")

    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Process.Start("https://github.com/ChrisAnd1998/FalconX/issues")

    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click

        RestartExplorer()
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged

        If CheckBox2.Checked = False Then
            SetWindowPos(SecondTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)
            SetWindowPos(ThirdTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)

        End If

    End Sub

    Private Sub CheckBox3_Click(sender As Object, e As EventArgs) Handles CheckBox3.Click

    End Sub

    Friend Sub EnableTaskbarStyle()
        Dim Progman As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Progman", CType(0, IntPtr)))

        Dim desktops As AutomationElement = AutomationElement.RootElement
        Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
        Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, condition)

        Dim accent = New AccentPolicy()
        Dim accentStructSize = Marshal.SizeOf(accent)

        accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT
        accent.GradientColor = 255

        Dim accentPtr = Marshal.AllocHGlobal(accentStructSize)
        Marshal.StructureToPtr(accent, accentPtr, False)
        Dim data = New WindowCompositionAttributeData With {
            .Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            .SizeOfData = accentStructSize,
            .Data = accentPtr
        }

        Dim trays As New ArrayList

        For Each trayWnd As AutomationElement In lists
            trays.Add(trayWnd.Current.NativeWindowHandle.ToString)
        Next

        Do
            For Each tray In trays
                Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
                SetWindowCompositionAttribute(CType(trayptr, IntPtr), data)
            Next
            System.Threading.Thread.Sleep(14)
        Loop Until CheckBox3.Checked = False

        SaveSettings()

        Marshal.FreeHGlobal(accentPtr)

    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        SaveMemory()
        If CheckBox3.Checked = True Then
            Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf EnableTaskbarStyle)
            t1.Start()
        End If
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Dim address As String = "https://chrisandriessen.nl/web/version/FalconX.txt"
        Dim client As WebClient = New WebClient()
        Dim reader As StreamReader = New StreamReader(client.OpenRead(address))

        Dim latest = reader.ReadToEnd.ToString

        If latest = Application.ProductVersion Then
            MessageBox.Show("You are up to date!")
        Else
            Console.WriteLine(latest)
            MessageBox.Show("Update " & latest & " is available!")
            Process.Start("https://chrisandriessen.nl/web/FalconX.html")
        End If

        reader.Dispose()
        client.Dispose()
    End Sub

End Class