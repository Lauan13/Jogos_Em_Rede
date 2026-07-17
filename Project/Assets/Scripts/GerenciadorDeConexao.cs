using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

namespace JogosEmRede
{
    public class GerenciadorDeConexao : MonoBehaviour
    {
        [Header("Elementos de Interface")]
        [SerializeField] private TMP_InputField campoIP;
        [SerializeField] private GameObject painelMenuInicial;

        private UnityTransport transporteDeRede;

        private void Awake()
        {
            if (NetworkManager.Singleton != null)
            {
                transporteDeRede = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }
        }

        private void Start()
        {
            // Se inscreve nos eventos do Netcode para controlar o menu dinamicamente
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += AoConectarSucesso;
                NetworkManager.Singleton.OnClientDisconnectCallback += AoDesconectarOuFalhar;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= AoConectarSucesso;
                NetworkManager.Singleton.OnClientDisconnectCallback -= AoDesconectarOuFalhar;
            }
        }

        public void StartHost()
        {
            ConfigurarIP("127.0.0.1");

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartHost())
            {
                // O Host inicia instantaneamente, então podemos esconder o menu direto
                EsconderMenuInicial();
                Debug.Log("[Rede] Host iniciado no endereço local: 127.0.0.1");
            }
        }

        public void StartClient()
        {
            string ipDigitado = campoIP != null ? campoIP.text : "";
            ipDigitado = ipDigitado.Replace("\u200b", "").Trim();

            if (string.IsNullOrWhiteSpace(ipDigitado))
            {
                ipDigitado = "127.0.0.1";
            }

            Debug.Log($"[Cliente] Tentando conectar no IP: '{ipDigitado}'...");
            ConfigurarIP(ipDigitado);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartClient();
                // NOTA: Não escondemos o menu aqui! Vamos esperar o callback de sucesso.
            }
        }

        public void StartServer()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartServer())
            {
                EsconderMenuInicial();
            }
        }

        private void ConfigurarIP(string enderecoIP)
        {
            if (transporteDeRede != null)
            {
                transporteDeRede.ConnectionData.Address = enderecoIP;
                Debug.Log($"[Rede] IP de conexão alterado para: {enderecoIP}");
            }
        }

        // Chamado automaticamente quando a conexão com o servidor dá certo
        private void AoConectarSucesso(ulong clientId)
        {
            if (NetworkManager.Singleton.IsClient && clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("[Rede] Conectado ao servidor com sucesso!");
                EsconderMenuInicial();
            }
        }

        // Chamado se a conexão falhar (Timeout) ou se cair durante a partida
        private void AoDesconectarOuFalhar(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.LogWarning("[Rede] Falha na conexão ou desconectado do servidor.");
                MostrarMenuInicial();
            }
        }

        private void EsconderMenuInicial()
        {
            if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        }

        private void MostrarMenuInicial()
        {
            if (painelMenuInicial != null) painelMenuInicial.SetActive(true);
        }
    }
}