# Bushi (Skarnd02_Bushi) — Claude 작업 노트

하늘에서 떨어지는 장애물을 칼로 베며 살아남는 픽셀 아트 2D 모바일(Android) 액션 게임.
v1.0을 GitHub Release로 배포함 (2026-10-05). v1.1(스킬·코인·상점·새 캐릭터)도 GitHub Release로 배포함 (2026-10-05, 태그 v1.1, Version 1.1 / Bundle Version Code 2). 2026-10-06에 타이틀(로딩) 화면을 추가한 APK로 v1.1 릴리스의 APK만 교체함 — `v1.1` 태그(소스 zip)는 10-05 커밋 그대로 두기로 사용자가 결정 (다시 언급하지 말 것). 이 파일은 다음 세션이 이어서 작업하기 위한 인수인계 노트다.

## 진행 규칙 (사용자 요청 — 반드시 지킬 것)

- **한국어**로, 게임 개발·프로그래밍을 **전혀 모르는 사람**에게 설명하듯 **단계별로 아주 자세히** 설명한다.
  용어는 쉬운 비유로 풀고, Unity 메뉴 경로·입력할 값·드래그할 대상을 하나하나 적는다.
- C# 스크립트는 Claude가 직접 작성한다 (한국어 주석, 기존 코드 스타일 유지). 사용자는 안내를 따라 Inspector/씬 작업을 한다.
- **기능 개발이나 버그 수정 단계가 끝날 때마다, 답변 마지막에 "✅ Commit & Push 안내" 섹션을 반드시 넣는다.**
  작은 수정이라도 빠뜨리지 않는다. 커밋·푸시는 Claude가 직접 하지 않고 사용자가 GitHub Desktop으로 한다.
  - 커밋 메시지는 **커밋 타입 + 한국어 설명**으로, 바로 복사할 수 있게 코드 블록에 넣어 추천한다.

    | 타입 | 쓰는 때 |
    |---|---|
    | `feat:` | 새 기능 추가 |
    | `fix:` | 버그 수정 |
    | `style:` | 기능은 그대로, 보이는 모습(UI 배치·색 등)만 변경 |
    | `chore:` | 설정·정리 작업 (빌드 설정, .gitignore, 테스트 로그 제거 등) |
    | `docs:` | 문서만 변경 (README, CLAUDE.md 등) |

  - 안내 형식 (항상 이 순서로):
    1. **Summary**에 아래 메시지를 적으세요. → 코드 블록으로 메시지 제시 (예: `feat: 방패 아이템 추가 및 획득 시 1회 피격 무효화 구현`)
    2. **Commit to main**을 누르세요.
    3. **Push origin**을 누르세요.
  - Summary 칸에는 메시지만 넣는다 (`git commit -m "..."` 통째로 넣지 않도록 안내).
  - 성격이 다른 작업이 섞여 있으면 **파일 체크박스로 나눠서** 여러 번 커밋하도록 안내하고, 각각의 메시지를 따로 추천한다.
  - 이전 단계를 아직 커밋하지 않았다면 함께 커밋할 메시지(묶음용)와 따로 커밋할 메시지를 모두 제시한다.
  - GitHub 웹사이트에서 직접 파일을 고쳤다면 커밋 전에 **Pull origin**부터 하도록 안내한다.
- 숫자(PPU, 피벗, 크기, 폰트 크기 등)는 **추측하지 말고 실제 파일(.meta, 씬 YAML, 이미지)을 확인**한 뒤 안내한다.
- 사용자가 스프라이트·사운드를 직접 준비한다. 새 에셋을 받으면 먼저 열어 보고(이미지 확인, 슬라이스 정보 확인) 활용 방법을 제안한다.

## 환경

- Unity 6.3 LTS (6000.3.25f1), 2D URP, Input System(새 입력 시스템만 사용), TextMeshPro (com.unity.ugui 2.0)
- 플랫폼: Android (Package `com.skarnd0814.bushi`, Product Name `Bushi`, Company `Skarnd0814`, 가로 화면 고정, IL2CPP/ARM64)
- 빌드 결과물은 `Builds/` 폴더 (gitignore 됨). GitHub: https://github.com/Skarnd0814/Skarnd02_Bushi
- 기준 해상도 1920×1080 (Canvas Scaler: Scale With Screen Size, Match 0.5)

