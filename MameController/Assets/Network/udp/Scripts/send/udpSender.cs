using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class udpSender : MonoBehaviour
{
    public VariableJoystick variableJoystick;

    private UdpClient m_udpclient;
    string m_Ip = null;
    int m_Port = 0;
    //public ToServerPacket m_SendPacket = new ToServerPacket();
    //public ToClientPacket m_ReceivePacket = new ToClientPacket();
    string m_SendMessage;
    private byte[] m_SendBytes;
    private IPEndPoint m_RemoteIpEndPoint;

    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] TMP_InputField portInputField;

    [SerializeField] GameObject UdpConnectBtn;
    [SerializeField] GameObject UdpClosedBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Application.targetFrameRate = 60;
        //ConnectUdp();
    }

    void OnApplicationQuit()
    {
        CloseUdp();
    }
    
    public void SetUdpIP()
    {
        m_Ip = ipInputField.text;
        //Debug.Log($"UDP IP : {m_Ip}");
    }

    public void SetUdpPort()
    {   
        //m_Port = int.Parse(portInputField.text);
        //Debug.Log($"UDP Port : {m_Port}");
        if (!int.TryParse(portInputField.text, out m_Port))        
        {
            ToastNotification.Show("Invalid Port", 2f, "info");
        }
    }

    public void ConnectUdp()
    {
        SetUdpIP();
        SetUdpPort();
        ConnectUdp(m_Ip, m_Port);
        UdpConnectBtn.SetActive(false);
    }

    void ConnectUdp(string ip, int port)
    {
        if (string.IsNullOrEmpty(ip) || port <= 0)
        {
            //Debug.LogError("Invalid IP or Port");
            ToastNotification.Show("Invalid IP or Port", 2f, "info");
            UdpConnectBtn.SetActive(true);
            return;
        }
        CloseUdp();        
        m_udpclient = new UdpClient();
        //m_udpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        //m_udpclient.Connect(ip, port);
        m_RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        //Debug.Log($"Connected {ip} : {port}");

        // 수신 리스너도 동일한 포트(혹은 서버 응답용 포트)로 다시 시작
        if (udpInputManager.Instance != null)
        {
            udpInputManager.Instance.RestartListener(port);
        }

        ToastNotification.Show($"UDP {ip} : {port} Open!!", 2f, "success");
        UdpClosedBtn.SetActive(true);
    }

    void SetSendPacket(string msg)
    {
        if( m_udpclient == null )
        {
            //Debug.LogError("UdpClient is not initialized. Please connect first.");
            ToastNotification.Show("UdpClient is not initialized. Please connect first.", 2f, "error");
            return;
        }
        if (string.IsNullOrEmpty(msg))
        {
            //Debug.LogError("Message is null or empty");
            ToastNotification.Show("Message is null or empty", 2f, "error");
            return;
        }
        //Debug.Log($"SetSendPacket {msg}");
        //m_SendMessage = msg;
        m_SendBytes = Encoding.UTF8.GetBytes(msg);
        m_udpclient.Send(m_SendBytes, m_SendBytes.Length, m_RemoteIpEndPoint);
    }

    public void Send(string msg)
    {
        try
        {
            SetSendPacket(msg);
            //DoBeginSend(m_SendBytes);
        }

        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            return;
        }
    }

    void DoBeginSend(byte[] packets)
    {
        m_udpclient.BeginSend(packets, packets.Length, new AsyncCallback(SendCallback), m_udpclient);
    }

    void SendCallback(IAsyncResult ar)
    {
        UdpClient udpClient = (UdpClient)ar.AsyncState;
        //Debug.Log($"SendCallback {ar.ToString()}");
    }

    public void CloseUdp()
    {
        UdpClosedBtn.SetActive(false);
        if (m_udpclient!=null)
        {
            m_udpclient.Close();
            m_udpclient = null;
        }
        ToastNotification.Show("UDP Closed", 2f, "success");        
        UdpConnectBtn.SetActive(true);
    }

    private bool leftPressed;
    private bool rightPressed;
    private bool upPressed;
    private bool downPressed;

    private float lastBackPressedTime;
    private const float interval = 2f;

    private void Update()
    {
        float x = variableJoystick.Direction.x;
        float y = variableJoystick.Direction.y;

        // X Axis
        bool newLeft = x < -0.1f;
        bool newRight = x > 0.1f;

        if (newLeft != leftPressed)
        {
            Send(newLeft ? "p1_left_start" : "p1_left_end");
            leftPressed = newLeft;
        }

        if (newRight != rightPressed)
        {
            Send(newRight ? "p1_right_start" : "p1_right_end");
            rightPressed = newRight;
        }

        // Y Axis
        bool newDown = y < -0.1f;
        bool newUp = y > 0.1f;

        if (newDown != downPressed)
        {
            Send(newDown ? "p1_down_start" : "p1_down_end");
            downPressed = newDown;
        }

        if (newUp != upPressed)
        {
            Send(newUp ? "p1_up_start" : "p1_up_end");
            upPressed = newUp;
        }
        

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Time.time - lastBackPressedTime < interval)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            else
            {
                lastBackPressedTime = Time.time;
                ToastNotification.Show("Press again to exit.", 2f, "info");
            }
        }
    }
}
