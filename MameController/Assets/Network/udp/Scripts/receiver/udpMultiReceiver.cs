using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class udpMultiReceiver : MonoBehaviour
{
    private UdpClient[] receivers = new UdpClient[4];
    private int[] ports = new int[4] { 5001, 5002, 5003, 5004 };

    // 각 포트별 클라이언트 등록 상태
    private string[] acceptedClient = new string[4];

    void Start()
    {
        StartReceivers();
    }

    void OnApplicationQuit()
    {
        CloseReceivers();
    }

    void StartReceivers()
    {
        for (int i = 0; i < 4; i++)
        {
            receivers[i] = new UdpClient(new IPEndPoint(IPAddress.Any, ports[i]));
            BeginReceive(i);
            Debug.Log($"Receiver {i + 1} Started on Port {ports[i]}");
        }
    }

    void BeginReceive(int index)
    {
        receivers[index].BeginReceive((ar) =>
        {
            UdpReceiveCallback(ar, index);
        }, null);
    }

    void UdpReceiveCallback(IAsyncResult ar, int index)
    {
        if (receivers[index] == null) return;

        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        byte[] data;

        try
        {
            data = receivers[index].EndReceive(ar, ref remote);
        }
        catch
        {
            return;
        }

        string senderKey = $"{remote.Address}:{remote.Port}";

        // 등록 없는 상태면 첫 접속자 등록
        if (acceptedClient[index] == null)
        {
            acceptedClient[index] = senderKey;
            Debug.Log($"Port {ports[index]} accepted client: {senderKey}");
        }
        else
        {
            // 등록된 클라이언트와 다르면 무시
            if (acceptedClient[index] != senderKey)
            {
                BeginReceive(index);
                return;
            }
        }

        // 등록된 클라이언트 메시지 처리
        string msg = Encoding.UTF8.GetString(data);
        ProcessMessage(index, msg);

        BeginReceive(index);
    }

    void ProcessMessage(int playerIndex, string msg)
    {
        /// 싱글턴 큐에 쌓아두고 메인 스레드에서 처리
        Debug.Log($"Player {playerIndex + 1} → {msg}");
    }

    // -------------------------------------------------------
    // 🔥 강제 리셋 기능: 포트별 클라이언트 초기화
    // -------------------------------------------------------
    public void ResetClientByPort(int portIndex)
    {
        if (portIndex < 0 || portIndex >= acceptedClient.Length)
            return;

        acceptedClient[portIndex] = null;
        Debug.Log($"Client on Port {ports[portIndex]} Reset!");
    }

    // UI 버튼에서 사용할 수 있도록 4개 메서드 제공
    public void ResetPlayer1() => ResetClientByPort(0);
    public void ResetPlayer2() => ResetClientByPort(1);
    public void ResetPlayer3() => ResetClientByPort(2);
    public void ResetPlayer4() => ResetClientByPort(3);
    // -------------------------------------------------------

    void CloseReceivers()
    {
        for (int i = 0; i < receivers.Length; i++)
        {
            receivers[i]?.Close();
            receivers[i] = null;
        }
    }
}
