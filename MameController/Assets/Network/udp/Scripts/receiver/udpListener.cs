using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class udpListener
{
    private readonly int _port;
    private readonly Action<string> _onMessage;
    private UdpClient _client;
    private bool _running;
    private Thread _thread;

    public udpListener(int port, Action<string> onMessage)
    {
        _port = port;
        _onMessage = onMessage;
    }

    public void Start()
    {
        _running = true;
        _client = new UdpClient(_port);

        _thread = new Thread(Listen);
        _thread.IsBackground = true;
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
        _client?.Close();
        _thread?.Join();
    }

    private void Listen()
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);

        while (_running)
        {
            try
            {
                byte[] data = _client.Receive(ref ep);
                string msg = Encoding.UTF8.GetString(data);

                _onMessage?.Invoke(msg);
            }
            catch { }
        }
    }
}

