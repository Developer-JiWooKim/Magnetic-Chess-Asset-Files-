# 리팩토링 기록

Unity 2022.3 → 6.5.3 업그레이드를 계기로 진행하는 코드 리팩토링 작업 기록. 각 항목은 "무엇을 왜 바꿨고, 이전엔 어떻게 되어있었는지"를 남긴다. 전체 계획은 `C:\Users\Admin\.claude\plans\wiggly-scribbling-wave.md` 참고.

---

## 2026-09-01 — Unity 6.5.3 업그레이드 + 스크립트 인코딩 정리

**브랜치/커밋:** `unity6-upgrade` (`ae059ef`)

**작업 내용:**
- 프로젝트를 Unity 2022.3에서 6.5.3(6000.5.3f1)으로 업그레이드. URP 17.5, TextMeshPro 등 관련 패키지가 재직렬화되며 다수의 `.mat` 파일이 갱신됨.
- 전체 51개 C# 스크립트의 인코딩을 CP949(EUC-KR)에서 UTF-8로 변환. 이 과정에서 기존 한글 주석은 전부 제거.

**이전 상태:**
- 프로젝트가 Unity 2022.3 기준으로 작성되어 있었고, 모든 `.cs` 파일이 CP949로 저장되어 있어 UTF-8 기반 에디터(VSCode, Rider 등)에서 한글 주석이 깨져 보였음(`ISO-8859 text`로 감지됨).

**이유:**
- Unity 6 업그레이드 겸 예전 프로젝트를 다시 열어보니 CP949 인코딩 문제가 확인되어, 앞으로 코드를 더 손보기 전에 먼저 정리. 어차피 전체적으로 코드를 다시 볼 예정이라 주석은 남기지 않고 제거하기로 결정.

---

## 2026-09-01 — Phase 0: UI 패널 구조 통합

**관련 계획:** `wiggly-scribbling-wave.md` Phase 0

**작업 내용:**
- 새 공통 베이스 `Assets/Scripts/AllScene/UIPanel.cs` 추가. `virtual Show()`(기본 `SetActive(true)`) / `virtual Hide()`(기본 `SetActive(false)`).
- `Panel_Base`가 `MonoBehaviour` 대신 `UIPanel`을 상속하도록 변경, 자체 `ShowPanel()/HidePanel()` 정의를 제거(이제 `UIPanel`의 것을 그대로 상속). `panel_Name` 필드는 유지.
- `StartPanel`, `ModeSelectPanel`, `GameSettingPanel`(모두 `Panel_Base` 상속)의 `override ShowPanel()/HidePanel()` → `override Show()/Hide()`로 개명. 호출부인 `Menu_Manager.ChangePanel()`도 `panel.ShowPanel()/HidePanel()` → `panel.Show()/Hide()`로 함께 수정.
- `MenuButtonBase`(`Show`/`Hide`만 있던 별도 베이스 클래스) 완전 삭제. 이걸 상속하던 `ExitButton`, `OptionButton`, `ResumeButton`, `ListButton`이 전부 `UIPanel`을 직접 상속하도록 변경. `ExitButton`/`OptionButton`은 override 내용이 `UIPanel` 기본 동작과 완전히 같아서 override 자체를 제거(빈 서브클래스로 축소), `ResumeButton`은 `Show()`에 씬 조건부 로직이 있어 그 부분만 유지, `ListButton`은 원래 `Show()/Hide()`가 빈 메서드(no-op)였는데 — 이건 "리스트 토글 버튼 자신은 다른 버튼들과 함께 꺼지면 안 된다"는 의도된 동작이라 그대로 빈 override 유지.
- `MenuList.cs`의 `List<MenuButtonBase>` → `List<UIPanel>`로 타입 변경 (`GetComponentsInChildren<UIPanel>()`).
- `OptionPanel`(`MonoBehaviour` → `UIPanel` 상속), `ExitPanel`(`MonoBehaviour` → `UIPanel`), `ResumePanel`(`MonoBehaviour` → `UIPanel`), `Result_Panel`(`MonoBehaviour` → `UIPanel`), `HelpPanel`(`MonoBehaviour` → `UIPanel`)로 전부 통일:
  - `OptionPanel.ShowPanel()/HidePanel()` → `override Show()/Hide()`
  - `ResumePanel.OnClickResumeButton()/OnClickContinueButton()` → `override Show()/Hide()` (`OnClickReplayButton`/`OnClickSelectModeButton` 내부에서 쓰던 `OnClickContinueButton()` 호출도 `Hide()`로 변경)
  - `ExitPanel.OnClickExitButton()/OnClickExit_no_Button()` → `override Show()/Hide()` (`OnClickExit_yes_Button()`은 그대로 유지)
  - `Result_Panel`은 원래 Show/Hide가 아예 없었음(호출부에서 직접 `SetActive`) — `UIPanel` 상속만 추가하고 기본 동작 사용. 이걸 호출하던 `GameDirector.cs`의 `result_Panel.gameObject.SetActive(true)`(EndBattle) / `SetActive(false)`(Setup)를 각각 `result_Panel.Show()` / `Hide()`로, `Result_Panel.OnClickQuitButton()`의 `exitPanel.OnClickExitButton()`도 `exitPanel.Show()`로 수정
  - `HelpPanel`은 별도 상태가 없어 override 없이 `UIPanel` 기본 동작 사용
