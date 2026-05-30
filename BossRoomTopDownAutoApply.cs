using System.Runtime.CompilerServices;

namespace DebugHeroFileDungeonRPG
{
    /// <summary>
    /// 이전 배포본에 포함됐던 BossRoomTopDownAutoApply.cs가 남아 있을 때를 대비한 호환용 안전 파일입니다.
    /// 실제 적용은 BossDungeonTopDownBackgroundPatch.cs가 2, 4, 6, 8, 10 보스 배경만 처리합니다.
    /// 이 파일은 의도적으로 아무 작업도 하지 않습니다.
    /// </summary>
    internal static class BossRoomTopDownAutoApply
    {
        [ModuleInitializer]
        internal static void ApplyTopDownBossRoomBackgrounds()
        {
            // Intentionally empty. 일반/잡몹 던전 배경을 건드리지 않기 위한 안전 장치입니다.
        }
    }
}
