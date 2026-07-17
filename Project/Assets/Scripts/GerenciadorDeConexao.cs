using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP; // Importante para alterar o IP do UnityTransport
using TMPro; // Importante para ler o Campo de IP do TextMeshPro

namespace JogosEmRede
{
    public class GerenciadorDeConexao : MonoBehaviour
    {
        [Header("Elementos de Interface")]
        [SerializeField] private TMP_InputField campoIP; // Arraste o seu CampoIP aqui no Inspector
        [SerializeField] private GameObject painelMenuInicial; // O painel que vamos esconder ao conectar

        private UnityTransport transporteDeRede;

        private void Awake()
        {
            // Busca o componente de transporte de rede que está junto ao NetworkManager
            if (NetworkManager.Singleton != null)
            {
                transporteDeRede = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }
        }

        /// <summary>
        /// Chamado pelo botão "Criar Sala (Host)"
        /// </summary>
        public void StartHost()
        {
            // SE SEGURANÇA: Se a rede já estiver ativa por algum motivo, desliga antes de reiniciar
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
            {
                NetworkManager.Singleton.Shutdown();
            }

            ConfigurarIP("127.0.0.1");

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartHost())
            {
                EsconderMenuInicial();
                Debug.Log("[Rede] Host iniciado no endereço local: 127.0.0.1");
            }
        }

        public void StartClient()
        {
            // SE SEGURANÇA: Se o jogo já estiver tentando escutar ou conectado, desliga para não dar o erro "Can't start while listening"
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
            {
                Debug.Log("[Rede] Reiniciando conexões anteriores ativas antes de tentar como Cliente...");
                NetworkManager.Singleton.Shutdown();
            }

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
            }
        }

        /// <summary>
        /// Chamado pelo botão de Server, caso queira rodar um servidor dedicado dedicado
        /// </summary>
        public void StartServer()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartServer())
            {
                EsconderMenuInicial();
            }
        }

        /// <summary>
        /// Modifica o endereço de IP do componente de transporte do Netcode
        /// </summary>
        private void ConfigurarIP(string enderecoIP)
        {
            if (transporteDeRede != null)
            {
                // Aplica o IP de forma dinâmica no componente de rede
                transporteDeRede.ConnectionData.Address = enderecoIP;
                Debug.Log($"[Rede] IP de conexão alterado com sucesso para: {enderecoIP}");
            }
            else
            {
                // Caso não tenha encontrado no Awake, tenta buscar novamente por segurança
                if (NetworkManager.Singleton != null)
                {
                    transporteDeRede = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    if (transporteDeRede != null)
                    {
                        transporteDeRede.ConnectionData.Address = enderecoIP;
                        Debug.Log($"[Rede] IP de conexão alterado (recuperado) para: {enderecoIP}");
                        return;
                    }
                }
                Debug.LogError("[Rede] Erro crítico: UnityTransport não foi encontrado no NetworkManager!");
            }
        }

        private void EsconderMenuInicial()
        {
            // Esconde a interface do menu para que o tabuleiro gerado apareça limpo por trás
            if (painelMenuInicial != null)
            {
                painelMenuInicial.SetActive(false);
            }
        }
    }
}