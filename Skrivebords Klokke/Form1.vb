Imports System
Imports System.Management


Public Class Widget
    Dim cpu As New PerformanceCounter
    ' Minna as MB
    Dim totaltMinneMB As ULong = Math.Round((My.Computer.Info.TotalPhysicalMemory / 1048576))
    Dim old_Tot_R_Bytes As Long = 0
    Dim old_Tot_T_Bytes As Long = 0
    Dim active_Nic As System.Net.NetworkInformation.NetworkInterface

    Function Get_Main_Nic() As System.Net.NetworkInformation.NetworkInterface
        Dim input_nic As String = ""
        Dim nics As System.Net.NetworkInformation.NetworkInterface() = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces
        Dim allNicsList As String = ""

        ' Loop through all NICs and return the NIC mathing on name from input.

        For Each nic In nics
            allNicsList += nic.Name + vbCrLf
        Next
        ' Get NIC to monitor from InputBox.
        input_nic = InputBox("Enter name of the network interface to monitor." + vbCrLf + "It should be one of these;" + vbCrLf + vbCrLf + allNicsList + vbCrLf, "Default NIC", "vEthernet (External Virtual Switch)")
        ' Close application if the InputBox is canceled or exited.
        If input_nic = "" Then
            Me.Close()
        End If
        For Each nic In nics
            If nic.Name = input_nic Then
                Return nic
            End If
        Next
        MsgBox("Could not find a Network Interface matching the name '" & input_nic & "' ." & vbCrLf & vbCrLf & "Using Default NIC.")
        Return nics(0)
    End Function

    Function get_Tot_R_Bytes()
        Return active_Nic.GetIPStatistics.BytesReceived
    End Function

    Function Get_Tot_T_Bytes()
        Return active_Nic.GetIPStatistics.BytesSent
    End Function

    ' Timer for each second
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        'Setter klokka
        Label1.Text = TimeOfDay

        'Setter CPU load
        With cpu
            .CategoryName = "Processor"
            .CounterName = "% Processor Time"
            .InstanceName = "_Total"
        End With

        Dim prosent As Integer = cpu.NextValue
        Label5.Text = prosent & "%"
        ProgressBar2.Value = prosent

        'Setter minne load
        Dim leigMinneMB As ULong = Math.Round((My.Computer.Info.AvailablePhysicalMemory / 1048576))
        Dim bruktMinneMB As ULong = (totaltMinneMB - leigMinneMB)
        ProgressBar1.Value = CShort(bruktMinneMB)
        Label9.Text = Math.Round((bruktMinneMB / 1024), 2) & "G"

    End Sub

    ' Timer for every 3 second. Network monitoring.
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        'Download
        Dim cur_tot_bytes As Long = get_Tot_R_Bytes()
        Dim bytes_Diff = cur_tot_bytes - old_Tot_R_Bytes
        old_Tot_R_Bytes = cur_tot_bytes
        Dim down_speed As Double = CDbl(bytes_Diff)
        Dim suffix As String = "error"

        Select Case down_speed
            Case 0 To 1024
                down_speed = down_speed / 3
                down_speed = Math.Round(down_speed, 0)
                suffix = "B/s"
            Case 1025 To 1048576
                down_speed = down_speed / 1024 / 3
                down_speed = Math.Round(down_speed, 0)
                suffix = "KB/s"
            Case 1048577 To 1000000000
                down_speed = down_speed / 1024 / 1024 / 3
                down_speed = Math.Round(down_speed, 1)
                suffix = "MB/s"
        End Select

        Label15.Text = down_speed.ToString & suffix

        'Upload
        cur_tot_bytes = Get_Tot_T_Bytes()
        bytes_Diff = cur_tot_bytes - old_Tot_T_Bytes
        old_Tot_T_Bytes = cur_tot_bytes

        Dim up_speed As Double = CDbl(bytes_Diff)
        suffix = "error"

        Select Case up_speed
            Case 0 To 1024
                up_speed = up_speed / 3
                up_speed = Math.Round(up_speed, 0)
                suffix = "B/s"
            Case 1025 To 1048576
                up_speed = up_speed / 1024 / 3
                up_speed = Math.Round(up_speed, 0)
                suffix = "KB/s"
            Case 1048577 To 1000000000
                up_speed = up_speed / 1024 / 1024 / 3
                up_speed = Math.Round(up_speed, 1)
                suffix = "MB/s"
        End Select

        Label12.Text = up_speed.ToString & suffix
    End Sub

    'Format stings in disk size to GB
    Function round_TotalSize(ByRef disk As System.IO.DriveInfo) As Integer
        Return Math.Round((disk.TotalSize / 1073741824), 0)
    End Function
    Function round_FreeSpace(ByRef disk As System.IO.DriveInfo) As Integer
        Return Math.Round((disk.AvailableFreeSpace / 1073741824), 0)
    End Function

    'Ved lasting flyttes prog opp i venste hjørne, CPU last skrives til label md timer.
    Private Sub Widget_Load() Handles MyBase.Load
        Me.Location = Screen.AllScreens(2).Bounds.Location + New Point(0, 0)
        RadioButton3.Checked = 1
        ProgressBar1.Maximum = totaltMinneMB
        Label10.Text = CStr(Math.Round((totaltMinneMB / 1024), 0)) + "GB"
        active_Nic = Get_Main_Nic()
        old_Tot_R_Bytes = get_Tot_R_Bytes()
        old_Tot_T_Bytes = Get_Tot_T_Bytes()

        'Les og sett disk størrelse og ledig
        Dim drive_Letters() As Char = {"C", "D", "E"}
        Dim drives(drive_Letters.Length) As System.IO.DriveInfo

        Dim i As Integer = 0
        For Each drv_letter As Char In drive_Letters
            Dim mount_point As String = drv_letter + ":\"
            drives(i) = My.Computer.FileSystem.GetDriveInfo(mount_point)
            i += 1
        Next

        Label3.Text = " " + CStr(round_TotalSize(drives(0))) + "GB"
        Label16.Text = " " + CStr(round_TotalSize(drives(1))) + "GB"

        Dim edrive As String = CStr(round_TotalSize(drives(2)))
        edrive = edrive.Remove(edrive.Length - 3)
        Label19.Text = " " + CStr(round_TotalSize(drives(2))) + "GB"

        'Dim fdrive As String = CStr(round_TotalSize(drives(3)))
        'fdrive = fdrive.Remove(fdrive.Length - 3)
        'Label22.Text = " " + fdrive + "TB"

        ProgressBar3.Value = ((round_TotalSize(drives(0))) - round_FreeSpace(drives(0))) / round_TotalSize(drives(0)) * 100
        ProgressBar4.Value = ((round_TotalSize(drives(1))) - round_FreeSpace(drives(1))) / round_TotalSize(drives(1)) * 100
        ProgressBar5.Value = ((round_TotalSize(drives(2))) - round_FreeSpace(drives(2))) / round_TotalSize(drives(2)) * 100
        'ProgressBar6.Value = ((round_TotalSize(drives(3))) - round_FreeSpace(drives(3))) / round_TotalSize(drives(3)) * 100

        ' Enable Timers
        Timer1.Enabled = True
        Timer2.Enabled = True
    End Sub
    ' Batch files located in 'project'\bin\debug
    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If RadioButton1.Checked = True Then
            System.Diagnostics.Process.Start("setTV.bat")
        End If

    End Sub

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        If RadioButton3.Checked = True Then
            System.Diagnostics.Process.Start("setPC.bat")
        End If

    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.CheckState = 1 Then
            CheckBox1.BackgroundImage = My.Resources.micred
        Else
            CheckBox1.BackgroundImage = My.Resources.images1
        End If
        System.Diagnostics.Process.Start("muteMic.bat")
    End Sub

    'Avslutt knapp
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    'Reload knapp
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Widget_Load()
    End Sub


End Class
