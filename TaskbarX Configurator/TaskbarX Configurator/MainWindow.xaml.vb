Option Strict On

Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports Microsoft.Win32.TaskScheduler
Imports ModernWpf.Controls

Class MainWindow

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

    Public Declare Function GetCursorPos Lib "user32" (ByRef lpPoint As PointAPI) As Boolean

    Declare Function GetAsyncKeyState Lib "user32" (ByVal vkey As Integer) As Short

    Structure PointAPI
        Public x As Int32
        Public y As Int32
    End Structure

    Public Shared SWP_NOSIZE As UInt32 = 1
    Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
    Public Shared SWP_NOACTIVATE As UInt32 = 16
    Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
    Public Shared SWP_NOZORDER As UInt32 = 4

    Public Delegate Function CallBack(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean

    Public Declare Function EnumWindows Lib "user32" (ByVal Adress As CallBack, ByVal y As Integer) As Integer
    Public Shared ActiveWindows As New System.Collections.ObjectModel.Collection(Of IntPtr)

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function GetClassName(ByVal hWnd As System.IntPtr, ByVal lpClassName As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    Public Shared Function GetActiveWindows() As ObjectModel.Collection(Of IntPtr)
        windowHandles.Clear()
        EnumWindows(AddressOf Enumerator, 0)
        Return ActiveWindows
    End Function

    Public Shared Function Enumerator(ByVal hwnd As IntPtr, ByVal lParam As Integer) As Boolean
        Dim sClassName As New StringBuilder("", 256)
        Call GetClassName(hwnd, sClassName, 256)
        If sClassName.ToString = "Shell_TrayWnd" Or sClassName.ToString = "Shell_SecondaryTrayWnd" Then
            windowHandles.Add(hwnd)
        End If
        Return True
    End Function

    Public Shared windowHandles As ArrayList = New ArrayList()

    Public Shared Sub RevertToZero()

        If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
            Try
                Dim processInfo As ProcessStartInfo = New ProcessStartInfo()
                processInfo.WindowStyle = ProcessWindowStyle.Hidden
                processInfo.FileName = "cmd.exe"
                processInfo.Arguments = " /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & "-stop"
                Process.Start(processInfo)
            Catch
            End Try
        Else
            Try
                System.Diagnostics.Process.Start("TaskbarX.exe", "-stop")
            Catch
            End Try
        End If

    End Sub

    Private bmp As Bitmap = New Bitmap(1, 1)

    Private Function GetColorAt(ByVal x As Integer, ByVal y As Integer) As Color
        Dim bounds As Rectangle = New Rectangle(x, y, 1, 1)

        Using g As Graphics = Graphics.FromImage(bmp)
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size)
        End Using

        Return bmp.GetPixel(0, 0)
    End Function

    Private Sub Window_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)

        'Checkbox10.Visibility = Visibility.Hidden

        ListBox1.SelectedIndex = 0

        sAlpha.Value = 50

        ComboBox1.Items.Add("none")
        ComboBox1.Items.Add("linear")
        ComboBox1.Items.Add("expoeaseout")
        ComboBox1.Items.Add("expoeasein")
        ComboBox1.Items.Add("expoeaseinout")
        ComboBox1.Items.Add("expoeaseoutin")
        ComboBox1.Items.Add("circeaseout")
        ComboBox1.Items.Add("circeasein")
        ComboBox1.Items.Add("circeaseinout")
        ComboBox1.Items.Add("circeaseoutin")
        ComboBox1.Items.Add("quadeaseout")
        ComboBox1.Items.Add("quadeasein")
        ComboBox1.Items.Add("quadeaseinout")
        ComboBox1.Items.Add("quadeaseoutin")
        ComboBox1.Items.Add("sineeaseout")
        ComboBox1.Items.Add("sineeasein")
        ComboBox1.Items.Add("sineeaseinout")
        ComboBox1.Items.Add("sineeaseoutin")
        ComboBox1.Items.Add("cubiceaseout")
        ComboBox1.Items.Add("cubiceasein")
        ComboBox1.Items.Add("cubiceaseinout")
        ComboBox1.Items.Add("cubiceaseoutin")
        ComboBox1.Items.Add("quarteaseout")
        ComboBox1.Items.Add("quarteasein")
        ComboBox1.Items.Add("quarteaseinout")
        ComboBox1.Items.Add("quarteaseoutin")
        ComboBox1.Items.Add("quinteaseout")
        ComboBox1.Items.Add("quinteasein")
        ComboBox1.Items.Add("quinteaseinout")
        ComboBox1.Items.Add("quinteaseoutin")
        ComboBox1.Items.Add("elasticeaseout")
        ComboBox1.Items.Add("elasticeasein")
        ComboBox1.Items.Add("elasticeaseinout")
        ComboBox1.Items.Add("elasticeaseoutin")
        ComboBox1.Items.Add("bounceeaseout")
        ComboBox1.Items.Add("bounceeasein")
        ComboBox1.Items.Add("bounceeaseinout")
        ComboBox1.Items.Add("bounceeaseoutin")
        ComboBox1.Items.Add("backeaseout")
        ComboBox1.Items.Add("backeasein")
        ComboBox1.Items.Add("backeaseinout")
        ComboBox1.Items.Add("backeaseoutin")

        ComboBox1.SelectedItem = "cubiceaseinout"

        ComboBox2.Items.Add("none")
        ComboBox2.Items.Add("linear")
        ComboBox2.Items.Add("expoeaseout")
        ComboBox2.Items.Add("expoeasein")
        ComboBox2.Items.Add("expoeaseinout")
        ComboBox2.Items.Add("expoeaseoutin")
        ComboBox2.Items.Add("circeaseout")
        ComboBox2.Items.Add("circeasein")
        ComboBox2.Items.Add("circeaseinout")
        ComboBox2.Items.Add("circeaseoutin")
        ComboBox2.Items.Add("quadeaseout")
        ComboBox2.Items.Add("quadeasein")
        ComboBox2.Items.Add("quadeaseinout")
        ComboBox2.Items.Add("quadeaseoutin")
        ComboBox2.Items.Add("sineeaseout")
        ComboBox2.Items.Add("sineeasein")
        ComboBox2.Items.Add("sineeaseinout")
        ComboBox2.Items.Add("sineeaseoutin")
        ComboBox2.Items.Add("cubiceaseout")
        ComboBox2.Items.Add("cubiceasein")
        ComboBox2.Items.Add("cubiceaseinout")
        ComboBox2.Items.Add("cubiceaseoutin")
        ComboBox2.Items.Add("quarteaseout")
        ComboBox2.Items.Add("quarteasein")
        ComboBox2.Items.Add("quarteaseinout")
        ComboBox2.Items.Add("quarteaseoutin")
        ComboBox2.Items.Add("quinteaseout")
        ComboBox2.Items.Add("quinteasein")
        ComboBox2.Items.Add("quinteaseinout")
        ComboBox2.Items.Add("quinteaseoutin")
        ComboBox2.Items.Add("elasticeaseout")
        ComboBox2.Items.Add("elasticeasein")
        ComboBox2.Items.Add("elasticeaseinout")
        ComboBox2.Items.Add("elasticeaseoutin")
        ComboBox2.Items.Add("bounceeaseout")
        ComboBox2.Items.Add("bounceeasein")
        ComboBox2.Items.Add("bounceeaseinout")
        ComboBox2.Items.Add("bounceeaseoutin")
        ComboBox2.Items.Add("backeaseout")
        ComboBox2.Items.Add("backeasein")
        ComboBox2.Items.Add("backeaseinout")
        ComboBox2.Items.Add("backeaseoutin")

        ComboBox2.SelectedItem = "cubiceaseinout"

        If Not System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
            Try
                Dim address As String = "https://raw.githubusercontent.com/ChrisAnd1998/FalconX-Center-Taskbar/master/VERSION"
                Dim client As WebClient = New WebClient()
                client.CachePolicy = New System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
                Dim reader As StreamReader = New StreamReader(client.OpenRead(address))

                Dim latest = reader.ReadToEnd.ToString

                If latest.Contains(Assembly.GetExecutingAssembly().GetName().Version.ToString()) Then
                    vers.Text = "You are up to date."
                Else
                    If latest.Substring(0, 7).ToString.Replace(".", "") <= Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(".", "") Then
                        vers.Text = "This is a Pre-Release! Unstable."
                    Else
                        vers.Text = "Update " & latest.Substring(0, 7) & " is available!"
                    End If

                End If

                reader.Dispose()
                client.Dispose()
            Catch
            End Try
        Else
            bb.Visibility = Visibility.Hidden
            vers.Visibility = Visibility.Hidden
        End If

        Try

            Using ts As TaskService = New TaskService()

                Dim td = ts.GetTask("TaskbarX")

                Dim cfg As String = Nothing

                If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
                    cfg = td.Definition.Actions.ToString.Replace("cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX", "")
                Else
                    cfg = td.Definition.Actions.ToString.Replace(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", "")
                End If

                Dim arguments() As String = cfg.Split(CType(" ", Char()))

                For Each argument In arguments
                    Dim val() As String = Split(argument, "=")
                    Console.WriteLine(val(0))
                    If argument.Contains("-tbs") Then
                        If CInt(val(1)) = 0 Then
                            RadioButton1.IsChecked = True
                        End If
                        If CInt(val(1)) = 1 Then
                            RadioButton2.IsChecked = True
                        End If
                        If CInt(val(1)) = 2 Then
                            RadioButton3.IsChecked = True
                        End If
                        If CInt(val(1)) = 3 Then
                            RadioButton4.IsChecked = True
                        End If
                        If CInt(val(1)) = 4 Then
                            RadioButtontc.IsChecked = True
                        End If
                        If CInt(val(1)) = 5 Then
                            RadioButtonoq.IsChecked = True
                        End If

                    End If

                    If argument.Contains("-color") Then
                        Dim colorval As String = val(1)
                        Dim colorsep = colorval.Split(CType(";", Char()))

                        sRed.Value = CDbl(colorsep(0))
                        sGreen.Value = CDbl(colorsep(1))
                        sBlue.Value = CDbl(colorsep(2))
                        sAlpha.Value = CDbl(colorsep(3))
                    End If

                    If argument.Contains("-ptbo") Then
                        NumericUpDown1.Text = val(1)
                    End If
                    If argument.Contains("-stbo") Then
                        NumericUpDown2.Text = val(1)
                    End If
                    If argument.Contains("-cpo") Then
                        If val(1) = "1" Then
                            CheckBox2.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cso") Then
                        If val(1) = "1" Then
                            CheckBox3.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-as") Then
                        ComboBox1.SelectedItem = val(1)

                    End If
                    If argument.Contains("-asp") Then
                        NumericUpDown4.Text = val(1)
                    End If
                    If argument.Contains("-sr") Then
                        NumericUpDown7.Text = val(1)
                    End If
                    If argument.Contains("-lr") Then
                        NumericUpDown3.Text = val(1)
                    End If
                    If argument.Contains("-cib") Then
                        If val(1) = "1" Then
                            CheckBox1.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-obas") Then
                        ComboBox2.SelectedItem = val(1)
                    End If
                    If argument.Contains("-oblr") Then
                        NumericUpDown5.Text = val(1)
                    End If
                    If argument.Contains("-ftotc") Then
                        If val(1) = "1" Then
                            CheckBox4.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-dtbsowm") Then
                        If val(1) = "1" Then
                            Checkbox10.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cfsa") Then
                        If val(1) = "1" Then
                            Checkbox9.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-dct") Then
                        If val(1) = "1" Then
                            CheckBox11.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hps") Then
                        If val(1) = "1" Then
                            Checkbox12.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hss") Then
                        If val(1) = "1" Then
                            Checkbox13.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hpt") Then
                        If val(1) = "1" Then
                            Checkbox14.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hst") Then
                        If val(1) = "1" Then
                            Checkbox15.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-sti") Then
                        If val(1) = "1" Then
                            Checkbox16.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-console") Then
                        checkboxconsole.IsChecked = True
                    End If

                Next

                Console.WriteLine(td.Definition.Actions.ToString)

                Dim lg As LogonTrigger = CType(td.Definition.Triggers.Item(0), LogonTrigger)
                Dim times As TimeSpan = lg.Delay

                NumericUpDown6.Value = times.Seconds
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Private Sub ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)

        Dim item = CType(sender, ListBox)
        Dim index = item.SelectedIndex

        TabControl1.SelectedIndex = index

    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Public Sub Button_Click_1(sender As Object, e As RoutedEventArgs)

        'Kill every other running instance of FalconX

        Try
            For Each prog As Process In Process.GetProcesses
                If prog.ProcessName = "TaskbarX" Then
                    prog.Kill()
                End If
            Next
        Catch ex As Exception

        End Try

        System.Threading.Thread.Sleep(50)

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf RevertToZero)
        t1.Start()

        System.Threading.Thread.Sleep(1000)

        'ResetTaskbarStyle()

        Dim parameters As String

        If RadioButton1.IsChecked = True Then
            parameters = parameters & "-tbs=0 "
        End If
        If RadioButton2.IsChecked = True Then
            parameters = parameters & "-tbs=1 "
        End If
        If RadioButton3.IsChecked = True Then
            parameters = parameters & "-tbs=2 "
        End If
        If RadioButton4.IsChecked = True Then
            parameters = parameters & "-tbs=3 "
        End If
        If RadioButtontc.IsChecked = True Then
            parameters = parameters & "-tbs=4 "
        End If
        If RadioButtonoq.IsChecked = True Then
            parameters = parameters & "-tbs=5 "
        End If

        parameters = parameters & "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

        If Not ComboBox1.SelectedItem Is Nothing Then
            parameters = parameters & "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not ComboBox2.SelectedItem Is Nothing Then
            parameters = parameters & "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not NumericUpDown4.Text = Nothing Then
            parameters = parameters & "-asp=" & NumericUpDown4.Text & " "
        End If

        If Not NumericUpDown1.Text = Nothing Then
            parameters = parameters & "-ptbo=" & NumericUpDown1.Text & " "
        End If
        If Not NumericUpDown2.Text = Nothing Then
            parameters = parameters & "-stbo=" & NumericUpDown2.Text & " "
        End If

        If CheckBox1.IsChecked = True Then
            parameters = parameters & "-cib=1 "
        End If

        If Not NumericUpDown3.Text = Nothing Then
            parameters = parameters & "-lr=" & NumericUpDown3.Text & " "
        End If

        If Not NumericUpDown5.Text = Nothing Then
            parameters = parameters & "-oblr=" & NumericUpDown5.Text & " "
        End If

        If Not NumericUpDown7.Text = Nothing Then
            parameters = parameters & "-sr=" & NumericUpDown7.Text & " "
        End If

        If CheckBox2.IsChecked = True Then
            parameters = parameters & "-cpo=1 "
        End If

        If CheckBox3.IsChecked = True Then
            parameters = parameters & "-cso=1 "
        End If

        If CheckBox4.IsChecked = True Then
            parameters = parameters & "-ftotc=1 "
        End If

        If Checkbox10.IsChecked = True Then
            parameters = parameters & "-dtbsowm=1 "
        End If
        If Checkbox9.IsChecked = True Then
            parameters = parameters & "-cfsa=1 "
        End If
        If CheckBox11.IsChecked = True Then
            parameters = parameters & "-dct=1 "
        End If
        If Checkbox12.IsChecked = True Then
            parameters = parameters & "-hps=1 "
        End If
        If Checkbox13.IsChecked = True Then
            parameters = parameters & "-hss=1 "
        End If
        If Checkbox14.IsChecked = True Then
            parameters = parameters & "-hpt=1 "
        End If
        If Checkbox15.IsChecked = True Then
            parameters = parameters & "-hst=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters = parameters & "-sti=1 "
        End If
        If checkboxconsole.IsChecked = True Then
            parameters = parameters & "-console "
        End If

        Try
            Using ts As TaskService = New TaskService()
                ts.RootFolder.DeleteTask("TaskbarX")
            End Using
        Catch ex As Exception
            ' MessageBox.Show(ex.Message)
        End Try

        Try
            Using ts As TaskService = New TaskService()

                Dim td As TaskDefinition = ts.NewTask()
                Dim delay As Integer = CInt(NumericUpDown6.Text)

                td.RegistrationInfo.Description = "Center taskbar icons"

                td.Triggers.Add(New LogonTrigger With {
                    .UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                    .Delay = TimeSpan.FromSeconds(delay)})

                td.Settings.DisallowStartIfOnBatteries = False
                td.Settings.StopIfGoingOnBatteries = False
                td.Settings.RunOnlyIfIdle = False
                td.Settings.IdleSettings.RestartOnIdle = False
                td.Settings.IdleSettings.StopOnIdleEnd = False
                td.Settings.Hidden = True
                td.Settings.ExecutionTimeLimit = TimeSpan.Zero
                td.RegistrationInfo.Author = "Chris Andriessen"

                If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then

                    td.Actions.Add(New ExecAction("cmd.exe", "/c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters, Nothing))

                    Dim processInfo As ProcessStartInfo = New ProcessStartInfo()
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden
                    processInfo.FileName = "cmd.exe"
                    processInfo.Arguments = " /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters
                    Process.Start(processInfo)
                Else

                    td.Actions.Add(New ExecAction(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", parameters, Nothing))

                    System.Diagnostics.Process.Start("TaskbarX.exe", parameters)

                End If

                ts.RootFolder.RegisterTaskDefinition("TaskbarX", td)

            End Using
        Catch ex As Exception
            ' MessageBox.Show(ex.Message)
        End Try

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

    Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
        'Kill every other running instance of FalconX

        Try
            For Each prog As Process In Process.GetProcesses
                If prog.ProcessName = "TaskbarX" Then
                    prog.Kill()
                End If
            Next
        Catch
        End Try

        System.Threading.Thread.Sleep(50)

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf RevertToZero)
        t1.Start()

        ' ResetTaskbarStyle()
    End Sub

    Private Async Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
        Try
            Using ts As TaskService = New TaskService()
                ts.RootFolder.DeleteTask("TaskbarX")
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Dim deleteFileDialog As ContentDialog = New ContentDialog With {
        .Title = "Taskscheduler",
        .Content = "Taskschedule Removed!",
        .PrimaryButtonText = "Ok"
    }
        Dim result As ContentDialogResult = Await deleteFileDialog.ShowAsync()

    End Sub

    Private Async Sub Button_Click_33(sender As Object, e As RoutedEventArgs)

        Dim parameters As String

        If RadioButton1.IsChecked = True Then
            parameters = parameters & "-tbs=0 "
        End If
        If RadioButton2.IsChecked = True Then
            parameters = parameters & "-tbs=1 "
        End If
        If RadioButton3.IsChecked = True Then
            parameters = parameters & "-tbs=2 "
        End If
        If RadioButton4.IsChecked = True Then
            parameters = parameters & "-tbs=3 "
        End If
        If RadioButtontc.IsChecked = True Then
            parameters = parameters & "-tbs=4 "
        End If
        If RadioButtonoq.IsChecked = True Then
            parameters = parameters & "-tbs=5 "
        End If

        parameters = parameters & "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

        If Not ComboBox1.SelectedItem Is Nothing Then
            parameters = parameters & "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not ComboBox2.SelectedItem Is Nothing Then
            parameters = parameters & "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not NumericUpDown4.Text = Nothing Then
            parameters = parameters & "-asp=" & NumericUpDown4.Text & " "
        End If

        If Not NumericUpDown1.Text = Nothing Then
            parameters = parameters & "-ptbo=" & NumericUpDown1.Text & " "
        End If
        If Not NumericUpDown2.Text = Nothing Then
            parameters = parameters & "-stbo=" & NumericUpDown2.Text & " "
        End If

        If CheckBox1.IsChecked = True Then
            parameters = parameters & "-cib=1 "
        End If

        If Not NumericUpDown3.Text = Nothing Then
            parameters = parameters & "-lr=" & NumericUpDown3.Text & " "
        End If

        If Not NumericUpDown5.Text = Nothing Then
            parameters = parameters & "-oblr=" & NumericUpDown5.Text & " "
        End If

        If Not NumericUpDown7.Text = Nothing Then
            parameters = parameters & "-sr=" & NumericUpDown7.Text & " "
        End If

        If CheckBox2.IsChecked = True Then
            parameters = parameters & "-cpo=1 "
        End If

        If CheckBox3.IsChecked = True Then
            parameters = parameters & "-cso=1 "
        End If

        If CheckBox4.IsChecked = True Then
            parameters = parameters & "-ftotc=1 "
        End If

        If Checkbox10.IsChecked = True Then
            parameters = parameters & "-dtbsowm=1 "
        End If
        If Checkbox9.IsChecked = True Then
            parameters = parameters & "-cfsa=1 "
        End If
        If CheckBox11.IsChecked = True Then
            parameters = parameters & "-dct=1 "
        End If
        If Checkbox12.IsChecked = True Then
            parameters = parameters & "-hps=1 "
        End If
        If Checkbox13.IsChecked = True Then
            parameters = parameters & "-hss=1 "
        End If
        If Checkbox14.IsChecked = True Then
            parameters = parameters & "-hpt=1 "
        End If
        If Checkbox15.IsChecked = True Then
            parameters = parameters & "-hst=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters = parameters & "-sti=1 "
        End If
        If checkboxconsole.IsChecked = True Then
            parameters = parameters & "-console "
        End If

        Try
            Using ts As TaskService = New TaskService()
                ts.RootFolder.DeleteTask("TaskbarX")
            End Using
        Catch ex As Exception
            ' MessageBox.Show(ex.Message)
        End Try

        Try
            Using ts As TaskService = New TaskService()

                Dim td As TaskDefinition = ts.NewTask()
                Dim delay As Integer = CInt(NumericUpDown6.Text)

                td.RegistrationInfo.Description = "Center taskbar icons"

                td.Triggers.Add(New LogonTrigger With {
                    .UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                    .Delay = TimeSpan.FromSeconds(delay)})

                td.Settings.DisallowStartIfOnBatteries = False
                td.Settings.StopIfGoingOnBatteries = False
                td.Settings.RunOnlyIfIdle = False
                td.Settings.IdleSettings.RestartOnIdle = False
                td.Settings.IdleSettings.StopOnIdleEnd = False
                td.Settings.Hidden = True
                td.Settings.ExecutionTimeLimit = TimeSpan.Zero
                td.RegistrationInfo.Author = "Chris Andriessen"

                If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then

                    td.Actions.Add(New ExecAction("cmd.exe", "/c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters, Nothing))
                Else

                    td.Actions.Add(New ExecAction(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", parameters, Nothing))

                End If

                ts.RootFolder.RegisterTaskDefinition("TaskbarX", td)

            End Using
        Catch ex As Exception
            ' MessageBox.Show(ex.Message)
        End Try

        Dim deleteFileDialog As ContentDialog = New ContentDialog With {
        .Title = "Taskscheduler",
        .Content = "Taskschedule Created!",
        .PrimaryButtonText = "Ok"
    }
        Dim result As ContentDialogResult = Await deleteFileDialog.ShowAsync()

    End Sub

    Private Sub NumberValidationTextBox(ByVal sender As Object, ByVal e As TextCompositionEventArgs)

        If Not Char.IsNumber(CChar(e.Text)) Then
            If Not e.Text = "-" Then
                e.Handled = True
            End If
        End If

    End Sub

    Private Sub Button_Click_4(sender As Object, e As RoutedEventArgs)

        Try
            Dim address As String = "https://raw.githubusercontent.com/ChrisAnd1998/FalconX-Center-Taskbar/master/VERSION"
            Dim client As WebClient = New WebClient()
            client.CachePolicy = New System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
            Dim reader As StreamReader = New StreamReader(client.OpenRead(address))

            Dim latest = reader.ReadToEnd.ToString

            If latest.Contains(Assembly.GetExecutingAssembly().GetName().Version.ToString()) Then
                vers.Text = "You are up to date."
            Else
                If latest.Substring(0, 7).ToString.Replace(".", "") <= Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(".", "") Then
                    vers.Text = "This is a Pre-Release! Unstable."
                Else
                    vers.Text = "Update " & latest.Substring(0, 7) & " is available!"
                    Process.Start("https://chrisandriessen.nl/taskbarx")
                End If

            End If

            reader.Dispose()
            client.Dispose()
        Catch
        End Try
    End Sub

    Private Sub Button_Click_5(sender As Object, e As RoutedEventArgs)
        Try

            Using ts As TaskService = New TaskService()

                Dim td = ts.GetTask("TaskbarX")

                Dim cfg As String = Nothing

                If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
                    cfg = td.Definition.Actions.ToString.Replace("cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX", "")
                Else
                    cfg = td.Definition.Actions.ToString.Replace(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", "")
                End If

                Dim arguments() As String = cfg.Split(CType(" ", Char()))

                For Each argument In arguments
                    Dim val() As String = Split(argument, "=")
                    Console.WriteLine(val(0))
                    If argument.Contains("-tbs") Then
                        If CInt(val(1)) = 0 Then
                            RadioButton1.IsChecked = True
                        End If
                        If CInt(val(1)) = 1 Then
                            RadioButton2.IsChecked = True
                        End If
                        If CInt(val(1)) = 2 Then
                            RadioButton3.IsChecked = True
                        End If
                        If CInt(val(1)) = 3 Then
                            RadioButton4.IsChecked = True
                        End If

                        If CInt(val(1)) = 4 Then
                            RadioButtontc.IsChecked = True
                        End If
                        If CInt(val(1)) = 5 Then
                            RadioButtonoq.IsChecked = True
                        End If

                    End If

                    If argument.Contains("-color") Then
                        Dim colorval As String = val(1)
                        Dim colorsep = colorval.Split(CType(";", Char()))

                        sRed.Value = CDbl(colorsep(0))
                        sGreen.Value = CDbl(colorsep(1))
                        sBlue.Value = CDbl(colorsep(2))
                        sAlpha.Value = CDbl(colorsep(3))
                    End If

                    If argument.Contains("-ptbo") Then
                        NumericUpDown1.Text = val(1)
                    End If
                    If argument.Contains("-stbo") Then
                        NumericUpDown2.Text = val(1)
                    End If
                    If argument.Contains("-cpo") Then
                        If val(1) = "1" Then
                            CheckBox2.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cso") Then
                        If val(1) = "1" Then
                            CheckBox3.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-as") Then
                        ComboBox1.SelectedItem = val(1)

                    End If
                    If argument.Contains("-asp") Then
                        NumericUpDown4.Text = val(1)
                    End If
                    If argument.Contains("-sr") Then
                        NumericUpDown7.Text = val(1)
                    End If
                    If argument.Contains("-lr") Then
                        NumericUpDown3.Text = val(1)
                    End If
                    If argument.Contains("-cib") Then
                        If val(1) = "1" Then
                            CheckBox1.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-obas") Then
                        ComboBox2.SelectedItem = val(1)
                    End If
                    If argument.Contains("-oblr") Then
                        NumericUpDown5.Text = val(1)
                    End If
                    If argument.Contains("-ftotc") Then
                        If val(1) = "1" Then
                            CheckBox4.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-dtbsowm") Then
                        If val(1) = "1" Then
                            Checkbox10.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cfsa") Then
                        If val(1) = "1" Then
                            Checkbox9.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-dct") Then
                        If val(1) = "1" Then
                            CheckBox11.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hps") Then
                        If val(1) = "1" Then
                            Checkbox12.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hss") Then
                        If val(1) = "1" Then
                            Checkbox13.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hpt") Then
                        If val(1) = "1" Then
                            Checkbox14.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hst") Then
                        If val(1) = "1" Then
                            Checkbox15.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-sti") Then
                        If val(1) = "1" Then
                            Checkbox16.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-console") Then
                        checkboxconsole.IsChecked = True
                    End If

                Next

                Console.WriteLine(td.Definition.Actions.ToString)

                Dim lg As LogonTrigger = CType(td.Definition.Triggers.Item(0), LogonTrigger)
                Dim times As TimeSpan = lg.Delay

                NumericUpDown6.Value = times.Seconds
            End Using
        Catch
        End Try

    End Sub

    Private Sub Button_Click_6(sender As Object, e As RoutedEventArgs)
        RadioButton1.IsChecked = True
        Checkbox10.IsChecked = False
        ComboBox1.SelectedItem = "cubiceaseinout"
        ComboBox2.SelectedItem = "cubiceaseinout"
        NumericUpDown4.Text = "300"
        NumericUpDown1.Text = "0"
        NumericUpDown2.Text = "0"
        CheckBox1.IsChecked = False
        NumericUpDown6.Text = "6"
        CheckBox2.IsChecked = False
        CheckBox3.IsChecked = False
        CheckBox4.IsChecked = True
        Checkbox9.IsChecked = False
        CheckBox11.IsChecked = False
        Checkbox12.IsChecked = False
        Checkbox13.IsChecked = False
        Checkbox14.IsChecked = False
        Checkbox15.IsChecked = False
        Checkbox16.IsChecked = False
        checkboxconsole.IsChecked = False
        NumericUpDown3.Text = "400"
        NumericUpDown5.Text = "400"
        NumericUpDown7.Text = "0"
        sAlpha.Value = 50
        sRed.Value = 0
        sGreen.Value = 0
        sBlue.Value = 0
    End Sub

    Private Sub Button_Click_7(sender As Object, e As RoutedEventArgs)

        'Kill every other running instance of FalconX

        Try
            For Each prog As Process In Process.GetProcesses
                If prog.ProcessName = "TaskbarX" Then
                    prog.Kill()
                End If
            Next
        Catch ex As Exception

        End Try

        System.Threading.Thread.Sleep(50)

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf RevertToZero)
        t1.Start()

        System.Threading.Thread.Sleep(1000)

        'ResetTaskbarStyle()

        Dim parameters As String

        If RadioButton1.IsChecked = True Then
            parameters = parameters & "-tbs=0 "
        End If
        If RadioButton2.IsChecked = True Then
            parameters = parameters & "-tbs=1 "
        End If
        If RadioButton3.IsChecked = True Then
            parameters = parameters & "-tbs=2 "
        End If
        If RadioButton4.IsChecked = True Then
            parameters = parameters & "-tbs=3 "
        End If
        If RadioButtontc.IsChecked = True Then
            parameters = parameters & "-tbs=4 "
        End If
        If RadioButtonoq.IsChecked = True Then
            parameters = parameters & "-tbs=5 "
        End If

        parameters = parameters & "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

        If Not ComboBox1.SelectedItem Is Nothing Then
            parameters = parameters & "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not ComboBox2.SelectedItem Is Nothing Then
            parameters = parameters & "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not NumericUpDown4.Text = Nothing Then
            parameters = parameters & "-asp=" & NumericUpDown4.Text & " "
        End If

        If Not NumericUpDown1.Text = Nothing Then
            parameters = parameters & "-ptbo=" & NumericUpDown1.Text & " "
        End If
        If Not NumericUpDown2.Text = Nothing Then
            parameters = parameters & "-stbo=" & NumericUpDown2.Text & " "
        End If

        If CheckBox1.IsChecked = True Then
            parameters = parameters & "-cib=1 "
        End If

        If Not NumericUpDown3.Text = Nothing Then
            parameters = parameters & "-lr=" & NumericUpDown3.Text & " "
        End If

        If Not NumericUpDown5.Text = Nothing Then
            parameters = parameters & "-oblr=" & NumericUpDown5.Text & " "
        End If

        If Not NumericUpDown7.Text = Nothing Then
            parameters = parameters & "-sr=" & NumericUpDown7.Text & " "
        End If

        If CheckBox2.IsChecked = True Then
            parameters = parameters & "-cpo=1 "
        End If

        If CheckBox3.IsChecked = True Then
            parameters = parameters & "-cso=1 "
        End If

        If CheckBox4.IsChecked = True Then
            parameters = parameters & "-ftotc=1 "
        End If

        If Checkbox10.IsChecked = True Then
            parameters = parameters & "-dtbsowm=1 "
        End If
        If Checkbox9.IsChecked = True Then
            parameters = parameters & "-cfsa=1 "
        End If
        If CheckBox11.IsChecked = True Then
            parameters = parameters & "-dct=1 "
        End If
        If Checkbox12.IsChecked = True Then
            parameters = parameters & "-hps=1 "
        End If
        If Checkbox13.IsChecked = True Then
            parameters = parameters & "-hss=1 "
        End If
        If Checkbox14.IsChecked = True Then
            parameters = parameters & "-hpt=1 "
        End If
        If Checkbox15.IsChecked = True Then
            parameters = parameters & "-hst=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters = parameters & "-sti=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters = parameters & "-console "
        End If

        If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
            Dim processInfo As ProcessStartInfo = New ProcessStartInfo()
            processInfo.WindowStyle = ProcessWindowStyle.Hidden
            processInfo.FileName = "cmd.exe"
            processInfo.Arguments = " /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters
            Process.Start(processInfo)
        Else
            System.Diagnostics.Process.Start("TaskbarX.exe", parameters)
        End If

    End Sub

    Private Async Sub Button_Click_8(sender As Object, e As RoutedEventArgs)

        Dim deleteFileDialog As ContentDialog = New ContentDialog With {
        .Title = "Uninstall TaskbarX?",
        .Content = "Are you sure you want to uninstall TaskbarX?",
        .PrimaryButtonText = "Yes",
        .CloseButtonText = "No"
    }
        Dim result As ContentDialogResult = Await deleteFileDialog.ShowAsync()

        If result = ContentDialogResult.Primary Then
            Try
                Using ts As TaskService = New TaskService()
                    ts.RootFolder.DeleteTask("TaskbarX")
                End Using
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            Try
                For Each prog As Process In Process.GetProcesses
                    If prog.ProcessName = "TaskbarX" Then
                        prog.Kill()
                    End If
                Next
            Catch
            End Try

            System.Threading.Thread.Sleep(50)

            Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf RevertToZero)
            t1.Start()

            Dim ggFileDialog As ContentDialog = New ContentDialog With {
        .Title = "Good bye.",
        .Content = "Ready for uninstall." & vbNewLine & "TaskbarX will be removed within 10 seconds once you click Ok...",
        .PrimaryButtonText = "Ok"
    }
            Dim results As ContentDialogResult = Await ggFileDialog.ShowAsync()

            If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
                Dim processInfo As ProcessStartInfo = New ProcessStartInfo()
                processInfo.WindowStyle = ProcessWindowStyle.Hidden
                processInfo.FileName = "powershell.exe"
                processInfo.Arguments = " Get-AppxPackage *40210ChrisAndriessen.FalconX* | Remove-AppxPackage"
                Process.Start(processInfo)
                End
            Else
                My.Computer.FileSystem.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory & "uninstall.bat", "timeout /t 10 /nobreak & timeout /t 10 /nobreak & RD " & Chr(34) & System.AppDomain.CurrentDomain.BaseDirectory & Chr(34) & " /Q /S", True)

                If Not File.Exists(System.AppDomain.CurrentDomain.BaseDirectory & "uninstall.bat") Then
                    Dim ffFileDialog As ContentDialog = New ContentDialog With {
      .Title = "Woops...",
      .Content = "The uninstaller could not be created." & vbNewLine & "Please remove the install directory manually.",
      .PrimaryButtonText = "Ok"
  }
                    Dim results2 As ContentDialogResult = Await ffFileDialog.ShowAsync()

                    Process.Start(System.AppDomain.CurrentDomain.BaseDirectory)
                    End
                End If

                Dim processInfo As ProcessStartInfo = New ProcessStartInfo()
                processInfo.WindowStyle = ProcessWindowStyle.Hidden
                processInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory & "uninstall.bat"
                processInfo.Arguments = ""
                Process.Start(processInfo)
                End
            End If
        Else
        End If

    End Sub

    Private Sub Button_Click_9(sender As Object, e As RoutedEventArgs)
        callanim()

    End Sub

    Private Sub callanim()
        Dim xx As Integer = CInt(NumericUpDown4.Value)
        Dim an As String = ComboBox1.Text

        If an = "linear" Then
            Dim t1 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.Linear, 500, xx))
            t1.Start()
        ElseIf an = "none" Then
            'Dim t0 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.Linear, 500, 1))
            ' t0.Start()
        ElseIf an = "expoeaseout" Then
            Dim t2 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ExpoEaseOut, 500, xx))
            t2.Start()
        ElseIf an = "expoeasein" Then
            Dim t3 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ExpoEaseIn, 500, xx))
            t3.Start()
        ElseIf an = "expoeaseinout" Then
            Dim t4 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ExpoEaseInOut, 500, xx))
            t4.Start()
        ElseIf an = "expoeaseoutin" Then
            Dim t5 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ExpoEaseOutIn, 500, xx))
            t5.Start()
        ElseIf an = "circeaseout" Then
            Dim t6 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CircEaseOut, 500, xx))
            t6.Start()
        ElseIf an = "circeasein" Then
            Dim t7 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CircEaseIn, 500, xx))
            t7.Start()
        ElseIf an = "circeaseinout" Then
            Dim t8 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CircEaseInOut, 500, xx))
            t8.Start()
        ElseIf an = "circeaseoutin" Then
            Dim t9 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CircEaseOutIn, 500, xx))
            t9.Start()
        ElseIf an = "quadeaseout" Then
            Dim t10 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuadEaseOut, 500, xx))
            t10.Start()
        ElseIf an = "quadeasein" Then
            Dim t11 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuadEaseIn, 500, xx))
            t11.Start()
        ElseIf an = "quadeaseinout" Then
            Dim t12 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuadEaseInOut, 500, xx))
            t12.Start()
        ElseIf an = "quadeaseoutin" Then
            Dim t13 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuadEaseOutIn, 500, xx))
            t13.Start()
        ElseIf an = "sineeaseout" Then
            Dim t14 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.SineEaseOut, 500, xx))
            t14.Start()
        ElseIf an = "sineeasein" Then
            Dim t15 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.SineEaseIn, 500, xx))
            t15.Start()
        ElseIf an = "sineeaseinout" Then
            Dim t16 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.SineEaseInOut, 500, xx))
            t16.Start()
        ElseIf an = "sineeaseoutin" Then
            Dim t17 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.SineEaseOutIn, 500, xx))
            t17.Start()
        ElseIf an = "cubiceaseout" Then
            Dim t18 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CubicEaseOut, 500, xx))
            t18.Start()
        ElseIf an = "cubiceasein" Then
            Dim t19 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CubicEaseIn, 500, xx))
            t19.Start()
        ElseIf an = "cubiceaseinout" Then
            Dim t20 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CubicEaseInOut, 500, xx))
            t20.Start()
        ElseIf an = "cubiceaseoutin" Then
            Dim t21 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.CubicEaseOutIn, 500, xx))
            t21.Start()
        ElseIf an = "quarteaseout" Then
            Dim t22 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuartEaseOut, 500, xx))
            t22.Start()
        ElseIf an = "quarteasein" Then
            Dim t23 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuartEaseIn, 500, xx))
            t23.Start()
        ElseIf an = "quarteaseinout" Then
            Dim t24 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuartEaseInOut, 500, xx))
            t24.Start()
        ElseIf an = "quarteaseoutin" Then
            Dim t25 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuartEaseOutIn, 500, xx))
            t25.Start()
        ElseIf an = "quinteaseout" Then
            Dim t26 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuintEaseOut, 500, xx))
            t26.Start()
        ElseIf an = "quinteasein" Then
            Dim t27 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuintEaseIn, 500, xx))
            t27.Start()
        ElseIf an = "quinteaseinout" Then
            Dim t28 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuintEaseInOut, 500, xx))
            t28.Start()
        ElseIf an = "quinteaseoutin" Then
            Dim t29 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.QuintEaseOutIn, 500, xx))
            t29.Start()
        ElseIf an = "elasticeaseout" Then
            Dim t30 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ElasticEaseOut, 500, xx))
            t30.Start()
        ElseIf an = "elasticeasein" Then
            Dim t31 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ElasticEaseIn, 500, xx))
            t31.Start()
        ElseIf an = "elasticeaseinout" Then
            Dim t32 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ElasticEaseInOut, 500, xx))
            t32.Start()
        ElseIf an = "elasticeaseoutin" Then
            Dim t33 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.ElasticEaseOutIn, 500, xx))
            t33.Start()
        ElseIf an = "bounceeaseout" Then
            Dim t34 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BounceEaseOut, 500, xx))
            t34.Start()
        ElseIf an = "bounceeasein" Then
            Dim t35 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BounceEaseIn, 500, xx))
            t35.Start()
        ElseIf an = "bounceeaseinout" Then
            Dim t36 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BounceEaseInOut, 500, xx))
            t36.Start()
        ElseIf an = "bounceeaseoutin" Then
            Dim t37 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BounceEaseOutIn, 500, xx))
            t37.Start()
        ElseIf an = "backeaseout" Then
            Dim t38 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BackEaseOut, 500, xx))
            t38.Start()
        ElseIf an = "backeasein" Then
            Dim t39 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BackEaseIn, 500, xx))
            t39.Start()
        ElseIf an = "backeaseinout" Then
            Dim t40 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BackEaseInOut, 500, xx))
            t40.Start()
        ElseIf an = "backeaseoutin" Then
            Dim t41 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.BackEaseOutIn, 500, xx))
            t41.Start()
        End If
    End Sub

    Private Sub Animate(ByVal hwnd As IntPtr, ByVal oldpos As Integer, ByVal orient As String, ByVal easing As EasingDelegate, ByVal valueToReach As Integer, ByVal duration As Integer)
        Try

            Dim sw As New Stopwatch
            Dim originalValue As Integer = oldpos
            Dim elapsed As New Integer
            Dim minValue As Integer = Math.Min(originalValue, valueToReach)
            Dim maxValue As Integer = Math.Abs(valueToReach - originalValue)
            Dim increasing As Boolean = originalValue < valueToReach

            elapsed = 0
            sw.Start()

            While Not elapsed >= duration
                elapsed = CInt(sw.ElapsedMilliseconds)

                Dim newValue As Integer = CInt(Math.Truncate(easing(elapsed, minValue, maxValue, duration)))

                If Not increasing Then
                    newValue = (originalValue + valueToReach) - newValue
                End If

                Me.Dispatcher.Invoke(Sub()
                                         Slider1.Value = newValue
                                     End Sub)

                Console.WriteLine(newValue)

            End While

            sw.Stop()

            Me.Dispatcher.Invoke(Sub()
                                     Slider1.Value = 0
                                 End Sub)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Private Sub Button_Click_10(sender As Object, e As RoutedEventArgs)
        ComboBox1.SelectedIndex = ComboBox1.SelectedIndex + 1

    End Sub

    Private Sub Button_Click_11(sender As Object, e As RoutedEventArgs)
        ComboBox1.SelectedIndex = ComboBox1.SelectedIndex - 1

    End Sub

    Private Sub Alpha_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sAlpha.Value)
        tAlpha.Text = CType(val, String) & "%"

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100

    End Sub

    Private Sub Blue_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sBlue.Value)
        tBlue.Text = CType(val, String)

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100
    End Sub

    Private Sub Green_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sGreen.Value)
        tGreen.Text = CType(val, String)

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100

    End Sub

    Private Sub Red_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sRed.Value)
        tRed.Text = CType(val, String)

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100
    End Sub

    Private Sub Button_Click_12(sender As Object, e As RoutedEventArgs)

        Dim t1 As Thread = New Thread(AddressOf colorthread)
        t1.Start()

    End Sub

    Sub colorthread()
        Dim lpPoint As PointAPI
        Dim x = GetAsyncKeyState(1) = 0

        Do
            System.Threading.Thread.Sleep(1)
            GetCursorPos(lpPoint)

            Console.WriteLine(GetColorAt(lpPoint.x, lpPoint.y))

            Dim colorp As Color = GetColorAt(lpPoint.x, lpPoint.y)

            Me.Dispatcher.Invoke(Sub()
                                     ' sAlpha.Value = colorp.A
                                     sRed.Value = colorp.R
                                     sGreen.Value = colorp.G
                                     sBlue.Value = colorp.B
                                 End Sub)

        Loop Until Not GetAsyncKeyState(1) = 0
    End Sub

End Class