- **계획에서 수정한 부분**: 원래 계획엔 `Menu_Manager.Change_Start_to_ModeSelect()`가 다른 두 전환 메서드처럼 공통 `ChangePanel()`을 거치도록 고치려 했으나, 구현 중 확인해보니 이건 버그가 아니라 의도된 연출이었음. Start → ModeSelect 전환은 `StartPanel.Hide()`(카메라 페이드아웃 + `MoveStart` 애니메이션 트리거) → 카메라 애니메이션 클립의 Animation Event(`Camera_Animation_Event.Loading_UI_Show()`) → `Tablet_Logic.Tablet_Logic_Start()`가 로딩 스피너를 띄우고 1.5초 뒤에 `ModeSelectPanel.Show()`를 별도로 호출하는 구조. 즉시 `ChangePanel()`을 태우면 이 타이밍 연출이 깨지므로, **전환 흐름 자체는 손대지 않고 메서드명만 통일**(`StartPanel.OnClickStartButton()`은 이름 그대로 유지, 내부에서 `Hide()` 호출; `Tablet_Logic.cs`의 `modeSelectPanel.ShowPanel()` → `Show()`로만 개명).

**이전 상태:**
- "패널 보이기/숨기기"라는 동일한 개념이 `Panel_Base`(`ShowPanel/HidePanel`), `MenuButtonBase`(`Show/Hide`), 그리고 클래스마다 제각각인 이름(`OnClickResumeButton`, `OnClickExitButton` 등)의 세 갈래로 중복 구현되어 있었음. `Result_Panel`은 아예 Show/Hide 메서드가 없어서 호출부(`GameDirector`)가 `SetActive`를 직접 호출.

**이유:**
- 여러 패널이 사실상 같은 일(보이기/숨기기)을 서로 다른 이름으로 구현하고 있어 신규 패널 추가나 유지보수 시 일관성이 없었음. Unity 6.5.3 이식을 계기로 전체 구조를 정리하기로 하면서 UI 쪽도 통일 대상에 포함.

**⚠️ 에디터에서 수동으로 재연결해야 하는 버튼 OnClick (TitleScene.unity, 아직 미완료 — 다음에 에디터 열 때 처리 필요):**
- `ExitPanel.OnClickExitButton` 바인딩 2곳 → `Show()`로 재지정
- `ExitPanel.OnClickExit_no_Button` 바인딩 1곳 → `Hide()`로 재지정
- `ResumePanel.OnClickResumeButton` 바인딩 1곳 → `Show()`로 재지정
- `ResumePanel.OnClickContinueButton` 바인딩 1곳 → `Hide()`로 재지정
- `OptionPanel.ShowPanel` 바인딩 1곳 → `Show()`로 재지정

~~이 외 메서드(Panel_Base 3종, MenuButtonBase 계열, `OptionPanel.HidePanel`)는 전부 C# 코드에서만 호출되어 재바인딩 불필요함을 확인함.~~ → **이 판단은 틀렸음. 아래 "메뉴 버튼 먹통 수정" 항목 참고** (프리팹을 검색 대상에 넣지 않아 4곳을 놓쳤음).

---

## 2026-09-01 — Phase 0.5: 스크립트 폴더 구조 정리

**관련 계획:** `wiggly-scribbling-wave.md` Phase 0.5

