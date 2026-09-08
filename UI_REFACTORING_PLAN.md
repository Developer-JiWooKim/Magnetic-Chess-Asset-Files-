# UI 구조 개편 계획

이 문서는 UI가 왜 지금처럼 엉켰는지, 어떤 구조로 바꿀 것인지, 어떤 순서로 옮길 것인지를
적는다. 진행하면서 결과는 `REFACTORING_LOG.md`에 남긴다.

- **엔진:** Unity 6000.5.3f1 (URP 17.5)
- **기준 커밋:** `800c4ea` (버튼 배선 코드 이관 배치 2까지)
- **범위:** TitleScene · GameScene의 UI 전체와 그것을 조작하는 코드

---

## 1. 왜 하는가

버튼 배선을 인스펙터에서 코드로 옮기는 작업을 배치 단위로 진행하던 중,
고칠 때마다 예상 못 한 것이 튀어나왔다. 원인을 추적해 보니 개별 버그가 아니라
**구조에서 나오는 증상**이었다.

- 같은 패널 프리팹이 두 곳에 인스턴스로 배치돼 있었다 (`GameSettingPanel (1)`의 "(1)")
- 같은 버튼이 씬에 따라 다른 메소드를 불러야 했다
  (`OnClickGamePlayButton` / `OnClickGamePlayButtonGameScene`)
- `MenuManager`가 두 씬에 하나씩, 총 두 개 존재한다
- 씬 경계를 넘는 참조를 `FindObjectOfType`으로 찾고 있다 (3곳)

이 상태에서 배선만 바꾸는 것은 엉킨 실 위에서 매듭을 옮기는 일이다.
구조를 먼저 편다.

---

## 2. 현재 구조 — 확인된 사실

### 2.1 UI가 두 벌 존재한다

| | 오버레이 캔버스 | 태블릿 캔버스 |
|---|---|---|
| 오브젝트 | `Canvas_Menu` / `Canvas_DontDestroyMenu` | `Tablet_UI` (프리팹) |
| 렌더 모드 | Screen Space Overlay (`m_RenderMode: 0`) | **World Space** (`m_RenderMode: 2`) |
| `GraphicRaycaster` | 있음 | **없음** |
| `worldCamera` | — | **미지정** (`m_Camera: {fileID: 0}`) |
| 역할 | 유저가 실제로 클릭하는 UI | 같은 프리팹의 **두 번째 인스턴스**, 보여주기 전용 |

태블릿 캔버스는 `GraphicRaycaster`가 없어 클릭을 받지 못한다. 그래서 `TabletLogic`이
오버레이 쪽 조작에 맞춰 태블릿 표시를 **손으로 동기화**하고 있다.

지금까지 조사에서 이해되지 않던 것들이 전부 여기서 나왔다 —
태블릿 인스턴스의 버튼 `m_Target`이 비어 있던 것, 같은 메소드가 씬마다 다르게
바인딩된 것, `MenuManager`가 둘인 것.

### 2.2 메뉴 캔버스가 씬을 넘어 살아남는다

`Canvas_DontDestroyMenu` 오브젝트가 `DontDestroyMenu` 컴포넌트를 갖고 있고,
이 컴포넌트는 `Singleton<T>`(기본 `IsPersistent = true`)를 통해
자기 게임오브젝트에 `DontDestroyOnLoad`를 건다. 즉 **타이틀의 메뉴 캔버스 전체가
GameScene까지 따라간다.**

그 결과:

- 같은 패널이 두 씬에서 다른 일을 해야 한다 → 메소드가 두 벌로 갈라진다
- GameScene은 자기 몫의 `MenuManager`를 또 하나 갖는다 → 중복
- 씬 경계를 넘는 참조를 인스펙터로 연결할 수 없다 → `FindObjectOfType`

### 2.3 씬 상태의 진실 원본이 둘이다

Unity의 실제 씬과, `DontDestroyMenu._currentScene`이라는 수동 추적 값이 따로 논다.
후자를 근거로 `MenuBar`(크기), `MenuList`(간격), `ResumeButton`(표시 여부),
`MenuManager`(시작 패널) 네 곳이 분기한다.
그런데 `ChangeTitleScene()`은 **아무도 호출하지 않는다** — 한 번 `Game`이 되면
영영 `Game`이다.

