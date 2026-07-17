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
        public NetworkVariable<int> turnoAtual = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public System.Action<int> OnTurnoAlterado;
        public System.Action<int> OnGameOver;

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

            // Inscreve a UI local na mudança do turno que vem da rede
            turnoAtual.OnValueChanged += (oldVal, newVal) => OnTurnoAlterado?.Invoke(newVal);

            if (IsServer)
            {
                GerarTabuleiroNoServidor();
                OnTurnoAlterado?.Invoke(turnoAtual.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (turnoAtual != null)
            {
                turnoAtual.OnValueChanged -= (oldVal, newVal) => OnTurnoAlterado?.Invoke(newVal);
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
            Debug.Log($"[Turno] Agora é a vez do Jogador {turnoAtual.Value}");
        }

        public GameObject GetBlock(int x, int y)
        {
            if (x < 0 || x >= largura || y < 0 || y >= altura) return null;
            return grade[x, y];
        }

        public void ReportBlockDestroyed(int x, int y)
        {
            if (!IsServer) return;

            if (x < 0 || x >= largura || y < 0 || y >= altura) return;

            grade[x, y] = null;

            // Roda o algoritmo de estabilidade customizado com reavaliação em cascata
            ChecarEstabilidade();

            // Se o bloco central (3,3) sumiu, o pinguim cai e o jogo acaba!
            int cx = largura / 2;
            int cy = altura / 2;
            if (grade[cx, cy] == null)
            {
                Pinguim pinguim = Object.FindFirstObjectByType<Pinguim>();
                if (pinguim != null)
                {
                    pinguim.Desabar(); 
                }

                Debug.Log($"[Fim de Jogo] O bloco central caiu! Jogador {turnoAtual.Value} PERDEU!");
                FinalizarJogo(turnoAtual.Value == 1 ? 2 : 1); // Vitória concedida ao oponente
            }
        }

        public void ChecarEstabilidade()
        {
            if (!IsServer) return;

            int centroX = largura / 2;
            int centroY = altura / 2;

            bool mudou = true;

            // Rodamos em loop enquanto houver desabamentos, para tratar cascatas perfeitamente
            while (mudou)
            {
                mudou = false;

                // 1. Descobrir quem ainda está conectado às bordas externas via Flood Fill
                bool[,] conectadoBorda = EncontrarConectadosBorda();

                // 2. Verificar se o bloco central (Pinguim) perdeu a conexão de suporte
                bool centroEstaConectado = (grade[centroX, centroY] != null && conectadoBorda[centroX, centroY]);

                List<Vector2Int> blocosParaDestruir = new List<Vector2Int>();

                for (int x = 0; x < largura; x++)
                {
                    for (int y = 0; y < altura; y++)
                    {
                        if (grade[x, y] != null)
                        {
                            bool ehBorda = (x == 0 || x == largura - 1 || y == 0 || y == altura - 1);

                            // GAME OVER: Se o centro perdeu sustentação, todos os blocos internos (não-borda) desabam juntos na hora!
                            if (!centroEstaConectado && !ehBorda)
                            {
                                blocosParaDestruir.Add(new Vector2Int(x, y));
                                continue;
                            }

                            // REGRA 1: Isolamento Total (Sem rota até as bordas)
                            if (!conectadoBorda[x, y])
                            {
                                blocosParaDestruir.Add(new Vector2Int(x, y));
                                continue;
                            }

                            // REGRA 2: Ponta Solta Vertical (Dente Solitário)
                            // Verifica se há qualquer bloco acima dele na mesma coluna
                            bool alguemAcima = false;
                            for (int ny = y + 1; ny < altura; ny++)
                            {
                                if (grade[x, ny] != null)
                                {
                                    alguemAcima = true;
                                    break;
                                }
                            }

                            // Se for o topo da coluna (não tem ninguém acima), precisa ter vizinho na esquerda OU na direita
                            if (!alguemAcima)
                            {
                                bool temEsquerda = (x - 1 >= 0 && grade[x - 1, y] != null);
                                bool temDireita = (x + 1 < largura && grade[x + 1, y] != null);

                                if (!temEsquerda && !temDireita)
                                {
                                    blocosParaDestruir.Add(new Vector2Int(x, y));
                                }
                            }
                        }
                    }
                }

                // Se identificamos blocos instáveis nesta passada, nós os removemos e repetimos a checagem
                if (blocosParaDestruir.Count > 0)
                {
                    foreach (var pos in blocosParaDestruir)
                    {
                        GameObject go = grade[pos.x, pos.y];
                        grade[pos.x, pos.y] = null; // Limpa fisicamente da grade do Servidor

                        if (go != null)
                        {
                            NetworkObject netObj = go.GetComponent<NetworkObject>();
                            if (netObj != null && netObj.IsSpawned)
                            {
                                netObj.Despawn(true); // Faz sumir síncronamente na tela de todos os jogadores
                            }
                        }
                    }
                    mudou = true; 
                }
            }
        }

        private bool[,] EncontrarConectadosBorda()
        {
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

            return visitado;
        }

        public void FinalizarJogo(int jogadorVencedor)
        {
            if (!IsServer) return;
            FinalizarJogoClientRpc(jogadorVencedor);
        }

        [ClientRpc]
        private void FinalizarJogoClientRpc(int jogadorVencedor)
        {
            OnGameOver?.Invoke(jogadorVencedor);
        }
    }
}