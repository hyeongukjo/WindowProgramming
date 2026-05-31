using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    public sealed partial class MainForm
    {
        // -----------------------------------------------------------------------------
        //  [휴지통 모달 연산 및 반전 밸런스 제어 격실]
        // -----------------------------------------------------------------------------
        private bool showTrashCanWindow = false;
        private readonly string trashCanCloseKey = "trashCanWinClose";

        // 중요 폴더(+10)와 함정 폴더(-10)의 순서를 악마적으로 무작위로 뒤섞었습니다!
        private readonly string[] trashFolderNames = {
            "System32_Essential_Core_Backup_Secure", // +10 (중요해 보임)
            "임시파일_삭제보관함_Junk_Files",            // -10 (쓰레기 같음 함정)
            "Kernel_Security_DonotDelete_Fatal",     // +10 (중요해 보임)
            "최종 발표자료 진짜 최종의 최종의 최종의 최종",          // -10 (쓰레기 같음 함정)
            "Registry_Root_Hardware_Config.sys",     // +10 (중요해 보임)
            "직박구리",           // -10 (쓰레기 같음 🔒함정)
            "윈도우 프로그래밍 개쩌는 시험 족보"          // -10 (쓰레기 같음 함정)
        };

        // 뒤섞인 이름 순서와 정확하게 1:1 매칭되도록 가감산 리스트 정렬 조율
        private readonly int[] trashFolderCoinChanges = { 10, -10, 10, -10, 10, -10, -10 };
        private bool[] trashFoldersDeleted = new bool[7];

        private Rectangle GetTrashCanWindowBounds()
        {
            int winW = 1000;
            int winH = 580;
            int winX = (ClientSize.Width - winW) / 2;
            int winY = (ClientSize.Height - winH) / 2;
            return new Rectangle(winX, winY, winW, winH);
        }

        //  휴지통 메인 팝업 윈도우 렌더러
        public void DrawTrashCanWindow(Graphics g)
        {
            if (!showTrashCanWindow) return;

            // 바탕화면 차단용 모달 암전 섀도우 처리
            using (SolidBrush dimBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                g.FillRectangle(dimBrush, ClientRectangle);

            Rectangle winBounds = GetTrashCanWindowBounds();

            // 공식 블루 시스템 에셋 프레임 엔진 가동
            SystemWindowUI.Shared.DrawSystemPanelFrame(
                g,
                winBounds,
                "시스템 보호 격리실 - 휴지통 무결성 검증 센터",
                SystemWindowStyle.Blue,
                true,
                buttons,
                trashCanCloseKey
            );

            // [X버튼 범위 확장]: 정품 에셋 X단추 위에 왼쪽으로 2배 넓힌 투명 히트박스 링크
            Rectangle closeBtnRect = new Rectangle(winBounds.Right - 66, winBounds.Y + 5, 60, 22);
            buttons.Add(new UiButton(closeBtnRect, trashCanCloseKey));

            int startY = winBounds.Y + 65;
            int rowHeight = 68;

            using (Font fName = Renderer.F(11f, FontStyle.Bold))
            using (Font fBtn = Renderer.F(10f, FontStyle.Bold))
            {
                for (int i = 0; i < 7; i++)
                {
                    int rowY = startY + (i * rowHeight);
                    Rectangle rowRect = new Rectangle(winBounds.X + 35, rowY, winBounds.Width - 70, 58);

                    // 행 컨테이너 스킨 배치
                    using (SolidBrush rowBg = new SolidBrush(Color.FromArgb(242, 245, 249)))
                        g.FillRectangle(rowBg, rowRect);
                    g.DrawRectangle(Pens.LightGray, rowRect);

                    // 상태별 상태 아이콘 (오류 방어 규격 조율)
                    if (trashFoldersDeleted[i])
                    {
                        using (Font iconFont = Renderer.F(13f, FontStyle.Regular))
                            g.DrawString("❌", iconFont, Brushes.Crimson, rowRect.X + 15, rowRect.Y + 16);
                    }
                    else
                    {
                        using (Font iconFont = Renderer.F(15f, FontStyle.Regular))
                            g.DrawString("📁", iconFont, Brushes.Goldenrod, rowRect.X + 15, rowRect.Y + 14);
                    }

                    // 폴더 라벨 명칭 출력
                    Brush textBrush = trashFoldersDeleted[i] ? Brushes.DarkGray : Brushes.Black;
                    string displayName = trashFolderNames[i];
                    g.DrawString(displayName, fName, textBrush, rowRect.X + 55, rowRect.Y + 19);

                    // 우측 삭제 연동식 UI 버튼 배치
                    Rectangle btnRect = new Rectangle(rowRect.Right - 130, rowRect.Y + 11, 110, 36);

                    if (!trashFoldersDeleted[i])
                    {
                        using (LinearGradientBrush bBtn = new LinearGradientBrush(btnRect, Color.FromArgb(235, 240, 250), Color.FromArgb(205, 215, 235), 90f))
                            g.FillRectangle(bBtn, btnRect);
                        g.DrawRectangle(Pens.SteelBlue, btnRect);
                        g.DrawString("삭제하기", fBtn, Brushes.DarkBlue, btnRect, Renderer.Center());

                        // 개별 행 버튼 등록
                        buttons.Add(new UiButton(btnRect, $"delete_trash_{i}"));
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Gainsboro, btnRect);
                        g.DrawRectangle(Pens.DarkGray, btnRect);
                        g.DrawString("완료됨", fBtn, Brushes.SlateGray, btnRect, Renderer.Center());
                    }
                }
            }
        }

        // 개별 삭제 커맨드 연산 파이프라인
        private void ExecuteTrashDelete(int index)
        {
            if (index < 0 || index >= 7 || trashFoldersDeleted[index]) return;

            trashFoldersDeleted[index] = true;
            int change = trashFolderCoinChanges[index];
            player.Coins = Math.Max(0, player.Coins + change); // 코인 음수 낙하 가드

            // 버퍼링 렉 현상을 완전히 없애기 위해 지속 프레임 수치(Duration)를 
            // 기존 45~50에서 전격 '18'(약 0.3초)로 대폭 압축 튜닝 누르자마자 팝업되고 소멸.
            if (change > 0)
            {
                effects.Add(new Effect("text", ClientSize.Width / 2, winFolderMessageY(), ClientSize.Width / 2, winFolderMessageY(), 18, Color.Lime, $"[정화 성공] 포상금 지급! (+{change} 코인)"));
                TryBeep(980, 60);
            }
            else
            {
                effects.Add(new Effect("text", ClientSize.Width / 2, winFolderMessageY(), ClientSize.Width / 2, winFolderMessageY(), 18, Color.OrangeRed, $"[경고] 필수 연동 개체 손상! ({change} 코인)"));
                TryBeep(280, 100);
            }
            Invalidate();
        }

        // 텍스트 위치 가이드라인 분할 함수
        private int winFolderMessageY()
        {
            return ClientSize.Height / 2 - 40;
        }
    }
}