Imports System.Net.NetworkInformation
Imports System.Text.RegularExpressions
Imports System.Net

Public Class mainForm
    'This is declaring the adaptors and required objects to allow the code further down to work.
    Dim Adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
    Private CurrentNetworkInterface As NetworkInterface
    Private InitNicStats As Boolean = False
    Delegate Sub FuncCallback(ByRef obj As Object, ByVal text As String)
    Dim Thread As Threading.Thread = Nothing
    Dim NetworkBW As New NetworkMonitor
    Event NetChangedHandler As NetworkInformation.NetworkAvailabilityChangedEventHandler

    Private Sub mainForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            Thread.Abort()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub mainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LoadNicComboBox()
        Me.MaximizeBox = False
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.Fixed3D
        NICComboBox.DisplayMember = "Name"
        NICComboBox.ValueMember = "Id"
        AddHandler NetworkChange.NetworkAvailabilityChanged, AddressOf OnNetWorkChanged_Event
        AddHandler NetworkChange.NetworkAddressChanged, AddressOf OnNetworkAddrChanged_Event
    End Sub

    Private Sub LoadNicComboBox()
        Adapters = NetworkInterface.GetAllNetworkInterfaces
        Dim NetInterfaceFilter As NetworkInterfaceType() = {NetworkInterfaceType.Ethernet, NetworkInterfaceType.Ppp, NetworkInterfaceType.Wireless80211}
        If NICComboBox.InvokeRequired Then
            Dim d As New FuncCallback(AddressOf LoadNicComboBox)
            Me.Invoke(d, New Object() {Nothing, Nothing})
            'This gathers the adaptors that are live on the device currently 
        Else
            'This allows the porgram to call Network Interface and to add the interface to the droplist
            NICComboBox.Items.Clear()
            For Each nic As NetworkInterface In NetworkBW.GetNetInteface
                NICComboBox.Items.Add(nic)
            Next

            For Each adpter As NetworkInterface In NICComboBox.Items
                If adpter.Name.Equals(My.Settings.LastAdapterSelected) Then
                    NICComboBox.SelectedItem = adpter
                End If
            Next
        End If

    End Sub

    Private Function inArrayNetType(ByVal arr As NetworkInterfaceType(), ByVal match As NetworkInterfaceType) As Boolean
        For Each nettype As NetworkInterfaceType In arr
            If nettype = match Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub NICComboBox_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NICComboBox.SelectedIndexChanged
        If NICComboBox.SelectedIndex > -1 Then
            CurrentNetworkInterface = NICComboBox.SelectedItem

            Dim IpInterface As IPInterfaceProperties = CurrentNetworkInterface.GetIPProperties
            Dim UnicastIpaddr As UnicastIPAddressInformationCollection = IpInterface.UnicastAddresses
            If UnicastIpaddr.Count > 1 Then
                IpaddrLabel.Text = UnicastIpaddr(1).Address.ToString
            Else
                IpaddrLabel.Text = UnicastIpaddr(0).Address.ToString
            End If

            My.Settings.LastAdapterSelected = CurrentNetworkInterface.Name
            'This saves the last used adaptor and presets it to be used next time
            InitNicStats = False
        End If
    End Sub

    Public Function BytesConverter(ByVal bytes As Long) As String
        'This converts the bits into bytes and leads it all the way up to TB
        Dim KB As Long = 1024
        Dim MB As Long = KB * KB
        Dim GB As Long = KB * KB * KB
        Dim TB As Long = KB * KB * KB * KB
        Dim returnVal As String = "0 Bytes"

        Select Case bytes
            Case Is <= KB
                returnVal = bytes & " Bytes"
            Case Is > TB
                returnVal = (bytes / KB / KB / KB / KB).ToString("0.00") & " TB"
            Case Is > GB
                returnVal = (bytes / KB / KB / KB).ToString("0.00") & " GB"
            Case Is > MB
                returnVal = (bytes / KB / KB).ToString("0.00") & " MB"
            Case Is > KB
                returnVal = (bytes / KB).ToString("0.00") & " KB"
        End Select

        Return returnVal.ToString
    End Function

    Private Sub BandwidthPerSec()
        'This changes the labels to show the data that has been collected.
        Try

            Dim NicStats As IPv4InterfaceStatistics = NICComboBox.SelectedItem.GetIPv4Statistics
            Static LastUpload As Long = NicStats.BytesSent
            Static LastDownload As Long = NicStats.BytesReceived

            If InitNicStats = True Then
                Dim Up = NicStats.BytesSent - LastUpload
                Dim Down = NicStats.BytesReceived - LastDownload

                DataSentLabel.Text = BytesConverter(If(Up < 0, 0, Up)) & "/s"
                DataRecLabel.Text = BytesConverter(If(Down < 0, 0, Down)) & "/s"
            End If
            LastUpload = NicStats.BytesSent
            LastDownload = NicStats.BytesReceived



            If DataRecLabel.Text < "30.0 MB/s" Then
                Label7.Text = "Busy/Bad"
            Else
                Label7.Text = "Quiet/Good"
            End If

            InitNicStats = True
        Catch ex As Exception

        End Try

    End Sub

    Private Sub BandwidthUpdateTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HealthUpdateTimer.Tick
        If NICComboBox.SelectedIndex >= 0 Then
            BandwidthPerSec()
        End If
    End Sub

    Private Sub OnNetWorkChanged_Event(ByVal sender As Object, ByVal e As NetworkInformation.NetworkAvailabilityEventArgs) ' Handles Me.NetChangedHandler
        LoadNicComboBox()
    End Sub

    Private Sub OnNetworkAddrChanged_Event(ByVal sender As Object, ByVal e As EventArgs)
        LoadNicComboBox()
        Invoke(New MethodInvoker(Sub()
                                     If NICComboBox.SelectedIndex > -1 Then
                                         Dim ip = (CType(NICComboBox.SelectedItem, NetworkInterface)).GetIPProperties.UnicastAddresses(0).Address.ToString
                                         IpaddrLabel.Text = ip
                                     End If
                                 End Sub))
    End Sub




    Private Sub UpdateLabelText(ByVal text As String, ByVal control As Control)
        Invoke(New MethodInvoker(Sub()
                                     control.Text = text
                                 End Sub))
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        NICComboBox.DataSource = NetworkBW.GetNetInteface
    End Sub
    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        End
    End Sub
End Class
