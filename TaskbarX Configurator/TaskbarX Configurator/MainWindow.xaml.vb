Option Strict On

Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Runtime
Imports System.Runtime.InteropServices
Imports System.Security.Principal
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

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function FindWindowEx(ByVal parentHandle As IntPtr, ByVal childAfter As IntPtr, ByVal lclassName As String, ByVal windowTitle As String) As IntPtr
    End Function

    Public Declare Function GetCursorPos Lib "user32" (ByRef lpPoint As PointAPI) As Boolean

    Declare Function GetAsyncKeyState Lib "user32" (ByVal vkey As Integer) As Short

    <System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint:="SetWindowLong")>
    Public Shared Function SetWindowLong(ByVal hWnd As IntPtr, <MarshalAs(UnmanagedType.I4)> nIndex As WindowStyles, ByVal dwNewLong As Integer) As Integer
    End Function

    Public Enum WindowStyles
        WS_BORDER = &H800000
        WS_CAPTION = &HC00000
        WS_CHILD = &H40000000
        WS_CLIPCHILDREN = &H2000000
        WS_CLIPSIBLINGS = &H4000000
        WS_DISABLED = &H8000000
        WS_DLGFRAME = &H400000
        WS_GROUP = &H20000
        WS_HSCROLL = &H100000
        WS_MAXIMIZE = &H1000000
        WS_MAXIMIZEBOX = &H10000
        WS_MINIMIZE = &H20000000
        WS_MINIMIZEBOX = &H20000
        WS_OVERLAPPED = &H0
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED Or WS_CAPTION Or WS_SYSMENU Or WS_SIZEFRAME Or WS_MINIMIZEBOX Or WS_MAXIMIZEBOX
        WS_SIZEFRAME = &H40000
        WS_SYSMENU = &H80000
        WS_TABSTOP = &H10000
        WS_VISIBLE = &H10000000
        WS_VSCROLL = &H200000
    End Enum
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

    Friend Declare Function SetWindowCompositionAttribute Lib "user32.dll" (ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    Private Declare Auto Function FindWindow Lib "user32.dll" (ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr

    Structure PointAPI
        Public x As Int32
        Public y As Int32
    End Structure

    Public Const GWL_STYLE = -16
    Public Const GWL_EXSTYLE = -20
    Public Const WS_MAXIMIZE = 16777216
    Public Const WS_POPUP = 2147483648
    Public Const WS_EX_LAYERED As Integer = 524288

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

        Dim maintaskbarfound As Boolean = False
        Dim sectaskbarfound As Boolean = False

        For Each Taskbar In windowHandles
            Dim sClassName As New StringBuilder("", 256)
            Call GetClassName(CType(Taskbar, IntPtr), sClassName, 256)
            If sClassName.ToString = "Shell_TrayWnd" Then
                maintaskbarfound = True
            End If
            If sClassName.ToString = "Shell_SecondaryTrayWnd" Then
                sectaskbarfound = True
            End If
            Console.WriteLine("=" & maintaskbarfound)
        Next

        If maintaskbarfound = False Then
            Try
                windowHandles.Add(FindWindow("Shell_TrayWnd", Nothing))
            Catch
            End Try
        End If

        If sectaskbarfound = False Then

            Try
                If Not FindWindow("Shell_SecondaryTrayWnd", Nothing) = CType(0, IntPtr) Then
                    If Not FindWindow("Shell_SecondaryTrayWnd", Nothing) = Nothing Then
                        windowHandles.Add(FindWindow("Shell_SecondaryTrayWnd", Nothing))
                    End If
                End If

            Catch
            End Try

        End If


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
                Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                    .WindowStyle = ProcessWindowStyle.Hidden,
                    .FileName = "explorer.exe",
                    .Arguments = " taskbarx:" & Chr(34) & "-stop" & Chr(34)
                }
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

    Private ReadOnly bmp As Bitmap = New Bitmap(1, 1)

    Private Function GetColorAt(ByVal x As Integer, ByVal y As Integer) As Color
        Dim bounds As Rectangle = New Rectangle(x, y, 1, 1)

        Using g As Graphics = Graphics.FromImage(bmp)
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size)
        End Using

        Return bmp.GetPixel(0, 0)
    End Function





    Private Async Sub Window_Loaded(ByVal sender As Object, ByVal e As EventArgs)

        InitializeComponent()



        If Me.Background.ToString = "#FF000000" Then
            Dim mySolidColorBrush As SolidColorBrush = New SolidColorBrush()
            mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(255, 10, 10, 10)
            Dim mySolidColorBrush2 As SolidColorBrush = New SolidColorBrush()
            mySolidColorBrush2.Color = System.Windows.Media.Color.FromArgb(255, 20, 20, 20)
            placeholder.Fill = mySolidColorBrush
            Me.Background = mySolidColorBrush
            ListBox1.Background = mySolidColorBrush2
        End If

        If Me.Background.ToString = "#FFFFFFFF" Then
            Dim mySolidColorBrush As SolidColorBrush = New SolidColorBrush()
            mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(255, 240, 240, 240)
            Dim mySolidColorBrush2 As SolidColorBrush = New SolidColorBrush()
            mySolidColorBrush2.Color = System.Windows.Media.Color.FromArgb(255, 255, 255, 255)
            placeholder.Fill = mySolidColorBrush
            Me.Background = mySolidColorBrush2
            ListBox1.Background = mySolidColorBrush
        End If


        Dim identity = WindowsIdentity.GetCurrent()
        Dim principal = New WindowsPrincipal(identity)
        Dim isElevated As Boolean = principal.IsInRole(WindowsBuiltInRole.Administrator)
        If isElevated Then
            Dim adminDialog As ContentDialog = New ContentDialog With {
        .Title = "Warning!",
        .Content = "Please DON'T run the Configurator as Administrator. This may cause the start-up task not to work properly!",
        .PrimaryButtonText = "Ok"
    }
            Dim result As ContentDialogResult = Await adminDialog.ShowAsync()
        End If

        'Checkbox10.Visibility = Visibility.Hidden

        ListBox1.SelectedIndex = 0

        sAlpha.Value = 50
        tpop.Value = 100
        tsop.Value = 100

        tbrounding.Value = 0

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





        If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
            startbutton_shortcut.Text = "explorer.exe taskbarx: -showstartmenu"
        Else


            startbutton_shortcut.Text = Chr(34) & System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe" & Chr(34) & " -showstartmenu"

        End If







        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12

        If Not System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
            Try
                Dim address As String = "https://raw.githubusercontent.com/ChrisAnd1998/FalconX-Center-Taskbar/master/VERSION"
                Dim client As WebClient = New WebClient With {
                    .CachePolicy = New System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
                }
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

                Dim td = ts.GetTask("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""))

                Dim cfg As String = Nothing



                If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then

                    ''cfg = td.Definition.Actions.ToString.Replace("cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX", "")
                    cfg = td.Definition.Actions.ToString.Replace("explorer.exe taskbarx:", "")
                Else
                    cfg = td.Definition.Actions.ToString.Replace(Chr(34) & System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe" & Chr(34), "")
                End If

                Dim arguments() As String = cfg.Split(CType(" ", Char()))

                For Each argument In arguments
                    Dim val() As String = Split(argument, "=")
                    Console.WriteLine(val(0))
                    If argument.Contains("-tbs=") Then

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

                    If argument.Contains("-color=") Then
                        Dim colorval As String = val(1)
                        Dim colorsep = colorval.Split(CType(";", Char()))

                        sRed.Value = CDbl(colorsep(0))
                        sGreen.Value = CDbl(colorsep(1))
                        sBlue.Value = CDbl(colorsep(2))
                        sAlpha.Value = CDbl(colorsep(3))
                    End If

                    If argument.Contains("-tpop=") Then
                        tpop.Value = CDbl(val(1))
                    End If

                    If argument.Contains("-tbr=") Then
                        tbrounding.Text = val(1)
                    End If

                    If argument.Contains("-tbsg=") Then
                        If val(1) = "1" Then
                            tbsegments.IsChecked = True
                        End If
                    End If

                    If argument.Contains("-tsop=") Then
                        tsop.Value = CDbl(val(1))
                    End If

                    If argument.Contains("-ptbo=") Then
                        NumericUpDown1.Text = val(1)
                    End If
                    If argument.Contains("-stbo=") Then
                        NumericUpDown2.Text = val(1)
                    End If
                    If argument.Contains("-cpo=") Then
                        If val(1) = "1" Then
                            CheckBox2.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cso=") Then
                        If val(1) = "1" Then
                            CheckBox3.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-as=") Then
                        ComboBox1.SelectedItem = val(1)

                    End If
                    If argument.Contains("-asp=") Then
                        NumericUpDown4.Text = val(1)
                    End If
                    If argument.Contains("-sr=") Then
                        NumericUpDown7.Text = val(1)
                    End If
                    If argument.Contains("-sr2=") Then
                        NumericUpDown7_Copy.Text = val(1)
                    End If
                    If argument.Contains("-sr3=") Then
                        NumericUpDown7_Copy1.Text = val(1)
                    End If
                    If argument.Contains("-lr=") Then
                        NumericUpDown3.Text = val(1)
                    End If
                    If argument.Contains("-cib=") Then
                        If val(1) = "1" Then
                            CheckBox1.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-obas=") Then
                        ComboBox2.SelectedItem = val(1)
                    End If
                    If argument.Contains("-oblr=") Then
                        NumericUpDown5.Text = val(1)
                    End If
                    If argument.Contains("-ftotc=") Then
                        If val(1) = "1" Then
                            CheckBox4.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-rzbt=") Then
                        If val(1) = "1" Then
                            CheckBox4_Copy.IsChecked = True
                        End If
                        If val(1) = "0" Then
                            CheckBox4_Copy.IsChecked = False
                        End If
                    End If

                    If argument.Contains("-dtbsowm=") Then
                        If val(1) = "1" Then
                            Checkbox10.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cfsa=") Then
                        If val(1) = "1" Then
                            Checkbox9.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-dct=") Then
                        If val(1) = "1" Then
                            CheckBox11.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hps=") Then
                        If val(1) = "1" Then
                            Checkbox12.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hss=") Then
                        If val(1) = "1" Then
                            Checkbox13.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hpt=") Then
                        If val(1) = "1" Then
                            Checkbox14.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hst=") Then
                        If val(1) = "1" Then
                            Checkbox15.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-sti=") Then
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



        ''Override Older settings if exist
        Try
            Using mts As TaskService = New TaskService()

                Dim mtd = mts.GetTask("TaskbarX")

                If mtd.Name = "TaskbarX" Then
                    Migrate()
                End If

            End Using
        Catch
            'older Task not found
        End Try




        '' Dim Win11Taskbar = FindWindowEx(FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr)), CType(0, IntPtr), "Windows.UI.Composition.DesktopWindowContentBridge", Nothing)
        '' If Not Win11Taskbar = CType(0, IntPtr) Then

        ''ListBox1.Items.RemoveAt(1)

        '' End If

    End Sub


    Sub Migrate()


        Try

            Using ts As TaskService = New TaskService()

                Dim td = ts.GetTask("TaskbarX")


                Dim cfg As String = Nothing



                If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
                    cfg = td.Definition.Actions.ToString.Replace("cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX", "")
                    ''  cfg = td.Definition.Actions.ToString.Replace("explorer.exe taskbarx:", "")
                Else
                    cfg = td.Definition.Actions.ToString.Replace(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", "")
                End If

                Dim arguments() As String = cfg.Split(CType(" ", Char()))

                For Each argument In arguments
                    Dim val() As String = Split(argument, "=")
                    Console.WriteLine(val(0))
                    If argument.Contains("-tbs=") Then
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

                    If argument.Contains("-color=") Then
                        Dim colorval As String = val(1)
                        Dim colorsep = colorval.Split(CType(";", Char()))

                        sRed.Value = CDbl(colorsep(0))
                        sGreen.Value = CDbl(colorsep(1))
                        sBlue.Value = CDbl(colorsep(2))
                        sAlpha.Value = CDbl(colorsep(3))
                    End If

                    If argument.Contains("-tpop=") Then
                        tpop.Value = CDbl(val(1))
                    End If

                    If argument.Contains("-tbr=") Then
                        tbrounding.Text = val(1)
                    End If

                    If argument.Contains("-tbsg=") Then
                        If val(1) = "1" Then
                            tbsegments.IsChecked = True
                        End If
                    End If

                    If argument.Contains("-tsop=") Then
                        tsop.Value = CDbl(val(1))
                    End If

                    If argument.Contains("-ptbo=") Then
                        NumericUpDown1.Text = val(1)
                    End If
                    If argument.Contains("-stbo=") Then
                        NumericUpDown2.Text = val(1)
                    End If
                    If argument.Contains("-cpo=") Then
                        If val(1) = "1" Then
                            CheckBox2.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cso=") Then
                        If val(1) = "1" Then
                            CheckBox3.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-as=") Then
                        ComboBox1.SelectedItem = val(1)

                    End If
                    If argument.Contains("-asp=") Then
                        NumericUpDown4.Text = val(1)
                    End If
                    If argument.Contains("-sr=") Then
                        NumericUpDown7.Text = val(1)
                    End If
                    If argument.Contains("-sr2=") Then
                        NumericUpDown7_Copy.Text = val(1)
                    End If
                    If argument.Contains("-sr3=") Then
                        NumericUpDown7_Copy1.Text = val(1)
                    End If
                    If argument.Contains("-lr=") Then
                        NumericUpDown3.Text = val(1)
                    End If
                    If argument.Contains("-cib=") Then
                        If val(1) = "1" Then
                            CheckBox1.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-obas=") Then
                        ComboBox2.SelectedItem = val(1)
                    End If
                    If argument.Contains("-oblr=") Then
                        NumericUpDown5.Text = val(1)
                    End If
                    If argument.Contains("-ftotc=") Then
                        If val(1) = "1" Then
                            CheckBox4.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-rzbt=") Then
                        If val(1) = "1" Then
                            CheckBox4_Copy.IsChecked = True
                        End If
                        If val(1) = "0" Then
                            CheckBox4_Copy.IsChecked = False
                        End If
                    End If
                    If argument.Contains("-dtbsowm=") Then
                        If val(1) = "1" Then
                            Checkbox10.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cfsa=") Then
                        If val(1) = "1" Then
                            Checkbox9.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-dct=") Then
                        If val(1) = "1" Then
                            CheckBox11.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hps=") Then
                        If val(1) = "1" Then
                            Checkbox12.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hss=") Then
                        If val(1) = "1" Then
                            Checkbox13.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hpt=") Then
                        If val(1) = "1" Then
                            Checkbox14.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hst=") Then
                        If val(1) = "1" Then
                            Checkbox15.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-sti=") Then
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




                Dim parameters As String = ""

                If RadioButton1.IsChecked = True Then
                    parameters &= "-tbs=0 "
                End If
                If RadioButton2.IsChecked = True Then
                    parameters &= "-tbs=1 "
                End If
                If RadioButton3.IsChecked = True Then
                    parameters &= "-tbs=2 "
                End If
                If RadioButton4.IsChecked = True Then
                    parameters &= "-tbs=3 "
                End If
                If RadioButtontc.IsChecked = True Then
                    parameters &= "-tbs=4 "
                End If
                If RadioButtonoq.IsChecked = True Then
                    parameters &= "-tbs=5 "
                End If

                parameters &= "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

                parameters &= "-tpop=" & tpopla.Text.ToString.Replace("%", "") & " "

                parameters &= "-tsop=" & tsopla.Text.ToString.Replace("%", "") & " "

                If Not ComboBox1.SelectedItem Is Nothing Then
                    parameters &= "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
                End If

                If Not ComboBox2.SelectedItem Is Nothing Then
                    parameters &= "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
                End If

                If Not tbrounding.Text = Nothing Then
                    parameters &= "-tbr=" & tbrounding.Text & " "
                End If

                If tbsegments.IsChecked = True Then
                    parameters &= "-tbsg=1 "
                End If

                If Not NumericUpDown4.Text = Nothing Then
                    parameters &= "-asp=" & NumericUpDown4.Text & " "
                End If

                If Not NumericUpDown1.Text = Nothing Then
                    parameters &= "-ptbo=" & NumericUpDown1.Text & " "
                End If
                If Not NumericUpDown2.Text = Nothing Then
                    parameters &= "-stbo=" & NumericUpDown2.Text & " "
                End If

                If CheckBox1.IsChecked = True Then
                    parameters &= "-cib=1 "
                End If

                If Not NumericUpDown3.Text = Nothing Then
                    parameters &= "-lr=" & NumericUpDown3.Text & " "
                End If

                If Not NumericUpDown5.Text = Nothing Then
                    parameters &= "-oblr=" & NumericUpDown5.Text & " "
                End If

                If Not NumericUpDown7.Text = Nothing Then
                    parameters &= "-sr=" & NumericUpDown7.Text & " "
                End If

                If Not NumericUpDown7_Copy.Text = Nothing Then
                    parameters &= "-sr2=" & NumericUpDown7_Copy.Text & " "
                End If

                If Not NumericUpDown7_Copy1.Text = Nothing Then
                    parameters &= "-sr3=" & NumericUpDown7_Copy1.Text & " "
                End If

                If CheckBox2.IsChecked = True Then
                    parameters &= "-cpo=1 "
                End If

                If CheckBox3.IsChecked = True Then
                    parameters &= "-cso=1 "
                End If

                If CheckBox4.IsChecked = True Then
                    parameters &= "-ftotc=1 "
                End If

                If CheckBox4_Copy.IsChecked = True Then
                    parameters &= "-rzbt=1 "
                End If


                If CheckBox4_Copy.IsChecked = False Then
                    parameters &= "-rzbt=0 "
                End If

                If Checkbox10.IsChecked = True Then
                    parameters &= "-dtbsowm=1 "
                End If
                If Checkbox9.IsChecked = True Then
                    parameters &= "-cfsa=1 "
                End If
                If CheckBox11.IsChecked = True Then
                    parameters &= "-dct=1 "
                End If
                If Checkbox12.IsChecked = True Then
                    parameters &= "-hps=1 "
                End If
                If Checkbox13.IsChecked = True Then
                    parameters &= "-hss=1 "
                End If
                If Checkbox14.IsChecked = True Then
                    parameters &= "-hpt=1 "
                End If
                If Checkbox15.IsChecked = True Then
                    parameters &= "-hst=1 "
                End If
                If Checkbox16.IsChecked = True Then
                    parameters &= "-sti=1 "
                End If
                If checkboxconsole.IsChecked = True Then
                    parameters &= "-console "
                End If

                Try
                    Using ts2 As TaskService = New TaskService()
                        ts2.RootFolder.DeleteTask("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""))
                        ts2.RootFolder.DeleteTask("TaskbarX")
                    End Using
                Catch ex As Exception
                    ' MessageBox.Show(ex.Message)
                End Try

                Try
                    Using ts2 As TaskService = New TaskService()

                        Dim td2 As TaskDefinition = ts2.NewTask()
                        Dim delay As Integer = CInt(NumericUpDown6.Text)

                        td2.RegistrationInfo.Description = "Center taskbar icons"

                        td2.Triggers.Add(New LogonTrigger With {
                            .UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                            .Delay = TimeSpan.FromSeconds(delay)})

                        td2.Settings.DisallowStartIfOnBatteries = False
                        td2.Settings.StopIfGoingOnBatteries = False
                        td2.Settings.RunOnlyIfIdle = False
                        td2.Settings.IdleSettings.RestartOnIdle = False
                        td2.Settings.IdleSettings.StopOnIdleEnd = False
                        td2.Settings.Hidden = True
                        td2.Settings.ExecutionTimeLimit = TimeSpan.Zero
                        td2.RegistrationInfo.Author = "Chris Andriessen"

                        If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then

                            td2.Actions.Add(New ExecAction("explorer.exe", "taskbarx:" & Chr(34) & parameters & Chr(34), Nothing))
                        Else

                            td2.Actions.Add(New ExecAction(Chr(34) & System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe" & Chr(34), parameters, Nothing))

                        End If

                        ts2.RootFolder.RegisterTaskDefinition("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""), td2)

                    End Using
                Catch ex As Exception
                    ' MessageBox.Show(ex.Message)
                End Try





            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try


        Try
            Using ts3 As TaskService = New TaskService()
                ts3.RootFolder.DeleteTask("TaskbarX")
            End Using
        Catch ex As Exception
            ' MessageBox.Show(ex.Message)
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



        Dim parameters As String = ""

        If RadioButton1.IsChecked = True Then
            parameters &= "-tbs=0 "
        End If
        If RadioButton2.IsChecked = True Then
            parameters &= "-tbs=1 "
        End If
        If RadioButton3.IsChecked = True Then
            parameters &= "-tbs=2 "
        End If
        If RadioButton4.IsChecked = True Then
            parameters &= "-tbs=3 "
        End If
        If RadioButtontc.IsChecked = True Then
            parameters &= "-tbs=4 "
        End If
        If RadioButtonoq.IsChecked = True Then
            parameters &= "-tbs=5 "
        End If

        parameters &= "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

        parameters &= "-tpop=" & tpopla.Text.ToString.Replace("%", "") & " "

        parameters &= "-tsop=" & tsopla.Text.ToString.Replace("%", "") & " "

        If Not ComboBox1.SelectedItem Is Nothing Then
            parameters &= "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not ComboBox2.SelectedItem Is Nothing Then
            parameters &= "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not tbrounding.Text = Nothing Then
            parameters &= "-tbr=" & tbrounding.Text & " "
        End If

        If tbsegments.IsChecked = True Then
            parameters &= "-tbsg=1 "
        End If

        If Not NumericUpDown4.Text = Nothing Then
            parameters &= "-asp=" & NumericUpDown4.Text & " "
        End If

        If Not NumericUpDown1.Text = Nothing Then
            parameters &= "-ptbo=" & NumericUpDown1.Text & " "
        End If
        If Not NumericUpDown2.Text = Nothing Then
            parameters &= "-stbo=" & NumericUpDown2.Text & " "
        End If

        If CheckBox1.IsChecked = True Then
            parameters &= "-cib=1 "
        End If

        If Not NumericUpDown3.Text = Nothing Then
            parameters &= "-lr=" & NumericUpDown3.Text & " "
        End If

        If Not NumericUpDown5.Text = Nothing Then
            parameters &= "-oblr=" & NumericUpDown5.Text & " "
        End If

        If Not NumericUpDown7.Text = Nothing Then
            parameters &= "-sr=" & NumericUpDown7.Text & " "
        End If

        If Not NumericUpDown7_Copy.Text = Nothing Then
            parameters &= "-sr2=" & NumericUpDown7_Copy.Text & " "
        End If

        If Not NumericUpDown7_Copy1.Text = Nothing Then
            parameters &= "-sr3=" & NumericUpDown7_Copy1.Text & " "
        End If

        If CheckBox2.IsChecked = True Then
            parameters &= "-cpo=1 "
        End If

        If CheckBox3.IsChecked = True Then
            parameters &= "-cso=1 "
        End If

        If CheckBox4.IsChecked = True Then
            parameters &= "-ftotc=1 "
        End If

        If CheckBox4_Copy.IsChecked = True Then
            parameters &= "-rzbt=1 "
        End If

        If CheckBox4_Copy.IsChecked = False Then
            parameters &= "-rzbt=0 "
        End If

        If Checkbox10.IsChecked = True Then
            parameters &= "-dtbsowm=1 "
        End If
        If Checkbox9.IsChecked = True Then
            parameters &= "-cfsa=1 "
        End If
        If CheckBox11.IsChecked = True Then
            parameters &= "-dct=1 "
        End If
        If Checkbox12.IsChecked = True Then
            parameters &= "-hps=1 "
        End If
        If Checkbox13.IsChecked = True Then
            parameters &= "-hss=1 "
        End If
        If Checkbox14.IsChecked = True Then
            parameters &= "-hpt=1 "
        End If
        If Checkbox15.IsChecked = True Then
            parameters &= "-hst=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters &= "-sti=1 "
        End If
        If checkboxconsole.IsChecked = True Then
            parameters &= "-console "
        End If

        Try
            Using ts As TaskService = New TaskService()
                ts.RootFolder.DeleteTask("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""))
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

                    td.Actions.Add(New ExecAction("explorer.exe", "taskbarx:" & Chr(34) & parameters & Chr(34), Nothing))

                    Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                        .WindowStyle = ProcessWindowStyle.Hidden,
                        .FileName = "explorer.exe",
                        .Arguments = " taskbarx:" & Chr(34) & parameters & Chr(34)
                    }
                    Process.Start(processInfo)
                Else

                    td.Actions.Add(New ExecAction(Chr(34) & System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe" & Chr(34), parameters, Nothing))

                    System.Diagnostics.Process.Start("TaskbarX.exe", parameters)

                End If

                ts.RootFolder.RegisterTaskDefinition("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""), td)

            End Using
        Catch ex As Exception
            ' MessageBox.Show(ex.Message)
        End Try

    End Sub

    Public Shared WM_DWMCOLORIZATIONCOLORCHANGED As Integer = &H320
    Public Shared WM_DWMCOMPOSITIONCHANGED As Integer = &H31E
    Public Shared WM_THEMECHANGED As Integer = &H31A

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
                ts.RootFolder.DeleteTask("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""))
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

        Dim parameters As String = ""

        If RadioButton1.IsChecked = True Then
            parameters &= "-tbs=0 "
        End If
        If RadioButton2.IsChecked = True Then
            parameters &= "-tbs=1 "
        End If
        If RadioButton3.IsChecked = True Then
            parameters &= "-tbs=2 "
        End If
        If RadioButton4.IsChecked = True Then
            parameters &= "-tbs=3 "
        End If
        If RadioButtontc.IsChecked = True Then
            parameters &= "-tbs=4 "
        End If
        If RadioButtonoq.IsChecked = True Then
            parameters &= "-tbs=5 "
        End If

        parameters &= "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

        parameters &= "-tpop=" & tpopla.Text.ToString.Replace("%", "") & " "

        parameters &= "-tsop=" & tsopla.Text.ToString.Replace("%", "") & " "

        If Not ComboBox1.SelectedItem Is Nothing Then
            parameters &= "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not ComboBox2.SelectedItem Is Nothing Then
            parameters &= "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not NumericUpDown4.Text = Nothing Then
            parameters &= "-asp=" & NumericUpDown4.Text & " "
        End If

        If Not tbrounding.Text = Nothing Then
            parameters &= "-tbr=" & tbrounding.Text & " "
        End If

        If tbsegments.IsChecked = True Then
            parameters &= "-tbsg=1 "
        End If

        If Not NumericUpDown1.Text = Nothing Then
            parameters &= "-ptbo=" & NumericUpDown1.Text & " "
        End If
        If Not NumericUpDown2.Text = Nothing Then
            parameters &= "-stbo=" & NumericUpDown2.Text & " "
        End If

        If CheckBox1.IsChecked = True Then
            parameters &= "-cib=1 "
        End If

        If Not NumericUpDown3.Text = Nothing Then
            parameters &= "-lr=" & NumericUpDown3.Text & " "
        End If

        If Not NumericUpDown5.Text = Nothing Then
            parameters &= "-oblr=" & NumericUpDown5.Text & " "
        End If

        If Not NumericUpDown7.Text = Nothing Then
            parameters &= "-sr=" & NumericUpDown7.Text & " "
        End If

        If Not NumericUpDown7_Copy.Text = Nothing Then
            parameters &= "-sr2=" & NumericUpDown7_Copy.Text & " "
        End If

        If Not NumericUpDown7_Copy1.Text = Nothing Then
            parameters &= "-sr3=" & NumericUpDown7_Copy1.Text & " "
        End If

        If CheckBox2.IsChecked = True Then
            parameters &= "-cpo=1 "
        End If

        If CheckBox3.IsChecked = True Then
            parameters &= "-cso=1 "
        End If

        If CheckBox4.IsChecked = True Then
            parameters &= "-ftotc=1 "
        End If

        If CheckBox4_Copy.IsChecked = True Then
            parameters &= "-rzbt=1 "
        End If


        If CheckBox4_Copy.IsChecked = False Then
            parameters &= "-rzbt=0 "
        End If

        If Checkbox10.IsChecked = True Then
            parameters &= "-dtbsowm=1 "
        End If
        If Checkbox9.IsChecked = True Then
            parameters &= "-cfsa=1 "
        End If
        If CheckBox11.IsChecked = True Then
            parameters &= "-dct=1 "
        End If
        If Checkbox12.IsChecked = True Then
            parameters &= "-hps=1 "
        End If
        If Checkbox13.IsChecked = True Then
            parameters &= "-hss=1 "
        End If
        If Checkbox14.IsChecked = True Then
            parameters &= "-hpt=1 "
        End If
        If Checkbox15.IsChecked = True Then
            parameters &= "-hst=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters &= "-sti=1 "
        End If
        If checkboxconsole.IsChecked = True Then
            parameters &= "-console "
        End If

        Try
            Using ts As TaskService = New TaskService()
                ts.RootFolder.DeleteTask("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""))
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

                    td.Actions.Add(New ExecAction("explorer.exe", "taskbarx: " & Chr(34) & parameters & Chr(34), Nothing))
                Else

                    td.Actions.Add(New ExecAction(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", parameters, Nothing))

                End If

                ts.RootFolder.RegisterTaskDefinition("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""), td)

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
            Dim client As WebClient = New WebClient With {
                .CachePolicy = New System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
            }
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

                Dim td = ts.GetTask("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""))

                Dim cfg As String = Nothing



                If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
                    ''cfg = td.Definition.Actions.ToString.Replace("cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX", "")
                    cfg = td.Definition.Actions.ToString.Replace("explorer.exe taskbarx:", "")

                Else
                    cfg = td.Definition.Actions.ToString.Replace(System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe", "")
                End If

                Dim arguments() As String = cfg.Split(CType(" ", Char()))

                For Each argument In arguments
                    Dim val() As String = Split(argument, "=")
                    Console.WriteLine(val(0))
                    If argument.Contains("-tbs=") Then
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

                    If argument.Contains("-color=") Then
                        Dim colorval As String = val(1)
                        Dim colorsep = colorval.Split(CType(";", Char()))

                        sRed.Value = CDbl(colorsep(0))
                        sGreen.Value = CDbl(colorsep(1))
                        sBlue.Value = CDbl(colorsep(2))
                        sAlpha.Value = CDbl(colorsep(3))
                    End If

                    If argument.Contains("-tpop=") Then
                        tpop.Value = CDbl(val(1))
                    End If

                    If argument.Contains("-tbr=") Then
                        tbrounding.Text = val(1)
                    End If

                    If argument.Contains("-tbsg=") Then
                        If val(1) = "1" Then
                            tbsegments.IsChecked = True
                        End If
                    End If

                    If argument.Contains("-tsop=") Then
                        tsop.Value = CDbl(val(1))
                    End If

                    If argument.Contains("-ptbo=") Then
                        NumericUpDown1.Text = val(1)
                    End If
                    If argument.Contains("-stbo=") Then
                        NumericUpDown2.Text = val(1)
                    End If
                    If argument.Contains("-cpo=") Then
                        If val(1) = "1" Then
                            CheckBox2.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cso=") Then
                        If val(1) = "1" Then
                            CheckBox3.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-as=") Then
                        ComboBox1.SelectedItem = val(1)

                    End If
                    If argument.Contains("-asp=") Then
                        NumericUpDown4.Text = val(1)
                    End If
                    If argument.Contains("-sr=") Then
                        NumericUpDown7.Text = val(1)
                    End If
                    If argument.Contains("-sr2=") Then
                        NumericUpDown7_Copy.Text = val(1)
                    End If
                    If argument.Contains("-sr3=") Then
                        NumericUpDown7_Copy1.Text = val(1)
                    End If
                    If argument.Contains("-lr=") Then
                        NumericUpDown3.Text = val(1)
                    End If
                    If argument.Contains("-cib=") Then
                        If val(1) = "1" Then
                            CheckBox1.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-obas=") Then
                        ComboBox2.SelectedItem = val(1)
                    End If
                    If argument.Contains("-oblr=") Then
                        NumericUpDown5.Text = val(1)
                    End If
                    If argument.Contains("-ftotc=") Then
                        If val(1) = "1" Then
                            CheckBox4.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-rzbt=") Then
                        If val(1) = "1" Then
                            CheckBox4_Copy.IsChecked = True
                        End If
                        If val(1) = "0" Then
                            CheckBox4_Copy.IsChecked = False
                        End If
                    End If
                    If argument.Contains("-dtbsowm=") Then
                        If val(1) = "1" Then
                            Checkbox10.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-cfsa=") Then
                        If val(1) = "1" Then
                            Checkbox9.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-dct=") Then
                        If val(1) = "1" Then
                            CheckBox11.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hps=") Then
                        If val(1) = "1" Then
                            Checkbox12.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hss=") Then
                        If val(1) = "1" Then
                            Checkbox13.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hpt=") Then
                        If val(1) = "1" Then
                            Checkbox14.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-hst=") Then
                        If val(1) = "1" Then
                            Checkbox15.IsChecked = True
                        End If
                    End If
                    If argument.Contains("-sti=") Then
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
        CheckBox4_Copy.IsChecked = True
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
        NumericUpDown7_Copy.Text = "0"
        NumericUpDown7_Copy1.Text = "0"
        sAlpha.Value = 50
        tpop.Value = 100
        tsop.Value = 100
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

        Dim parameters As String = ""

        If RadioButton1.IsChecked = True Then
            parameters &= "-tbs=0 "
        End If
        If RadioButton2.IsChecked = True Then
            parameters &= "-tbs=1 "
        End If
        If RadioButton3.IsChecked = True Then
            parameters &= "-tbs=2 "
        End If
        If RadioButton4.IsChecked = True Then
            parameters &= "-tbs=3 "
        End If
        If RadioButtontc.IsChecked = True Then
            parameters &= "-tbs=4 "
        End If
        If RadioButtonoq.IsChecked = True Then
            parameters &= "-tbs=5 "
        End If

        parameters &= "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

        parameters &= "-tpop=" & tpopla.Text.ToString.Replace("%", "") & " "

        parameters &= "-tsop=" & tsopla.Text.ToString.Replace("%", "") & " "

        If Not ComboBox1.SelectedItem Is Nothing Then
            parameters &= "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not ComboBox2.SelectedItem Is Nothing Then
            parameters &= "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not tbrounding.Text = Nothing Then
            parameters &= "-tbr=" & tbrounding.Text & " "
        End If

        If tbsegments.IsChecked = True Then
            parameters &= "-tbsg=1 "
        End If

        If Not NumericUpDown4.Text = Nothing Then
            parameters &= "-asp=" & NumericUpDown4.Text & " "
        End If

        If Not NumericUpDown1.Text = Nothing Then
            parameters &= "-ptbo=" & NumericUpDown1.Text & " "
        End If
        If Not NumericUpDown2.Text = Nothing Then
            parameters &= "-stbo=" & NumericUpDown2.Text & " "
        End If

        If CheckBox1.IsChecked = True Then
            parameters &= "-cib=1 "
        End If

        If Not NumericUpDown3.Text = Nothing Then
            parameters &= "-lr=" & NumericUpDown3.Text & " "
        End If

        If Not NumericUpDown5.Text = Nothing Then
            parameters &= "-oblr=" & NumericUpDown5.Text & " "
        End If

        If Not NumericUpDown7.Text = Nothing Then
            parameters &= "-sr=" & NumericUpDown7.Text & " "
        End If

        If Not NumericUpDown7_Copy.Text = Nothing Then
            parameters &= "-sr2=" & NumericUpDown7_Copy.Text & " "
        End If

        If Not NumericUpDown7_Copy1.Text = Nothing Then
            parameters &= "-sr3=" & NumericUpDown7_Copy1.Text & " "
        End If

        If CheckBox2.IsChecked = True Then
            parameters &= "-cpo=1 "
        End If

        If CheckBox3.IsChecked = True Then
            parameters &= "-cso=1 "
        End If

        If CheckBox4.IsChecked = True Then
            parameters &= "-ftotc=1 "
        End If
        If CheckBox4_Copy.IsChecked = True Then
            parameters &= "-rzbt=1 "
        End If
        If CheckBox4_Copy.IsChecked = False Then
            parameters &= "-rzbt=0 "
        End If

        If Checkbox10.IsChecked = True Then
            parameters &= "-dtbsowm=1 "
        End If
        If Checkbox9.IsChecked = True Then
            parameters &= "-cfsa=1 "
        End If
        If CheckBox11.IsChecked = True Then
            parameters &= "-dct=1 "
        End If
        If Checkbox12.IsChecked = True Then
            parameters &= "-hps=1 "
        End If
        If Checkbox13.IsChecked = True Then
            parameters &= "-hss=1 "
        End If
        If Checkbox14.IsChecked = True Then
            parameters &= "-hpt=1 "
        End If
        If Checkbox15.IsChecked = True Then
            parameters &= "-hst=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters &= "-sti=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters &= "-console "
        End If

        If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
            Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                .WindowStyle = ProcessWindowStyle.Hidden,
                .FileName = "explorer.exe",
                .Arguments = " taskbarx:" & Chr(34) & parameters & Chr(34)
            }
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
                    ts.RootFolder.DeleteTask("TaskbarX" & " " & System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\", ""))
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

            If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then

                Dim ggFileDialog As ContentDialog = New ContentDialog With {
        .Title = "Good bye.",
        .Content = "Ready for uninstall." & vbNewLine & "TaskbarX will be removed within a minute once you click Ok...",
        .PrimaryButtonText = "Ok"
    }
                Dim results As ContentDialogResult = Await ggFileDialog.ShowAsync()

                Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                    .WindowStyle = ProcessWindowStyle.Hidden,
                    .FileName = "powershell.exe",
                    .Arguments = " Get-AppxPackage *40210ChrisAndriessen.FalconX* | Remove-AppxPackage"
                }
                Process.Start(processInfo)
                End
            Else

                Dim ffFileDialog As ContentDialog = New ContentDialog With {
      .Title = "Ready for removal.",
      .Content = "The Taskschedule is successfully removed." & vbNewLine & "You can now remove TaskbarX's files.",
      .PrimaryButtonText = "Ok"
  }
                Dim results2 As ContentDialogResult = Await ffFileDialog.ShowAsync()

                Process.Start(System.AppDomain.CurrentDomain.BaseDirectory)

                End
            End If
        Else
        End If

    End Sub

    Private Sub Button_Click_9(sender As Object, e As RoutedEventArgs)
        Callanim()

    End Sub

    Private Sub Callanim()
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
        If Not ComboBox1.SelectedIndex = ComboBox1.Items.Count Then
            ComboBox1.SelectedIndex = ComboBox1.SelectedIndex + 1
        End If
    End Sub

    Private Sub Button_Click_11(sender As Object, e As RoutedEventArgs)
        If Not ComboBox1.SelectedIndex = 0 Then
            ComboBox1.SelectedIndex = ComboBox1.SelectedIndex - 1
        End If
    End Sub

    Private Sub Alpha_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sAlpha.Value)
        tAlpha.Text = CType(val, String) & "%"

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100



        calchexcolor2()
    End Sub

    Private Sub Blue_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sBlue.Value)
        tBlue.Text = CType(val, String)

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100

        calchexcolor2()
    End Sub

    Private Sub Green_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sGreen.Value)
        tGreen.Text = CType(val, String)

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100

        calchexcolor2()
    End Sub

    Private Sub Red_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim val As Integer = CInt(sRed.Value)
        tRed.Text = CType(val, String)

        colorprev.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(CByte(sRed.Value), CByte(sGreen.Value), CByte(sBlue.Value)))

        colorprev.Opacity = sAlpha.Value / 100

        calchexcolor2()
    End Sub


    Sub calchexcolor2()
        Try
            Dim myColor As Color = Color.FromArgb(CInt(sRed.Value), CInt(sGreen.Value), CInt(sBlue.Value))
            Dim hex As String = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2")

            hexcolorbox.Text = "#" & hex
        Catch
        End Try


    End Sub
    Sub calchexcolor()
        Try
            Dim color As Color = ColorTranslator.FromHtml(hexcolorbox.Text)
            Dim r As Integer = Convert.ToInt16(color.R)
            Dim g As Integer = Convert.ToInt16(color.G)
            Dim b As Integer = Convert.ToInt16(color.B)

            sRed.Value = r
            sGreen.Value = g
            sBlue.Value = b
        Catch
        End Try


    End Sub

    Private Sub Button_Click_12(sender As Object, e As RoutedEventArgs)

        Dim t1 As Thread = New Thread(AddressOf Colorthread)
        t1.Start()

    End Sub

    Sub Colorthread()
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

    Private Sub Button_Click_13(sender As Object, e As RoutedEventArgs)

        Dim parameters As String = ""

        If RadioButton1.IsChecked = True Then
            parameters &= "-tbs=0 "
        End If
        If RadioButton2.IsChecked = True Then
            parameters &= "-tbs=1 "
        End If
        If RadioButton3.IsChecked = True Then
            parameters &= "-tbs=2 "
        End If
        If RadioButton4.IsChecked = True Then
            parameters &= "-tbs=3 "
        End If
        If RadioButtontc.IsChecked = True Then
            parameters &= "-tbs=4 "
        End If
        If RadioButtonoq.IsChecked = True Then
            parameters &= "-tbs=5 "
        End If

        parameters &= "-color=" & tRed.Text.ToString & ";" & tGreen.Text.ToString & ";" & tBlue.Text.ToString & ";" & tAlpha.Text.ToString.Replace("%", "") & " "

        parameters &= "-tpop=" & tpopla.Text.ToString.Replace("%", "") & " "

        parameters &= "-tsop=" & tsopla.Text.ToString.Replace("%", "") & " "

        If Not ComboBox1.SelectedItem Is Nothing Then
            parameters &= "-as=" & ComboBox1.SelectedItem.ToString.ToLower & " "
        End If

        If Not tbrounding.Text = Nothing Then
            parameters &= "-tbr=" & tbrounding.Text & " "
        End If

        If tbsegments.IsChecked = True Then
            parameters &= "-tbsg=1 "
        End If

        If Not ComboBox2.SelectedItem Is Nothing Then
            parameters &= "-obas=" & ComboBox2.SelectedItem.ToString.ToLower & " "
        End If

        If Not NumericUpDown4.Text = Nothing Then
            parameters &= "-asp=" & NumericUpDown4.Text & " "
        End If

        If Not NumericUpDown1.Text = Nothing Then
            parameters &= "-ptbo=" & NumericUpDown1.Text & " "
        End If
        If Not NumericUpDown2.Text = Nothing Then
            parameters &= "-stbo=" & NumericUpDown2.Text & " "
        End If

        If CheckBox1.IsChecked = True Then
            parameters &= "-cib=1 "
        End If

        If Not NumericUpDown3.Text = Nothing Then
            parameters &= "-lr=" & NumericUpDown3.Text & " "
        End If

        If Not NumericUpDown5.Text = Nothing Then
            parameters &= "-oblr=" & NumericUpDown5.Text & " "
        End If

        If Not NumericUpDown7.Text = Nothing Then
            parameters &= "-sr=" & NumericUpDown7.Text & " "
        End If

        If Not NumericUpDown7_Copy.Text = Nothing Then
            parameters &= "-sr2=" & NumericUpDown7_Copy.Text & " "
        End If

        If Not NumericUpDown7_Copy1.Text = Nothing Then
            parameters &= "-sr3=" & NumericUpDown7_Copy1.Text & " "
        End If


        If CheckBox2.IsChecked = True Then
            parameters &= "-cpo=1 "
        End If

        If CheckBox3.IsChecked = True Then
            parameters &= "-cso=1 "
        End If

        If CheckBox4.IsChecked = True Then
            parameters &= "-ftotc=1 "
        End If

        If CheckBox4_Copy.IsChecked = True Then
            parameters &= "-rzbt=1 "
        End If

        If CheckBox4_Copy.IsChecked = False Then
            parameters &= "-rzbt=0 "
        End If

        If Checkbox10.IsChecked = True Then
            parameters &= "-dtbsowm=1 "
        End If
        If Checkbox9.IsChecked = True Then
            parameters &= "-cfsa=1 "
        End If
        If CheckBox11.IsChecked = True Then
            parameters &= "-dct=1 "
        End If
        If Checkbox12.IsChecked = True Then
            parameters &= "-hps=1 "
        End If
        If Checkbox13.IsChecked = True Then
            parameters &= "-hss=1 "
        End If
        If Checkbox14.IsChecked = True Then
            parameters &= "-hpt=1 "
        End If
        If Checkbox15.IsChecked = True Then
            parameters &= "-hst=1 "
        End If
        If Checkbox16.IsChecked = True Then
            parameters &= "-sti=1 "
        End If
        If checkboxconsole.IsChecked = True Then
            parameters &= "-console "
        End If

        If System.AppDomain.CurrentDomain.BaseDirectory.Contains("40210ChrisAndriessen") Then
            '' TextboxLink.Text = "cmd.exe /c start shell:AppsFolder\40210ChrisAndriessen.FalconX_y1dazs5f5wq00!TaskbarX " & parameters
            TextboxLink.Text = "explorer.exe taskbarx:" & Chr(34) & parameters & Chr(34)
        Else
            TextboxLink.Text = Chr(34) & System.AppDomain.CurrentDomain.BaseDirectory & "TaskbarX.exe" & Chr(34) & " " & parameters
        End If

    End Sub

    Private Sub hexcolorbox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles hexcolorbox.TextChanged
        hexcolorbox.MaxLength = 7
        calchexcolor()
    End Sub

    Private Sub tpop_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles tpop.ValueChanged
        Dim val As Integer = CInt(tpop.Value)
        tpopla.Text = CType(val, String) & "%"
    End Sub

    Private Sub tsop_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles tsop.ValueChanged
        Dim val As Integer = CInt(tsop.Value)
        tsopla.Text = CType(val, String) & "%"
    End Sub

    Private Sub Button_Click_14(sender As Object, e As RoutedEventArgs)
        Process.Start("https://docs.microsoft.com/en-us/windows/win32/winauto/microsoft-active-accessibility-and-ui-automation-compared")
    End Sub
End Class