**작업 내용:**
- `Assets/Scripts/` 하위 폴더를 전부 재편. 모든 이동은 `git mv`로 `.cs`+`.cs.meta` 쌍을 함께 옮겨 GUID(=씬/프리팹의 컴포넌트 참조) 보존.
- `AllScene/`의 싱글턴·데이터 타입(`GameManager`, `DataManager`, `SoundManager`, `DontDestroy_Menu`, `GameSetting`, `OptionData`) → `Core/`로 이동.
- `AllScene/`의 UI류(`Menu List/*`, `Exit Panel/*`, `Help Panel/*`, `Option Panel/*`)와 `Title Scene/FadeEffect_UI.cs`(GameScene에서도 쓰는 공용 유틸) → `UI/`로 통합, 하위 폴더 없이 평탄화.
- `Title Scene/` → `TitleScene/`로 개명, `Canvas_Menu/` 하위 3단 폴더(`Start Panel`, `Mode Select Panel`, `Game Setting Panel`) → `TitleScene/Menu/`로 평탄화.
- `GameScene/` 하위 폴더명에서 공백 제거(`Camera Move`→`Camera`, `Magnet System`→`Magnet`), `Game System/GameDirector.cs`는 파일이 하나뿐이라 `GameScene/` 바로 밑으로, `MemoryPool/`의 두 파일은 `Magnet/`로 흡수(어차피 Phase 4에서 삭제될 파일들).
- 이동 후 남은 빈 폴더와 그 폴더들의 stray `.meta`(`AllScene.meta`, `Title Scene.meta`, `Camera Move.meta` 등) 전부 정리.
- 코드 자체는 네임스페이스가 없어 전역 클래스명으로 참조하므로, 폴더 이동만으로는 `using` 이나 컴파일에 영향 없음 — 실제 `.cs` 내용은 건드리지 않음.

**이전 상태:**
```
Assets/Scripts/
  AllScene/ (싱글턴 + UI 패널류가 뒤섞임, 하위에 "Menu List", "Exit Panel" 등 공백 폴더명)
  GameScene/ ("Camera Move", "Game System", "Magnet System", "MemoryPool" 등 공백 폴더명)
  Title Scene/ (공백 포함 최상위 폴더명, Canvas_Menu 하위에 3단 중첩)
```

**이유:**
- 폴더명에 공백/붙여쓰기가 뒤섞여 있었고, "씬 기준" 분류와 "기능 기준" 분류가 한 depth에 섞여 있어 일관성이 없었음. `AllScene`은 특히 싱글턴/데이터와 UI 패널이 구분 없이 섞여 있어 새 스크립트를 어디에 둬야 할지 애매했음. Unity 6.5.3 이식을 계기로 전체 구조를 손보면서 폴더 구조도 함께 정리하기로 함.

**검증:** Unity 에디터로 열어서 콘솔에 "missing script" 경고가 없는지 확인 필요 (아직 미확인 — 다음에 에디터 열 때 체크).

---

## 2026-09-01 — Phase 0.5 후속: 씬 기준 → 기능 기준 폴더 재편

**작업 내용:**
- 바로 위 Phase 0.5에서 `TitleScene/`·`GameScene/`로 나눴던 구조를 완전히 없애고, 시스템/기능 기준 폴더로 재편. `Core/`, `UI/`는 그대로 두고 나머지를 아래처럼 재배치(전부 `git mv`로 `.cs`+`.cs.meta` 쌍 이동):
  - `TitleScene/Menu/*`(14개 파일) → `MainMenu/`
  - `TitleScene/CameraMoving.cs` + `GameScene/Camera/CameraView.cs`, `Character_FaceCam.cs` → `Camera/`(씬 구분 없이 카메라 스크립트 전부 통합)
  - `GameScene/GameDirector.cs`, `GameScene/Player/Player.cs`, `GameScene/UI/*`(6개 파일) → `Match/`
  - `GameScene/AI/AI_FSM.cs` → `AI/`(최상위로 승격)
  - `GameScene/Magnet/*`(7개 파일) → `Magnet/`(최상위로 승격)
- 이동 후 빈 껍데기만 남은 `TitleScene/`, `GameScene/`와 그 하위 폴더들의 stray `.meta`(`GameScene.meta`, `GameScene/AI.meta`, `GameScene/Player.meta`, `GameScene/UI.meta`) 전부 제거.

