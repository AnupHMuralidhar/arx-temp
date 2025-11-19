using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class UDPDiscoveryClient : MonoBehaviour
{
    public int discoveryPort = 37020;
    public string discoveryMessage = "ARX_DISCOVERY";
    public string responseMessage = "ARX_BACKEND_RESPONSE";

    UdpClient udpClient;

    async void Start()
    {
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;

        await BroadcastAndScan();
    }

    private async Task BroadcastAndScan()
    {
        var messageBytes = Encoding.UTF8.GetBytes(discoveryMessage);

        // 1️⃣ Send to global broadcast
        var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
        udpClient.Send(messageBytes, messageBytes.Length, broadcastEndpoint);
        Debug.Log("UDP broadcast sent to 255.255.255.255");

        // 2️⃣ Send to subnet broadcast (for hotspot)
        foreach (var subnetBroadcast in GetSubnetBroadcasts())
        {
            var endpoint = new IPEndPoint(subnetBroadcast, discoveryPort);
            udpClient.Send(messageBytes, messageBytes.Length, endpoint);
            Debug.Log($"UDP broadcast sent to {endpoint.Address}");
        }

        // 3️⃣ Wait for response
        var listenEndpoint = new IPEndPoint(IPAddress.Any, 0);
        var startTime = DateTime.Now;

        udpClient.Client.ReceiveTimeout = 3000; // 3s timeout

        try
        {
            while ((DateTime.Now - startTime).TotalMilliseconds < 5000)
            {
                if (udpClient.Available > 0)
                {
                    var receivedBytes = udpClient.Receive(ref listenEndpoint);
                    var response = Encoding.UTF8.GetString(receivedBytes);

                    if (response == responseMessage)
                    {
                        Debug.Log($"✅ Received backend response from {listenEndpoint.Address}");
                        ConnectWebSocket(listenEndpoint.Address.ToString());
                        return;
                    }
                }
                await Task.Delay(200);
            }

            Debug.LogWarning("⚠️ No backend found via UDP. Please enter IP manually.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("UDP Receive exception: " + ex.Message);
        }
    }

    private void ConnectWebSocket(string backendIP)
    {
        var wsClient = FindFirstObjectByType<WebSocketClient>();
        if (wsClient != null)
            wsClient.Connect(backendIP);
        else
            Debug.LogWarning("⚠️ WebSocketClient not found in scene!");
    }

    private List<IPAddress> GetSubnetBroadcasts()
    {
        var broadcasts = new List<IPAddress>();
        var hostEntry = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in hostEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var parts = ip.ToString().Split('.');
                if (parts.Length == 4)
                {
                    string subnet = $"{parts[0]}.{parts[1]}.{parts[2]}.255";
                    if (IPAddress.TryParse(subnet, out var broadcastIP))
                        broadcasts.Add(broadcastIP);
                }
            }
        }

        return broadcasts;
    }

    private void OnDestroy()
    {
        udpClient?.Close();
    }
}