### 2.4 패널을 이름표로 찾는다

```csharp
_panelList.Find(panel => panel.panelName == currName).Show();
```

패널마다 `panelName`을 들고, 그 값을 각 패널의 `Setup()`이 코드로 덮어쓰고,
매니저는 리스트를 훑어 찾는다. 직렬화된 값은 어차피 코드가 덮어쓰므로 의미가 없다.
컨트롤러가 패널을 **필드로 직접 들고 있으면** 이 층 전체가 사라진다.

---

## 3. 목표 구조

씬마다 **UI 루트 하나 + 컨트롤러 하나**. 컨트롤러가 그 씬 UI의 유일한 진입점이다.

```
TitleScene                          GameScene
  TitleUI (Canvas)                    MatchUI (Canvas)
    TitleUIController                   MatchUIController
      ├ StartPanel                        ├ PlayerPanel x2
      ├ ModeSelectPanel                   ├ ResultPanel
      ├ GameSettingPanel                  ├ ResumePanel
      └ CommonMenu (프리팹)                └ CommonMenu (프리팹)
          MenuBar · Option · Exit · Help      MenuBar · Option · Exit · Help

[영구 객체는 데이터·오디오만]
  GameManager · DataManager · SoundManager
```

### 역할 경계

| | 하는 일 | 하지 않는 일 |
|---|---|---|
| **패널** | 자기 버튼을 코드로 배선하고, 결과를 이벤트로 알린다 | 다른 패널을 모른다 |
| **컨트롤러** | 패널 참조를 직접 소유하고 조율한다. 의도 단위 API만 노출 | 씬을 직접 로드하지 않는다 |
| **GameManager** | 씬 전환과 대전 설정 | UI를 모른다 |

컨트롤러가 노출하는 것은 `ShowModeSelect()`, `ShowGameSetting()`, `StartMatch()`처럼
**무엇을 하려는지**만 담은 메소드다. 어느 패널을 켜고 끄는지는 컨트롤러 안에 갇힌다.

### 사라지는 것

| 없어짐 | 이유 |
|---|---|
| `DontDestroyMenu` | 씬 상태를 수동 추적할 이유가 없어짐 |
| `UIPanelName` · `PanelBase.panelName` | 컨트롤러가 패널을 직접 참조 |
| `MenuManager` | 컨트롤러로 대체 |
| `AddResumeAction` | `MatchUIController`가 자기 `ResumePanel`을 구독 |
| `TabletLogic`의 수동 동기화 | 복제본이 없어짐 |
| `GameSettingPanel.OnClickGamePlayButtonGameScene` | 씬마다 컨트롤러가 다르므로 이중 경로 불필요 |
| `ResultPanel`의 `FindObjectOfType` 2곳 | 컨트롤러가 연결 |
| `MenuBar`·`MenuList`·`ResumeButton`의 씬 분기 | 씬마다 다른 인스턴스라 인스펙터 값으로 구분 |
| `ListButton`의 빈 `Show`/`Hide` 오버라이드 | 컨트롤러가 무엇을 토글할지 알고 있음 |

작업이 끝나면 `FindObjectOfType`이 코드에서 사라진다.

---

## 4. 태블릿 연출 — 지금은 제거, 나중에 재도입

### 지금 하는 것

태블릿의 복제 UI를 제거한다. 태블릿은 화면이 꺼진 소품으로 남는다.
`TabletLogic`의 수동 동기화도 함께 제거한다.

### 나중에 다시 넣을 때를 위한 설계 제약

재도입 방법은 그때 정하되, **지금의 설계가 그 길을 막지 않도록** 한 가지만 지킨다.

> **컨트롤러는 화면 전환을 이벤트로 알린다.**
> ```csharp
> public event Action<TitleScreen> ScreenChanged;   // Loading · ModeSelect · GameSetting
> ```

이렇게 해 두면 나중에 태블릿 표시를 담당하는 뷰가 이 이벤트를 구독하는 것만으로
따라오게 만들 수 있다. **손으로 맞추는 코드가 다시 생기지 않는다.**

재도입 시 선택지(그때 판단):

