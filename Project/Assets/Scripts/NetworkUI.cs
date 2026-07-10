using UnityEngine;
using Unity.Netcode;

namespace JogosEmRede
{
    /// <summary>
    /// Pequeno helper para iniciar Host/Client/Server a partir da UI.
    /// Ligue os botões na cena para os métodos StartHost/StartClient/StartServer.
    /// </summary>
    public class NetworkUI : MonoBehaviour
    {
        public void StartHost()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                Debug.LogWarning("NetworkManager.Singleton é nulo. Adicione um NetworkManager à cena.");
            }
        }

        public void StartClient()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartClient();
            }
            else
            {
                Debug.LogWarning("NetworkManager.Singleton é nulo. Adicione um NetworkManager à cena.");
            }
        }

        public void StartServer()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartServer();
            }
            else
            {
                Debug.LogWarning("NetworkManager.Singleton é nulo. Adicione um NetworkManager à cena.");
            }
        }
    }
}

