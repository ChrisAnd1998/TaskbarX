Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Windows
Imports System.Windows.Automation
Imports Microsoft.VisualBasic.CompilerServices
Imports Transitions

Public Class Form1

    Private Const SWP_NOSIZE As Integer = &H1
    Private Const SWP_NOZORDER As Integer = &H4
    Private Const SWP_SHOWWINDOW As Integer = &H40
    Private Const SWP_ASYNCWINDOWPOS As Integer = &H4000
    Private Const SWP_NOSENDCHANGING As Integer = &H400
    Private Const SWP_NOACTIVATE As Integer = &H10
    Private Const SWP_NOMOVE As Integer = &H2

    Private Const SWP_NOOWNERZORDER As Integer = &H200
    Private Const SWP_DRAWFRAME As Integer = &H20
    Private Const SWP_NOCOPYBITS As Integer = &H100

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
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure AccentPolicy
        Public AccentState As AccentState
        Public AccentFlags As Integer
        Public GradientColor As Integer
        Public AnimationId As Integer
    End Structure

    Private Declare Auto Function SetWindowPos Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As Integer) As Boolean
    Private Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (ByVal hProcess As IntPtr, ByVal dwMinimumWorkingSetSize As Int32, ByVal dwMaximumWorkingSetSize As Int32) As Int32
    Public Declare Function TerminateProcess Lib "kernel32" (ByVal hProcess As IntPtr, ByVal uExitCode As UInteger) As Integer
    Friend Declare Function SetWindowCompositionAttribute Lib "user32.dll" (ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    Private Declare Auto Function FindWindow Lib "user32.dll" (ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr

    Private TaskbarWidthFull As Integer
    Private tasklistPtr As IntPtr
    Private trayWndPtr As IntPtr
    Private tasklistWidth As Integer
    Private tasklistHeight As Integer
    Private tasklistLeft As Integer

    Dim refresh As Boolean = False

    Private Sub start()

        Dim readValue = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", Nothing)

        If readValue = 1 Then
            NotifyIcon1.Icon = My.Resources.light
        Else
            NotifyIcon1.Icon = My.Resources.dark
        End If

        Me.Hide()
        refresh = False

        Me.Left = 0
        Me.Width = Screen.PrimaryScreen.Bounds.Width
        '  Hide()

        Dim strx As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"

        If File.Exists(strx + "\FalconX.lnk") Then
            RunAtStartupToolStripMenuItem.Checked = True
        Else
            RunAtStartupToolStripMenuItem.Checked = False
        End If

        If My.Settings.pos = Nothing Then
            ToolStripTextBox2.Text = 0
        Else
            ToolStripTextBox2.Text = Label3.Text
        End If

        AccelerationToolStripMenuItem.Checked = False
        NoneToolStripMenuItem.Checked = False
        CriticalDampingToolStripMenuItem.Checked = False
        DecelerationToolStripMenuItem.Checked = False
        EaseInEaseOutToolStripMenuItem.Checked = False
        LinearToolStripMenuItem.Checked = False

        NoneToolStripMenuItem1.Checked = False
        TransparantToolStripMenuItem.Checked = False
        BlurToolStripMenuItem.Checked = False
        ToolStripMenuItem5.Checked = False

        If CheckBox1.Checked = True Then
            animation = 0
            NoneToolStripMenuItem.Checked = True
        End If

        If CheckBox2.Checked = True Then
            animation = 1
            AccelerationToolStripMenuItem.Checked = True
        End If

        If CheckBox3.Checked = True Then
            animation = 2
            CriticalDampingToolStripMenuItem.Checked = True
        End If

        If CheckBox4.Checked = True Then
            animation = 3
            DecelerationToolStripMenuItem.Checked = True
        End If

        If CheckBox5.Checked = True Then
            animation = 4
            EaseInEaseOutToolStripMenuItem.Checked = True
        End If

        If CheckBox6.Checked = True Then
            animation = 5
            LinearToolStripMenuItem.Checked = True
        End If

        If CheckBox7.Checked = True Then
            NoneToolStripMenuItem1.Checked = True
        End If

        If CheckBox8.Checked = True Then
            TransparantToolStripMenuItem.Checked = True
        End If

        If CheckBox9.Checked = True Then
            BlurToolStripMenuItem.Checked = True
        End If

        If CheckBox10.Checked = True Then
            ToolStripMenuItem5.Checked = True
        End If

        Dim t0 As System.Threading.Thread = New System.Threading.Thread(AddressOf TTBissuefix)
        t0.Start()

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf ConstantlyCalculateWidth)
        t1.Start()

        System.Threading.Thread.Sleep(1000) : Application.DoEvents()

        Dim t2 As System.Threading.Thread = New System.Threading.Thread(AddressOf CalculateLeftOffset)
        t2.Start()

        System.Threading.Thread.Sleep(1000) : Application.DoEvents()

        Dim t3 As System.Threading.Thread = New System.Threading.Thread(AddressOf ConstantlyMoveTaskbar)
        t3.Start()

        ReleaseMemory()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        start()

    End Sub

    Private Sub ConstantlyCalculateWidth()
        Try

            Dim desktop As AutomationElement = AutomationElement.RootElement
            Dim tasklisty As AutomationElement = Nothing
            Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"))
            Dim trayWnd As AutomationElement = desktop.FindFirst(TreeScope.Children, condition)

            Dim tasklist As AutomationElement = trayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))

            tasklisty = tasklist

            tasklistPtr = tasklist.Current.NativeWindowHandle
            tasklistWidth = trayWnd.Current.BoundingRectangle.Width
            tasklistHeight = trayWnd.Current.BoundingRectangle.Height
            tasklistLeft = tasklist.Current.BoundingRectangle.Left

            SetWindowPos(tasklistPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOZORDER Or SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE)

            System.Threading.Thread.Sleep(500) : Application.DoEvents()

            Do
                Try

                    System.Threading.Thread.Sleep(500) : Application.DoEvents()

                    Dim TaskbarWidth = 0
                    Dim TaskbarWidth2

                    If refresh = True Then
                        Exit Sub
                    End If

                    For Each ui As AutomationElement In tasklisty.FindAll(TreeScope.Descendants, New PropertyCondition(AutomationElement.IsControlElementProperty, True))
                        If Not ui.Current.Name = Nothing Then
                            Dim Bounds As Rect = ui.Current.BoundingRectangle
                            TaskbarWidth = TaskbarWidth + Bounds.Width
                            If refresh = True Then
                                Exit Sub
                            End If

                            System.Threading.Thread.Sleep(20) : Application.DoEvents()
                        End If
                    Next
                    If Not TaskbarWidth = TaskbarWidth2 Then
                        TaskbarWidthFull = TaskbarWidth
                        TaskbarWidth2 = TaskbarWidth
                    End If
                Catch

                End Try
            Loop
        Catch ex As Exception
            Console.WriteLine("Error Restarting...")
            ConstantlyCalculateWidth()
        End Try

    End Sub

    Private Sub CalculateLeftOffset()

        Dim desktop As AutomationElement = AutomationElement.RootElement
        Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"))

        Do
            Try

                If refresh = True Then
                    Exit Sub
                End If

                System.Threading.Thread.Sleep(3000) : Application.DoEvents()

                If refresh = True Then
                    Exit Sub
                End If
                Dim listsSW As AutomationElement = desktop.FindFirst(TreeScope.Children, condition)
                Dim tasklistSW As AutomationElement = listsSW.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskSwWClass"))
                tasklistLeft = tasklistSW.Current.BoundingRectangle.Left

                System.Threading.Thread.Sleep(2000) : Application.DoEvents()

                Dim readValue = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", Nothing)

                If readValue = 1 Then
                    NotifyIcon1.Icon = My.Resources.light
                Else
                    NotifyIcon1.Icon = My.Resources.dark
                End If
            Catch

            End Try
        Loop
    End Sub

    Dim hori As Boolean

    Private Sub ConstantlyMoveTaskbar()

        System.Threading.Thread.Sleep(2000) : Application.DoEvents()

        Do

            Try

                System.Threading.Thread.Sleep(10) : Application.DoEvents()

                If refresh = True Then
                    Exit Sub
                End If

                Dim TaskbarWidthHalf = TaskbarWidthFull / 2

                Dim Display1 As Integer = tasklistWidth / 2
                Dim Display1h As Integer = tasklistHeight / 2

                Dim positionh = Display1h - TaskbarWidthHalf - tasklistLeft - 2

                Dim dd As Integer

                If ToolStripTextBox2.Text = Nothing Then
                    dd = 0
                Else
                    dd = ToolStripTextBox2.Text
                End If

                Dim position = Display1 - TaskbarWidthHalf - tasklistLeft - 2 + dd

                Gotoit = position

                Me.Invoke(New Action(Sub()

                                         Label1.Text = position

                                     End Sub))

                If NoneToolStripMenuItem1.Checked = True Then
                Else
                    If TransparantToolStripMenuItem.Checked = True Then
                        EnableTaskbarTransparancy()
                    End If
                    If BlurToolStripMenuItem.Checked = True Then
                        EnableTaskbarTransparancy()
                    End If
                    If ToolStripMenuItem5.Checked = True Then
                        EnableTaskbarTransparancy()
                    End If
                End If

                If cmoving = False Then
                    If refresh = True Then
                        Exit Sub
                    End If

                    SetWindowPos(tasklistPtr, IntPtr.Zero, position, 0, 0, 0, SWP_NOZORDER Or SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE Or SWP_NOCOPYBITS Or SWP_NOOWNERZORDER)

                End If
            Catch

            End Try
        Loop
    End Sub

    Sub TTBissuefix()
        Do
            Try




                If NoneToolStripMenuItem1.Checked = True Then
                Else
                    If TransparantToolStripMenuItem.Checked = True Then
                        Process.GetProcessesByName("TranslucentTB")(0).Kill()
                    End If
                    If BlurToolStripMenuItem.Checked = True Then
                        Process.GetProcessesByName("TranslucentTB")(0).Kill()
                    End If
                    If ToolStripMenuItem5.Checked = True Then
                        Process.GetProcessesByName("TranslucentTB")(0).Kill()
                    End If
                End If

                System.Threading.Thread.Sleep(5000) : Application.DoEvents()
            Catch

            End Try
        Loop
    End Sub

    Friend Sub EnableTaskbarTransparancy()


        Dim tskBarClassName As String = "Shell_TrayWnd"
        Dim tskBarHwnd As IntPtr = FindWindow(tskBarClassName, Nothing)

        Dim accent = New AccentPolicy()
        Dim accentStructSize = Marshal.SizeOf(accent)

        If ToolStripMenuItem5.Checked = True Then
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND
            accent.GradientColor = 10 'Or 16777215
        End If

        If BlurToolStripMenuItem.Checked = True Then
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
        End If

        If TransparantToolStripMenuItem.Checked = True Then
            accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARANT

        End If

        Dim accentPtr = Marshal.AllocHGlobal(accentStructSize)
        Marshal.StructureToPtr(accent, accentPtr, False)
        Dim data = New WindowCompositionAttributeData()
        data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY
        data.SizeOfData = accentStructSize
        data.Data = accentPtr
        SetWindowCompositionAttribute(tskBarHwnd, data)
        Marshal.FreeHGlobal(accentPtr)
    End Sub

    Friend Sub ReleaseMemory()
        Try
            GC.Collect()
            GC.WaitForPendingFinalizers()
            If Environment.OSVersion.Platform = PlatformID.Win32NT Then
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        exitf()
    End Sub

    Private Sub exitf()
        Me.Invoke(New Action(Sub()

                                 My.Settings.Save()

                                 refresh = True

                                 System.Threading.Thread.Sleep(5000) : Application.DoEvents()

                                 NotifyIcon1.Visible = False
                                 Application.ExitThread()
                                 Application.ExitThread()
                                 Application.ExitThread()
                                 Application.ExitThread()
                                 NotifyIcon1.Visible = False
                                 Application.ExitThread()
                                 Application.ExitThread()
                                 Application.ExitThread()
                                 Application.ExitThread()
                                 NotifyIcon1.Visible = False

                                 Application.Exit()
                                 Me.Close()

                                 restartexplorer()

                             End Sub))

        End
        End
        End
        End
    End Sub

    Private Sub Label1_TextChanged(sender As Object, e As EventArgs) Handles Label1.TextChanged
        ReleaseMemory()
        If refresh = True Then
            Exit Sub
        End If
        FluentMove()
    End Sub

    Sub FluentMove()
        Try

            cmoving = True

            If My.Settings.speedss = Nothing Then
                ToolStripTextBox1.Text = 500
            Else
                ToolStripTextBox1.Text = My.Settings.speedss
            End If

            If ToolStripTextBox1.Text = 0 Then
                ToolStripTextBox1.Text = 1
            End If

            Dim speed As Integer = ToolStripTextBox1.Text

            If animation = 1 Then
                Dim r1 As Transition = New Transition(New TransitionType_Acceleration(speed))
                r1.add(Panel1, "Left", Gotoit)
                r1.run()
            End If

            If animation = 2 Then
                Dim r3 As Transition = New Transition(New TransitionType_CriticalDamping(speed))
                r3.add(Panel1, "Left", Gotoit)
                r3.run()
            End If

            If animation = 3 Then
                Dim r4 As Transition = New Transition(New TransitionType_Deceleration(speed))
                r4.add(Panel1, "Left", Gotoit)
                r4.run()
            End If

            If animation = 4 Then
                Dim r5 As Transition = New Transition(New TransitionType_EaseInEaseOut(speed))
                r5.add(Panel1, "Left", Gotoit)
                r5.run()
            End If

            If animation = 5 Then
                Dim r7 As Transition = New Transition(New TransitionType_Linear(speed))
                r7.add(Panel1, "Left", Gotoit)
                r7.run()
            End If

            If animation = 0 Then
                Panel1.Left = Gotoit
            End If

            Do Until Panel1.Left = Gotoit
                Application.DoEvents()
            Loop
            cmoving = False
        Catch ex As Exception

        End Try
    End Sub

    Dim Gotoit As Integer
    Dim cmoving As Boolean = False
    Dim animation As Integer = 2

    Private Sub Panel1_Move(sender As Object, e As EventArgs) Handles Panel1.Move
        SetWindowPos(tasklistPtr, IntPtr.Zero, Panel1.Left, 0, 0, 0, SWP_NOZORDER Or SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOSENDCHANGING Or SWP_NOACTIVATE)
    End Sub

    Sub clearchecks()
        AccelerationToolStripMenuItem.Checked = False
        NoneToolStripMenuItem.Checked = False
        CriticalDampingToolStripMenuItem.Checked = False
        DecelerationToolStripMenuItem.Checked = False
        EaseInEaseOutToolStripMenuItem.Checked = False
        LinearToolStripMenuItem.Checked = False

        CheckBox1.Checked = False
        CheckBox2.Checked = False
        CheckBox3.Checked = False
        CheckBox4.Checked = False
        CheckBox5.Checked = False
        CheckBox6.Checked = False
    End Sub

    Private Sub NoneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NoneToolStripMenuItem.Click
        clearchecks()
        animation = 0
        NoneToolStripMenuItem.Checked = True
        CheckBox1.Checked = True
        My.Settings.Save()
    End Sub

    Private Sub AccelerationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AccelerationToolStripMenuItem.Click
        clearchecks()
        animation = 1
        AccelerationToolStripMenuItem.Checked = True
        CheckBox2.Checked = True
        My.Settings.Save()
    End Sub

    Private Sub CriticalDampingToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CriticalDampingToolStripMenuItem.Click
        clearchecks()
        animation = 2
        CriticalDampingToolStripMenuItem.Checked = True
        CheckBox3.Checked = True
        My.Settings.Save()
    End Sub

    Private Sub DecelerationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DecelerationToolStripMenuItem.Click
        clearchecks()
        animation = 3
        DecelerationToolStripMenuItem.Checked = True
        CheckBox4.Checked = True
        My.Settings.Save()
    End Sub

    Private Sub EaseInEaseOutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EaseInEaseOutToolStripMenuItem.Click
        clearchecks()
        animation = 4
        EaseInEaseOutToolStripMenuItem.Checked = True
        CheckBox5.Checked = True
        My.Settings.Save()
    End Sub

    Private Sub LinearToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LinearToolStripMenuItem.Click
        clearchecks()
        animation = 5
        LinearToolStripMenuItem.Checked = True
        CheckBox6.Checked = True
        My.Settings.Save()
    End Sub

    Private Sub RunAtStartupToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RunAtStartupToolStripMenuItem.Click

        Dim strx As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"

        If File.Exists(strx + "\FalconX.lnk") Then
            Dim str As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
            File.Delete(str + "\FalconX.lnk")
            RunAtStartupToolStripMenuItem.Checked = False
        Else
            Dim objectValue As Object = RuntimeHelpers.GetObjectValue(Interaction.CreateObject("WScript.Shell", ""))
            objectValue = RuntimeHelpers.GetObjectValue(Interaction.CreateObject("WScript.Shell", ""))
            Dim objectValue2 As Object = RuntimeHelpers.GetObjectValue(NewLateBinding.LateGet(objectValue, Nothing, "SpecialFolders", New Object() {"Startup"}, Nothing, Nothing, Nothing))
            Dim objectValue3 As Object = RuntimeHelpers.GetObjectValue(NewLateBinding.LateGet(objectValue, Nothing, "CreateShortcut", New Object() {Operators.ConcatenateObject(objectValue2, "\FalconX.lnk")}, Nothing, Nothing, Nothing))
            NewLateBinding.LateSet(objectValue3, Nothing, "TargetPath", New Object() {NewLateBinding.LateGet(objectValue, Nothing, "ExpandEnvironmentStrings", New Object() {Application.ExecutablePath}, Nothing, Nothing, Nothing)}, Nothing, Nothing)
            NewLateBinding.LateSet(objectValue3, Nothing, "WorkingDirectory", New Object() {NewLateBinding.LateGet(objectValue, Nothing, "ExpandEnvironmentStrings", New Object() {Application.StartupPath}, Nothing, Nothing, Nothing)}, Nothing, Nothing)
            NewLateBinding.LateSet(objectValue3, Nothing, "WindowStyle", New Object() {4}, Nothing, Nothing)
            NewLateBinding.LateCall(objectValue3, Nothing, "Save", New Object(-1) {}, Nothing, Nothing, Nothing, True)
            RunAtStartupToolStripMenuItem.Checked = True
        End If

    End Sub

    Private Sub restartexplorer()
        Const explorer As String = "explorer.exe"
        Dim explorerPath As String = String.Format("{0}\{1}", Environment.GetEnvironmentVariable("WINDIR"), explorer)
        For Each process__1 As Process In Process.GetProcesses()
            ' In case we get Access Denied
            Try
                If String.Compare(process__1.MainModule.FileName, explorerPath, StringComparison.OrdinalIgnoreCase) = 0 Then
                    process__1.Kill()
                End If
            Catch
                '
            End Try
        Next
    End Sub

    Private Sub ToolStripTextBox1_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox1.TextChanged
        Label2.Text = ToolStripTextBox1.Text
        My.Settings.Save()
    End Sub

    Private Sub ToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        refresh = True
        System.Threading.Thread.Sleep(3000) : Application.DoEvents()

        Application.Restart()
    End Sub

    Private Sub ToolStripTextBox2_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox2.TextChanged
        Label3.Text = ToolStripTextBox2.Text
        My.Settings.Save()
    End Sub

    Private Sub NoneToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles NoneToolStripMenuItem1.Click
        NoneToolStripMenuItem1.Checked = True
        BlurToolStripMenuItem.Checked = False
        TransparantToolStripMenuItem.Checked = False
        ToolStripMenuItem5.Checked = False

        CheckBox7.Checked = True
        CheckBox8.Checked = False
        CheckBox9.Checked = False
        CheckBox10.Checked = False

        My.Settings.Save()

        restartexplorer()

        refresh = True
        System.Threading.Thread.Sleep(3000) : Application.DoEvents()

        Application.Restart()
    End Sub

    Private Sub TransparantToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TransparantToolStripMenuItem.Click
        TransparantToolStripMenuItem.Checked = True
        NoneToolStripMenuItem1.Checked = False
        BlurToolStripMenuItem.Checked = False
        ToolStripMenuItem5.Checked = False

        CheckBox7.Checked = False
        CheckBox8.Checked = True
        CheckBox9.Checked = False
        CheckBox10.Checked = False

        My.Settings.Save()
    End Sub

    Private Sub BlurToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BlurToolStripMenuItem.Click
        BlurToolStripMenuItem.Checked = True
        TransparantToolStripMenuItem.Checked = False
        NoneToolStripMenuItem1.Checked = False
        ToolStripMenuItem5.Checked = False

        CheckBox7.Checked = False
        CheckBox8.Checked = False
        CheckBox9.Checked = True
        CheckBox10.Checked = False

        My.Settings.Save()
    End Sub

    Private Sub ToolStripMenuItem5_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem5.Click
        BlurToolStripMenuItem.Checked = False
        TransparantToolStripMenuItem.Checked = False
        NoneToolStripMenuItem1.Checked = False

        ToolStripMenuItem5.Checked = True

        CheckBox7.Checked = False
        CheckBox8.Checked = False
        CheckBox9.Checked = False

        CheckBox10.Checked = True

        My.Settings.Save()
    End Sub

End Class