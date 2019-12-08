Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows
Imports System.Windows.Automation
Imports FalconX4.VisualEffects
Imports FalconX4.VisualEffects.Animations.Effects
Imports FalconX4.VisualEffects.Easing

Public Class Taskbar

    <DllImport("user32.dll", ExactSpelling:=True, CharSet:=CharSet.Auto)>
    Public Shared Function GetParent(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="FindWindow", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindowByClass(ByVal lpClassName As String, ByVal zero As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Boolean, ByVal lParam As Int32) As Integer
    End Function

    <DllImport("Shell32.dll")>
    Public Shared Function SHChangeNotify(ByVal eventId As Integer, ByVal flags As Integer, ByVal item1 As IntPtr, ByVal item2 As IntPtr) As Integer

    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetWindowCompositionAttribute(ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function SetProcessWorkingSetSize(ByVal hProcess As IntPtr, ByVal dwMinimumWorkingSetSize As Int32, ByVal dwMaximumWorkingSetSize As Int32) As Int32
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function SetParent(ByVal hWndChild As IntPtr, ByVal hWndNewParent As IntPtr) As IntPtr
    End Function

    Public Shared SWP_NOSIZE As UInt32 = 1
    Public Shared SWP_ASYNCWINDOWPOS As UInt32 = 16384
    Public Shared SWP_NOACTIVATE As UInt32 = 16
    Public Shared SWP_NOSENDCHANGING As UInt32 = 1024
    Public Shared SWP_NOZORDER As UInt32 = 4

    Public Shared WM_DWMCOLORIZATIONCOLORCHANGED As Integer = &H320
    Public Shared WM_DWMCOMPOSITIONCHANGED As Integer = &H31E
    Public Shared WM_THEMECHANGED As Integer = &H31A
    Public Shared WM_STYLECHANGED As Integer = &H7D

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

    ' Public Shared [Desktop] As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("[Desktop]", CType(0, IntPtr)))
    Public Shared WorkerW As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("WorkerW", CType(0, IntPtr)))

    Public Shared SHELLDLL_DefView As AutomationElement = WorkerW.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "SHELLDLL_DefView"))

    ' Public Shared SysListView32 As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("SysListView32", CType(0, IntPtr)))

    Public Shared Shell_TrayWnd As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr)))
    Public Shared MSTaskListWClass As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
    Public Shared TrayNotifyWnd As AutomationElement = Shell_TrayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "TrayNotifyWnd"))
    Public Shared MSTaskSwWClass As IntPtr = GetParent(CType(MSTaskListWClass.Current.NativeWindowHandle, IntPtr))
    Public Shared ReBarWindow32 As IntPtr = GetParent(MSTaskSwWClass)

    Public Shared Root As AutomationElement = AutomationElement.RootElement
    Public Shared Condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
    Public Shared AllTrayWnds As AutomationElementCollection = Root.FindAll(TreeScope.Children, Condition)

    Public Shared Shell_TrayWndPtr As IntPtr = CType(Shell_TrayWnd.Current.NativeWindowHandle, IntPtr)
    Public Shared MSTaskListWClassPtr As IntPtr = CType(MSTaskListWClass.Current.NativeWindowHandle, IntPtr)
    Public Shared TrayNotifyWndPtr As IntPtr = CType(TrayNotifyWnd.Current.NativeWindowHandle, IntPtr)
    Public Shared MSTaskSwWClassPtr As IntPtr = MSTaskSwWClass
    Public Shared ReBarWindow32Ptr As IntPtr = ReBarWindow32

    Public Const WM_SETREDRAW As Integer = 11

    Public Shared Horizontal As Boolean
    Public Shared TaskbarTransparant As Boolean
    Public Shared CenterBetween As Boolean
    Public Shared UpdateTaskbar As Boolean
    Public Shared TaskbarChanged As Boolean
    Public Shared OffsetPosition As String
    Public Shared OffsetPosition2 As String
    Public Shared TaskbarStyle As Integer
    Public Shared UpdateTaskbarStyle As Boolean

    Public Shared Updating As Boolean

    Public Shared OldPosition1 As Integer
    Public Shared OldPosition2 As Integer
    Public Shared OldPosition3 As Integer

    Public Shared OldLeft1 As Integer
    Public Shared OldLeft2 As Integer
    Public Shared OldLeft3 As Integer

    Public Shared Ready As Boolean

    Public Shared AppClosing As Boolean

    Public Shared Sub Main()

        SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, False, 0)

        Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf TaskbarCalculator)
        t1.Start()

    End Sub

    Public Shared Sub TaskbarCalculator()
        Dim Laps As Integer
        Do
            Try

                Updating = True

                If AppClosing = True Then
                    Exit Sub
                End If

                Dim TaskbarCount As Integer = 0

                Dim OldTrayNotifyWidth As Integer
                Dim TrayNotifyWidth As Integer = 0
                Dim Resolution As Integer
                Dim OldResolution As Integer
                Dim OldTaskbarCount As Integer

                Dim TaskbarTreeWalker As TreeWalker = TreeWalker.ControlViewWalker
                Dim child As AutomationElement = TaskbarTreeWalker.GetLastChild(MSTaskListWClass)

                If Shell_TrayWnd.Current.BoundingRectangle.Height >= 200 Then
                    If Horizontal = True Then
                        UpdateTaskbar = True
                        Console.WriteLine("Taskbar Position Changed")
                    End If
                    Horizontal = False
                Else
                    If Horizontal = False Then
                        UpdateTaskbar = True
                        Console.WriteLine("Taskbar Position Changed")
                    End If
                    Horizontal = True
                End If

                Dim screencount As Integer
                screencount = 0

                For Each screenX In Screen.AllScreens
                    screencount = screencount + screenX.Bounds.Width
                Next

                Resolution = screencount

                ' Console.WriteLine(Resolution)

                If Horizontal = True Then
                    'Resolution = Screen.PrimaryScreen.Bounds.Width
                    TaskbarCount = CInt(child.Current.BoundingRectangle.Left)
                    If CenterBetween = True Then
                        TrayNotifyWidth = CInt(TrayNotifyWnd.Current.BoundingRectangle.Left)
                    End If
                Else
                    'Resolution = Screen.PrimaryScreen.Bounds.Height
                    TaskbarCount = CInt(child.Current.BoundingRectangle.Top)
                    If CenterBetween = True Then
                        TrayNotifyWidth = CInt(TrayNotifyWnd.Current.BoundingRectangle.Top)
                    End If
                End If

                System.Threading.Thread.Sleep(400)
                '  If Not TaskbarCount = OldTaskbarCount Or UpdateTaskbar = True Or Not Resolution = OldResolution Or Not TrayNotifyWidth = OldTrayNotifyWidth Or TaskbarCount = OldTaskbarCount + 1 Or TaskbarCount = OldTaskbarCount + 2 Or TaskbarCount = OldTaskbarCount + 3 Or TaskbarCount = OldTaskbarCount + 4 Or TaskbarCount = OldTaskbarCount + 5 Or TaskbarCount = OldTaskbarCount - 1 Or TaskbarCount = OldTaskbarCount - 2 Or TaskbarCount = OldTaskbarCount - 3 Or TaskbarCount = OldTaskbarCount - 4 Or TaskbarCount = OldTaskbarCount - 5 Then

                If Not TaskbarCount = OldTaskbarCount Or UpdateTaskbar = True Or Not Resolution = OldResolution Or Not TrayNotifyWidth = OldTrayNotifyWidth Then

                    Laps = 0

                    Console.WriteLine("")
                    Console.WriteLine("The Taskbar Watcher found a difference")
                    Console.WriteLine("TaskbarCount = " & TaskbarCount & " | OldTaskbarCount = " & OldTaskbarCount)
                    Console.WriteLine("Resolution = " & Resolution & " | OldResolution = " & OldResolution)
                    Console.WriteLine("")

                    System.Threading.Thread.Sleep(400)

                    If Not Resolution = OldResolution Then
                        TaskbarChanged = True
                    End If

                    If Not TrayNotifyWidth = OldTrayNotifyWidth Then
                        TaskbarChanged = True
                    End If

                    OldResolution = Resolution
                    OldTrayNotifyWidth = TrayNotifyWidth

                    Dim TaskListID As Integer = 0

                    Console.WriteLine("UpdateTaskbar = " & UpdateTaskbar)

                    For Each trayWnd As AutomationElement In AllTrayWnds

                        TaskListID = TaskListID + 1
                        Dim TaskList As AutomationElement = trayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"))
                        Dim TrayNotify As AutomationElement = trayWnd.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ClassNameProperty, "TrayNotifyWnd"))

                        OldTaskbarCount = TaskbarCount

                        If TaskListID = 1 Then

                            Dim TreeWalker1 As TreeWalker = TreeWalker.ControlViewWalker
                            Dim BChildFirst1 As AutomationElement = TreeWalker1.GetFirstChild(TaskList)
                            Dim BChildLast1 As AutomationElement = TreeWalker1.GetLastChild(TaskList)

                            Dim TaskbarWidth1 As Integer

                            If Horizontal = True Then
                                TaskbarWidth1 = CInt((BChildFirst1.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + (BChildLast1.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + BChildLast1.Current.BoundingRectangle.Width)
                            Else
                                TaskbarWidth1 = CInt((BChildFirst1.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + (BChildLast1.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + BChildLast1.Current.BoundingRectangle.Height)
                            End If

                            Dim RebarWnd1 As AutomationElement = AutomationElement.FromHandle(GetParent(CType(TaskList.Current.NativeWindowHandle, IntPtr)))

                            Dim TrayWndLeft1 As Integer
                            Dim TrayWndWidth1 As Integer
                            Dim RebarWndLeft1 As Integer
                            Dim TaskbarLeft1 As Integer

                            If Horizontal = True Then
                                TrayWndLeft1 = CInt(trayWnd.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                                TrayWndWidth1 = CInt(trayWnd.Current.BoundingRectangle.Width.ToString.Replace("-", ""))
                                RebarWndLeft1 = CInt(RebarWnd1.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                                TaskbarLeft1 = CInt((RebarWndLeft1 - TrayWndLeft1).ToString.Replace("-", ""))
                            Else
                                TrayWndLeft1 = CInt(trayWnd.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                                TrayWndWidth1 = CInt(trayWnd.Current.BoundingRectangle.Height.ToString.Replace("-", ""))
                                RebarWndLeft1 = CInt(RebarWnd1.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                                TaskbarLeft1 = CInt((RebarWndLeft1 - TrayWndLeft1).ToString.Replace("-", ""))
                            End If

                            Dim Position1 As Integer

                            If trayWnd.Current.ClassName = "Shell_TrayWnd" Then
                                If CenterBetween = True Then
                                    Dim offset = (TrayNotify.Current.BoundingRectangle.Width / 2 - (TaskbarLeft1 \ 2))
                                    Position1 = CInt((TrayWndWidth1 / 2 - (TaskbarWidth1 / 2) - TaskbarLeft1 - offset).ToString.Replace("-", "")) + OffsetPosition
                                Else
                                    Position1 = CInt((TrayWndWidth1 / 2 - (TaskbarWidth1 / 2) - TaskbarLeft1).ToString.Replace("-", "")) + OffsetPosition
                                End If
                            Else
                                Position1 = CInt((TrayWndWidth1 / 2 - (TaskbarWidth1 / 2) - TaskbarLeft1).ToString.Replace("-", "")) + OffsetPosition2
                            End If

                            XLocationEffect.FirstTaskbarPtr = CType(TaskList.Current.NativeWindowHandle, IntPtr)
                            XLocationEffect.FirstTaskbarPosition = CInt(Position1.ToString.Replace("-", ""))
                            XLocationEffect.FirstTaskbarOldPosition = CInt(OldPosition1.ToString.Replace("-", ""))

                            'Console.WriteLine(XLocationEffect.FirstTaskbarOldPosition)

                            SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, False, 0)

                            SendMessage(GetParent(Shell_TrayWndPtr), WM_SETREDRAW, False, 0) ' [Desktop]

                            '  SendMessage(GetParent(SHELLDLL_DefView.Current.NativeWindowHandle), WM_SETREDRAW, False, 0)
                            'SetParent(Gui.Panel1.Handle, GetParent(Shell_TrayWndPtr))

                            Console.WriteLine("FirstTaskbarCalculation | OldLeft = " & OldLeft1 & " Left = " & TaskbarLeft1 + TaskbarWidth1 & " <-- If not the same we call the Animator")

                            ' If Not OldLeft1 = TaskbarLeft1 + TaskbarWidth1 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 + 1 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 + 2 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 + 3 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 + 4 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 + 5 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 - 1 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 - 2 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 - 3 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 - 4 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 - 5 Or UpdateTaskbar = True Then

                            If Not OldLeft1 = TaskbarLeft1 + TaskbarWidth1 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 + 1 Or OldLeft1 = TaskbarLeft1 + TaskbarWidth1 - 1 Or UpdateTaskbar = True Or TaskbarChanged = True Then
                                Console.WriteLine("Call Animator 1")
                                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf AnimationControl.AnimateTaskbar)
                                t1.Start()
                            End If

                            OldPosition1 = Position1
                            OldLeft1 = TaskbarLeft1 + TaskbarWidth1

                            ' Console.WriteLine(OldLeft1 & " | " & TaskbarLeft1)

                        End If

                        If TaskListID = 2 Then

                            Dim TreeWalker2 As TreeWalker = TreeWalker.ControlViewWalker
                            Dim BChildFirst2 As AutomationElement = TreeWalker2.GetFirstChild(TaskList)
                            Dim BChildLast2 As AutomationElement = TreeWalker2.GetLastChild(TaskList)

                            Dim TaskbarWidth2 As Integer

                            If Horizontal = True Then
                                TaskbarWidth2 = CInt((BChildFirst2.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + (BChildLast2.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + BChildLast2.Current.BoundingRectangle.Width)
                            Else
                                TaskbarWidth2 = CInt((BChildFirst2.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + (BChildLast2.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + BChildLast2.Current.BoundingRectangle.Height)
                            End If

                            Dim RebarWnd2 As AutomationElement = AutomationElement.FromHandle(GetParent(CType(TaskList.Current.NativeWindowHandle, IntPtr)))

                            Dim TrayWndLeft2 As Integer
                            Dim TrayWndWidth2 As Integer
                            Dim RebarWndLeft2 As Integer
                            Dim TaskbarLeft2 As Integer

                            If Horizontal = True Then
                                TrayWndLeft2 = CInt(trayWnd.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                                TrayWndWidth2 = CInt(trayWnd.Current.BoundingRectangle.Width.ToString.Replace("-", ""))
                                RebarWndLeft2 = CInt(RebarWnd2.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                                TaskbarLeft2 = CInt((RebarWndLeft2 - TrayWndLeft2).ToString.Replace("-", ""))
                            Else
                                TrayWndLeft2 = CInt(trayWnd.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                                TrayWndWidth2 = CInt(trayWnd.Current.BoundingRectangle.Height.ToString.Replace("-", ""))
                                RebarWndLeft2 = CInt(RebarWnd2.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                                TaskbarLeft2 = CInt((RebarWndLeft2 - TrayWndLeft2).ToString.Replace("-", ""))
                            End If

                            Dim Position2 As Integer

                            If trayWnd.Current.ClassName = "Shell_TrayWnd" Then
                                If CenterBetween = True Then
                                    Dim offset = (TrayNotify.Current.BoundingRectangle.Width / 2 - (TaskbarLeft2 \ 2))
                                    Position2 = CInt((TrayWndWidth2 / 2 - (TaskbarWidth2 / 2) - TaskbarLeft2 - offset).ToString.Replace("-", "")) + OffsetPosition
                                Else
                                    Position2 = CInt((TrayWndWidth2 / 2 - (TaskbarWidth2 / 2) - TaskbarLeft2).ToString.Replace("-", "")) + OffsetPosition
                                End If
                            Else
                                Position2 = CInt((TrayWndWidth2 / 2 - (TaskbarWidth2 / 2) - TaskbarLeft2).ToString.Replace("-", "")) + OffsetPosition2
                            End If

                            XLocationEffect2.SecondTaskbarPtr = CType(TaskList.Current.NativeWindowHandle, IntPtr)
                            XLocationEffect2.SecondTaskbarPosition = CInt(Position2.ToString.Replace("-", ""))
                            XLocationEffect2.SecondTaskbarOldPosition = CInt(OldPosition2.ToString.Replace("-", ""))

                            Console.WriteLine("SecondTaskbarCalculation | OldLeft = " & OldLeft1 & " Left = " & TaskbarLeft2 + TaskbarWidth2 & " <-- If not the same we call the Animator")

                            '    If Not OldLeft2 = TaskbarLeft2 + TaskbarWidth2 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 + 1 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 + 2 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 + 3 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 + 4 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 + 5 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 - 1 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 - 2 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 - 3 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 - 4 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 - 5 Or UpdateTaskbar = True Then
                            If Not OldLeft2 = TaskbarLeft2 + TaskbarWidth2 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 + 1 Or OldLeft2 = TaskbarLeft2 + TaskbarWidth2 - 1 Or UpdateTaskbar = True Or TaskbarChanged = True Then
                                Console.WriteLine("Call Animator 2")
                                Dim t2 As System.Threading.Thread = New System.Threading.Thread(AddressOf AnimationControl2.AnimateTaskbar2)
                                t2.Start()
                            End If

                            OldPosition2 = Position2
                            OldLeft2 = TaskbarLeft2 + TaskbarWidth2

                        End If

                        If TaskListID = 3 Then

                            Dim TreeWalker3 As TreeWalker = TreeWalker.ControlViewWalker
                            Dim BChildFirst3 As AutomationElement = TreeWalker3.GetFirstChild(TaskList)
                            Dim BChildLast3 As AutomationElement = TreeWalker3.GetLastChild(TaskList)

                            Dim TaskbarWidth3 As Integer

                            If Horizontal = True Then
                                TaskbarWidth3 = CInt((BChildFirst3.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + (BChildLast3.Current.BoundingRectangle.Left - TaskList.Current.BoundingRectangle.Left) + BChildLast3.Current.BoundingRectangle.Width)
                            Else
                                TaskbarWidth3 = CInt((BChildFirst3.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + (BChildLast3.Current.BoundingRectangle.Top - TaskList.Current.BoundingRectangle.Top) + BChildLast3.Current.BoundingRectangle.Height)
                            End If

                            Dim RebarWnd3 As AutomationElement = AutomationElement.FromHandle(GetParent(CType(TaskList.Current.NativeWindowHandle, IntPtr)))

                            Dim TrayWndLeft3 As Integer
                            Dim TrayWndWidth3 As Integer
                            Dim RebarWndLeft3 As Integer
                            Dim TaskbarLeft3 As Integer

                            If Horizontal = True Then
                                TrayWndLeft3 = CInt(trayWnd.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                                TrayWndWidth3 = CInt(trayWnd.Current.BoundingRectangle.Width.ToString.Replace("-", ""))
                                RebarWndLeft3 = CInt(RebarWnd3.Current.BoundingRectangle.Left.ToString.Replace("-", ""))
                                TaskbarLeft3 = CInt((RebarWndLeft3 - TrayWndLeft3).ToString.Replace("-", ""))
                            Else
                                TrayWndLeft3 = CInt(trayWnd.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                                TrayWndWidth3 = CInt(trayWnd.Current.BoundingRectangle.Height.ToString.Replace("-", ""))
                                RebarWndLeft3 = CInt(RebarWnd3.Current.BoundingRectangle.Top.ToString.Replace("-", ""))
                                TaskbarLeft3 = CInt((RebarWndLeft3 - TrayWndLeft3).ToString.Replace("-", ""))
                            End If

                            Dim Position3 As Integer

                            If trayWnd.Current.ClassName = "Shell_TrayWnd" Then
                                If CenterBetween = True Then
                                    Dim offset = (TrayNotify.Current.BoundingRectangle.Width / 2 - (TaskbarLeft3 \ 2))
                                    Position3 = CInt((TrayWndWidth3 / 2 - (TaskbarWidth3 / 2) - TaskbarLeft3 - offset).ToString.Replace("-", "")) + OffsetPosition
                                Else
                                    Position3 = CInt((TrayWndWidth3 / 2 - (TaskbarWidth3 / 2) - TaskbarLeft3).ToString.Replace("-", "")) + OffsetPosition
                                End If
                            Else
                                Position3 = CInt((TrayWndWidth3 / 2 - (TaskbarWidth3 / 2) - TaskbarLeft3).ToString.Replace("-", "")) + OffsetPosition2
                            End If

                            ' Console.WriteLine(Position3)

                            XLocationEffect3.ThirdTaskbarPtr = CType(TaskList.Current.NativeWindowHandle, IntPtr)
                            XLocationEffect3.ThirdTaskbarPosition = CInt(Position3.ToString.Replace("-", ""))
                            XLocationEffect3.ThirdTaskbarOldPosition = CInt(OldPosition3.ToString.Replace("-", ""))

                            Console.WriteLine("ThirdTaskbarCalculation | OldLeft = " & OldLeft3 & " Left = " & TaskbarLeft3 + TaskbarWidth3 & " <-- If not the same we call the Animator")

                            ' If Not OldLeft3 = TaskbarLeft3 + TaskbarWidth3 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 + 1 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 + 2 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 + 3 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 + 4 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 + 5 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 - 1 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 - 2 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 - 3 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 - 4 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 - 5 Or UpdateTaskbar = True Then
                            If Not OldLeft3 = TaskbarLeft3 + TaskbarWidth3 Or OldLeft3 = TaskbarLeft3 + TaskbarWidth3 + 1 Or OldLeft2 = TaskbarLeft3 + TaskbarWidth3 - 1 Or UpdateTaskbar = True Or TaskbarChanged = True Then

                                Console.WriteLine("Call Animator 3")
                                Dim t3 As System.Threading.Thread = New System.Threading.Thread(AddressOf AnimationControl3.AnimateTaskbar3)
                                t3.Start()
                            End If

                            OldPosition3 = Position3
                            OldLeft3 = TaskbarLeft3 + TaskbarWidth3

                        End If

                    Next

                    If UpdateTaskbar = True Then
                        UpdateTaskbar = False
                        AnimationControl.TaskbarRefresh = True
                    End If

                    TaskbarChanged = False

                End If
            Catch ex As Exception

                If ex.ToString.Contains("E_ACCESSDENIED") Then

                    Dim Handle As IntPtr
                    Dim Laps2 As Integer

                    ClearMemory()

                    Do

                        Laps2 = Laps2 + 1

                        If Laps2 = 50 Then
                            Laps2 = 0
                            ClearMemory()
                        End If

                        Handle = Nothing
                        System.Threading.Thread.Sleep(250)
                        Handle = FindWindowByClass("Shell_TrayWnd", CType(0, IntPtr))

                    Loop Until Not Handle = Nothing

                    Gui.NotifyIcon1.Visible = False
                    Application.Restart()
                    End
                End If

            End Try

            Laps = Laps + 1

            ' Console.WriteLine(Laps)

            If Laps = 150 Then
                Laps = 0
                ClearMemory()
            End If

        Loop
    End Sub

    Public Shared Sub RefreshWindowsExplorer()

        SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, True, 0)
        '  SHChangeNotify(134217728, 4096, IntPtr.Zero, IntPtr.Zero)
        Dim CLSID_ShellApplication As Guid = New Guid("13709620-C279-11CE-A49E-444553540000")
        Dim shellApplicationType As Type = Type.GetTypeFromCLSID(CLSID_ShellApplication, True)
        Dim shellApplication As Object = Activator.CreateInstance(shellApplicationType)
        Dim windows As Object = shellApplicationType.InvokeMember("Windows", System.Reflection.BindingFlags.InvokeMethod, Nothing, shellApplication, New Object(-1) {})
        Dim windowsType As Type = windows.GetType
        Dim count As Object = windowsType.InvokeMember("Count", System.Reflection.BindingFlags.GetProperty, Nothing, windows, Nothing)
        Dim i As Integer = 0
        Do While (i < CType(count, Integer))
            Dim item As Object = windowsType.InvokeMember("Item", System.Reflection.BindingFlags.InvokeMethod, Nothing, windows, New Object() {i})
            Dim itemType As Type = item.GetType
            Dim itemName As String = CType(itemType.InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, Nothing, item, Nothing), String)
            If (itemName = "Shell_TrayWnd") Then
                itemType.InvokeMember("Refresh", System.Reflection.BindingFlags.InvokeMethod, Nothing, item, Nothing)
            End If
            i = (i + 1)
        Loop

    End Sub

    Public Shared Function ClearMemory() As Int32

        Return SetProcessWorkingSetSize(Diagnostics.Process.GetCurrentProcess.Handle, 2097152, 2097152)

    End Function

    Public Shared Sub EnableTaskbarStyle()

        Dim Progman As AutomationElement = AutomationElement.FromHandle(FindWindowByClass("Progman", CType(0, IntPtr)))

        Dim desktops As AutomationElement = AutomationElement.RootElement
        Dim condition As New OrCondition(New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"), New PropertyCondition(AutomationElement.ClassNameProperty, "Shell_SecondaryTrayWnd"))
        Dim lists As AutomationElementCollection = desktops.FindAll(TreeScope.Children, condition)

        Dim accent = New AccentPolicy()
        Dim accentStructSize = Marshal.SizeOf(accent)

        If TaskbarStyle = 1 Then
            accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARANT
            ' accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT
            ' accent.GradientColor = 255
        End If

        If TaskbarStyle = 3 Then
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND
            accent.GradientColor = &H0 Or &H0
        End If

        If TaskbarStyle = 2 Then
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
        End If

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

        Loop Until TaskbarTransparant = False Or UpdateTaskbarStyle = True

        RefreshWindowsExplorer()

        For Each tray In trays
            Dim trayptr As IntPtr = CType(tray.ToString, IntPtr)
            SendMessage(trayptr, WM_THEMECHANGED, True, 0)
            SendMessage(trayptr, WM_DWMCOLORIZATIONCOLORCHANGED, True, 0)
            SendMessage(trayptr, WM_DWMCOMPOSITIONCHANGED, True, 0)
        Next

        Marshal.FreeHGlobal(accentPtr)

        ' Console.WriteLine(TaskbarStyle.ToString)

        If UpdateTaskbarStyle = True Then
            If TaskbarTransparant = True Then
                UpdateTaskbarStyle = False
                Dim t1 As System.Threading.Thread = New System.Threading.Thread(AddressOf Taskbar.EnableTaskbarStyle)
                t1.Start()
            End If
        End If

    End Sub

    Public Shared Sub Closing()

        AppClosing = True

        SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, True, 0)

        TaskbarTransparant = False

        System.Threading.Thread.Sleep(50) : Application.DoEvents()

        SendMessage(XLocationEffect.FirstTaskbarPtr, WM_THEMECHANGED, True, 0)
        SendMessage(XLocationEffect.FirstTaskbarPtr, WM_DWMCOLORIZATIONCOLORCHANGED, True, 0)
        SendMessage(XLocationEffect.FirstTaskbarPtr, WM_DWMCOMPOSITIONCHANGED, True, 0)

        SendMessage(XLocationEffect2.SecondTaskbarPtr, WM_THEMECHANGED, True, 0)
        SendMessage(XLocationEffect2.SecondTaskbarPtr, WM_DWMCOLORIZATIONCOLORCHANGED, True, 0)
        SendMessage(XLocationEffect2.SecondTaskbarPtr, WM_DWMCOMPOSITIONCHANGED, True, 0)

        SendMessage(XLocationEffect3.ThirdTaskbarPtr, WM_THEMECHANGED, True, 0)
        SendMessage(XLocationEffect3.ThirdTaskbarPtr, WM_DWMCOLORIZATIONCOLORCHANGED, True, 0)
        SendMessage(XLocationEffect3.ThirdTaskbarPtr, WM_DWMCOMPOSITIONCHANGED, True, 0)

        System.Threading.Thread.Sleep(500) : Application.DoEvents()

        SendMessage(GetParent(Shell_TrayWndPtr), WM_SETREDRAW, True, 0) ' [Desktop]
        SendMessage(ReBarWindow32Ptr, WM_SETREDRAW, True, 0)

        SetWindowPos(XLocationEffect.FirstTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
        SetWindowPos(XLocationEffect2.SecondTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)
        SetWindowPos(XLocationEffect3.ThirdTaskbarPtr, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_ASYNCWINDOWPOS Or SWP_NOACTIVATE Or SWP_NOZORDER Or SWP_NOSENDCHANGING)

    End Sub

End Class