**이전 상태:** 바로 위 항목의 "이후 상태"(`TitleScene/`, `GameScene/` 씬 기준 2단 분류)가 이번 항목의 "이전 상태"임.

**이유:** 사용자가 "왜 TitleScene/GameScene으로 나눴냐"고 물어본 뒤, 씬이 2개뿐인 프로젝트라 기능 기준이 더 낫겠다고 판단해서 요청. 카메라처럼 두 씬에 각각 존재하지만 본질적으로 같은 역할을 하는 스크립트를 한 곳에서 볼 수 있게 됨.

---

## 2026-09-01 — 전체 스크립트에 폴더 기준 namespace 적용

**작업 내용:**
- 51개 스크립트 전부에 `namespace Assets.Scripts.<폴더명>` 적용 (`AI`, `Camera`, `Core`, `MainMenu`, `Magnet`, `Match`, `UI` — 7개, 폴더 구조와 1:1 대응).
- 프로젝트가 지금까지 네임스페이스가 전혀 없는 전역(global) 클래스 구조였기 때문에, 폴더를 나누는 순간부터 다른 폴더의 타입을 쓰는 곳마다 `using Assets.Scripts.X;`를 추가해야 함 — 파일마다 실제로 참조하는 타입을 전부 추적해서(문자열 리터럴에 있는 동명의 단어는 걸러내고) 필요한 `using`만 정확히 추가.
- **`Camera` 네임스페이스 충돌 이슈 발견 및 대응**: `Character_FaceCam.cs`가 `UnityEngine.Camera` 타입을 필드로 쓰는데, 이 파일이 `namespace Assets.Scripts.Camera` 안에 들어가면서 `Camera`라는 이름이 자기 네임스페이스 이름과 겹쳐 컴파일 모호성이 발생. 해당 필드 타입을 `UnityEngine.Camera`로 명시적으로 풀네임 처리해서 해결. `GameDirector.cs`의 `Camera.main` 호출도 동일한 이유로 `UnityEngine.Camera.main`으로 수정.
- C# 언어 버전이 9.0(`LangVersion` 9.0, 파일 스코프 namespace 문법인 C# 10 미지원)이라 `namespace X { ... }` 블록 스타일로 전체 파일 내용을 감싸고 4칸 들여쓰기 적용.
- **빌드로 실제 검증**: 프로젝트에 이미 생성되어 있던 `Assembly-CSharp.csproj`(Unity/에디터 연동 도구가 자동으로 최신 경로에 맞춰 갱신해둔 상태였음)를 대상으로 `dotnet build`를 돌려서 확인. 우리 스크립트 관련 오류/경고 0건. 빌드에서 뜨는 오류 2건은 Unity ProBuilder 패키지 자체의 기존 버그(`ObjectPool<>` 모호한 참조)로 이번 작업과 무관함을 확인.

**이전 상태:** 모든 클래스가 네임스페이스 없이 전역(global) 스코프에 정의되어 있었음.

**이유:** 폴더 구조를 정리한 김에 네임스페이스도 폴더와 일치시켜서, 어떤 타입이 어느 시스템 소속인지 코드만 보고도 명확히 알 수 있게 하기 위함(사용자 요청).

---

## 2026-09-01 — Camera 폴더 해체 및 죽은 코드 제거

**작업 내용:**
- **`Camera/` 폴더 해체**: 세 파일의 실제 사용처를 확인해보니 하나의 시스템이 아니라 이름만 보고 묶인 상태였음.
  - `CameraView.cs` → `Match/` (유일한 사용처가 `GameDirector`)
  - `Character_FaceCam.cs` → `Match/` (유일한 사용처가 `InGameUI_Manager`. 이름과 달리 카메라 시스템이 아니라 플레이어 패널의 얼굴 초상화를 비추는 HUD 부속임)
  - `CameraMoving.cs` → **삭제** (`Start`/`Update`가 빈 껍데기. 코드 참조 0건, 씬·프리팹에 GUID 참조 0건 확인)
