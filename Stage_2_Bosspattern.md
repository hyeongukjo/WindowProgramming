# 2-스테이지 보스 패턴 구현

## 수정사항
1. 매 스테이지 시작 시 HP / MP 풀 회복으로 수정
2. 보스 스테이지 일반 몬스터 제거
3. 보스 스테이지 맵 축소
4. 테스트용으로 HP / MP 1000으로 조정, 데미지도 기존 2배 강하게 조정 -> 테스트 끝나면 다시 정상화 예정

## 2스테이지 보스 패턴 세분화
1. 특수기믹은 보스 체력기준 75% 50% 25% 순으로 등장
2. 75% 25% 기믹 : 30초안에 맵에 랜덤하게 소환되는 드라이브조각 수집 -> 밸런스 조정 시 제한시간 조정 가능
3. 50% 기믹 : 화면에 뜨는 debug 아이콘 4.5초안에 마우스로 클릭하여 지우기 -> 밸런스 조정 시 제한시간 및 아이콘 개수 조정 가능
4. 일반 공격 패턴은 구체발사 : 멀리있을 때는 피하기 가능, 근접시 피격 불가능 -> 밸런스 조정가능


## UI관련 생각


### 1. 레퍼런스 이미지 검색을 위한 구글 쿼리 가이드

현재 프로젝트는 **마우스 기반 2.5D(아이소메트릭) 시점**이므로, 이에 맞는 소스를 찾는 것이 중요합니다. 다음 키워드들을 조합해 보세요.

| 목적 | 추천 검색어 (영문 검색이 훨씬 결과가 많습니다) |
| --- | --- |
| **캐릭터/몬스터** | `Isometric 2D sprite sheet`, `Pixel art character walk cycle`, `Top down RPG monster assets` |
| **보스 및 대형 개체** | `Large pixel art boss sprite`, `2D game boss animations sheet` |
| **UI/UX 인터페이스** | `Game UI kit pixel art`, `RPG HUD design reference`, `Fantasy game window assets` |
| **이펙트 (마법/폭발)** | `2D FX sprite sheet`, `Pixel art explosion animation`, `Magic spell vfx sheet` |

> **팁:** 검색 시 이미지 탭의 **'도구' -> '색상' -> '투명'**을 선택하면 배경이 없는 PNG 파일을 더 쉽게 찾을 수 있습니다. 또한 [Itch.io](https://itch.io/game-assets/free)나 [OpenGameArt](https://opengameart.org/) 같은 사이트에서 무료 에셋을 찾는 것도 좋은 방법입니다.

### **opengameart <- 여기 사이트에서 UI가져오는 거 좋다고 생각함!!**
---

### 2. C# WinForms에 이미지 적용하기

현재 `Renderer.cs`는 `Graphics` 객체를 이용해 직접 도형을 그리고 있습니다. 이를 이미지 출력 방식으로 바꾸는 단계는 다음과 같습니다.

### ### Step 1: 이미지 리소스 준비

이미지 파일을 프로젝트 폴더에 넣거나, `MainForm.resx` 등 리소스 파일에 등록합니다.

### ### Step 2: 코드에서 이미지 로드 (`Renderer.cs` 또는 `MainForm.cs`)

매 프레임 이미지를 파일에서 읽으면 게임이 느려지므로, 게임 시작 시 한 번만 로드하여 변수에 저장해둡니다.

```csharp
// MainForm.cs 클래스 상단
private Image heroSprite = Image.FromFile("Resources/hero_walk.png");

// 만약 스프라이트 시트(여러 프레임이 합쳐진 이미지)라면
private Bitmap spriteSheet = new Bitmap("Resources/boss_sheet.png");

```

### ### Step 3: 이미지 그리기 및 애니메이션 구현

`Renderer.DrawHero` 메서드 안의 `g.FillRectangle` 같은 코드 대신 `g.DrawImage`를 사용합니다.

```csharp
public static void DrawHero(Graphics g, Player p, float cameraX, float cameraY, int tick, bool moving, Image sprite)
{
    [cite_start]PointF s = WorldToScreen(p.X, p.Y, cameraX, cameraY); [cite: 23]
    
    // 1. 단순 이미지 출력
    // g.DrawImage(sprite, s.X - 32, s.Y - 64, 64, 64);

    // 2. 역동적인 애니메이션 (스프라이트 시트 활용)
    int frameCount = 4; // 4프레임짜리 걷기 모션이라고 가정
    int currentFrame = (tick / 10) % frameCount; [cite_start]// 10틱마다 프레임 변경 [cite: 15]
    
    int frameWidth = sprite.Width / frameCount;
    Rectangle srcRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, sprite.Height);
    Rectangle destRect = new Rectangle((int)s.X - 32, (int)s.Y - 64, 64, 64);
    
    g.DrawImage(sprite, destRect, srcRect, GraphicsUnit.Pixel);
}

```

---

### 3. 역동적인 움직임을 위한 추가 팁

도형 방식을 유지하더라도 다음 수식을 적용하면 훨씬 역동적으로 보입니다.

**진동 효과 (Idle):** `Math.Sin(tick / 5.0) * 2` 값을 Y 좌표에 더해 캐릭터가 숨 쉬는 듯한 효과를 줍니다.

**가속도 표현:** 플레이어의 현재 속도(`VX`, `VY`)에 따라 캐릭터 이미지를 살짝 기울이거나(`RotateTransform`), 대시 시 잔상을 남기는 `Effect`를 추가하세요.

**피격 연출:** 보스가 맞았을 때 `m.HitFlash`가 0보다 크면 이미지의 밝기를 일시적으로 올리는 `ColorMatrix`를 적용할 수 있습니다.