## 씬 구성

- `TitleScene` (Build Index 0): 앱 첫 화면(로딩). LoadingCamera(Depth -10, 검은 배경) + LoadingCanvas(Overlay, Sort Order 100: PlayBG 배경, 큰 타이틀 ×3, 제작자 이름 "by Skarnd0814" — CreatorBackground(검은 반투명 상자, HorizontalLayoutGroup+ContentSizeFitter로 글자에 맞춤, CanvasGroup) 안의 CreatorText) + `TitleLoader`
- `MainMenuScene` (Build Index 1): GameTitle(타이틀 ×2, `MainMenuTitle`), 배경, START / SETTINGS 버튼, SettingsPopup(BGM/SFX/EFFECT 슬라이더), BEST 점수 표시, AudioManager
- `InGameScene` (Build Index 2): Player, Ground(Tiled 나무 바닥, 폭 30), Background(BackgroundFitter), ObstacleSpawner,
  GameManager, ScoreManager, ComboManager, FeverManager, ScorePopupSpawner, AudioManager(프리팹 복제본),
  Canvas(SafeArea 안에 ScorePanel/ComboText/FeverText/MobileControls/PauseButton, 그 밖에 PauseScreen, GameOverScreen)

## 스크립트 구조 (Assets/Scripts)

| 스크립트 | 역할 |
|---|---|
| `Audio/AudioManager` | 싱글톤 + DontDestroyOnLoad. BGM 플레이리스트 교대 재생, 스피커 3개(BGM/SFX/Combat), 볼륨 PlayerPrefs 저장, 씬 로드 시 BGM 꺼져 있으면 재시작. `Prefabs/AudioManager`를 두 씬 모두에 배치(복제본은 스스로 삭제) |
| `Player/PlayerController` | 이동/점프(키보드 ←→ Space + 모바일 `SetMobileMove`/`RequestJump`), Collider Cast 바닥 감지, 화면 밖 이동 제한, `MovementLocked`(공격 중), `Die()` |
| `Player/PlayerAttack` | Q / `RequestAttack`. 판정 창(hitStart~hitEnd) 동안 OverlapBox, 성공 0.5초 / 실패 3초 쿨타임, `CooldownDisabled`(피버), 이벤트 `AttackJudged(int 파괴수)` |
| `Player/PlayerAnimator` | Animator 파라미터 `Speed`, `IsGrounded`, `IsAttacking`, `IsDead` 전달 (방향 전환 시 IDLE 끼어듦 방지 grace time). 스킬은 전환 화살표 없이 `animator.Play(상태 이름)`으로 직접 재생, 스킬 중엔 IsGrounded=true/IsAttacking=false로 고정해 Any State가 끊지 못하게 함 |
| `Skills/PlayerSkill` (abstract) | 스킬 공통: 번호·이름·아이콘·상태 이름·길이·쿨타임·효과음 칸, 구매 여부(`IsAvailable`), 코루틴 `Perform()`, 사용 중 `MovementLocked` |
| `Skills/PlayerSkills` | Player의 스킬 관리. 키보드 1/2/3, `RequestSkill(번호)`, 한 번에 하나·공격 중 불가, 죽으면 Cancel. `unlockAllOnPC`(휴대폰에선 무시) |
| `Skills/CycloneSlashSkill` (1) | 회전 판정 창 동안 머리 중심 정사각형(키 1.44 × 2~3배) OverlapBox, 발밑 흙먼지 SpriteFlipbook |
| `Skills/TripleComboSkill` (2) | fireTimes 3회 검기 발사(바라보는 방향 + shotAngles) |
| `Skills/RisingCrescentSkill` (3) | `PlayerController.AirJumpPressed`(공중 점프 입력) 구독 → `Leap()` 후 초승달 검기, 착지 전 1회, 검기 debrisMultiplier 3 |
| `Skills/SkillProjectile` + `SkillProjectileSettings` | 코드 생성 검기. 관통, 화면 밖에서 소멸, OverlapBox로 `Obstacle.Break(debrisMultiplier)` |
| `Skills/SkillUnlocks` | 스킬 구매 기록(PlayerPrefs `SkillUnlocked_번호`). 상점에서 `Unlock(번호)` 호출 예정 |
| `Effects/SpriteFlipbook` | 코드 생성 프레임 애니메이션(1회 후 삭제 / 반복), Follow, SkipTime |
| `Effects/CoinPopupSpawner` + `FlyingCoin` | `CoinManager.CoinEarned` 수신 → "Coin +N" 월드 TMP(점수 글자 아래) + 회전 코인(Coin_Split_0~7) 튀어나옴 + 코인 효과음(minSoundInterval로 겹침 제한). 표시만 담당 |
| `Coins/CoinManager` | InGame. `ObstacleScored` 수신 → 피버 중이면 `coinsPerObstacle`만큼 `CoinWallet.Add`, `EarnedThisRun`, 이벤트 `CoinEarned(위치, 개수)`. 게임 오버·OnDestroy·OnApplicationPause에서 저장 |
| `Coins/CoinWallet` | static 지갑. PlayerPrefs `Coins`, `Add`(메모리만) / `Save` / `TrySpend`(상점용), 이벤트 `Changed`. 메인 메뉴 `MainMenuController.coinText`에 표시 |
| `UI/SkillShopPopup` + `SkillShopItem` | 메인 메뉴 상점 팝업. 상품 칸(스킬 번호·가격)마다 `CoinWallet.TrySpend` → `SkillUnlocks.Unlock`, OWNED/가격 표시, 안내 글자(PURCHASED! / NOT ENOUGH COINS), 구매음은 `AudioManager.PlaySfx`. MainMenuController의 shopButton/shopPopup |
| `Editor/SkillShopBuilder` | 메뉴 **Bushi > 스킬 상점 UI 만들기**: MainMenuScene에 ShopButton(오른쪽 위) + SkillShopPopup(ShopUI 598×394의 2배 패널, 상품 3칸)을 자동 생성·연결 |
| `Editor/BushiTestTools` | 메뉴 **Bushi > 테스트**: 코인 +500, 코인 0, 스킬 구매 기록 초기화 |
| `UI/SkillButtonUI` | 스킬 버튼(버튼 Image = 아이콘)의 쿨타임 덮개(버튼 그림 복사, Radial360 자동 설정)·남은 초·누름 어둡게. 스킬 버튼의 Pressed Sprite는 비워 둠. 버튼 입력은 MobileControlButton(Skill1~3) + MobileControls, 미구매 스킬 버튼은 MobileControls가 숨김 |
| `Player/CooldownBar` | 머리 위 하얀 쿨타임 게이지 |
| `Obstacle/Obstacle` | Kinematic 낙하, 랜덤 스프라이트/기울기, 콜라이더 자동 맞춤. `Break()`(점수 O) / `Vanish()`(점수 X) / 바닥 도달 시 소멸. 정적 이벤트 `HitPlayer`, `Broken` |
| `Obstacle/ObstacleSpawner` | 시간 경과로 생성 간격 감소(Lerp), `StartBurst()`(피버 폭우), `ClearAndPause()`(피버 종료 정리), `StopSpawning()` |
| `GameManager` | `Obstacle.HitPlayer` 수신 → 피버 중이면 장애물 Break, 아니면 게임 오버(생성 정지, Die, BGM 정지, 랜덤 게임오버음). 이벤트 `GameOverHappened` |
| `ScoreManager` | 생존 점수 + `AddDestroyScore(combo)`, 최고 점수 저장(`BestScoreKey` = "BestScore"), `DestroyScoreMultiplier`(피버) |
| `ComboManager` | `Obstacle.Broken` 수신 → 콤보+1 → ScoreManager에 점수 요청. 공격 실패 시 초기화(`ProtectComboOnFail` 예외). 이벤트 `ComboChanged`, `ObstacleScored(위치, 점수)` |
| `FeverManager` | `ComboChanged` 수신 → 피버 시작/종료, 쿨타임·배율·콤보보호 토글, 폭우, 종료 시 장애물 정리, 랜덤 피버 시작음 |
| `Effects/*` | `DebrisPiece`(코드 생성 조각), `CameraShake`(파괴/피버/게임오버 이벤트 구독), `ScorePopupSpawner`+`ScorePopup`(월드 TMP "+130") |
| `UI/MobileControls` + `MobileControlButton` | 매 프레임 터치/마우스 위치를 직접 검사(손가락 미끄러뜨려 버튼 전환 가능), touchPadding, 공격 버튼 쿨타임 색 |
| `UI/PauseMenu` | Time.timeScale=0 일시정지, Esc, OnApplicationPause 자동 정지, 씬 전환 전 timeScale 복구, 선택 해제 |
| `UI/GameOverUI` | 1.2초 뒤 결과 화면(SCORE/BEST, NEW BEST — MAX COMBO 줄은 v1.1에서 제거), RESTART/MAIN, 게임 오버 시 숨길 UI 목록. coinManager 연결 시 금색 `COIN +이번 판` 줄 추가 (rich text) |
| `UI/CoinWalletDisplay` | TMP 글자에 붙이면 보유 코인 표시, `CoinWallet.Changed`로 즉시 갱신 (인게임 점수판 옆 CoinWallet) |
| `UI/MainMenuController`, `SettingsPopup`, `ButtonClickSound`, `SafeAreaFitter` | 메인 메뉴 버튼/BEST, 볼륨 팝업, 버튼 클릭음, 안전 영역 |
| `UI/TitleLoader` | TitleScene. 메인 메뉴를 Additive로 미리 로드 → 불투명한 로딩 화면 위 검은 막(코드 생성 BlackCover)이 걷힘(0.5; 로딩 화면을 투명하게 시작하면 휴대폰에서 먼저 열린 메인 메뉴가 비쳐 보였음) → 최소 2초 → 제작자 이름 페이드 아웃(0.5) → 타이틀이 `MainMenuTitle` 자리·크기로 이동·축소하며 배경 페이드 아웃(0.9) → TitleScene 언로드. 메인 메뉴가 열리면 로딩 AudioListener 끔 |
| `UI/MainMenuTitle` | 메인 메뉴 타이틀 표시용 이름표(RectTransform 제공, 이동 중 숨김) |
| `Editor/TitleSceneBuilder` | 메뉴 **Bushi > 타이틀(로딩) 화면 만들기**: MainMenu에 GameTitle 추가, TitleScene 생성, 빌드 씬 순서 Title→MainMenu→InGame. 메뉴 **Bushi > 제작자 이름에 검은 배경 넣기**로 기존 TitleScene에 상자 추가 |
| `BackgroundFitter` | [ExecuteAlways] 화면 비율에 맞춰 배경을 덮도록 스케일 |