- **`UI/GameOption.cs` 삭제** — 해상도 옵션을 만들려다 만 스텁 클래스. MonoBehaviour도 아니고 참조 0건.
- 위 폴더 해체로 `UnityEngine.Camera` 이름 충돌의 원인이 사라져서, 직전 작업에서 넣었던 우회 코드를 원복:
  - `Character_FaceCam.cs`: `UnityEngine.Camera[]` → `Camera[]`
  - `GameDirector.cs`: `UnityEngine.Camera.main` → `Camera.main`
  - `GameDirector.cs`/`InGameUI_Manager.cs`에서 불필요해진 `using Assets.Scripts.Camera;` 제거
- 이동한 두 파일의 namespace를 `Assets.Scripts.Match`로 변경. `dotnet build`로 검증 — 우리 코드 오류 0건.

**⚠️ 삭제하려다 취소한 항목 — `Menu_Manager.All_HidePanel()`:**
직전 분석에서 "죽은 코드"로 판단했으나, 삭제 직전 재확인 결과 **살아있는 코드**였음. `GameScene.unity`에서 GameSetting 프리팹 인스턴스 오버라이드 형태로 GamePlay 버튼의 OnClick 3번째 호출에 연결되어 있음. 이전 확인이 불완전했던 이유는 `TitleScene.unity`만, 그것도 `m_MethodName: All_HidePanel` 형식으로만 검색했기 때문 — 프리팹 오버라이드는 `propertyPath: ...m_MethodName` / `value: All_HidePanel` 형식이라 걸리지 않았음. **교훈: Unity에서 메서드 삭제 전에는 두 씬 파일 + 프리팹을 대상으로 두 가지 형식 모두 검색할 것.**

**이후 구조 (6개 폴더):** `AI`, `Core`, `MainMenu`, `Magnet`, `Match`, `UI`

**이유:** 폴더 구조 적정성 분석 결과 `Camera/`만 응집도가 없었고(서로 무관한 3개 파일 + 죽은 파일 1개), 네임스페이스 이름 충돌까지 유발하고 있어 해체하는 편이 나았음.

---

## 2026-09-01 — Phase 2: 제네릭 싱글턴 베이스로 통합

**관련 계획:** `wiggly-scribbling-wave.md` Phase 2

**작업 내용:**
- 새 파일 `Assets/Scripts/Core/Singleton.cs` — `public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>`.
  - 제약을 `where T : MonoBehaviour`가 아니라 **`where T : Singleton<T>`(CRTP)** 로 둠. `class Foo : Singleton<Bar>` 같은 실수를 컴파일 단계에서 걸러내기 위함.
  - 셋업을 `protected virtual void Awake()`에 둠. 이렇게 하면 파생 클래스가 실수로 `private void Awake()`를 다시 정의했을 때 컴파일러가 "상속된 멤버를 숨깁니다" 경고를 내줌 — Unity에서 가장 흔한 함정(파생 Awake가 베이스 Awake를 가려서 싱글턴이 조용히 초기화되지 않는 문제)에 대한 방어.
- `GameManager`, `DataManager`, `SoundManager`, `DontDestroy_Menu` 4개가 이 베이스를 상속하도록 변경. 각자 갖고 있던 `instance` 필드 / `Instance` 프로퍼티 / `Awake()` / `SingletonSetup()`을 전부 제거 (4중 중복 → 1곳).
- `DataManager`만 Awake에 추가 작업(`LoadGameOptionData()`)이 있어서 `protected override void Awake() { base.Awake(); LoadGameOptionData(); }` 형태로 순서를 그대로 보존.
- `DontDestroy_Menu`는 UnityEngine 타입을 직접 쓰는 코드가 전부 베이스로 올라가서 `using UnityEngine;`도 제거.
- `dotnet build` 검증 — 우리 코드 오류/경고 0건.

**동작이 미묘하게 달라진 부분(의도한 개선):**
- **null일 때 로그**: 기존에는 4개가 제각각이었음(`SoundManager`만 로그 출력, `GameManager`/`DataManager`는 조용히 null 반환, `DontDestroy_Menu`는 null 체크조차 없음). 이제 전부 `"<타입명> instance is null!!"` 로그로 통일. 호출부 어디에서도 `Instance`를 null 체크하지 않는 것(전부 `X.Instance.메서드()` 직접 호출)을 확인했기 때문에, 어차피 NullReference가 날 자리에서 **어느 매니저가 없는지 먼저 알려주는** 개선임.
- **중복 인스턴스 파괴 조건**: `else` → `else if (instance != this)`. 실제 중복 오브젝트 시나리오의 동작은 동일하고, Awake가 같은 인스턴스에서 두 번 돌 경우 자기 자신을 파괴하는 것만 막음.

