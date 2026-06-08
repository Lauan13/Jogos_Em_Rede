using UnityEngine;
using System.Collections.Generic;

namespace JogosEmRede
{
    /// <summary>
    /// Gera e controla a grade 7x7 de blocos de gelo, controla turnos e verifica estabilidade.
    /// Usa BFS para determinar quais blocos estão conectados de forma estável.
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

            // Dispara a checagem correta de estabilidade
            ChecarEstabilidade();

            if (grade[centerX, centerY] == null)
            {
                Debug.Log($"[Game Over] Bloco central caiu! Jogador {turnoAtual} PERDEU!");
            }
        }

        /// <summary>
        /// Nova lógica baseada no brinquedo real: Se o bloco perder conexão com a estrutura 
        /// conectada ao bloco central (do pinguim), ele cai!
        /// </summary>
        public void ChecarEstabilidade()
        {
            if (emVerificacao)
                return;

            emVerificacao = true;

            try
            {
                // Se o bloco central já foi destruído, o jogo acabou, não precisa checar
                if (grade[centerX, centerY] == null)
                    return;

                // Matriz para registrar quem está conectado à estrutura estável do pinguim
                bool[,] conectadoAoCentro = new bool[colunas, linhas];
                
                // Fila para realizar a busca em largura (BFS) a partir do centro
                Queue<Vector2Int> fila = new Queue<Vector2Int>();

                // Começamos a varredura a partir do bloco central do pinguim
                Vector2Int centro = new Vector2Int(centerX, centerY);
                fila.Enqueue(centro);
                conectadoAoCentro[centerX, centerY] = true;

                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                // Passo 1: Descobre todos os blocos que ainda têm ligação física com o centro
                while (fila.Count > 0)
                {
                    Vector2Int atual = fila.Dequeue();

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = atual.x + dx[i];
                        int ny = atual.y + dy[i];

                        if (nx >= 0 && nx < colunas && ny >= 0 && ny < linhas)
                        {
                            // Se existe um bloco ali e ele ainda não foi mapeado
                            if (grade[nx, ny] != null && !conectadoAoCentro[nx, ny])
                            {
                                conectadoAoCentro[nx, ny] = true;
                                fila.Enqueue(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }

                // Passo 2: Qualquer bloco que ficou de fora da lista de conexões do centro cai!
                List<Vector2Int> blocosParaDestruir = new List<Vector2Int>();

                for (int x = 0; x < colunas; x++)
                {
                    for (int y = 0; y < linhas; y++)
                    {
                        // Se existe o bloco, mas ele não tem conexão com a estrutura do pinguim
                        if (grade[x, y] != null && !conectadoAoCentro[x, y])
                        {
                            blocosParaDestruir.Add(new Vector2Int(x, y));
                        }
                    }
                }

                // Passo 3: Destruição dos blocos órfãos (ilhas isoladas)
                if (blocosParaDestruir.Count > 0)
                {
                    Debug.Log($"[Física Real] Destruindo {blocosParaDestruir.Count} blocos que ficaram flutuando isolados.");
                    foreach (var coord in blocosParaDestruir)
                    {
                        GameObject go = grade[coord.x, coord.y];
                        grade[coord.x, coord.y] = null;

                        if (go != null)
                        {
                            Destroy(go);
                        }
                    }
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