using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
public class SimpleServer : MonoBehaviour
{
    public TMP_Text logText;
    TcpListener server;
    Thread serverThread;
    Queue<string> messages =
        new Queue<string>();
    void Start()
    {
        AddMessage("Servidor iniciado");
        serverThread =
            new Thread(Listen);
        serverThread.Start();
    }

    void Listen()
    {
        server =
            new TcpListener(
                IPAddress.Any,
                7777);
        server.Start();
        AddMessage(
            "Escutando porta 7777");
        while(true)
        {
            TcpClient client =
                server.AcceptTcpClient();
            AddMessage(
                "Cliente conectado");
            NetworkStream stream =
                client.GetStream();
            byte[] buffer =
                new byte[1024];
            int size =
                stream.Read(
                    buffer,
                    0,
                    buffer.Length);
            string msg =
                Encoding.UTF8.GetString(
                    buffer,
                    0,
                    size);
            AddMessage(
                "Recebido: " + msg);
            client.Close();
        }
    }

    void AddMessage(string msg)
    {
        lock(messages)
        {
            messages.Enqueue(msg);
        }
    }
    void Update()
    {
        lock(messages)
        {
            while(messages.Count > 0)
            {
                logText.text +=
                    "\n" +
                    messages.Dequeue();
            }
        }
    }
}