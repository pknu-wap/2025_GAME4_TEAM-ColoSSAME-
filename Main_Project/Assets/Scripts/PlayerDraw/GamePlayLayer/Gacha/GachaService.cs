using Colosseum.Data;

namespace Colosseum.GamePlay.Gacha
{
    /// 1회 뽑기 절차(직업 결정 → 성급 결정 → 결과 생성)만 담당하는 순수 C# 클래스.
    /// 확률 테이블/캐릭터 데이터베이스 등 내부 데이터 연동은 추후 작업에서 추가한다.
    public class GachaService
    {
        public GachaResult Roll()
        {
            GladiatorJob job = DetermineJob();
            int starGrade = DetermineStarGrade();

            return GachaResult.SucceedWith(job, starGrade);
        }

        private GladiatorJob DetermineJob()
        {
            // TODO: 직업별 확률 데이터 연동
            // WeightedRandomSelector.Select(jobProbabilityTable.Entries, e => e.Weight)
            throw new System.NotImplementedException();
        }

        private int DetermineStarGrade()
        {
            // TODO: 성급별 확률 데이터 연동
            // WeightedRandomSelector.Select(starGradeProbabilityTable.Entries, e => e.Weight)
            throw new System.NotImplementedException();
        }
    }
}