**이전 상태:** 4개 클래스가 완전히 동일한 `SingletonSetup()`(instance 체크 → `DontDestroyOnLoad` → 중복 시 `Destroy`)을 각자 복붙해서 갖고 있었고, `Instance` 게터의 null 처리 방식만 제각각이었음.

---

## 2026-09-01 — Phase 1: 핫패스 씬 검색 제거

**관련 계획:** `wiggly-scribbling-wave.md` Phase 1

### 진짜 핫패스 2곳 (이번 Phase의 핵심)

**1. `MagnetWorld.FixedUpdate()`의 `FindObjectsOfType<Magnet>()` — 매 물리 프레임 씬 전체 검색**
- `Magnet`이 `OnEnable`/`OnDisable`에서 자기 자신을 `static List<Magnet>`에 등록/해제하도록 변경하고, `MagnetWorld`는 `Magnet.ActiveMagnets`를 읽도록 교체.
- **동작 동일성**: `FindObjectsOfType<T>()`는 기본적으로 비활성 오브젝트를 제외하는데, `OnEnable`/`OnDisable` 등록은 정확히 같은 집합(활성 오브젝트만)을 만든다. 자석볼이 오브젝트 풀로 `SetActive(false)` 되며 관리되기 때문에 이 부분이 중요했음.

**2. `MagnetContact.OnCollisionEnter()`의 `FindObjectOfType<GameDirector>()` — 매 충돌마다 씬 전체 검색**
- `GameDirector`를 `Singleton<GameDirector>` 상속으로 바꾸고 `GameDirector.Instance`로 교체.
- `GameDirector`는 GameScene에만 존재하고 씬과 함께 사라져야 하므로, `Singleton<T>`에 **`protected virtual bool IsPersistent => true`** 를 추가하고 `GameDirector`만 `false`로 override. (DontDestroyOnLoad를 걸면 안 되는 씬 종속 매니저를 같은 싱글턴 베이스로 다룰 수 있게 됨)
- 같은 메서드 안에 있던 깨진 문자열의 `Debug.Log`도 함께 제거 — 충돌마다 문자열 결합 + 로그가 발생하던 자리라 핫패스 정리 목적에 부합. 무슨 일을 하는지는 한글 주석으로 대체.

### 계획과 달라진 부분 — `[SerializeField]` 전환은 일부만 가능

계획에는 나머지 `FindObjectOfType`도 전부 `[SerializeField]` 인스펙터 참조로 바꾼다고 되어 있었으나, **스크립트별 씬 배치를 GUID로 확인한 결과 대부분이 씬을 넘나드는 참조**여서 인스펙터 연결이 원천적으로 불가능했음:

| 스크립트 | 위치 | 찾는 대상 | 대상 위치 | 판정 |
|---|---|---|---|---|
| `InGameUI_Manager` | GameScene | `Character_FaceCam` | GameScene | ✅ SerializeField 전환 |
| `AddResumeAction` | GameScene | `InGameUI_Manager` | GameScene | ✅ SerializeField 전환 |
| `AddResumeAction` | GameScene | `ResumePanel` | **TitleScene** | ❌ 씬 간 참조 → 유지 |
| `Result_Panel` | GameScene | `ResumePanel`, `ExitPanel` | **TitleScene** | ❌ 씬 간 참조 → 유지 |
| `GameSettingPanel` | 프리팹 | `GameDirector` | GameScene | ❌ 프리팹은 씬 참조 불가 |

`ResumePanel`/`ExitPanel`은 TitleScene의 DontDestroyOnLoad 메뉴 캔버스에 있고 GameScene으로 넘어오는 구조라, GameScene 오브젝트의 인스펙터에서는 연결할 수 없음. 남은 4개 호출은 전부 `Start()`에서 **1회만** 실행되는 것이라 핫패스가 아니므로 그대로 두는 것이 맞다고 판단.

`GameSettingPanel`만은 예외적으로 개선 — 이번에 `GameDirector.Instance`가 생겼으므로, Start에서 미리 찾아 캐싱하던 것을 **버튼 클릭 시점에 조회**하도록 변경. 이 패널은 DontDestroyOnLoad 캔버스에 있어 TitleScene에서 Start가 돌 때는 GameDirector가 존재하지 않는데, 기존 코드는 그때 null을 캐싱하면 이후에도 계속 null이었음. 클릭 시점 조회로 바꿔 그 타이밍 문제까지 같이 해소.

