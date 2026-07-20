using System;

namespace Game.Events
{
    /// <summary>
    /// Trung tam su kien dung chung cho toan bo game.
    /// Cac thanh vien khac (Player, Enemy) se "ban" (invoke) cac event nay
    /// khi co thay doi. UIManager va AudioManager chi lang nghe (subscribe).
    ///
    /// Ly do lam nhu vay: neu UIManager goi truc tiep vao script Player
    /// (vi du player.currentHealth), moi lan Member 1 doi cau truc script
    /// Player la code cua Tu bi gay. Voi cach nay, Player chi can goi
    /// GameEvents.RaiseHealthChanged(...) o dung cho, con lai UI tu lo.
    /// Giam conflict khi merge Git vi khong ai dung vao file cua ai.
    /// </summary>
    public static class GameEvents
    {
        // Gameplay bat dau
        public static event Action OnGameplayStart;
        public static void RaiseGameplayStart() => OnGameplayStart?.Invoke();

        // HP thay doi: current, max
        public static event Action<int, int> OnHealthChanged;
        public static void RaiseHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);

        // Score thay doi
        public static event Action<int> OnScoreChanged;
        public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);

        // Player chet
        public static event Action OnPlayerDied;
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();

        // Player thang (qua man cuoi / ha boss cuoi)
        public static event Action OnPlayerWin;
        public static void RaisePlayerWin() => OnPlayerWin?.Invoke();
    }
}
