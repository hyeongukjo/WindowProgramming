using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    /// <summary>
    /// 인트로가 끝난 뒤가 아니라, 프로그램 시작 즉시 보여주는 ADMIN 시작 메뉴 화면입니다.
    /// 중요: 사용자가 제공한 AdminStartMenuExact.png 이미지를 그대로 전체 화면에 출력합니다.
    /// 아이콘, ADMIN 창, 글자, 작업표시줄은 코드로 다시 그리지 않고 이미지 위에 투명 클릭 영역만 둡니다.
    /// </summary>
    public static class StartMenuScreen
    {
        private const int SourceWidth = 1672;
        private const int SourceHeight = 933;

        // 사용자가 제공한 기준 이미지 안의 버튼 위치입니다.
        // 이미지를 그대로 쓰기 위해 버튼은 보이지 않는 투명 클릭 영역만 등록합니다.
        private static readonly Rectangle StartButtonSource = new Rectangle(650, 574, 378, 72);
        private static readonly Rectangle ContinueButtonSource = new Rectangle(650, 646, 378, 70);
        private static readonly Rectangle ExitButtonSource = new Rectangle(650, 716, 378, 72);

        private static Image adminImage;

        private static Image AdminImage
        {
            get
            {
                if (adminImage == null)
                {
                    string path = Path.Combine(Application.StartupPath, "Assets", "AdminStartMenuExact.png");
                    if (File.Exists(path))
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            adminImage = Image.FromStream(new MemoryStream(ms.ToArray()));
                        }
                    }
                }
                return adminImage;
            }
        }

        public static void Draw(Graphics g, Rectangle client, List<UiButton> buttons, bool hasSave)
        {
            Image img = AdminImage;
            g.Clear(Color.Black);

            if (img != null)
            {
                InterpolationMode oldInterpolation = g.InterpolationMode;
                PixelOffsetMode oldPixel = g.PixelOffsetMode;
                SmoothingMode oldSmooth = g.SmoothingMode;

                // 픽셀 이미지가 흐릿해지지 않도록 가장 가까운 픽셀 방식으로 확대/축소합니다.
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.None;
                g.DrawImage(img, client);

                g.InterpolationMode = oldInterpolation;
                g.PixelOffsetMode = oldPixel;
                g.SmoothingMode = oldSmooth;
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 90, 220)))
                    g.FillRectangle(b, client);
                using (Font f = new Font("Arial", 36, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.White))
                    g.DrawString("ADMIN", f, b, new Rectangle(0, client.Height / 2 - 60, client.Width, 80), Center());
            }

            buttons.Add(new UiButton(Scale(StartButtonSource, client), "adminStart"));
            buttons.Add(new UiButton(Scale(ContinueButtonSource, client), "adminContinue"));
            buttons.Add(new UiButton(Scale(ExitButtonSource, client), "adminExit"));

            // 저장 파일이 없을 때도 이미지를 바꾸지 않습니다.
            // 사용자가 요구한 화면 그대로 유지하기 위해 Continue 비활성 표시는 그리지 않고,
            // 클릭했을 때만 동작하지 않도록 처리합니다.
        }

        private static Rectangle Scale(Rectangle src, Rectangle client)
        {
            float sx = client.Width / (float)SourceWidth;
            float sy = client.Height / (float)SourceHeight;
            return new Rectangle(
                client.Left + (int)Math.Round(src.X * sx),
                client.Top + (int)Math.Round(src.Y * sy),
                (int)Math.Round(src.Width * sx),
                (int)Math.Round(src.Height * sy));
        }

        private static StringFormat Center()
        {
            return new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        }
    }
}
