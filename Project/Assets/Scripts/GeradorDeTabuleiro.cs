using UnityEngine;
using System.Collections.Generic;

namespace JogosEmRede
{
    /// <summary>
    /// Controla a grade de blocos de gelo, turnos e estabilidade física real.
    /// Remove ilhas flutuantes e blocos solitários sem suporte lateral.
    /// </summary>
    public class GeradorDeTabuleiro : MonoBehaviour
    {
        public static GeradorDeTabuleiro Instance;

        public GameObject blocoPrefab;

        private int colunas = 7;
        private int linhas = 7;

        public float espacamento = 1f;

        private GameObject[,] grade;

        public int turnoAtual = 1;

        private bool emVerificacao = false;

        private int centerX;
        private int centerY;

        public event System.Action<int> OnTurnoAlterado;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            grade = new GameObject[colunas, linhas];
            centerX = (colunas - 1) / 2; // 3
            centerY = (linhas - 1) / 2; // 3
        }

        void Start()
        {
            for (int x = 0; x < colunas; x++)
            {
                for (int y = 0; y < linhas; y++)
                {
                    float posX = (x - centerX) * espacamento;
                    float posY = (y - centerY) * espacamento;

                    Vector3 pos = new Vector3(posX, posY, 0f);
                    GameObject go = Instantiate(blocoPrefab, pos, Quaternion.identity, transform);
                    go.name = $"Bloco_{x}_{y}";

                    BlocoDeGelo bloco = go.GetComponent<BlocoDeGelo>();
                    if (bloco != null)
                    {
                        bloco.gridX = x;
                        bloco.gridY = y;

                        if (x == centerX && y == centerY)
                        {
                            bloco.protegido = true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("O prefab do bloco não tem o componente BlocoDeGelo.");
                    }

                    grade[x, y] = go;
                }
            }
        }

        public void AlternarTurno()
        {
            turnoAtual = (turnoAtual == 1) ? 2 : 1;
            Debug.Log($"[Turno] Agora é o turno do Jogador {turnoAtual}.");
            OnTurnoAlterado?.Invoke(turnoAtual);
        }

        public void ReportBlockDestroyed(int x, int y)
        {
            if (x < 0 || x >= colunas || y < 0 || y >= linhas)
            {
                Debug.LogWarning($"ReportBlockDestroyed chamado com coordenadas inválidas: ({x}, {y})");
                return;
            }

            if (grade[x, y] != null)
            {
                grade[x, y] = null;
                Debug.Log($"[Bloco Destruído] Bloco em ({x}, {y}) removido da grade.");
            }

            ChecarEstabilidade();

            if (grade[centerX, centerY] == null)
            {
                Debug.Log($"[Game Over] Bloco central caiu! Jogador {turnoAtual} PERDEU!");
            }
        }

        /// <summary>
        /// Valida a estabilidade baseada em conexões centrais e suporte lateral contra blocos solitários.
        /// </summary>
        public void ChecarEstabilidade()
        {
            if (emVerificacao)
                return;

            emVerificacao = true;

            try
            {
                if (grade[centerX, centerY] == null)
                    return;

                // --- PASSO 1: FLOOD FILL A PARTIR DO CENTRO ---
                bool[,] conectadoAoCentro = new bool[colunas, linhas];
                Queue<Vector2Int> fila = new Queue<Vector2Int>();

                Vector2Int centro = new Vector2Int(centerX, centerY);
                fila.Enqueue(centro);
                conectadoAoCentro[centerX, centerY] = true;

                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                while (fila.Count > 0)
                {
                    Vector2Int atual = fila.Dequeue();

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = atual.x + dx[i];
                        int ny = atual.y + dy[i];

                        if (nx >= 0 && nx < colunas && ny >= 0 && ny < linhas)
                        {
                            if (grade[nx, ny] != null && !conectadoAoCentro[nx, ny])
                            {
                                conectadoAoCentro[nx, ny] = true;
                                fila.Enqueue(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }

                // --- PASSO 2: FILTRAR BLOCOS SEM SUPORTE LATERAL (COLUNAS ISOLADAS) ---
                List<Vector2Int> blocosParaDestruir = new List<Vector2Int>();

                for (int x = 0; x < colunas; x++)
                {
                    for (int y = 0; y < linhas; y++)
                    {
                        if (grade[x, y] == null) continue;

                        // Se já falhou no teste de conexão com o centro, cai direto
                        if (!conectadoAoCentro[x, y])
                        {
                            blocosParaDestruir.Add(new Vector2Int(x, y));
                            continue;
                        }

                        // Não derruba o bloco central por regras de suporte lateral
                        if (x == centerX && y == centerY) continue;

                        // Checa se o bloco está exposto no topo (não há bloco imediatamente acima dele)
                        bool topoLivre = (y == linhas - 1) || (grade[x, y + 1] == null);

                        if (topoLivre)
                        {
                            // Verifica se possui vizinhos imediatos na esquerda ou direita
                            bool temVizinhoEsquerda = (x > 0) && (grade[x - 1, y] != null);
                            bool temVizinhoDireita = (x < colunas - 1) && (grade[x + 1, y] != null);

                            // Se for um bloco solitário saliente (sem vizinho na esquerda E na direita)
                            if (!temVizinhoEsquerda && !temVizinhoDireita)
                            {
                                blocosParaDestruir.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                }

                // --- PASSO 3: REMOÇÃO DOS BLOCOS INSTÁVEIS ---
                if (blocosParaDestruir.Count > 0)
                {
                    Debug.Log($"[Física Brinquedo] Derrubando {blocosParaDestruir.Count} blocos por instabilidade ou falta de suporte lateral.");
                    foreach (var coord in blocosParaDestruir)
                    {
                        GameObject go = grade[coord.x, coord.y];
                        grade[coord.x, coord.y] = null;

                        if (go != null)
                        {
                            Destroy(go);
                        }
                    }

                    // Executa uma nova checagem recursiva caso a queda desses blocos tenha gerado novas pontas soltas
                    emVerificacao = false;
                    ChecarEstabilidade();
                    return;
                }
            }
            finally
            {
                emVerificacao = false;
            }
        }

        public GameObject GetBlock(int x, int y)
        {
            if (x < 0 || x >= colunas || y < 0 || y >= linhas)
                return null;

            return grade[x, y];
        }

        public int GetColunas()
        {
            return colunas;
        }

        public int GetLinhas()
        {
            return linhas;
        }

        public Vector2Int GetCenterCoordinates()
        {
            return new Vector2Int(centerX, centerY);
        }
    }
}