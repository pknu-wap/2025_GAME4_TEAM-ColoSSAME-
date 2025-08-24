using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerManagerC : MonoBehaviour
{
    public PlayerObjC _prefabObj;
    public List<SPUM_Prefabs> _savedUnitList = new List<SPUM_Prefabs>();
    public Vector2 _startPos;
    public Vector2 _addPos;
    public int _columnNum;
    public int UnitMaxCount = 20;
    public Transform _playerPool;
    public List<PlayerObjC> _playerList = new List<PlayerObjC>();
    public PlayerObjC _nowObj; // ★ 통일
    public Transform _playerObjCircle;
    public Transform _goalObjCircle;
    public Camera _camera;
    Texture2D imageSave;
    public GameObject _bg;
    public RectTransform CommandPanel;
    public Button AnimationButton;
    public Transform AnimationPanelParent;
    public GameObject AnimationPanel;

    public enum ScreenShotSize { HD, FHD, UHD }
    public ScreenShotSize _screenShotSize = ScreenShotSize.HD;

    void Start()
    {
        if (_prefabObj == null || _playerPool == null)
        {
            Debug.LogError("[PlayerManagerC] _prefabObj 또는 _playerPool 이 비었습니다.");
            return;
        }

        if (_savedUnitList.Count.Equals(0) || _playerList.Count.Equals(0))
            GetPlayerList();
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                bool isHitPlayer = hit.collider.CompareTag("Player");
                if (CommandPanel != null) CommandPanel.gameObject.SetActive(isHitPlayer); // ★ 널 가드

                if (isHitPlayer)
                {
                    _nowObj = hit.collider.GetComponent<PlayerObjC>(); // ★ 통일
                    if (_nowObj != null)
                        CreateAnimationPanel(_nowObj);
                }
                else
                {
                    if (_nowObj != null && _goalObjCircle != null)
                    {
                        Vector2 goalPos = hit.point;
                        _goalObjCircle.transform.position = hit.point;
                        // _nowObj.SetMovePos(goalPos); // 필요 시 PlayerObjC에 구현
                    }
                }
            }
        }

        if (_nowObj != null && _playerObjCircle != null)
            _playerObjCircle.transform.position = _nowObj.transform.position;
    }

    public void CreateAnimationPanel(PlayerObjC Unit)
    {
        if (Unit == null || Unit._prefabs == null ||
            AnimationPanelParent == null || AnimationButton == null || AnimationPanel == null)
            return;

        foreach (Transform item in AnimationPanelParent.transform)
            Destroy(item.gameObject);

        var info = Unit._prefabs.StateAnimationPairs;
        foreach (var stateName in info.Keys)
        {
            var panel = Instantiate(AnimationPanel, AnimationPanelParent);
            string stateNameText = $"{stateName} State";
            panel.GetComponentInChildren<Text>().text = stateNameText;
            var parentT = panel.GetComponentInChildren<ContentSizeFitter>().transform;

            foreach (var clip in info[stateName])
            {
                var btn = Instantiate(AnimationButton, parentT);
                btn.GetComponentInChildren<Text>().text = clip.name;

                // ★ 클로저 안전: 지역 복사
                var selectedStateName = stateName;
                var selectedClip = clip;

                btn.onClick.AddListener(() =>
                {
                    if (Enum.TryParse(selectedStateName, true, out PlayerState state))
                    {
                        Unit.isAction = true;

                        // 인덱스 계산
                        int index = info[selectedStateName].FindIndex(x => x == selectedClip);
                        if (index < 0) index = 0;

                        Debug.Log($"{state} : {index} ({selectedClip.name})");

                        // AOC/애니메이터 재바인드는 여기서 해도 OK
                        if (Unit._prefabs != null && Unit._prefabs._anim != null)
                            Unit._prefabs._anim.Rebind();

                        Unit.SetStateAnimationIndex(state, index);
                        Unit.PlayStateAnimation(state);
                    }
                });
            }
        }
    }

    public void ClearPlayerList()
    {
        List<GameObject> tList = new List<GameObject>();
        for (var i = 0; i < _playerPool.transform.childCount; i++)
        {
            GameObject tOBjj = _playerPool.transform.GetChild(i).gameObject;
            tList.Add(tOBjj);
        }
        foreach (var obj in tList)
        {
            DestroyImmediate(obj);
        }

        _savedUnitList.Clear();
        _playerList.Clear();
    }

    public void GetPlayerList()
    {
        ClearPlayerList();

        var saveArray = Resources.LoadAll<SPUM_Prefabs>("");
        foreach (var unit in saveArray)
        {
            if (unit != null && unit.ImageElement != null && unit.ImageElement.Count > 0)
            {
                _savedUnitList.Add(unit);
                unit.PopulateAnimationLists();
            }
        }

        float numXStart = _startPos.x;
        float numYStart = _startPos.y;
        float numX = _addPos.x;
        float numY = _addPos.y;
        float ttV = 0;
        int sColumnNum = _columnNum;

        for (var i = 0; i < UnitMaxCount; i++)
        {
            if (i > _savedUnitList.Count - 1) continue;
            if (i > sColumnNum - 1)
            {
                numYStart -= 1f;
                numXStart -= numX * _columnNum;
                sColumnNum += _columnNum;
                ttV += numY;
            }

            GameObject root = Instantiate(_prefabObj.gameObject) as GameObject;
            root.transform.SetParent(_playerPool);
            root.transform.localScale = Vector3.one;

            var spum = Instantiate(_savedUnitList[i]);
            spum.transform.SetParent(root.transform);
            spum.transform.localScale = Vector3.one;
            spum.transform.localPosition = Vector3.zero;

            root.name = _savedUnitList[i].name;

            PlayerObjC poc = root.GetComponent<PlayerObjC>();
            if (poc == null)
            {
                Debug.LogError($"[PlayerManagerC] {_prefabObj.name} 프리팹에 PlayerObjC가 없습니다.");
                DestroyImmediate(root);
                continue;
            }

            // ★ 안전 초기화 (내부에서 AOC 중첩/무베이스 방지)
            poc.InitWithPrefabs(spum);

            root.transform.localPosition = new Vector3(numXStart + numX * i, numYStart + ttV, 0);
            _playerList.Add(poc);
        }
    }

    public void SetAlignUnits()
    {
        float numXStart = _startPos.x;
        float numYStart = _startPos.y;
        float numX = _addPos.x;
        float numY = _addPos.y;
        float ttV = 0;

        int sColumnNum = _columnNum;
        _playerList = _playerList.Where(s => s != null).ToList();
        for (var i = 0; i < _playerList.Count - 1; i++)
        {
            if (i > sColumnNum - 1)
            {
                numYStart -= 1f;
                numXStart -= numX * _columnNum;
                sColumnNum += _columnNum;
                ttV += numY;
            }

            GameObject ttObj = _playerList[i].gameObject;
            ttObj.transform.localPosition = new Vector3(numXStart + numX * i, numYStart + ttV, 0);
        }
    }

    // ─────────────────────────────────────────
    // 스크린샷: _playerPool 하위 유닛을 개별 캡처하여 저장
    // ─────────────────────────────────────────
    public void SetScreenShot()
    {
        if (_playerPool == null || _camera == null)
        {
            Debug.LogError("[SetScreenShot] _playerPool 또는 _camera가 비어 있습니다.");
            return;
        }

        // 배경 비활성화(있다면)
        if (_bg != null) _bg.SetActive(false);

        // 현재 해상도 백업
        Vector2 nowSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

        // 캡처 해상도 설정
        switch (_screenShotSize)
        {
            case ScreenShotSize.HD:  Screen.SetResolution(1280, 720, false);  break;
            case ScreenShotSize.FHD: Screen.SetResolution(1920, 1080, false); break;
            case ScreenShotSize.UHD: Screen.SetResolution(3840, 2160, false); break;
        }

        // 저장 폴더 보장
        string directory = Path.Combine(Application.dataPath, "SPUM", "ScreenShots");
        Directory.CreateDirectory(directory);

        // 캡처 대상: _playerPool 직속 자식(각 유닛 루트)
        var unitRoots = new List<Transform>();
        foreach (Transform child in _playerPool)
            unitRoots.Add(child);

        // 원복용 상태 저장
        var allRoots = new List<GameObject>();
        var activeStates = new Dictionary<GameObject, bool>();
        foreach (Transform t in _playerPool)
        {
            allRoots.Add(t.gameObject);
            activeStates[t.gameObject] = t.gameObject.activeSelf;
        }

        // 루프: 한 유닛만 켜고 렌더 → 저장
        foreach (Transform unit in unitRoots)
        {
            foreach (var go in allRoots) go.SetActive(false);
            unit.gameObject.SetActive(true);

            // 카메라 프레이밍
            Vector3 center = unit.position + new Vector3(0, 2f, 0);
            _camera.transform.position = new Vector3(center.x, center.y, _camera.transform.position.z);
            if (_camera.orthographic) _camera.orthographicSize = 2.5f;

            int tX = _camera.scaledPixelWidth;
            int tY = _camera.scaledPixelHeight;

            var tempRT = new RenderTexture(tX, tY, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            _camera.targetTexture = tempRT;
            RenderTexture.active = tempRT;
            _camera.Render();

            imageSave = new Texture2D(tX, tY, TextureFormat.ARGB32, false, true);
            imageSave.ReadPixels(new Rect(0, 0, tX, tY), 0, 0);
            imageSave.Apply();

            string fileName = $"{unit.name}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.png";
            string filePath = Path.Combine(directory, fileName);
            File.WriteAllBytes(filePath, imageSave.EncodeToPNG());
            Debug.Log($"✅ Screenshot saved: {filePath}");

            RenderTexture.active = null;
            _camera.targetTexture = null;
            DestroyImmediate(tempRT);
            DestroyImmediate(imageSave);
        }

        // 원래 활성 상태 복구
        foreach (var kv in activeStates)
            kv.Key.SetActive(kv.Value);

        // 해상도/배경 원복
        Screen.SetResolution((int)nowSize.x, (int)nowSize.y, false);
        if (_bg != null) _bg.SetActive(true);
    }
}
