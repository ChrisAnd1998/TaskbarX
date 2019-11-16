Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Environment
Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CompilerServices

Public Class Gui
    Public Launch As Boolean

    Public Const WM_NCLBUTTONDBLCLK As Integer = &HA3

    Private Const CP_NOCLOSE_BUTTON As Integer = &H200

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Dim myCp As CreateParams = MyBase.CreateParams
            myCp.ClassStyle = myCp.ClassStyle Or CP_NOCLOSE_BUTTON
            Return myCp
        End Get
    End Property

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
        Button2.Text = "Closing..."
        NotifyIcon1.Visible = False
        SaveSettings()

        Taskbar.Closing()
        System.Threading.Thread.Sleep(500)
        Taskbar.RefreshWindowsExplorer()
        Taskbar.Closing()
        End
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://chrisandriessen.nl")
    End Sub

    Private Sub Gui_Load(sender As Object, e As EventArgs) Handles Me.Load

        ContextMenuStrip1.Renderer = New MyRenderer

        Taskbar.UpdateTaskbar = True
        Taskbar.RefreshWindowsExplorer()
        Dim currentProcess As Process = Process.GetCurrentProcess
        currentProcess.PriorityClass = ProcessPriorityClass.High
        Launch = True

        ComboBox1.Text = "BackEaseOut"
        LoadSettings()
        System.Threading.Thread.Sleep(500)
        RunAtStartUp()
        Taskbar.Main()
        Me.Hide()

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        AnimationControl.AnimationSelection = ComboBox1.Text
        AnimationControl2.AnimationSelection = ComboBox1.Text
        AnimationControl3.AnimationSelection = ComboBox1.Text
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Button3.Text = "Refreshing..."
        SaveSettings()
        NotifyIcon1.Visible = False
        Taskbar.Closing()
        ' Taskbar.Updating = False
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

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Me.Opacity = 0
        SaveSettings()
        Me.Hide()
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.Click

        If e.Button = MouseButtons.Left Then
            LoadSettings()
            Me.WindowState = WindowState.Normal

            Dim TitlebarHeight As Integer = Me.Height - Me.ClientSize.Height - 2
            Me.Size = New Size(Panel2.Width + 15, Panel2.Location.Y + TitlebarHeight + Panel2.Height)

            Me.Location = New Size(Screen.PrimaryScreen.Bounds.Width / 2 - Me.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2 - Me.Height / 2)

            Me.Show()

            Me.Opacity = 100

        End If

    End Sub

    Private Sub Gui_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        NotifyIcon1.Visible = False
    End Sub

    Sub RunAtStartUp()

        Try

            Try
                If Application.StartupPath.Contains("40210ChrisAndriessen") Then
                    CheckBox3.Visible = False
                    CheckForUpdatesToolStripMenuItem.Visible = False
                    Button5.Visible = False

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
                TextBox1.Text = CDec(System.IO.File.ReadAllLines(path)(1))
                TextBox2.Text = CDec(System.IO.File.ReadAllLines(path)(2))

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

                TextBox3.Text = CDec(System.IO.File.ReadAllLines(path)(7))

            End If
        Catch
        End Try

    End Sub

    Sub SaveSettings()
        Try

            Dim path As String = GetFolderPath(SpecialFolder.ApplicationData) + "\FalconX.cfg"
            Dim fs As FileStream = File.Create(path)

            Dim Animation = ComboBox1.Text
            Dim Speed = TextBox1.Text
            Dim Offset = TextBox2.Text
            Dim RunAtStartUp As Boolean
            Dim Transparant As Boolean
            Dim CBT As Boolean
            Dim TaskbarStyle As Integer

            Dim Offset2 = TextBox3.Text

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

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        ' CheckBox1.Checked = True
        If RadioButton1.Checked = True Then
            Taskbar.TaskbarStyle = 1
        End If
        Taskbar.UpdateTaskbar = True
        Taskbar.UpdateTaskbarStyle = True
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        'CheckBox1.Checked = True
        If RadioButton2.Checked = True Then
            Taskbar.TaskbarStyle = 2
        End If
        Taskbar.UpdateTaskbar = True
        Taskbar.UpdateTaskbarStyle = True
    End Sub

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        ' CheckBox1.Checked = True
        If RadioButton3.Checked = True Then
            Taskbar.TaskbarStyle = 3
        End If
        Taskbar.UpdateTaskbar = True
        Taskbar.UpdateTaskbarStyle = True
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        NotifyIcon1.Visible = False
        SaveSettings()
        Taskbar.RefreshWindowsExplorer()
        Taskbar.Closing()

        End
    End Sub

    Private Sub CheckForUpdatesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckForUpdatesToolStripMenuItem.Click

        Try
            Dim address As String = "https://chrisandriessen.nl/web/version/FalconX.txt"
            Dim client As WebClient = New WebClient()
            Dim reader As StreamReader = New StreamReader(client.OpenRead(address))

            Dim latest = reader.ReadToEnd.ToString

            If latest = Application.ProductVersion Then
                MessageBox.Show("You are up to date!")
            Else
                Dim result As Integer = MessageBox.Show("Update " & latest & " is available!", "FalconX Update", MessageBoxButtons.OKCancel)
                If result = DialogResult.OK Then
                    Process.Start("https://chrisandriessen.nl/web/FalconX.html")
                End If
            End If

            reader.Dispose()
            client.Dispose()
        Catch
        End Try
    End Sub

    Private Sub OpenSettingsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenSettingsToolStripMenuItem.Click
        LoadSettings()
        Me.WindowState = WindowState.Normal

        Dim TitlebarHeight As Integer = Me.Height - Me.ClientSize.Height - 2
        Me.Size = New Size(Panel2.Width + 15, Panel2.Location.Y + TitlebarHeight + Panel2.Height)

        Me.Location = New Size(Screen.PrimaryScreen.Bounds.Width / 2 - Me.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2 - Me.Height / 2)

        Me.Show()

        Me.Opacity = 100
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        TextBox1.Text = TrackBar1.Value
    End Sub

    Private Sub TrackBar2_Scroll(sender As Object, e As EventArgs) Handles TrackBar2.Scroll
        TextBox2.Text = TrackBar2.Value
    End Sub

    Private Sub TrackBar3_Scroll(sender As Object, e As EventArgs) Handles TrackBar3.Scroll
        TextBox3.Text = TrackBar3.Value
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        Try
            TrackBar1.Value = TextBox1.Text
            AnimationControl.AnimationSpeed = CInt(TextBox1.Text)
            AnimationControl2.AnimationSpeed = CInt(TextBox1.Text)
            AnimationControl3.AnimationSpeed = CInt(TextBox1.Text)
        Catch
            If TextBox1.Text = "" Then
                AnimationControl.AnimationSpeed = 100
                AnimationControl2.AnimationSpeed = 100
                AnimationControl3.AnimationSpeed = 100
                TrackBar1.Value = 100
                Exit Try
            End If

            If TextBox1.Text <= TrackBar1.Minimum Then
                AnimationControl.AnimationSpeed = 100
                AnimationControl2.AnimationSpeed = 100
                AnimationControl3.AnimationSpeed = 100
                TrackBar1.Value = 100
            Else
                AnimationControl.AnimationSpeed = 2000
                AnimationControl2.AnimationSpeed = 2000
                AnimationControl3.AnimationSpeed = 2000
                TrackBar1.Value = 2000
            End If

        End Try

    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        Try

            TrackBar2.Value = TextBox2.Text
            Taskbar.OffsetPosition = TextBox2.Text
            Taskbar.UpdateTaskbar = True
        Catch
            If TextBox1.Text = "" Then
                TrackBar2.Value = 0
                Taskbar.OffsetPosition = 0
                Exit Try
            End If
        End Try
    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged

        Try

            TrackBar3.Value = TextBox3.Text
            Taskbar.OffsetPosition2 = TextBox3.Text
            Taskbar.UpdateTaskbar = True
        Catch
            If TextBox1.Text = "" Then
                TrackBar3.Value = 0
                Taskbar.OffsetPosition2 = 0
                Exit Try
            End If
        End Try
    End Sub

    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar <> ControlChars.Back Then
            e.Handled = Not (Char.IsDigit(e.KeyChar))
        End If
    End Sub

    Private Sub TextBox2_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox2.KeyPress
        If e.KeyChar <> ControlChars.Back Then
            e.Handled = Not (Char.IsDigit(e.KeyChar) Or e.KeyChar = "-")
        End If
    End Sub

    Private Sub TextBox3_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox3.KeyPress
        If e.KeyChar <> ControlChars.Back Then
            e.Handled = Not (Char.IsDigit(e.KeyChar) Or e.KeyChar = "-")
        End If
    End Sub

    Private Sub PictureBox2_Click(sender As Object, e As EventArgs) Handles PictureBox2.Click
        Process.Start("https://chrisandriessen.nl")
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Process.Start("https://github.com/ChrisAnd1998/FalconX-Center-Taskbar/issues")
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        Try
            Dim address As String = "https://chrisandriessen.nl/web/version/FalconX.txt"
            Dim client As WebClient = New WebClient()
            Dim reader As StreamReader = New StreamReader(client.OpenRead(address))

            Dim latest = reader.ReadToEnd.ToString

            If latest = Application.ProductVersion Then
                MessageBox.Show("You are up to date!")
            Else

                Dim result As Integer = MessageBox.Show("Update " & latest & " is available!", "FalconX Update", MessageBoxButtons.OKCancel)
                If result = DialogResult.OK Then
                    Process.Start("https://chrisandriessen.nl/web/FalconX.html")
                End If

            End If

            reader.Dispose()
            client.Dispose()
        Catch
        End Try
    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        Process.Start("https://github.com/ChrisAnd1998/FalconX-Center-Taskbar/issues")
    End Sub

