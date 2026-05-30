using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    public sealed partial class MainForm
    {
        // -----------------------------------------------------------------------------
        // 💻 [몬스터 도감 모달 제어 격실]
        // -----------------------------------------------------------------------------
        private bool showMonsterBookWindow = false;
        private readonly string monsterBookCloseKey = "monsterBookWinClose";

        private Image imgNormalMonster;
        private Image[] imgBosses = new Image[5];
        private bool bookImagesLoaded = false;

        // Assets/UI 디렉토리 내부에서 그래픽 자산 적재
        private void LoadBookImages()
        {
            if (bookImagesLoaded) return;
            string uiDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UI");

            try
            {
                string normalPath = Path.Combine(uiDir, "normal_monster.png");
                if (File.Exists(normalPath)) imgNormalMonster = Image.FromFile(normalPath);

                for (int i = 0; i < 5; i++)
                {
                    string bossPath = Path.Combine(uiDir, $"d{i + 1}.png");
                    if (File.Exists(bossPath)) imgBosses[i] = Image.FromFile(bossPath);
                }
            }
            catch { }
            bookImagesLoaded = true;
        }

        private Rectangle GetMonsterBookWindowBounds()
        {
            int winW = 1150;
            int winH = 600;
            int winX = (ClientSize.Width - winW) / 2;
            int winY = (ClientSize.Height - winH) / 2;
            return new Rectangle(winX, winY, winW, winH);
        }

        // ② 파일 클릭 시 활성화될 대형 팝업 도감 윈도우 그리기 (공식 UI 프레임 적용 사양)
        public void DrawMonsterBookWindow(Graphics g)
        {
            if (!showMonsterBookWindow) return;
            LoadBookImages();

            // [모달 시각 효과] 바탕화면 전체 암전용 블러 섀도우막 가동
            using (SolidBrush dimBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                g.FillRectangle(dimBrush, ClientRectangle);

            Rectangle winBounds = GetMonsterBookWindowBounds();

            // 🛠️ [수정사항 2] 도감 팝업창도 내 컴퓨터와 동일하게 정품 UI 프레임워크 엔진 프레임으로 대개조!
            SystemWindowUI.Shared.DrawSystemPanelFrame(
                g,
                winBounds,
                "📂 시스템 보안 관리자 - 감염 코드 개체 도감 리포트 [보안 격리 구역]",
                SystemWindowStyle.Blue,
                true,
                buttons,
                monsterBookCloseKey
            );

            // 🛠️ [수정사항 2] 임의로 그리던 가짜 X UI를 완전 삭제하고, 순정 에셋 X버튼 위치에 투명 히트박스만 결합!
            Rectangle closeBtnRect = new Rectangle(winBounds.Right - 66, winBounds.Y + 5, 60, 22);
            buttons.Add(new UiButton(closeBtnRect, monsterBookCloseKey));

            int contentX = winBounds.X + 25;
            int normalY = winBounds.Y + 55;
            Rectangle normalRect = new Rectangle(contentX, normalY, winBounds.Width - 50, 240);

            
            // 만약 겉의 테두리 선마저 완전히 없애고 싶으시다면 바로 아래 DrawRectangle 줄도 주석 처리 하시면 됩니다.
            //g.DrawRectangle(Pens.LightSteelBlue, normalRect);

            //  캐릭터 상자 위에 "일반 몬스터" 명시적 텍스트 출력
            using (Font subTitleFont = Renderer.F(12f, FontStyle.Bold))
            {
                // 상자 윗면에서 살짝 위쪽(Y - 22픽셀 지점)에 흰색 글씨로 배치합니다.
                g.DrawString("몬스터", subTitleFont, Brushes.Black, normalRect.X + 5, normalRect.Y - 2);
            }

            if (imgNormalMonster != null)
            {
                // 
                // 하단의 숫자 4개를 현국님이 보시면서 원하시는 스케일로 직접 커스텀 하시면 됩니다!
                // ---------------------------------------------------------------------
                int normalWidth = 831;  // ◀️ [가로 크기]: 기본 가로폭입니다. 숫자를 줄이면 작아집니다.
                int normalHeight = 125;  // ◀️ [세로 크기]: 기본 세로폭입니다. 숫자를 줄이면 작아집니다.

                // 상자 레이아웃 정중앙 안착을 위한 수학적 오프셋 정렬 계산식
                int normalXPos = normalRect.X + (normalRect.Width - normalWidth) / 2;    // ◀️ 필요시 정렬 무시하고 + 50 등으로 고정 가능
                int normalYPos = normalRect.Y + (normalRect.Height - normalHeight) / 2;  // ◀️ 필요시 Y축 고정 배치 가능
                // ---------------------------------------------------------------------

                Rectangle normalImgRect = new Rectangle(normalXPos, normalYPos, normalWidth, normalHeight);
                g.DrawImage(imgNormalMonster, normalImgRect);
            }

            // [하단 아랫줄] : 200x200 고정 크기 순차 배치 보스 리스트 
            int bossY = normalRect.Bottom + 25;
            int bossSize = 200;

         
            int spacing = 12; 

         
            int totalBossWidth = (bossSize * 5) + (spacing * 4); 
            int startX = winBounds.X + (winBounds.Width - totalBossWidth) / 2; 

            int[] previousClearRequirements = { 1, 3, 5, 7, 9 };
            string[] bossNames = { "Stage 02: Driver-K", "Stage 04: High-Kernel", "Stage 06: BSOD Dragon", "Stage 08: Exception Queen", "Stage 10: Illegal_Binny" };

            using (Font bFont = Renderer.F(10f, FontStyle.Bold))
            using (Font maskFont = Renderer.F(24f, FontStyle.Bold))
            {
                for (int i = 0; i < 5; i++)
                {
                    
                    int bx = startX + i * (bossSize + spacing);
                    Rectangle bossRect = new Rectangle(bx, bossY, bossSize, bossSize);

                    using (SolidBrush cardBg = new SolidBrush(Color.FromArgb(25, 28, 38)))
                        g.FillRectangle(cardBg, bossRect);
                    g.DrawRectangle(Pens.LightSteelBlue, bossRect);

                    bool isUnlocked = player.ClearedStages >= previousClearRequirements[i];

                    if (isUnlocked && imgBosses[i] != null)
                    {
                        g.DrawImage(imgBosses[i], bossRect);

                        Rectangle nameBox = new Rectangle(bx, bossRect.Bottom - 26, bossSize, 26);
                        using (SolidBrush nBg = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                            g.FillRectangle(nBg, nameBox);
                        g.DrawString(bossNames[i], bFont, Brushes.Lime, nameBox, Renderer.Center());
                    }
                    else
                    {
                        g.DrawString("???", maskFont, Brushes.Crimson, bossRect, Renderer.Center());

                        Rectangle nameBox = new Rectangle(bx, bossRect.Bottom - 26, bossSize, 26);
                        using (SolidBrush nBg = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                            g.FillRectangle(nBg, nameBox);
                        g.DrawString("🔒 정화 후 확인 가능", bFont, Brushes.DarkGray, nameBox, Renderer.Center());
                    }
                }
            }
        }
    }
}