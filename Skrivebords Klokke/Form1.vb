Imports System
Imports System.Management


Public Class Klokke
    Dim cpu As New PerformanceCounter
    Dim totaltMinne As String = (CStr(My.Computer.Info.TotalPhysicalMemory)).Remove(5)
    Dim old_Tot_R_Bytes As Long = 0
    Dim old_Tot_T_Bytes As Long = 0

    Function get_Tot_R_Bytes()
        Dim ipv4Stats As System.Net.NetworkInformation.NetworkInterface
        Return ipv4Stats.GetAllNetworkInterfaces(1).GetIPStatistics.BytesReceived
    End Function

    Function get_Tot_T_Bytes()
        Dim ipv4Stats As System.Net.NetworkInformation.NetworkInterface
        Return ipv4Stats.GetAllNetworkInterfaces(1).GetIPStatistics.BytesSent
    End Function

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
        Dim ledigMinne As Integer = (CStr(My.Computer.Info.AvailablePhysicalMemory)).Remove(5)
        Dim ledigMinneGB As Double = Math.Round((ledigMinne / 1000), 2)
        Dim bruktMinne As Double = (totaltMinne - ledigMinne)
        Dim bruktMinneGB As Double = Math.Round((bruktMinne / 1000), 2)
        ProgressBar1.Value = CShort(bruktMinne)
        Label9.Text = bruktMinneGB & "G"




    End Sub

    'Avslutt knapp
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    'Ved lasting flyttes prog opp i venste hjørne, CPU last skrives til label md timer.
    Private Sub Klokke_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Location = Screen.AllScreens(2).Bounds.Location + New Point(0, 0)
        RadioButton3.Checked = 1
        old_Tot_R_Bytes = get_Tot_R_Bytes()
        old_Tot_T_Bytes = get_Tot_T_Bytes()

        ProgressBar1.Maximum = totaltMinne

        'Les og sett disk størrelse og ledig
        Dim cdrive As System.IO.DriveInfo
        Dim ddrive As System.IO.DriveInfo
        Dim edrive As System.IO.DriveInfo
        Dim fdrive As System.IO.DriveInfo
        cdrive = My.Computer.FileSystem.GetDriveInfo("C:\")
        ddrive = My.Computer.FileSystem.GetDriveInfo("D:\")
        edrive = My.Computer.FileSystem.GetDriveInfo("E:\")
        fdrive = My.Computer.FileSystem.GetDriveInfo("F:\")

        Dim cdrive_Total As Integer = Math.Round((cdrive.TotalSize / 1000000000), 0)
        Dim ddrive_Total As Integer = Math.Round((ddrive.TotalSize / 1000000000), 0)
        Dim edrive_Total As Integer = Math.Round((edrive.TotalSize / 1000000000000), 0)
        Dim fdrive_Total As Integer = Math.Round((fdrive.TotalSize / 1000000000000), 0)

        Dim cdrive_Free As Integer = Math.Round((cdrive.AvailableFreeSpace / 1000000000), 0)
        Dim ddrive_Free As Integer = Math.Round((ddrive.AvailableFreeSpace / 1000000000), 0)
        Dim edrive_Free As Integer = Math.Round((edrive.AvailableFreeSpace / 1000000000000), 0)
        Dim fdrive_Free As Integer = Math.Round((fdrive.AvailableFreeSpace / 1000000000000), 0)

        Label3.Text = " " + CStr(cdrive_Total) + "GB"
        Label16.Text = " " + CStr(ddrive_Total) + "GB"
        Label19.Text = " " + CStr(edrive_Total) + "TB"
        Label22.Text = " " + CStr(fdrive_Total) + "TB"

        ProgressBar3.Value = (((cdrive_Total - cdrive_Free) / cdrive_Total)) * 100
        ProgressBar4.Value = (((ddrive_Total - ddrive_Free) / ddrive_Total)) * 100
        ProgressBar5.Value = (((edrive_Total - edrive_Free) / edrive_Total)) * 100
        ProgressBar6.Value = (((fdrive_Total - fdrive_Free) / fdrive_Total)) * 100

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

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        'Nettverk monitorering

        'Download
        Dim cur_tot_bytes As Long = get_Tot_R_Bytes()
        Dim bytes_Diff = cur_tot_bytes - old_Tot_R_Bytes
        old_Tot_R_Bytes = cur_tot_bytes
        Dim down_speed As Double = CDbl(bytes_Diff)
        Dim suffix As String = "error"

        Select Case down_speed
            Case 0 To 1000
                down_speed = down_speed / 3
                down_speed = Math.Round(down_speed, 0)
                suffix = "B/s"
            Case 1000 To 1000000
                down_speed = down_speed / 1000 / 3
                down_speed = Math.Round(down_speed, 0)
                suffix = "KB/s"
            Case 1000000 To 1000000000
                down_speed = down_speed / 1000 / 1000 / 3
                down_speed = Math.Round(down_speed, 1)
                suffix = "MB/s"
        End Select

        Label15.Text = down_speed.ToString & suffix

        'Upload
        cur_tot_bytes = get_Tot_T_Bytes()
        bytes_Diff = cur_tot_bytes - old_Tot_T_Bytes
        old_Tot_T_Bytes = cur_tot_bytes

        Dim up_speed As Double = CDbl(bytes_Diff)
        suffix = "error"

        Select Case up_speed
            Case 0 To 1000
                up_speed = up_speed / 3
                up_speed = Math.Round(up_speed, 0)
                suffix = "B/s"
            Case 1000 To 1000000
                up_speed = up_speed / 1000 / 3
                up_speed = Math.Round(up_speed, 0)
                suffix = "KB/s"
            Case 1000000 To 1000000000
                up_speed = up_speed / 1000 / 1000 / 3
                up_speed = Math.Round(up_speed, 1)
                suffix = "MB/s"
        End Select

        Label12.Text = up_speed.ToString & suffix
    End Sub

End Class
