using UnityEngine;
using System.Collections.Generic;

namespace JogosEmRede
{
    /// <summary>
    /// Controla a grade de blocos de gelo, turnos e a integridade estrutural do anel de gelo.
    /// Se o bloco central perder a conexão com as bordas externas (paredes), a estrutura desaba e o pinguim cai.
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
        // Evento disparado quando o jogo termina. O int é o número do jogador vencedor (1 ou 2).
        public event System.Action<int> OnGameOver;

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
        }

        /// <summary>
        /// Verifica se a estrutura do jogo desabou baseando-se nas conexões com as bordas externas estáveis.
        /// </summary>
        public void ChecarEstabilidade()
        {
            if (emVerificacao)
                return;

            emVerificacao = true;

            try
            {
                // Se o bloco central já sumiu, o jogo já acabou
                if (grade[centerX, centerY] == null)
                    return;

                // --- PASSO 1: FLOOD FILL A PARTIR DAS BORDAS EXTERNAS (PAREDES FIXAS) ---
                bool[,] conectadoAsBordas = new bool[colunas, linhas];
                Queue<Vector2Int> fila = new Queue<Vector2Int>();

                // Insere todos os blocos das extremidades (borda do tabuleiro real) que ainda existem
                for (int x = 0; x < colunas; x++)
                {
                    if (grade[x, 0] != null) { fila.Enqueue(new Vector2Int(x, 0)); conectadoAsBordas[x, 0] = true; }
                    if (grade[x, linhas - 1] != null) { fila.Enqueue(new Vector2Int(x, linhas - 1)); conectadoAsBordas[x, linhas - 1] = true; }
                }
                for (int y = 1; y < linhas - 1; y++)
                {
                    if (grade[0, y] != null) { fila.Enqueue(new Vector2Int(0, y)); conectadoAsBordas[0, y] = true; }
                    if (grade[colunas - 1, y] != null) { fila.Enqueue(new Vector2Int(colunas - 1, y)); conectadoAsBordas[colunas - 1, y] = true; }
                }

                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                // Propaga a estabilidade de fora para dentro
                while (fila.Count > 0)
                {
                    Vector2Int atual = fila.Dequeue();

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = atual.x + dx[i];
                        int ny = atual.y + dy[i];

                        if (nx >= 0 && nx < colunas && ny >= 0 && ny < linhas)
                        {
                            if (grade[nx, ny] != null && !conectadoAsBordas[nx, ny])
                            {
                                conectadoAsBordas[nx, ny] = true;
                                fila.Enqueue(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }

                // --- PASSO 2: SE O BLOCO CENTRAL NÃO SE CONECTA ÀS BORDAS, TUDO DESABA ---
                if (!conectadoAsBordas[centerX, centerY])
                {
                    int vencedor = (turnoAtual == 1) ? 2 : 1;
                    Debug.Log($"[Game Over] O anel de suporte foi rompido! O bloco central desabou! Jogador {turnoAtual} PERDEU! Jogador {vencedor} VENCEU.");

                    // --- PASSO 2 (alterado): Encontra o pinguim (pílula verde) e o remove da cena imediatamente ---
                    // Tenta primeiro pelo nome "Pinguim", se não achar tenta pela Tag "Player".
                    GameObject pinguimObj = GameObject.Find("Pinguim");
                    if (pinguimObj == null)
                    {
                        try
                        {
                            pinguimObj = GameObject.FindWithTag("Player");
                        }
                        catch (System.Exception)
                        {
                            // Se não existir a Tag, FindWithTag lança exceção; apenas ignora neste caso.
                            pinguimObj = null;
                        }
                    }

                    if (pinguimObj != null)
                    {
                        Destroy(pinguimObj);
                    }
                    else
                    {
                        Debug.LogWarning("[GeradorDeTabuleiro] Pinguim não encontrado para destruir (nome 'Pinguim' ou Tag 'Player').");
                    }

                    // Notifica a UI / gerenciador de jogo sobre o vencedor
                    OnGameOver?.Invoke(vencedor);

                    // Derruba o centro e todos os blocos órfãos que perderam sustentação externa
                    for (int x = 0; x < colunas; x++)
                    {
                        for (int y = 0; y < linhas; y++)
                        {
                            if (grade[x, y] != null && !conectadoAsBordas[x, y])
                            {
                                GameObject go = grade[x, y];
                                grade[x, y] = null;
                                Destroy(go);
                            }
                        }
                    }

                    return;
                }

                // --- PASSO 3: LIMPEZA DE BLOCOS ISOLADOS E PONTAS SOLTAS ---
                List<Vector2Int> blocosParaDestruir = new List<Vector2Int>();

                for (int x = 0; x < colunas; x++)
                {
                    for (int y = 0; y < linhas; y++)
                    {
                        if (grade[x, y] == null) continue;

                        // Se não tem conexão com a estrutura das bordas, cai
                        if (!conectadoAsBordas[x, y])
                        {
                            blocosParaDestruir.Add(new Vector2Int(x, y));
                            continue;
                        }

                        if (x == centerX && y == centerY) continue;

                        // Impede dentes solitários suspensos sem vizinhos laterais
                        bool topoLivre = (y == linhas - 1) || (grade[x, y + 1] == null);
                        if (topoLivre)
                        {
                            bool temVizinhoEsquerda = (x > 0) && (grade[x - 1, y] != null);
                            bool temVizinhoDireita = (x < colunas - 1) && (grade[x + 1, y] != null);

                            if (!temVizinhoEsquerda && !temVizinhoDireita)
                            {
                                blocosParaDestruir.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                }

                if (blocosParaDestruir.Count > 0)
                {
                    Debug.Log($"[Física] Removendo {blocosParaDestruir.Count} blocos sem sustentação mecânica.");
                    foreach (var coord in blocosParaDestruir)
                    {
                        GameObject go = grade[coord.x, coord.y];
                        grade[coord.x, coord.y] = null;
                        if (go != null) Destroy(go);
                    }

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