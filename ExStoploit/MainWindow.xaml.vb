Imports System.Security.Principal
Imports System.Security
Imports Microsoft.Win32
Imports System.Windows.Threading
Imports System.Threading
Imports System.Windows

Class MainWindow

#Region "Includings"

    ''' <summary>
    ''' Has the Shown() event already been raised once?
    ''' </summary>
    ''' <remarks></remarks>
    Private Shown As Boolean = False

    Private SelectedRegistry As String

    ''' <summary>
    ''' Equals to Shown() in WinForm programming.
    ''' </summary>
    ''' <remarks></remarks>
    Private Event OnWindowShown()

    ''' <summary>
    ''' Wait a minute... emmm...
    ''' via Stack Overflow.
    ''' </summary>
    ''' <param name="seconds"></param>
    ''' <remarks></remarks>
    Private Sub Wait(ByVal Seconds As Double)
        Dim Frame = New DispatcherFrame()
        Dim Thr As New Thread(CType((Sub()
                                         Thread.Sleep(TimeSpan.FromSeconds(Seconds))
                                         Frame.[Continue] = False
                                     End Sub), ThreadStart))
        Thr.Start()
        Dispatcher.PushFrame(Frame)
    End Sub

    ''' <summary>
    ''' Converts an object into System.Windows.Media.ImageSource object.
    ''' </summary>
    ''' <param name="source">Source object to convert.</param>
    ''' <returns>Converted ImageSource object.</returns>
    ''' <remarks></remarks>
    Private Function ConvertToImageSource(source As Object, Width As Integer, Height As Integer) As Media.ImageSource
        Dim result As ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
             source.GetHbitmap(),
             IntPtr.Zero,
             System.Windows.Int32Rect.Empty,
             BitmapSizeOptions.FromWidthAndHeight(Width, Height))
        Return result
    End Function

    ''' <summary>
    ''' Equals to OnShown() in WinForm programming.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnContentRendered(e As EventArgs)
        MyBase.OnContentRendered(e)
        If Shown Then
            Exit Sub
        Else
            RaiseEvent OnWindowShown()
            Shown = True
        End If
    End Sub

    Private Enum DeviceType
        WPD_DEVICE_TYPE_GENERIC = 0
        WPD_DEVICE_TYPE_CAMERA = 1
        WPD_DEVICE_TYPE_MEDIA_PLAYER = 2
        WPD_DEVICE_TYPE_PHONE = 3
    End Enum

