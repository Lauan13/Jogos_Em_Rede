using UnityEngine;
using Unity.Netcode;

public class GerenciadorDeConexao : MonoBehaviour
{
    // Limpamos quaisquer modificações experimentais e deixamos o script original pronto para receber as chamadas de conexão locais.
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
}