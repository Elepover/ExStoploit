Imports System.Security.Principal
Imports System.Security
Imports Microsoft.Win32
Imports System.Windows.Threading
Imports System.Threading
Imports System.Windows

Class MainWindow

#Region "Licenses"
    Private Const GPLv3 As String = "Copyright (C) 2017 Elepover" & vbCrLf & vbCrLf & "This program is free software: you can redistribute it and/or modify" & vbCrLf & "it under the terms of the GNU General Public License as published by" & vbCrLf & "the Free Software Foundation, either version 3 of the License, or" & vbCrLf & "(at your option) any later version." & vbCrLf & vbCrLf & "This program is distributed in the hope that it will be useful," & vbCrLf & "but WITHOUT ANY WARRANTY; without even the implied warranty of" & vbCrLf & "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the" & vbCrLf & "GNU General Public License for more details." & vbCrLf & vbCrLf & "You should have received a copy of the GNU General Public License" & vbCrLf & "along with this program.  If not, see <https://www.gnu.org/licenses/>."
    Private Const MITLicense As String = "MIT License (MIT)" & vbCrLf & vbCrLf & "Copyright (c) 2016 MahApps" & vbCrLf & vbCrLf & "Permission is hereby granted, free of charge, to any person obtaining a copy" & vbCrLf & "of this software and associated documentation files (the 'Software'), to deal" & vbCrLf & "in the Software without restriction, including without limitation the rights" & vbCrLf & "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell" & vbCrLf & "copies of the Software, and to permit persons to whom the Software is" & vbCrLf & "furnished to do so, subject to the following conditions:" & vbCrLf & vbCrLf & "The above copyright notice and this permission notice shall be included in all" & vbCrLf & "copies or substantial portions of the Software." & vbCrLf & vbCrLf & "THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR" & vbCrLf & "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY," & vbCrLf & "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE" & vbCrLf & "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER" & vbCrLf & "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM," & vbCrLf & "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE" & vbCrLf & "SOFTWARE."
#End Region

#Region "Multithreading Helper"
    ''' <summary>
    ''' Asyncously show a picture (usually an alert) for a while. All exceptions will be ignored.
    ''' </summary>
    ''' <param name="Seconds">Time, measured as seconds.</param>
    ''' <param name="Image">Target image.</param>
    ''' <param name="Control">Target control.
    ''' CONTROL ONLY, DO NOT ADD .SOURCE.</param>
    ''' <remarks></remarks>
    Private Sub DelayImg(Seconds As Double, Image As Object, Width As Integer, Height As Integer, Control As Image)
        Try
            Dim Thr As New Thread(CType((Sub()
                                             Try
                                                 Control.Dispatcher.Invoke(Sub()
                                                                               InlineAssignHelper(Control.Source, ConvertToImageSource(Image, Width, Height))
                                                                           End Sub, DispatcherPriority.Normal)
                                                 Thread.Sleep(TimeSpan.FromSeconds(Seconds))
                                                 Control.Dispatcher.Invoke(Sub()
                                                                               InlineAssignHelper(Control.Source, Nothing)
                                                                           End Sub, DispatcherPriority.Normal)
                                             Catch ex As Exception
                                             End Try
                                         End Sub), ThreadStart))
            Thr.Start()
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' Inline value assigning helper.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="target"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function
#End Region

#Region "Includings"
    ''' <summary>
    ''' Has the Shown() event already been raised once?
    ''' </summary>
    ''' <remarks></remarks>
    Private Shown As Boolean = False

    Private CommandLineArgs As String

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

    ''' <summary>
    ''' WPD Device Type Enum
    ''' </summary>
    ''' <remarks></remarks>
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
        'CommandLine
        CommandLineArgs = Environment.CommandLine
        'Hide controls
        Grid_Summary.Visibility = Windows.Visibility.Hidden
        ProgressRing_List_LoadIndicator.Visibility = Windows.Visibility.Hidden
        ProgressRing_Delete.Visibility = Windows.Visibility.Hidden
        Button_Reload.Visibility = Windows.Visibility.Hidden
        TabItem_Actions.Visibility = Windows.Visibility.Hidden
        TabItem_List.Visibility = Windows.Visibility.Hidden
        TabItem_About.Visibility = Windows.Visibility.Hidden
        TabItem_Status.Header = "LOADING"
        'Oh Copyright, yes.
        TextBox_Copyright.Text = "ExStoploit" & vbCrLf & GPLv3 & vbCrLf & vbCrLf & "MahApps.Metro" & vbCrLf & MITLicense
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
            Wait(0.05)
            If New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) Then
                Elevated = True
                Canvas_Permissions.Source = ConvertToImageSource(My.Resources.ic_done_black_16dp_2x, 32, 32)
                TextBlock_Permissions.Foreground = System.Windows.Media.Brushes.Green
                TextBlock_Permissions.Text = "Granted"
            Else
                Try
                    If Command().ToLower.Contains("-p") Then
                        'Ignore permissions acquiring.
                        Elevated = False
                        Canvas_Permissions.Source = ConvertToImageSource(My.Resources.ic_warning_black_36dp_2x, 16, 16)
                        TextBlock_Permissions.Foreground = System.Windows.Media.Brushes.Orange
                        TextBlock_Permissions.Text = "Not Granted"
                        GoTo JumpOut
                    End If
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

