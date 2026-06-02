using UnityEngine;
using System.Collections.Generic;

namespace JogosEmRede
{
    // Gera e controla a grade 7x7 de blocos de gelo, controla turnos e checa estabilidade (conexão às bordas).
    public class GeradorDeTabuleiro : MonoBehaviour
    {
    // Instância singleton para que os blocos possam avisar quando quebram.
    public static GeradorDeTabuleiro Instance;

    // Prefab do bloco de gelo (deve conter o componente BlocoDeGelo)
    public GameObject blocoPrefab;

    // Tamanho fixo da grade
    int colunas = 7;
    int linhas = 7;

    // Espaçamento entre blocos (configurável no Inspector)
    public float espacamento = 1f;

    // Matriz que guarda referências aos blocos instanciados
    private GameObject[,] grade;

    // Controle de turnos: 1 = Jogador 1, 2 = Jogador 2
    public int turnoAtual = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        grade = new GameObject[colunas, linhas];
    }

    void Start()
    {
        // Centraliza a grade em torno da origem (0,0)
        int centerX = (colunas - 1) / 2; // 3
        int centerY = (linhas - 1) / 2; // 3

        for (int x = 0; x < colunas; x++)
        {
            for (int y = 0; y < linhas; y++)
            {
                // Calcula posição para centralizar a grade
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

                    // Protege o bloco central (onde ficará o pinguim)
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

    // Alterna o turno entre os dois jogadores
    public void AlternarTurno()
    {
        turnoAtual = (turnoAtual == 1) ? 2 : 1;
        Debug.Log($"Turno alternado. Agora é o Jogador {turnoAtual}.");
    }

    // Chamado por um bloco quando ele se quebra (ou é removido).
    public void ReportBlockDestroyed(int x, int y)
    {
        if (x < 0 || x >= colunas || y < 0 || y >= linhas)
            return;

        // Remove referência ao bloco da grade
        grade[x, y] = null;

        // Verifica quais blocos ficam desconectados das bordas e os remove
        ChecarEstabilidade();

        // Se o bloco central (3,3) estiver ausente, o jogador do turno atual perdeu
        int cx = (colunas - 1) / 2;
        int cy = (linhas - 1) / 2;
        if (grade[cx, cy] == null)
        {
            Debug.Log($"Bloco central caiu. Jogador {turnoAtual} perdeu o jogo!");
        }
    }

    // Verifica a estabilidade da grade: remove blocos que não estão conectados (direta/indiretamente) às bordas
    public void ChecarEstabilidade()
    {
        bool[,] visitado = new bool[colunas, linhas];
        Queue<Vector2Int> fila = new Queue<Vector2Int>();

        // Enfileira todos os blocos das bordas que ainda existem
        for (int x = 0; x < colunas; x++)
        {
            for (int y = 0; y < linhas; y++)
            {
                if (x == 0 || x == colunas - 1 || y == 0 || y == linhas - 1)
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
                if (nx >= 0 && nx < colunas && ny >= 0 && ny < linhas)
                {
                    if (!visitado[nx, ny] && grade[nx, ny] != null)
                    {
                        visitado[nx, ny] = true;
                        fila.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        // Qualquer bloco não visitado deve cair (ser destruído)
        List<Vector2Int> paraRemover = new List<Vector2Int>();
        for (int x = 0; x < colunas; x++)
        {
            for (int y = 0; y < linhas; y++)
            {
                if (grade[x, y] != null && !visitado[x, y])
                {
                    paraRemover.Add(new Vector2Int(x, y));
                }
            }
        }

        foreach (var coord in paraRemover)
        {
            GameObject go = grade[coord.x, coord.y];
            grade[coord.x, coord.y] = null;

            if (go != null)
            {
                // Se necessário, podemos avisar antes de destruir (por exemplo, animar queda)
                BlocoDeGelo bloco = go.GetComponent<BlocoDeGelo>();
                if (bloco != null)
                {
                    int cx = (colunas - 1) / 2;
                    int cy = (linhas - 1) / 2;
                    if (bloco.gridX == cx && bloco.gridY == cy)
                    {
                        Debug.Log($"Bloco central caiu por instabilidade. Jogador {turnoAtual} perdeu!");
                    }
                }

                Destroy(go);
            }
        }
    }

    // Retorna o GameObject do bloco na posição (x,y) da grade, ou null se não existir.
    public GameObject GetBlock(int x, int y)
    {
        if (x < 0 || x >= colunas || y < 0 || y >= linhas) return null;
        return grade[x, y];
    }


}

}


