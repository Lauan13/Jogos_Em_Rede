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
            // Vincula funções do script para reagir ao status real da rede do Unity
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
            // Desliga conexões fantasmas para não dar erro de "Can't start while listening"
            LimparConexoesAtivas();

            ConfigurarIP("127.0.0.1");

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartHost())
            {
                EsconderMenuInicial();
                Debug.Log("[Rede] Host iniciado no endereço local: 127.0.0.1");
            }
        }

        public void StartClient()
        {
            LimparConexoesAtivas();

            string ipDigitado = campoIP != null ? campoIP.text : "";
            ipDigitado = ipDigitado.Replace("\u200b", "").Trim();

            if (string.IsNullOrWhiteSpace(ipDigitado))
            {
                ipDigitado = "127.0.0.1";
            }

            Debug.Log($"[Cliente] Carregando interface e tentando conectar no IP: '{ipDigitado}'...");
            ConfigurarIP(ipDigitado);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartClient();
                // O Menu NÃO é escondido aqui. Esperamos o evento 'AoConectarSucesso' dar o sinal verde!
            }
        }

        public void StartServer()
        {
            LimparConexoesAtivas();

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
                Debug.Log($"[Rede] IP de conexão alterado dinamicamente para: {enderecoIP}");
            }
        }

        private void LimparConexoesAtivas()
        {
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
            {
                Debug.Log("[Rede] Resetando instâncias anteriores do Netcode.");
                NetworkManager.Singleton.Shutdown();
            }
        }

        // Resposta automática do Netcode quando a conexão realmente funciona
        private void AoConectarSucesso(ulong clientId)
        {
            if (NetworkManager.Singleton.IsClient && clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("[Rede] Conexão estabelecida com o servidor! Escondendo menu.");
                EsconderMenuInicial();
            }
        }

        // Resposta automática do Netcode se a conexão falhar por Timeout ou cair
        private void AoDesconectarOuFalhar(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.LogWarning("[Rede] Falha crítica de conexão. Retornando ao Menu Inicial.");
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