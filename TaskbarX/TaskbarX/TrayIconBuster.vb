
Imports System.Runtime.InteropServices
Imports System.Text


Namespace TrayIconBuster
	Friend Class TrayIconBuster
		Private Const TB_BUTTONCOUNT As UInteger = &H418
		Private Const TB_GETBUTTON As UInteger = &H417
		Private Const TB_DELETEBUTTON As UInteger = &H416

		Private Shared key As New Object()

		Public Shared is64bit As Boolean



		<StructLayout(LayoutKind.Sequential)>
		Public Structure SYSTEM_INFO
			Public processorArchitecture As UShort
			Private reserved As UShort
			Public pageSize As UInteger
			Public minimumApplicationAddress As IntPtr
			Public maximumApplicationAddress As IntPtr
			Public activeProcessorMask As UIntPtr
			Public numberOfProcessors As UInteger
			Public processorType As UInteger
			Public allocationGranularity As UInteger
			Public processorLevel As UShort
			Public processorRevision As UShort
		End Structure

		<DllImport("kernel32.dll")>
		Private Shared Sub GetSystemInfo(ByRef lpSystemInfo As SYSTEM_INFO)
		End Sub

		Public Shared Function RemovePhantomIcons() As UInteger
			Dim is64bitWin As Boolean = Environment.Is64BitOperatingSystem
			Dim tbb64 As New ToolBarButton64()
			Dim tbb32 As New ToolBarButton32()
			Dim td As New TrayData()
			Dim foundSomeExe As Boolean = False
			Dim totalRemovedCount As Integer = 0
			Dim totalItemCount As Integer = 0

			SyncLock key
				For pass As Integer = 1 To 2
					For kind As Integer = 0 To 1
						Dim hWnd As IntPtr = IntPtr.Zero
						If kind = 0 Then
							FindNestedWindow(hWnd, "Shell_TrayWnd")
							FindNestedWindow(hWnd, "TrayNotifyWnd")
							FindNestedWindow(hWnd, "SysPager")
							FindNestedWindow(hWnd, "ToolbarWindow32")
						Else
							' get the hidden icon collection that exists since Windows 7
							Try
								FindNestedWindow(hWnd, "NotifyIconOverflowWindow")
								FindNestedWindow(hWnd, "ToolbarWindow32")
							Catch
								' fail silently, as NotifyIconOverflowWindow did not exist prior to Win7
								Exit For
							End Try
						End If
						' create an object so we can exchange data with other process
						Using process As New LP_Process(hWnd)
							Dim remoteButtonPtr As IntPtr
							If is64bitWin Then
								remoteButtonPtr = process.Allocate(tbb64)
							Else
								remoteButtonPtr = process.Allocate(tbb32)
							End If
							process.Allocate(td)
							Dim itemCount As UInteger = CUInt(Math.Truncate(CDec(SendMessage(hWnd, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero))))
							'log("There are "+itemCount+" tray icons (some of them hidden)");
							Dim removedCount As Integer = 0
							For item As UInteger = 0 To CUInt(itemCount - 1)

								totalItemCount += 1
								' index changes when previous items got removed !
								Dim item2 As UInteger = CUInt(item - removedCount)
								Dim SOK As UInteger = CUInt(Math.Truncate(CDec(SendMessage(hWnd, TB_GETBUTTON, New IntPtr(item2), remoteButtonPtr))))
								If SOK <> 1 Then
									Throw New ApplicationException("TB_GETBUTTON failed")
								End If
								If is64bitWin Then
									process.Read(tbb64, remoteButtonPtr)
									process.Read(td, tbb64.dwData)
								Else
									process.Read(tbb32, remoteButtonPtr)
									process.Read(td, tbb32.dwData)
								End If
								Dim hWnd2 As IntPtr = td.hWnd

								Using proc As New LP_Process(hWnd2)
									Dim filename As String = proc.GetImageFileName()

									' a phantom icon has no imagefilename
									If filename Is Nothing Then
										SOK = CUInt(Math.Truncate(CDec(SendMessage(hWnd, TB_DELETEBUTTON, New IntPtr(item2), IntPtr.Zero))))

										removedCount += 1
										totalRemovedCount += 1
									End If
								End Using
							Next item
						End Using
					Next kind ' next kind


				Next pass
			End SyncLock ' release lock

			Return CUInt(totalRemovedCount)
		End Function

		' Find a topmost or nested window with specified name
		Private Shared Sub FindNestedWindow(ByRef hWnd As IntPtr, ByVal name As String)
			If hWnd = IntPtr.Zero Then
				hWnd = FindWindow(name, Nothing)
			Else
				hWnd = FindWindowEx(hWnd, IntPtr.Zero, name, Nothing)
			End If
		End Sub

		<DllImport("user32.dll", EntryPoint:="SendMessageA", CallingConvention:=CallingConvention.StdCall)>
		Public Shared Function SendMessage(ByVal Hdc As IntPtr, ByVal Msg_Const As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
		End Function

		<DllImport("user32.dll", EntryPoint:="FindWindowA", CallingConvention:=CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
		Public Shared Function FindWindow(ByVal lpszClass As String, ByVal lpszWindow As String) As IntPtr
		End Function

		<DllImport("user32.dll", EntryPoint:="FindWindowExA", CallingConvention:=CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
		Public Shared Function FindWindowEx(ByVal hwndParent As IntPtr, ByVal hwndChildAfter As IntPtr, ByVal lpszClass As String, ByVal lpszWindow As String) As IntPtr
		End Function


		<StructLayout(LayoutKind.Sequential)>
		Public Class ToolBarButton32
			Public iBitmap As UInteger
			Public idCommand As UInteger
			Public fsState As Byte
			Public fsStyle As Byte
			Private bReserved0 As Byte
			Private bReserved1 As Byte
			Public dwData As IntPtr
			Public iString As UInteger
		End Class
		<StructLayout(LayoutKind.Sequential)>
		Public Class ToolBarButton64
			Public iBitmap As UInteger
			Public idCommand As UInteger
			Public fsState As Byte
			Public fsStyle As Byte
			Private bReserved0 As Byte
			Private bReserved1 As Byte
			Private bReserved2 As Byte
			Private bReserved3 As Byte
			Private bReserved4 As Byte
			Private bReserved5 As Byte
			Public dwData As IntPtr
			Public iString As UInteger
		End Class


		<StructLayout(LayoutKind.Sequential)>
		Public Class TrayData
			Public hWnd As IntPtr
			Public uID As UInteger
			Public uCallbackMessage As UInteger
			Private reserved0 As UInteger
			Private reserved1 As UInteger
			Public hIcon As IntPtr
		End Class
	End Class


	Public Class LP_Process
		Implements IDisposable


		Private Const PROCESS_VM_OPERATION As UInteger = &H8
		Private Const PROCESS_VM_READ As UInteger = &H10
		Private Const PROCESS_VM_WRITE As UInteger = &H20
		Private Const PROCESS_QUERY_INFORMATION As UInteger = &H400

		Private Const MEM_COMMIT As UInteger = &H1000
		Private Const MEM_RELEASE As UInteger = &H8000
		Private Const PAGE_READWRITE As UInteger = &H4


		Private hProcess As IntPtr


		Private ownerProcessID As UInteger


		Private allocations As New ArrayList()

		Public Sub New(ByVal hWnd As IntPtr)
			GetWindowThreadProcessId(hWnd, ownerProcessID)

			hProcess = OpenProcess(PROCESS_VM_OPERATION Or PROCESS_VM_READ Or PROCESS_VM_WRITE Or PROCESS_QUERY_INFORMATION, False, ownerProcessID)

		End Sub


		Public Sub Dispose() Implements IDisposable.Dispose
			If hProcess <> IntPtr.Zero Then
				For Each ptr As IntPtr In allocations
					VirtualFreeEx(hProcess, ptr, 0, MEM_RELEASE)
				Next ptr
				CloseHandle(hProcess)
			End If
		End Sub


		Public Function GetImageFileName() As String
			Dim sb As New StringBuilder(1024)
			Dim OK As Boolean = GetProcessImageFileName(hProcess, sb, sb.Capacity - 1)
			If Not OK Then
				Return Nothing
			End If
			Return sb.ToString()
		End Function


		Public Function Allocate(ByVal managedObject As Object) As IntPtr
			Dim size As Integer = Marshal.SizeOf(managedObject)
			Dim ptr As IntPtr = VirtualAllocEx(hProcess, 0, size, MEM_COMMIT, PAGE_READWRITE)

			If ptr <> IntPtr.Zero Then
				allocations.Add(ptr)
			End If
			Return ptr
		End Function


		Public Sub Read(ByVal obj As Object, ByVal ptr As IntPtr)
			Using pin As New LP_Pinner(obj)
				Dim bytesRead As UInteger = 0
				Dim size As Integer = Marshal.SizeOf(obj)
				If Not ReadProcessMemory(hProcess, ptr, pin.Ptr, size, bytesRead) Then
					Dim err As Integer = GetLastError()
					Dim s As String = "Read failed; err=" & err & "; bytesRead=" & bytesRead
					Throw New ApplicationException(s)
				End If
			End Using
		End Sub


		Public Function ReadString(ByVal size As Integer, ByVal ptr As IntPtr) As String
			Dim sb As New StringBuilder(size)
			Dim bytesRead As UInteger = 0
			If Not ReadProcessMemory(hProcess, ptr, sb, size, bytesRead) Then
				Dim err As Integer = GetLastError()
				Dim s As String = "Read failed; err=" & err & "; bytesRead=" & bytesRead
				Throw New ApplicationException(s)
			End If
			Return sb.ToString()
		End Function


		Public Sub Write(ByVal obj As Object, ByVal size As Integer, ByVal ptr As IntPtr)
			Using pin As New LP_Pinner(obj)
				Dim bytesWritten As UInteger = 0
				If Not WriteProcessMemory(hProcess, ptr, pin.Ptr, size, bytesWritten) Then
					Dim err As Integer = GetLastError()
					Dim s As String = "Write failed; err=" & err & "; bytesWritten=" & bytesWritten
					Throw New ApplicationException(s)
				End If
			End Using
		End Sub


		<DllImport("user32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function GetWindowThreadProcessId(ByVal hWnd As IntPtr, ByRef procId As UInteger) As UInteger
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function OpenProcess(ByVal access As UInteger, ByVal inheritHandle As Boolean, ByVal procID As UInteger) As IntPtr
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function CloseHandle(ByVal handle As IntPtr) As Boolean
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function VirtualAllocEx(ByVal hProcess As IntPtr, ByVal address As Integer, ByVal size As Integer, ByVal allocationType As UInteger, ByVal protection As UInteger) As IntPtr
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function VirtualFreeEx(ByVal hProcess As IntPtr, ByVal address As IntPtr, ByVal size As Integer, ByVal freeType As UInteger) As Boolean
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function WriteProcessMemory(ByVal hProcess As IntPtr, ByVal otherAddress As IntPtr, ByVal localAddress As IntPtr, ByVal size As Integer, ByRef bytesWritten As UInteger) As Boolean
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal otherAddress As IntPtr, ByVal localAddress As IntPtr, ByVal size As Integer, ByRef bytesRead As UInteger) As Boolean
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal otherAddress As IntPtr, ByVal localAddress As StringBuilder, ByVal size As Integer, ByRef bytesRead As UInteger) As Boolean
		End Function

		<DllImport("psapi.dll", CallingConvention:=CallingConvention.StdCall)>
		Private Shared Function GetProcessImageFileName(ByVal hProcess As IntPtr, ByVal fileName As StringBuilder, ByVal fileNameSize As Integer) As Boolean
		End Function

		<DllImport("kernel32.dll", CallingConvention:=CallingConvention.StdCall)>
		Public Shared Function GetLastError() As Integer
		End Function
	End Class

	Public Class LP_Pinner
		Implements IDisposable

		Private handle As GCHandle
		Private disposed As Boolean

		Private ptr_Conflict As IntPtr


		Public Sub New(ByVal obj As Object)

			handle = GCHandle.Alloc(obj, GCHandleType.Pinned)
			ptr_Conflict = handle.AddrOfPinnedObject()
		End Sub


		Protected Overrides Sub Finalize()
			Dispose()
		End Sub


		Public Sub Dispose() Implements IDisposable.Dispose
			If Not disposed Then

				disposed = True
				handle.Free()
				ptr_Conflict = IntPtr.Zero
			End If
		End Sub


		Public ReadOnly Property Ptr() As IntPtr
			Get
				Return ptr_Conflict
			End Get
		End Property
	End Class
End Namespace
