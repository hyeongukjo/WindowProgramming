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
            int panelWidth = 358;

            int topY = 48;
            int margin = 12;
            int titleAreaHeight = 30;
            int gap = 2;
            int bottomPadding = 30;

            int programH = 64;
            int itemH = 128;
            int commandH = 132;
            int progressH = 78;

            int panelHeight =
                titleAreaHeight +
                programH + gap +
                itemH + gap +
                commandH + gap +
                progressH +
                bottomPadding;

            Rectangle win = new Rectangle(
                client.Width - panelWidth - 24,
                topY,
                panelWidth,
                panelHeight
            );

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

            int y = win.Y + titleAreaHeight;
            int innerWidth = win.Width - margin * 2;

            Rectangle programBox = new Rectangle(win.X + margin, y, innerWidth, programH);
            y = programBox.Bottom + gap;

            Rectangle itemBox = new Rectangle(win.X + margin, y, innerWidth, itemH);
            y = itemBox.Bottom + gap;

            Rectangle commandBox = new Rectangle(win.X + margin, y, innerWidth, commandH);
            y = commandBox.Bottom + gap;

            Rectangle progressBox = new Rectangle(win.X + margin, y, innerWidth, progressH);

            DrawDesktopProgramBox(g, programBox, player);
            DrawDesktopItemBox(g, itemBox, player);
            DrawDesktopCommandBox(g, commandBox);
            DrawDesktopProgressBox(g, progressBox, player, unlockedStage, totalStages);
        }

        private void DrawDesktopProgramBox(Graphics g, Rectangle r, PlayerState player)
        {
            DrawGroupBox(g, r, "프로그램 상태");

            using (Font f = Renderer.F(8.4f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (StringFormat sf = LeftMiddle())
            {
                string profileName = string.IsNullOrWhiteSpace(player.ProfileName)
                    ? "ProfileName"
                    : player.ProfileName;

                g.DrawString(
                    "Profile: " + profileName,
                    f,
                    textBrush,
                    new Rectangle(r.X + 12, r.Y + 31, r.Width - 24, 17),
                    sf
                );

                g.DrawString(
                    "Level " + player.Level + "    Weapon +" + player.WeaponLevel,
                    f,
                    blueBrush,
                    new Rectangle(r.X + 12, r.Y + 49, r.Width - 24, 17),
                    sf
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
            Rectangle icon = new Rectangle(row.X + 2, row.Y + 1, 32, 32);
            DrawItemIcon(g, iconImage, iconSource, icon);

            using (Font f = Renderer.F(8.1f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (StringFormat left = LeftMiddle())
            using (StringFormat right = RightMiddle())
            {
                g.DrawString(
                    itemName,
                    f,
                    textBrush,
                    new Rectangle(icon.Right + 7, row.Y, 130, row.Height),
                    left
                );

                g.DrawString(
                    "x" + count,
                    f,
                    blueBrush,
                    new Rectangle(row.Right - 82, row.Y, 34, row.Height),
                    right
                );

                g.DrawString(
                    shortcutText,
                    f,
                    textBrush,
                    new Rectangle(row.Right - 44, row.Y, 44, row.Height),
                    right
                );
            }
        }
        private void DrawDesktopItemBox(Graphics g, Rectangle r, PlayerState player)
        {
            DrawGroupBox(g, r, "보유 도구");

            int rowY = r.Y + 33;
            int rowHeight = 34;

            DrawDesktopCoinRow(
                g,
                new Rectangle(r.X + 10, rowY, r.Width - 20, rowHeight),
                player.Coins
            );

            DrawDesktopItemRow(
                g,
                new Rectangle(r.X + 10, rowY + rowHeight, r.Width - 20, rowHeight),
                recoveryKitImage,
                recoveryKitSource,
                "Recovery Kit",
                player.HpPotions,
                "D 사용"
            );

            DrawDesktopItemRow(
                g,
                new Rectangle(r.X + 10, rowY + rowHeight * 2, r.Width - 20, rowHeight),
                memoryKitImage,
                memoryKitSource,
                "Memory Kit",
                player.MpPotions,
                "F 사용"
            );
        }

        private void DrawDesktopCoinRow(Graphics g, Rectangle row, int coins)
        {
            using (Font f = Renderer.F(8.8f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (StringFormat left = LeftMiddle())
            using (StringFormat right = RightMiddle())
            {
                g.DrawString(
                    "Recovered Coin",
                    f,
                    textBrush,
                    new Rectangle(row.X + 4, row.Y, 170, row.Height),
                    left
                );

                g.DrawString(
                    coins.ToString(),
                    f,
                    blueBrush,
                    new Rectangle(row.Right - 90, row.Y, 86, row.Height),
                    right
                );
            }
        }

        private void DrawDesktopCommandBox(Graphics g, Rectangle r)
        {
            DrawGroupBox(g, r, "복구 명령");

            int y = r.Y + 34;
            int lineHeight = 18;

            DrawCommandLine(g, r.X + 12, y + lineHeight * 0, r.Width - 24, "Q", "Quick Scan", "기본 공격");
            DrawCommandLine(g, r.X + 12, y + lineHeight * 1, r.Width - 24, "W", "Data Sweep", "범위 정리");
            DrawCommandLine(g, r.X + 12, y + lineHeight * 2, r.Width - 24, "E", "Error Flash", "오류 섬광");
            DrawCommandLine(g, r.X + 12, y + lineHeight * 3, r.Width - 24, "R", "Guard Process", "방어 / 안정화");

            using (Font f = Renderer.F(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 70, 70)))
            using (StringFormat sf = LeftMiddle())
            {
                g.DrawString(
                    "Mouse Click: 이동 / 부드러운 추적",
                    f,
                    b,
                    new Rectangle(r.X + 12, y + lineHeight * 4 + 3, r.Width - 24, 18),
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
     string description)
        {
            using (Font keyFont = Renderer.F(8.2f, FontStyle.Bold))
            using (Font textFont = Renderer.F(8.0f, FontStyle.Bold))
            using (SolidBrush keyBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat left = LeftMiddle())
            {
                g.DrawString(key + ":",keyFont,keyBrush,new Rectangle(x, y, 24, 19),left);
                g.DrawString(commandName + "  -  " + description,textFont,textBrush,new Rectangle(x + 28, y, width - 28, 19),left);
            }
        }
        private void DrawDesktopProgressBox(
    Graphics g,
    Rectangle r,
    PlayerState player,
    int unlockedStage,
    int totalStages)
        {
            DrawGroupBox(g, r, "진행 상태");

            using (Font f = Renderer.F(7.9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat sf = LeftMiddle())
            {
                int y = r.Y + 33;
                int lineHeight = 16;

                g.DrawString(
                    "해금된 파일: " + unlockedStage + " / " + totalStages,
                    f,
                    b,
                    new Rectangle(r.X + 12, y, r.Width - 24, lineHeight),
                    sf
                );

                g.DrawString(
                    "클리어: " + player.ClearedStages + " / " + totalStages,
                    f,
                    b,
                    new Rectangle(r.X + 12, y + lineHeight, r.Width - 24, lineHeight),
                    sf
                );

                g.DrawString(
                    "격리된 보스: " + player.QuarantinedBosses,
                    f,
                    b,
                    new Rectangle(r.X + 12, y + lineHeight * 2, r.Width - 24, lineHeight),
                    sf
                );
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
                client.Width / 2 - 460,
                client.Height / 2 - 286,
                920,
                572
            );

            DrawShopFrame(g, win, buttons);

            int margin = 28;
            int contentTop = win.Y + 96;

            Rectangle guideBox = new Rectangle(
                win.X + margin,
                contentTop,
                win.Width - margin * 2,
                54
            );

            Rectangle statusBox = new Rectangle(
                win.X + margin,
                guideBox.Bottom + 10,
                win.Width - margin * 2,
                70
            );

            Rectangle footerBox = new Rectangle(
                win.X + margin,
                win.Bottom - 76,
                win.Width - margin * 2,
                50
            );

            int mainTop = statusBox.Bottom + 14;
            int mainHeight = footerBox.Y - mainTop - 14;
            int gap = 16;
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

            using (Font titleFont = Renderer.F(13.5f, FontStyle.Bold))
            using (SolidBrush white = new SolidBrush(Color.White))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle titleRect = new Rectangle(
                    win.X + 28,
                    win.Y + 6,
                    win.Width - 130,
                    38
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
                win.Right - 88,
                win.Y + 8,
                62,
                58
            );

            if (buttons != null)
                buttons.Add(new UiButton(closeRect, "shopBack"));
        }

        private void DrawGuideBox(Graphics g, Rectangle r)
        {
            DrawInsetPanel(g, r);

            Rectangle icon = new Rectangle(r.X + 24, r.Y + 11, 32, 32);
            DrawInfoIcon(g, icon);

            using (Font f = Renderer.F(11.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle textRect = new Rectangle(
                    r.X + 78,
                    r.Y,
                    r.Width - 96,
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
            DrawInsetPanel(g, r);

            Rectangle monitor = new Rectangle(r.X + 24, r.Y + 12, 46, 46);
            DrawMonitorIcon(g, monitor);

            using (Font f = Renderer.F(10.2f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 62, 160)))
            {
                int x = r.X + 94;
                int y1 = r.Y + 13;
                int y2 = r.Y + 39;

                g.DrawString("보유 Recovered Coin:", f, textBrush, x, y1);
                g.DrawString(player.Coins.ToString(), f, blueBrush, x + 205, y1);

                g.DrawString("보유 Recovery Kit:", f, textBrush, x, y2);
                g.DrawString(player.HpPotions.ToString(), f, blueBrush, x + 205, y2);

                g.DrawString("/  보유 Memory Kit:", f, textBrush, x + 250, y2);
                g.DrawString(player.MpPotions.ToString(), f, blueBrush, x + 465, y2);
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
                r.X + 14,
                r.Y + 54,
                r.Width - 28,
                r.Height - 72
            );

            DrawInsetPanel(g, inner);

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
                "25 coin",
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
                "90 coin",
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

            int iconSize = Math.Min(54, row.Height - 10);
            Rectangle iconRect = new Rectangle(
                row.X + 20,
                row.Y + (row.Height - iconSize) / 2,
                iconSize,
                iconSize
            );

            DrawItemIcon(g, iconImage, sourceRect, iconRect);

            using (Font itemFont = Renderer.F(10.8f, FontStyle.Bold))
            using (SolidBrush itemBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat leftMiddle = LeftMiddle())
            using (StringFormat rightMiddle = RightMiddle())
            {
                Rectangle nameRect = new Rectangle(
                    row.X + 100,
                    row.Y,
                    row.Width - 220,
                    row.Height
                );

                Rectangle priceRect = new Rectangle(
                    row.Right - 112,
                    row.Y,
                    100,
                    row.Height
                );

                g.DrawString(number + ". " + name, itemFont, itemBrush, nameRect, leftMiddle);
                g.DrawString(price, itemFont, itemBrush, priceRect, rightMiddle);
            }

            if (buttons != null)
                buttons.Add(new UiButton(row, actionId));
        }

        private void DrawDescriptionBox(Graphics g, Rectangle r, string selectedItem)
        {
            DrawGroupBox(g, r, "선택 항목 설명");

            Rectangle inner = new Rectangle(
                r.X + 14,
                r.Y + 54,
                r.Width - 28,
                r.Height - 72
            );

            DrawInsetPanel(g, inner);

            ShopItemInfo info = GetShopItemInfo(selectedItem);

            Rectangle iconRect = new Rectangle(inner.X + 22, inner.Y + 18, 64, 64);
            DrawItemIcon(g, info.IconImage, info.IconSource, iconRect);

            using (Font titleFont = Renderer.F(13.2f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle titleRect = new Rectangle(
                    iconRect.Right + 18,
                    inner.Y + 18,
                    inner.Width - 126,
                    42
                );

                g.DrawString(info.Title, titleFont, titleBrush, titleRect, sf);
            }

            using (Pen line = new Pen(Color.FromArgb(190, 178, 158)))
                g.DrawLine(line, inner.X + 20, inner.Y + 96, inner.Right - 20, inner.Y + 96);

            using (Font bodyFont = Renderer.F(10.2f, FontStyle.Regular))
            using (SolidBrush bodyBrush = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (StringFormat sf = LeftTopTrim())
            {
                Rectangle bodyRect = new Rectangle(
                    inner.X + 22,
                    inner.Y + 112,
                    inner.Width - 44,
                    inner.Height - 126
                );

                g.DrawString(info.Description, bodyFont, bodyBrush, bodyRect, sf);
            }
        }

        private ShopItemInfo GetShopItemInfo(string selectedItem)
        {
            if (selectedItem == ItemMp)
            {
                return new ShopItemInfo(
                    "Memory Kit",
                    "MP 포션 1개를 보충합니다.\n" +
                    "던전에서는 F 키로 사용합니다.\n\n" +
                    "구매하려면 아래 [확인]을 누르세요.",
                    memoryKitImage,
                    memoryKitSource
                );
            }

            if (selectedItem == ItemBundle)
            {
                return new ShopItemInfo(
                    "Supply Bundle",
                    "Recovery Kit 2개와 Memory Kit 2개를 함께 보충합니다.\n" +
                    "묶음 가격은 90 coin입니다.\n\n" +
                    "구매하려면 아래 [확인]을 누르세요.",
                    bundleImage,
                    bundleSource
                );
            }

            return new ShopItemInfo(
                "Recovery Kit",
                "HP 포션 1개를 보충합니다.\n" +
                "던전에서는 D 키로 사용합니다.\n\n" +
                "구매하려면 아래 [확인]을 누르세요.",
                recoveryKitImage,
                recoveryKitSource
            );
        }

        private void DrawFooter(Graphics g, Rectangle r, List<UiButton> buttons)
        {
            DrawInsetPanel(g, r);

            using (Font f = Renderer.F(9.3f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 70, 70)))
            using (StringFormat sf = LeftMiddle())
            {
                Rectangle textRect = new Rectangle(r.X + 18, r.Y, r.Width - 190, r.Height);

                g.DrawString(
                    "왼쪽 목록에서 도구를 선택한 뒤, 구매하려면 [확인]을 누르세요.",
                    f,
                    b,
                    textRect,
                    sf
                );
            }

            Rectangle ok = new Rectangle(r.Right - 150, r.Y + 10, 120, 30);

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