1. **World Space 캔버스를 진짜 UI로 승격** — `GraphicRaycaster` 추가 + `worldCamera` 지정.
   태블릿을 직접 클릭한다. 복제본이 아예 필요 없다. 가장 깔끔하다.
2. **RenderTexture** — 오버레이 UI를 텍스처로 렌더해 태블릿 화면에 입힌다.
   클릭 좌표 변환이 필요해 복잡하다.

지금 시점의 선호는 **1번**이다. 태블릿 캔버스는 이미 World Space이므로
컴포넌트 두 개만 붙이면 되고, Unity가 3D 공간 UI 클릭을 기본 지원한다.

---

## 5. 이행 순서

에셋(씬·프리팹) 재구성이 가장 위험하므로 코드로 받침을 먼저 만든다.
**각 단계가 끝날 때마다 게임이 돌아가는 상태**를 유지한다.

| 단계 | 내용 | 대상 | 위험 |
|---|---|---|---|
| **S0** | 태블릿 복제 UI 제거 + `TabletLogic` 동기화 제거 | 에셋 + 코드 | ★★☆ |
| **S1** | `GameManager`에 씬 전환 API 정리 (`LoadMatch()` / `LoadTitle(entry)`), 재시작을 씬 리로드로 | 코드 | ★★☆ |
| **S2** | 모든 이벤트 구독에 `OnDestroy` 해제 추가 | 코드 | ★☆☆ |
| **S3** | `MatchUIController` 신설 — GameScene UI부터 | 코드 + 참조 연결 | ★★☆ |
| **S4** | `TitleUIController` 신설 + `CommonMenu` 프리팹 분리 | 코드 + 씬 재구성 | ★★★ |
| **S5** | `DontDestroyOnLoad` 해제, `DontDestroyMenu`·`MenuManager`·`AddResumeAction` 제거 | 코드 + 씬 | ★★★ |
| **S6** | 남은 인스펙터 바인딩 일괄 제거 + 검사기 뒤집기 | 도구 실행 | ★☆☆ |

### 왜 이 순서인가

- **S0을 먼저** — 복제본이 남아 있으면 이후 모든 단계에서 "어느 쪽 얘기인가"를 계속 따져야 한다.
- **S2가 S1 다음** — 씬 리로드를 도입하는 순간 구독 해제가 선택이 아니라 필수가 된다.
  파괴된 오브젝트를 가리키는 델리게이트가 쌓인다.
- **GameScene(S3)을 TitleScene(S4)보다 먼저** — 패널이 3종뿐이고 타이틀 UI에 의존하지 않아,
  컨트롤러 패턴이 맞는지 작은 범위에서 먼저 확인할 수 있다.
- **S5가 마지막에서 두 번째** — 두 컨트롤러가 자리를 잡은 뒤에야 영구 캔버스를 뗄 수 있다.

### 단계별 검증

1. `dotnet build` — 오류 0. 단, **경고에 우리 소스 경로가 실제로 찍히는지** 확인한다.
   (Phase 5에서 `Assembly-CSharp`가 빌드 대상에서 조용히 제외돼, 컴파일된 적도 없는데
   오류 0으로 보이던 함정이 있었다.)
2. `Tools > Magnetic Chess > Validate UnityEvent Bindings` — 끊어진 바인딩 0
3. `Tools > Magnetic Chess > Validate Prefab Overrides` — 고아 오버라이드 0
4. 플레이 테스트 — 오프라인 2인 / AI, 그리고 **판 재시작 2회차**까지

---

## 6. 하지 않을 것

| 항목 | 이유 |
|---|---|
| UI 프레임워크·DI 컨테이너 도입 | 49파일 프로젝트에 순손실 |
| 패널마다 MVP/MVVM 계층 분리 | 패널이 6개뿐. 컨트롤러 한 층이면 충분 |
| 전역 이벤트 버스 | 컨트롤러가 직접 참조하므로 필요 없음. 추적 불가능해지기만 한다 |
| 단일 씬 통합 | 씬 리로드로 상태를 초기화하는 이점을 잃는다 |
| 태블릿 RenderTexture 방식 | 클릭 좌표 변환 비용 대비 이득이 작다. World Space 승격이 더 간단 |
| 씬 분기를 `SceneManager` 조회로 대체 | 분기 자체를 없애는 것이 목적. 조회로 바꾸면 문제가 남는다 |

