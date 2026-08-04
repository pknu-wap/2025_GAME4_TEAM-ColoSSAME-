using BattleK.Scripts.Data.Type;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class UnitPresentationSetup
    {
        private readonly string _playerLayerName;
        private readonly string _enemyLayerName;

        public UnitPresentationSetup(string playerLayerName, string enemyLayerName)
        {
            _playerLayerName = playerLayerName;
            _enemyLayerName = enemyLayerName;
        }

        public void Apply(GameObject go, UnitSpawnRequest req)
        {
            go.SetActive(false);

            var layerName = req.isPlayer ? _playerLayerName : _enemyLayerName;
            var layerIndex = LayerMask.NameToLayer(layerName);
            SetLayerRecursively(go, layerIndex);

            go.SetActive(true);

            var scale = go.transform.localScale;
            scale.x = req.faceLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            go.transform.localScale = scale;

            go.transform.localPosition = req.startPos;
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (!obj) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}