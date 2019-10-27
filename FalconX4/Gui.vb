Imports System.ComponentModel
Imports System.Environment
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports FalconX4.VisualEffects
Imports FalconX4.VisualEffects.Animations.Effects
Imports FalconX4.VisualEffects.Easing
Imports Microsoft.VisualBasic.CompilerServices

Public Class Gui
    Public Launch As Boolean

    Public Const WM_NCLBUTTONDBLCLK As Integer = &HA3

    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        If m.Msg = WM_NCLBUTTONDBLCLK Then Return
        MyBase.WndProc(m)
    End Sub

    Private Sub Gui_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        NotifyIcon1.Visible = True

        If Launch = True Then
            Launch = False
            Me.Hide()
            Me.Opacity = 0

        End If

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        NotifyIcon1.Visible = False
        SaveSettings()
        Taskbar.RefreshWindowsExplorer()
        Taskbar.Closing()

        End
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://chrisandriessen.nl")
    End Sub

    Private Sub Gui_Load(sender As Object, e As EventArgs) Handles Me.Load

        Taskbar.RefreshWindowsExplorer()
        Dim currentProcess As Process = Process.GetCurrentProcess
        currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal
        Launch = True
        Me.Opacity = 0
        ComboBox1.Text = "BackEaseOut"
        LoadSettings()
        System.Threading.Thread.Sleep(500) : Application.DoEvents()
        RunAtStartUp()
        Taskbar.Main()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        AnimationControl.AnimationSelection = ComboBox1.Text

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SaveSettings()
        NotifyIcon1.Visible = False
        Taskbar.Closing()
        Taskbar.RefreshWindowsExplorer()
        Application.Restart()
        End
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

        If CheckBox1.Checked = True Then

            RadioButton1.Enabled = True
            RadioButton2.Enabled = True
            RadioButton3.Enabled = True

            Taskbar.TaskbarTransparant = True
            Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf Taskbar.EnableTaskbarStyle)
            t1.Start()
        Else
            RadioButton1.Enabled = False
            RadioButton2.Enabled = False
            RadioButton3.Enabled = False
            Taskbar.TaskbarTransparant = False
        End If

    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        AnimationControl.AnimationSpeed = CInt(NumericUpDown1.Value.ToString)

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Opacity = 0
        SaveSettings()
        Me.Hide()
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.Click
        LoadSettings()
        Me.WindowState = WindowState.Normal

        Dim TitlebarHeight As Integer = Me.Height - Me.ClientSize.Height - 2
        Me.Size = New Size(Panel2.Width + 15, Panel2.Location.Y + TitlebarHeight + Panel2.Height)

        Me.Location = New Size(Screen.PrimaryScreen.Bounds.Width / 2 - Me.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2 - Me.Height / 2)

        Me.Show()

        Me.Opacity = 100
    End Sub

    Private Sub Gui_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        NotifyIcon1.Visible = False
    End Sub

    Sub RunAtStartUp()

        Try

            Try
                If Application.StartupPath.Contains("40210ChrisAndriessen") Then
                    CheckBox3.Visible = False

                    Dim strxx As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
                    If File.Exists(strxx + "\FalconX.lnk") Then
                        Dim str As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\Microsoft\Windows\Start Menu\Programs\Startup"
                        File.Delete(str + "\FalconX.lnk")
                    End If

                End If
            Catch

            End Try

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

            If CheckBox3.Checked = True Then
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

    Sub LoadSettings()

        Try

            Dim path As String = GetFolderPath(SpecialFolder.ApplicationData) + "\FalconX.cfg"

            If File.Exists(path) Then

                ComboBox1.Text = System.IO.File.ReadAllLines(path)(0)
                NumericUpDown1.Value = CDec(System.IO.File.ReadAllLines(path)(1))
                NumericUpDown2.Value = CDec(System.IO.File.ReadAllLines(path)(2))

                If System.IO.File.ReadAllLines(path)(3) = "True" Then
                    CheckBox3.Checked = True
                Else
                    CheckBox3.Checked = False
                End If

                If System.IO.File.ReadAllLines(path)(4) = "True" Then
                    CheckBox2.Checked = True
                Else
                    CheckBox2.Checked = False
                End If

                If System.IO.File.ReadAllLines(path)(5) = "True" Then
                    CheckBox1.Checked = True
                Else
                    CheckBox1.Checked = False
                End If

                If System.IO.File.ReadAllLines(path)(6) = "1" Then
                    RadioButton1.Checked = True
                End If
                If System.IO.File.ReadAllLines(path)(6) = "2" Then
                    RadioButton2.Checked = True
                End If
                If System.IO.File.ReadAllLines(path)(6) = "3" Then
                    RadioButton3.Checked = True
                End If

                NumericUpDown3.Value = CDec(System.IO.File.ReadAllLines(path)(7))

            End If
        Catch
        End Try

    End Sub

    Sub SaveSettings()
        Try

            Dim path As String = GetFolderPath(SpecialFolder.ApplicationData) + "\FalconX.cfg"
            Dim fs As FileStream = File.Create(path)

            Dim Animation = ComboBox1.Text
            Dim Speed = NumericUpDown1.Value
            Dim Offset = NumericUpDown2.Value
            Dim RunAtStartUp As Boolean
            Dim Transparant As Boolean
            Dim CBT As Boolean
            Dim TaskbarStyle As Integer

            Dim Offset2 = NumericUpDown3.Value

            If RadioButton1.Checked = True Then
                TaskbarStyle = 1
            End If
            If RadioButton2.Checked = True Then
                TaskbarStyle = 2
            End If
            If RadioButton3.Checked = True Then
                TaskbarStyle = 3
            End If

            If CheckBox3.Checked = False Then
                RunAtStartUp = False
            Else
                RunAtStartUp = True
            End If

            If CheckBox1.Checked = False Then
                Transparant = False
            Else
                Transparant = True
            End If

            If CheckBox2.Checked = False Then
                CBT = False
            Else
                CBT = True
            End If

            ' Add text to the file.
            Dim info As Byte() = New UTF8Encoding(True).GetBytes(Animation.ToString & Environment.NewLine & Speed.ToString & Environment.NewLine & Offset.ToString & Environment.NewLine & RunAtStartUp.ToString & Environment.NewLine & CBT.ToString & Environment.NewLine & Transparant.ToString & Environment.NewLine & TaskbarStyle.ToString & Environment.NewLine & Offset2.ToString)

            fs.Write(info, 0, info.Length)
            fs.Close()
        Catch
        End Try
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        RunAtStartUp()

    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = True Then
            Taskbar.CenterBetween = True
        Else
            Taskbar.CenterBetween = False
        End If

        Taskbar.UpdateTaskbar = True
    End Sub

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        Taskbar.OffsetPosition = CInt(NumericUpDown2.Value.ToString)
        Taskbar.UpdateTaskbar = True
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If RadioButton1.Checked = True Then
            Taskbar.TaskbarStyle = 1
        End If
        Taskbar.UpdateTaskbarStyle = True
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If RadioButton2.Checked = True Then
            Taskbar.TaskbarStyle = 2
        End If
        Taskbar.UpdateTaskbarStyle = True
    End Sub

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        If RadioButton3.Checked = True Then
            Taskbar.TaskbarStyle = 3
        End If
        Taskbar.UpdateTaskbarStyle = True
    End Sub

    Private Sub NumericUpDown3_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown3.ValueChanged
        Taskbar.OffsetPosition2 = CInt(NumericUpDown3.Value.ToString)
        Taskbar.UpdateTaskbar = True
    End Sub

End Class