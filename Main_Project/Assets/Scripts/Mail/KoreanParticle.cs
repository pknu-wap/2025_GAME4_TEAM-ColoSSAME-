public enum Particle { EulReul, GwaWa }   // 을/를, 와/과

public static class KoreanParticle
{
    public static string Get(string name, Particle p)
    {
        bool hasBatchim = false;
        if (!string.IsNullOrEmpty(name))
        {
            char last = name[name.Length - 1];
            if (last >= 0xAC00 && last <= 0xD7A3)
                hasBatchim = (last - 0xAC00) % 28 != 0;   // 받침 있음
        }
        return p switch
        {
            Particle.EulReul => hasBatchim ? "을" : "를",
            Particle.GwaWa => hasBatchim ? "과" : "와",
            _ => ""
        };
    }
}
