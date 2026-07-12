using UnityEngine;
using Colosseum.Data;
using Colosseum.GamePlay.Gacha;

namespace Colosseum.Manager
{
    /// "뽑기 요청 → 결과 반환" 흐름만 담당한다.
    /// 재화 확인/차감 로직은 추후 작업에서 추가한다.
    public class GachaManager : MonoBehaviour
    {
        private GachaService _service;

        public event System.Action<GachaResult> OnGachaCompleted;

        private void Awake()
        {
            _service = new GachaService();
        }

        public void RequestRoll()
        {
            // TODO: 재화 확인/차감 로직(IWalletService 등) 추후 추가

            GachaResult result = _service.Roll();
            OnGachaCompleted?.Invoke(result);
        }
    }
}
