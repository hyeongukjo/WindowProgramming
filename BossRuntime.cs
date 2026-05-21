using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    public sealed class BossRuntime
    {
        public readonly BossPatternManager patternManager = new BossPatternManager();
        private int currentStage;

        public void Reset(int stageIndex)
        {
            currentStage = stageIndex;
            patternManager.Reset();
        }

        public void Update(int stageIndex, GameEntity boss, PlayerState player, List<Effect> effects, Rectangle client, float mapWidth)
        {
            currentStage = stageIndex;
            patternManager.Update(boss, player, effects, mapWidth);
        }

        public bool HandleClick(Point mousePos)
        {
            return patternManager.HandleClick(mousePos);
        }

        public void DrawOverlay(Graphics g, int stageIndex, bool stageBossPhase, float cameraX, Size clientSize)
        {
            if (!stageBossPhase) return;
            DrawProjectiles(g, cameraX);
            DrawShardPattern(g, cameraX);
            DrawResourcePattern(g, clientSize);

            // 💡 [핵심 추가] 눈에 보이지 않던 BSOD 드래곤의 기믹 요소들을 화면에 직접 드로우합니다.
            DrawBSODGimmicks(g, cameraX, clientSize);

            DrawExceptionQueenGimmicks(g, cameraX, clientSize);
            DrawIllegalBinnyGimmicks(g, cameraX, clientSize);

            DrawNotice(g, clientSize);
        }

        // ==========================================
        // 5번 보스 (Illegal_Binny) 기믹 시각화 드로우
        // ==========================================
        private void DrawIllegalBinnyGimmicks(Graphics g, float cameraX, Size clientSize)
        {
            // 1. 75%, 25% 패턴: 영구 소거 소용돌이 블랙홀 시각화
            if (patternManager.IsBlackholeActive)
            {
                float bx = patternManager.AnchorPos.X - cameraX;
                float by = patternManager.AnchorPos.Y;

                // 중력장이 왜곡되는 듯한 3중 동심원 네온 오라 효과 연출
                int pulse = (Environment.TickCount / 4) % 360;
                using (Pen p1 = new Pen(Color.FromArgb(120, Color.DarkRed), 4f))
                using (Pen p2 = new Pen(Color.FromArgb(200, Color.Red), 1.5f))
                {
                    p2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawEllipse(p1, bx - 150, by - 150, 300, 300);
                    g.DrawEllipse(p2, bx - (100 + (float)Math.Sin(pulse * Math.PI / 180) * 15), by - (100 + (float)Math.Sin(pulse * Math.PI / 180) * 15), 200, 200);
                }

                // 블랙홀의 소거 코어 핵 드로우
                using (SolidBrush coreBr = new SolidBrush(Color.FromArgb(230, 15, 15, 15)))
                {
                    g.FillEllipse(coreBr, bx - 45, by - 45, 90, 90);
                }
                g.DrawEllipse(Pens.Red, bx - 45, by - 45, 90, 90);

                // 상단 보스 상태 알림 오버레이
                using (Font font = Renderer.F(11f, FontStyle.Bold))
                {
                    g.DrawString($"⚠️ 치명적 중력 데이터 동기화 감지 (제한시간: {patternManager.BlackholeTimer / 60}초)", font, Brushes.Red, clientSize.Width / 2, 105, Renderer.Center());
                }
            }

            // 2. 50% 패턴: 디스크 포맷 레이저 스캐너 (줄넘기 가시화)
            if (patternManager.IsScannerActive)
            {
                float sx = patternManager.ScannerX - cameraX;
                float safeY = patternManager.SafeHoleY;

                // 스크린 상단부터 하단까지 내리꽂히는 묵직한 수직 레이저 스케일선 드로우
                using (Pen laserWall = new Pen(Color.FromArgb(220, Color.OrangeRed), 5f))
                using (Pen laserCore = new Pen(Color.White, 1.5f))
                {
                    // 💡 [줄넘기 구멍 구현] 안전 구역(SafeHoleY)의 틈새 위아래까지만 레이저를 끊어서 그립니다!
                    g.DrawLine(laserWall, sx, 0, sx, safeY - 45);
                    g.DrawLine(laserCore, sx, 0, sx, safeY - 45);

                    g.DrawLine(laserWall, sx, safeY + 45, sx, clientSize.Height);
                    g.DrawLine(laserCore, sx, safeY + 45, sx, clientSize.Height);
                }

                // 레이저가 안전 구역을 통과하고 있음을 보여주는 유도 사각형 타겟팅 선
                using (Pen safeBorder = new Pen(Color.Lime, 2f))
                {
                    safeBorder.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    g.DrawRectangle(safeBorder, sx - 25, safeY - 45, 50, 90);
                }

                using (Font font = Renderer.F(10f, FontStyle.Bold))
                {
                    g.DrawString("ESCAPE THROUGH THE GAP!", font, Brushes.Lime, sx, safeY - 65, Renderer.Center());
                }
            }

            // 3. 10% 패턴: 타임어택 DPS 체크 변조 배리어 시각화
            if (patternManager.IsDPSCheckActive)
            {
                // 보스의 임시 월드 위치를 복구하여 오버레이 추적
                float bossScreenX = (clientSize.Width / 2) - cameraX; // 기믹 도중 중앙 고정 연산 반영
                float bossScreenY = 330;

                // 보스 몸 주위를 철통방어하는 데이터 철벽 쉴드 구체 시각화
                using (Pen shieldPen = new Pen(Color.FromArgb(200, Color.DeepSkyBlue), 4f))
                using (SolidBrush shieldBg = new SolidBrush(Color.FromArgb(35, Color.DodgerBlue)))
                {
                    g.FillEllipse(shieldBg, bossScreenX - 120, bossScreenY - 120, 240, 240);
                    g.DrawEllipse(shieldPen, bossScreenX - 120, bossScreenY - 120, 240, 240);
                }

                // 보스 머리 위에 장엄하게 표기되는 실시간 쉴드 체력 및 시간 미터기 오버레이
                Rectangle bar = new Rectangle(clientSize.Width / 2 - 150, 100, 300, 16);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(140, Color.Black))) g.FillRectangle(bg, bar);
                int fillW = (int)(bar.Width * (patternManager.DPSCheckTimer / 600f));
                using (SolidBrush fg = new SolidBrush(Color.DeepSkyBlue)) g.FillRectangle(fg, bar.X, bar.Y, fillW, bar.Height);
                using (Pen border = new Pen(Color.White, 1.5f)) g.DrawRectangle(border, bar);

                using (Font font = Renderer.F(11f, FontStyle.Bold))
                {
                    g.DrawString($"SYSTEM REBOOT BARRIER: {patternManager.BinnyShield} SHIELD", font, Brushes.White, clientSize.Width / 2, bar.Y - 18, Renderer.Center());
                }
            }

            // 4. 1% 패턴: 가비지 컬렉터 메모리 누수 분신 가시화
            if (patternManager.IsIllusionActive && patternManager.BinnyClone != null)
            {
                float cx = patternManager.BinnyClone.X - cameraX;
                float cy = patternManager.BinnyClone.Y;

                // 💡 [도형 임시 렌더링] 나중에 이 구역에 클론 전용 UI를 입히기 쉽도록, 네온 퍼플 네모 박스로 영역 박멸 표기
                using (SolidBrush cloneBr = new SolidBrush(Color.FromArgb(160, Color.Purple)))
                using (Pen clonePen = new Pen(Color.Magenta, 3f))
                {
                    g.FillRectangle(cloneBr, cx - 45, cy - 90, 90, 120);
                    g.DrawRectangle(clonePen, cx - 45, cy - 90, 90, 120);
                }

                // 분신용 임시 미니 체력바 오버레이 상단 표기
                Rectangle cloneHp = new Rectangle((int)cx - 45, (int)cy - 110, 90, 7);
                using (SolidBrush bg = new SolidBrush(Color.Black)) g.FillRectangle(bg, cloneHp);
                int fillW = (int)(cloneHp.Width * ((float)patternManager.BinnyClone.Hp / patternManager.BinnyClone.MaxHp));
                using (SolidBrush fg = new SolidBrush(Color.Purple)) g.FillRectangle(fg, cloneHp.X, cloneHp.Y, fillW, cloneHp.Height);
                g.DrawRectangle(Pens.White, cloneHp);
            }
        }

        // ==========================================
        // 4번 보스 (Exception Queen) 전용 비주얼 렌더링
        // ==========================================
        private void DrawExceptionQueenGimmicks(Graphics g, float cameraX, Size clientSize)
        {
            // 1. NullReference 궤도 유도 장판 드로우
            // 1. NullReference 궤도 유도 장판 및 실시간 디버그선 드로우
            if (patternManager.IsNullRefActive)
            {
                float nx = patternManager.NullRefNode.X - cameraX;
                float ny = patternManager.NullRefNode.Y;

                // 플레이어의 스크린 좌표 산출
                float px = patternManager.PlayerPos.X - cameraX;
                float py = patternManager.PlayerPos.Y;

                // 실제 월드 거리 연산
                float dx = patternManager.PlayerPos.X - patternManager.NullRefNode.X;
                float dy = patternManager.PlayerPos.Y - patternManager.NullRefNode.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                // 💡 [유도선 실시간 색상반전 시스템] 120~280 사이면 초록실선, 이탈 시 빨간점선
                using (Pen linkPen = new Pen(Color.White, 3f))
                {
                    if (dist >= 120 && dist <= 280)
                    {
                        linkPen.Color = Color.Lime; // 조건 만족 시 형광 초록색
                        linkPen.Width = 4f;
                    }
                    else
                    {
                        linkPen.Color = Color.Red;  // 범위 이탈 시 경고 빨간색
                        linkPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    }
                    // 노드의 중심과 플레이어의 허리(py - 25) 위치를 다이렉트로 연결하는 광선 드로우
                    g.DrawLine(linkPen, nx, ny, px, py - 25);
                }

                // 황금 공략 궤도 가이드 도넛 라인 렌더링
                using (Pen p1 = new Pen(Color.FromArgb(40, Color.Magenta), 6f))
                using (Pen p2 = new Pen(Color.FromArgb(150, Color.Magenta), 1.5f))
                {
                    g.DrawEllipse(p1, nx - 280, ny - 280, 560, 560);
                    g.DrawEllipse(p2, nx - 280, ny - 280, 560, 560); // 최대선 (280)
                    g.DrawEllipse(p2, nx - 120, ny - 120, 240, 240); // 최소선 (120)
                }

                // 중앙 코어 노드
                using (SolidBrush nodeCore = new SolidBrush(Color.Magenta))
                {
                    g.FillRectangle(nodeCore, nx - 15, ny - 15, 30, 30);
                }

                // 진행 상황 상단 미터기 바
                Rectangle bar = new Rectangle(clientSize.Width / 2 - 150, 95, 300, 15);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(120, Color.Black))) g.FillRectangle(bg, bar);
                int fillW = (int)(bar.Width * (patternManager.NullRefGauge / 100f));
                using (SolidBrush fg = new SolidBrush(Color.Magenta)) g.FillRectangle(fg, bar.X, bar.Y, fillW, bar.Height);
                using (Pen border = new Pen(Color.White, 1.5f)) g.DrawRectangle(border, bar);

                using (Font font = Renderer.F(10f, FontStyle.Bold))
                using (SolidBrush textBr = new SolidBrush(Color.White))
                {
                    g.DrawString($"REFERENCE LINK DEBUGER: {patternManager.NullRefGauge:0.0}%", font, textBr, clientSize.Width / 2, bar.Y - 18, Renderer.Center());
                }
            }

            // 2. Try-Catch 보안 예외구역 및 알파뉴메릭 해킹 문자열 드로우
            if (patternManager.IsTryCatchActive)
            {
                foreach (var zone in patternManager.CatchZones)
                {
                    float zx = zone.X - cameraX;
                    float zy = zone.Y;
                    bool isTarget = (zone.ExceptionName == patternManager.TargetException);

                    // 타겟 장판은 경고의 의미로 다크 옐로우/브릭 레드 네온 효과 연출
                    Color zoneColor = isTarget ? Color.FromArgb(190, 60, 0) : Color.FromArgb(40, 70, 90);
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(45, zoneColor)))
                    {
                        g.FillEllipse(sb, zx - 75, zy - 75, 150, 150);
                    }
                    using (Pen pen = new Pen(zoneColor, 2.5f))
                    {
                        if (isTarget) pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        g.DrawEllipse(pen, zx - 75, zy - 75, 150, 150);
                    }

                    using (Font f = Renderer.F(9f, FontStyle.Bold))
                    using (SolidBrush b = new SolidBrush(isTarget ? Color.Orange : Color.Gray))
                    {
                        g.DrawString($"[{zone.ExceptionName}]", f, b, zx, zy, Renderer.Center());
                    }
                }

                // 💡 [핵심] 타이핑 페이즈 활성화 시 캐릭터 머리 위에 해킹 코드 실시간 출력
                if (patternManager.IsTypingPhaseActive)
                {
                    // 화면 정중앙이나 텍스트 전용 위치에 출력 (여기선 캐릭터 위치 위쪽 스크린에 바인딩 추천)
                    using (Font typingFont = Renderer.F(24f, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.Lime))
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(180, Color.Black)))
                    {
                        string displayStr = patternManager.CurrentTypingTarget;
                        Size textRect = TextRenderer.MeasureText(displayStr, typingFont);
                        Rectangle backgroundBox = new Rectangle(clientSize.Width / 2 - textRect.Width / 2 - 20, 180, textRect.Width + 40, 50);

                        g.FillRectangle(bgBrush, backgroundBox);
                        g.DrawRectangle(Pens.Lime, backgroundBox);
                        g.DrawString(displayStr, typingFont, textBrush, clientSize.Width / 2, backgroundBox.Y + 25, Renderer.Center());
                    }
                }
            }

            // 3. StackOverflow 압박 온도계 미터기 및 컬렉터 스위치 드로우
            if (patternManager.IsStackOverflowActive)
            {
                // 위험 적재 미터기 우측 벽면에 배치
                Rectangle verticalBar = new Rectangle(clientSize.Width - 65, 180, 22, 280);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(140, Color.Black))) g.FillRectangle(bg, verticalBar);
                int fillH = (int)(verticalBar.Height * (patternManager.StackGauge / 100f));

                using (SolidBrush fg = new SolidBrush(patternManager.StackGauge > 75f ? Color.Red : Color.OrangeRed))
                {
                    g.FillRectangle(fg, verticalBar.X, verticalBar.Bottom - fillH, verticalBar.Width, fillH);
                }
                using (Pen border = new Pen(Color.White, 1.5f)) g.DrawRectangle(border, verticalBar);

                // 스위치 기믹 패드 드로우
                PointF currentSwitchPos = patternManager.TargetSwitch == 0 ? patternManager.SwitchLeftPos : patternManager.SwitchRightPos;
                float sx = currentSwitchPos.X - cameraX;
                float sy = currentSwitchPos.Y;

                using (SolidBrush switchBrush = new SolidBrush(Color.FromArgb(80, Color.Lime)))
                {
                    g.FillEllipse(switchBrush, sx - 65, sy - 65, 130, 130);
                }
                using (Pen switchPen = new Pen(Color.Lime, 3f))
                {
                    g.DrawEllipse(switchPen, sx - 65, sy - 65, 130, 130);
                }

                using (Font font = Renderer.F(10f, FontStyle.Bold))
                using (SolidBrush textBr = new SolidBrush(Color.White))
                {
                    g.DrawString("GC MEMORY FLUSH\n(이곳으로 도약)", font, textBr, sx, sy, Renderer.Center());
                    g.DrawString($"STACK STRESS: {patternManager.StackGauge:0.0}%", font, Brushes.Red, verticalBar.X - 55, verticalBar.Y - 25);
                }
            }
        }

        // ==========================================
        // 3번 보스 (BSOD 드래곤) 비주얼 이펙트 렌더링
        // ==========================================
        private void DrawBSODGimmicks(Graphics g, float cameraX, Size clientSize)
        {
            // 1. Lotus 패턴 그리기 (십자 회전 전깃줄 효과)
            if (patternManager.IsLotusActive)
            {
                // 보스가 맵 중앙(mapWidth / 2)에 고정되므로 스케일링된 맵 중앙 좌표 산출
                float bossSx = (patternManager.BossPos.X) - cameraX;
                float bossSy = patternManager.BossPos.Y;

                // 형광 네온 하늘색 전깃줄 스타일 선 렌더링
                using (Pen p = new Pen(Color.FromArgb(220, 0, 235, 255), 3.5f))
                {
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot; // 전깃줄 느낌 연출
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = patternManager.LotusAngle + (float)(i * Math.PI / 2);
                        float dx = (float)Math.Cos(angle) * 2000f; // 화면 끝까지 뻗어나가는 길이
                        float dy = (float)Math.Sin(angle) * 2000f;
                        g.DrawLine(p, bossSx, bossSy - 30, bossSx + dx, bossSy - 30 + dy);
                    }
                }

                // 보스 중심 코어에 경고 아우라 추가
                using (SolidBrush core = new SolidBrush(Color.FromArgb(40, Color.Cyan)))
                {
                    g.FillEllipse(core, bossSx - 80, bossSy - 110, 160, 160);
                }
            }

            // 2. Leak 패턴 그리기 (패치 장판 및 실시간 충전 로딩바)
            if (patternManager.IsLeakActive)
            {
                float px = patternManager.CurrentPatchPos.X - cameraX;
                float py = patternManager.CurrentPatchPos.Y;
                float r = patternManager.PatchRadius;

                // 바닥 세이프가드 패치 원 영역
                using (SolidBrush zoneBrush = new SolidBrush(Color.FromArgb(40, Color.Lime)))
                {
                    g.FillEllipse(zoneBrush, px - r, py - r, r * 2, r * 2);
                }
                using (Pen zonePen = new Pen(Color.Lime, 2f))
                {
                    g.DrawEllipse(zonePen, px - r, py - r, r * 2, r * 2);
                }

                // 💡 [로딩바 드로우] 장판 머리 위에 실시간 충전바 배치
                Rectangle barBounds = new Rectangle((int)px - 60, (int)py - (int)r - 25, 120, 12);
                using (SolidBrush barBg = new SolidBrush(Color.FromArgb(140, Color.Black)))
                {
                    g.FillRectangle(barBg, barBounds); // 게이지 배경 검은 박스
                }

                // StandTicks(0~60) 비율 계산
                float fillRatio = Math.Min(1f, (float)patternManager.StandTicks / 60f);
                Rectangle barFill = new Rectangle(barBounds.X, barBounds.Y, (int)(barBounds.Width * fillRatio), barBounds.Height);
                using (SolidBrush barFg = new SolidBrush(Color.Lime))
                {
                    g.FillRectangle(barFg, barFill); // 충전되는 초록색 게이지
                }
                using (Pen border = new Pen(Color.White, 1f))
                {
                    g.DrawRectangle(border, barBounds); // 테두리선
                }

                // 패치 스택 텍스트 스코어보드 (1/3, 2/3 등)
                using (Font font = Renderer.F(9.5f, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString($"REPAIR COUNTER: {patternManager.PatchCount} / 3", font, textBrush, px, py - r - 42, Renderer.Center());
                }
            }

            // 3. Magnus 패턴 그리기 (압박 축소 안전구역 테두리)
            if (patternManager.IsMagnusActive)
            {
                float mx = patternManager.BossPos.X - cameraX;
                float my = patternManager.BossPos.Y;
                float mw = patternManager.MagnusWidth;
                float mh = patternManager.MagnusHeight;

                RectangleF safeZoneBox = new RectangleF(mx - mw / 2, my - mh / 2, mw, mh);

                // 안전구역 투명 하늘색 장벽 드로우
                using (SolidBrush fill = new SolidBrush(Color.FromArgb(20, Color.DeepSkyBlue)))
                {
                    g.FillRectangle(fill, safeZoneBox);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(200, Color.DeepSkyBlue), 3f))
                {
                    borderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawRectangle(borderPen, safeZoneBox.X, safeZoneBox.Y, safeZoneBox.Width, safeZoneBox.Height);
                }

                // 위험 경고 문구 드로우
                using (Font font = Renderer.F(12f, FontStyle.Bold))
                using (SolidBrush warnBrush = new SolidBrush(Color.DeepSkyBlue))
                {
                    g.DrawString("⚠️ CRITICAL: 안전구역 외부 이탈 시 시스템 폭파 경고 ⚠️", font, warnBrush, mx, my - mh / 2 - 30, Renderer.Center());
                }
            }
        }
        private void DrawProjectiles(Graphics g, float cameraX)
        {
            foreach (BossProjectile p in patternManager.Projectiles)
            {
                int sx = (int)(p.X - cameraX);
                int sy = (int)p.Y;

                // ❌ [삭제 완료] 기존의 보라색 원형 오라와 흰색 코어 GDI+ 도형 코드를 제거했습니다.

                // 💡 [대체 완료] 일반 탄막들도 전부 보스 컨셉에 맞게 미니 회전 하드디스크 형태로 변환합니다.
                if (Renderer.Img_DiskSprite != null)
                {
                    // 탄막마다 약간씩 회전 타이밍이 다르게 보이도록 픽셀 위치값을 틱에 연동하는 센스 추가
                    int frameIndex = ((Environment.TickCount + sx) / 80) % 10;
                    int srcFrameW = Renderer.Img_DiskSprite.Width / 10;
                    int srcFrameH = Renderer.Img_DiskSprite.Height;
                    Rectangle srcRect = new Rectangle(frameIndex * srcFrameW, 0, srcFrameW, srcFrameH);

                    // 일반 탄막 크기 규격 (28x28)
                    int drawSize = 28;
                    Rectangle destRect = new Rectangle(sx - drawSize / 2, sy - drawSize / 2, drawSize, drawSize);

                    g.DrawImage(Renderer.Img_DiskSprite, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    // 💡 만약의 이미지 로드 실패를 대비한 보라색 방어선 백업용 드로우 코드 유지
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(90, 150, 80, 230))) g.FillEllipse(glow, sx - 14, sy - 14, 28, 28);
                    using (SolidBrush core = new SolidBrush(Color.White)) g.FillEllipse(core, sx - 5, sy - 5, 10, 10);
                }
            }
        }

        private void DrawShardPattern(Graphics g, float cameraX)
        {
            if (!patternManager.IsShardPatternActive) return;
            int sx = (int)(patternManager.CurrentShardPos.X - cameraX);
            int sy = (int)patternManager.CurrentShardPos.Y;

          

            // 10등분하여 회전하는 디스크 스프라이트 이미지를 정밀 투사합니다.
            if (Renderer.Img_DiskSprite != null)
            {
                int frameIndex = (Environment.TickCount / 100) % 10;
                int srcFrameW = Renderer.Img_DiskSprite.Width / 10;
                int srcFrameH = Renderer.Img_DiskSprite.Height;
                Rectangle srcRect = new Rectangle(frameIndex * srcFrameW, 0, srcFrameW, srcFrameH);

                // 드라이브 핵(Shard)인 만큼 투사체보다 조금 더 크게(64x64) 세팅하여 포스를 줍니다.
                int drawSize = 64;
                Rectangle destRect = new Rectangle(sx - drawSize / 2, sy - drawSize / 2, drawSize, drawSize);

                g.DrawImage(Renderer.Img_DiskSprite, destRect, srcRect, GraphicsUnit.Pixel);
            }

            // 💡 [유지] 패턴 진행 상황과 남은 시간을 알려주는 상단 UI 레이블은 그대로 살려둡니다.
            using (Font f = Renderer.F(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            {
                Rectangle label = new Rectangle(sx - 135, sy - 76, 270, 30);
                g.FillRectangle(bg, label);
                g.DrawString("DRIVE SHARD " + (patternManager.ShardSequence + 1) + "/3  " + (patternManager.ShardTimer / 60.0).ToString("0.0") + "s", f, b, label, Renderer.Center());
            }
        }
        private void DrawResourcePattern(Graphics g, Size clientSize)
        {
            if (!patternManager.IsResourcePatternActive) return;
            using (Font f = Renderer.F(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
            {
                Rectangle label = new Rectangle(clientSize.Width / 2 - 260, 82, 520, 34);
                g.FillRectangle(bg, label);
                g.DrawString("RESOURCE 부족 경고: DEBUG 아이콘을 클릭해 삭제하세요  " + (patternManager.ResourceTimer / 60.0).ToString("0.0") + "s", f, b, label, Renderer.Center());
            }
            for (int i = 0; i < patternManager.DebugButtons.Count; i++)
            {
                Rectangle r = patternManager.DebugButtons[i];
                using (LinearGradientBrush br = new LinearGradientBrush(r, Color.FromArgb(255, 245, 245), Color.FromArgb(255, 110, 110), 90f)) g.FillRectangle(br, r);
                using (Pen pen = new Pen(Color.DarkRed, 2f)) g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                using (Font f = Renderer.F(10f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.DarkRed)) g.DrawString("DEBUG", f, b, r, Renderer.Center());
            }
        }

        private void DrawNotice(Graphics g, Size clientSize)
        {
            if (patternManager.NoticeTicks <= 0 || string.IsNullOrEmpty(patternManager.NoticeText)) return;
            using (Font f = Renderer.F(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 240, 120)))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                Rectangle label = new Rectangle(clientSize.Width / 2 - 230, 122, 460, 30);
                g.FillRectangle(bg, label);
                g.DrawString(patternManager.NoticeText, f, b, label, Renderer.Center());
            }
        }
    }
}
