using System.Collections.Generic;
using UnityEngine;

public class udpInputManager : MonoBehaviour
{
    private static udpInputManager _instance;
    public static udpInputManager Instance => _instance;

    private udpListener _listener;

    private readonly Queue<udpInputMessage> _queue = new Queue<udpInputMessage>();
    private readonly object _lockObj = new object();

    private void Awake()
    {
        _instance = this;
    }

    public void RestartListener(int port)
    {
        _listener?.Stop();
        
        // 새로운 포트로 리스너 초기화 및 시작
        _listener = new udpListener(port, OnReceive);
        _listener.Start();
        Debug.Log($"UDP Listener started on port: {port}");
    }

    private void OnDestroy()
    {
        _listener?.Stop();
    }

    private void OnReceive(string raw)
    {
        var msg = udpInputMessage.Parse(raw);
        if (!msg.IsValid)
            return;

        lock (_lockObj)
            _queue.Enqueue(msg);
    }

    private void Update()
    {
        lock (_lockObj)
        {
            while (_queue.Count > 0)
            {
                var msg = _queue.Dequeue();
                ProcessInput(msg);
            }
        }
    }

    private void ProcessInput(udpInputMessage msg)
    {
        // 수신된 메시지를 화면에 출력
        ToastNotification.Show($": {msg.KeyCode}", 3f, "info");
        Debug.Log($"UDP Message Received: {msg.KeyCode}");
    }
}