### 부수 정리
- 문자열 태그 비교 → `CompareTag()` : `GameDirector`(Board), `MagnetContact`(Magnet), `MagnetBallSpawnPoint`(Magnet 2곳)
- `Invoke("AI_SpawnAndStartTimer", 0.5f)` → `Invoke(nameof(AI_SpawnAndStartTimer), 0.5f)`

**검증:** `dotnet build` — 우리 코드 오류/경고 0건.

**⚠️ 에디터에서 연결해야 하는 인스펙터 필드 (GameScene, 아직 미완료):**
- `InGameUI_Manager`의 `faceCam` → 씬의 `Character_FaceCam` 오브젝트
- `AddResumeAction`의 `inGameUI_Manager` → 씬의 `InGameUI_Manager` 오브젝트

두 필드는 연결하지 않으면 **런타임에 NullReference가 발생**하므로, 다음에 에디터를 열 때 반드시 채워야 함.

---

## 2026-09-01 — 메뉴 버튼 먹통 수정 (Phase 0 후속 — 놓친 바인딩 4곳)

**증상:** 플레이 시 콘솔 에러는 없는데 옵션 버튼을 눌러도 옵션창이 안 열리고, 인게임 멈춤(Resume) 버튼도 반응 없음.

**원인:** Phase 0에서 메서드명을 `Show()`/`Hide()`로 통일했는데, 씬·프리팹에 저장된 버튼 OnClick 바인딩은 **옛 메서드명 문자열 그대로** 남아 있었음. UnityEvent는 대상 메서드가 없으면 **에러 없이 조용히 아무것도 하지 않기 때문에** 콘솔이 깨끗한 채로 버튼만 죽는 형태로 나타남.

**Phase 0 체크리스트가 불완전했던 이유:** 당시 `.unity` 씬 파일만, 그것도 `m_MethodName:` 형식만 검색했음. 실제로는 두 가지를 더 봤어야 했음 —
1. **`.prefab` 파일** (`OptionPanel.prefab`, `Tablet_UI.prefab`에 바인딩이 있었음)
2. **프리팹 오버라이드 직렬화 형식** (`propertyPath: ...m_MethodName` / `value: <메서드명>`)

그 결과 6곳이라고 했던 것이 실제로는 **10곳**이었음. 특히 "재바인딩 불필요"라고 단정했던 `OptionPanel.HidePanel`이 `OptionPanel.prefab`에 실제로 바인딩되어 있었음.

**수정한 바인딩 10곳** (인스펙터에서 10개를 일일이 찾는 것보다 정확해서 파일의 해당 줄을 직접 수정):

| 파일 | 줄 | 이전 | 이후 |
|---|---|---|---|
| TitleScene.unity | 697, 965 | `OnClickExitButton` | `Show` |
| TitleScene.unity | 26638 | `OnClickExit_no_Button` | `Hide` |
| TitleScene.unity | 25994 | `OnClickResumeButton` | `Show` |
| TitleScene.unity | 25531 | `OnClickContinueButton` | `Hide` |
| TitleScene.unity | 30534 | `ShowPanel` | `Show` |
| TitleScene.unity | 4645 | `value: HidePanel` | `Hide` |
| OptionPanel.prefab | 2089 | `HidePanel` | `Hide` |
| Tablet_UI.prefab | 560, 565 | `HidePanel`, `ShowPanel` | `Hide`, `Show` |

**함께 확인한 것 (문제 없음):** `MenuList`가 `GetComponentsInChildren<MenuButtonBase>` → `<UIPanel>`로 바뀌면서 패널까지 잡히는 회귀를 의심했으나, MenuList GameObject의 자식은 Exit/Option/Resume/List 버튼 4개뿐이라 이전과 동일한 집합임을 확인.

**교훈 (재확인):** Unity에서 public 메서드명을 바꾸거나 지울 때는 반드시 **`.unity` + `.prefab` 양쪽**을, **`m_MethodName:` 와 `value:` 두 형식 모두** 검색할 것. 컴파일러도 콘솔도 잡아주지 않는 유일한 부류의 참조임.

---

<!-- 이후 작업은 이 아래에 최신순으로 추가 -->
