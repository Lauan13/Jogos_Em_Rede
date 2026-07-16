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

        public System.Action<int> OnTurnoAlterado;
        public System.Action<int> OnGameOver;

        // Usamos uma matriz bidimensional de GameObject para facilitar a lógica original do BFS
        private GameObject[,] grade;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            grade = new GameObject[largura, altura];
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                GerarTabuleiroNoServidor();
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

                    grade[x, y] = novoBloco;
                }
            }
        }

        public void AlternarTurno()
        {
            if (!IsServer) return;

            turnoAtual.Value = (turnoAtual.Value == 1) ? 2 : 1;
            OnTurnoAlterado?.Invoke(turnoAtual.Value);

            Debug.Log($"[Turno] Agora é a vez do Jogador {turnoAtual.Value}");
        }

        public GameObject GetBlock(int x, int y)
        {
            if (x < 0 || x >= largura || y < 0 || y >= altura) return null;
            return grade[x, y];
        }

        // Chamado no Servidor quando um bloco é totalmente destruído
        public void ReportBlockDestroyed(int x, int y)
        {
            if (!IsServer) return;

            if (x < 0 || x >= largura || y < 0 || y >= altura) return;

            // Remove a referência física da grade do servidor
            grade[x, y] = null;

            // Roda o seu algoritmo BFS original adaptado para rede
            ChecarEstabilidade();

            // Se o bloco central (3,3) sumiu, o jogo acaba!
            int cx = largura / 2;
            int cy = altura / 2;
            if (grade[cx, cy] == null)
            {
                Debug.Log($"[Fim de Jogo] O bloco central caiu! Jogador {turnoAtual.Value} PERDEU!");
                FinalizarJogo(turnoAtual.Value == 1 ? 2 : 1); // Vitória concedida ao oponente
            }
        }

        // O seu algoritmo original BFS, agora rodando de forma autoritativa no Servidor!
        public void ChecarEstabilidade()
        {
            if (!IsServer) return;

            bool[,] visitado = new bool[largura, altura];
            Queue<Vector2Int> fila = new Queue<Vector2Int>();

            // Enfileira todos os blocos das bordas que ainda existem
            for (int x = 0; x < largura; x++)
            {
                for (int y = 0; y < altura; y++)
                {
                    if (x == 0 || x == largura - 1 || y == 0 || y == altura - 1)
                    {
                        if (grade[x, y] != null)
                        {
                            visitado[x, y] = true;
                            fila.Enqueue(new Vector2Int(x, y));
                        }
                    }
                }
            }

            // BFS para marcar todos os blocos conectados às bordas
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (fila.Count > 0)
            {
                Vector2Int atual = fila.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = atual.x + dx[i];
                    int ny = atual.y + dy[i];
                    if (nx >= 0 && nx < largura && ny >= 0 && ny < altura)
                    {
                        if (!visitado[nx, ny] && grade[nx, ny] != null)
                        {
                            visitado[nx, ny] = true;
                            fila.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }

            // Qualquer bloco não visitado deve cair (ser despawnado da rede)
            for (int x = 0; x < largura; x++)
            {
                for (int y = 0; y < altura; y++)
                {
                    if (grade[x, y] != null && !visitado[x, y])
                    {
                        GameObject go = grade[x, y];
                        grade[x, y] = null; // Remove da matriz local

                        if (go != null)
                        {
                            NetworkObject netObj = go.GetComponent<NetworkObject>();
                            if (netObj != null && netObj.IsSpawned)
                            {
                                // Remove o bloco de todas as telas na rede de forma sincronizada!
                                netObj.Despawn(true); 
                            }
                        }
                    }
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