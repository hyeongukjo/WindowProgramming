DebugHeroFileDungeonRPG - 보스방 배경 최종 적용본

요청대로 새 이미지를 다시 생성하지 않고, 이전에 만들어둔 보스방 배경 이미지를 게임 Assets 폴더에 StageBg_01_Boss.png ~ StageBg_10_Boss.png 이름으로 적용했습니다.

적용 방식:
- 일반 몬스터를 모두 처치하면 stageBossPhase=true가 되어 보스방으로 자동 전환됩니다.
- Renderer.DrawStageBackground(..., bossRoom:true)가 호출되면 Assets/StageBg_XX_Boss.png를 우선 표시합니다.
- Stage 1 보스방도 StageBg_01_Boss.png를 사용하도록 Renderer의 특수 매핑을 제거했습니다.

적용된 보스방 배경:
01: StageBg_01_Boss.png
02: StageBg_02_Boss.png
03: StageBg_03_Boss.png
04: StageBg_04_Boss.png
05: StageBg_05_Boss.png
06: StageBg_06_Boss.png
07: StageBg_07_Boss.png
08: StageBg_08_Boss.png
09: StageBg_09_Boss.png
10: StageBg_10_Boss.png

기존 기능 유지:
- 모든 스테이지 몬스터 처치 후 보스방 자동 진입
- Player.exe 캐릭터 / 걷기 애니메이션
- 코인 상점 / D,F 포션
- NPC 중앙 에러창
- StagePlans 문서 기반 진행
