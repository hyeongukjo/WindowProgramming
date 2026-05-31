using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace DebugHeroFileDungeonRPG
{
    /// <summary>
    /// 보스 맵 배경만 고급 탑다운 이미지로 적용하는 추가 전용 패치입니다.
    ///
    /// 절대 건드리지 않는 대상:
    /// - 캐릭터 이미지 / 캐릭터 코드
    /// - 일반 맵 배경 Assets\StageBg_XX.png
    /// - 잡몹 던전 구간
    /// - MainForm.cs, Renderer.cs, PlayerMovementSystem.cs, csproj
    ///
    /// 적용 대상:
    /// - 보스 스테이지 02, 04, 06, 08, 10의 보스방 배경만
    /// - 빌드 출력 폴더의 Assets\StageBg_XX_Boss.png 파일만 런타임 시작 시 교체
    ///
    /// 자연스러운 탑다운 이동 적용 방식:
    /// - 기존 Renderer는 stageBossPhase == true일 때만 StageBg_XX_Boss.png를 읽습니다.
    /// - 기존 Renderer.DrawStageBackground는 cameraX와 mapWidth 기준으로 배경을 스크롤합니다.
    /// - 이 패치의 BossTopDown_XX.png 이미지는 보스 맵 가로 폭에 맞춘 탑다운 배경이라
    ///   캐릭터 이동/카메라 이동 로직을 건드리지 않아도 자연스럽게 따라 움직입니다.
    ///
    /// 새 배경 복사를 잠시 막으려면 환경 변수 DEBUGHERO_DISABLE_TOPDOWN_BOSS_BG=1 을 설정하세요.
    /// 이미 출력 폴더에 복사된 배경을 원본으로 되돌리려면 Clean/Rebuild를 함께 실행하면 됩니다.
    /// </summary>
    internal static class BossDungeonTopDownBackgroundPatch
    {
        private const string DisableEnvNameA = "DEBUGHERO_DISABLE_TOPDOWN_BOSS_BG";
        private const string DisableEnvNameB = "DEBUGHERO_DISABLE_BOSS_TOPDOWN_BG";
        private static readonly int[] BossStages = new int[] { 2, 4, 6, 8, 10 };

        [ModuleInitializer]
        internal static void ApplyOnStartup()
        {
            try
            {
                if (Environment.GetEnvironmentVariable(DisableEnvNameA) == "1" ||
                    Environment.GetEnvironmentVariable(DisableEnvNameB) == "1")
                {
                    return;
                }

                ApplyBossMapBackgroundsOnly();
            }
            catch (Exception ex)
            {
                // 배경 적용 실패가 게임 실행 자체를 막지 않도록 안전하게 로그만 남깁니다.
                Debug.WriteLine("[BossDungeonTopDownBackgroundPatch] apply failed: " + ex.Message);
            }
        }

        public static void ApplyBossMapBackgroundsOnly()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string assetsDir = Path.Combine(baseDir, "Assets");

            if (!Directory.Exists(assetsDir))
            {
                Debug.WriteLine("[BossDungeonTopDownBackgroundPatch] Assets directory not found: " + assetsDir);
                return;
            }

            foreach (int stage in BossStages)
            {
                ApplySingleBossBackground(assetsDir, stage);
            }
        }

        private static void ApplySingleBossBackground(string assetsDir, int stage)
        {
            string sourcePath = Path.Combine(assetsDir, "BossTopDown_" + stage.ToString("00") + ".png");
            string targetPath = Path.Combine(assetsDir, "StageBg_" + stage.ToString("00") + "_Boss.png");

            try
            {
                if (!File.Exists(sourcePath))
                {
                    Debug.WriteLine("[BossDungeonTopDownBackgroundPatch] source missing: " + sourcePath);
                    return;
                }

                if (File.Exists(targetPath) && FilesAreSame(sourcePath, targetPath))
                    return;

                CopyBossBackground(sourcePath, targetPath);
                Debug.WriteLine("[BossDungeonTopDownBackgroundPatch] boss-only background applied: " + Path.GetFileName(targetPath));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[BossDungeonTopDownBackgroundPatch] stage " + stage.ToString("00") + " failed: " + ex.Message);
            }
        }

        private static void CopyBossBackground(string sourcePath, string targetPath)
        {
            string fullSource = Path.GetFullPath(sourcePath);
            string fullTarget = Path.GetFullPath(targetPath);

            if (string.Equals(fullSource, fullTarget, StringComparison.OrdinalIgnoreCase))
                return;

            string targetDir = Path.GetDirectoryName(fullTarget);
            if (!string.IsNullOrEmpty(targetDir))
                Directory.CreateDirectory(targetDir);

            if (File.Exists(fullTarget))
            {
                FileAttributes attributes = File.GetAttributes(fullTarget);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    File.SetAttributes(fullTarget, attributes & ~FileAttributes.ReadOnly);
            }

            File.Copy(fullSource, fullTarget, true);
        }

        private static bool FilesAreSame(string firstPath, string secondPath)
        {
            FileInfo firstInfo = new FileInfo(firstPath);
            FileInfo secondInfo = new FileInfo(secondPath);

            if (!secondInfo.Exists || firstInfo.Length != secondInfo.Length)
                return false;

            const int BufferSize = 64 * 1024;
            byte[] firstBuffer = new byte[BufferSize];
            byte[] secondBuffer = new byte[BufferSize];

            using (FileStream first = File.OpenRead(firstPath))
            using (FileStream second = File.OpenRead(secondPath))
            {
                while (true)
                {
                    int firstRead = first.Read(firstBuffer, 0, firstBuffer.Length);
                    int secondRead = second.Read(secondBuffer, 0, secondBuffer.Length);

                    if (firstRead != secondRead)
                        return false;
                    if (firstRead == 0)
                        return true;

                    for (int i = 0; i < firstRead; i++)
                    {
                        if (firstBuffer[i] != secondBuffer[i])
                            return false;
                    }
                }
            }
        }
    }
}
