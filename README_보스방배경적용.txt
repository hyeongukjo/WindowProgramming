DebugHeroFileDungeonRPG - 보스방 배경 적용 수정본

요청 내용 반영:
- 가장 최근에 작업한 AllStagesAutoBoss 코드 구조를 기준으로 작업했습니다.
- 일반 몬스터를 모두 처치하면 보스방으로 자동 전환되는 기존 흐름을 유지했습니다.
- 보스방 전용 배경을 Stage 02~10에 각각 다르게 적용했습니다.
- Stage 1의 Driver-K 보스전은 기존 코드 구조상 Stage 02 보스방 배경을 사용하도록 유지했습니다.

적용된 보스방 배경 매핑:
- Stage 01 Boss: Stage 02 Driver Vault 보스방 배경 공유
- Stage 02 Boss: futuristic_sci_fi_lab_core_arena.png
- Stage 03 Boss: futuristic_data_temple_with_glowing_core.png
- Stage 04 Boss: futuristic_digital_temple_arena.png
- Stage 05 Boss: cyberpunk_arena_with_glowing_gateway.png
- Stage 06 Boss: ruins_of_a_digital_temple.png
- Stage 07 Boss: futuristic_data_temple_with_glowing_portal.png
- Stage 08 Boss: glitchy_cyberpunk_server_core_arena.png
- Stage 09 Boss: industrial_furnace_chamber_with_glowing_core.png
- Stage 10 Boss: neon_lit_industrial_recycling_factory_boss_arena.png

실행 방법:
1. DebugHeroFileDungeonRPG.sln 열기
2. 빌드 > 솔루션 정리
3. 빌드 > 솔루션 빌드
4. 실행
