Imports System.Security.Principal
Imports System.Security
Imports Microsoft.Win32

Class MainWindow
    Private Shown As Boolean = False
    Private Event OnWindowShown()
    Protected Overrides Sub OnContentRendered(e As EventArgs)
        MyBase.OnContentRendered(e)
        If Shown Then
            Exit Sub
        Else
            RaiseEvent OnWindowShown()
            Shown = True
        End If
    End Sub
    ''' <summary>
    ''' Main Entry of Application.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub MainWindow_OnWindowShown() Handles Me.OnWindowShown
        Dim Elevated As Boolean
        Dim RegEntriesFound As Integer
        Try
            'Check if elevated.
            TextBlock_Status.Text = "Detecting privileges..."
            If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
                Elevated = True
            Else
                Try
                    'Restart with admin. priv.
                    TextBlock_Status.Text = "Trying to grant administrators' privileges..."
                    Dim proc As New Process
                    With proc.StartInfo
                        .UseShellExecute = True
                        .Verb = "runas"
                        .FileName = Reflection.Assembly.GetExecutingAssembly.Location
                        .Arguments = Process.GetCurrentProcess.StartInfo.Arguments
                    End With
                    proc.Start()
                    Application.Current.Shutdown(0)
                Catch ex As Exception 'Maybe cancelled by user.
                    Elevated = False
                    Throw New SecurityException("Failed to elevate: " & ex.Message)
                End Try
            End If

            'Proceed registery keys.
            'HKLM\SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM
            'HKLM\SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM
            'HKLM\SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM
            'HKLM\SOFTWARE\Microsoft\Windows Portable Devices\Devices
            TextBlock_Status.Text = "Detecting registery keys..."
            Dim Bkey As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
            Dim Skey1 As RegistryKey = Bkey.OpenSubKey("SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM", False)
            Dim Skey2 As RegistryKey = Bkey.OpenSubKey("SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM", False)
            Dim Skey3 As RegistryKey = Bkey.OpenSubKey("SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM", False)
            Dim Skey4 As RegistryKey = Bkey.OpenSubKey("SOFTWARE\Microsoft\Windows Portable Devices\Devices", False)

            For Each SkeyName1 As String In Skey1.GetSubKeyNames()
                RegEntriesFound += 1
            Next
            For Each SkeyName2 As String In Skey2.GetSubKeyNames()
                RegEntriesFound += 1
            Next
            For Each SkeyName3 As String In Skey3.GetSubKeyNames()
                RegEntriesFound += 1
            Next
            For Each SkeyName4 As String In Skey4.GetSubKeyNames()
                RegEntriesFound += 1
            Next

            'Proceed control changes.
            TextBlock_Status.Text = "Processing control changes..."
            If RegEntriesFound = 0 Then
                TextBlock_FoundEntries.Text = "None"
            Else
                TextBlock_FoundEntries.Text = RegEntriesFound
            End If


            'Show controls and hide loading indicator.
            TextBlock_Status.Text = "Finalizing..."
            Grid_Summary.Visibility = Windows.Visibility.Visible
            Button_Reload.Visibility = Windows.Visibility.Visible
            Grid_Loading.Visibility = Windows.Visibility.Hidden
        Catch ex As Exception
            TextBlock_Status.Text = "Fatal failure: " & ex.Message
        End Try
    End Sub
End Class