JumpOut:

            'Proceed registry keys.
            'HKLM\SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM
            'HKLM\SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM
            'HKLM\SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM
            'HKLM\SOFTWARE\Microsoft\Windows Portable Devices\Devices
            TextBlock_Status.Text = "Detecting registry keys..."
            Wait(0.05)
            Dim Bkey As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
            Dim Skey1 As RegistryKey = Bkey.OpenSubKey("SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM", False)
            Dim Skey2 As RegistryKey = Bkey.OpenSubKey("SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM", False)
            Dim Skey3 As RegistryKey = Bkey.OpenSubKey("SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM", False)
            Dim Skey4 As RegistryKey = Bkey.OpenSubKey("SOFTWARE\Microsoft\Windows Portable Devices\Devices", False)

            Try
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
            Catch ex As Exception
            End Try


            'Proceed control changes.
            TextBlock_Status.Text = "Processing control changes..."
            If RegEntriesFound = 0 Then
                TextBlock_FoundEntries.Foreground = System.Windows.Media.Brushes.Green
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
            If CommandLineArgs.Contains("-d") Then
                MsgBox(ex.ToString)
            End If
            TextBlock_Status.Text = "Fatal failure: " & ex.Message
        End Try
    End Sub

    Private Sub Button_Reload_Click(sender As Object, e As RoutedEventArgs) Handles Button_Reload.Click
        Button_Reload.IsEnabled = False

        TextBlock_FoundEntries.Text = "Waiting..."
        TextBlock_Permissions.Text = "Waiting..."
        TextBlock_FoundEntries.Foreground = System.Windows.Media.Brushes.DarkCyan
        TextBlock_Permissions.Foreground = System.Windows.Media.Brushes.DarkCyan
        Image_Reloaded.Source = ConvertToImageSource(My.Resources.ic_autorenew_black_36dp_2x, 72, 72)

        Grid_Loading.Visibility = Windows.Visibility.Visible
        MainWindow_OnWindowShown()

        Button_Reload.IsEnabled = True
        DelayImg(2.5, My.Resources.ic_done_black_32dp_2x, 64, 64, Image_Reloaded)
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

        Try
            Dim Items() As String = Skey.GetSubKeyNames()
            For Each s As String In Items
                ComboBox_DetectedDrives.Items.Add(New ComboBoxItem With {.Content = s})
            Next
        Catch ex As Exception
            Image_List_Status.Source = ConvertToImageSource(My.Resources.ic_warning_black_36dp_2x, 32, 32)
        End Try
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

            'Only works with beginning "_??_USBSTOR#Disk"
            'Example: _??_USBSTOR#Disk&Ven_SMI&Prod_USB_DISK&Rev_233#8&00000000&0#{00000000-0000-0000-0000-000000000000}
            Dim ProdCode As String = "Not available."
            Dim DevGUID As String = "Not available."
            Dim DevRevision As String = "Not available."
            If Dev.IndexOf("_??_USBSTOR#Disk") = 0 Then
                'Try to get device product code.
                Dim ProdCodeCharBeginLoc As Integer = Dev.IndexOf("&Prod_") + 6
                Dim ProdCodeCharEndLoc As Integer = Dev.IndexOf("&Rev_")
                ProdCode = Dev.Substring(ProdCodeCharBeginLoc, ProdCodeCharEndLoc - ProdCodeCharBeginLoc)

                'Try to get device GUID.
                Dim GUIDCharBeginLoc As Integer = Dev.IndexOf("#{") + 2
                Dim GUIDCharEndLoc As Integer = Dev.IndexOf("}")
                DevGUID = Dev.Substring(GUIDCharBeginLoc, GUIDCharEndLoc - GUIDCharBeginLoc)

                'Try to get device Revision Number.
                Dim RevCharBeginLoc As Integer = Dev.IndexOf("&Rev_") + 5
                Dim CuttedStr As String = Dev.Substring(RevCharBeginLoc, Dev.Length - RevCharBeginLoc)
                Dim HashtagLoc As Integer = CuttedStr.IndexOf("#")
                DevRevision = CuttedStr.Substring(0, HashtagLoc)

            End If

            TextBox_Result.Clear()

            TextBox_Result.AppendText("## Device Information ##" & vbCrLf)
            TextBox_Result.AppendText("Manufacturer: " & Skey.GetValue("Mfg") & vbCrLf)
            TextBox_Result.AppendText("Friendly Name: " & Skey.GetValue("FriendlyName") & vbCrLf)
            TextBox_Result.AppendText("Detected Device Type: " & DevType & vbCrLf)
            TextBox_Result.AppendText("*Device Product Code: " & ProdCode & vbCrLf)
            TextBox_Result.AppendText("*Device GUID: " & DevGUID & vbCrLf)
            TextBox_Result.AppendText("*Device Revision: " & DevRevision & vbCrLf & vbCrLf)

            TextBox_Result.AppendText("## System Information ##" & vbCrLf)
            TextBox_Result.AppendText("Class GUID: " & Skey.GetValue("ClassGUID") & vbCrLf)
            TextBox_Result.AppendText("Device Container GUID: " & Skey.GetValue("ContainerID") & vbCrLf)
            TextBox_Result.AppendText("Driver: " & Skey.GetValue("Driver") & vbCrLf)
            TextBox_Result.AppendText("Service: " & Skey.GetValue("Service") & vbCrLf)
            TextBox_Result.AppendText("WMDMSP CLSID: " & SubSkey.GetValue("WMDMSPCLSID") & vbCrLf & vbCrLf)

            TextBox_Result.AppendText("Legacy Support Enabled: " & SubSkey.GetValue("EnableLegacySupport") & vbCrLf)
            If SubSkey.GetValue("PortableDeviceIsMassStorage") = 1 Then TextBox_Result.AppendText("Optimal Transfer Size: " & SubSkey.GetValue("OptimalTransferSize") & vbCrLf & vbCrLf)

            TextBox_Result.AppendText("## Note ##" & vbCrLf)
            TextBox_Result.AppendText("- The Class GUID should be the same across all detected storage devices, even on different computers." & vbCrLf)
            TextBox_Result.AppendText("- WMDMSP CLSID is the short for 'Windows Master Data Management Security Policy CLasS IDentifier'." & vbCrLf)
            TextBox_Result.AppendText("- Items beginning with * means this property is only available among USB Flash Drives." & vbCrLf)

            DelayImg(2.5, My.Resources.ic_done_black_32dp_2x, 32, 32, Image_List_Status)
        Catch ex As Exception
            Image_List_Status.Source = ConvertToImageSource(My.Resources.ic_warning_black_36dp_2x, 32, 32)
        End Try
        ProgressRing_List_LoadIndicator.Visibility = Windows.Visibility.Hidden
    End Sub

    Private Sub Button_Export_Click(sender As Object, e As RoutedEventArgs) Handles Button_Export.Click
        Image_List_Status.Source = ConvertToImageSource(My.Resources.ic_autorenew_black_36dp_2x, 32, 32)
        Try
            My.Computer.Clipboard.SetText(TextBox_Result.Text)
        Catch ex As Exception
        End Try
        DelayImg(2.5, My.Resources.ic_done_black_32dp_2x, 32, 32, Image_List_Status)
    End Sub

    Private Sub Button_Delete_Click(sender As Object, e As RoutedEventArgs) Handles Button_Delete.Click
        Try
            Button_Delete.IsEnabled = False
            CheckBox_AcceptDeletion.IsEnabled = False
            ProgressRing_Delete.Visibility = Windows.Visibility.Visible

            If CheckBox_AcceptDeletion.IsChecked = False Then
                DelayImg(2.5, My.Resources.ic_warning_black_36dp_2x, 32, 32, Image_Actions_Status)
                CheckBox_AcceptDeletion.IsChecked = True
                Wait(0.15)
                CheckBox_AcceptDeletion.IsChecked = False
                Exit Sub
            End If

            'Start deletion
            Dim Bkey As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
            Dim Skey1 As RegistryKey = Bkey.OpenSubKey("SYSTEM\ControlSet001\Enum\SWD\WPDBUSENUM", True)
            Dim Skey2 As RegistryKey = Bkey.OpenSubKey("SYSTEM\ControlSet002\Enum\SWD\WPDBUSENUM", True)
            Dim Skey3 As RegistryKey = Bkey.OpenSubKey("SYSTEM\CurrentControlSet\Enum\SWD\WPDBUSENUM", True)
            Dim Skey4 As RegistryKey = Bkey.OpenSubKey("SOFTWARE\Microsoft\Windows Portable Devices\Devices", True)

            If CheckBox_ControlSet001.IsChecked Then
                For Each SkeyName1 As String In Skey1.GetSubKeyNames()
                    Skey1.DeleteSubKeyTree(SkeyName1)
                Next
            End If

            If CheckBox_ControlSet002.IsChecked Then
                For Each SkeyName2 As String In Skey2.GetSubKeyNames()
                    Skey2.DeleteSubKeyTree(SkeyName2)
                Next
            End If

            If CheckBox_CurrentControlSet.IsChecked Then
                For Each SkeyName3 As String In Skey3.GetSubKeyNames()
                    Skey3.DeleteSubKeyTree(SkeyName3)
                Next
            End If

            If CheckBox_WPDENUM.IsChecked Then
                For Each SkeyName4 As String In Skey4.GetSubKeyNames()
                    Skey4.DeleteSubKeyTree(SkeyName4)
                Next
            End If
            DelayImg(10, My.Resources.ic_done_black_32dp_2x, 32, 32, Image_Actions_Status)
        Catch ex As Exception
            Image_Actions_Status.Source = ConvertToImageSource(My.Resources.ic_warning_black_36dp_2x, 32, 32)
        Finally
            Button_Delete.IsEnabled = True
            CheckBox_AcceptDeletion.IsEnabled = True
            ProgressRing_Delete.Visibility = Windows.Visibility.Hidden
        End Try
    End Sub
End Class