---

## 7. 이미 끝난 것

이 계획 이전에 완료된 작업. 그대로 살아남는다 — 컨트롤러가 생겨도
각 패널이 자기 버튼을 배선하는 방식은 동일하다.

| 커밋 | 내용 |
|---|---|
| `d7b95f6` | 에디터 도구: UnityEvent 바인딩 검사기, 에셋 재직렬화 |
| `ccca30d` | 코드 컨벤션 통일 (클래스 13 · 메소드 51 · 필드 108 · 상수 25) |
| `56f310f` | `UIBinder` 신설, `ResultPanel`·`ResumePanel` 배선, 끊어진 참조 4개 복구, 오버라이드 검사기 |
| `800c4ea` | 모드 버튼 배선, `ModeBase`로 중복 제거 (93줄 → 33줄) |

### 이 과정에서 얻은 규칙

- **버튼 핸들러는 `private`으로 둔다.** UnityEvent는 public 인스턴스 메소드만 호출하므로,
  인스펙터에 남은 옛 바인딩이 자동으로 무력화된다. 패널을 하나씩 옮길 수 있게 된다.
- **클릭 소리는 `UIBinder`가 붙인다.** 규칙을 핸들러마다 적어두면 언젠가 빠뜨린다.
- **`[FormerlySerializedAs]`는 프리팹 인스턴스의 오버라이드를 옮겨주지 않는다.**
  필드명을 바꾸면 `m_Modifications`의 `propertyPath`는 고아가 된다.
  `PrefabOverrideValidator`가 이것을 잡는다.
- **에셋이 실제로 다시 쓰였는지는 파일 수정 시각으로 확인한다.**
  도구가 "처리했다"고 보고해도 파일이 그대로일 수 있다.

---

## 8. 착수 전 확인할 것

에디터를 열지 않고 씬 YAML을 파싱해 채웠다. 전문은 `REFACTORING_LOG.md` 0절에 있다.

- [x] `Canvas_Menu`와 `Canvas_DontDestroyMenu` 각각에 어떤 패널이 들어 있는가
      → `Canvas_Menu`(+`MenuManager`): `Start Panel`, `GameSettingPanel`, `Mode Select Panel`
      → `Canvas_DontDestroyMenu`(+`DontDestroyMenu`): `Help Button`, `MenuBar`, `Help Panel`,
        `Resume Panel`, `Exit Panel`, `Loading Window`, `Option Panel`
- [x] `Tablet_UI` 프리팹 인스턴스가 씬 계층 어디에 있는가 → **씬 루트**. `Meshes/Tablet_Window` 메시는 별개
- [x] 태블릿 복제본이 참조하는 프리팹이 오버레이 쪽과 동일한 것인가
      → **아니다.** `Tablet_UI`는 자체 프리팹이고 그 안에 별도의 `GameSettingMenu`를 하나 더 갖고 있었다.
        실제 복제본은 두 씬이 각각 들고 있던 `GameSettingPanel`·`Mode Select Panel` 쪽이다 (2.1절 정정)
- [x] GameScene의 `MenuManager`가 붙어 있는 오브젝트와 그 자식 구성
      → `Menu_Canvas`(+`AddResumeAction`): `StartButton`, `ResultPanel`, `PreventTouchScreenImage`,
        `Background`, `GameSettingPanel`, `Mode Select Panel`
- [x] 로딩 화면(`loadingWindow`)·페이드(`fadeWindow`)가 어느 캔버스 소속인가
      → 둘 다 `Canvas_DontDestroyMenu/Loading Window`. **S5의 가장 큰 걸림돌이었다** —
        영구 캔버스를 떼면 이 창이 씬과 함께 파괴되므로, 덮는 쪽(`LoadingScreen`)과
        걷는 쪽(`SceneFadeIn`)을 서로 다른 씬의 컴포넌트로 나눴다

### 2.1절 정정

태블릿 캔버스에는 `GraphicRaycaster`가 **있다.** 클릭을 받지 못한 이유는 그것이 아니라
World Space인데 `m_Camera`가 비어 있어서다. 4절의 재도입 1번 안(World Space 승격)은
컴포넌트 추가 없이 `worldCamera` 지정만으로 된다는 뜻이다.
