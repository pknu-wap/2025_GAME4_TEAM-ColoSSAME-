// AIManagerLocator.cs
using System.Collections.Generic;
using BattleK.Scripts.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AIManagerLocator
{
    /// <summary>
    /// 현재 활성 씬의 모든 루트에서 (비활성 포함) AI_Manager를 찾아 반환.
    /// 발견하지 못하면 null.
    /// </summary>
    public static AI_Manager FindInActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return null;

        List<GameObject> roots = new List<GameObject>(scene.rootCount);
        scene.GetRootGameObjects(roots);

        foreach (var root in roots)
        {
            if (root == null) continue;
            var found = root.GetComponentInChildren<AI_Manager>(true); // includeInactive:true
            if (found != null) return found;
        }

        return null;
    }
}