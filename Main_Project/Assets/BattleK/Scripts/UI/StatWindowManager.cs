using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class StatWindowManager : MonoBehaviour
    {
        [Header("스탯 소스 (CalculateManager)")]
        public CalculateManager statsCollector;

        [Header("Player 상태창")]
        public List<StatWindow> PlayerStatWindows = new();

        [Header("Enemy 상태창")]
        public List<StatWindow> EnemyStatWindows = new();

        [Header("이미지 경로 설정")]
        [Tooltip("이미지 폴더 경로. 예: Assets/BattleK/Family/{CLAN}/Images/ 또는 Resources/Portraits")]
        public string imageDirectory = "Assets/BattleK/Family/{CLAN}/Images";
        [Tooltip("이미지 파일 확장자 (기본 .png)")]
        public string imageExtension = ".png";
        [Tooltip("유닛 이름 또는 Unit_ID 기준으로 이미지 파일명 매칭")]
        public bool useUnitNameInsteadOfID = false;

        [Header("팀 구분 기준")]
        public string playerLayerName = "Player";
        public string enemyLayerName  = "Enemy";

        [Header("디버그")]
        public bool debugLog = false;

        private void Awake()
        {
            if (statsCollector == null)
                statsCollector = FindObjectOfType<CalculateManager>(true);

            if ((PlayerStatWindows == null || PlayerStatWindows.Count == 0) ||
                (EnemyStatWindows  == null || EnemyStatWindows.Count  == 0))
            {
                AutoCollectWindows();
            }
        }

        [ContextMenu("Rebuild StatWindow Lists")]
        public void AutoCollectWindows()
        {
            PlayerStatWindows ??= new List<StatWindow>();
            EnemyStatWindows  ??= new List<StatWindow>();
            PlayerStatWindows.Clear();
            EnemyStatWindows.Clear();

            var all = FindObjectsOfType<StatWindow>(true);
            foreach (var w in all)
            {
                int layerIndex = GetSingleLayerIndex(w.TeamLayer);
                string layerName = LayerMask.LayerToName(layerIndex);

                if (layerName == playerLayerName)
                    PlayerStatWindows.Add(w);
                else if (layerName == enemyLayerName)
                    EnemyStatWindows.Add(w);
                else if (debugLog)
                    Debug.LogWarning($"[StatWindowManager] '{w.name}' 알 수 없는 팀 레이어({layerName}).");
            }

            if (debugLog)
                Debug.Log($"[StatWindowManager] Found Player={PlayerStatWindows.Count}, Enemy={EnemyStatWindows.Count}");
        }

        public void StatsUpdate()
        {
            StartCoroutine(StatsUpdateFlow());
        }

        private IEnumerator StatsUpdateFlow()
        {
            if (statsCollector == null)
            {
                statsCollector = FindObjectOfType<CalculateManager>(true);
                if (statsCollector == null)
                {
                    Debug.LogWarning("[StatWindowManager] CalculateManager를 찾을 수 없습니다.");
                    yield break;
                }
            }

            statsCollector.RefreshFromCollectorOnce();
            yield return null;

            var players = statsCollector.PlayerStats;
            var enemies = statsCollector.EnemyStats;

            FillTeam(PlayerStatWindows, players, "PLAYER");
            FillTeam(EnemyStatWindows, enemies, "ENEMY");
        }

        private void FillTeam(List<StatWindow> windows, IReadOnlyList<FamilyStatsCollector.CharacterStatsRow> rows, string tag)
        {
            if (windows == null || windows.Count == 0) return;
            if (rows == null) return;

            var used = new HashSet<int>();

            // 1) 정확 매칭 (UnitId 기반)
            for (int i = 0; i < windows.Count; i++)
            {
                var w = windows[i];
                if (w == null || string.IsNullOrWhiteSpace(w.UnitId)) continue;

                int idx = IndexOfUnitId(rows, w.UnitId);
                if (idx >= 0)
                {
                    used.Add(idx);
                    ApplyStatAndImage(w, rows[idx]);
                    continue;
                }
                else if (debugLog)
                    Debug.LogWarning($"[StatWindowManager] [{tag}] '{w.name}' UnitId={w.UnitId} 매칭 실패 → 인덱스 폴백 예정.");
            }

            // 2) 폴백 (순서 매칭)
            int r = 0;
            for (int i = 0; i < windows.Count; i++)
            {
                var w = windows[i];
                if (w == null) continue;

                if (!string.IsNullOrWhiteSpace(w.UnitId))
                {
                    int already = IndexOfUnitId(rows, w.UnitId);
                    if (already >= 0) continue;
                }

                while (r < rows.Count && used.Contains(r)) r++;
                if (r >= rows.Count) continue;

                ApplyStatAndImage(w, rows[r]);
                used.Add(r);
                r++;
            }
        }

        /// <summary>
        /// 스탯 텍스트 + 이미지 적용 통합 함수
        /// </summary>
        private void ApplyStatAndImage(StatWindow window, FamilyStatsCollector.CharacterStatsRow row)
        {
            if (window == null || row == null) return;

            // 기본 스탯 표시
            window.Apply(row);

            // 이미지 로드 시도
            string fileKey = useUnitNameInsteadOfID ? row.Unit_Name : row.Unit_ID;
            if (string.IsNullOrEmpty(fileKey))
            {
                if (debugLog) Debug.LogWarning($"[StatWindowManager] {row.Unit_ID} 이름/ID 없음 → 이미지 생략");
                return;
            }

            // clan 추출 ("Astra_Orion" → "Astra")
            string clan = ExtractClan(row.Unit_ID);
            string dir = imageDirectory.Replace("{CLAN}", clan);
            string absPath = ResolveToAbsolute(Path.Combine(dir, fileKey + imageExtension));

            if (!File.Exists(absPath))
            {
                if (debugLog)
                    Debug.LogWarning($"[StatWindowManager] 이미지 없음: {absPath}");
                return;
            }

            Texture2D tex = LoadImageAny(absPath);
            if (tex == null)
            {
                if (debugLog)
                    Debug.LogWarning($"[StatWindowManager] {fileKey} 이미지 로드 실패");
                return;
            }

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                          new Vector2(0.5f, 0.5f), 100f);
            if (window.CharacterImage != null)
                window.CharacterImage.sprite = sprite;

            if (debugLog)
                Debug.Log($"[StatWindowManager] {fileKey} 이미지 적용 완료 ({absPath})");
        }

        private static int IndexOfUnitId(IReadOnlyList<FamilyStatsCollector.CharacterStatsRow> rows, string unitId)
        {
            if (rows == null || string.IsNullOrWhiteSpace(unitId)) return -1;
            for (int i = 0; i < rows.Count; i++)
            {
                if (string.Equals(rows[i]?.Unit_ID, unitId, System.StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

        private string ExtractClan(string unitId)
        {
            if (string.IsNullOrWhiteSpace(unitId)) return "Unknown";
            int pos = unitId.IndexOf('_');
            return (pos > 0) ? unitId.Substring(0, pos) : "Unknown";
        }

        private static string ResolveToAbsolute(string input)
        {
            string path = input.Replace("\\", "/");
            if (Path.IsPathRooted(path)) return path;

            string assets = Application.dataPath.Replace("\\", "/");
            string root = Directory.GetParent(assets).FullName.Replace("\\", "/");
            if (path.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase))
                return Path.Combine(root, path).Replace("\\", "/");

            return Path.Combine(assets, path).Replace("\\", "/");
        }

        private Texture2D LoadImageAny(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                    return tex;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StatWindowManager] 이미지 로드 예외: {e.Message}");
            }
            return null;
        }

        private static int GetSingleLayerIndex(LayerMask mask)
        {
            int val = mask.value;
            if (val == 0) return 0;
            int idx = 0;
            while ((val & 1) == 0) { val >>= 1; idx++; }
            return idx;
        }
    }
}
