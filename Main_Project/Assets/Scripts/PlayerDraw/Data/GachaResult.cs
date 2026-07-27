namespace Colosseum.Data
{
    /// 1회 뽑기의 결과. 캐릭터 데이터 연동은 추후 추가
    public readonly struct GachaResult
    {
        public bool Success { get; }
        public GladiatorJob Job { get; }
        public int StarGrade { get; }
        public string FailReason { get; }

        private GachaResult(bool success, GladiatorJob job, int starGrade, string failReason)
        {
            Success = success;
            Job = job;
            StarGrade = starGrade;
            FailReason = failReason;비
        }

        public static GachaResult SucceedWith(GladiatorJob job, int starGrade)
            => new GachaResult(true, job, starGrade, null);

        public static GachaResult Fail(string reason)
            => new GachaResult(false, default, 0, reason);
    }
}