End Class

Public Class MyColorTable
    Inherits ProfessionalColorTable

    Public Overrides ReadOnly Property MenuItemBorder As Color
        Get
            Return Color.FromArgb(65, 65, 65)
        End Get
    End Property

    Public Overrides ReadOnly Property MenuItemSelected As Color
        Get
            Return Color.FromArgb(65, 65, 65)
        End Get
    End Property

    Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
        Get
            Return Color.FromArgb(43, 43, 43)
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
        Get
            Return Color.FromArgb(43, 43, 43)
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
        Get
            Return Color.FromArgb(43, 43, 43)
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
        Get
            Return Color.FromArgb(43, 43, 43)
        End Get
    End Property

End Class

Public Class MyRenderer
    Inherits ToolStripProfessionalRenderer

    Public Sub New()
        MyBase.New(New MyColorTable)

    End Sub

    Protected Overrides Sub OnRenderArrow(ByVal e As ToolStripArrowRenderEventArgs)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim r = New Rectangle(e.ArrowRectangle.Location, e.ArrowRectangle.Size)
        r.Inflate(-2, -6)
        e.Graphics.DrawLines(Pens.Black, New Point() {New Point(r.Left, r.Top), New Point(r.Right, (r.Top _
                                    + (r.Height / 2))), New Point(r.Left, (r.Top + r.Height))})
    End Sub

    Protected Overrides Sub OnRenderItemCheck(ByVal e As ToolStripItemImageRenderEventArgs)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim r = New Rectangle(e.ImageRectangle.Location, e.ImageRectangle.Size)
        r.Inflate(-4, -6)
        e.Graphics.DrawLines(Pens.Black, New Point() {New Point(r.Left, (r.Bottom _
                                    - (r.Height / 2))), New Point((r.Left _
                                    + (r.Width / 3)), r.Bottom), New Point(r.Right, r.Top)})
    End Sub

End Class