기능끼리는 **이벤트(알림)** 로 연결되어 있다. 새 기능은 기존 코드 수정 대신 이벤트 구독으로 덧붙이는 방식을 우선한다.

## 게임 규칙 (설계값 — 실제 값은 Inspector가 기준)

- 이동 속도 6, 점프 힘 12, 중력 3
- 공격: 길이 0.58초, 판정 0.25~0.5초, 범위 Offset(0.6, 1.3) Size(1.4, 1.8), 성공 쿨타임 0.5초 / 실패 3초, 공격 중 지상 정지·공중 공격 가능
- 장애물 4종: 세로 통나무(01_0~2), 가로 통나무(01_3~5), 바위(02_0~3), 큰 바위(02_4~7, Scale 1.4). 생성 간격 0.8~1.6초 → 120초에 걸쳐 0.5초로
- 점수: 생존 1초 10점, 파괴 100 + 콤보당 10 (보너스 최대 200), 피버 ×2
- 콤보: 장애물 1개당 +1, 공격 실패 시 0 (바닥 낙하는 무관)
- 피버: **5콤보** 발동 (처음 계획은 10이었으나 너무 어려워서 사용자가 5로 조정), 5초, 무적(닿은 장애물 Break), 쿨타임 없음, 점수 2배, 실패해도 콤보 유지, 시작 시 폭우(Burst 2초 / 0.05초 간격), 종료 시 장애물 전체 Vanish + 1초 생성 중지, 콤보 0