#End Region

    ''' <summary>
    ''' Do some UI adjustments before the window shows up.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Hide controls
        Grid_Summary.Visibility = Windows.Visibility.Hidden
        ProgressRing_List_LoadIndicator.Visibility = Windows.Visibility.Hidden
        Button_Reload.Visibility = Windows.Visibility.Hidden
        TabItem_Actions.Visibility = Windows.Visibility.Hidden
        TabItem_List.Visibility = Windows.Visibility.Hidden
        TabItem_About.Visibility = Windows.Visibility.Hidden
        TabItem_Status.Header = "LOADING"
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
            Wait(0.2)
            If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
                Elevated = True
                Canvas_Permissions.Source = ConvertToImageSource(My.Resources.ic_done_black_16dp_2x, 32, 32)
            Else
                Try
                    'Restart with admin. priv.
                    TextBlock_Status.Text = "Acquiring administrators' privileges..."
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

            'Proceed registry keys.
            'HKLM\SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM
            'HKLM\SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM
            'HKLM\SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM
            'HKLM\SOFTWARE\Microsoft\Windows Portable Devices\Devices
            TextBlock_Status.Text = "Detecting registry keys..."
            Wait(0.2)
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
                Canvas_FoundEntries.Source = ConvertToImageSource(My.Resources.ic_done_black_16dp_2x, 32, 32)
            Else
                TextBlock_FoundEntries.Foreground = System.Windows.Media.Brushes.Orange
                TextBlock_FoundEntries.Text = RegEntriesFound
                Canvas_FoundEntries.Source = ConvertToImageSource(My.Resources.ic_info_outline_black_18dp_2x, 36, 36)
            End If

            'Show controls and hide loading indicator.
            TextBlock_Status.Text = "Finalizing..."
            Grid_Summary.Visibility = Windows.Visibility.Visible
            Button_Reload.Visibility = Windows.Visibility.Visible
            TabItem_Actions.Visibility = Windows.Visibility.Visible
            TabItem_List.Visibility = Windows.Visibility.Visible
            TabItem_About.Visibility = Windows.Visibility.Visible
            Grid_Loading.Visibility = Windows.Visibility.Hidden
            TabItem_Status.Header = "STATUS"
        Catch ex As Exception
            TextBlock_Status.Text = "Fatal failure: " & ex.Message
        End Try
    End Sub

    Private Sub Button_Reload_Click(sender As Object, e As RoutedEventArgs) Handles Button_Reload.Click
        Button_Reload.IsEnabled = False
        Image_Reloaded.Source = ConvertToImageSource(My.Resources.ic_autorenew_black_36dp_2x, 72, 72)
        Grid_Loading.Visibility = Windows.Visibility.Visible
        MainWindow_OnWindowShown()
        Image_Reloaded.Source = ConvertToImageSource(My.Resources.ic_done_black_32dp_2x, 64, 64)
        Button_Reload.IsEnabled = True
        Wait(3)
        Image_Reloaded.Source = Nothing
    End Sub

    Private Sub ComboBox_SourceRegistry_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBox_SourceRegistry.SelectionChanged
        ProgressRing_List_LoadIndicator.Visibility = Windows.Visibility.Visible
        ComboBox_DetectedDrives.Items.Clear()
        Dim Bkey As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
        Dim Skey As RegistryKey = Nothing

        Select Case ComboBox_SourceRegistry.SelectedIndex
            Case 0 'CurrentControlSet
                Skey = Bkey.OpenSubKey("SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM", False)
                SelectedRegistry = "SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM"
            Case 1 'ControlSet001
                Skey = Bkey.OpenSubKey("SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM", False)
                SelectedRegistry = "SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM"
            Case 2 'ControlSet002
                Skey = Bkey.OpenSubKey("SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM", False)
                SelectedRegistry = "SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM"
            Case 3 'WPD
                Skey = Bkey.OpenSubKey("SOFTWARE\Microsoft\Windows Portable Devices\Devices", False)
                SelectedRegistry = "SOFTWARE\Microsoft\Windows Portable Devices\Devices"
        End Select

        Dim Items() As String = Skey.GetSubKeyNames()
        For Each s As String In Items
            ComboBox_DetectedDrives.Items.Add(New ComboBoxItem With {.Content = s})
        Next
        ProgressRing_List_LoadIndicator.Visibility = Windows.Visibility.Hidden
    End Sub

    Private Sub ComboBox_DetectedDrives_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBox_DetectedDrives.SelectionChanged
        ProgressRing_List_LoadIndicator.Visibility = Windows.Visibility.Visible
        Image_List_Status.Source = ConvertToImageSource(My.Resources.ic_autorenew_black_36dp_2x, 32, 32)

        TextBox_Result.Clear()

        Try
            If ComboBox_DetectedDrives.SelectedValue.Equals(Nothing) Then 'Prevent null error.
                Image_List_Status.Source = Nothing
				Exit Sub
            End If
        Catch ex As Exception
			Image_List_Status.Source = Nothing
            Exit Sub
        End Try

        Try 'Prevent, again, errors.
            Dim DevType As String = "Unknown"
            Dim Dev As String = ComboBox_DetectedDrives.SelectedItem.Content
            Dim DestinationPath As String = SelectedRegistry & "\" & Dev
            Dim Skey As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey(DestinationPath, False)
            Dim SubSkey As RegistryKey = Skey.OpenSubKey("Device Parameters", False)
            Select Case SubSkey.GetValue("PortableDeviceType")
                Case DeviceType.WPD_DEVICE_TYPE_GENERIC
                    If SubSkey.GetValue("PortableDeviceIsMassStorage") = 1 Then
                        If Dev.ToLower.IndexOf("{") = 0 Then
                            DevType = "UAS (USB Attached SCSI) Device"
                        End If
                        If Dev.ToLower.IndexOf("_??_USBSTOR#Disk".ToLower) = 0 Then
                            DevType = "USB Flash Drive"
                        End If
                    Else
                        DevType = "Generic"
                    End If
                Case DeviceType.WPD_DEVICE_TYPE_CAMERA
                    DevType = "Camera"
                Case DeviceType.WPD_DEVICE_TYPE_MEDIA_PLAYER
                    DevType = "Media Player"
                Case DeviceType.WPD_DEVICE_TYPE_PHONE
                    DevType = "Phone"
                Case Else
                    DevType = "Unknown"
            End Select

            TextBox_Result.Clear()
            TextBox_Result.AppendText("## System Information ##" & vbCrLf)
            TextBox_Result.AppendText("Class GUID: " & Skey.GetValue("ClassGUID") & vbCrLf)
            TextBox_Result.AppendText("Device Container GUID: " & Skey.GetValue("ContainerID") & vbCrLf)
            TextBox_Result.AppendText("Driver: " & Skey.GetValue("Driver") & vbCrLf)
            TextBox_Result.AppendText("Service: " & Skey.GetValue("Service") & vbCrLf)
            TextBox_Result.AppendText("WMDMSP CLSID: " & SubSkey.GetValue("WMDMSPCLSID") & vbCrLf & vbCrLf)

            TextBox_Result.AppendText("Legacy Support Enabled: " & SubSkey.GetValue("EnableLegacySupport") & vbCrLf)
            If SubSkey.GetValue("PortableDeviceIsMassStorage") = 1 Then TextBox_Result.AppendText("Optimal Transfer Size: " & SubSkey.GetValue("OptimalTransferSize") & vbCrLf & vbCrLf)

            TextBox_Result.AppendText("## Device Information ##" & vbCrLf)
            TextBox_Result.AppendText("Manufacturer: " & Skey.GetValue("Mfg") & vbCrLf)
            TextBox_Result.AppendText("Friendly Name: " & Skey.GetValue("FriendlyName") & vbCrLf)
            TextBox_Result.AppendText("Detected device type: " & DevType & vbCrLf & vbCrLf)

            TextBox_Result.AppendText("## Note ##" & vbCrLf)
            TextBox_Result.AppendText("- The Class GUID should be the same across all detected storage devices, even on different computers." & vbCrLf)
            TextBox_Result.AppendText("- WMDMSP CLSID is the short for 'Windows Master Data Management Security Policy CLasS IDentifier'." & vbCrLf)

            Image_List_Status.Source = ConvertToImageSource(My.Resources.ic_done_black_32dp_2x, 32, 32)
        Catch ex As Exception
            Image_List_Status.Source = ConvertToImageSource(My.Resources.ic_warning_black_36dp_2x, 32, 32)
        End Try
        ProgressRing_List_LoadIndicator.Visibility = Windows.Visibility.Hidden
    End Sub
End Class
