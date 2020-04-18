Imports System.ComponentModel
Imports System.IO
Imports System.Management
Imports System.Net
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Windows.Automation
Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.Win32
Imports Microsoft.Win32.TaskScheduler

Public Class Form1

    <DllImport("user32.dll", ExactSpelling:=True, CharSet:=CharSet.Auto)>
    Public Shared Function GetParent(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=Runtime.InteropServices.CharSet.Auto)>
    Private Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    Public Shared SWP_NOSIZE As UInt32 = 1
    Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
    Public Shared SWP_NOACTIVATE As UInt32 = 16
    Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
    Public Shared SWP_NOZORDER As UInt32 = 4

    Public Shared Sub RevertToZero()
        Try
            'Put all taskbars back to default position
            Dim desktops As AutomationElement = AutomationElement.RootElement
            Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
            Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, condition)
            Dim TrayWnds As New ArrayList
            Dim Taskbars As New ArrayList

            'Put all TrayWnds into an ArrayList
            For Each TrayWnd As AutomationElement In lists
                TrayWnds.Add(TrayWnd)
            Next

            'Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
            For Each Taskbar As AutomationElement In TrayWnds
                Dim mstasklist As AutomationElement = Taskbar.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
                Taskbars.Add(mstasklist)
            Next

            For Each TaskList As AutomationElement In Taskbars
                SendMessage(GetParent(GetParent(TaskList.Current.NativeWindowHandle)), 11, True, 0)
                SetWindowPos(TaskList.Current.NativeWindowHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
            Next
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        'Kill every other running instance of FalconX
        Shell("Taskkill /F /T /IM TaskbarX.exe")
        Shell("Taskkill /F /T /IM dllhost.exe")
        Try
            For Each prog As Process In Process.GetProcesses
                If prog.ProcessName = "TaskbarX" Then
                    prog.Kill()
                End If
            Next
        Catch
        End Try

        System.Threading.Thread.Sleep(50) : Application.DoEvents()

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf RevertToZero)
        t1.Start()

        ResetTaskbarStyle()

        Dim parameters As String

        If RadioButton1.Checked = True Then
            parameters = parameters & "-taskbarstyle=0 "
        End If
        If RadioButton2.Checked = True Then
            parameters = parameters & "-taskbarstyle=1 "
        End If
        If RadioButton3.Checked = True Then
            parameters = parameters & "-taskbarstyle=2 "
        End If
        If RadioButton4.Checked = True Then
            parameters = parameters & "-taskbarstyle=3 "
        End If

        If Not ComboBox1.SelectedItem = Nothing Then
            parameters = parameters & "-animationstyle=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not ComboBox2.SelectedItem = Nothing Then
            parameters = parameters & "-onbatteryanimationstyle=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not NumericUpDown4.Value = Nothing Then
            parameters = parameters & "-animationspeed=" & NumericUpDown4.Value & " "
        End If

        If Not NumericUpDown1.Value = Nothing Then
            parameters = parameters & "-primarytaskbaroffset=" & NumericUpDown1.Value & " "
        End If
        If Not NumericUpDown2.Value = Nothing Then
            parameters = parameters & "-secondarytaskbaroffset=" & NumericUpDown2.Value & " "
        End If

        If CheckBox1.Checked = True Then
            parameters = parameters & "-centerinbetween=1 "
        End If

        If Not NumericUpDown3.Value = Nothing Then
            parameters = parameters & "-looprefreshrate=" & NumericUpDown3.Value & " "
        End If

        If Not NumericUpDown5.Value = Nothing Then
            parameters = parameters & "-onbatterylooprefreshrate=" & NumericUpDown5.Value & " "
        End If

        If CheckBox2.Checked = True Then
            parameters = parameters & "-centerprimaryonly=1 "
        End If

        If CheckBox3.Checked = True Then
            parameters = parameters & "-centersecondaryonly=1 "
        End If

        If CheckBox4.Checked = True Then
            parameters = parameters & "-fixtoolbarsontraychange=1 "
        End If

        If CheckBox5.Checked = True Then
            parameters = parameters & "-startupshortcut=1 "
        End If

        If CheckBox5.Checked = True Then

            Dim regKey1 As RegistryKey
            regKey1 = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
            regKey1.DeleteValue("TaskbarX", False)
            regKey1.Close()

            If Application.StartupPath.Contains("40210ChrisAndriessen") Then

                Dim regKey2 As RegistryKey
                regKey2 = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
                regKey2.SetValue("TaskbarX", "cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters)
                regKey2.Close()

                Shell("cmd /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters)
            Else

                Dim regKey3 As RegistryKey
                regKey3 = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
                regKey3.SetValue("TaskbarX", Application.StartupPath & "\TaskbarX.exe " & parameters)
                regKey3.Close()

                Process.Start("TaskbarX.exe", parameters)
            End If
        Else

            Try
                Using ts As TaskService = New TaskService()
                    ts.RootFolder.DeleteTask("TaskbarX")
                End Using
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            Try

                Using ts As TaskService = New TaskService()

                    Dim td As TaskDefinition = ts.NewTask()

                    td.RegistrationInfo.Description = "Center taskbar icons"

                    td.Triggers.Add(New LogonTrigger With {
                    .UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                    .Delay = TimeSpan.FromSeconds(NumericUpDown6.Value)})

                    td.Settings.DisallowStartIfOnBatteries = False
                    td.Settings.StopIfGoingOnBatteries = False
                    td.Settings.RunOnlyIfIdle = False
                    td.Settings.IdleSettings.RestartOnIdle = False
                    td.Settings.IdleSettings.StopOnIdleEnd = False
                    td.Settings.Hidden = True
                    td.Settings.ExecutionTimeLimit = TimeSpan.Zero
                    td.RegistrationInfo.Author = "Chris Andriessen"
                    'td.Principal.LogonType = TaskLogonType.S4U
                    ' td.Principal.LogonType = TaskLogonType.InteractiveTokenOrPassword

                    If Application.StartupPath.Contains("40210ChrisAndriessen") Then
                        'td.Actions.Add(New ExecAction("powershell.exe", "Start-Process 'shell:AppsFolder\40210ChrisAndriessen.TaskbarX_y1dazs5f5wq00!TaskbarX' -Args '" & parameters & "'", Nothing))
                        'Process.Start("powershell.exe", "Start-Process 'shell:AppsFolder\40210ChrisAndriessen.TaskbarX_y1dazs5f5wq00!TaskbarX' -Args '" & parameters & "'")
                        td.Actions.Add(New ExecAction("cmd.exe", "/c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters, Nothing))
                        Shell("cmd /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters)
                    Else
                        td.Actions.Add(New ExecAction(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", parameters, Nothing))
                        Process.Start("TaskbarX.exe", parameters)
                    End If

                    ts.RootFolder.RegisterTaskDefinition("TaskbarX", td)

                End Using
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try

        End If

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'Kill every other running instance of FalconX
        Shell("Taskkill /F /T /IM TaskbarX.exe")
        Shell("Taskkill /F /T /IM dllhost.exe")
        Try
            For Each prog As Process In Process.GetProcesses
                If prog.ProcessName = "TaskbarX" Then
                    prog.Kill()
                End If
            Next
        Catch
        End Try

        System.Threading.Thread.Sleep(50) : Application.DoEvents()

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf RevertToZero)
        t1.Start()

        ResetTaskbarStyle()
    End Sub

    Public Shared WM_DWMCOLORIZATIONCOLORCHANGED As Integer = &H320
    Public Shared WM_DWMCOMPOSITIONCHANGED As Integer = &H31E
    Public Shared WM_THEMECHANGED As Integer = &H31A

    Public Shared Sub RefreshWindowsExplorer()
        Dim CLSID_ShellApplication As Guid = New Guid("13709620-C279-11CE-A49E-444553540000")
        Dim shellApplicationType As Type = Type.GetTypeFromCLSID(CLSID_ShellApplication, True)
        Dim shellApplication As Object = Activator.CreateInstance(shellApplicationType)
        Dim windows As Object = shellApplicationType.InvokeMember("Windows", BindingFlags.InvokeMethod, Nothing, shellApplication, New Object(-1) {})
        Dim windowsType As Type = windows.GetType
        Dim count As Object = windowsType.InvokeMember("Count", BindingFlags.GetProperty, Nothing, windows, Nothing)
        Dim i As Integer = 0
        Do While (i < CType(count, Integer))
            Dim item As Object = windowsType.InvokeMember("Item", BindingFlags.InvokeMethod, Nothing, windows, New Object() {i})
            Dim itemType As Type = item.GetType
            Dim itemName As String = CType(itemType.InvokeMember("Name", BindingFlags.GetProperty, Nothing, item, Nothing), String)
            If (itemName = "Shell_TrayWnd") Then
                itemType.InvokeMember("Refresh", BindingFlags.InvokeMethod, Nothing, item, Nothing)
            End If
            i = (i + 1)
        Loop
    End Sub

    Public Shared Sub ResetTaskbarStyle()
        Dim Progman As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Progman", CType(0, IntPtr)))
        Dim desktops As AutomationElement = AutomationElement.RootElement
        Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
        Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, condition)
        Dim trays As New ArrayList
        For Each trayWnd As AutomationElement In lists
            trays.Add(trayWnd.Current.NativeWindowHandle.ToString)
        Next
        RefreshWindowsExplorer()
        For Each tray As String In trays
            Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
            SendMessage(trayptr, WM_THEMECHANGED, True, 0)
            SendMessage(trayptr, WM_DWMCOLORIZATIONCOLORCHANGED, True, 0)
            SendMessage(trayptr, WM_DWMCOMPOSITIONCHANGED, True, 0)
        Next
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.Close()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        MessageBox.Show("TaskbarX Configurator" & vbNewLine & "Version: " & Application.ProductVersion & vbNewLine & "Author: Chris Andriessen" & vbNewLine & "Website: www.chrisandriessen.nl", "About")
    End Sub

    Private Sub ExportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem.Click
        SaveFileDialog1.Filter = "TaskbarX Config (*.tbx*)|*.tbx"
        SaveFileDialog1.FileName = "TaskbarXCfg"
        If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK _
         Then

            Dim parameters As String

            If RadioButton1.Checked = True Then
                parameters = parameters & "-taskbarstyle=0 "
            End If
            If RadioButton2.Checked = True Then
                parameters = parameters & "-taskbarstyle=1 "
            End If
            If RadioButton3.Checked = True Then
                parameters = parameters & "-taskbarstyle=2 "
            End If
            If RadioButton4.Checked = True Then
                parameters = parameters & "-taskbarstyle=3 "
            End If

            If Not ComboBox1.SelectedItem = Nothing Then
                parameters = parameters & "-animationstyle=" & ComboBox1.SelectedItem.ToString.ToLower & " "
            End If

            If Not ComboBox2.SelectedItem = Nothing Then
                parameters = parameters & "-onbatteryanimationstyle=" & ComboBox2.SelectedItem.ToString.ToLower & " "
            End If

            If Not NumericUpDown4.Value = Nothing Then
                parameters = parameters & "-animationspeed=" & NumericUpDown4.Value & " "
            End If

            If Not NumericUpDown1.Value = Nothing Then
                parameters = parameters & "-primarytaskbaroffset=" & NumericUpDown1.Value & " "
            End If
            If Not NumericUpDown2.Value = Nothing Then
                parameters = parameters & "-secondarytaskbaroffset=" & NumericUpDown2.Value & " "
            End If

            If CheckBox1.Checked = True Then
                parameters = parameters & "-centerinbetween=1 "
            End If

            If Not NumericUpDown3.Value = Nothing Then
                parameters = parameters & "-looprefreshrate=" & NumericUpDown3.Value & " "
            End If

            If Not NumericUpDown6.Value = Nothing Then
                parameters = parameters & "-taskdelay=" & NumericUpDown6.Value & " "
            End If

            If Not NumericUpDown5.Value = Nothing Then
                parameters = parameters & "-onbatterylooprefreshrate=" & NumericUpDown5.Value & " "
            End If

            If CheckBox2.Checked = True Then
                parameters = parameters & "-centerprimaryonly=1 "
            End If

            If CheckBox3.Checked = True Then
                parameters = parameters & "-centersecondaryonly=1 "
            End If

            If CheckBox4.Checked = True Then
                parameters = parameters & "-fixtoolbarsontraychange=1 "
            End If

            If CheckBox5.Checked = True Then
                parameters = parameters & "-startupshortcut=1 "
            End If

            My.Computer.FileSystem.WriteAllText _
            (SaveFileDialog1.FileName, parameters, True)
        End If
    End Sub

    Private Sub ImportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportToolStripMenuItem.Click

        OpenFileDialog1.Filter = "TaskbarX Config (*.tbx*)|*.tbx"
        OpenFileDialog1.FileName = "TaskbarXCfg"

        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then

            Dim cfg As String = My.Computer.FileSystem.ReadAllText(OpenFileDialog1.FileName)
            Dim arguments() As String = cfg.Split(" ")

            For Each argument In arguments
                Dim val() As String = Split(argument, "=")
                If argument.Contains("-taskbarstyle") Then
                    If val(1) = 0 Then
                        RadioButton1.Checked = True
                    End If
                    If val(1) = 1 Then
                        RadioButton2.Checked = True
                    End If
                    If val(1) = 2 Then
                        RadioButton3.Checked = True
                    End If
                    If val(1) = 3 Then
                        RadioButton4.Checked = True
                    End If
                End If
                If argument.Contains("-primarytaskbaroffset") Then
                    NumericUpDown1.Value = val(1)
                End If
                If argument.Contains("-secondarytaskbaroffset") Then
                    NumericUpDown2.Value = val(1)
                End If
                If argument.Contains("-centerprimaryonly") Then
                    If val(1) = "1" Then
                        CheckBox2.Checked = True
                    End If
                End If
                If argument.Contains("-centersecondaryonly") Then
                    If val(1) = "1" Then
                        CheckBox3.Checked = True
                    End If
                End If
                If argument.Contains("-animationstyle") Then
                    ComboBox1.Text = val(1)
                End If
                If argument.Contains("-animationspeed") Then
                    NumericUpDown4.Value = val(1)
                End If
                If argument.Contains("-looprefreshrate") Then
                    NumericUpDown3.Value = val(1)
                End If
                If argument.Contains("-taskdelay") Then
                    NumericUpDown6.Value = val(1)
                End If
                If argument.Contains("-centerinbetween") Then
                    If val(1) = "1" Then
                        CheckBox1.Checked = True
                    End If
                End If
                If argument.Contains("-onbatteryanimationstyle") Then
                    ComboBox2.Text = val(1)
                End If
                If argument.Contains("-onbatterylooprefreshrate") Then
                    NumericUpDown5.Value = val(1)
                End If
                If argument.Contains("-fixtoolbarsontraychange") Then
                    If val(1) = "1" Then
                        CheckBox4.Checked = True
                    End If
                End If
                If argument.Contains("-startupshortcut") Then
                    If val(1) = "1" Then
                        CheckBox5.Checked = True
                    End If
                End If
            Next

        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.Text = "CubicEaseInOut"
        ComboBox2.Text = "CubicEaseInOut"

        If Application.StartupPath.Contains("40210ChrisAndriessen") Then
            CheckForUpdateToolStripMenuItem.Visible = False
        End If

        Try

            Using ts As TaskService = New TaskService()

                Dim td = ts.GetTask("TaskbarX")

                Dim cfg As String = Nothing

                If Application.StartupPath.Contains("40210ChrisAndriessen") Then
                    cfg = td.Definition.Actions.ToString.Replace("cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX", "")
                Else
                    cfg = td.Definition.Actions.ToString.Replace(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", "")
                End If

                Dim arguments() As String = cfg.Split(" ")

                For Each argument In arguments
                    Dim val() As String = Split(argument, "=")
                    If argument.Contains("-taskbarstyle") Then
                        If val(1) = 0 Then
                            RadioButton1.Checked = True
                        End If
                        If val(1) = 1 Then
                            RadioButton2.Checked = True
                        End If
                        If val(1) = 2 Then
                            RadioButton3.Checked = True
                        End If
                        If val(1) = 3 Then
                            RadioButton4.Checked = True
                        End If
                    End If
                    If argument.Contains("-primarytaskbaroffset") Then
                        NumericUpDown1.Value = val(1)
                    End If
                    If argument.Contains("-secondarytaskbaroffset") Then
                        NumericUpDown2.Value = val(1)
                    End If
                    If argument.Contains("-centerprimaryonly") Then
                        If val(1) = "1" Then
                            CheckBox2.Checked = True
                        End If
                    End If
                    If argument.Contains("-centersecondaryonly") Then
                        If val(1) = "1" Then
                            CheckBox3.Checked = True
                        End If
                    End If
                    If argument.Contains("-animationstyle") Then
                        ComboBox1.Text = val(1)
                    End If
                    If argument.Contains("-animationspeed") Then
                        NumericUpDown4.Value = val(1)
                    End If
                    If argument.Contains("-looprefreshrate") Then
                        NumericUpDown3.Value = val(1)
                    End If
                    If argument.Contains("-centerinbetween") Then
                        If val(1) = "1" Then
                            CheckBox1.Checked = True
                        End If
                    End If
                    If argument.Contains("-onbatteryanimationstyle") Then
                        ComboBox2.Text = val(1)
                    End If
                    If argument.Contains("-onbatterylooprefreshrate") Then
                        NumericUpDown5.Value = val(1)
                    End If
                    If argument.Contains("-fixtoolbarsontraychange") Then
                        If val(1) = "1" Then
                            CheckBox4.Checked = True
                        End If
                    End If
                    If argument.Contains("-startupshortcut") Then
                        If val(1) = "1" Then
                            CheckBox5.Checked = True
                        End If
                    End If
                Next

                Console.WriteLine(td.Definition.Actions.ToString)

                Dim lg As LogonTrigger = td.Definition.Triggers.Item(0)
                Dim times As TimeSpan = lg.Delay

                NumericUpDown6.Value = times.Seconds
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Private Sub CheckForUpdateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckForUpdateToolStripMenuItem.Click
        Try
            Dim address As String = "https://raw.githubusercontent.com/ChrisAnd1998/FalconX-Center-Taskbar/master/VERSION"
            Dim client As WebClient = New WebClient()
            client.CachePolicy = New System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
            Dim reader As StreamReader = New StreamReader(client.OpenRead(address))

            Dim latest = reader.ReadToEnd.ToString

            If latest.Contains(Application.ProductVersion) Then
                MessageBox.Show("You are up to date!", "Check for Update")
            Else

                MessageBox.Show("Update " & latest & "is available!", "Check for Update")

            End If

            reader.Dispose()
            client.Dispose()
        Catch
        End Try
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        ' Dim result As DialogResult = MessageBox.Show("Before you close the configurator... Your settings won't be saved automatically! You can use File -> Export and Import to save and modify your settings." & vbNewLine & vbNewLine & "Do you want to close now?", "Closing...", MessageBoxButtons.YesNo)
        ' If (result = DialogResult.Yes) Then
        'Else
        ' e.Cancel = True
        'End If
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim regKey1 As RegistryKey
        regKey1 = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
        regKey1.DeleteValue("TaskbarX", False)
        regKey1.Close()

        MessageBox.Show("Startup Shortcut Removed!")
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Try
            Using ts As TaskService = New TaskService()
                ts.RootFolder.DeleteTask("TaskbarX")
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        MessageBox.Show("Taskschedule Removed!")
    End Sub

End Class