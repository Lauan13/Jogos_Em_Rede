using UnityEngine;
using TMPro;

using System.Net.Sockets;
using System.Text;

public class SimpleClient : MonoBehaviour
{
    public TMP_Text logText;
    void Start()
    {
        SendHello();
    }
    void SendHello()
    {
        string ip =
            "127.0.0.1";
        TcpClient client =
            new TcpClient(
                ip,
                7777);
        NetworkStream stream =
            client.GetStream();
        string msg =
            "HELLO";
        byte[] data =
            Encoding.UTF8.GetBytes(msg);
        stream.Write(
            data,
            0,
            data.Length);
        logText.text =
            "Mensagem enviada:\n" +
            msg;
        stream.Close();
        client.Close();
    }
}