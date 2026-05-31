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
        // 💾 [순정 에셋 위치 최적화 - 시작 메뉴 팝업 격실]
        // -----------------------------------------------------------------------------
        private bool showStartMenuPopup = false;
        private Image imgStartMenuPopup = null;
        private bool startMenuLoaded = false;

        private readonly string startMenuSaveKey = "startMenuSaveAction";
        private readonly string startMenuHelpKey = "startMenuHelpAction";
        private readonly string startMenuExitKey = "startMenuExitAction";

        // Assets/UI/startmenu_popup.png 이미지 자산 로드[cite: 7]
        private void LoadStartMenuImage()
        {
            if (startMenuLoaded) return;
            string uiDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UI"); 
            try
            {
                string popupPath = Path.Combine(uiDir, "startmenu_popup.png"); 
                if (File.Exists(popupPath)) imgStartMenuPopup = Image.FromFile(popupPath);
            }
            catch { }
            startMenuLoaded = true;
        }

        // 📊 [위치 수정]: 시작 메뉴 팝업창을 왼쪽 최하단 작업 표시줄 바로 위에 딱 붙도록 튜닝
        private Rectangle GetStartMenuPopupBounds()
        {
            int menuW = 320; // 팝업창 가로 폭[cite: 7]
            int menuH = 460; // 팝업창 세로 높이[cite: 7]

            int menuX = 0;   // 🛠️ 시작 버튼 라인과 일치하도록 가로 좌표를 좌측 끝(0)으로 밀착
            int menuY = ClientSize.Height - 45 - menuH; // 🛠️ 붕 뜨는 현상 제거: 작업 표시줄(45px) 바로 위에 여백 없이 락온
            return new Rectangle(menuX, menuY, menuW, menuH);
        }

        // ① 시작 메뉴 순정 에셋 + 기능 레이어 위치 교정 렌더러
        public void DrawStartMenuPopup(Graphics g)
        {
            if (!showStartMenuPopup) return; 
            LoadStartMenuImage(); 

            Rectangle bounds = GetStartMenuPopupBounds();

            // 배경 에셋 프레임 출력[cite: 7]
            if (imgStartMenuPopup != null)
            {
                g.DrawImage(imgStartMenuPopup, bounds); 
            }
            else
            {
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(236, 233, 216))) g.FillRectangle(bg, bounds); 
                g.DrawRectangle(Pens.SteelBlue, bounds);
            }

            // 내부 기능 텍스트 및 투명 클릭 히트박스 위치 동기화 조율
            using (Font itemFont = Renderer.F(9.5f, FontStyle.Bold))
            using (Font bottomFont = Renderer.F(8.5f, FontStyle.Bold)) // 작은 종료 버튼 에셋 규격 맞춤형 폰트
            {
                // A. [지금 저장하기] - 왼쪽 흰색 구역 상단 정렬선 매칭
                Rectangle saveRect = new Rectangle(bounds.X + 20, bounds.Y + 62, 150, 32);
                buttons.Add(new UiButton(saveRect, startMenuSaveKey));
                g.DrawString("💾  지금 저장하기", itemFont, Brushes.Black, saveRect, Renderer.LeftMiddle());

                // B. [도움말 안내] - 왼쪽 흰색 구역 중단 정렬선 매칭
                Rectangle helpRect = new Rectangle(bounds.X + 20, bounds.Y + 112, 150, 32);
                buttons.Add(new UiButton(helpRect, startMenuHelpKey));
                g.DrawString("❓  도움말 (F1)", itemFont, Brushes.Black, helpRect, Renderer.LeftMiddle());

                // C. 🔴 [시스템 종료] - 🛠️ 위치 수정: 외부 풀밭에서 팝업창 왼쪽 하단의 작은 사각형 버튼 안으로 매립 완료!!
                // 에셋 이미지 내부 최하단 파란색 바의 왼쪽 작은 버튼 박스 좌표와 투명 히트박스를 정밀하게 겹치게 지정합니다.
                Rectangle exitRect = new Rectangle(bounds.X + 8, bounds.Bottom - 35, 86, 24);
                buttons.Add(new UiButton(exitRect, startMenuExitKey));

                // 파란색 버튼 내부 중앙에 쏙 안착되도록 Center 포맷터와 흰색 브러시 주입
                g.DrawString("시스템 종료", bottomFont, Brushes.White, exitRect, Renderer.Center());
            }
        }
    }
}