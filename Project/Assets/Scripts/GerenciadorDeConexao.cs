using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP; // Importante para alterar o IP do UnityTransport
using TMPro; // Importante para ler o Campo de IP do TextMeshPro

public class GerenciadorDeConexao : MonoBehaviour
{
    [Header("Elementos de Interface")]
    [SerializeField] private TMP_InputField campoIP; // Arrastaremos o CampoIP aqui no Inspector
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

    public void StartHost()
    {
        // O Host sempre inicia localmente para abrir a sala na sua máquina
        ConfigurarIP("127.0.0.1"); 
        
        if (NetworkManager.Singleton.StartHost())
        {
            EsconderMenuInicial();
        }
    }

    public void StartClient()
    {
        // Lê o que o jogador digitou. Se estiver em branco, usa o IP padrão local (localhost)
        string ipDigitado = campoIP != null ? campoIP.text : "";
        if (string.IsNullOrWhiteSpace(ipDigitado))
        {
            ipDigitado = "127.0.0.1";
        }

        // Configura o IP de destino dinamicamente antes de conectar
        ConfigurarIP(ipDigitado.Trim());

        if (NetworkManager.Singleton.StartClient())
        {
            EsconderMenuInicial();
        }
    }

    public void StartServer()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            EsconderMenuInicial();
        }
    }

    private void ConfigurarIP(string enderecoIP)
    {
        if (transporteDeRede != null)
        {
            // Aplica o IP no componente de transporte de rede do Netcode
            transporteDeRede.ConnectionData.Address = enderecoIP;
            Debug.Log($"[Rede] IP de conexão alterado para: {enderecoIP}");
        }
    }

    private void EsconderMenuInicial()
    {
        // Esconde a interface de conexão para que o jogo apareça limpo por trás
        if (painelMenuInicial != null)
        {
            painelMenuInicial.SetActive(false);
        }
    }
}