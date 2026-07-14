using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace JogosEmRede
{
    public class GerenciadorDeConexao : MonoBehaviour
    {
        [Header("Referências da UI")]
        public TMP_InputField campoIP; // Arraste o seu InputField aqui no Inspector
        public GameObject painelMenuInicial; // Arraste o painel que contém os botões e o InputField para podermos escondê-lo ao iniciar

        private UnityTransport transporteDeRede;

        private void Start()
        {
            // Busca o componente de transporte de rede do NetworkManager
            if (NetworkManager.Singleton != null)
            {
                transporteDeRede = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }
        }

        /// <summary>
        /// Lê o IP do campo de texto e o configura como o endereço de destino no UnityTransport.
        /// </summary>
        private void ConfigurarEnderecoDeIP()
        {
            if (transporteDeRede == null)
            {
                Debug.LogError("[Rede] UnityTransport não encontrado no NetworkManager!");
                return;
            }

            // Se o campo de texto não estiver vazio, aplica o IP digitado
            if (campoIP != null && !string.IsNullOrEmpty(campoIP.text))
            {
                string ipDigitado = campoIP.text.Trim();
                transporteDeRede.SetConnectionData(ipDigitado, transporteDeRede.ConnectionData.Port);
                Debug.Log($"[Rede] Endereço de IP configurado para: {ipDigitado}");
            }
            else
            {
                // Se estiver vazio, usa o IP padrão configurado no Inspector (ex: 127.0.0.1)
                Debug.LogWarning("[Rede] Campo de IP vazio. Usando o IP padrão do NetworkManager.");
            }
        }

        // --- FUNÇÕES DOS BOTÕES DA UI ---

        public void ClicouEmHost()
        {
            // O Host roda localmente, então ele ouve em sua própria máquina. 
            // O IP configurado aqui define em qual interface de rede ele vai escutar (geralmente 0.0.0.0 ou IP local).
            ConfigurarEnderecoDeIP();
            
            if (NetworkManager.Singleton.StartHost())
            {
                EsconderMenuInicial();
            }
        }

        public void ClicouEmClient()
        {
            // O Cliente obrigatoriamente precisa do IP correto do Host para conectar!
            ConfigurarEnderecoDeIP();

            if (NetworkManager.Singleton.StartClient())
            {
                EsconderMenuInicial();
            }
        }

        public void ClicouEmServer()
        {
            ConfigurarEnderecoDeIP();

            if (NetworkManager.Singleton.StartServer())
            {
                EsconderMenuInicial();
            }
        }

        private void EsconderMenuInicial()
        {
            if (painelMenuInicial != null)
            {
                painelMenuInicial.SetActive(false);
            }
        }
    }
}