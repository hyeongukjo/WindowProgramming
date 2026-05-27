using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    public static class Renderer
    {
        private static readonly Dictionary<NpcMood, Image> npcCache = new Dictionary<NpcMood, Image>();
        private static Image npcEmotionSheetCache = null;
        private static Image xpBlissBackgroundCache = null;
        private static Bitmap xpBlissScaledCache = null;
        private static Size xpBlissScaledSize = Size.Empty;
        private static readonly Dictionary<string, Bitmap> stageBackgroundCache = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<string, Image> stageImageCache = new Dictionary<string, Image>();
        private static Image playerAgentSheet = null;
        private static Image playerAgentIdleFrame = null;
        private static readonly Image[] playerAgentWalkFrames = new Image[8];
        private static System.Drawing.Text.PrivateFontCollection fontCollection;
        private static FontFamily customFontFamily;
        public static Image Img_AlarmBg = null;
        public static Image Img_PopupBtn = null;

        // 보스 이미지
        public static Image ImgBoss_DriverK;
        public static Image ImgBoss_BSOD;
        public static Image ImgBoss_HighKernel;
        public static Image ImgBoss_ExceptionQueen;
        public static Image ImgBoss_IllegalBinny;
        private static readonly Dictionary<string, Image> playerSpriteSheets = new Dictionary<string, Image>();
        private static readonly Dictionary<string, Image> playerActionSheetCache = new Dictionary<string, Image>();
        private static readonly Dictionary<string, Image> playerMotionSheetCache = new Dictionary<string, Image>();
        private static readonly Dictionary<string, Rectangle> playerActionTrimCache = new Dictionary<string, Rectangle>();
        private static Image playerStillSwordImage;
        private static Image playerShieldImage;
        private static Image normalMonsterSheet = null;
        // 몬스터 이미지 호출 관련 코드 //
        private static readonly Dictionary<string, Image> strictMonsterCache = new Dictionary<string, Image>();
        private static readonly object strictMonsterLock = new object();


        // 보스 공격 이펙트
        public static Image Img_DiskSprite;
        public static Image Img_Meteor;
        public static Image Img_Meteor2;
        public static Image Img_Safezone;
        public static Image Img_IceSword;
        public static Image Img_FireSword;
        public static Image Img_LightningSword;

        static Renderer()
        {
            try
            {
                fontCollection = new System.Drawing.Text.PrivateFontCollection();
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

           
                string alarmPath = Path.Combine(baseDir, "Assets", "UI", "SystemAlarmBlueCancel.png");
                if (File.Exists(alarmPath)) Img_AlarmBg = Image.FromFile(alarmPath);

                string btnPath = Path.Combine(baseDir, "Assets", "UI", "button.png");
                if (File.Exists(btnPath)) Img_PopupBtn = Image.FromFile(btnPath);

                string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "custom_font.ttf");

                if (File.Exists(fontPath))
                {
                    fontCollection.AddFontFile(fontPath);
                    customFontFamily = fontCollection.Families[0]; // 로드된 BM JUA 패밀리 선점
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("폰트 로드 실패: " + ex.Message);
            }
        }



        public static Font F(float size, FontStyle style)
        {
            if (customFontFamily != null)
            {
                return new Font(customFontFamily, size, style);
            }
            // 폰트 파일 유실 시 시스템 기본 맑은 고딕으로 안전 무대 백업
            return new Font("Malgun Gothic", size, style);
        }

        public static StringFormat Center()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static StringFormat Left()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static StringFormat LeftMiddle()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static Color Lighten(Color c, int v)
        {
            return Color.FromArgb(c.A, Math.Min(255, c.R + v), Math.Min(255, c.G + v), Math.Min(255, c.B + v));
        }

        public static Color Darken(Color c, int v)
        {
            return Color.FromArgb(c.A, Math.Max(0, c.R - v), Math.Max(0, c.G - v), Math.Max(0, c.B - v));
        }

        public static void DrawXPWallpaper(Graphics g, Rectangle r)
        {
            DesktopBackgroundUI.Shared.Draw(g, r);
        }

        private static void DrawFallbackXPWallpaper(Graphics g, Rectangle r)
        {
            using (LinearGradientBrush sky = new LinearGradientBrush(new Rectangle(0, 0, r.Width, r.Height), Color.FromArgb(38, 111, 219), Color.FromArgb(160, 220, 255), 90f))
                g.FillRectangle(sky, r);
            DrawCloud(g, 120, 76, 1.05f);
            DrawCloud(g, 620, 130, 0.85f);
            DrawCloud(g, 1030, 92, 1.1f);
            Point[] far = { new Point(-120, r.Bottom), new Point(220, r.Bottom - 170), new Point(560, r.Bottom - 100), new Point(980, r.Bottom - 215), new Point(r.Right + 130, r.Bottom - 90), new Point(r.Right + 130, r.Bottom) };
            using (SolidBrush b = new SolidBrush(Color.FromArgb(55, 150, 72))) g.FillPolygon(b, far);
            Point[] near = { new Point(-120, r.Bottom), new Point(300, r.Bottom - 95), new Point(780, r.Bottom - 140), new Point(1180, r.Bottom - 78), new Point(r.Right + 160, r.Bottom - 135), new Point(r.Right + 160, r.Bottom) };
            using (SolidBrush b = new SolidBrush(Color.FromArgb(100, 181, 76))) g.FillPolygon(b, near);
        }

        private static Image LoadXPBlissBackground()
        {
            if (xpBlissBackgroundCache != null) return xpBlissBackgroundCache;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "stage01_background.png");
            try
            {
                if (File.Exists(path)) xpBlissBackgroundCache = Image.FromFile(path);
            }
            catch
            {
                xpBlissBackgroundCache = null;
            }
            return xpBlissBackgroundCache;
        }

        private static void DrawImageCover(Graphics g, Image img, Rectangle dest)
        {
            if (img == null || dest.Width <= 0 || dest.Height <= 0) return;
            float scale = Math.Max(dest.Width / (float)img.Width, dest.Height / (float)img.Height);
            int sw = Math.Max(1, (int)(dest.Width / scale));
            int sh = Math.Max(1, (int)(dest.Height / scale));
            int sx = Math.Max(0, (img.Width - sw) / 2);
            int sy = Math.Max(0, (img.Height - sh) / 2);
            Rectangle src = new Rectangle(sx, sy, Math.Min(sw, img.Width - sx), Math.Min(sh, img.Height - sy));
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
        }

        // 몬스터 그리기 //

        public static void DrawEnemy(Graphics g, GameEntity e, float cameraX, int clientHeight)
        {
            if (e == null) return;

            // 1. 카메라 좌표계 스크린 월드 변환
            int screenX = (int)(e.X - cameraX);
            Rectangle r = new Rectangle(screenX - 32, (int)e.Y - 32, 64, 64);

            if (e.IsBoss || e.Kind == "BOSS")
            {
                DrawBoss(g, r, e);
                return;
            }
            else
            {
                // 3x3 중 딱 1칸만 오려 그리는 완성형 렌더러 호출
                DrawFileMonster(g, r, e);
            }

            // ---------------------------------------------------------------------------------
            // 💡 145x120 크기로 그려지는 실제 몬스터의 '진짜 가로 정중앙' 축 계산 (e.X 기반)
            // ---------------------------------------------------------------------------------
            int monsterCenterX = screenX; // 몬스터의 완벽한 도트 중심축 (e.X 보정값)
            int uiTopY = (int)e.Y - 70;   // 몬스터 머리 위 안전 포지셔닝 Y축

            // 2. 머리 위 체력바 정상화 (몬스터 정중앙에 완벽 정렬)
            int hpBarWidth = 64;
            Rectangle hp = new Rectangle(monsterCenterX - (hpBarWidth / 2), uiTopY, hpBarWidth, 8);
            DrawBar(g, hp, e.Hp, e.MaxHp, Color.OrangeRed);

            // 3. 텍스트 길이에 맞춰 검은색 배경 칸 유동적 확장 (MeasureString)
            string monsterName = e.Name ?? "UNKNOWN";

            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                // 몬스터 이름의 실제 가로/세로 픽셀 길이를 실시간으로 정확하게 측정
                SizeF textSize = g.MeasureString(monsterName, f);

                // 글자가 박스 벽에 딱 붙지 않도록 좌우 여백(Padding) 부여
                int paddingX = 14;
                int boxWidth = (int)Math.Ceiling(textSize.Width) + paddingX;
                int boxHeight = 16;

                // 💡 [교정 완료]: monsterCenterX를 사용하여 이름 박스가 중앙을 기준으로 늘어나도록 배치
                Rectangle nameRect = new Rectangle(
                    monsterCenterX - (boxWidth / 2),
                    uiTopY - 20,
                    boxWidth,
                    boxHeight
                );

                // 동적으로 조절된 크기의 검은색 반투명 배경판 드로우
                g.FillRectangle(bg, nameRect);

                // 늘어난 칸 정중앙에 텍스트 안착
                g.DrawString(monsterName, f, sb, nameRect, Center());
            }
        }

        // * [교정형 자산 로더]: 낡은 이름 비교 명세를 청소하고 오직 e.Kind 파일 이름으로만 스캔 경로를 뚫습니다.
        private static Image LoadStrictMonsterAssetDirect(GameEntity e)
        {
            if (e == null || string.IsNullOrEmpty(e.Kind)) return null;

            string targetPngName = e.Kind.Trim();

            lock (strictMonsterLock)
            {
                if (strictMonsterCache.TryGetValue(targetPngName, out Image cachedImg) && cachedImg != null)
                    return cachedImg;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] folderNames = { "Assets/Characters/Enemies", "Assets/Characters/Enemise", "Assets/Monsters" };
                string fullPath = "";

                foreach (var folder in folderNames)
                {
                    string checkPath = Path.Combine(baseDir, folder.Replace('/', Path.DirectorySeparatorChar), targetPngName);
                    if (File.Exists(checkPath)) { fullPath = checkPath; break; }
                }
                try
                {
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        Image img = Image.FromFile(fullPath);
                        strictMonsterCache[targetPngName] = img;
                        return img;
                    }
                }
                catch { }
                return null;
            }
        }

        // * [Renderer.cs 내부: 3x3 전체 시트 번짐 현상 전면 처단 최종 종결 버전]
        private static void DrawFileMonster(Graphics g, Rectangle r, GameEntity e)
        {
            Image sheet = LoadStrictMonsterAssetDirect(e);

            if (sheet == null)
            {
                DrawFallbackFileMonster(g, r, e);
                return;
            }

            // 💡 [최종 종결 가드 격실]: 팩토리가 주는 에셋 이름이 "Teleport_2.png"일 때 하단의 낡은 연산틀을 차단합니다.
            if (e.Kind != null && e.Kind.Contains("Teleport_2"))
            {
                // 형진님이 명세하신 1024x1024 해상도 기반 오차 없는 완벽한 정수 3분할 세팅
                const int fixedCellW = 341;
                const int fixedCellH = 341;
                const int cols = 3;

                int targetFrameIndex = 0;

                // * [StateTimer 기반 애니메이션 동기화]
                if (e.StateTimer >= 115) // 증발 단계: 2행 3열 (인덱스 5)
                {
                    targetFrameIndex = 5;
                }
                else if (e.StateTimer >= 0 && e.StateTimer < 10) // 안착 단계: 3행 3열 (인덱스 8)
                {
                    targetFrameIndex = 8;
                }
                else // 평상시 대기 애니메이션 (0~4번 프레임 순환)
                {
                    targetFrameIndex = (Environment.TickCount / 150) % 5;
                }

                int col = targetFrameIndex % cols;
                int row = targetFrameIndex / cols;

                // 💡 [진짜 9등분 크롭]: 옆 칸 레이아웃이 절대 침범하지 못하도록 341 단위로 정확히 쪼개냅니다.
                int srcX = col * fixedCellW;
                int srcY = row * fixedCellH;

                // 외곽 테두리선의 압축 노이즈 제거를 위해 사방 3픽셀 안전 가드 마진 수축
                Rectangle srcRect = new Rectangle(srcX + 3, srcY + 3, fixedCellW - 6, fixedCellH - 6);

                // 💡 [찌그러짐 원천 차단 그릇]: 145x120의 비대칭 규격을 깨부수고, 완벽한 1:1 정방형 130px 크기 그릇 강제 부여!
                int displaySize = 130;
                Rectangle perfectGridDst = new Rectangle(
                    r.X + (r.Width / 2) - (displaySize / 2),
                    r.Y + r.Height - displaySize + 12, // 발바닥 지면 완벽 밀착 보정값
                    displaySize,
                    displaySize
                );

                // * 도트 깨짐 및 압축 오차 방지 필터 세팅
                var oldInterpolation = g.InterpolationMode;
                var oldPixelOffset = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                // 🌟 전체 시트 출력을 완전 중단하고, 정밀 분할된 딱 1칸만 드로우!
                g.DrawImage(sheet, perfectGridDst, srcRect, GraphicsUnit.Pixel);

                // * 엔진 상태 복원 후 즉시 종료하여 하단의 전체 드로우 else 구역을 원천 차단(Bypass)합니다.
                g.InterpolationMode = oldInterpolation;
                g.PixelOffsetMode = oldPixelOffset;
                return;
            }

            // ==========================================================
            // 기존의 멀쩡했던 4x4 일반 잡몹들(Dash, Spread 등)의 원본 라우팅 구역
            // ==========================================================
            int drawW = 145, drawH = 120;
            Rectangle dst = new Rectangle(
                r.X + 32 - (drawW / 2),
                r.Y + 32 - (drawH / 2) - 4,
                drawW,
                drawH
            );

            var prevInterpolation = g.InterpolationMode;
            var prevPixelOffset = g.PixelOffsetMode;
            var prevSmoothing = g.SmoothingMode;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;

            if (e.Kind == "Dash_1.png")
            {
                Draw_Image_Interpretation_1(g, sheet, dst, e);
            }
            else if (e.Kind == "Dash_2.png")
            {
                Draw_Image_Interpretation_2(g, sheet, dst, e);
            }
            else if (e.Kind == "Spread_1.png" || e.Kind == "Spread_2.png")
            {
                Draw_Image_Interpretation_3(g, sheet, dst, e);
            }
            else if (e.Kind == "Teleport_1.png")
            {
                Draw_Image_Interpretation_4(g, sheet, dst, e);
            }
            else
            {
                Rectangle src = new Rectangle(0, 0, sheet.Width, sheet.Height);
                g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
            }

            g.InterpolationMode = prevInterpolation;
            g.PixelOffsetMode = prevPixelOffset;
            g.SmoothingMode = prevSmoothing;

            if (e.HitFlash > 0)
            {
                using (SolidBrush flash = new SolidBrush(Color.FromArgb(90, Color.White)))
                    g.FillEllipse(flash, dst.X + 8, dst.Y + 10, dst.Width - 16, dst.Height - 18);
            }
        }

        // * [해석 방식 1]: 4x4 구조 (방패형 - Security_Firewall 전용)
        private static void Draw_Image_Interpretation_1(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 4;
            int cellW = sheet.Width / cols; int cellH = sheet.Height / rows;
            int targetFrameIndex = 15; // 기본 대기 프레임

            // * [돌진 상태(1)일 때 0~14번 프레임 루프 재생]
            if (e.MonsterState == 1)
            {
                targetFrameIndex = (Environment.TickCount / 90) % 15;
            }

            int col = targetFrameIndex % cols; int row = targetFrameIndex / cols; int pad = 2;
            Rectangle src = new Rectangle(col * cellW + pad, row * cellH + pad, Math.Max(1, cellW - pad * 2), Math.Max(1, cellH - pad * 2));
            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }

        private static void Draw_Image_Interpretation_2(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 2;
            int cellW = sheet.Width / cols; int cellH = sheet.Height / rows;
            int targetFrameIndex = 2;

            if (e.MonsterState == 1)
            {
                int[] dashSequence = { 0, 1, 3, 4, 5, 6, 7 };
                int framesPerTick = 5;
                int currentFramePointer = e.StateTimer / framesPerTick;

                if (currentFramePointer >= dashSequence.Length)
                {
                    targetFrameIndex = dashSequence[dashSequence.Length - 1];
                }
                else
                {
                    targetFrameIndex = dashSequence[currentFramePointer];
                }
            }
            else
            {
                targetFrameIndex = 2;
            }

            int col = targetFrameIndex % cols; int row = targetFrameIndex / cols;

            // 💡 [버그 해결 1: 하얀 선 제거] 
            // * [자르는 영역의 외곽 경계 찌꺼기가 번져서 아래에 하얀 선이 남지 않도록 내부로 1.5픽셀 마진 수축 연산]
            int padX = 2;
            int padY = 2;
            Rectangle src = new Rectangle(
                col * cellW + padX,
                row * cellH + padY,
                Math.Max(1, cellW - padX * 2),
                Math.Max(1, cellH - padY * 2 - 1) // * [하단 경계 1픽셀 강제 컷오프 가드]
            );
            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }
        // * [해석 방식 3 최종 종결판 - 원본 이미지 완벽 호환 버전]
        // * [형진님이 주신 원본의 '0808' 디자인을 100% 그대로 쓰면서 아래칸 찌꺼기만 코드로 칼같이 잘라냅니다]
        private static void Draw_Image_Interpretation_3(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 4;

            // 💡 이미지 전체 해상도를 기반으로 분할하되, 아랫칸 침범을 막기 위해 수동 마진 적용
            int cellW = sheet.Width / cols;
            int cellH = sheet.Height / rows;

            int targetFrameIndex = 4; // 1행(0~3) 배제, 2행 첫 칸(4) 시작

            int localAiTick = (Environment.TickCount / 33) % 70;

            if (localAiTick >= 0 && localAiTick < 20)
            {
                int[] prepare = { 4, 5, 6 };
                targetFrameIndex = prepare[(localAiTick / 7) % prepare.Length];
            }
            else if (localAiTick >= 20 && localAiTick < 45)
            {
                int[] fire = { 7, 8, 9, 10, 11 };
                targetFrameIndex = fire[((localAiTick - 20) / 5) % fire.Length];
            }
            else if (localAiTick >= 45 && localAiTick < 60)
            {
                int[] recovery = { 12, 13, 14, 15 };
                targetFrameIndex = recovery[((localAiTick - 45) / 4) % recovery.Length];
            }
            else
            {
                targetFrameIndex = 4 + ((localAiTick - 60) / 5) % 2;
            }

            int col = targetFrameIndex % cols;
            int row = targetFrameIndex / rows;

            // 💡 [핵심 보정]: 원본의 불균일한 경계를 잡기 위해, 자르는 사각형의 상단은 내리고, 하단은 대폭 깎아냅니다.
            int startX = col * cellW + 4;               // 좌측 마진 증가
            int startY = row * cellH + 4;               // 상단 마진을 내려서 윗칸 잔상 제거
            int cropW = cellW - 8;                      // 가로폭 압축
            int cropH = cellH - 12;                     // 🌟 높이를 12픽셀이나 바짝 줄여서 아래칸 안테나 절대 침범 불가하도록 가드

            Rectangle src = new Rectangle(startX, startY, Math.Max(1, cropW), Math.Max(1, cropH));
            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }
        // * [해석 방식 4 최종 종결 보정판]: 4x4 구조 (유령형 - 사방 마진 가드로 하얀 선 완벽 소멸)
        // * [소수점 반올림 오차로 인해 아래 행/옆 칸의 이미지 조각이 딸려 올라오는 현상을 물리적으로 완전 가드합니다]
        private static void Draw_Image_Interpretation_4(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 4;
            int cellW = sheet.Width / cols; int cellH = sheet.Height / rows;
            int targetFrameIndex = 0;

            // * [EnemyLogicSystem.cs의 120틱 주기 순간이동 타이머와 정밀 동기화]
            if (e.StateTimer >= 115) // A. 텔레포트 직전 (증발 흔적 프레임)
            {
                targetFrameIndex = 14;
            }
            else if (e.StateTimer >= 0 && e.StateTimer < 10) // B. 텔레포트 직후 (안착 프레임)
            {
                targetFrameIndex = 15;
            }
            else // C. 일반 대기 및 이동 애니메이션 (0~13번 프레임 순환)
            {
                targetFrameIndex = (Environment.TickCount / 120) % 14;
            }

            int col = targetFrameIndex % cols; int row = targetFrameIndex / cols;

            // 💡 [버그 해결]: 상하좌우 모든 방향에 대한 가드 마진(Inset) 3픽셀로 확장 강화
            // * [좌우 여백(padX)과 상하 여백(padY)을 사방으로 3픽셀씩 깊숙하게 깎아내어 경계선 격리]
            int padX = 3;
            int padY = 3;

            // * [정밀 크롭 연산]: 셀 본연의 크기에서 사방 마진 영역을 확실하게 빼주어
            // * [아랫줄 프레임의 데이터 찌꺼기가 단 1픽셀도 위로 침범하여 하얀 선을 만들 수 없도록 차단]
            int safeCropWidth = cellW - (padX * 2);
            int safeCropHeight = cellH - (padY * 2);

            Rectangle src = new Rectangle(
                col * cellW + padX,
                row * cellH + padY,
                Math.Max(1, safeCropWidth),
                Math.Max(1, safeCropHeight)
            );

            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }
        // * [Renderer.cs 내부: 1024x1024 자산 비율 왜곡 전면 수정 버전]
        private static void Draw_Image_Interpretation_5(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            // 💡 [수학적 9등분 공식]: 1024 / 3 = 정확히 341px 픽셀 격리
            const int cellW = 341;
            const int cellH = 341;
            const int cols = 3;

            int targetFrameIndex = 0;

            // * [타이머 기반 프레임 추적 애니메이션]
            if (e.StateTimer >= 115) // 증발 단계: 2행 3열 (인덱스 5)
            {
                targetFrameIndex = 5;
            }
            else if (e.StateTimer >= 0 && e.StateTimer < 10) // 안착 단계: 3행 3열 (인덱스 8)
            {
                targetFrameIndex = 8;
            }
            else // 일반 대기 (0~4번 프레임 순환)
            {
                targetFrameIndex = (Environment.TickCount / 150) % 5;
            }

            int col = targetFrameIndex % cols;
            int row = targetFrameIndex / cols;

            // 💡 [원본 소스 크롭]: 옆 칸 데이터가 절대 스며들지 못하게 정밀 341px 컷오프
            int srcX = col * cellW;
            int srcY = row * cellH;

            // 외곽 경계선 노이즈 방지를 위해 사방 2픽셀씩만 안쪽으로 격리 수축
            Rectangle srcRect = new Rectangle(srcX + 2, srcY + 2, cellW - 4, cellH - 4);

            // 💡 [진짜 해결책 - 왜곡 그릇 전면 폐기]:
            // * 위쪽 함수에서 강제로 구겨 넣은 dst.Width(145), dst.Height(120)의 비대칭 규격을 무시합니다!
            // * 원본 1:1 도트 비율이 완벽하게 유지되도록 가로세로를 동일한 160px 정방형 그릇으로 재조정합니다.
            int finalDisplaySize = 160;

            // * 체력바 밑 지면에 발바닥이 완벽하게 밀착되도록 중심축 좌표 재계산
            Rectangle perfectGridDst = new Rectangle(
                dst.X + (dst.Width / 2) - (finalDisplaySize / 2),
                dst.Y + dst.Height - finalDisplaySize + 10, // Y축 발바닥 고정 보정값
                finalDisplaySize,
                finalDisplaySize
            );

            // * [도트 깨짐 및 압축 오차 방지 필터 세팅]
            var oldInterpolation = g.InterpolationMode;
            var oldPixelOffset = g.PixelOffsetMode;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            // 🌟 교정된 정방형 그릇에 정밀 분할된 1칸 이미지 드로우
            g.DrawImage(sheet, perfectGridDst, srcRect, GraphicsUnit.Pixel);

            // * 엔진 상태 복원
            g.InterpolationMode = oldInterpolation;
            g.PixelOffsetMode = oldPixelOffset;
        }
        private static Image LoadStageBackgroundImage(int stageIndex, bool bossRoom)
        {
            int assetStage = stageIndex;
            string fileName = "StageBg_" + assetStage.ToString("00") + (bossRoom ? "_Boss" : "") + ".png";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
            if (!File.Exists(path) && bossRoom)
            {
                fileName = "StageBg_" + assetStage.ToString("00") + ".png";
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
            }
            if (!File.Exists(path)) return null;
            if (stageImageCache.ContainsKey(path)) return stageImageCache[path];
            try
            {
                Image img = Image.FromFile(path);
                stageImageCache[path] = img;
                return img;
            }
            catch
            {
                return null;
            }
        }


        private static void DrawCloud(Graphics g, float x, float y, float s)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            {
                g.FillEllipse(b, x, y + 16 * s, 76 * s, 34 * s);
                g.FillEllipse(b, x + 28 * s, y, 92 * s, 58 * s);
                g.FillEllipse(b, x + 90 * s, y + 18 * s, 74 * s, 36 * s);
                g.FillRectangle(b, x + 24 * s, y + 30 * s, 128 * s, 25 * s);
            }
        }

        public static void DrawXPTaskbar(Graphics g, Rectangle client, string title)
        {
            TaskbarUI.Shared.Draw(g, client);
        }

        public static void DrawXPWindow(Graphics g, Rectangle r, string title, bool error)
        {
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(55, 0, 0, 0))) g.FillRectangle(sh, r.X + 8, r.Y + 8, r.Width, r.Height);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(236, 241, 248))) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(0, 68, 170), 2f)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            Rectangle tb = new Rectangle(r.X + 2, r.Y + 2, r.Width - 4, 30);
            Color c1 = error ? Color.FromArgb(210, 35, 35) : Color.FromArgb(20, 88, 210);
            Color c2 = error ? Color.FromArgb(116, 0, 0) : Color.FromArgb(60, 145, 255);
            using (LinearGradientBrush b = new LinearGradientBrush(tb, c1, c2, 0f)) g.FillRectangle(b, tb);
            using (Font f = F(10f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
                g.DrawString(title, f, sb, new Rectangle(tb.X + 10, tb.Y, tb.Width - 90, tb.Height), LeftMiddle());
            DrawXPButton(g, new Rectangle(tb.Right - 76, tb.Y + 5, 22, 20), "_");
            DrawXPButton(g, new Rectangle(tb.Right - 52, tb.Y + 5, 22, 20), "□");
            DrawXPButton(g, new Rectangle(tb.Right - 28, tb.Y + 5, 22, 20), "X");
        }

        public static void DrawXPButton(Graphics g, Rectangle r, string text)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.White, Color.FromArgb(190, 210, 240), 90f)) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(40, 80, 150))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = F(8f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(25, 35, 60)))
                g.DrawString(text, f, sb, r, Center());
        }

        public static void DrawButton(Graphics g, Rectangle r, string text, bool selected)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, selected ? Color.FromArgb(255, 246, 182) : Color.White, selected ? Color.FromArgb(240, 178, 54) : Color.FromArgb(202, 222, 250), 90f)) g.FillRectangle(b, r);
            using (Pen p = new Pen(selected ? Color.FromArgb(190, 105, 0) : Color.FromArgb(56, 110, 190), selected ? 2f : 1f)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = F(9f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(30, 38, 54)))
                g.DrawString(text, f, sb, r, Center());
        }

        public static void DrawBar(Graphics g, Rectangle r, int value, int max, Color color)
        {
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(230, 238, 248))) g.FillRectangle(bg, r);
            using (Pen p = new Pen(Color.FromArgb(92, 118, 154))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            int w = 0;
            if (max > 0) w = Math.Max(0, Math.Min(r.Width - 2, (int)((r.Width - 2) * (value / (float)max))));
            if (w > 0)
            {
                Rectangle fill = new Rectangle(r.X + 1, r.Y + 1, w, r.Height - 2);
                using (LinearGradientBrush b = new LinearGradientBrush(fill, Lighten(color, 30), Darken(color, 18), 90f)) g.FillRectangle(b, fill);
            }
        }

        private const int NpcEmotionCanvasWidth = 240;
        private const int NpcEmotionCanvasHeight = 270;
        private const int NpcEmotionCropMargin = 6;
        private const int NpcEmotionBottomExtra = 28;
        private const int NpcEmotionAlphaThreshold = 1;

        private const int NpcDrawYOffset = 0;

        private static Image LoadNpc(NpcMood mood)
        {
            if (npcCache.ContainsKey(mood))
                return npcCache[mood];

            Image sheet = LoadNpcEmotionSheet();

            if (sheet != null)
            {
                Rectangle cell = GetNpcEmotionCell(sheet, mood);
                Bitmap normalized = CropNpcEmotionNormalized(sheet, cell, mood);

                if (normalized != null)
                {
                    npcCache[mood] = normalized;
                    return normalized;
                }
            }

            // 실패했을 때 기존 개별 NPC 이미지 fallback
            string name = "basic";

            if (mood == NpcMood.Welcome) name = "welcome";
            else if (mood == NpcMood.Thinking) name = "thinking";
            else if (mood == NpcMood.Happy) name = "happy";
            else if (mood == NpcMood.Question) name = "question";
            else if (mood == NpcMood.Error) name = "error";
            else if (mood == NpcMood.Bsod) name = "bsod";
            else if (mood == NpcMood.Progress) name = "progress";
            else if (mood == NpcMood.Loading) name = "loading";
            else if (mood == NpcMood.Damaged) name = "damaged";
            else if (mood == NpcMood.Log) name = "log";
            else if (mood == NpcMood.Warning) name = "warning";

            string oldPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "NPC404_" + name + ".png"
            );

            Image img = null;

            try
            {
                if (File.Exists(oldPath))
                    img = Image.FromFile(oldPath);
            }
            catch
            {
                img = null;
            }

            npcCache[mood] = img;
            return img;
        }

        private static Image LoadNpcEmotionSheet()
        {
            if (npcEmotionSheetCache != null)
                return npcEmotionSheetCache;

            string fileName = "npc_emotions.png";

            string[] paths =
            {
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets",
                    "Characters",
                    "NPC",
                    fileName
                ),

                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "Assets",
                    "Characters",
                    "NPC",
                    fileName
                ),

                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Assets",
                    "Characters",
                    "NPC",
                    fileName
                )
            };

            foreach (string rawPath in paths)
            {
                string path = Path.GetFullPath(rawPath);

                try
                {
                    if (File.Exists(path))
                    {
                        npcEmotionSheetCache = Image.FromFile(path);
                        return npcEmotionSheetCache;
                    }
                }
                catch
                {
                    npcEmotionSheetCache = null;
                }
            }

            return null;
        }

        private static Rectangle GetNpcEmotionCell(Image sheet, NpcMood mood)
        {
            int col = 0;
            int row = 0;

            if (mood == NpcMood.Basic)
            {
                col = 0;
                row = 0;
            }
            else if (mood == NpcMood.Welcome)
            {
                col = 1;
                row = 0;
            }
            else if (mood == NpcMood.Happy)
            {
                col = 0;
                row = 1;
            }
            else if (mood == NpcMood.Question)
            {
                col = 0;
                row = 2;
            }
            else if (mood == NpcMood.Error)
            {
                col = 1;
                row = 2;
            }
            else if (mood == NpcMood.Bsod)
            {
                col = 3;
                row = 2;
            }
            else if (mood == NpcMood.Progress)
            {
                col = 0;
                row = 3;
            }
            else if (mood == NpcMood.Loading)
            {
                col = 3;
                row = 0;
            }
            else if (mood == NpcMood.Damaged)
            {
                col = 2;
                row = 2;
            }
            else if (mood == NpcMood.Log)
            {
                col = 3;
                row = 3;
            }
            else if (mood == NpcMood.Warning)
            {
                col = 2;
                row = 3;
            }
            else if (mood == NpcMood.Thinking)
            {
                col = 1;
                row = 3;
            }

            int x1 = (int)Math.Round(sheet.Width * (col / 4.0));
            int y1 = (int)Math.Round(sheet.Height * (row / 4.0));
            int x2 = (int)Math.Round(sheet.Width * ((col + 1) / 4.0));
            int y2 = (int)Math.Round(sheet.Height * ((row + 1) / 4.0));

            return new Rectangle(
                x1,
                y1,
                Math.Max(1, x2 - x1),
                Math.Max(1, y2 - y1)
            );
        }
        private static int GetNpcEmotionTopIgnore(NpcMood mood)
        {
            if (mood == NpcMood.Log)
            {
                return 15;
            }

            if (mood == NpcMood.Progress ||
                mood == NpcMood.Thinking ||
                mood == NpcMood.Warning)
            {
                return 10;
            }


            return 0;
        }
        private static Rectangle AdjustNpcEmotionCell(Rectangle cell, NpcMood mood)
        {
            Rectangle adjusted = cell;

            if (mood == NpcMood.Happy ||
                mood == NpcMood.Question ||
                mood == NpcMood.Error ||
                mood == NpcMood.Damaged ||
                mood == NpcMood.Bsod)
            {
                adjusted.Y += 30;
                adjusted.Height -= 30;
            }

            if (mood == NpcMood.Welcome)
            {
                adjusted.Height += 36;
            }

            if (mood == NpcMood.Basic || mood == NpcMood.Loading)
            {
                adjusted.Height += 14;
            }

            if (mood == NpcMood.Progress ||
                mood == NpcMood.Thinking ||
                mood == NpcMood.Warning ||
                mood == NpcMood.Log)
            {
                adjusted.Y += 8;
                adjusted.Height -= 8;
            }

            if (adjusted.Y < 0)
                adjusted.Y = 0;

            if (adjusted.Bottom > cell.Bottom + 36)
                adjusted.Height = cell.Bottom + 36 - adjusted.Y;

            return adjusted;
        }

        private static Bitmap ExtractNpcEmotionFixed(Image sheet, Rectangle cell, NpcMood mood)
        {
            Rectangle src = AdjustNpcEmotionCell(cell, mood);

            if (src.X < 0) src.X = 0;
            if (src.Y < 0) src.Y = 0;
            if (src.Right > sheet.Width) src.Width = sheet.Width - src.X;
            if (src.Bottom > sheet.Height) src.Height = sheet.Height - src.Y;

            Bitmap result = new Bitmap(NpcEmotionCanvasWidth, NpcEmotionCanvasHeight);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float scale = Math.Min(
                    NpcEmotionCanvasWidth / (float)src.Width,
                    NpcEmotionCanvasHeight / (float)src.Height
                );

                int drawW = Math.Max(1, (int)(src.Width * scale));
                int drawH = Math.Max(1, (int)(src.Height * scale));

                int drawX = (NpcEmotionCanvasWidth - drawW) / 2;
                int drawY = NpcEmotionCanvasHeight - drawH;

                g.DrawImage(
                    sheet,
                    new Rectangle(drawX, drawY, drawW, drawH),
                    src,
                    GraphicsUnit.Pixel
                );
            }

            return result;
        }
        private static Bitmap CropNpcEmotionNormalized(Image sheet, Rectangle cell, NpcMood mood)
        {
            using (Bitmap src = new Bitmap(sheet))
            {
                int topIgnore = GetNpcEmotionTopIgnore(mood);
                int scanTop = Math.Min(cell.Bottom - 1, cell.Top + topIgnore);

                int left = cell.Right;
                int top = cell.Bottom;
                int right = cell.Left;
                int bottom = cell.Top;

                for (int y = scanTop; y < cell.Bottom && y < src.Height; y++)
                {
                    for (int x = cell.Left; x < cell.Right && x < src.Width; x++)
                    {
                        Color c = src.GetPixel(x, y);

                        if (c.A > NpcEmotionAlphaThreshold)
                        {
                            if (x < left) left = x;
                            if (x > right) right = x;
                            if (y < top) top = y;
                            if (y > bottom) bottom = y;
                        }
                    }
                }

                if (right <= left || bottom <= top)
                    return null;

                left = Math.Max(cell.Left, left - NpcEmotionCropMargin);

                top = Math.Max(scanTop, top - NpcEmotionCropMargin);

                right = Math.Min(cell.Right - 1, right + NpcEmotionCropMargin);
                bottom = Math.Min(cell.Bottom - 1, bottom + NpcEmotionCropMargin + NpcEmotionBottomExtra);

                Rectangle crop = new Rectangle(
                    left,
                    top,
                    right - left + 1,
                    bottom - top + 1
                );

                Bitmap result = new Bitmap(NpcEmotionCanvasWidth, NpcEmotionCanvasHeight);

                using (Graphics g = Graphics.FromImage(result))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    int drawW = crop.Width;
                    int drawH = crop.Height;

                    float scale = Math.Min(
                        NpcEmotionCanvasWidth / (float)drawW,
                        NpcEmotionCanvasHeight / (float)drawH
                    );

                    if (scale < 1f)
                    {
                        drawW = Math.Max(1, (int)(drawW * scale));
                        drawH = Math.Max(1, (int)(drawH * scale));
                    }

                    int drawX = (NpcEmotionCanvasWidth - drawW) / 2;
                    int drawY = NpcEmotionCanvasHeight - drawH;

                    g.DrawImage(
                        sheet,
                        new Rectangle(drawX, drawY, drawW, drawH),
                        crop,
                        GraphicsUnit.Pixel
                    );
                }

                return result;
            }
        }

        public static void DrawNpcImage(Graphics g, Rectangle r, NpcMood mood)
        {
            Image img = LoadNpc(mood);

            if (img != null)
            {
                InterpolationMode oldInterpolation = g.InterpolationMode;
                PixelOffsetMode oldPixelOffset = g.PixelOffsetMode;

                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float scale = Math.Min(
                    r.Width / (float)img.Width,
                    r.Height / (float)img.Height
                );

                int w = Math.Max(1, (int)(img.Width * scale));
                int h = Math.Max(1, (int)(img.Height * scale));

                Rectangle dst = new Rectangle(r.X + (r.Width - w) / 2,r.Y + (r.Height - h) / 2 + NpcDrawYOffset,w,h);
                g.DrawImage(img, dst);

                g.InterpolationMode = oldInterpolation;
                g.PixelOffsetMode = oldPixelOffset;
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 70, 120)))
                    g.FillRectangle(b, r);

                using (Font f = F(9f, FontStyle.Bold))
                    g.DrawString("Recovery\nAssistant", f, Brushes.White, r, Center());
            }
        }

        public static void DrawNotification(Graphics g, Rectangle r, string title, string text, NpcMood mood, bool error)
        {
            DrawXPWindow(g, r, title, error);
            Rectangle npc = new Rectangle(r.X + 22, r.Y + 58, 140, Math.Max(120, r.Height - 104));
            DrawNpcImage(g, npc, mood);

            if (error)
            {
                Rectangle warn = new Rectangle(r.X + 172, r.Y + 58, 42, 42);
                Point[] tri = new Point[] { new Point(warn.X + warn.Width / 2, warn.Y + 2), new Point(warn.Right - 2, warn.Bottom - 2), new Point(warn.X + 2, warn.Bottom - 2) };
                using (SolidBrush wb = new SolidBrush(Color.FromArgb(255, 220, 70))) g.FillPolygon(wb, tri);
                using (Pen wp = new Pen(Color.DarkRed, 2f)) g.DrawPolygon(wp, tri);
                using (Font wf = F(18f, FontStyle.Bold))
                using (SolidBrush rb = new SolidBrush(Color.DarkRed))
                    g.DrawString("!", wf, rb, warn, Center());
            }

            Rectangle tx = new Rectangle(r.X + 222, r.Y + 56, r.Width - 246, r.Height - 110);
            using (Font f = F(10f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(26, 34, 54)))
                g.DrawString(text, f, b, tx, Left());
            Rectangle btn = new Rectangle(r.Right - 120, r.Bottom - 46, 96, 30);
            DrawButton(g, btn, "확인", true);
        }

        private static Image LoadPlayerSheet(string fileName)
        {
            if (playerSpriteSheets.ContainsKey(fileName))
                return playerSpriteSheets[fileName];

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets", "Characters", "Player", fileName
            );

            Image img = null;

            try
            {
                if (File.Exists(path))
                    img = Image.FromFile(path);
            }
            catch
            {
                img = null;
            }

            playerSpriteSheets[fileName] = img;
            return img;
        }

        private static void DrawPlayerSwordSprite(Graphics g, PlayerState p, float drawX, float baseY, bool walking)
        {
            string fileName;
            int columns;
            int rows;
            int row;
            bool flip = false;

            if (!walking)
            {
                fileName = "player_still_sword.png";
                columns = 1;
                rows = 1;
                row = 0;
            }
            else if (p.Direction == 0)
            {
                fileName = "player_walk_sword_front.png";
                columns = 4;
                rows = 1;
                row = 0;
            }
            else if (p.Direction == 2)
            {
                fileName = "player_walk_sword_back.png";
                columns = 4;
                rows = 1;
                row = 0;
            }
            else
            {
                fileName = "player_walk_sword_right.png";
                columns = 5;
                rows = 5;
                row = 0;
                flip = p.Direction == 3;
            }

            Image sheet = LoadPlayerSheet(fileName);

            if (sheet == null)
            {
                DrawAntiVirusAgentCharacter(g, drawX, baseY, p.Facing, walking, Environment.TickCount / 180.0, false, false);
                return;
            }

            int frameCount = walking ? columns : 1;
            int frame = walking ? ((int)Math.Floor(p.WalkCycle)) % frameCount : 0;

            int frameW = sheet.Width / columns;
            int frameH = sheet.Height / rows;

            Rectangle src = new Rectangle(frame * frameW, row * frameH, frameW, frameH);

            int destW = 112;
            int destH = 112;
            int offsetX = 0;
            int offsetY = 0;

            if (p.Direction == 0 && walking)
            {
                int[] frontFrameOffsetX = { 0, 0, 0, 4 };
                offsetX += frontFrameOffsetX[frame % frontFrameOffsetX.Length];
                offsetY += 4;
            }

            Rectangle dest = new Rectangle((int)(drawX - destW / 2 + offsetX), (int)(baseY - destH + 8 + offsetY), destW, destH);

            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (flip)
            {
                g.TranslateTransform(drawX, baseY);
                g.ScaleTransform(-1f, 1f);
                dest = new Rectangle(-destW / 2, -destH + 8, destW, destH);
            }

            g.DrawImage(sheet, dest, src, GraphicsUnit.Pixel);
            g.Restore(state);
        }

        private static Image LoadPlayerAgentFrame(int index)
        {
            // index < 0 : idle frame. index 0~7 : video-style Player.exe walk frames with fixed baseline, bent knees, and alternating arms/legs.
            if (index < 0)
            {
                if (playerAgentIdleFrame == null)
                {
                    string idlePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentIdle.png");
                    if (File.Exists(idlePath))
                    {
                        try { playerAgentIdleFrame = Image.FromFile(idlePath); } catch { playerAgentIdleFrame = null; }
                    }
                }
                return playerAgentIdleFrame;
            }

            int slot = Math.Max(0, Math.Min(7, index));
            if (playerAgentWalkFrames[slot] == null)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentWalk" + slot.ToString() + ".png");
                if (File.Exists(path))
                {
                    try { playerAgentWalkFrames[slot] = Image.FromFile(path); } catch { playerAgentWalkFrames[slot] = null; }
                }
            }
            return playerAgentWalkFrames[slot];
        }

        private static void DrawPlayerAgentFrameImage(Graphics g, float worldX, float worldY, int facing, int frameIndex, bool walking)
        {
            Image frame = LoadPlayerAgentFrame(frameIndex);
            if (frame == null)
            {
                DrawAntiVirusAgentCharacter(g, worldX, worldY, facing, walking, Environment.TickCount / 180.0, false, false);
                return;
            }

            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            if (facing < 0)
            {
                g.TranslateTransform(worldX, worldY);
                g.ScaleTransform(-1f, 1f);
                worldX = 0;
                worldY = 0;
            }

            // PlayerAgentIdle/Walk frames are right-facing by default and share one transparent canvas with the character centered at the bottom.
            // This avoids the previous left-right "clone/jitter" feeling caused by uneven sprite-sheet crops.
            int destW = 138;
            int destH = 174;
            Rectangle dest = new Rectangle((int)(worldX - destW / 2), (int)(worldY - destH + 4), destW, destH);
            g.DrawImage(frame, dest);
            g.Restore(state);
        }

        private static Image GetPlayerAgentSheet()
        {
            if (playerAgentSheet != null) return playerAgentSheet;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentSpriteSheet.png");
            if (File.Exists(path))
            {
                try { playerAgentSheet = Image.FromFile(path); } catch { playerAgentSheet = null; }
            }
            return playerAgentSheet;
        }

        private static Rectangle GetPlayerAgentSource(string pose, int frame)
        {
            // Source image is a transparent sprite sheet generated from the provided Player.exe reference.
            // Coordinates are hand-picked to avoid labels and keep the character/action clean.
            if (pose == "clean" || pose == "scan") return new Rectangle(80, 370, 460, 240);
            if (pose == "delete") return new Rectangle(760, 370, 650, 240);
            if (pose == "clean2") return new Rectangle(420, 670, 320, 240);
            if (pose == "delete2") return new Rectangle(740, 670, 650, 240);
            int[] xs = new int[] { 85, 265, 445, 630 };
            int idx = Math.Abs(frame) % xs.Length;
            return new Rectangle(xs[idx], 70, 180, 250);
        }

        private static void DrawPlayerAgentSprite(Graphics g, string pose, float worldX, float worldY, int facing, int frame)
        {
            Image sheet = GetPlayerAgentSheet();
            if (sheet == null) return;
            Rectangle src = GetPlayerAgentSource(pose, frame);
            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (facing < 0)
            {
                g.TranslateTransform(worldX, worldY);
                g.ScaleTransform(-1f, 1f);
                worldX = 0;
                worldY = 0;
            }
            Rectangle dest;
            if (pose == "clean" || pose == "scan")
            {
                dest = new Rectangle((int)(worldX - 100), (int)(worldY - 166), 375, 196);
            }
            else if (pose == "delete")
            {
                dest = new Rectangle((int)(worldX - 100), (int)(worldY - 166), 470, 174);
            }
            else
            {
                dest = new Rectangle((int)(worldX - 53), (int)(worldY - 145), 106, 148);
            }
            g.DrawImage(sheet, dest, src, GraphicsUnit.Pixel);
            g.Restore(state);
        }


        private static void DrawPlayerAgentCharacterFrame(Graphics g, string pose, float worldX, float worldY, int facing, int frame)
        {
            bool walking = pose == "walk";
            if (pose == "idle" || pose == "walk")
            {
                // Idle stays in a fixed attention pose; walking uses the right-facing 8-frame real walk cycle. Left movement is rendered by horizontal flip to prevent moonwalk.
                int frameIndex = walking ? Math.Abs(frame) % 8 : -1;
                DrawPlayerAgentFrameImage(g, worldX, worldY, facing, frameIndex, walking);
                return;
            }

            Image sheet = GetPlayerAgentSheet();
            if (sheet == null)
            {
                DrawAntiVirusAgentCharacter(g, worldX, worldY, facing, false, Environment.TickCount / 150.0, pose == "clean" || pose == "delete", pose == "delete");
                return;
            }
            Rectangle src = pose == "delete" ? new Rectangle(760, 370, 650, 240) : new Rectangle(80, 370, 460, 240);
            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (facing < 0)
            {
                g.TranslateTransform(worldX, worldY);
                g.ScaleTransform(-1f, 1f);
                worldX = 0;
                worldY = 0;
            }
            Rectangle dest = pose == "delete"
                ? new Rectangle((int)(worldX - 100), (int)(worldY - 166), 470, 174)
                : new Rectangle((int)(worldX - 100), (int)(worldY - 166), 375, 196);
            g.DrawImage(sheet, dest, src, GraphicsUnit.Pixel);
            g.Restore(state);
        }

        private static Image GetPlayerActionSheet(int weaponLevel)
        {
            string fileName = "player_action.png";

            if (weaponLevel >= 3)
                fileName = "player_action_level3.png";
            else if (weaponLevel >= 2)
                fileName = "player_action_level2.png";

            Image cached;
            if (playerActionSheetCache.TryGetValue(fileName, out cached))
                return cached;

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Characters",
                "Player",
                fileName
            );

            if (File.Exists(path))
            {
                cached = Image.FromFile(path);
                playerActionSheetCache[fileName] = cached;
                return cached;
            }

            string fallbackPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Characters",
                "Player",
                "player_action.png"
            );

            if (File.Exists(fallbackPath))
            {
                cached = Image.FromFile(fallbackPath);
                playerActionSheetCache[fileName] = cached;
                return cached;
            }

            return null;
        }

        private static Rectangle GetPlayerActionSourceRect(Image sheet, int skillIndex, int frame)
        {
           
            int totalRows = 3;
            int totalColumns = 5;

            int row;

            if (skillIndex == 0)
                row = 0;
            else if (skillIndex == 1)
                row = 1;
            else
                row = 2;

            int usableFrameCount = 5;

            if (frame < 0) frame = 0;
            if (frame >= usableFrameCount) frame = usableFrameCount - 1;

            int cellX1 = (int)Math.Round(frame * sheet.Width / (double)totalColumns);
            int cellX2 = (int)Math.Round((frame + 1) * sheet.Width / (double)totalColumns);

            int cellY1 = (int)Math.Round(row * sheet.Height / (double)totalRows);
            int cellY2 = (int)Math.Round((row + 1) * sheet.Height / (double)totalRows);

            Rectangle cell = Rectangle.FromLTRB(cellX1, cellY1, cellX2, cellY2);

            string cacheKey = sheet.GetHashCode() + "_" + skillIndex + "_" + frame + "_" + sheet.Width + "x" + sheet.Height;

            if (playerActionTrimCache.TryGetValue(cacheKey, out Rectangle cached))
                return cached;

            Bitmap bmp = sheet as Bitmap;

            if (bmp == null)
            {
                playerActionTrimCache[cacheKey] = cell;
                return cell;
            }

            bool hasTransparentBackground = HasTransparentPixel(bmp, cell);

            int minX = cell.Right - 1;
            int maxX = cell.Left;
            int minY = cell.Bottom - 1;
            int maxY = cell.Top;
            bool found = false;

            for (int y = cell.Top; y < cell.Bottom; y++)
            {
                for (int x = cell.Left; x < cell.Right; x++)
                {
                    Color c = bmp.GetPixel(x, y);

                    if (!IsVisibleActionPixel(c, hasTransparentBackground))
                        continue;

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;

                    found = true;
                }
            }

            if (!found)
            {
                playerActionTrimCache[cacheKey] = cell;
                return cell;
            }

            // 핵심 보정값
            // 오른쪽과 위쪽만 더 보여주기.
            int padLeft = 0;
            int padRight = 16;
            int padTop = 24;
            int padBottom = 2;

            int left = minX - padLeft;
            int right = maxX + 1 + padRight;
            int top = minY - padTop;
            int bottom = maxY + 1 + padBottom;

            // 옆 프레임이 섞이지 않도록 기본적으로 현재 칸 안에서만 제한
            if (left < cell.Left) left = cell.Left;
            if (right > cell.Right) right = cell.Right;
            if (top < cell.Top) top = cell.Top;
            if (bottom > cell.Bottom) bottom = cell.Bottom;

            if (right <= left) right = left + 1;
            if (bottom <= top) bottom = top + 1;

            Rectangle trimmed = Rectangle.FromLTRB(left, top, right, bottom);

            playerActionTrimCache[cacheKey] = trimmed;
            return trimmed;
        }

        private static bool HasTransparentPixel(Bitmap bmp, Rectangle area)
        {
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    if (bmp.GetPixel(x, y).A < 250)
                        return true;
                }
            }

            return false;
        }

        private static bool IsVisibleActionPixel(Color c, bool hasTransparentBackground)
        {
            if (c.A <= 10)
                return false;

            // 투명 배경 PNG라면 검은 머리/검은 외곽선도 실제 캐릭터로 인정
            if (hasTransparentBackground)
                return true;

            // 검은 배경 이미지라면 거의 검은색은 배경으로 취급
            bool nearBlack = c.R <= 8 && c.G <= 8 && c.B <= 8;

            return !nearBlack;
        }
        private static Image GetPlayerStillSwordImage()
        {
            if (playerStillSwordImage != null)
                return playerStillSwordImage;

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Characters",
                "Player",
                "player_still_sword.png"
            );

            if (File.Exists(path))
                playerStillSwordImage = Image.FromFile(path);

            return playerStillSwordImage;
        }
        private static Rectangle GetPlayerShieldSourceRect(Image sheet, int frame)
        {
            int columns = 3;
            int rows = 3;
            int totalFrames = columns * rows;

            frame %= totalFrames;
            if (frame < 0) frame = 0;

            int col = frame % columns;
            int row = frame / columns;

            int x1 = (int)Math.Round(col * sheet.Width / (double)columns);
            int x2 = (int)Math.Round((col + 1) * sheet.Width / (double)columns);
            int y1 = (int)Math.Round(row * sheet.Height / (double)rows);
            int y2 = (int)Math.Round((row + 1) * sheet.Height / (double)rows);

            // 칸 구분선이 같이 잘리는 것 방지
            int padding = 3;

            return Rectangle.FromLTRB(
                x1 + padding,
                y1 + padding,
                x2 - padding,
                y2 - padding
            );
        }
        private static Image GetPlayerShieldImage()
        {
            if (playerShieldImage != null)
                return playerShieldImage;

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Caracters",
                "Player",
                "player_shield.png"
            );

            if (File.Exists(path))
                playerShieldImage = Image.FromFile(path);

            return playerShieldImage;
        }
        private static bool DrawPlayerShieldOverlay(Graphics g, PlayerState p, float drawX, float baseY)
        {
            Image shield = GetPlayerShieldImage();

            if (shield == null)
                return false;

            // DefenseTicks = 100에서 시작한다고 가정
            // 시간이 지날수록 0~8번 프레임 순서대로 재생
            int elapsed = Math.Max(0, 100 - p.DefenseTicks);
            int frame = (elapsed / 4) % 9;

            Rectangle src = GetPlayerShieldSourceRect(shield, frame);

            // 크기 조절
            float scale = 0.60f;

            int drawW = (int)(src.Width * scale);
            int drawH = (int)(src.Height * scale);

            // 캐릭터 중심보다 살짝 위에 방어막 중심 배치
            float shieldCenterY = baseY - 62f;

            Rectangle dst = new Rectangle(
                (int)(drawX - drawW / 2),
                (int)(shieldCenterY - drawH / 2),
                drawW,
                drawH
            );

            GraphicsState state = g.Save();

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            g.DrawImage(shield, dst, src, GraphicsUnit.Pixel);

            g.Restore(state);

            return true;
        }
        private static void DrawPlayerStillSprite(Graphics g, PlayerState p, float drawX, float baseY, int facing)
        {
            Image img = GetPlayerStillSwordImage();

            if (img == null)
            {
                DrawPlayerAgentCharacterFrame(g, "idle", drawX, baseY, facing, 0);
                return;
            }

            float scale = 0.10f;

            int drawW = (int)(img.Width * scale);
            int drawH = (int)(img.Height * scale);

            Rectangle dst = new Rectangle(
                (int)(drawX - drawW / 2),
                (int)(baseY - drawH),
                drawW,
                drawH
            );

            if (facing < 0)
            {
                using (Bitmap flipped = new Bitmap(img))
                {
                    flipped.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    g.DrawImage(flipped, dst);
                }
            }
            else
            {
                g.DrawImage(img, dst);
            }
        }
        private static Image GetPlayerMotionSheet(string fileName)
        {
            Image cached;
            if (playerMotionSheetCache.TryGetValue(fileName, out cached))
                return cached;

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Characters",
                "Player",
                fileName
            );

            if (File.Exists(path))
            {
                cached = Image.FromFile(path);
                playerMotionSheetCache[fileName] = cached;
                return cached;
            }

            playerMotionSheetCache[fileName] = null;
            return null;
        }

        private static Rectangle GetPlayerMotionSourceRect(Image sheet, int frameCount, int frame)
        {
            if (frame < 0) frame = 0;
            if (frame >= frameCount) frame = frameCount - 1;

            int x1 = (int)Math.Round(frame * sheet.Width / (double)frameCount);
            int x2 = (int)Math.Round((frame + 1) * sheet.Width / (double)frameCount);

            int cropY = (int)(sheet.Height * 0.22f);
            int cropH = (int)(sheet.Height * 0.50f);

            if (cropY + cropH > sheet.Height)
                cropH = sheet.Height - cropY;

            return new Rectangle(x1, cropY, x2 - x1, cropH);
        }

        private static void DrawPlayerMotionFrame(Graphics g, PlayerState p, float drawX, float baseY, int facing, string fileName)
        {
            Image sheet = GetPlayerMotionSheet(fileName);

            if (sheet == null)
            {
                DrawPlayerStillSprite(g, p, drawX, baseY, facing);
                return;
            }

            Rectangle src = GetPlayerMotionSourceRect(sheet, 6, p.ActionFrame);

            float scale = 0.35f;

            int drawW = (int)(src.Width * scale);
            int drawH = (int)(src.Height * scale);

            float actionCenterY = baseY - 55f;

            Rectangle dst = new Rectangle(
                (int)(drawX - drawW / 2),
                (int)(actionCenterY - drawH / 2),
                drawW,
                drawH
            );

            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (facing < 0)
            {
                using (Bitmap frameBmp = new Bitmap(src.Width, src.Height))
                {
                    using (Graphics fg = Graphics.FromImage(frameBmp))
                    {
                        fg.DrawImage(sheet, new Rectangle(0, 0, src.Width, src.Height), src, GraphicsUnit.Pixel);
                    }

                    frameBmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    g.DrawImage(frameBmp, dst);
                }
            }
            else
            {
                g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
            }

            g.Restore(state);
        }

        private static void DrawPlayerActionFrame(Graphics g, PlayerState p, float drawX, float baseY, int facing)
        {
            Image sheet = GetPlayerActionSheet(p.WeaponLevel);

            if (sheet == null)
                return;

            Rectangle src = GetPlayerActionSourceRect(sheet, p.SkillIndex, p.ActionFrame);

            float scale = 0.32f;

            int drawW = (int)(src.Width * scale);
            int drawH = (int)(src.Height * scale);

            float actionCenterY = baseY - 55f;

            Rectangle dst = new Rectangle(
                (int)(drawX - drawW / 2),
                (int)(actionCenterY - drawH / 2),
                drawW,
                drawH
            );

            if (facing < 0)
            {
                using (Bitmap frameBmp = new Bitmap(src.Width, src.Height))
                {
                    using (Graphics fg = Graphics.FromImage(frameBmp))
                    {
                        fg.DrawImage(sheet, new Rectangle(0, 0, src.Width, src.Height), src, GraphicsUnit.Pixel);
                    }

                    frameBmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    g.DrawImage(frameBmp, dst);
                }
            }
            else
            {
                g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
            }
        }
        public static void DrawRecoveryProgram(Graphics g, PlayerState p, bool selected)
        {
            DrawRecoveryProgram(g, p, selected, 0f, false);
        }

        public static void DrawRecoveryProgram(Graphics g, PlayerState p, bool selected, float cameraX, bool moving)
        {
            float drawX = p.X - cameraX;
            float baseY = p.Y;
            float speed = (float)Math.Sqrt(p.MoveVelocityX * p.MoveVelocityX + p.MoveVelocityY * p.MoveVelocityY);
            bool walking = moving && speed > 0.18f;
            int facing = p.Facing == 0 ? 1 : p.Facing;

            int frame = walking ? ((int)Math.Floor(p.WalkCycle)) % 8 : -1;

            float walkPhase = (float)((p.WalkCycle / 8f) * Math.PI * 2f);
            float bob = walking ? -Math.Abs((float)Math.Sin(walkPhase)) * 1.15f : 0f;

            if (selected)
            {
                using (SolidBrush aura = new SolidBrush(Color.FromArgb(walking ? 24 : 18, 70, 180, 255)))
                    g.FillEllipse(aura, (int)drawX - 54, (int)baseY - 112, 108, 120);
            }
            /*if (p.DefenseTicks > 0)
            {
                using (Pen shield = new Pen(Color.FromArgb(150, 120, 210, 255), 3f))
                    g.DrawEllipse(shield, drawX - 58, baseY - 120, 116, 130);
            }*/
            if (p.ActionState == PlayerActionState.Die)
            {
                DrawPlayerMotionFrame(g, p, drawX, baseY, facing, "player_gameover.png");
            }
            else if (p.ActionState == PlayerActionState.Hit)
            {
                DrawPlayerMotionFrame(g, p, drawX, baseY, facing, "player_attacked.png");
            }
            else if (p.ActionState == PlayerActionState.Skill)
            {
                DrawPlayerSkillAction(g, p, drawX, baseY, facing);
            }
            else if (walking)
            {
                DrawPlayerSwordSprite(g, p, drawX, baseY + bob, walking);
            }
            else
            {
                DrawPlayerStillSprite(g, p, drawX, baseY + bob, facing);
            }

          

            using (Font f = F(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush back = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                Rectangle label = new Rectangle((int)drawX - 75, (int)baseY + 18, 150, 34);
                g.FillRectangle(back, label);
                g.DrawString("Player.exe\n(AntiVirus Agent)", f, b, label, Center());
            }
        }
        private static void DrawPlayerSkillAction(Graphics g, PlayerState p, float drawX, float baseY, int facing)
        {
            Image sheet = GetPlayerActionSheet(p.WeaponLevel);

            if (sheet == null)
            {
                DrawPlayerAgentCharacterFrame(g, "idle", drawX, baseY, facing, 0);
                return;
            }

            Rectangle src = GetPlayerActionSourceRect(sheet, p.SkillIndex, p.ActionFrame);

            float scale = 0.35f;

            int drawW = (int)(src.Width * scale);
            int drawH = (int)(src.Height * scale);

            Rectangle dst = new Rectangle(
                (int)(drawX - drawW / 2),
                (int)(baseY - drawH),
                drawW,
                drawH
            );

            if (facing < 0)
            {
                using (Bitmap frameBmp = new Bitmap(src.Width, src.Height))
                {
                    using (Graphics fg = Graphics.FromImage(frameBmp))
                    {
                        fg.DrawImage(sheet, new Rectangle(0, 0, src.Width, src.Height), src, GraphicsUnit.Pixel);
                    }

                    frameBmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    g.DrawImage(frameBmp, dst);
                }
            }
            else
            {
                g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
            }
        }


        public static void DrawShopShortcut(Graphics g, Rectangle r, int coins)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(75, 0, 0, 0)))
                g.FillEllipse(shadow, r.X + 22, r.Y + 74, r.Width - 44, 18);
            Rectangle bin = new Rectangle(r.X + 43, r.Y + 10, 62, 68);
            using (LinearGradientBrush b = new LinearGradientBrush(bin, Color.White, Color.FromArgb(190, 215, 235), 90f))
                g.FillRectangle(b, bin);
            using (Pen p = new Pen(Color.FromArgb(70, 100, 120), 2f))
                g.DrawRectangle(p, bin);
            using (Pen p = new Pen(Color.FromArgb(70, 165, 90), 3f))
                g.DrawArc(p, bin.X + 10, bin.Y + 12, bin.Width - 20, bin.Height - 20, 30, 260);
            Rectangle label = new Rectangle(r.X + 4, r.Y + 82, r.Width - 8, 36);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(150, 20, 70, 150))) g.FillRectangle(bg, label);
            using (Font f = F(8.5f, FontStyle.Bold))
            using (SolidBrush wb = new SolidBrush(Color.White))
                g.DrawString("Recovery Tools.exe\n" + coins + " coin", f, wb, label, Center());
        }

        public static void DrawFileShortcut(Graphics g, Rectangle r, StageInfo st, bool selected, bool newlyCreated)
        {
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(selected ? 76 : 30, 50, 120, 255))) g.FillEllipse(glow, r.X - 6, r.Y - 2, r.Width + 12, r.Height + 18);
            Rectangle icon = new Rectangle(r.X + 18, r.Y + 6, 52, 62);
            Point[] paper = { new Point(icon.X + 6, icon.Y), new Point(icon.Right - 12, icon.Y), new Point(icon.Right, icon.Y + 12), new Point(icon.Right, icon.Bottom), new Point(icon.X + 6, icon.Bottom) };
            using (LinearGradientBrush b = new LinearGradientBrush(icon, Color.White, Color.FromArgb(214, 226, 244), 90f)) g.FillPolygon(b, paper);
            using (Pen p = new Pen(st.Accent, 2f)) g.DrawPolygon(p, paper);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(44, st.Accent))) g.FillRectangle(b, icon.X + 14, icon.Y + 24, icon.Width - 24, 17);
            using (Pen p = new Pen(st.Accent, 2.2f))
            {
                g.DrawLine(p, icon.X + 16, icon.Y + 28, icon.Right - 14, icon.Y + 28);
                g.DrawLine(p, icon.X + 16, icon.Y + 36, icon.Right - 18, icon.Y + 36);
            }
            // shortcut arrow
            Rectangle ar = new Rectangle(r.X + 10, r.Y + 52, 22, 18);
            using (SolidBrush b = new SolidBrush(Color.White)) g.FillRectangle(b, ar);
            using (Pen p = new Pen(Color.FromArgb(28, 74, 180), 3f))
            {
                g.DrawLine(p, ar.X + 5, ar.Y + 12, ar.Right - 6, ar.Y + 12);
                g.DrawLine(p, ar.X + 6, ar.Y + 12, ar.X + 12, ar.Y + 5);
            }
            if (newlyCreated)
            {
                Rectangle tag = new Rectangle(r.Right - 40, r.Y + 4, 36, 18);
                using (SolidBrush b = new SolidBrush(Color.Red)) g.FillRectangle(b, tag);
                using (Font f = F(7f, FontStyle.Bold)) g.DrawString("NEW", f, Brushes.White, tag, Center());
            }
            if (selected)
            {
                using (Pen p = new Pen(Color.FromArgb(255, 210, 60), 3f)) g.DrawRectangle(p, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
            }
            using (Font f = F(8.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                Rectangle lab = new Rectangle(r.X - 12, r.Bottom - 34, r.Width + 24, 34);
                g.FillRectangle(bg, lab);
                g.DrawString(st.FileName, f, b, lab, Center());
            }
        }


        private static void DrawNaturalWalkOverlay(Graphics g, float x, float y, int facing, double phase, float speed)
        {
            float swing = (float)Math.Sin(phase * Math.PI * 2.0);
            float counter = -swing;
            int dir = facing >= 0 ? 1 : -1;
            int alpha = 185;

            // 팔 스윙: 손이 앞뒤로 번갈아 움직여 걷는 느낌을 보강합니다.
            using (Pen armBack = new Pen(Color.FromArgb(alpha, 20, 45, 90), 4.2f))
            using (Pen armFront = new Pen(Color.FromArgb(alpha, 235, 242, 250), 4.6f))
            {
                g.DrawLine(armBack, x - dir * 16, y - 75, x - dir * (22 + counter * 9), y - 55 + counter * 3);
                g.DrawLine(armFront, x + dir * 15, y - 75, x + dir * (23 + swing * 9), y - 55 + swing * 3);
            }

            // 다리 스윙: 신발 위치를 교차시켜 사람처럼 걷는 실루엣을 만듭니다.
            using (Pen legBack = new Pen(Color.FromArgb(alpha, 20, 25, 34), 5.0f))
            using (Pen legFront = new Pen(Color.FromArgb(alpha, 22, 28, 40), 5.4f))
            using (SolidBrush shoe = new SolidBrush(Color.FromArgb(alpha, 240, 245, 250)))
            using (Pen shoeLine = new Pen(Color.FromArgb(alpha, 35, 55, 85), 1.2f))
            {
                float hipY = y - 44;
                float footY = y - 5;
                float backFootX = x - dir * (10 + counter * 9);
                float frontFootX = x + dir * (10 + swing * 9);
                g.DrawLine(legBack, x - 8, hipY, backFootX, footY);
                g.DrawLine(legFront, x + 8, hipY, frontFootX, footY);
                RectangleF shoe1 = new RectangleF(backFootX - 9, footY - 1, 18, 7);
                RectangleF shoe2 = new RectangleF(frontFootX - 9, footY - 1, 18, 7);
                g.FillEllipse(shoe, shoe1);
                g.FillEllipse(shoe, shoe2);
                g.DrawEllipse(shoeLine, shoe1);
                g.DrawEllipse(shoeLine, shoe2);
            }
        }

        private static void DrawAttentionIdleOverlay(Graphics g, float x, float y, int facing)
        {
            // 대기 상태: 손과 발이 거의 고정된 정자세 느낌만 살짝 강조합니다.
            int alpha = 130;
            using (Pen hand = new Pen(Color.FromArgb(alpha, 240, 245, 250), 4.0f))
            using (Pen foot = new Pen(Color.FromArgb(alpha, 240, 245, 250), 3.6f))
            {
                g.DrawLine(hand, x - 20, y - 67, x - 20, y - 54);
                g.DrawLine(hand, x + 20, y - 67, x + 20, y - 54);
                g.DrawLine(foot, x - 16, y - 5, x - 5, y - 5);
                g.DrawLine(foot, x + 5, y - 5, x + 16, y - 5);
            }
        }

        private static int ClampAlpha(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }


        private static void DrawShimmerBeam(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress, bool deleteBeam)
        {
            if (deleteBeam)
            {
                DrawDeleteBeam(g, x1, y1, x2, y2, color, alpha, progress);
            }
            else
            {
                DrawCleanSprayBeam(g, x1, y1, x2, y2, color, alpha, progress);
            }
        }


        private static void RoundCaps(Pen pen)
        {
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            pen.LineJoin = LineJoin.Round;
        }

        private static void DrawLimb(Graphics g, Pen pen, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            RoundCaps(pen);
            g.DrawLine(pen, x1, y1, x2, y2);
            g.DrawLine(pen, x2, y2, x3, y3);
        }

        private static void DrawAntiVirusAgentCharacter(Graphics g, float x, float groundY, int facing, bool walking, double phase, bool aiming, bool deleteMode)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int dir = facing >= 0 ? 1 : -1;
            float walk = walking ? 1f : 0f;
            float s = walking ? (float)Math.Sin(phase) : 0f;
            float c = walking ? (float)Math.Cos(phase) : 1f;
            float bob = walking ? -Math.Abs(s) * 3.2f : 0f;
            float y = groundY + bob;
            Color jacket = Color.FromArgb(22, 76, 155);
            Color jacketLight = Color.FromArgb(48, 125, 220);
            Color outline = Color.FromArgb(18, 26, 38);

            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(105, 0, 0, 0)))
                g.FillEllipse(shadow, x - 38, groundY + 3, 76, 16);

            // Backpack behind body.
            RectangleF pack = new RectangleF(x - dir * 43 - 10, y - 92, 22, 58);
            using (LinearGradientBrush pb = new LinearGradientBrush(pack, Color.FromArgb(70, 110, 145), Color.FromArgb(24, 42, 64), 90f))
                g.FillRoundedRectangle(pb, pack, 5);
            using (Pen p = new Pen(outline, 2f)) g.DrawRoundedRectangle(p, pack, 5);
            using (SolidBrush led = new SolidBrush(Color.FromArgb(105, 255, 90)))
                g.FillRectangle(led, pack.X + pack.Width / 2 - 3, pack.Y + 7, 6, 12);

            // Legs first: two-segment walk with alternating feet.
            float hipY = y - 43;
            float kneeY = y - 22;
            float footY = groundY + 2;
            float leftFoot = -12f + dir * s * 12f;
            float rightFoot = 12f - dir * s * 12f;
            float leftKnee = -8f + dir * s * 5f;
            float rightKnee = 8f - dir * s * 5f;
            if (!walking)
            {
                leftFoot = -14f; rightFoot = 14f; leftKnee = -9f; rightKnee = 9f;
                kneeY = y - 22; footY = groundY + 1;
            }
            using (Pen pants = new Pen(Color.FromArgb(24, 28, 38), 7f))
            using (Pen pantsHi = new Pen(Color.FromArgb(52, 66, 88), 2f))
            {
                DrawLimb(g, pants, x - 8, hipY, x + leftKnee, kneeY, x + leftFoot, footY);
                DrawLimb(g, pants, x + 8, hipY, x + rightKnee, kneeY, x + rightFoot, footY);
                g.DrawLine(pantsHi, x + leftKnee - 1, kneeY, x + leftFoot - 1, footY);
                g.DrawLine(pantsHi, x + rightKnee - 1, kneeY, x + rightFoot - 1, footY);
            }
            using (SolidBrush shoe = new SolidBrush(Color.FromArgb(235, 242, 250)))
            using (Pen shoeLine = new Pen(Color.FromArgb(15, 36, 68), 2f))
            {
                RectangleF sh1 = new RectangleF(x + leftFoot - 11, footY - 2, 22, 8);
                RectangleF sh2 = new RectangleF(x + rightFoot - 11, footY - 2, 22, 8);
                g.FillRoundedRectangle(shoe, sh1, 4);
                g.FillRoundedRectangle(shoe, sh2, 4);
                g.DrawRoundedRectangle(shoeLine, sh1, 4);
                g.DrawRoundedRectangle(shoeLine, sh2, 4);
            }

            // Body.
            RectangleF body = new RectangleF(x - 19, y - 86, 38, 48);
            using (LinearGradientBrush jb = new LinearGradientBrush(body, jacketLight, jacket, 90f))
                g.FillRoundedRectangle(jb, body, 7);
            using (Pen p = new Pen(outline, 2.2f)) g.DrawRoundedRectangle(p, body, 7);
            using (SolidBrush stripe = new SolidBrush(Color.FromArgb(190, 125, 185, 255)))
            {
                g.FillRectangle(stripe, body.X + 5, body.Y + 8, 4, body.Height - 12);
                g.FillRectangle(stripe, body.Right - 9, body.Y + 8, 4, body.Height - 12);
            }
            using (SolidBrush badge = new SolidBrush(Color.White)) g.FillRoundedRectangle(badge, new RectangleF(x - 7, y - 64, 14, 18), 2);
            using (SolidBrush badgeCore = new SolidBrush(Color.FromArgb(75, 190, 100))) g.FillRectangle(badgeCore, x - 3, y - 58, 6, 5);

            // Arms. During walk arms swing opposite to legs. During attack, front arm is extended.
            float armSwing = walking ? -s * 12f : 0f;
            float rearSwing = walking ? s * 10f : 0f;
            using (Pen armBlue = new Pen(Color.FromArgb(22, 68, 145), 7f))
            using (Pen glove = new Pen(Color.FromArgb(235, 242, 250), 7f))
            using (Pen scanner = new Pen(deleteMode ? Color.FromArgb(190, 28, 28) : Color.FromArgb(45, 170, 70), 6f))
            {
                RoundCaps(armBlue); RoundCaps(glove); RoundCaps(scanner);
                if (aiming)
                {
                    // Rear arm lowered, front arm extended like firing the scanner.
                    g.DrawLine(armBlue, x - dir * 15, y - 76, x - dir * 22, y - 55);
                    g.DrawLine(glove, x - dir * 22, y - 55, x - dir * 25, y - 47);
                    g.DrawLine(armBlue, x + dir * 15, y - 76, x + dir * 41, y - 69);
                    g.DrawLine(glove, x + dir * 38, y - 69, x + dir * 50, y - 67);
                    g.DrawLine(scanner, x + dir * 48, y - 67, x + dir * 63, y - 67);
                    DrawTinyCross(g, x + dir * 62, y - 67, 4, deleteMode ? Color.FromArgb(255, 75, 70) : Color.FromArgb(120, 255, 100), 210);
                }
                else
                {
                    g.DrawLine(armBlue, x + dir * 15, y - 77, x + dir * (20 + armSwing), y - 55 + Math.Abs(s) * 2);
                    g.DrawLine(glove, x + dir * (20 + armSwing), y - 55 + Math.Abs(s) * 2, x + dir * (24 + armSwing), y - 47);
                    g.DrawLine(armBlue, x - dir * 15, y - 77, x - dir * (20 + rearSwing), y - 55 + Math.Abs(c) * 2);
                    g.DrawLine(glove, x - dir * (20 + rearSwing), y - 55 + Math.Abs(c) * 2, x - dir * (24 + rearSwing), y - 47);
                }
            }

            // Head and hair on top.
            using (SolidBrush skin = new SolidBrush(Color.FromArgb(246, 198, 142)))
                g.FillEllipse(skin, x - 14, y - 117, 28, 30);
            using (Pen p = new Pen(outline, 2f)) g.DrawEllipse(p, x - 14, y - 117, 28, 30);
            using (SolidBrush hair = new SolidBrush(Color.FromArgb(18, 22, 28)))
            {
                g.FillPie(hair, x - 18, y - 124, 36, 25, 180, 180);
                PointF[] bangs = new PointF[]
                {
                    new PointF(x - 17, y - 106), new PointF(x - 10, y - 118), new PointF(x - 5, y - 106),
                    new PointF(x + 1, y - 119), new PointF(x + 7, y - 106), new PointF(x + 15, y - 116), new PointF(x + 18, y - 104)
                };
                g.FillPolygon(hair, bangs);
            }
            using (SolidBrush eye = new SolidBrush(Color.FromArgb(24, 120, 80)))
            {
                float eyeOffset = dir > 0 ? 4f : -8f;
                g.FillRectangle(eye, x + eyeOffset, y - 103, 4, 5);
                if (!aiming) g.FillRectangle(eye, x - eyeOffset - 4, y - 103, 3, 5);
            }

            // Shoulder security emblem.
            using (SolidBrush shield = new SolidBrush(Color.FromArgb(225, 245, 255)))
            using (Pen sp = new Pen(Color.FromArgb(55, 130, 220), 1.5f))
            {
                RectangleF sr = new RectangleF(x - dir * 27 - 8, y - 82, 16, 18);
                g.FillEllipse(shield, sr);
                g.DrawEllipse(sp, sr);
                using (SolidBrush core = new SolidBrush(Color.FromArgb(75, 185, 95))) g.FillRectangle(core, sr.X + 6, sr.Y + 6, 4, 8);
            }
        }

        private static void DrawTinyCross(Graphics g, float x, float y, int size, Color color, int alpha)
        {
            using (Pen glow = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), color), Math.Max(2f, size * 0.7f)))
            using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha), Color.White), Math.Max(1.2f, size * 0.28f)))
            {
                RoundCaps(glow); RoundCaps(core);
                g.DrawLine(glow, x - size, y, x + size, y);
                g.DrawLine(glow, x, y - size, x, y + size);
                g.DrawLine(core, x - size, y, x + size, y);
                g.DrawLine(core, x, y - size, x, y + size);
            }
        }

        private static float EaseOut(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return 1f - (1f - t) * (1f - t);
        }

        private static void DrawCleanSprayBeam(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress)
        {
            int dir = x2 >= x1 ? 1 : -1;
            float dx = x2 - x1;
            float dy = y2 - y1;
            float visible = EaseOut(progress * 1.08f);
            float endX = x1 + dx * visible;
            float endY = y1 + dy * visible;
            Color green = Color.FromArgb(95, 255, 80);
            Color mint = Color.FromArgb(150, 255, 150);

            // 부드럽게 분사되는 부채꼴 안개. 시작부터 전체가 터지지 않고 길이가 점점 늘어납니다.
            PointF[] cone = new PointF[]
            {
                new PointF(x1, y1 - 5),
                new PointF(endX, endY - 34),
                new PointF(endX, endY + 34),
                new PointF(x1, y1 + 5)
            };
            using (SolidBrush haze = new SolidBrush(Color.FromArgb(ClampAlpha(alpha / 5), green))) g.FillPolygon(haze, cone);

            using (Pen stream = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), green), 4.2f))
            using (Pen stream2 = new Pen(Color.FromArgb(ClampAlpha(alpha / 3), mint), 2.4f))
            using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), Color.White), 1.2f))
            {
                RoundCaps(stream); RoundCaps(stream2); RoundCaps(core);
                for (int i = 0; i < 2; i++)
                {
                    float off = (i - 0.5f) * 10f;
                    float wave = (float)Math.Sin(progress * 7.5f + i * 1.3f) * 9f;
                    float sx = x1;
                    float sy = y1 + off * 0.18f;
                    float ex = endX;
                    float ey = endY + off * 0.42f;
                    g.DrawBezier(stream, sx, sy, x1 + dx * 0.24f * visible, y1 + off + wave, x1 + dx * 0.68f * visible, y2 - off - wave, ex, ey);
                    g.DrawBezier(stream2, sx, sy, x1 + dx * 0.30f * visible, y1 + off - wave, x1 + dx * 0.72f * visible, y2 + off + wave, ex, ey);
                    if (i == 1) g.DrawLine(core, x1, y1, endX, endY);
                }
            }

            // 녹색 십자가가 “뾰롱뾰롱” 순서대로 생기도록 birth time을 나누어 줍니다.
            int count = 28;
            for (int i = 0; i < count; i++)
            {
                float birth = i / (float)count * 0.94f;
                float life = (progress - birth) / 0.28f;
                if (life < 0f || life > 1f) continue;
                float t = birth + life * 0.18f;
                if (t > visible) t = visible;
                float px = x1 + dx * t;
                float spread = (float)Math.Sin(i * 1.71 + progress * 8.0f) * (10f + 22f * t);
                float py = y1 + dy * t + spread;
                int a = ClampAlpha((int)(alpha * (1f - life * 0.75f)));
                int size = 4 + (i % 4);
                DrawTinyCross(g, px, py, size, i % 2 == 0 ? green : mint, a);

                // 작은 반짝이 점을 함께 흘려서 분사감 보강.
                if (i % 2 == 0)
                {
                    using (SolidBrush dot = new SolidBrush(Color.FromArgb(ClampAlpha(a / 2), Color.White)))
                        g.FillEllipse(dot, px + dir * (7 + i % 4), py - 3, 3, 3);
                }
            }
        }

        private static void DrawDeleteBeam(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress)
        {
            int dir = x2 >= x1 ? 1 : -1;
            float dx = x2 - x1;
            float dy = y2 - y1;
            float visible = EaseOut(progress * 1.2f);
            float endX = x1 + dx * visible;
            float endY = y1 + dy * visible;
            PointF[] fan = new PointF[]
            {
                new PointF(x1, y1 - 8), new PointF(endX, endY - 32), new PointF(endX, endY + 32), new PointF(x1, y1 + 8)
            };
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha / 4), color))) g.FillPolygon(glow, fan);
            using (Pen beam = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), color), 11f))
            using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha), Color.White), 2.2f))
            {
                RoundCaps(beam); RoundCaps(core);
                g.DrawLine(beam, x1, y1, endX, endY);
                g.DrawLine(core, x1, y1, endX, endY);
            }
            for (int i = 0; i < 26; i++)
            {
                float t = (i / 26f + progress * 0.22f) % 1f;
                if (t > visible) continue;
                float px = x1 + dx * t;
                float py = y1 + dy * t + (float)Math.Sin(i * 1.9 + progress * 8) * 18f;
                int sz = 3 + i % 4;
                using (SolidBrush rb = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - i * 3), color)))
                    g.FillRectangle(rb, px - sz / 2, py - sz / 2, sz, sz);
            }
        }
        public static void DrawEnemy(Graphics g, GameEntity e, float cameraX)
        {
            RectangleF b = e.Bounds;
            Rectangle r = Rectangle.Round(new RectangleF(b.X - cameraX, b.Y, b.Width, b.Height));

            // 그림자 연산
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                g.FillEllipse(sh, r.X + 4, r.Bottom - 10, r.Width - 8, 12);

            if (e.IsBoss)
            {
                DrawBoss(g, r, e);
                return;
            }
            else
            {
                // 3x3 중 딱 1칸만 오려 그리는 형진님의 완성형 렌더러 호출
                DrawFileMonster(g, r, e);
            }

            // ---------------------------------------------------------------------------------
            // 💡 [이름 & 체력바 위치 보정]: 145x120으로 커진 형진님의 몬스터 규격에 맞춰 
            // 레이아웃 좌표(UI가 머리 위에 이쁘게 안착하도록 Y축 - 56 보정)를 정교하게 재계산합니다!
            // ---------------------------------------------------------------------------------
            int uiX = r.X + r.Width / 2;
            int uiY = r.Y + r.Height / 2 - 60; // 몬스터 머리 위 정중앙 포지셔닝

            // 1. 머리 위 체력바 복구
            Rectangle hp = new Rectangle(uiX - 32, uiY, 64, 8);
            DrawBar(g, hp, e.Hp, e.MaxHp, Color.OrangeRed);

            // 2. 머리 위 이름(Name) 텍스트 복구
            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                Rectangle nameRect = new Rectangle(uiX - 50, uiY - 20, 100, 16);
                g.FillRectangle(bg, nameRect);

                // 기획안 명세에 맞게 몬스터 고유 이름(e.Name)을 화면에 출력합니다.
                g.DrawString(e.Name, f, sb, nameRect, Center());
            }
        }

        public static void DrawWeaponUpgradeFile(Graphics g, WeaponUpgradeFile drop, float cameraX)
        {
            RectangleF b = drop.Bounds;
            Rectangle r = Rectangle.Round(new RectangleF(b.X - cameraX, b.Y, b.Width, b.Height));

            int iconSize = drop.Dragging ? 68 : 60;

            Rectangle iconRect = new Rectangle(
                (int)(drop.X - cameraX - iconSize / 2),
                (int)(drop.Y - 38),
                iconSize,
                iconSize
            );

            int glowAlpha = drop.Dragging ? 120 : 75;

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(glowAlpha, 80, 190, 255)))
                g.FillEllipse(glow, iconRect.X - 14, iconRect.Y - 10, iconRect.Width + 28, iconRect.Height + 20);

            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(85, 0, 0, 0)))
                g.FillEllipse(shadow, iconRect.X + 8, iconRect.Bottom - 8, iconRect.Width - 16, 12);

            DesktopIconUI.Shared.DrawIconOnly(g, 2, 3, iconRect);

            Rectangle levelBox = new Rectangle(
                iconRect.Right - 24,
                iconRect.Bottom - 20,
                24,
                18
            );

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(165, 0, 0, 0)))
                g.FillRectangle(bg, levelBox);

            using (Font f = F(8f, FontStyle.Bold))
            using (SolidBrush text = new SolidBrush(Color.White))
                g.DrawString("+" + drop.UpgradeLevel, f, text, levelBox, Center());

            Rectangle nameBox = new Rectangle(
                iconRect.X - 24,
                iconRect.Bottom + 2,
                iconRect.Width + 48,
                18
            );

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(135, 0, 0, 0)))
                g.FillRectangle(bg, nameBox);

            using (Font f = F(7.5f, FontStyle.Bold))
            using (SolidBrush text = new SolidBrush(Color.White))
                g.DrawString("Weapon Patch", f, text, nameBox, Center());
        }
        private static void DrawFallbackFileMonster(Graphics g, Rectangle r, GameEntity e)
        {
            if (e.HitFlash > 0)
            {
                using (SolidBrush flash = new SolidBrush(Color.FromArgb(120, Color.White)))
                    g.FillEllipse(flash, r.X - 8, r.Y - 8, r.Width + 16, r.Height + 16);
            }

            Rectangle icon = new Rectangle(r.X + r.Width / 2 - 25, r.Y + 8, 50, 55);

            using (LinearGradientBrush b = new LinearGradientBrush(icon, Color.White, Color.FromArgb(225, 232, 246), 90f))
                g.FillRectangle(b, icon);

            using (Pen p = new Pen(e.Color, 2f))
                g.DrawRectangle(p, icon.X, icon.Y, icon.Width - 1, icon.Height - 1);

            using (SolidBrush b = new SolidBrush(Color.FromArgb(54, e.Color)))
                g.FillRectangle(b, icon.X + 8, icon.Y + 14, icon.Width - 16, 15);

            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Darken(e.Color, 30)))
                g.DrawString(e.Kind, f, sb, new Rectangle(icon.X + 3, icon.Y + 33, icon.Width - 6, 16), Center());

            using (SolidBrush eye = new SolidBrush(Color.Red))
            {
                g.FillEllipse(eye, icon.X + 12, icon.Y + 9, 5, 5);
                g.FillEllipse(eye, icon.Right - 17, icon.Y + 9, 5, 5);
            }
        }

        private static void DrawBoss(Graphics g, Rectangle r, GameEntity e)
        {
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(55, e.Color))) g.FillEllipse(aura, r.X - 20, r.Y - 20, r.Width + 40, r.Height + 40);
            // ==========================================================
            // 1번 보스: Driver-K 
            // ==========================================================         
            if (e.Name.Contains("Driver-K"))
            {
                if (Renderer.ImgBoss_DriverK != null)
                {
                    int cols = 4;
                    int rows = 2;
                    int fW = Renderer.ImgBoss_DriverK.Width / cols;
                    int fH = Renderer.ImgBoss_DriverK.Height / rows;

                    int fY = 0;
                    int fX = 0;

                    // 특수 기믹 패턴(75%, 50%, 25%) 시전 중일 때 무조건 7번 사진!
                    if (e.IsCastingPattern)
                    {
                        fY = 1; // 2번째 줄
                        fX = 2; // 3번째 칸 (즉, 7번 사진)
                    }
                    else
                    {
                        // 💡 대기 상태: 플레이어 위치에 따라 1번 / 4번 사진 교체
                        if (e.Facing == -1)
                        {
                            fY = 0;
                            fX = 3; // 4번 사진 (왼쪽 대기)
                        }
                        else
                        {
                            fY = 0;
                            fX = 0; // 1번 사진 (오른쪽 대기)
                        }
                    }

                    Rectangle srcRect = new Rectangle(fX * fW, fY * fH, fW, fH);
                    int centerX = r.X + r.Width / 2;

                    // r(Rectangle) 변수를 사용하여 보스의 위치를 잡습니다.
                    Rectangle destRect = new Rectangle(centerX - 170, r.Bottom - 300, 340, 340);

                    g.DrawImage(Renderer.ImgBoss_DriverK, destRect, srcRect, GraphicsUnit.Pixel);
                }
                return;
            }
            // ==========================================================
            // 2번 보스: High-Kernel 
            // ==========================================================
            if (e.Name.Contains("High-Kernel"))
            {
                if (Renderer.ImgBoss_HighKernel != null)
                {
                    int cols = 4; // 1행 4열 가로 배치 기준
                    int rows = 1;
                    int fW = Renderer.ImgBoss_HighKernel.Width / cols;
                    int fH = Renderer.ImgBoss_HighKernel.Height / rows;

                    int fX = 0;
                    int fY = 0;

                    // 1. 특수 패턴 시전 중일 때: 무조건 4번 사진 (인덱스 3)
                    if (e.IsCastingPattern)
                    {
                        fX = 3;
                        fY = 0;
                    }
                    else
                    {
                        // 2. 평상시 대기 상태: 플레이어 방향 추적
                        if (e.Facing == -1)
                        {
                            fX = 1; // 왼쪽에 있으면 2번 사진 (인덱스 1)
                            fY = 0;
                        }
                        else
                        {
                            fX = 0; // 오른쪽에 있으면 1번 사진 (인덱스 0)
                            fY = 0;
                        }
                    }

                    Rectangle srcRect = new Rectangle(fX * fW, fY * fH, fW, fH);
                    int centerX = r.X + r.Width / 2;

                    // High-Kernel의 인게임 위상에 맞춘 드로우 박스 (크기 조정 필요시 340 숫자 변경)
                    Rectangle destRect = new Rectangle(centerX - 170, r.Bottom - 300, 340, 340);

                    g.DrawImage(Renderer.ImgBoss_HighKernel, destRect, srcRect, GraphicsUnit.Pixel);
                }
                return;
            }
            // ==========================================================
            // 3번 보스: BSOD 렌더링 수정 (위아래 중 위쪽 줄만 사용)
            // ==========================================================
            if (e.Name.Contains("BSOD"))
            {
                if (Renderer.ImgBoss_BSOD != null)
                {
                    // 💡 [핵심 수정] 올려주신 이미지 파일은 물리적으로 가로로 4칸(cols), 세로로 2칸(rows) 배치되어 있습니다.
                    // 위아래가 동시에 나왔던 이유는 기존 rows가 1이어서 frame height가 두 배로 크게 계산되었기 때문입니다.
                    int cols = 4; // 가로 4칸
                    int rows = 2; // 💡 [핵심 수정] 물리적인 세로줄 개수를 2로 명시합니다. (위쪽 줄, 아래쪽 줄)
                    int fW = Renderer.ImgBoss_BSOD.Width / cols;
                    int fH = Renderer.ImgBoss_BSOD.Height / rows;

                    // 유저님의 매핑 요청: 0번, 1번, 3번 인덱스는 모두 '위쪽 줄'에 위치한다고 가정합니다.
                    int targetIndex = 0;

                    // 1. 특수 기믹 시전 중일 때: 무조건 1번 인덱스 사진 고정
                    if (e.IsCastingPattern)
                    {
                        targetIndex = 1;
                    }
                    else
                    {
                        // 2. 평상시 대기 상태: 플레이어 위치에 따른 인덱스 분기
                        if (e.Facing == -1)
                        {
                            targetIndex = 3; // 플레이어가 왼쪽에 있으면 3번 인덱스
                        }
                        else
                        {
                            targetIndex = 0; // 플레이어가 오른쪽에 있으면 0번 인덱스
                        }
                    }

                    // 💡 [자동화 공식 해설]
                    // targetIndex가 0, 1, 3 중 하나라면 
                    // fX = 0, 1, 3 (순서대로)
                    // fY = targetIndex / cols = (0/4), (1/4), (3/4) = 모두 정수 결과값은 0이 됩니다.
                    // fY가 0으로 고정되므로, srcRect는 무조건 이미지의 '위쪽 줄' 영역만 잘라냅니다.
                    int fX = targetIndex % cols;
                    int fY = targetIndex / cols;

                    // 정확히 계산된 fY 좌표를 사용하여 위쪽 줄의 프레임만 잘라냅니다.
                    Rectangle srcRect = new Rectangle(fX * fW, fY * fH, fW, fH);
                    int centerX = r.X + r.Width / 2;
                    Rectangle destRect = new Rectangle(centerX - 170, r.Bottom - 300, 340, 340);

                    g.DrawImage(Renderer.ImgBoss_BSOD, destRect, srcRect, GraphicsUnit.Pixel);
                }
                return;
            }
            // ==========================================================
            // 4번 보스: Exception Queen [크기 1.5배 축소 및 바닥선 정렬 완벽 반영]
            // ==========================================================
            if (e.Name.Contains("Exception Queen") || e.Name.Contains("Exception_Queen"))
            {
                if (Renderer.ImgBoss_ExceptionQueen != null)
                {
                    int totalH = Renderer.ImgBoss_ExceptionQueen.Height; // 686

                    // 유저님이 지정해주신 최적의 슬라이싱 픽셀 수치 그대로 고정
                    int srcX = 0;
                    int srcY = 0;
                    int srcW = 800;
                    int srcH = totalH;

                    if (e.Facing == -1)
                    {
                        // 👈 [플레이어가 왼쪽에 있을 때] -> 왼쪽 대기 칸 출력
                        srcX = 800 + 10;
                        srcW = 790;
                    }
                    else
                    {
                        // 👈 [플레이어가 오른쪽에 있을 때] -> 오른쪽 대기 칸 출력
                        srcX = 0;
                        srcW = 800;
                    }

                    Rectangle srcRect = new Rectangle(srcX, srcY, srcW, srcH);
                    int centerX = r.X + r.Width / 2;

                    // 💡 [핵심: 1.5배 축소 연산] 기존 580px 크기를 1.5로 나누어 386px 규격으로 조정합니다.
                    int destW = 386;
                    int destH = 356;

                    // 💡 크기가 작아진 만큼 발바닥 위치가 공중에 뜨지 않도록 (r.Bottom - destH - 40) 공식으로 지면에 밀착시킵니다.
                    Rectangle destRect = new Rectangle(centerX - (destW / 2), r.Bottom - destH - 40, destW, destH);

                    g.DrawImage(Renderer.ImgBoss_ExceptionQueen, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    using (Font f = F(11f, FontStyle.Bold))
                    using (SolidBrush sb = new SolidBrush(Color.Red))
                    {
                        g.DrawString("⚠️ Exception_Queen 로드 실패!\n(Resource/boss/Exception_Queen 파일 경로 확인)", f, sb, r, Center());
                    }
                }
                return;
            }

            // ==========================================================
            // 5번 보스: Illegal_Binny 
            // ==========================================================
            if (e.Name.Contains("Illegal_Binny"))
            {
                if (Renderer.ImgBoss_IllegalBinny != null)
                {
                    int totalH = Renderer.ImgBoss_IllegalBinny.Height; // 543
                    int frameW = 521; // 1563 / 3

                    int srcX = 0;
                    int srcY = 0;
                    int srcW = frameW;
                    int srcH = totalH;

                    // [조절 포인트] 모션 인덱스별 정밀 슬라이싱 (유저 제공 코드 유지)
                    if (e.MotionIndex == 0) // 오른쪽 바라봄
                    {
                        srcX = 0;      // 시작 위치
                        srcW = 450;    // 가로 폭
                    }
                    else if (e.MotionIndex == 1) // 왼쪽 바라봄
                    {
                        srcX = frameW - 50;
                        srcW = 511;
                    }
                    else // 인덱스 2번: 특수 기믹
                    {
                        srcX = frameW * 2;
                        srcW = 521;
                    }

                    Rectangle srcRect = new Rectangle(srcX, srcY, srcW, srcH);
                    int centerX = r.X + r.Width / 2;

                    // ==========================================================
                    // 💡 [수정 포인트 1] 보스 크기 30% 축소 연산
                    // 기존: W 347, H 360 -> 수정: 기존값 * 0.7
                    // ==========================================================
                    int destW = 243; // (int)(347 * 0.7f)
                    int destH = 252; // (int)(360 * 0.7f)

                    // 축소된 크기에 맞춰 바닥 정렬 좌표 재계산
                    Rectangle destRect = new Rectangle(centerX - (destW / 2), r.Bottom - destH - 40, destW, destH);

                    // 1. 축소된 보스 본체 그리기
                    g.DrawImage(Renderer.ImgBoss_IllegalBinny, destRect, srcRect, GraphicsUnit.Pixel);

                    // ==========================================================
                    // 💡 [수정 포인트 2] 검 크기 변경 (보스 키의 절반) 및 재배치
                    // ==========================================================
                    if (Renderer.Img_IceSword != null && Renderer.Img_FireSword != null && Renderer.Img_LightningSword != null)
                    {
                        // 보스 현재 키(destH=252)의 절반(0.5)으로 검의 키를 설정
                        int swordH = 252; // 252 / 2

                        // 원본 PNG 비율(408x1419)을 유지하기 위한 가로폭 역산 (126 * 408 / 1419)
                        int swordW = 72;

                        // 검이 커진 만큼 머리 위 공중 정렬 좌표(Y) 세밀 조정
                        int swordY = destRect.Y - swordH - 2; // 간격을 20px로 약간 넓힘

                        // [얼음 - 화염 - 번개] 순으로 배치 (간격 55px로 확정)
                        g.DrawImage(Renderer.Img_IceSword, centerX - 95 - swordW / 2, swordY, swordW, swordH);       // 왼쪽
                        g.DrawImage(Renderer.Img_FireSword, centerX - swordW / 2, swordY, swordW, swordH);            // 중앙
                        g.DrawImage(Renderer.Img_LightningSword, centerX + 95 - swordW / 2, swordY, swordW, swordH);  // 오른쪽
                    }
                    // ==========================================================
                }
                else
                {
                    // (유저 제공 에러 예외 처리 코드 유지)
                    using (Font f = F(11f, FontStyle.Bold))
                    using (SolidBrush sb = new SolidBrush(Color.Red))
                    {
                        g.DrawString("⚠️ Illegal_Binny 로드 실패!\n(Resource/boss/Illegal_Binny.jpg 확인)", f, sb, r, Center());
                    }
                }
                return;
            }
        }

        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX)
        {
            DrawStageBackground(g, client, st, cameraX, false);
        }

        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX, bool bossRoom)
        {
            DrawStageBackground(g, client, st, cameraX, bossRoom, client.Width);
        }

        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX, bool bossRoom, int mapWidth)
        {
            if (st == null || client.Width <= 0 || client.Height <= 0) return;
            int virtualWidth = Math.Max(client.Width, mapWidth);
            string key = st.Index.ToString() + "_" + (bossRoom ? "boss_" : "normal_") + virtualWidth.ToString() + "x" + client.Height.ToString();
            Bitmap cached;
            if (!stageBackgroundCache.TryGetValue(key, out cached))
            {
                cached = new Bitmap(virtualWidth, client.Height);
                using (Graphics cg = Graphics.FromImage(cached))
                {
                    cg.SmoothingMode = SmoothingMode.HighSpeed;
                    cg.CompositingQuality = CompositingQuality.HighSpeed;
                    cg.InterpolationMode = InterpolationMode.Low;
                    cg.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    DrawStageBackgroundCore(cg, new Rectangle(0, 0, virtualWidth, client.Height), st, 0, bossRoom);
                }
                stageBackgroundCache[key] = cached;
            }
            int scrollX = (int)Math.Max(0, Math.Min(cameraX, virtualWidth - client.Width));
            Rectangle source = new Rectangle(scrollX, 0, client.Width, client.Height);
            g.DrawImage(cached, client, source, GraphicsUnit.Pixel);
        }

        private static void DrawStageBackgroundCore(Graphics g, Rectangle client, StageInfo st, float cameraX, bool bossRoom)
        {
            Image stageImage = LoadStageBackgroundImage(st.Index, bossRoom);
            if (stageImage != null)
            {
                DrawImageCover(g, stageImage, client);

                using (LinearGradientBrush shade = new LinearGradientBrush(client, Color.FromArgb(18, 0, 0, 0), Color.FromArgb(42, 0, 0, 0), 90f))
                    g.FillRectangle(shade, client);

                using (SolidBrush topGlow = new SolidBrush(Color.FromArgb(30, st.Accent)))
                    g.FillEllipse(topGlow, client.Width / 2 - 360, 20, 720, 180);

                if (st.Index == 1 && !bossRoom)
                {
                    DesktopIconUI.Shared.DrawFixedDesktopIcons(g, client);
                }

                //DrawXPTaskbar(g, client, bossRoom ? st.Name + " - 보스방" : st.Name);
                return;
            }
            if (st.Index == 1)
            {
                DrawXPWallpaper(g, client);
                DesktopIconUI.Shared.DrawFixedDesktopIcons(g, client);
                return;
            }
            using (LinearGradientBrush b = new LinearGradientBrush(client, Darken(st.BackColor, 22), Lighten(st.BackColor, 34), 90f)) g.FillRectangle(b, client);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(48, st.Accent)))
            {
                g.FillEllipse(glow, client.Width / 2 - 400, 60, 800, 240);
                g.FillEllipse(glow, client.Width - 360, client.Height - 300, 520, 280);
            }
            Rectangle window = new Rectangle(80, 70, client.Width - 160, client.Height - 150);
            string title = st.Name;
            if (st.Index == 2) title = "Driver Vault - Device Manager";
            else if (st.Index == 3) title = "Windows Update Lab";
            else if (st.Index == 4) title = "C:\\WINDOWS\\system32";
            else if (st.Index == 5) title = "Network Connections - Port Harbor";
            else if (st.Index == 6) title = "System Stability Check";
            else if (st.Index == 7) title = "Registry Editor - Hive Archive";
            else if (st.Index == 8) title = "Exception Report - Popup Error Maze";
            else if (st.Index == 9) title = "Temp Cache - Local Settings\\Temp";
            else if (st.Index == 10) title = "Recycle Bin Dungeon";
            DrawXPWindow(g, window, title, st.Index == 8 || st.Index == 10);
            Rectangle content = new Rectangle(window.X + 16, window.Y + 48, window.Width - 32, window.Height - 58);
            if (st.Index == 2) DrawDeviceManagerField(g, content);
            else if (st.Index == 3) DrawUpdateField(g, content);
            else if (st.Index == 4) DrawSystem32Field(g, content);
            else if (st.Index == 5) DrawNetworkField(g, content);
            else if (st.Index == 6) DrawBsodField(g, content);
            else if (st.Index == 7) DrawRegistryField(g, content);
            else if (st.Index == 8) DrawPopupField(g, content);
            else if (st.Index == 9) DrawTempField(g, content);
            else if (st.Index == 10) DrawRecycleField(g, content);
        }

        private static void DrawDesktopIcon(Graphics g, string label, int x, int y, Color c)
        {
            Rectangle icon = new Rectangle(x + 18, y, 48, 48);
            using (LinearGradientBrush b = new LinearGradientBrush(icon, Color.White, c, 90f)) g.FillRectangle(b, icon);
            using (Pen p = new Pen(Color.FromArgb(40, 80, 160), 2f)) g.DrawRectangle(p, icon.X, icon.Y, icon.Width - 1, icon.Height - 1);
            using (Font f = F(8f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
            {
                Rectangle lab = new Rectangle(x, y + 52, 86, 32);
                g.FillRectangle(bg, lab);
                g.DrawString(label, f, sb, lab, Center());
            }
        }

        private static void DrawDeviceManagerField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 247, 250))) g.FillRectangle(b, r);
            using (Font f = F(9f, FontStyle.Regular))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(30, 40, 60)))
            {
                string[] lines = { "+ Display Adapter", "+ Sound Driver", "+ USB Controller     ⚠", "+ Network Adapter", "+ Unknown Device     ⚠", "+ Legacy Device      ⚠" };
                for (int i = 0; i < lines.Length; i++) g.DrawString(lines[i], f, sb, r.X + 24, r.Y + 18 + i * 28);
            }
        }

        private static void DrawUpdateField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 242, 255))) g.FillRectangle(b, r);
            Rectangle pr = new Rectangle(r.X + 90, r.Y + 70, r.Width - 180, 28);
            DrawBar(g, pr, 67, 100, Color.FromArgb(50, 135, 245));
            using (Font f = F(20f, FontStyle.Bold)) g.DrawString("Windows Update", f, Brushes.Navy, new Rectangle(r.X, r.Y + 20, r.Width, 40), Center());
            using (Font f = F(10f, FontStyle.Bold)) g.DrawString("Installing updates... 67%", f, Brushes.DarkBlue, new Rectangle(r.X, r.Y + 112, r.Width, 22), Center());
        }

        private static void DrawSystem32Field(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 245, 248))) g.FillRectangle(b, r);
            using (Font f = F(10f, FontStyle.Regular))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(60, 50, 55)))
            {
                string[] files = { "kernel32.dll", "ntoskrnl.exe", "drivers", "config", "protected.dat", "system.map" };
                for (int i = 0; i < files.Length; i++) g.DrawString(files[i], f, sb, r.X + 30 + (i % 2) * 220, r.Y + 30 + (i / 2) * 48);
            }
            using (Font f = F(16f, FontStyle.Bold)) g.DrawString("PROTECTED AREA", f, Brushes.DarkRed, new Rectangle(r.X, r.Bottom - 70, r.Width, 40), Center());
        }

        private static void DrawNetworkField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 244, 250))) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(40, 140, 200), 4f))
            {
                for (int i = 0; i < 7; i++) g.DrawBezier(p, r.X + 40, r.Y + 40 + i * 36, r.X + 250, r.Y + 15 + i * 20, r.Right - 260, r.Y + 80 + i * 22, r.Right - 40, r.Y + 40 + i * 36);
            }
            using (Font f = F(11f, FontStyle.Bold)) g.DrawString("PORT 80  PORT 443  PORT 404", f, Brushes.DarkBlue, new Rectangle(r.X, r.Y + 20, r.Width, 24), Center());
        }

        private static void DrawBsodField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 40, 180))) g.FillRectangle(b, r);
            using (Font f = F(16f, FontStyle.Bold)) g.DrawString("A problem has been detected and Windows has been shut down to prevent damage", f, Brushes.White, new Rectangle(r.X + 30, r.Y + 28, r.Width - 60, 44), Center());
            using (Font f = F(12f, FontStyle.Bold))
            {
                for (int i = 0; i < 8; i++) g.DrawString("STOP: 0x000000" + (7 + i).ToString("X"), f, Brushes.White, r.X + 70, r.Y + 100 + i * 32);
            }
        }

        private static void DrawRegistryField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(248, 247, 250))) g.FillRectangle(b, r);
            using (Font f = F(9f, FontStyle.Regular))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(45, 35, 70)))
            {
                string[] keys = { "HKEY_CLASSES_ROOT", "HKEY_CURRENT_USER", "HKEY_LOCAL_MACHINE", "RecentActions", "ProfileName", "LastSession" };
                for (int i = 0; i < keys.Length; i++) g.DrawString((i < 3 ? "+ " : "   └ ") + keys[i], f, sb, r.X + 28, r.Y + 20 + i * 32);
            }
        }

        private static void DrawPopupField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(238, 238, 246))) g.FillRectangle(b, r);
            for (int i = 0; i < 8; i++)
            {
                Rectangle pop = new Rectangle(r.X + 40 + i * 38, r.Y + 38 + (i % 3) * 48, 220, 90);
                DrawXPWindow(g, pop, "Error", true);
                using (Font f = F(8f, FontStyle.Bold)) g.DrawString("Unhandled exception", f, Brushes.DarkRed, new Rectangle(pop.X + 16, pop.Y + 40, pop.Width - 32, 20), Center());
            }
        }

        private static void DrawTempField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(238, 232, 210))) g.FillRectangle(b, r);
            using (Font f = F(10f, FontStyle.Bold))
            {
                string[] names = { "TEMP_01.tmp", "UNSENT_REPORT.tmp", "recent.cache", "profile.cache", "cache_heap.bin", "thumbnail.db" };
                for (int i = 0; i < names.Length; i++) g.DrawString(names[i], f, Brushes.SaddleBrown, r.X + 32 + (i % 3) * 220, r.Y + 35 + (i / 3) * 90);
            }
        }

        private static void DrawRecycleField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 232, 220))) g.FillRectangle(b, r);
            using (Font f = F(10f, FontStyle.Bold))
            {
                string[] names = { "Driver-K.log", "High-Kernel.protect", "BSOD_dump.tmp", "ExceptionQueen.err", "UNSENT_REPORT.tmp", "Illegal_Binny.dat" };
                for (int i = 0; i < names.Length; i++) g.DrawString(names[i], f, Brushes.DarkGreen, r.X + 32 + (i % 2) * 320, r.Y + 35 + (i / 2) * 60);
            }
        }

        private static void DrawSwordSlash(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress, string text)
        {
            int dir = x2 >= x1 ? 1 : -1;
            float handX = x1 + dir * 40f;
            float handY = y1 - 62f;
            float reach = Math.Min(138f, Math.Max(92f, Math.Abs(x2 - x1) * 0.72f));
            float centerX = handX + dir * 48f;
            float centerY = handY + 16f;
            RectangleF arcBounds = new RectangleF(centerX - reach * 0.55f, centerY - reach * 0.48f, reach * 1.1f, reach * 0.96f);

            float sweep = dir > 0 ? 148f : -148f;
            float start = dir > 0 ? -112f + progress * 28f : 112f - progress * 28f;
            Color slashOuter = Color.FromArgb(alpha, 245, 250, 255);
            Color slashInner = Color.FromArgb(ClampAlpha(alpha + 20), Color.FromArgb(255, 224, 92));
            using (Pen glow = new Pen(Color.FromArgb(ClampAlpha(alpha / 3), color), 18f))
            using (Pen trail = new Pen(Color.FromArgb(ClampAlpha(alpha - 35), slashOuter), 8f))
            using (Pen core = new Pen(slashInner, 3f))
            {
                RoundCaps(glow);
                RoundCaps(trail);
                RoundCaps(core);
                g.DrawArc(glow, arcBounds, start, sweep);
                g.DrawArc(trail, arcBounds, start + dir * 8f, sweep * 0.82f);
                g.DrawArc(core, arcBounds, start + dir * 18f, sweep * 0.54f);
            }

            double bladeAngle = (-68.0 + progress * 136.0) * Math.PI / 180.0;
            float tipX = handX + dir * (float)Math.Cos(bladeAngle) * 82f;
            float tipY = handY + (float)Math.Sin(bladeAngle) * 82f;
            float guardX = handX - dir * 8f;
            float guardY = handY + 8f;
            using (Pen bladeShadow = new Pen(Color.FromArgb(ClampAlpha(alpha - 75), 25, 35, 48), 7f))
            using (Pen blade = new Pen(Color.FromArgb(alpha, 235, 242, 252), 4f))
            using (Pen edge = new Pen(Color.FromArgb(ClampAlpha(alpha + 10), Color.White), 1.5f))
            using (Pen hilt = new Pen(Color.FromArgb(alpha, 88, 56, 28), 5f))
            {
                RoundCaps(bladeShadow);
                RoundCaps(blade);
                RoundCaps(edge);
                RoundCaps(hilt);
                g.DrawLine(bladeShadow, guardX, guardY, tipX, tipY);
                g.DrawLine(blade, guardX, guardY, tipX, tipY);
                g.DrawLine(edge, guardX + dir * 2f, guardY - 2f, tipX, tipY);
                g.DrawLine(hilt, handX - dir * 15f, handY + 18f, handX + dir * 10f, handY + 3f);
            }

            if (!string.IsNullOrEmpty(text))
            {
                using (Font f = F(10f, FontStyle.Bold))
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 55), 0, 0, 0)))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, slashInner)))
                {
                    RectangleF rr = new RectangleF(centerX + dir * 18f - 45f, centerY - 82f, 90f, 26f);
                    g.DrawString(text, f, shadow, new RectangleF(rr.X + 1, rr.Y + 1, rr.Width, rr.Height), Center());
                    g.DrawString(text, f, b, rr, Center());
                }
            }
        }

        

        public static void DrawEffect(Graphics g, Effect e, float cameraX)
        {
            int alpha = ClampAlpha((int)(235 * e.Ticks / (float)Math.Max(1, e.MaxTicks)));
            float progress = 1f - e.Ticks / (float)Math.Max(1, e.MaxTicks);
            float x1 = e.X - cameraX;
            float x2 = e.X2 - cameraX;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (e.Kind == "binnyIce" || e.Kind == "binnyFire" || e.Kind == "binnyLight")
            {
                Image swordImg = null;
                if (e.Kind == "binnyIce") swordImg = Renderer.Img_IceSword;
                else if (e.Kind == "binnyFire") swordImg = Renderer.Img_FireSword;
                else if (e.Kind == "binnyLight") swordImg = Renderer.Img_LightningSword;

                if (swordImg != null)
                {
                    System.Drawing.Drawing2D.GraphicsState state = g.Save();

                    // 2번 보스 메테오처럼 하늘에서 쾅 내려찍는 종적 가속 낙하 수식
                    float targetY = e.Y;
                    float currentY = targetY;
                    if (progress < 0.25f) // 효과 시작 후 25% 시간동안 하늘 위(-400)에서 지면까지 초고속 하강
                    {
                        float dropRatio = progress / 0.25f;
                        currentY = (targetY - 400f) + (400f * dropRatio);
                    }

                    // 칼날 끝이 바닥을 향하도록 무기 타겟 좌표 이동 후 180도 대반전 회전 매트릭스 가동
                    g.TranslateTransform(x1, currentY);
                    g.RotateTransform(180f);

                    // 단일 해상도 원본 비례(408x1419)를 무너뜨리지 않는 축소 크기 할당 (가로 45, 세로 150)
                    int sw = 45;
                    int sh = 150;

                    // 스킬 소멸 틱 타임(alpha 계수)에 맞춰 잔상이 부드럽게 투명 블렌딩 아웃되도록 세팅
                    using (System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes())
                    {
                        System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha / 255f };
                        ia.SetColorMatrix(cm);

                        // 회전 중심 정렬 좌표를 보정하기 위해 중심 오프셋 반감 매핑 드로우
                        g.DrawImage(swordImg, new Rectangle(-sw / 2, 0, sw, sh), 0, 0, swordImg.Width, swordImg.Height, GraphicsUnit.Pixel, ia);
                    }

                    g.Restore(state);
                }
                return; // 기본 공격 드로우 처리가 끝났으므로 즉시 종료
            }

            if (e.Kind == "driveShard")
            {
                if (Renderer.Img_DiskSprite != null)
                {
                    // 1. 투사체의 현재 월드 좌표 실시간 계산
                    float visibleProgress = EaseOut(1f - e.Ticks / (float)Math.Max(1, e.MaxTicks));
                    float dx = e.X2 - e.X; float dy = e.Y2 - e.Y;
                    float drawX = (e.X + dx * visibleProgress) - cameraX;
                    float drawY = e.Y + dy * visibleProgress;

                    // 2. 10등분 애니메이션 적용 (틱 타임에 따라 0~9번 프레임 순환)
                    int frameIndex = (Environment.TickCount / 100) % 10;

                    // 3. 3680 x 320 해상도 기반 정밀 10등분 슬라이싱 (한 칸당 368px)
                    int srcFrameW = Renderer.Img_DiskSprite.Width / 10;
                    int srcFrameH = Renderer.Img_DiskSprite.Height;
                    Rectangle srcRect = new Rectangle(frameIndex * srcFrameW, 0, srcFrameW, srcFrameH);

                    // 4. 인게임 화면에 그려질 디스크 크기 결정 (눈에 잘 보이도록 45x45 스케일 지정)
                    int drawSize = 45;
                    Rectangle destRect = new Rectangle((int)drawX - drawSize / 2, (int)drawY - drawSize / 2, drawSize, drawSize);

                    g.DrawImage(Renderer.Img_DiskSprite, destRect, srcRect, GraphicsUnit.Pixel);
                }
                return; // 디스크를 그렸으므로 아래 일반 이펙트 코드를 타지 않고 종료
            }

            if (e.Kind == "safeZone" || e.Kind == "safezone")
            {
                if (Renderer.Img_Safezone != null)
                {
                    // 💡 2.5D 측면 시선(입체감)을 주기 위해 가로와 세로 비율을 2:1에 가깝게 조절합니다.
                    int drawW = 260;                   // 가로 폭을 살짝 넓혀서 플레이어 안착 영역 확보
                    int drawH = (int)(drawW * 0.52f); 

                    // 안전구역의 중심점(x1, e.Y) 기준으로 정확히 정중앙에 정렬되도록 좌표 보정
                    Rectangle destRect = new Rectangle((int)x1 - drawW / 2, (int)e.Y - drawH / 2, drawW, drawH);

                    // 보정된 타원형 destRect 박스 크기에 맞춰 이펙트를 입체적으로 렌더링
                    g.DrawImage(Renderer.Img_Safezone, destRect);
                }
                return;
            }




          
            if (e.Kind == "playerSlash")
            {
                //// 플레이어 고유의 슬래시 검기 연출 가동
                //DrawSwordSlash(g, x1, e.Y, x2, e.Y2, e.Color, alpha, progress, e.Text);
                return;
            }

            if (e.Kind == "playerClean" || e.Kind == "playerScan" || e.Kind == "playerDelete")
            {
                //int dir = e.X2 >= e.X ? 1 : -1;
                //bool deleteBeam = e.Kind == "playerDelete";

                //// 스킬 종류별 순수 백신 안개 및 지우기 레이저 빔 컬러 추출
                //Color beamColor = deleteBeam ? Color.FromArgb(255, 70, 60) : e.Kind == "playerClean" ? Color.FromArgb(110, 255, 95) : Color.FromArgb(85, 230, 255);
                //float handX = x1 + dir * 42;
                //float handY = e.Y - 64;

                //// 캐릭터의 손끝 포지션에서 오리지널 안개/광선 발사 연산 수행
                //DrawShimmerBeam(g, handX, handY, x2, e.Y2 - 42, beamColor, alpha, progress, deleteBeam);

                //if (!string.IsNullOrEmpty(e.Text))
                //{
                //    using (Font f = F(10f, FontStyle.Bold))
                //    using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 40), 0, 0, 0)))
                //    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, beamColor)))
                //    {
                //        RectangleF rr = new RectangleF(handX + dir * 30 - 42, handY - 44, 84, 26);
                //        g.DrawString(e.Text, f, shadow, new RectangleF(rr.X + 1, rr.Y + 1, rr.Width, rr.Height), Center());
                //        g.DrawString(e.Text, f, b, rr, Center());
                //    }
                //}
                return;
            }

            if (e.Kind == "projectile")
            {
                float cx = x1 + (x2 - x1) * progress;
                float cy = e.Y + (e.Y2 - e.Y) * progress;
                bool deleteBeam = e.Color.R > 180 && e.Color.G < 160;
                DrawShimmerBeam(g, x1, e.Y, cx, cy, e.Color, alpha, progress, deleteBeam);
                using (SolidBrush core = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    g.FillEllipse(core, cx - 5, cy - 5, 10, 10);
                return;
            }
            else if (e.Kind == "text")
            {
                using (Font f = F(13f, FontStyle.Bold))
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 80), 0, 0, 0)))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, e.Color)))
                {
                    RectangleF rr = new RectangleF(x1 - 70, e.Y - progress * 34, 140, 28);
                    g.DrawString(e.Text, f, shadow, new RectangleF(rr.X + 1, rr.Y + 1, rr.Width, rr.Height), Center());
                    g.DrawString(e.Text, f, b, rr, Center());
                }
            }
            else if (e.Kind == "spark")
            {
                int cx = (int)x1;
                int cy = (int)e.Y;
                using (Pen p = new Pen(Color.FromArgb(alpha, e.Color), 3f))
                using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha - 30), Color.White), 1.3f))
                {
                    for (int i = 0; i < 12; i++)
                    {
                        double a = i * Math.PI * 2 / 12 + progress * 2.4;
                        int inner = (int)(6 + progress * 12);
                        int outer = (int)(22 + progress * 38);
                        int xA = cx + (int)(Math.Cos(a) * inner);
                        int yA = cy + (int)(Math.Sin(a) * inner);
                        int xB = cx + (int)(Math.Cos(a) * outer);
                        int yB = cy + (int)(Math.Sin(a) * outer);
                        g.DrawLine(p, xA, yA, xB, yB);
                        if (i % 3 == 0) g.DrawLine(core, cx, cy, xB, yB);
                    }
                }
            }
        }
        // ==========================================================
        // [추가] 보스 영어 고유 명칭에 대응하는 한글 화면 표기명 매핑 테이블
        // ==========================================================
        public static string GetBossKoreanName(string engName)
        {
            if (engName.Contains("Driver-K")) return "드라이버 정비공";
            if (engName.Contains("High-Kernel") || engName.Contains("High_Kernel")) return "커널 골렘";
            if (engName.Contains("BSOD")) return "블루스크린 드래곤";
            if (engName.Contains("Exception Queen") || engName.Contains("Exception_Queen")) return "오류지옥 집행관";
            if (engName.Contains("Illegal_Binny") || engName.Contains("Binny")) return "휴지통 관리자";
            return engName;
        }

        // ==========================================================
        // [추가] 모든 보스맵 공통 상단 고정 거대 HP 바 및 최하단 쿨타임 시스템
        // ==========================================================
        public static void DrawBossGlobalUI(Graphics g, GameEntity boss, Size clientSize)
        {
            if (boss == null || boss.Hp <= 0) return;

            // --------------------------------------------------
            // 1. [상단 고정 레이드 스타일 거대 보스 HP 바] (가로 700px)
            // --------------------------------------------------
            Rectangle hpBarRect = new Rectangle(clientSize.Width / 2 - 350, 45, 700, 24);
            // 묵직한 다크 레드 메탈 색상으로 보스 라이프 게이지 렌더링
            DrawBar(g, hpBarRect, boss.Hp, boss.MaxHp, Color.FromArgb(210, 35, 35));

            // 거대 테두리 선 보정
            using (Pen p = new Pen(Color.FromArgb(200, 20, 20, 20), 2f))
                g.DrawRectangle(p, hpBarRect);

            // 한글 표기 변환 세팅 및 데이터 로드
            string krName = GetBossKoreanName(boss.Name);
            string hpText = $"{krName}  [ HP : {boss.Hp} / {boss.MaxHp} ]";

            using (Font f = F(11.5f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
            {
                // 가시성 극대화를 위한 백그라운드 블랙 드롭 섀도우 연출
                g.DrawString(hpText, f, Brushes.Black, hpBarRect.X + 2, hpBarRect.Y + 2, Center());
                g.DrawString(hpText, f, sb, hpBarRect, Center());
            }

            // --------------------------------------------------
            // 2. [중앙 최하단 스킬 쿨타임 UI 표기] (초 단위 실시간 스캔)
            // --------------------------------------------------
            // 현재 MainForm.cs에서 timer.Interval = 33(30 FPS)으로 리미트가 걸려 있으므로,
            // 1초는 정확히 30틱입니다. 30.0f로 나누어 리얼타임 초 단위 소수점 디스플레이를 가동합니다.
            //if (boss.AttackCooldown > 0)
            //{
            //    float cooldownSeconds = boss.AttackCooldown / 30.0f;
            //    string coolText = $"⚡ 시스템 브레이크 특수 패턴 재충전 중: {cooldownSeconds:0.0}초";
            //    Rectangle coolRect = new Rectangle(clientSize.Width / 2 - 220, clientSize.Height - 88, 440, 26);

            //    using (SolidBrush bg = new SolidBrush(Color.FromArgb(170, 10, 15, 25))) g.FillRectangle(bg, coolRect);
            //    using (Pen p = new Pen(Color.FromArgb(240, 180, 40), 1.5f)) g.DrawRectangle(p, coolRect);
            //    using (Font f = F(9.5f, FontStyle.Bold))
            //    {
            //        g.DrawString(coolText, f, Brushes.Gold, coolRect, Center());
            //    }
            //}
        }
    }
   

    internal static class GraphicsRoundedExtensions
    {
        private static GraphicsPath RoundedPath(RectangleF bounds, float radius)
        {
            float r = Math.Max(1f, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2f));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, r * 2f, r * 2f, 180, 90);
            path.AddArc(bounds.Right - r * 2f, bounds.Y, r * 2f, r * 2f, 270, 90);
            path.AddArc(bounds.Right - r * 2f, bounds.Bottom - r * 2f, r * 2f, r * 2f, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - r * 2f, r * 2f, r * 2f, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF bounds, float radius)
        {
            using (GraphicsPath path = RoundedPath(bounds, radius)) g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF bounds, float radius)
        {
            using (GraphicsPath path = RoundedPath(bounds, radius)) g.DrawPath(pen, path);
        }

        
    }




}
