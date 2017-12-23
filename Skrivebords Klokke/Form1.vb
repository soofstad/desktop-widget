Imports System
Imports System.Management


Public Class Klokke
    Dim cpu As New PerformanceCounter
    Dim totaltMinne As String = (CStr(My.Computer.Info.TotalPhysicalMemory)).Remove(5)
    Dim old_Tot_R_Bytes As Long = 0
    Dim old_Tot_T_Bytes As Long = 0
    Dim active_Nic As System.Net.NetworkInformation.NetworkInterface

    Function Get_Main_Nic() As System.Net.NetworkInformation.NetworkInterface
        Dim nics As System.Net.NetworkInformation.NetworkInterface() = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces
        For Each nic As System.Net.NetworkInformation.NetworkInterface In nics
            If nic.Name = "Ethernet" Then
                Return nic
                Exit For
            End If
        Next
        MsgBox("Could not find a Network Interface matching the name 'Ethernet'")
        Return nics(0)
    End Function

    Function get_Tot_R_Bytes()
        'Dim ipv4Stats As System.Net.NetworkInformation.NetworkInterface
        Return active_Nic.GetIPStatistics.BytesReceived
    End Function

    Function Get_Tot_T_Bytes()
        'Dim ipv4Stats As System.Net.NetworkInformation.NetworkInterface
        Return active_Nic.GetIPStatistics.BytesSent
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
        'Dim ledigMinneGB As Double = Math.Round((ledigMinne / 1000), 2)
        Dim bruktMinne As Double = (totaltMinne - ledigMinne)
        Dim bruktMinneGB As Double = Math.Round((bruktMinne / 1000), 2)
        ProgressBar1.Value = CShort(bruktMinne)
        Label9.Text = bruktMinneGB & "G"

    End Sub

    'Avslutt knapp
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    'Formater disk plass
    Function round_TotalSize(ByRef disk As System.IO.DriveInfo) As Integer
        Return Math.Round((disk.TotalSize / 1000000000), 0)
    End Function
    Function round_FreeSpace(ByRef disk As System.IO.DriveInfo) As Integer
        Return Math.Round((disk.AvailableFreeSpace / 1000000000), 0)
    End Function

    'Ved lasting flyttes prog opp i venste hjørne, CPU last skrives til label md timer.
    Private Sub Klokke_Load() Handles MyBase.Load
        Me.Location = Screen.AllScreens(2).Bounds.Location + New Point(0, 0)
        RadioButton3.Checked = 1
        ProgressBar1.Maximum = totaltMinne
        active_Nic = Get_Main_Nic()
        old_Tot_R_Bytes = get_Tot_R_Bytes()
        old_Tot_T_Bytes = Get_Tot_T_Bytes()

        'Les og sett disk størrelse og ledig
        Dim drive_Letters() As Char = {"C", "D", "E", "F"}
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
        Label19.Text = " " + edrive + "TB"

        Dim fdrive As String = CStr(round_TotalSize(drives(3)))
        fdrive = fdrive.Remove(fdrive.Length - 3)
        Label22.Text = " " + fdrive + "TB"

        ProgressBar3.Value = ((round_TotalSize(drives(0))) - round_FreeSpace(drives(0))) / round_TotalSize(drives(0)) * 100
        ProgressBar4.Value = ((round_TotalSize(drives(1))) - round_FreeSpace(drives(1))) / round_TotalSize(drives(1)) * 100
        ProgressBar5.Value = ((round_TotalSize(drives(2))) - round_FreeSpace(drives(2))) / round_TotalSize(drives(2)) * 100
        ProgressBar6.Value = ((round_TotalSize(drives(3))) - round_FreeSpace(drives(3))) / round_TotalSize(drives(3)) * 100

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
        cur_tot_bytes = Get_Tot_T_Bytes()
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

    'Reload knapp
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Klokke_Load()
    End Sub

End Class
