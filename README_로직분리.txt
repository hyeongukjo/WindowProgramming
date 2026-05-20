# 로직 분리 수정본

MainForm.cs에서 보스 패턴, 보스 UI, 스테이지 몬스터 생성, 플레이어 이동, 몬스터 AI, 보상 지급을 별도 파일로 분리했습니다.

## 분리된 파일
- BossRuntime.cs: 보스 패턴 호출, 클릭 처리, 보스 패턴 UI 오버레이
- BossPatternManager.cs: 보스 패턴 계산/상태 관리
- StageEnemyFactory.cs: 일반 몬스터와 보스 생성
- StageFlowRules.cs: 일반 스테이지/보스방 맵 크기 규칙
- PlayerMovementSystem.cs: 이동, 방향, WalkCycle, 카메라 계산
- EnemyLogicSystem.cs: 몬스터 이동, 보스 업데이트 호출, 충돌 피해, 전멸 판정
- RewardSystem.cs: 코인과 보스 보상 지급
- MainForm.StageFlow.cs / MainForm.Input.cs / MainForm.Rendering.cs: 기능별 partial 분리

MainForm.cs는 폼 생성, 타이머, 전체 흐름 호출 중심으로 줄였습니다.
