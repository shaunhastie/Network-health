Imports System.Text.RegularExpressions
Imports System.Net.NetworkInformation

Public Class NetworkMonitor
    Private Adapters As NetworkInterface()

    Public Sub New()

    End Sub

    Public Function GetNetInteface() As List(Of NetworkInterface)
        Adapters = NetworkInterface.GetAllNetworkInterfaces
        Dim NetList As New List(Of NetworkInterface)
        Dim NetInterfaceFilter As NetworkInterfaceType() = {NetworkInterfaceType.Ethernet, NetworkInterfaceType.Ppp, NetworkInterfaceType.Wireless80211}
        For Each adapter As NetworkInterface In Adapters
            If inArrayNetType(NetInterfaceFilter, adapter.NetworkInterfaceType) Then
                NetList.Add(adapter)
            End If
        Next
        Return NetList
    End Function

    Private Function inArrayNetType(ByVal arr As NetworkInterfaceType(), ByVal match As NetworkInterfaceType) As Boolean
        For Each nettype As NetworkInterfaceType In arr
            If nettype = match Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function GetExtIpAddr() As String
        Try
            Dim ExternalIP As String
            ExternalIP = (New Net.WebClient()).DownloadString("http://checkip.dyndns.org/")
            ExternalIP = (New Regex("\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")) _
                         .Matches(ExternalIP)(0).ToString()

            Return ExternalIP
        Catch
            Return "No Connection"
        End Try
    End Function


End Class
