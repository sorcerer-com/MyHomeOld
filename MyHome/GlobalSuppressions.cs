// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "MyHome.TcpConnection.Server.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "MyHome.TcpConnection.Client")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Scope = "member", Target = "MyHome.TcpConnection.Client.#CommandReceived")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "MyHome.TcpConnection.Client.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "MyHome.Controls.Emulator")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "MyHome.Controls.Emulator.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "client", Scope = "member", Target = "MyHome.Controls.Emulator.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "MyHome.MainWindow")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#VkKeyScan(System.Char)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3", Scope = "member", Target = "MyHome.Services.PCControl.#keybd_event(System.Byte,System.Byte,System.UInt32,System.UInt64)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#keybd_event(System.Byte,System.Byte,System.UInt32,System.UInt64)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#GetCursorPos(MyHome.Services.PCControl+Win32Point&)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#SetCursorPos(System.Int32,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "4", Scope = "member", Target = "MyHome.Services.PCControl.#mouse_event(System.UInt32,System.Int32,System.Int32,System.Int32,System.UInt64)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#mouse_event(System.UInt32,System.Int32,System.Int32,System.Int32,System.UInt64)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "MyHome.TcpConnection.Server")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Scope = "member", Target = "MyHome.TcpConnection.Server.#CommandReceived")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#mouse_event(System.UInt32,System.Int32,System.Int32,System.Int32,System.UInt32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3", Scope = "member", Target = "MyHome.Services.PCControl.#keybd_event(System.Byte,System.Byte,System.UInt32,System.UInt32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#keybd_event(System.Byte,System.Byte,System.UInt32,System.UInt32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "4", Scope = "member", Target = "MyHome.Services.PCControl.#mouse_event(System.UInt32,System.Int32,System.Int32,System.Int32,System.UInt32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0", Scope = "member", Target = "MyHome.Services.PCControl.#capCreateCaptureWindowA(System.String,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#capCreateCaptureWindowA(System.String,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Scope = "member", Target = "MyHome.Services.PCControl.#SendMessage(System.Int32,System.UInt32,System.Int32,MyHome.Services.PCControl+capVideoStreamCallback_t)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2", Scope = "member", Target = "MyHome.Services.PCControl.#SendMessage(System.Int32,System.UInt32,System.Int32,MyHome.Services.PCControl+capVideoStreamCallback_t)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "0", Scope = "member", Target = "MyHome.Services.PCControl.#SendMessage(System.Int32,System.UInt32,System.Int32,MyHome.Services.PCControl+capVideoStreamCallback_t)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#SendMessage(System.Int32,System.UInt32,System.Int32,MyHome.Services.PCControl+capVideoStreamCallback_t)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "MyHome.Services.PCControl.#DestroyWindow(System.Int32)")]
