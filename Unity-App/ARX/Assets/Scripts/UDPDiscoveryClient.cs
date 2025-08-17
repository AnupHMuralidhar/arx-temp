using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPDiscoveryClient : MonoBehaviour
{
    public int discoveryPort = 37020;
    public string discoveryMessage = "ARX_DISCOVERY";
    public string responseMessage = "ARX_BACKEND_RESPONSE";

    UdpClient udpClient;

    void Start()
    {
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;

        BroadcastDiscoveryMessage();
    }

    async void BroadcastDiscoveryMessage()
    {
        var messageBytes = Encoding.UTF8.GetBytes(discoveryMessage);
        var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);

        // Send discovery message multiple times to increase chance of backend receiving it
        for (int i = 0; i < 5; i++)
        {
            udpClient.Send(messageBytes, messageBytes.Length, broadcastEndpoint);
            Debug.Log("UDP Discovery broadcast sent.");
            await Task.Delay(1000);
        }

        // Listen for response with timeout
        var listenEndpoint = new IPEndPoint(IPAddress.Any, 0);
        var startTime = DateTime.Now;

        udpClient.Client.ReceiveTimeout = 3000; // 3 seconds timeout

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
                        Debug.Log($"Received backend response from {listenEndpoint.Address}");
                        // You now have backend IP here: listenEndpoint.Address
                        // TODO: Connect WebSocket client to this IP next
                        break;
                    }
                }
                await Task.Delay(200);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("UDP Receive exception: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        udpClient?.Close();
    }
}
