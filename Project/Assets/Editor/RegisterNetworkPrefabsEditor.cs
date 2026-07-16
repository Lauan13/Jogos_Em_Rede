using UnityEditor;
using UnityEngine;

public static class RegisterNetworkPrefabsEditor
{
    [MenuItem("Netcode/Prepare Pinguim Prefab (Add NetworkObject)")]
    public static void PreparePinguimPrefab()
    {
        var guids = AssetDatabase.FindAssets("Pinguim t:prefab");
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("Prefab 'Pinguim' não encontrado. Coloque-o em Assets/Prefabs ou nomeie como 'Pinguim'.");
            return;
        }

        string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null)
        {
            Debug.LogError($"Falha ao abrir o prefab em '{prefabPath}'.");
            return;
        }

        var type = System.Type.GetType("Unity.Netcode.NetworkObject, Unity.Netcode.Runtime") ?? System.Type.GetType("Unity.Netcode.NetworkObject");
        if (type == null)
        {
            Debug.LogWarning("Tipo NetworkObject não encontrado. Instale o pacote Netcode for GameObjects antes de executar este utilitário.");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        if (root.GetComponent(type) == null)
        {
            root.AddComponent(type);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log("Adicionado NetworkObject ao prefab Pinguim.");
        }
        else
        {
            Debug.Log("Prefab Pinguim já possui NetworkObject.");
        }

        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"Preparação concluída para prefab: {prefabPath}. Registre-o no NetworkManager (Inspector) se necessário.");
    }

    [MenuItem("Netcode/Create NetworkManager GameObject in Scene")]
    public static void CreateNetworkManagerInScene()
    {
        // Correção do aviso de obsolescência: alterado de FindObjectOfType para FindFirstObjectByType
        var existing = Object.FindFirstObjectByType(typeof(Unity.Netcode.NetworkManager));
        if (existing != null)
        {
            Debug.Log("Já existe um NetworkManager na cena.");
            return;
        }

        GameObject go = new GameObject("NetworkManager");

        var type = System.Type.GetType("Unity.Netcode.NetworkManager, Unity.Netcode.Runtime") ?? System.Type.GetType("Unity.Netcode.NetworkManager");
        if (type == null)
        {
            Debug.LogWarning("Tipo NetworkManager não encontrado. Instale o pacote Netcode for GameObjects antes de executar este utilitário.");
            return;
        }

        go.AddComponent(type);
        Selection.activeGameObject = go;
        Debug.Log("NetworkManager criado na cena. Configure o Transport (ex: Unity Transport) e registre os NetworkPrefabs via Inspector.");
    }
}
