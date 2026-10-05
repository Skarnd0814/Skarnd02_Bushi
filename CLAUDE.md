# Bushi (Skarnd02_Bushi) — Claude 작업 노트

하늘에서 떨어지는 장애물을 칼로 베며 살아남는 픽셀 아트 2D 모바일(Android) 액션 게임.
v1.0을 GitHub Release로 배포함 (2026-10-05). 이 파일은 다음 세션이 이어서 작업하기 위한 인수인계 노트다.

## 진행 규칙 (사용자 요청 — 반드시 지킬 것)

- **한국어**로, 게임 개발·프로그래밍을 **전혀 모르는 사람**에게 설명하듯 **단계별로 아주 자세히** 설명한다.
  용어는 쉬운 비유로 풀고, Unity 메뉴 경로·입력할 값·드래그할 대상을 하나하나 적는다.
- C# 스크립트는 Claude가 직접 작성한다 (한국어 주석, 기존 코드 스타일 유지). 사용자는 안내를 따라 Inspector/씬 작업을 한다.
- **기능 개발이나 버그 수정 단계가 끝날 때마다 Commit & Push를 안내**한다.
  - 사용자는 **GitHub Desktop**을 쓴다 (Summary 입력 → Commit to main → Push origin).
  - 커밋 메시지는 **커밋 타입을 포함한 한국어**로 추천한다: `feat:`, `fix:`, `chore:`, `style:`, `docs:`
  - Summary 칸에는 메시지만 넣는다 (`git commit -m "..."` 통째로 넣지 않도록 안내).
- 숫자(PPU, 피벗, 크기, 폰트 크기 등)는 **추측하지 말고 실제 파일(.meta, 씬 YAML, 이미지)을 확인**한 뒤 안내한다.
- 사용자가 스프라이트·사운드를 직접 준비한다. 새 에셋을 받으면 먼저 열어 보고(이미지 확인, 슬라이스 정보 확인) 활용 방법을 제안한다.

## 환경

- Unity 6.3 LTS (6000.3.25f1), 2D URP, Input System(새 입력 시스템만 사용), TextMeshPro (com.unity.ugui 2.0)
- 플랫폼: Android (Package `com.skarnd.bushi`, Product Name `Bushi`, Company `Skarnd0814`, 가로 화면 고정, IL2CPP/ARM64)
- 빌드 결과물은 `Builds/` 폴더 (gitignore 됨). GitHub: https://github.com/Skarnd0814/Skarnd02_Bushi
- 기준 해상도 1920×1080 (Canvas Scaler: Scale With Screen Size, Match 0.5)

## 씬 구성

- `MainMenuScene` (Build Index 0): 배경, START / SETTINGS 버튼, SettingsPopup(BGM/SFX/EFFECT 슬라이더), BEST 점수 표시, AudioManager
- `InGameScene` (Build Index 1): Player, Ground(Tiled 나무 바닥, 폭 30), Background(BackgroundFitter), ObstacleSpawner,
  GameManager, ScoreManager, ComboManager, FeverManager, ScorePopupSpawner, AudioManager(프리팹 복제본),
  Canvas(SafeArea 안에 ScorePanel/ComboText/FeverText/MobileControls/PauseButton, 그 밖에 PauseScreen, GameOverScreen)

## 스크립트 구조 (Assets/Scripts)

| 스크립트 | 역할 |
|---|---|
| `Audio/AudioManager` | 싱글톤 + DontDestroyOnLoad. BGM 플레이리스트 교대 재생, 스피커 3개(BGM/SFX/Combat), 볼륨 PlayerPrefs 저장, 씬 로드 시 BGM 꺼져 있으면 재시작. `Prefabs/AudioManager`를 두 씬 모두에 배치(복제본은 스스로 삭제) |
| `Player/PlayerController` | 이동/점프(키보드 ←→ Space + 모바일 `SetMobileMove`/`RequestJump`), Collider Cast 바닥 감지, 화면 밖 이동 제한, `MovementLocked`(공격 중), `Die()` |
| `Player/PlayerAttack` | Q / `RequestAttack`. 판정 창(hitStart~hitEnd) 동안 OverlapBox, 성공 0.5초 / 실패 3초 쿨타임, `CooldownDisabled`(피버), 이벤트 `AttackJudged(int 파괴수)` |
| `Player/PlayerAnimator` | Animator 파라미터 `Speed`, `IsGrounded`, `IsAttacking`, `IsDead` 전달 (방향 전환 시 IDLE 끼어듦 방지 grace time) |
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
| `UI/GameOverUI` | 1.2초 뒤 결과 화면(SCORE/BEST/MAX COMBO, NEW BEST), RESTART/MAIN, 게임 오버 시 숨길 UI 목록 |
| `UI/MainMenuController`, `SettingsPopup`, `ButtonClickSound`, `SafeAreaFitter` | 메인 메뉴 버튼/BEST, 볼륨 팝업, 버튼 클릭음, 안전 영역 |
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

- 스프라이트: Filter Point, Compression None. 캐릭터 시트마다 그림 크기가 달라 PPU로 보정: Player 100, Player_Run 89(Player.png에서 분리), Player_Jump 75, Player_Attack 79, Player_Die 92. 모두 Bottom 피벗
- Player_Attack은 프레임별 **Custom 피벗**을 .meta에 직접 맞춰 둠 → Sprite Editor에서 다시 Slice 하면 사라짐. `Player_Attack_6`은 잔여 조각이라 애니메이션에서 제외
- 장애물 PPU: Obstacle01 180, Obstacle02 160
- Galmuri 도트 폰트(TMP): Render Mode **RASTER**(HINTED 금지), Sampling Point Size는 도트 격자의 정확한 배수 (Galmuri9=10의 배수, Galmuri11=12의 배수, 현재 80/72), Font Size도 같은 배수, Bold 금지
- Animator Any State 전이는 Can Transition To Self 끄기, Has Exit Time 끄기, Duration 0
- Hierarchy 아래쪽이 앞에 그려진다 (패널은 텍스트보다 위에 둘 것)
- Play 중 Inspector 변경은 사라진다 / 테스트용으로 바꾼 값은 빌드·커밋 전에 되돌릴 것. 단, 값이 계획과 달라 보여도 사용자가 일부러 조정했을 수 있으니 바꾸기 전에 먼저 물어볼 것
- Android 빌드 시 생기는 `.utmp/`는 gitignore 대상

## 남은 일 / 아이디어

- 배포된 v1.0 APK는 피버 발동 콤보 3으로 빌드되었을 수 있음 → 다음 릴리스(v1.1)에서 5로 반영 (릴리스 설명의 "10콤보" 문구도 5로 수정 필요)
- 아이디어: 새 장애물·아이템(방패, 자석), 일시정지 창에 볼륨 설정, Play 스토어 출시(정식 Keystore, Bundle Version Code 증가)
