using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    public sealed class RecoveryToolsUI
    {
        public static readonly RecoveryToolsUI Shared = new RecoveryToolsUI();

        private Image recoveryKitImage;
        private Image memoryKitImage;
        private Image bundleImage;

        private Rectangle recoveryKitSource;
        private Rectangle memoryKitSource;
        private Rectangle bundleSource;

        private const string ItemHp = "hp";
        private const string ItemMp = "mp";
        private const string ItemBundle = "bundle";

        private RecoveryToolsUI()
        {
            LoadImages();
        }

        private void LoadImages()
        {
            string uiDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UI");

            recoveryKitImage = LoadFirstImage(
                uiDir,
                "HP.png",
                "RecoveryKit.png",
                "RecoveryKit-Photoroom.png",
                "HP-Photoroom.png"
            );

            memoryKitImage = LoadFirstImage(
                uiDir,
                "MP.png",
                "mp.png",
                "MemoryKit.png",
                "MemoryKit-Photoroom.png",
                "MP-Photoroom.png",
                "icons.png"
            );

            bundleImage = LoadFirstImage(
                uiDir,
                "HPMP.png",
                "SupplyBundle.png",
                "SupplyBundle-Photoroom.png",
                "HPMP-Photoroom.png"
            );

            recoveryKitSource = GetVisibleBounds(recoveryKitImage);
            memoryKitSource = GetVisibleBounds(memoryKitImage);
            bundleSource = GetVisibleBounds(bundleImage);
        }

        private Image LoadFirstImage(string directory, params string[] fileNames)
        {
            for (int i = 0; i < fileNames.Length; i++)
            {
                Image image = LoadImage(Path.Combine(directory, fileNames[i]));
                if (image != null)
                    return image;
            }

            return null;
        }

        private Image LoadImage(string path)
        {
            try
            {
                if (File.Exists(path))
                    return Image.FromFile(path);
            }
            catch
            {
            }

            return null;
        }

        public void DrawDesktopShortcut(
            Graphics g,
            Rectangle client,
            int coins,
            List<UiButton> buttons)
        {
            Rectangle iconRect = new Rectangle(
                120,
                client.Height - 170,
                150,
                116
            );

            Renderer.DrawShopShortcut(g, iconRect, coins);

            if (buttons != null)
                buttons.Add(new UiButton(iconRect, "openShop"));
        }
       
        public void DrawDesktopStatusPanel(
             Graphics g,
             Rectangle client,
             PlayerState player,
             int unlockedStage,
             int totalStages)
        {
            // 가로 45%, 세로 하단 5% 마진 동적 계산 수식 유지
            int panelWidth = (int)(client.Width * 0.45f);
            int topY = 55;
            int taskbarH = 45;
            int safetyMarginY = (int)(client.Height * 0.05f);

            int panelHeight = client.Height - topY - taskbarH - safetyMarginY;

            Rectangle win = new Rectangle(
                client.Width - panelWidth - 35,
                topY,
                panelWidth,
                panelHeight
            );

            // 네이티브 블루 테마 시스템 프레임 드로우
            SystemWindowUI.Shared.DrawSystemPanelFrame(
                g,
                win,
                "Recovery Program - Status",
                SystemWindowStyle.Blue,
                true,
                null,
                null,
                30,
                30,
                76
            );

            int titleAreaHeight = 30;
            int margin = 16;
            int gap = 12;

            int totalAvailableH = panelHeight - titleAreaHeight - (gap * 3) - 35;

            // 4개 그룹 박스의 세로 해상도 비율 재배치
            int programH = (int)(totalAvailableH * 0.16f); // 프로그램 상태 (16%)
            int itemH = (int)(totalAvailableH * 0.19f); // 보유 도구 (30% -> 18%로 콤팩트하게 축소)
            int commandH = (int)(totalAvailableH * 0.36f); // 복구 명령 (36%)
            int iconDescH = totalAvailableH - programH - itemH - commandH; // 아이콘 기능 설명 (18% -> 30%로 거의 2배 확장!)

            int y = win.Y + titleAreaHeight + 10;
            int innerWidth = win.Width - margin * 2;

            Rectangle programBox = new Rectangle(win.X + margin, y, innerWidth, programH);
            y = programBox.Bottom + gap;

            Rectangle itemBox = new Rectangle(win.X + margin, y, innerWidth, itemH);
            y = itemBox.Bottom + gap;

            Rectangle commandBox = new Rectangle(win.X + margin, y, innerWidth, commandH);
            y = commandBox.Bottom + gap;

            // [구조 치환]: 구 progressBox 자리에 iconDescBox 레이아웃 대입
            Rectangle iconDescBox = new Rectangle(win.X + margin, y, innerWidth, iconDescH);

            // 하위 연동 렌더러 파이프라인 호출
            DrawDesktopProgramBox(g, programBox, player);
            DrawDesktopItemBox(g, itemBox, player);
            DrawDesktopCommandBox(g, commandBox);
            DrawDesktopIconDescBox(g, iconDescBox); // 新 스코어 가이드 설명 박스 가동
        }

        private void DrawDesktopProgramBox(Graphics g, Rectangle r, PlayerState player)
        {
            DrawGroupBox(g, r, "프로그램 상태");

            
            using (Font fontProfile = Renderer.F(18f, FontStyle.Bold)) // 1. 프로필 이름 크기 
            using (Font fontLevel = Renderer.F(12f, FontStyle.Bold))   // 2. 레벨, 무기레벨 크기
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (StringFormat sf = LeftMiddle())
            {
                string profileName = string.IsNullOrWhiteSpace(player.ProfileName)
                    ? "ProfileName"
                    : player.ProfileName;

                // 늘어난 상자 높이에 맞춰 줄 간격을 행간 비율로 균등 분할 계산
                int contentH = r.Height - 31;
                int lineHeight = contentH / 2;

                // 프로필 이름 그리기 
                g.DrawString(
                    "Profile: " + profileName,
                    fontProfile,
                    textBrush,
                    new Rectangle(r.X + 16, r.Y + 31, r.Width - 32, lineHeight),
                    sf
                );

                // 레벨 및 무기레벨 그리기 
                g.DrawString(
                    "Level " + player.Level + "    Weapon +" + player.WeaponLevel,
                    fontLevel,
                    blueBrush,
                    new Rectangle(r.X + 16, r.Y + 31 + lineHeight, r.Width - 32, lineHeight),
                    sf
                );
            }
        }

        private void DrawDesktopItemBox(Graphics g, Rectangle r, PlayerState player)
        {
            DrawGroupBox(g, r, "보유 도구");

            int startY = r.Y + 33;
            int contentH = r.Height - 33;
            int rowHeight = contentH / 3; // 3개 아이템 행 높이 균등 분할

            DrawDesktopCoinRow(
                g,
                new Rectangle(r.X + 10, startY, r.Width - 20, rowHeight),
                player.Coins
            );

            DrawDesktopItemRow(
                g,
                new Rectangle(r.X + 10, startY + rowHeight, r.Width - 20, rowHeight),
                recoveryKitImage,
                recoveryKitSource,
                "Recovery Kit",
                player.HpPotions,
                "D 사용"
            );

            DrawDesktopItemRow(
                g,
                new Rectangle(r.X + 10, startY + rowHeight * 2, r.Width - 20, rowHeight),
                memoryKitImage,
                memoryKitSource,
                "Memory Kit",
                player.MpPotions,
                "F 사용"
            );
        }

        private void DrawDesktopCoinRow(Graphics g, Rectangle row, int coins)
        {
            // 코인 문자열 크기 제어
            using (Font f = Renderer.F(15f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (StringFormat left = LeftMiddle())
            using (StringFormat right = RightMiddle())
            {
                g.DrawString(
                    "보유 코인",
                    f,
                    textBrush,
                    new Rectangle(row.X + 6, row.Y, row.Width / 2, row.Height),
                    left
                );

                g.DrawString(
                    coins.ToString(),
                    f,
                    blueBrush,
                    new Rectangle(row.X + row.Width / 2, row.Y, row.Width / 2 - 6, row.Height),
                    right
                );
            }
        }

        private void DrawDesktopItemRow(
            Graphics g,
            Rectangle row,
            Image iconImage,
            Rectangle iconSource,
            string itemName,
            int count,
            string shortcutText)
        {
            // 늘어난 행 높이에 맞춰 아이콘 이미지를 수직 중앙 정렬 처리
            int iconSize = 32;
            Rectangle icon = new Rectangle(row.X + 4, row.Y + (row.Height - iconSize) / 2, iconSize, iconSize);
            DrawItemIcon(g, iconImage, iconSource, icon);

            // 아이템 리스트 글자 크기 제어
            using (Font f = Renderer.F(10f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (StringFormat left = LeftMiddle())
            using (StringFormat right = RightMiddle())
            {
                g.DrawString(
                    itemName,
                    f,
                    textBrush,
                    new Rectangle(icon.Right + 12, row.Y, row.Width - 160, row.Height),
                    left
                );

                g.DrawString(
                    "x" + count,
                    f,
                    blueBrush,
                    new Rectangle(row.Right - 110, row.Y, 45, row.Height),
                    right
                );

                g.DrawString(
                    shortcutText,
                    f,
                    textBrush,
                    new Rectangle(row.Right - 55, row.Y, 50, row.Height),
                    right
                );
            }
        }

        private void DrawDesktopCommandBox(Graphics g, Rectangle r)
        {
            DrawGroupBox(g, r, "스킬 정보");

            int startY = r.Y + 34;
            int contentH = r.Height - 34;
            int lineHeight = contentH / 5; // 5개 텍스트 가이드 라인 균등 분할

            DrawCommandLine(g, r.X + 12, startY + lineHeight * 0, r.Width - 24, "Q", "Quick Scan", "기본 공격", lineHeight);
            DrawCommandLine(g, r.X + 12, startY + lineHeight * 1, r.Width - 24, "W", "OverClock", "오버 클럭 (stage 2 클리어 시 해제)", lineHeight);
            DrawCommandLine(g, r.X + 12, startY + lineHeight * 2, r.Width - 24, "E", "DataSheild", "데이터실드 (stage 5 클리어 시 해제)", lineHeight);
            DrawCommandLine(g, r.X + 12, startY + lineHeight * 3, r.Width - 24, "R", "SysCall", "시스템콜 (stage 8 클리어 시 해제)", lineHeight);

            // 마우스 조작 가이드 안내 글씨 크기 제어
            using (Font f = Renderer.F(13f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 70, 70)))
            using (StringFormat sf = LeftMiddle())
            {
                g.DrawString(
                    "마우스 우클릭: 플레이어 이동",
                    f,
                    b,
                    new Rectangle(r.X + 12, startY + lineHeight * 4, r.Width - 24, lineHeight),
                    sf
                );
            }
        }

        private void DrawCommandLine(
             Graphics g,
             int x,
             int y,
             int width,
             string key,
             string commandName,
             string description,
             int h)
        {
            // 단축키 명세 폰트 사이즈 제어
            using (Font keyFont = Renderer.F(13f, FontStyle.Bold))
            using (Font textFont = Renderer.F(12f, FontStyle.Bold))
            using (SolidBrush keyBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat left = LeftMiddle())
            {
                g.DrawString(key + ":", keyFont, keyBrush, new Rectangle(x, y, 30, h), left);
                g.DrawString(commandName + "  -  " + description, textFont, textBrush, new Rectangle(x + 32, y, width - 32, h), left);
            }
        }

        private void DrawDesktopIconDescBox(Graphics g, Rectangle r)
        {
            DrawGroupBox(g, r, "아이콘 기능 설명");

            using (Font boldFont = Renderer.F(14f, FontStyle.Bold))
            using (Font regularFont = Renderer.F(12f, FontStyle.Regular))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(0, 62, 160))) // 아이콘 이름 색상 (블루)
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))   // 설명 글씨 색상 (다크그레이)
            using (StringFormat sf = LeftMiddle())
            {
                int startY = r.Y + 32;
                int contentH = r.Height - 37;
                int lineHeight = contentH / 4; // 4개의 아이콘 간격을 균등 분할 계산

              
                int titleWidth = 130;         
                int descStartX = r.X + 150;   
                int descWidth = r.Width - 165; 
                // -----------------------------------------------------------------------------

                // 1. 내 컴퓨터 설명
                g.DrawString("• 내 컴퓨터 :", boldFont, titleBrush, new Rectangle(r.X + 14, startY + lineHeight * 0, titleWidth, lineHeight), sf);
                g.DrawString("실시간 통합 랭킹 리더보드 창 활성화", regularFont, textBrush, new Rectangle(descStartX, startY + lineHeight * 0, descWidth, lineHeight), sf);

                // 2. 파일 설명 구역
                g.DrawString("• 파      일 :", boldFont, titleBrush, new Rectangle(r.X + 14, startY + lineHeight * 1, titleWidth, lineHeight), sf);
                g.DrawString("윈도우 세상 몬스터 도감", regularFont, Brushes.Black, new Rectangle(descStartX, startY + lineHeight * 1, descWidth, lineHeight), sf);

                // 3. 인터넷 설명 구역
                g.DrawString("• 인 터 넷 :", boldFont, titleBrush, new Rectangle(r.X + 14, startY + lineHeight * 2, titleWidth, lineHeight), sf);
                g.DrawString("???: 무언가 엄청난 것이 있는 기분이다..", regularFont, Brushes.Black, new Rectangle(descStartX, startY + lineHeight * 2, descWidth, lineHeight), sf);

                // 4. 휴지통 설명 구역 
                g.DrawString("• 휴 지 통 :", boldFont, titleBrush, new Rectangle(r.X + 14, startY + lineHeight * 3, titleWidth, lineHeight), sf);
                g.DrawString("쓸모 없는 파일은 이곳으로.. 쓸모가 있을 수도??", regularFont, Brushes.Black, new Rectangle(descStartX, startY + lineHeight * 3, descWidth, lineHeight), sf);
            }
        }

        // 기존 호출부가 아직 안 바뀌어도 컴파일되도록 남겨둔 기본 버전
        public void DrawShop(
            Graphics g,
            Rectangle client,
            PlayerState player,
            List<UiButton> buttons)
        {
            DrawShop(g, client, player, ItemHp, buttons);
        }

        // 선택형 상점 UI 버전
        public void DrawShop(
            Graphics g,
            Rectangle client,
            PlayerState player,
            string selectedItem,
            List<UiButton> buttons)
        {
            if (string.IsNullOrEmpty(selectedItem))
                selectedItem = ItemHp;

            Renderer.DrawXPWallpaper(g, client);
            TaskbarUI.Shared.Draw(g, client);
            Rectangle win = new Rectangle(
    client.Width / 2 - 420,
    client.Height / 2 - 215,
    840,
    430
);

            DrawShopFrame(g, win, buttons);

            int margin = 22;
            int contentTop = win.Y + 48;

            Rectangle guideBox = new Rectangle(
                win.X + margin,
                contentTop,
                win.Width - margin * 2,
                34
            );

            Rectangle statusBox = new Rectangle(
                win.X + margin,
                guideBox.Bottom + 4,
                win.Width - margin * 2,
                44
            );

            Rectangle footerBox = new Rectangle(
                win.X + margin,
                win.Bottom - 66,
                win.Width - margin * 2,
                30
            );

            int mainTop = statusBox.Bottom + 5;
            int mainHeight = footerBox.Y - mainTop - 5;
            int gap = 12;
            int leftWidth = (win.Width - margin * 2 - gap) / 2;

            Rectangle listBox = new Rectangle(
                win.X + margin,
                mainTop,
                leftWidth,
                mainHeight
            );

            Rectangle descBox = new Rectangle(
                listBox.Right + gap,
                mainTop,
                win.Width - margin * 2 - leftWidth - gap,
                mainHeight
            );

            DrawGuideBox(g, guideBox);
            DrawStatusBox(g, statusBox, player);
            DrawItemList(g, listBox, selectedItem, buttons);
            DrawDescriptionBox(g, descBox, selectedItem);
            DrawFooter(g, footerBox, buttons);
        }

        private void DrawShopFrame(Graphics g, Rectangle win, List<UiButton> buttons)
        {
            SystemWindowUI.Shared.DrawBlueCancelFrameNineSlice(g, win);

            using (Font titleFont = Renderer.F(12.5f, FontStyle.Bold))
            using (SolidBrush white = new SolidBrush(Color.White))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle titleRect = new Rectangle(
                    win.X + 28,
                    win.Y + 1,
                    win.Width - 130,
                    30
                );

                g.DrawString(
                    "Recovery Tools - Supply Panel",
                    titleFont,
                    white,
                    titleRect,
                    sf
                );
            }

            Rectangle closeRect = new Rectangle(
                win.Right - 72,
                win.Y + 5,
                54,
                34
            );

            if (buttons != null)
                buttons.Add(new UiButton(closeRect, "shopBack"));
        }

        private void DrawGuideBox(Graphics g, Rectangle r)
        {
            //DrawInsetPanel(g, r);

            Rectangle icon = new Rectangle(r.X + 24, r.Y + 3, 28, 28);
            DrawInfoIcon(g, icon);

            using (Font f = Renderer.F(10.4f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle textRect = new Rectangle(
                    r.X + 66,
                    r.Y,
                    r.Width - 84,
                    r.Height
                );

                g.DrawString(
                    "복구 중 수집한 Recovered Coin으로 사용할 도구를 선택하세요.",
                    f,
                    b,
                    textRect,
                    sf
                );
            }
        }

        private void DrawStatusBox(Graphics g, Rectangle r, PlayerState player)
        {
            using (Font labelFont = Renderer.F(9.4f, FontStyle.Bold))
            using (Font valueFont = Renderer.F(9.8f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (Pen dividerPen = new Pen(Color.FromArgb(145, 138, 118), 2f))
            using (StringFormat left = LeftMiddle())
            {
                int y1 = r.Y + 2;
                int y2 = r.Y + 22;
                int lineH = 18;

                int leftLabelX = r.X + 18;
                int leftValueX = r.X + 205;

                int rightLabelX = r.X + 345;
                int rightValueX = r.X + 525;

                g.DrawString(
                    "Recovered Coin:",
                    labelFont,
                    textBrush,
                    new Rectangle(leftLabelX, y1, 170, lineH),
                    left
                );

                g.DrawString(
                    player.Coins.ToString(),
                    valueFont,
                    blueBrush,
                    new Rectangle(leftValueX, y1, 40, lineH),
                    left
                );

                g.DrawString(
                    "Recovery Kit:",
                    labelFont,
                    textBrush,
                    new Rectangle(leftLabelX, y2, 170, lineH),
                    left
                );

                g.DrawString(
                    player.HpPotions.ToString(),
                    valueFont,
                    blueBrush,
                    new Rectangle(leftValueX, y2, 40, lineH),
                    left
                );

                g.DrawString(
                    "Memory Kit:",
                    labelFont,
                    textBrush,
                    new Rectangle(rightLabelX, y2, 150, lineH),
                    left
                );

                g.DrawString(
                    player.MpPotions.ToString(),
                    valueFont,
                    blueBrush,
                    new Rectangle(rightValueX, y2, 40, lineH),
                    left
                );

                g.DrawLine(dividerPen, r.X + 4, r.Bottom - 2, r.Right - 4, r.Bottom - 2);
            }
        }

        private void DrawItemList(
            Graphics g,
            Rectangle r,
            string selectedItem,
            List<UiButton> buttons)
        {
            DrawGroupBox(g, r, "사용 가능 도구");

            Rectangle inner = new Rectangle(
                r.X + 10,
                r.Y + 36,
                r.Width - 20,
                r.Height - 42
                );

            //DrawInsetPanel(g, inner);

            int rowHeight = inner.Height / 3;

            DrawItemRow(
                g,
                new Rectangle(inner.X, inner.Y, inner.Width, rowHeight),
                1,
                recoveryKitImage,
                recoveryKitSource,
                "Recovery Kit",
                "30 coin",
                "selecthp",
                selectedItem == ItemHp,
                buttons
            );

            DrawItemRow(
                g,
                new Rectangle(inner.X, inner.Y + rowHeight, inner.Width, rowHeight),
                2,
                memoryKitImage,
                memoryKitSource,
                "Memory Kit",
                "30 coin",
                "selectmp",
                selectedItem == ItemMp,
                buttons
            );

            DrawItemRow(
                g,
                new Rectangle(inner.X, inner.Y + rowHeight * 2, inner.Width, inner.Height - rowHeight * 2),
                3,
                bundleImage,
                bundleSource,
                "Supply Bundle",
                "110 coin",
                "selectbundle",
                selectedItem == ItemBundle,
                buttons
            );
        }

        private void DrawItemRow(
            Graphics g,
            Rectangle row,
            int number,
            Image iconImage,
            Rectangle sourceRect,
            string name,
            string price,
            string actionId,
            bool selected,
            List<UiButton> buttons)
        {
            if (selected)
            {
                using (SolidBrush sel = new SolidBrush(Color.FromArgb(238, 235, 222)))
                    g.FillRectangle(sel, row);

                using (Pen p = new Pen(Color.FromArgb(140, 135, 120), 2f))
                    g.DrawRectangle(p, row.X + 2, row.Y + 2, row.Width - 3, row.Height - 3);
            }

            if (number > 1)
            {
                using (Pen line = new Pen(Color.FromArgb(190, 178, 158)))
                    g.DrawLine(line, row.X + 6, row.Y, row.Right - 6, row.Y);
            }

            int iconSize = Math.Min(40, row.Height - 8);
            Rectangle iconRect = new Rectangle(
                row.X + 18,
                row.Y + (row.Height - iconSize) / 2,
                iconSize,
                iconSize
            );

            DrawItemIcon(g, iconImage, sourceRect, iconRect);

            using (Font itemFont = Renderer.F(9.8f, FontStyle.Bold))
            using (SolidBrush itemBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat leftMiddle = LeftMiddle())
            using (StringFormat rightMiddle = RightMiddle())
            {
                Rectangle nameRect = new Rectangle(
                    row.X + 82,
                    row.Y,
                    row.Width - 190,
                    row.Height
                );

                Rectangle priceRect = new Rectangle(
                    row.Right - 102,
                    row.Y,
                    90,
                    row.Height
                );

                g.DrawString(number + ". " + name, itemFont, itemBrush, nameRect, leftMiddle);
                g.DrawString(price, itemFont, itemBrush, priceRect, rightMiddle);
            }

            if (buttons != null)
                buttons.Add(new UiButton(row, actionId));
        }

        // [RecoveryToolsUI.cs 내부: 2줄 설명글 위아래 겹침 및 잘림 현상 전면 처단 최종 완결판]
        private void DrawDescriptionBox(Graphics g, Rectangle r, string selectedItem)
        {
            DrawGroupBox(g, r, "선택 항목 설명");

            Rectangle inner = new Rectangle(
                r.X + 10,
                r.Y + 36,
                r.Width - 20,
                r.Height - 42
            );

            ShopItemInfo info = GetShopItemInfo(selectedItem);

            Rectangle iconRect = new Rectangle(inner.X + 20, inner.Y + 12, 48, 48);
            DrawItemIcon(g, info.IconImage, info.IconSource, iconRect);

            using (Font titleFont = Renderer.F(12.8f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle titleRect = new Rectangle(
                    iconRect.Right + 16,
                    inner.Y + 14,
                    inner.Width - 104,
                    32
                );

                g.DrawString(info.Title, titleFont, titleBrush, titleRect, sf);
            }

            int lineY = inner.Y + 66;

            using (Pen line = new Pen(Color.FromArgb(145, 138, 118), 2f))
            {
                g.DrawLine(line, inner.X + 18, lineY, inner.Right - 18, lineY);
            }

            // =============================================================================
            //  22px, 24px로 쪼개져 텍스트를 파쇄하던 낡은 이중 박스를 영구 폐기합니다!
            // =============================================================================
            // 위아래 문장이 겹치지 않고 자연스러운 행간 여유를 가지며 자동으로 줄바꿈 되도록,
            // 가로세로를 통째로 아우르는 높이 65px짜리 거대 통합 설명 상자를 신설합니다.
            Rectangle unifiedDescRect = new Rectangle(
                inner.X + 20,
                lineY + 12,
                inner.Width - 40,
                65
            );

            // 폰트 크기를 표준 규격인 10f 수준으로 단일 통일하여 해상도 엇박자를 방지합니다.
            using (Font descFont = Renderer.F(10.0f, FontStyle.Bold))
            using (SolidBrush bodyBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            {
                // 줄바꿈 정렬 시 윗줄과 아랫줄이 절대 간섭하지 못하도록 정방향 전용 포맷터 빌드
                using (StringFormat descLayoutFormat = new StringFormat())
                {
                    descLayoutFormat.Alignment = StringAlignment.Near;     // 좌측 정렬
                    descLayoutFormat.LineAlignment = StringAlignment.Near; // 상단 정렬

                    // 영역 초과 스트레스 억까 및 외곽선 크롭 가드를 전면 해제
                    descLayoutFormat.FormatFlags = StringFormatFlags.NoClip;
                    descLayoutFormat.Trimming = StringTrimming.None;

                    // 쪼개진 파편 드로우를 중단하고, 2줄 전체 문장을 통합 상자 안에서 깨끗하게 한 번에 렌더링!
                    g.DrawString(info.Description, descFont, bodyBrush, unifiedDescRect, descLayoutFormat);
                }
            }
            // =============================================================================
        }

        private ShopItemInfo GetShopItemInfo(string selectedItem)
        {
            if (selectedItem == ItemMp)
            {
                return new ShopItemInfo(
                    "Memory Kit",
                    "MP 포션 1개를 보충합니다.\n던전에서는 F 키로 사용합니다.",
                    memoryKitImage,
                    memoryKitSource
                );
            }

            if (selectedItem == ItemBundle)
            {
                return new ShopItemInfo(
                    "Supply Bundle",
                    "Recovery Kit 2개와 Memory Kit 2개를 함께 보충합니다.\n묶음 가격은 110 coin입니다.",
                    bundleImage,
                    bundleSource
                );
            }

            return new ShopItemInfo(
                "Recovery Kit",
                "HP 포션 1개를 보충합니다.\n던전에서는 D 키로 사용합니다.",
                recoveryKitImage,
                recoveryKitSource
            );
        }

        private void DrawFooter(Graphics g, Rectangle r, List<UiButton> buttons)
        {
            //DrawInsetPanel(g, r);

            using (Font f = Renderer.F(8.5f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 70, 70)))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle textRect = new Rectangle(r.X + 18, r.Y, r.Width - 160, r.Height);

                g.DrawString(
                    "도구를 선택한 뒤 [확인]을 누르면 구매합니다.",
                    f,
                    b,
                    textRect,
                    sf
                );
            }

            Rectangle ok = new Rectangle(r.Right - 126, r.Y + 1, 104, 26);

            SystemWindowUI.Shared.DrawDialogImageButton(
                g,
                ok,
                SystemWindowButtonKind.Ok,
                "confirmShopPurchase",
                buttons
            );
        }

        private void DrawGroupBox(Graphics g, Rectangle r, string title)
        {
            using (Font titleFont = Renderer.F(10.5f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (Pen linePen = new Pen(Color.FromArgb(185, 180, 155), 1f))
            {
                Rectangle titleRect = new Rectangle(
                    r.X + 10,
                    r.Y + 2,
                    r.Width - 20,
                    26
                );

                g.DrawString(
                    title,
                    titleFont,
                    titleBrush,
                    titleRect,
                    LeftMiddle()
                );

                g.DrawLine(
                    linePen,
                    r.X + 10,
                    r.Y + 31,
                    r.Right - 10,
                    r.Y + 31
                );
            }
        }

        private void DrawInsetPanel(Graphics g, Rectangle r)
        {
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(255, 252, 240)))
                g.FillRectangle(fill, r);

            using (Pen outer = new Pen(Color.FromArgb(185, 175, 155), 1.3f))
                g.DrawRectangle(outer, r);

            using (Pen inner = new Pen(Color.FromArgb(255, 255, 248), 1f))
                g.DrawRectangle(inner, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
        }

        private void DrawInfoIcon(Graphics g, Rectangle r)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(
                r,
                Color.FromArgb(88, 170, 255),
                Color.FromArgb(0, 65, 210),
                90f))
            {
                g.FillEllipse(b, r);
            }

            using (Pen p = new Pen(Color.White, 2f))
                g.DrawEllipse(p, r);

            using (Font f = Renderer.F(16f, FontStyle.Bold))
            using (SolidBrush wb = new SolidBrush(Color.White))
            using (StringFormat sf = CenterMiddle())
            {
                g.DrawString("i", f, wb, r, sf);
            }
        }

        private void DrawMonitorIcon(Graphics g, Rectangle r)
        {
            using (SolidBrush body = new SolidBrush(Color.FromArgb(50, 62, 70)))
                g.FillRectangle(body, r);

            using (Pen edge = new Pen(Color.FromArgb(180, 190, 196), 3f))
                g.DrawRectangle(edge, r);

            Rectangle screen = new Rectangle(r.X + 7, r.Y + 7, r.Width - 14, r.Height - 16);

            using (SolidBrush black = new SolidBrush(Color.FromArgb(18, 24, 24)))
                g.FillRectangle(black, screen);

            using (Pen wave = new Pen(Color.FromArgb(60, 230, 80), 2f))
            {
                g.DrawLine(wave, screen.X + 4, screen.Y + 22, screen.X + 12, screen.Y + 12);
                g.DrawLine(wave, screen.X + 12, screen.Y + 12, screen.X + 20, screen.Y + 20);
                g.DrawLine(wave, screen.X + 20, screen.Y + 20, screen.X + 28, screen.Y + 9);
                g.DrawLine(wave, screen.X + 28, screen.Y + 9, screen.X + 36, screen.Y + 17);
            }
        }

        private void DrawItemIcon(Graphics g, Image image, Rectangle sourceRect, Rectangle destRect)
        {
            if (image == null)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 240, 255)))
                    g.FillRectangle(b, destRect);

                using (Pen p = new Pen(Color.FromArgb(40, 90, 180), 2f))
                    g.DrawRectangle(p, destRect);

                return;
            }

            if (sourceRect.IsEmpty)
                sourceRect = new Rectangle(0, 0, image.Width, image.Height);

            Rectangle fitRect = GetFitRect(sourceRect.Size, destRect, 4);

            InterpolationMode oldInterpolation = g.InterpolationMode;
            PixelOffsetMode oldPixelOffset = g.PixelOffsetMode;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            g.DrawImage(image, fitRect, sourceRect, GraphicsUnit.Pixel);

            g.InterpolationMode = oldInterpolation;
            g.PixelOffsetMode = oldPixelOffset;
        }

        private Rectangle GetFitRect(Size sourceSize, Rectangle destRect, int padding)
        {
            if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
                return destRect;

            int targetW = Math.Max(1, destRect.Width - padding * 2);
            int targetH = Math.Max(1, destRect.Height - padding * 2);

            float scaleX = (float)targetW / sourceSize.Width;
            float scaleY = (float)targetH / sourceSize.Height;
            float scale = Math.Min(scaleX, scaleY);

            int drawW = Math.Max(1, (int)(sourceSize.Width * scale));
            int drawH = Math.Max(1, (int)(sourceSize.Height * scale));

            int x = destRect.X + (destRect.Width - drawW) / 2;
            int y = destRect.Y + (destRect.Height - drawH) / 2;

            return new Rectangle(x, y, drawW, drawH);
        }

        private Rectangle GetVisibleBounds(Image image)
        {
            if (image == null)
                return Rectangle.Empty;

            Bitmap bitmap = image as Bitmap;
            if (bitmap == null)
                return new Rectangle(0, 0, image.Width, image.Height);

            int minX = bitmap.Width;
            int minY = bitmap.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);

                    if (color.A <= 10)
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
                return new Rectangle(0, 0, image.Width, image.Height);

            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

        private StringFormat LeftMiddle()
        {
            return new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };
        }

        private StringFormat RightMiddle()
        {
            return new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };
        }

        private StringFormat CenterMiddle()
        {
            return new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };
        }

        private StringFormat LeftTopTrim()
        {
            return new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.Word,
                FormatFlags = StringFormatFlags.LineLimit
            };
        }

        private sealed class ShopItemInfo
        {
            public string Title;
            public string Description;
            public Image IconImage;
            public Rectangle IconSource;

            public ShopItemInfo(string title, string description, Image iconImage, Rectangle iconSource)
            {
                Title = title;
                Description = description;
                IconImage = iconImage;
                IconSource = iconSource;
            }
        }
    }
}