using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace JogosEmRede
{
    public class GeradorDeTabuleiro : NetworkBehaviour
    {
        public static GeradorDeTabuleiro Instance { get; private set; }

        [Header("Configurações do Tabuleiro")]
        public int largura = 7;
        public int altura = 7;
        public GameObject blocoPrefab; 
        public float espacamento = 1.1f; 

        [Header("Estado da Rede")]
        // Sincroniza qual jogador deve jogar (1 = Host, 2 = Client)
        public NetworkVariable<int> turnoAtual = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // O evento agora avisa apenas qual é o turno atual
        public System.Action<int> OnTurnoAlterado;
        public System.Action<int> OnGameOver;

        private Dictionary<Vector2Int, GameObject> matrizDeBlocos = new Dictionary<Vector2Int, GameObject>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                GerarTabuleiroNoServidor();
                // Avisa a UI sobre o turno inicial
                OnTurnoAlterado?.Invoke(turnoAtual.Value);
            }
        }

        private void GerarTabuleiroNoServidor()
        {
            int centroX = largura / 2;
            int centroY = altura / 2;
            Vector3 origem = new Vector3(-(largura - 1) * espacamento / 2f, -(altura - 1) * espacamento / 2f, 0);

            for (int x = 0; x < largura; x++)
            {
                for (int y = 0; y < altura; y++)
                {
                    Vector3 posicaoBloco = origem + new Vector3(x * espacamento, y * espacamento, 0);
                    GameObject novoBloco = Instantiate(blocoPrefab, posicaoBloco, Quaternion.identity);

                    BlocoDeGelo componenteBloco = novoBloco.GetComponent<BlocoDeGelo>();
                    if (componenteBloco != null)
                    {
                        componenteBloco.gridX = x;
                        componenteBloco.gridY = y;
                        if (x == centroX && y == centroY) componenteBloco.protegido = true;
                    }

                    NetworkObject netObj = novoBloco.GetComponent<NetworkObject>();
                    if (netObj != null) netObj.Spawn(true);

                    matrizDeBlocos[new Vector2Int(x, y)] = novoBloco;
                }
            }
        }

        public void AlternarTurno()
        {
            if (!IsServer) return;

            // Alterna de forma simples entre 1 e 2
            turnoAtual.Value = (turnoAtual.Value == 1) ? 2 : 1;

            // Dispara o evento apenas com o turno
            OnTurnoAlterado?.Invoke(turnoAtual.Value);

            Debug.Log($"[Turno] Agora é a vez do Jogador {turnoAtual.Value}");
        }

        public GameObject GetBlock(int x, int y)
        {
            Vector2Int chave = new Vector2Int(x, y);
            return matrizDeBlocos.ContainsKey(chave) ? matrizDeBlocos[chave] : null;
        }

        public void ReportBlockDestroyed(int x, int y)
        {
            if (!IsServer) return;

            Vector2Int chave = new Vector2Int(x, y);
            if (matrizDeBlocos.ContainsKey(chave))
            {
                GameObject blocoObj = matrizDeBlocos[chave];
                matrizDeBlocos.Remove(chave);

                if (blocoObj != null)
                {
                    NetworkObject netObj = blocoObj.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned) netObj.Despawn(true);
                }
            }
        }

        public void FinalizarJogo(int jogadorVencedor)
        {
            if (!IsServer) return;
            OnGameOver?.Invoke(jogadorVencedor);
        }
    }
}