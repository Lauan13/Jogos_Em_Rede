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
            // O Host sempre inicia localmente para abrir a sala na sua máquina
            ConfigurarIP("127.0.0.1"); 
            
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartHost())
            {
                EsconderMenuInicial();
                Debug.Log("[Rede] Host iniciado no endereço local: 127.0.0.1");
            }
        }

        /// <summary>
        /// Chamado pelo botão "Entrar (Client)"
        /// </summary>
        public void StartClient()
        {
            // Lê o que o jogador digitou no campo do TextMeshPro
            string ipDigitado = campoIP != null ? campoIP.text : "";

            // REMOVE O VILÃO: Remove o caractere invisível de controle do TextMeshPro (\u200b) e os espaços extras
            ipDigitado = ipDigitado.Replace("\u200b", "").Trim();

            // Se o campo estiver totalmente em branco, usa o IP padrão local (localhost)
            if (string.IsNullOrWhiteSpace(ipDigitado))
            {
                ipDigitado = "127.0.0.1";
            }

            Debug.Log($"[Cliente] Tentando conectar no IP: '{ipDigitado}'");

            // Configura o IP de destino dinamicamente antes de conectar
            ConfigurarIP(ipDigitado);

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartClient())
            {
                EsconderMenuInicial();
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