## 에셋·설정 메모 (그동안 겪은 문제와 해결)

- 스프라이트: Filter Point, Compression None
- v1.1 캐릭터 시트(`Bushi-idle/run/jump` 256칸, `Bushi-asm_attack/skill1~3` 512칸, 5×5): **PPU 64**, 칸 단위로 자르고 피벗을 .meta에 직접 지정(발밑·몸 중심, jump/skill3 공중 프레임은 장마다 발 높이). 2560px 시트는 Max Size 4096. attack은 22장. **Sprite Editor에서 다시 Slice 금지**
- 애니메이션: Idle 0~24 @20, Run 0~24 @30, Jump 3~13 @15, Attack 0~21 @38(=0.58초), Skill1 2~20 @30, Skill2 2~21 @25, Skill3 4~14 @20
- 스킬 이펙트(`Sprites/SkillEffect`, PPU 64): skill1 흙먼지 7장(고리 중심 피벗), skill2 100칸 20장(검기로는 _1~_3 사용), skill3 초승달 6장(그림이 왼쪽을 향함 → `flipSprite` 체크). 아이콘은 `Bushi_Skill_Icon` 한 장에 4개(_0~_2 스킬1~3, _3 빈 나무판), 조작 버튼과 같은 나무판 디자인이라 스킬 버튼의 Image 자체로 사용
- Player_Die는 옛 캐릭터 그림(PPU 92)을 그대로 사용 — **사용자 결정으로 앞으로도 교체하지 않음** (교체 제안·알려진 문제로 언급하지 말 것)
- 장애물 PPU: Obstacle01 180, Obstacle02 160
- `Bushi_Title`: 670×372 중 글자 부분만 417×186으로 자름(.meta 직접 지정). UI 크기는 이 배수로(메인 메뉴 834×372, 로딩 1251×558) — 두 크기의 비율이 같아야 이동 애니메이션이 정확히 겹침
- Galmuri 도트 폰트(TMP): Render Mode **RASTER**(HINTED 금지), Sampling Point Size는 도트 격자의 정확한 배수 (Galmuri9=10의 배수, Galmuri11=12의 배수, 현재 80/72), Font Size도 같은 배수, Bold 금지
- Animator Any State 전이는 Can Transition To Self 끄기, Has Exit Time 끄기, Duration 0
- Hierarchy 아래쪽이 앞에 그려진다 (패널은 텍스트보다 위에 둘 것)
- Play 중 Inspector 변경은 사라진다 / 테스트용으로 바꾼 값은 빌드·커밋 전에 되돌릴 것. 단, 값이 계획과 달라 보여도 사용자가 일부러 조정했을 수 있으니 바꾸기 전에 먼저 물어볼 것
- Android 빌드 시 생기는 `.utmp/`는 gitignore 대상
- Unity 종료 직후 `ProjectSettings.asset`의 `preloadedAssets`에서 InputSystem_Actions 한 줄만 빠지는 변경은 Input System이 자동으로 넣고 빼는 것 → 커밋하지 말고 Discard (코드는 Keyboard.current/Touchscreen.current를 직접 사용)

## 남은 일 / 아이디어

- 릴리스 서명: v1.0·v1.1은 **디버그 키(이 PC의 debug.keystore)** 로 서명됨 → 같은 PC에서 빌드해야 휴대폰에서 덮어쓰기 업데이트 가능. 정식 Keystore로 바꾸면 기존 설치본은 삭제 후 재설치 필요 (Play 스토어 출시 때 전환)
- 다음 릴리스마다 Player Settings의 Version과 Bundle Version Code(+1)를 올릴 것
- 아이디어: 새 장애물·아이템(방패, 자석), 일시정지 창에 볼륨 설정, 한글 UI(TMP 폰트 아틀라스에 한글 추가 필요), Play 스토어 출시
