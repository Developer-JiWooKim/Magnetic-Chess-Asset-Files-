# Magnetic Chess — 리팩토링 기록

2020년에 Google Play에 출시했던 개인 프로젝트를 Unity 2022.3 → **Unity 6 (6000.5.3f1)** 로 이식하면서, 당시의 코드를 전면적으로 개선한 과정을 기록한 문서다. 각 항목은 **① 문제 상황 → ② 이전 코드 → ③ 변경 후 코드 → ④ 판단 근거 → ⑤ 결과 → ⑥ 계획 대비 달라진 점**의 형식으로 남긴다.

- **엔진:** Unity 6000.5.3f1 (URP 17.5), C# 9.0 / netstandard2.1
- **규모:** C# 스크립트 49개
- **기준 커밋:** 리팩토링 이전 상태는 `ae059ef`
- **전체 계획:** `wiggly-scribbling-wave.md`
- **검증 방식:** 자동 테스트가 없는 프로젝트라, 각 단계마다 `dotnet build`로 컴파일을 검증하고 실제 플레이(오프라인 2인 대전 / AI 대전)로 회귀를 확인했다.

## 한눈에 보기

| # | 작업 | 핵심 성과 |
|---|---|---|
| 0 | UI 패널 구조 통합 | 3갈래로 흩어진 표시/숨김 API를 `UIPanel` 하나로 통합 |
| 0.5 | 폴더 구조 재편 + namespace | 씬/기능이 혼재된 분류를 기능 기준 6개 폴더로, 전역 클래스 49개에 namespace 부여 |
| 1 | 핫패스 씬 검색 제거 | **매 물리 프레임** 씬 전체 검색 → 자기등록 리스트, **매 충돌** 씬 검색 → 싱글턴 참조 |
| 2 | 제네릭 싱글턴 통합 | 4개 클래스에 복붙돼 있던 싱글턴 보일러플레이트를 `Singleton<T>` 하나로 |
| 3 | GameManager 캡슐화 | public 필드 노출 → 컴파일러가 강제하는 읽기 전용 + 명시적 setter |

---

## Phase 1 — 핫패스에서 씬 전체 검색 제거

### ① 문제 상황

자석 물리 시뮬레이션의 핵심 루프가 **매 물리 프레임마다 씬 전체를 검색**하고 있었다. `FindObjectsOfType`은 씬의 모든 GameObject를 순회하는 O(n) 연산인데, 이것이 `FixedUpdate`(기본 초당 50회) 안에서 호출된 뒤 다시 O(n²) 자력 계산으로 이어졌다.

또 자석볼끼리 충돌할 때마다 `FindObjectOfType<GameDirector>()`로 씬을 검색해 매니저를 찾고 있었다. 자석볼이 수십 개 붙는 게임 특성상 충돌은 짧은 시간에 대량으로 발생한다.

### ② 이전 코드

```csharp
// MagnetWorld.cs — 매 물리 프레임 씬 전체 검색
private void FixedUpdate()
{
    if (IsActive == false) { return; }

    Magnet[] magnets = FindObjectsOfType<Magnet>();   // ← 초당 50회 씬 전체 순회

    for (int i = 0; i < magnets.Length; i++) { /* ... O(n²) 자력 계산 ... */ }
}
```

```csharp
// MagnetContact.cs — 매 충돌마다 씬 전체 검색
private void OnCollisionEnter(Collision collision)
{
    if (isContact) { return; }
    if (collision.collider.tag == "Magnet")
    {
        GameDirector director = FindObjectOfType<GameDirector>();   // ← 충돌마다 씬 전체 순회
        director.confirmTime += (GameManager.Instance.gameSetting.waitingTime / 2);
        // 충돌마다 문자열 결합 + 로그. 문자열 리터럴이라 CP949 깨짐이 그대로 남아 있었다.
        Debug.Log("OnCollisionEnter()에서 ...(깨진 문자열)... confirmTime에 "
                  + (GameManager.Instance.gameSetting.waitingTime / 2) + " 추가");
        isContact = true;
    }
}
```

### ③ 변경 후 코드

```csharp
// Magnet.cs — 각 자석이 자기 자신을 등록/해제
public sealed class Magnet : MonoBehaviour
{
    private static readonly List<Magnet> activeMagnets = new List<Magnet>();
    public static IReadOnlyList<Magnet> ActiveMagnets => activeMagnets;

    private void OnEnable()  => activeMagnets.Add(this);
    private void OnDisable() => activeMagnets.Remove(this);
}
```

```csharp
// MagnetWorld.cs — 검색 없이 리스트를 바로 사용
IReadOnlyList<Magnet> magnets = Magnet.ActiveMagnets;
for (int i = 0; i < magnets.Count; i++) { /* ... */ }
```

```csharp
// MagnetContact.cs
if (collision.collider.CompareTag("Magnet"))
{
    // 자석볼끼리 붙으면 그만큼 확정 대기 시간을 늘려준다.
    GameDirector.Instance.confirmTime += GameManager.Instance.CurrentSetting.waitingTime / 2;
    isContact = true;
}
```

### ④ 판단 근거

**`OnEnable`/`OnDisable`을 고른 이유가 핵심이다.** 이 게임의 자석볼은 오브젝트 풀로 관리되어 `SetActive(false)`로 비활성화된다. 그런데 `FindObjectsOfType<T>()`는 **기본적으로 비활성 오브젝트를 제외**한다. 즉 기존 코드는 "활성 자석만" 계산하고 있었다.

`OnEnable`/`OnDisable` 시점 등록은 정확히 이 집합(활성 오브젝트만)과 일치한다. 만약 `Awake`/`OnDestroy`에 등록했다면 풀에서 잠자는 자석까지 계산에 포함되어 **물리 동작이 조용히 달라졌을 것이다.** 성능 최적화에서 동작이 바뀌면 최적화가 아니라 버그다.

`GameDirector`는 씬에 하나뿐이라 싱글턴이 자연스러웠지만, **GameScene에만 존재하고 씬과 함께 사라져야 하므로** `DontDestroyOnLoad`를 걸면 안 됐다. 그래서 공용 `Singleton<T>`에 `IsPersistent` 훅을 추가해 이 클래스만 `false`로 재정의했다(Phase 2 참고).

충돌마다 실행되던 `Debug.Log`는 문자열 결합 비용까지 있어 함께 제거하고, 무슨 일을 하는지는 주석으로 남겼다.

### ⑤ 결과

- `FixedUpdate`의 씬 전체 순회 **완전 제거** (초당 50회 → 0회)
- 충돌 콜백의 씬 전체 순회 **완전 제거**, 문자열 결합 로그도 제거
- 부수적으로 문자열 태그 비교 4곳을 `CompareTag()`로, `Invoke("메서드명")`을 `Invoke(nameof(...))`로 교체해 오타가 컴파일 단계에서 잡히도록 함

### ⑥ 계획 대비 달라진 점

계획에는 "나머지 `FindObjectOfType`도 전부 `[SerializeField]` 인스펙터 참조로 교체"라고 적어뒀지만, **실행 전 스크립트별 씬 배치를 GUID로 조사한 결과 대부분 불가능했다.**

| 스크립트 | 위치 | 찾는 대상 | 대상 위치 | 판정 |
|---|---|---|---|---|
| `InGameUI_Manager` | GameScene | `Character_FaceCam` | GameScene | ✅ 전환 |
| `AddResumeAction` | GameScene | `InGameUI_Manager` | GameScene | ✅ 전환 |
| `AddResumeAction` | GameScene | `ResumePanel` | **TitleScene** | ❌ 씬 간 참조 |
| `Result_Panel` | GameScene | `ResumePanel`, `ExitPanel` | **TitleScene** | ❌ 씬 간 참조 |
| `GameSettingPanel` | 프리팹 | `GameDirector` | GameScene | ❌ 프리팹→씬 참조 불가 |

`ResumePanel`/`ExitPanel`은 TitleScene의 `DontDestroyOnLoad` 캔버스에 있고 GameScene으로 넘어오는 구조라, GameScene 오브젝트의 인스펙터에서는 **연결할 수 있는 대상 자체가 존재하지 않는다.** 남은 4개 호출은 모두 `Start()`에서 1회만 실행되는 것이라 핫패스가 아니므로 그대로 두는 것이 옳다고 판단했다.

대신 `GameSettingPanel`에서는 **계획에 없던 버그를 발견했다.** 이 패널은 `DontDestroyOnLoad` 캔버스에 있어 TitleScene에서 `Start()`가 도는데, 그 시점에는 GameDirector가 존재하지 않는다. 기존 코드는 그때 찾은 `null`을 필드에 캐싱한 뒤 **다시는 갱신하지 않았다.**

```csharp
// 이전 — TitleScene에서 null을 캐싱하고 끝
private void Setup()
{
    gameDirector = FindObjectOfType<GameDirector>();
    if (gameDirector == null) { Debug.Log("gameDirector is null"); }
}

// 이후 — 실제로 필요한 클릭 시점에 조회
public void OnClickGamePlayButton_GameScene()
{
    GameDirector director = GameDirector.Instance;
    if (director != null) { director.Setup(); }
    else { Debug.Log("GameDirector is null!"); }
}
```

---

## Phase 2 — 제네릭 싱글턴 베이스로 통합

### ① 문제 상황

싱글턴 4개(`GameManager`, `DataManager`, `SoundManager`, `DontDestroy_Menu`)가 **완전히 동일한 보일러플레이트를 각자 복사해서** 갖고 있었다. 게다가 `Instance` 게터의 null 처리 방식만 클래스마다 제각각이라, 어떤 매니저가 없을 때 어떤 클래스는 조용히 null을 반환하고 어떤 클래스는 로그를 남기는 등 진단 정보가 일관되지 않았다.

### ② 이전 코드

같은 코드가 4개 파일에 반복됐다.

```csharp
// SoundManager.cs — 로그를 남기는 버전
private static SoundManager instance;
public static SoundManager Instance
{
    get
    {
        if (instance == null) { Debug.Log("Sound Manager instance is null!!"); return null; }
        return instance;
    }
}
private void Awake() { SingletonSetup(); }
private void SingletonSetup()
{
    if (instance == null) { instance = this; DontDestroyOnLoad(this.gameObject); }
    else { Destroy(this.gameObject); }
}
```

```csharp
// GameManager.cs / DataManager.cs — 조용히 null을 반환하는 버전
private static GameManager instance = null;
public static GameManager Instance
{
    get { if (instance == null) { return null; } return instance; }
}
// SingletonSetup()은 위와 완전히 동일 ...

// DontDestroy_Menu.cs — null 체크조차 없는 버전
public static DontDestroy_Menu Instance { get { return instance; } }
```

### ③ 변경 후 코드

```csharp
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null) { Debug.Log(typeof(T).Name + " instance is null!!"); }
            return instance;
        }
    }

    /// <summary>씬이 바뀌어도 유지할지 여부. 씬에 종속된 매니저는 false로 override.</summary>
    protected virtual bool IsPersistent => true;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (IsPersistent) { DontDestroyOnLoad(this.gameObject); }
        }
        else if (instance != this) { Destroy(this.gameObject); }
    }
}
```

```csharp
public sealed class GameManager      : Singleton<GameManager> { /* 싱글턴 코드 0줄 */ }
public sealed class SoundManager     : Singleton<SoundManager> { /* 0줄 */ }
public sealed class DontDestroy_Menu : Singleton<DontDestroy_Menu> { /* 0줄 */ }

public sealed class DataManager : Singleton<DataManager>
{
    protected override void Awake()   // Awake에 추가 작업이 있는 유일한 케이스
    {
        base.Awake();
        LoadGameOptionData();
    }
}
```

### ④ 판단 근거

**제약을 `where T : MonoBehaviour`가 아니라 `where T : Singleton<T>`(CRTP)로 둔 이유:** 전자를 쓰면 `class Foo : Singleton<Bar>` 같은 실수가 컴파일을 통과하고, 런타임에 `Foo.Instance`가 영원히 null인 형태로 터진다. 원인 추적이 매우 어려운 부류의 버그다. CRTP 제약은 이걸 컴파일 단계에서 막는다.

**셋업을 `protected virtual void Awake()`에 둔 이유:** Unity에서 가장 흔한 함정이 파생 클래스가 무심코 `private void Awake()`를 정의해 베이스의 `Awake`를 가려버리고, 그 결과 **싱글턴이 조용히 초기화되지 않는** 것이다. `virtual`로 두면 그 상황에서 컴파일러가 "상속된 멤버를 숨깁니다" 경고를 내준다. 완벽한 방어는 아니지만, 아무 신호도 없는 것보다 훨씬 낫다.

**`Instance` null 로그를 통일한 근거:** 먼저 호출부를 전수 조사해 `Instance`를 null 체크하는 코드가 **한 곳도 없음**을 확인했다(전부 `X.Instance.메서드()` 직접 호출). 즉 어차피 NullReference가 날 자리이므로, 그 직전에 **어느 매니저가 없는지 이름과 함께** 알려주는 편이 진단에 유리하다고 판단했다.

### ⑤ 결과

- 싱글턴 보일러플레이트가 **4곳 → 1곳**. 이후 새 매니저는 `: Singleton<T>` 한 줄로 끝난다.
- `Instance`가 null일 때 **타입명이 찍히는 로그로 통일** — 기존에는 3가지 방식이 섞여 있었다.
- 중복 파괴 조건을 `else` → `else if (instance != this)`로 보강해, 같은 인스턴스에서 `Awake`가 두 번 도는 엣지 케이스에 자기 자신을 파괴하지 않도록 했다.
- 이 베이스에 추가한 `IsPersistent` 훅 덕분에, 씬에 종속된 `GameDirector`(Phase 1)도 **같은 싱글턴 메커니즘 하나로** 다룰 수 있게 됐다.

---

## Phase 3 — GameManager 캡슐화

### ① 문제 상황

게임 설정이 `public` 필드로 그대로 노출되어 있어, 설정 패널·모드 버튼·인게임 UI 등 **아무 데서나 직접 필드를 찔러 쓸 수 있었다.** 설정이 언제 어디서 바뀌는지 추적이 불가능했다.

### ② 이전 코드

```csharp
// GameManager.cs
public GameSetting gameSetting;   // ← 완전 노출

public GameSetting DefaultGameSetting()
{
    {                                    // ← 의미 없는 중첩 블록
        gameSetting = new GameSetting();
        gameSetting.gameMode = GameMode.OfflineMulti;
        gameSetting.pieceCount = 20;
        gameSetting.maxTurn = 20;
        gameSetting.waitingTime = 1f;
        return gameSetting;              // ← 반환값을 아무도 쓰지 않음
    }
}
```

```csharp
// 호출부 — 9곳에서 필드를 직접 수정
GameManager.Instance.gameSetting.gameMode = GameMode.AI;          // AIBattleMode
GameManager.Instance.gameSetting.pieceCount = option_value;       // GameSettingMenu
GameManager.Instance.gameSetting.maxTurn = 20;                    // GameSettingMenu
```

```csharp
// GameDirector.cs — 프로퍼티명이 타입명과 완전히 동일
private GameSetting GameSetting { get; set; }
...
if (GameSetting.gameMode == GameMode.AI)   // 타입인지 값인지 읽어서 구분이 안 됨
```

### ③ 변경 후 코드

```csharp
// GameManager.cs
[SerializeField]
private GameSetting gameSetting;

/// <summary>
/// 현재 게임 설정(읽기 전용). GameSetting은 struct라 이 프로퍼티는 복사본을 돌려주므로,
/// 값을 바꾸려면 반드시 아래 Set 메서드들을 거쳐야 한다.
/// </summary>
public GameSetting CurrentSetting => gameSetting;

public void SetGameMode(GameMode mode)     => gameSetting.gameMode = mode;
public void SetPieceCount(int count)       => gameSetting.pieceCount = count;
public void SetPieceCount_AI(int count)    => gameSetting.pieceCount_AI = count;
public void SetWaitingTime(float seconds)  => gameSetting.waitingTime = seconds;
public void SetMaxTurn(int turn)           => gameSetting.maxTurn = turn;

private void DefaultGameSetting()
{
    gameSetting = new GameSetting
    {
        gameMode = GameMode.OfflineMulti,
        pieceCount = 20,
        maxTurn = 20,
        waitingTime = 1f,
    };
}
```

```csharp
// 호출부
GameManager.Instance.SetGameMode(GameMode.AI);
GameManager.Instance.SetPieceCount(option_value);
```

```csharp
// GameDirector.cs
private GameSetting currentSetting;
...
// GameSetting은 struct라 여기서 값이 복사된다. 즉 이 시점 이후 GameManager 쪽 설정이
// 바뀌어도 진행 중인 판에는 반영되지 않는다(판 시작 시점의 설정으로 끝까지 진행).
currentSetting = GameManager.Instance.CurrentSetting;
```

### ④ 판단 근거

**struct라는 점이 이 설계의 핵심이다.** `GameSetting`이 struct이므로 `CurrentSetting` 프로퍼티는 **복사본**을 돌려준다. 따라서

```csharp
GameManager.Instance.CurrentSetting.pieceCount = 5;   // 컴파일 에러 CS1612
```

가 된다. 캡슐화가 "팀 규칙"이 아니라 **컴파일러가 강제하는 구조**가 되는 것이다. 클래스였다면 프로퍼티로 감싸도 내부 필드를 얼마든지 바꿀 수 있었다.

**`[SerializeField]`를 유지한 이유:** 기존에 `public` 필드라 Unity가 직렬화하고 있었다. 그냥 private으로만 바꾸면 직렬화가 끊겨 씬에 저장돼 있던 값이 사라지고 씬이 dirty 처리된다. 필드명을 그대로 두고 `[SerializeField]`를 붙이면 **기존 직렬화 데이터가 그대로 매핑**된다.

**`GameDirector`의 프로퍼티명을 바꾼 이유:** `private GameSetting GameSetting { get; set; }` 는 타입명과 프로퍼티명이 같아, 코드를 읽을 때 `GameSetting.gameMode`가 정적 멤버 접근인지 인스턴스 값 접근인지 구분되지 않았다. `currentSetting`으로 바꿔 해소했다.

### ⑤ 결과

- 외부에서 `.gameSetting`에 접근하는 코드 **0건** (전수 확인)
- 설정 변경 경로가 5개 메서드로 좁혀져, "언제 무엇이 바뀌는지" 추적 가능
- struct 복사 시점을 주석으로 명시해, **판 진행 중 설정 변경이 반영되지 않는 것이 의도된 동작**임을 코드에 남김
- `DefaultGameSetting()`의 죽은 반환값과 중첩 블록 정리

### ⑥ 계획 대비 달라진 점

작업 전 `GameSettingMenu`의 setter 4개(`SetPieceCount`, `SetPieceCount_AI`, `SetWaitingTime`, `SetMaxTurn`)가 **`GameSettingPanel.prefab`, `Tablet_UI.prefab`, `GameScene.unity` 3곳씩** 드롭다운 이벤트에 바인딩돼 있음을 확인했다. 그래서 **메서드명은 건드리지 않고 본문만** 교체하는 방식을 택했고, 그 결과 이 Phase는 **에디터 재바인딩 작업이 0건**이다. (이 확인 습관은 아래 Phase 0에서 크게 데인 뒤 생겼다.)

---

## Phase 0 — UI 패널 구조 통합

### ① 문제 상황

"패널을 보이기/숨기기"라는 **하나의 개념이 세 갈래로 흩어져** 있었다. 새 패널을 추가할 때 어떤 규칙을 따라야 하는지 알 수 없는 상태였다.

1. `Panel_Base` — `ShowPanel()`/`HidePanel()`, 타이틀 메뉴 3개 패널이 사용
2. `MenuButtonBase` — `Show()`/`Hide()`, 하단 내비게이션 버튼 전용
3. 나머지는 **클래스마다 제각각인 이름** — `OnClickResumeButton`, `OnClickExitButton`, `OnClickExit_no_Button`...
4. `Result_Panel`은 아예 표시/숨김 메서드가 없어 **호출부가 `SetActive`를 직접** 건드렸다

### ② 이전 코드

```csharp
// 베이스가 두 개, 시그니처도 다름
public class Panel_Base : MonoBehaviour
{
    public E_UI_Panel_Name panel_Name;
    public virtual void ShowPanel() { }
    public virtual void HidePanel() { }
}

public class MenuButtonBase : MonoBehaviour
{
    public virtual void Show() { }
    public virtual void Hide() { }
}
```

```csharp
// 같은 일을 하는데 이름이 전부 다름
public class OptionPanel : MonoBehaviour          // 어느 베이스도 상속 안 함
{
    public void ShowPanel() { gameObject.SetActive(true); }
    public void HidePanel() { gameObject.SetActive(false); }
}

public class ResumePanel : MonoBehaviour
{
    public void OnClickResumeButton()   { resumePanel.SetActive(true); }   // = Show
    public void OnClickContinueButton() { resumePanel.SetActive(false); }  // = Hide
}

public class ExitPanel : MonoBehaviour
{
    public void OnClickExitButton()      { exitPanel.SetActive(true); }    // = Show
    public void OnClickExit_no_Button()  { exitPanel.SetActive(false); }   // = Hide
}
```

```csharp
// GameDirector.cs — Result_Panel은 호출부가 직접 SetActive
result_Panel.gameObject.SetActive(true);
```

### ③ 변경 후 코드

```csharp
public class UIPanel : MonoBehaviour
{
    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
```

```csharp
public class Panel_Base : UIPanel          // Show/Hide는 상속받고 태그만 추가
{
    public E_UI_Panel_Name panel_Name;
}

public sealed class OptionPanel : UIPanel
{
    public override void Show() { gameObject.SetActive(true); }
    public override void Hide() { gameObject.SetActive(false); }
}

public sealed class ResumePanel : UIPanel
{
    public override void Show() { resumePanel.SetActive(true); }
    public override void Hide() { resumePanel.SetActive(false); }
}

public sealed class ExitButton : UIPanel { }        // 기본 동작으로 충분 → 본문 0줄
```

```csharp
// GameDirector.cs — 이제 다형적 호출
result_Panel.Show();
```

`MenuButtonBase`는 삭제하고 상속하던 버튼 4개를 `UIPanel`로 옮겼다. `MenuList`의 컬렉션 타입도 `List<MenuButtonBase>` → `List<UIPanel>`로 교체했다.

### ④ 판단 근거

`ExitButton`/`OptionButton`은 override 내용이 새 베이스의 기본 동작과 **완전히 동일**해서 override 자체를 지웠다. 반면 `ListButton`은 `Show()`/`Hide()`가 **빈 메서드**였는데, 이건 "리스트 토글 버튼 자신은 다른 버튼들과 함께 꺼지면 안 된다"는 **의도된 동작**이었다. 여기서 override를 지웠다면 기본 `SetActive`가 동작해 토글 버튼이 사라졌을 것이므로, 빈 override를 그대로 유지했다. **"같아 보이는 코드"와 "의도적으로 비어 있는 코드"를 구분하는 것이 이 작업의 실제 난이도였다.**

`ResumePanel`의 `OnClickReplayButton`/`OnClickSelectModeButton`처럼 표시/숨김이 아닌 **고유 동작** 메서드는 이름을 유지했다. 통일의 대상은 "보이기/숨기기"라는 개념이지, 모든 메서드가 아니다.

### ⑤ 결과

- 표시/숨김 API가 **3갈래 → 1개(`UIPanel.Show/Hide`)** 로 통일
- 베이스 클래스 2개 → 1개 (`MenuButtonBase` 삭제)
- `Result_Panel`도 다형성 체계에 편입, 호출부의 `SetActive` 직접 조작 제거

### ⑥ 계획 대비 달라진 점 — 두 가지 중요한 발견

**(1) "비대칭 버그"로 판단했던 것이 사실은 의도된 연출이었다.**

`Menu_Manager`의 세 전환 메서드 중 `Change_Start_to_ModeSelect()`만 공통 `ChangePanel()`을 거치지 않아, 처음엔 이걸 일관성 버그로 보고 계획에 "수정" 항목으로 넣었다. 그런데 구현 중 호출 경로를 추적해보니 다음 연출이었다.

```
StartPanel.Hide()  →  카메라 페이드아웃 + "MoveStart" 애니메이션 트리거
                   →  애니메이션 클립의 Animation Event
                   →  Camera_Animation_Event.Loading_UI_Show()
                   →  Tablet_Logic: 로딩 스피너 표시 → 1.5초 대기
                   →  ModeSelectPanel.Show()
```

즉 ModeSelect 패널은 **일부러 1.5초 뒤에** 나타나야 했다. 계획대로 `ChangePanel()`을 태웠다면 패널이 즉시 떠서 로딩 연출이 통째로 깨졌을 것이다. **전환 흐름은 그대로 두고 메서드명만 통일**하는 것으로 계획을 수정했다.

**(2) UnityEvent 바인딩을 놓쳐 실제로 버튼이 먹통이 됐다. → 아래 별도 항목**

---

## 사고 기록 — 메뉴 버튼 먹통 (Phase 0의 대가)

### ① 증상

Phase 0 이후 플레이 테스트에서 **콘솔 에러가 하나도 없는데** 옵션 버튼을 눌러도 옵션창이 열리지 않고, 인게임 일시정지 버튼도 반응하지 않았다.

### ② 원인

Unity의 `UnityEvent`(버튼 OnClick)는 대상 메서드를 **문자열로** 참조한다. Phase 0에서 메서드명을 `Show`/`Hide`로 바꿨지만 씬·프리팹에 저장된 문자열은 옛 이름 그대로였고, **대상 메서드가 없으면 UnityEvent는 예외 없이 조용히 아무것도 하지 않는다.** 컴파일러도 런타임도 잡아주지 않는 유일한 부류의 참조다.

```yaml
# TitleScene.unity — 코드에는 더 이상 존재하지 않는 메서드를 가리키고 있었다
- m_Target: {fileID: 1401899690}
  m_TargetAssemblyTypeName: ResumePanel, Assembly-CSharp
  m_MethodName: OnClickResumeButton      # 실제 코드: Show()
```

### ③ 조치

씬·프리팹의 해당 줄을 직접 수정했다(인스펙터에서 10곳을 찾는 것보다 정확하다).

| 파일 | 이전 | 이후 |
|---|---|---|
| TitleScene.unity (2곳) | `OnClickExitButton` | `Show` |
| TitleScene.unity | `OnClickExit_no_Button` | `Hide` |
| TitleScene.unity | `OnClickResumeButton` | `Show` |
| TitleScene.unity | `OnClickContinueButton` | `Hide` |
| TitleScene.unity | `ShowPanel` | `Show` |
| TitleScene.unity | `value: HidePanel` | `Hide` |
| OptionPanel.prefab | `HidePanel` | `Hide` |
| Tablet_UI.prefab (2곳) | `HidePanel`, `ShowPanel` | `Hide`, `Show` |

### ④ 왜 놓쳤는가 — 이게 이 항목의 핵심

Phase 0 당시 나는 **"재바인딩 필요한 곳은 6곳"** 이라고 정리했고, `OptionPanel.HidePanel`은 **"코드에서만 호출되므로 불필요"** 라고 단정했다. 둘 다 틀렸다. 실제로는 10곳이었다.

검색 범위에 두 가지가 빠져 있었다.

1. **`.prefab` 파일** — `.unity` 씬만 검색했다. 정작 `OptionPanel.prefab`에 바인딩이 있었다.
2. **프리팹 오버라이드 직렬화 형식** — 일반 형식은 `m_MethodName: X`지만, 프리팹 인스턴스의 오버라이드는 `propertyPath: ...m_MethodName` / `value: X` 형태로 저장된다. 완전히 다른 문자열이라 기존 검색 패턴에 걸리지 않았다.

사실 이 프로젝트에서 **같은 함정에 두 번 걸렸다.** 앞서 `Menu_Manager.All_HidePanel()`을 죽은 코드로 판단해 삭제하려다, 삭제 직전 재확인에서 GameScene의 오버라이드 형식 바인딩을 발견해 취소한 적이 있다. 그 교훈을 정작 Phase 0 체크리스트에는 적용하지 못했다.

### ⑤ 이후 적용한 규칙

Unity에서 public 메서드명을 바꾸거나 지우기 전에는 **반드시** 아래를 모두 검색한다.

```bash
grep -rn -E "(m_MethodName|value): <메서드명>$" Assets --include=*.unity --include=*.prefab
```

그리고 **가능하면 이름을 바꾸지 않고 본문만 교체한다.** Phase 3에서 `GameSettingMenu`의 드롭다운 핸들러 4개를 이 방식으로 처리해 재바인딩 0건으로 끝냈다.

---

## Phase 0.5 — 스크립트 폴더 구조 재편

### ① 문제 상황

폴더 분류에 **두 가지 기준이 한 depth에 섞여** 있었고, 폴더명 표기도 제각각(`Title Scene` / `GameScene` / `Camera Move` / `Canvas_Menu`)이었다. 새 스크립트를 어디에 둬야 할지 판단할 규칙이 없었다.

### ② 이전 구조

```
Assets/Scripts/
  AllScene/                    # 씬 기준 + 싱글턴과 UI가 뒤섞임
    GameManager.cs, DataManager.cs, SoundManager.cs, GameSetting.cs, OptionData.cs
    Menu List/, Exit Panel/, Help Panel/, Option Panel/
  GameScene/                   # 씬 기준
    Camera Move/, Game System/, Magnet System/, MemoryPool/, Player/, UI/
  Title Scene/                 # 씬 기준 (공백 포함)
    Camera/
    Canvas_Menu/               # 3단 중첩
      Start Panel/, Mode Select Panel/, Game Setting Panel/
    FadeEffect_UI.cs           # 실제로는 GameScene에서도 쓰는 공용 유틸인데 여기 있음
```

### ③ 변경 후 구조

```
Assets/Scripts/
  Core/       # 앱 전역 싱글턴·데이터 (GameManager, DataManager, SoundManager, DontDestroy_Menu, Singleton, GameSetting, OptionData)
  UI/         # 씬 공용 UI 프레임워크·상시 패널 (UIPanel, FadeEffect_UI, MenuBar/MenuList/버튼류, ExitPanel, HelpPanel, OptionPanel)
  MainMenu/   # 타이틀 메뉴 흐름 전체 (14개)
  Match/      # 인게임 대전 진행 + HUD + 인게임 카메라 (10개)
  Magnet/     # 자석 물리 시스템 (7개)
  AI/         # AI_FSM
```

추가로 전체 49개 스크립트에 **폴더와 1:1 대응하는 namespace**(`Assets.Scripts.<폴더명>`)를 부여했다.

### ④ 판단 근거

처음에는 기존 구조를 존중해 **씬 기준**(`TitleScene/`, `GameScene/`)으로 재편했다. 그러나 이 프로젝트는 씬이 2개뿐이고, 카메라 스크립트처럼 **두 씬에 각각 존재하지만 본질적으로 같은 역할**을 하는 코드가 서로 다른 폴더로 갈라지는 문제가 있었다. "어느 씬 소속인가"보다 "무슨 시스템인가"가 코드를 찾는 실제 기준이라고 판단해 **기능 기준으로 다시 재편**했다.

**모든 이동은 `git mv`로 `.cs`와 `.cs.meta`를 반드시 함께 옮겼다.** Unity는 스크립트를 `.meta`의 GUID로 참조하므로, `.meta`가 함께 가지 않으면 **씬·프리팹에 붙어 있던 모든 컴포넌트 연결이 끊긴다.** 폴더 자체의 `.meta`는 컴포넌트 참조와 무관하므로 재생성되도록 두었다.

### ⑤ 결과

- 최상위 폴더 **6개**, 전부 "기능" 하나의 기준으로 통일. 공백 포함 폴더명 제거.
- namespace가 폴더와 1:1 대응해 파일 위치와 논리적 소속이 일치
- 컴포넌트 참조 손실 **0건** (GUID 보존 확인)

### ⑥ 계획 대비 달라진 점

**namespace 도입 과정에서 이름 충돌이 발생했다.** `Camera/` 폴더의 `Character_FaceCam.cs`가 `UnityEngine.Camera` 타입을 필드로 쓰는데, 파일이 `namespace Assets.Scripts.Camera`에 들어가면서 `Camera`가 자기 네임스페이스명과 겹쳐 모호해졌다. 당시엔 `UnityEngine.Camera`로 풀네임 처리해 우회했다.

이후 폴더 구조 적정성을 재검토하면서 **`Camera/` 폴더 자체를 해체**해 근본 원인을 없앴다. 세 파일의 실제 사용처를 조사한 결과:

| 파일 | 실제 정체 | 유일한 사용처 |
|---|---|---|
| `CameraView.cs` | 인게임 쿼터뷰↔탑뷰 전환 | `GameDirector` (Match) |
| `Character_FaceCam.cs` | 이름과 달리 **플레이어 패널 초상화용** = HUD 부속 | `InGameUI_Manager` (Match) |
| `CameraMoving.cs` | `Start`/`Update`가 빈 껍데기 | **없음 (죽은 코드)** |

하나의 "카메라 시스템"이 아니라 이름만 보고 묶인 상태였다. 앞의 둘을 `Match/`로 옮기고 `CameraMoving.cs`는 삭제했다. 그 결과 우회용으로 넣었던 `UnityEngine.Camera` 풀네임도 평범한 `Camera`로 되돌릴 수 있었다. **폴더 구조 문제를 고치니 코드 문제가 함께 사라진 사례다.**

같이 발견한 죽은 코드 `GameOption.cs`(해상도 옵션을 만들다 만 스텁, 참조 0건)도 삭제했다. 삭제 전 코드 참조와 씬·프리팹 GUID 참조가 모두 0건임을 확인했다.

---

## 사전 작업 — Unity 6 이식 및 스크립트 인코딩 정리

### ① 문제 상황

프로젝트를 다시 열었을 때 모든 `.cs` 파일의 한글 주석이 깨져 보였다. 파일이 **CP949(EUC-KR)로 저장**되어 있었기 때문이다. 당시 한국어 Windows 환경의 Visual Studio가 시스템 기본 코드페이지로 저장하면서 생긴 문제로, UTF-8을 기대하는 현대 에디터(VS Code, Rider)에서는 전부 깨진다.

```csharp
// 실제로 이렇게 보였다
private List<Player> playerList = new List<Player>();   // ���� ���� �� �÷��̾� ����
```

### ② 조치

- Unity 2022.3 → 6000.5.3f1 업그레이드 (URP 17.5, TextMeshPro 등 재직렬화)
- 49개 스크립트를 CP949 → UTF-8로 변환. 어차피 전체 코드를 다시 볼 예정이었으므로 **기존 주석은 제거**하고, 이후 리팩토링 과정에서 "왜"를 설명하는 주석만 선별적으로 다시 넣기로 했다.

### ③ 결과

- 모든 스크립트가 UTF-8 (또는 순수 ASCII)로 정규화되어 인코딩 문제 재발 없음
- 이후 모든 리팩토링 작업의 깨끗한 출발점(`ae059ef`) 확보

---

## 이 프로젝트에서 얻은 교훈

**1. Unity의 문자열 참조는 컴파일러가 지켜주지 않는다.**
`UnityEvent`(버튼 OnClick, 드롭다운), Animation Event, `Invoke("메서드명")`은 모두 메서드를 문자열로 참조한다. 이름을 바꾸면 **에러 없이 조용히 죽는다.** 이 프로젝트에서 두 번 데였고, 결국 "이름을 바꾸지 않고 본문만 교체"를 기본 전략으로 삼게 됐다.

**2. 성능 최적화에서 동작이 바뀌면 그건 버그다.**
`FindObjectsOfType`을 자기등록 리스트로 바꿀 때, 이 API가 비활성 오브젝트를 제외한다는 점을 놓쳤다면 오브젝트 풀에서 잠자는 자석까지 물리 계산에 포함됐을 것이다. `OnEnable`/`OnDisable` 시점을 고른 것은 성능이 아니라 **동일성**을 위한 선택이었다.

**3. 계획은 실행하면서 검증해야 한다.**
"비대칭 구조 = 버그"라고 계획에 적었지만 실제로는 Animation Event로 짜인 의도된 연출이었고, "SerializeField로 교체"라고 적었지만 대부분 씬 간 참조라 불가능했다. **계획 대비 무엇이 왜 달라졌는지를 남기는 것**이 계획 자체보다 가치 있었다.

**4. 구조를 고치면 코드 문제가 따라서 사라진다.**
`Camera/` 폴더를 해체하자 namespace 충돌 우회 코드가 필요 없어졌고, 죽은 코드도 함께 드러났다. 응집도가 낮은 묶음은 그 자체로 여러 증상의 원인이었다.

---

## 2026-09-03 — MyAssets 폴더 재편에 따른 namespace 갱신

### ① 문제 상황

외부 에셋과 직접 만든 에셋이 `Assets/` 최상위에 뒤섞여 있어 관리가 어려웠다. 사용자가 Unity 에디터에서 직접 `Assets/MyAssets/`를 만들고 자체 제작 에셋(Scripts 포함)을 그 아래로 옮겼다. 그 결과 `Scripts/`의 실제 경로가 `Assets/Scripts/` → `Assets/MyAssets/Scripts/`로 바뀌었는데, Phase 0.5에서 폴더 기준으로 부여한 namespace(`Assets.Scripts.<폴더명>`)는 파일 위치와 어긋난 채로 남아 있었다.

### ② 이전 코드

```csharp
// Assets/MyAssets/Scripts/Core/GameManager.cs
using Assets.Scripts.UI;

namespace Assets.Scripts.Core
{
    public sealed class GameManager : Singleton<GameManager> { ... }
}
```

### ③ 변경 후 코드

```csharp
// Assets/MyAssets/Scripts/Core/GameManager.cs
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Core
{
    public sealed class GameManager : Singleton<GameManager> { ... }
}
```

50개 파일 전체의 `namespace Assets.Scripts.*` → `namespace Assets.MyAssets.Scripts.*`, `using Assets.Scripts.*` → `using Assets.MyAssets.Scripts.*`를 일괄 치환했다.

### ④ 판단 근거

작업 전 본문 코드에서 `Assets.Scripts.X.Y` 같은 완전정규화(FQN) 참조를 쓰는 곳이 있는지 먼저 확인했다 — 없었다. 모든 참조가 `namespace` 선언과 파일 최상단의 `using` 지시문 두 곳에만 몰려 있었기 때문에, **의미 분석 없이 `Assets.Scripts.` → `Assets.MyAssets.Scripts.` 문자열 치환만으로 안전하게** 끝낼 수 있다고 판단했다. (만약 본문에 FQN 참조가 있었다면 같은 치환이 의도치 않게 걸릴 수 있어 더 조심스럽게 접근해야 했다.)

폴더 이동 자체는 Unity 에디터에서 사용자가 직접 수행했다 — `.meta`가 자동으로 함께 이동해 GUID(씬·프리팹의 컴포넌트 참조)는 보존된다. 이 작업은 순수하게 **소스 코드 안의 namespace 문자열을 실제 폴더 위치와 다시 맞추는** 것이었다.

### ⑤ 결과

- 50개 파일 전부 갱신, 폴더 위치와 namespace가 다시 1:1 대응
- `dotnet build` 검증 — 우리 코드 오류/경고 0건 (Unity가 `Assembly-CSharp.csproj`의 `<Compile Include>` 경로도 이동 시점에 자동으로 `Assets\MyAssets\Scripts\...`로 갱신해둔 상태였음을 확인)
- 저장소 전체에 옛 `Assets.Scripts.` namespace/using 참조 0건 (재검색으로 재확인 — 최초 검색에서 `Assets.MyAssets.Scripts.`가 문자열 `MyAssets`에 `Assets`가 포함되어 있어 오탐이 났었고, 패턴을 `^namespace `/`using ` 접두어로 좁혀 재확인함)

### ⑥ 계획 대비 달라진 점

이 작업 자체가 계획에 없던 추가 작업이다. `MyAssets/`로의 에셋 재편은 별도로 진행된 정리 작업이었고, 그 부작용으로 namespace 정합성이 깨진 것을 바로 수정했다. Phase 0.5에서 "namespace를 폴더 기준으로 둔다"는 규칙을 세워뒀기 때문에, 이번처럼 폴더가 또 옮겨지면 같은 작업이 다시 필요하다는 점을 기록해둔다.

---

## 2026-09-03 — Phase 4: 오브젝트 풀 단순화

**관련 계획:** `wiggly-scribbling-wave.md` Phase 4

### ① 문제 상황

자석볼 오브젝트 풀이 **3단 래퍼**로 되어 있었다.

```
MemoryPool (직접 구현한 범용 풀)
  └ MagnetBallMemoryPool (MonoBehaviour 래퍼, 프리팹 참조 보유)
      └ MagnetBallSpawner (또 다른 래퍼, 메서드를 그대로 전달만 함)
```

그런데 정작 `GameDirector`는 이 계층을 존중하지 않고 **중간 레이어를 뚫고** 들어갔다. `MagnetBallSpawner`를 참조하면서도 실제로는 `GetComponent<MagnetBallMemoryPool>()`로 한 단계 밑을 직접 호출했다. 즉 래퍼가 캡슐화 역할을 전혀 못 하고 있었다.

### ② 이전 코드

```csharp
// MagnetBallSpawner.cs — 전달만 하는 껍데기 (27줄)
public sealed class MagnetBallSpawner : MonoBehaviour
{
    private MagnetBallMemoryPool magnetBallMemoryPool;

    private void Awake() { magnetBallMemoryPool = GetComponent<MagnetBallMemoryPool>(); }

    public void SpawnMagnetBall(Vector3 pos, Quaternion rot) => magnetBallMemoryPool.ActivateMagnetBall(pos, rot);
    public void DeactivateAllMagnetBall() => magnetBallMemoryPool.DeactivateAllMagnetBall();
}
```

```csharp
// MemoryPool.cs — 손으로 만든 풀. 빈 슬롯을 매번 선형 탐색한다 (126줄 중 발췌)
public class PoolItem
{
    public bool isActive;
    public GameObject gameObject;
}

public GameObject ActivatePoolItem()
{
    if (poolItemList == null) { Debug.Log("..."); return null; }

    PoolItem item = poolItemList.Find(poolItem => poolItem.isActive == false);
    if (item == null) { Debug.Log("ActivatePoolItem() : ActiveItem is null!!"); return null; }

    activeCount++;
    item.isActive = true;
    item.gameObject.SetActive(true);
    return item.gameObject;
}
```

```csharp
// GameDirector.cs — 래퍼를 뚫고 내부 자료구조까지 직접 만짐
if (isContact)
{
    magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
    FindAll(contactMagnetBall =>
        contactMagnetBall.gameObject.GetComponent<MagnetContact>().IsContact == true).
    ForEach(magnetBall =>
        magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().DeactivateMagnetBall(magnetBall.gameObject));
}

int contactMagnetBallCount = magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
    FindAll(magnet => magnet.gameObject.GetComponent<MagnetContact>().IsContact == true).Count;
```

### ③ 변경 후 코드

```csharp
// MagnetBallSpawner.cs — 단일 컴포넌트로 통합 (91줄)
public sealed class MagnetBallSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject magnetBallPrefab;

    private ObjectPool<GameObject> pool;

    /// <summary>
    /// 현재 판에 나와 있는 자석볼. ObjectPool<T>는 활성 객체를 열거하는 API가 없어서
    /// Get/Release 콜백에서 직접 관리한다.
    /// </summary>
    private readonly List<GameObject> activeMagnetBalls = new List<GameObject>();
    public IReadOnlyList<GameObject> ActiveMagnetBalls => activeMagnetBalls;

    private void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(magnetBallPrefab),
            actionOnGet: OnGetMagnetBall,
            actionOnRelease: OnReleaseMagnetBall,
            actionOnDestroy: magnetBall => Destroy(magnetBall));
    }

    private void OnGetMagnetBall(GameObject magnetBall)
    {
        activeMagnetBalls.Add(magnetBall);
        magnetBall.SetActive(true);
    }

    private void OnReleaseMagnetBall(GameObject magnetBall)
    {
        activeMagnetBalls.Remove(magnetBall);
        magnetBall.transform.position = Vector3.zero;
        magnetBall.SetActive(false);
    }

    public void DeactivateAllMagnetBall()
    {
        // Release 콜백이 activeMagnetBalls를 수정하므로 뒤에서부터 순회한다.
        for (int i = activeMagnetBalls.Count - 1; i >= 0; i--) { pool.Release(activeMagnetBalls[i]); }
    }
}
```

```csharp
// GameDirector.cs — 공개 API만 사용
if (isContact)
{
    // 반납하면 ActiveMagnetBalls가 줄어들므로 뒤에서부터 순회한다.
    IReadOnlyList<GameObject> activeMagnetBalls = magnetBallSpawner.ActiveMagnetBalls;
    for (int i = activeMagnetBalls.Count - 1; i >= 0; i--)
    {
        GameObject magnetBall = activeMagnetBalls[i];
        if (magnetBall.GetComponent<MagnetContact>().IsContact)
        {
            magnetBallSpawner.DeactivateMagnetBall(magnetBall);
        }
    }
}
```

### ④ 판단 근거

**`ObjectPool<T>`만으로는 부족했던 지점.** Unity 내장 `ObjectPool<T>`에는 **활성 객체를 열거하는 API가 없다**(`CountActive`로 개수만 알 수 있다). 그런데 `GameDirector`는 "접촉된 자석볼을 찾아 반납"해야 하므로 활성 목록 순회가 반드시 필요하다. 그래서 `activeMagnetBalls` 리스트를 직접 들고 있되, **`actionOnGet`/`actionOnRelease` 콜백 안에서 자동으로 갱신**되게 했다. 호출부가 등록/해제를 신경 쓸 필요가 없어 누락 위험이 없다.

그럼에도 `ObjectPool<T>`를 쓴 이유는, 빈 슬롯 선형 탐색·`isActive` 플래그 관리·null 가드 같은 **풀의 기계적인 부분을 전부 검증된 표준 구현에 맡길 수 있기 때문**이다. 남은 코드는 "이 게임에서 자석볼을 어떻게 다루는가"만 담게 됐다.

**동작 동일성을 위해 의도적으로 유지한 것들:**
- 반납 시 `position = Vector3.zero` → `SetActive(false)` **순서**를 원본 그대로 유지했다. 순서를 바꾸면 비활성화 전 이동으로 발생하던 `OnTriggerExit`(스폰 포인트 해제) 타이밍이 달라질 수 있다.
- `Get()` 후에 위치를 지정하는 순서도 원본과 동일하게 뒀다.
- 사전 생성은 `ObjectPool<T>`에 해당 API가 없어 **필요한 개수만큼 `Get()` 했다가 곧바로 `Release()`** 하는 표준 방식으로 구현했다. 원본의 "이미 충분하면 추가 생성 안 함" 로직은 `pool.CountAll`로 동일하게 재현했다.

**활성 목록만 순회해도 되는 이유.** 원본은 풀 전체(활성+비활성)를 순회하며 `IsContact`로 걸렀다. `MagnetContact`가 `OnEnable`/`OnDisable` 양쪽에서 `isContact = false`로 초기화하므로 **비활성 볼은 항상 `IsContact == false`** 다. 따라서 활성 목록만 도는 것과 결과가 동일하다.

### ⑤ 결과

- **3개 파일 228줄 → 1개 파일 91줄** (`MemoryPool.cs` 126줄 + `MagnetBallMemoryPool.cs` 75줄 + `MagnetBallSpawner.cs` 27줄 삭제/통합)
- `GameDirector`에서 `GetComponent<MagnetBallMemoryPool>()` 체인 **5곳 전부 제거** — 이제 `magnetBallSpawner`의 공개 API만 호출한다
- 내부 자료구조(`MemoryPool.PoolItem` 리스트)가 외부로 새어나가던 `GetPoolItemList()` 제거
- `dotnet build` 검증 — Assembly-CSharp 진단 0건

### ⑥ 계획 대비 달라진 점

**씬에 직렬화된 프리팹 참조 이관이 계획에 없던 실제 과제였다.** `magnetBallPrefab` 참조는 삭제 대상인 `MagnetBallMemoryPool` 컴포넌트가 들고 있었다. 컴포넌트를 지우면 이 참조도 함께 사라져, 인스펙터에서 수동 재지정이 필요하고 빠뜨리면 런타임에 `Instantiate(null)`로 터진다.

인스펙터 작업을 남기는 대신 **씬 YAML에서 직접 이관**했다. `MagnetBallSapwner` GameObject의 컴포넌트 3개 중 삭제 대상 블록을 걷어내고, 프리팹 참조를 살아남는 `MagnetBallSpawner` 블록으로 옮겼다.

```yaml
# 이관 후 — MagnetBallSpawner 컴포넌트가 프리팹 참조를 이어받음
--- !u!114 &760977142
MonoBehaviour:
  m_Script: {fileID: 11500000, guid: e89dbc88bdfbd39439a40a9fc0392731, type: 3}
  magnetBallPrefab: {fileID: 5492801683074353094, guid: 164d543588676324da803ee5501ada40,
    type: 3}
```

GameObject의 `m_Component` 목록에서 삭제된 컴포넌트 항목도 함께 제거해 **고아 참조(missing script)가 남지 않도록** 했다. 삭제 전 `MagnetBallMemoryPool`의 GUID가 GameScene 한 곳에서만 쓰이는지 전수 확인했고, 이관 후 해당 GUID와 고아 fileID가 씬에 0건임을 재검증했다.

그 결과 **이 Phase도 에디터 수동 작업 0건**으로 끝났다.

---

## 2026-09-03 — Phase 5: GameDirector 분리 (타이머 · 턴 상태머신 추출)

**관련 계획:** `wiggly-scribbling-wave.md` Phase 5

### ① 문제 상황

`GameDirector`가 성격이 다른 책임을 한 클래스에 쥐고 있었다. 그중 **타이머**와 **턴 상태**는 Unity API에 거의 의존하지 않는 순수 규칙인데도 MonoBehaviour 안에 묻혀 있어 단독으로 확인할 방법이 없었다.

특히 대기 시간이 `public float confirmTime`으로 **완전히 노출**되어 있었고, 물리 충돌 콜백(`MagnetContact`)이 이 필드를 외부에서 직접 증가시키고 있었다. 누가 언제 이 값을 바꾸는지 추적이 어려웠다.

### ② 이전 코드

```csharp
// GameDirector.cs — 타이머 상태가 public 필드로 노출
public float confirmTime;

private IEnumerator StartTimer()
{
    confirmTime = currentSetting.waitingTime;
    int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

    while (confirmTime > 0)
    {
        confirmTime -= Time.deltaTime;

        if (confirmTime >= 0)
            inGameUI_Manager.UpdateUI_WaitingTime_Text(confirmTime, playerList[player_index].playerName);
        else
            inGameUI_Manager.UpdateUI_WaitingTime_Text(0, playerList[player_index].playerName);

        yield return null;
    }
    // ... 턴 결과 처리 ...
}
```

```csharp
// MagnetContact.cs — 외부에서 필드를 직접 조작
GameDirector.Instance.confirmTime += (GameManager.Instance.CurrentSetting.waitingTime / 2);
```

```csharp
// GameDirector.cs — 턴 상태가 필드 3개 + 메서드로 흩어져 있음
private enum E_GameState { None, Player_1, Player_2, End }
private E_GameState gameState;
private E_GameState winPlayer;
private int turnCount;

private E_GameState ChangeTurn(E_GameState gameState)
    => gameState == E_GameState.Player_1 ? E_GameState.Player_2 : E_GameState.Player_1;

private void CurrentTurnPieceDecrease(E_GameState currTurn)
{
    if (currTurn == E_GameState.Player_1)      playerList[__PLAYER__1].PieceCount--;
    else if (currTurn == E_GameState.Player_2) playerList[__PLAYER__2].PieceCount--;
}

// "현재 턴 플레이어의 인덱스" 계산이 3곳에 똑같이 반복됨
int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

// 승패 확정
winPlayer = gameState;
gameState = E_GameState.End;

// 최대 턴 도달 판정
if (currentSetting.maxTurn == turnCount && gameState == E_GameState.Player_2) { isPlaying = false; }
```

### ③ 변경 후 코드

```csharp
// MatchTimer.cs (신규, 36줄) — Unity 의존 없는 순수 C# 클래스
public sealed class MatchTimer
{
    private float remainingTime;

    /// <summary>UI에 표시할 남은 시간. 음수로 내려가지 않는다.</summary>
    public float DisplayTime => Mathf.Max(remainingTime, 0f);
    public bool IsFinished => remainingTime <= 0f;

    public void Begin(float duration)     => remainingTime = duration;
    public void Tick(float deltaTime)     => remainingTime -= deltaTime;
    public void ExtendTime(float seconds) => remainingTime += seconds;
}
```

```csharp
// TurnStateMachine.cs (신규, 71줄) — 턴 진행 규칙 전담
public sealed class TurnStateMachine
{
    public E_GameState Current { get; private set; }
    public E_GameState WinPlayer { get; private set; }
    public int TurnCount { get; private set; }

    public bool IsPlayerTurn => Current == E_GameState.Player_1 || Current == E_GameState.Player_2;
    public int CurrentPlayerIndex => Current == E_GameState.Player_1 ? PLAYER_1_INDEX : PLAYER_2_INDEX;

    public void Reset() { Current = E_GameState.None; WinPlayer = E_GameState.None; TurnCount = 0; }
    public void BeginFirstTurn() => Current = E_GameState.Player_1;
    public void ChangeTurn() => Current = Current == E_GameState.Player_1 ? E_GameState.Player_2 : E_GameState.Player_1;
    public void IncreaseTurnCount() => TurnCount++;

    public void FinishWithCurrentPlayerAsWinner() { WinPlayer = Current; Current = E_GameState.End; }
    public void Stop() => Current = E_GameState.None;

    public bool IsMaxTurnReached(int maxTurn) => maxTurn == TurnCount && Current == E_GameState.Player_2;
}
```

```csharp
// GameDirector.cs — 규칙을 두 클래스에 위임하고 조율만 한다
private readonly MatchTimer matchTimer = new MatchTimer();
private readonly TurnStateMachine turnState = new TurnStateMachine();

/// <summary>자석볼 충돌처럼 결과가 아직 확정되지 않았을 때 대기 시간을 늘린다(MagnetContact에서 호출).</summary>
public void ExtendConfirmTime(float seconds) => matchTimer.ExtendTime(seconds);

private IEnumerator StartTimer()
{
    matchTimer.Begin(currentSetting.waitingTime);
    int player_index = turnState.CurrentPlayerIndex;

    while (!matchTimer.IsFinished)
    {
        matchTimer.Tick(Time.deltaTime);
        inGameUI_Manager.UpdateUI_WaitingTime_Text(matchTimer.DisplayTime, playerList[player_index].playerName);
        yield return null;
    }
    // ...
}

// 승패 · 턴 전환 · 조각 수 증감이 전부 상태머신 API를 거친다
if (turnState.IsMaxTurnReached(currentSetting.maxTurn)) { isPlaying = false; }
turnState.FinishWithCurrentPlayerAsWinner();
turnState.IncreaseTurnCount();
turnState.ChangeTurn();

private void CurrentTurnPieceDecrease()
{
    if (turnState.IsPlayerTurn == false) { return; }
    playerList[turnState.CurrentPlayerIndex].PieceCount--;
}
```

```csharp
// MagnetContact.cs — 필드 직접 조작 대신 의도가 드러나는 메서드 호출
GameDirector.Instance.ExtendConfirmTime(GameManager.Instance.CurrentSetting.waitingTime / 2);
```

### ④ 판단 근거

**두 클래스 모두 MonoBehaviour가 아닌 순수 C# 클래스로 만들었다.** MonoBehaviour로 만들면 씬에 컴포넌트를 붙여야 하고, 그러면 인스펙터 연결이라는 새 숙제가 생긴다(Phase 0에서 이런 종류의 누락으로 버튼이 먹통이 된 전례가 있다). 순수 클래스는 **씬 변경이 0건**이고 Unity 없이 단독 테스트가 가능하다. 프레임 진행은 소유자인 `GameDirector`가 `Tick(Time.deltaTime)`으로 넣어준다.

**동작 동일성을 위해 확인한 것들:**

- `DisplayTime => Mathf.Max(remainingTime, 0f)`는 원본의 `if (confirmTime >= 0) ... else 0` 분기와 **정확히 같은 값**을 낸다. 루프 조건도 `confirmTime > 0` ↔ `!IsFinished`(`remainingTime <= 0`)로 등가다.
- `CurrentTurnPieceDecrease`는 원본이 `Player_1`/`Player_2`일 때만 동작하고 `None`/`End`에서는 아무것도 하지 않았다. `IsPlayerTurn` 가드로 이 조건을 그대로 옮겼다.
- `IncreasePieceCount`의 2중 분기도 같은 이유로 `IsPlayerTurn` + `CurrentPlayerIndex` 한 줄로 합쳤다.

**부수적으로 정리된 중복.** "현재 턴 플레이어의 인덱스"를 구하는 삼항식이 3곳에 복사돼 있었는데 `CurrentPlayerIndex` 하나로 모였다. `__PLAYER__2` 상수는 쓰이지 않게 되어 삭제했다.

### ⑤ 결과

- `GameDirector` **432줄 → 402줄**. 빠진 규칙이 `MatchTimer`(36줄) + `TurnStateMachine`(71줄)으로 **이름을 갖고 독립**했다
- `public float confirmTime` 제거 — 외부에서 필드를 직접 쓰던 경로가 `ExtendConfirmTime()` 하나로 좁혀짐
- 턴 상태 필드 3개(`gameState`/`winPlayer`/`turnCount`)가 `turnState` 하나로 대체
- 승패 판정·최대 턴 도달 판정·턴 전환이 **이름 있는 메서드**가 되어, `if (maxTurn == turnCount && gameState == Player_2)` 같은 조건식을 매번 해석할 필요가 없어짐
- 씬/프리팹 변경 **0건**, 에디터 수동 작업 **0건**
- 컴파일 검증 오류 0개. 플레이 테스트로 타이머(자석볼 접촉 시 시간 연장 포함) 정상 동작 확인

### ⑥ 계획 대비 달라진 점 — 빌드 검증이 무의미했다는 사실 발견

이 Phase 도중 **그동안의 빌드 검증이 전부 무의미했다는 것을 발견했다.**

`MatchTimer.cs`를 `Assembly-CSharp.csproj`에 등록하지 않은 채 `GameDirector`가 그 타입을 참조하고 있었는데도 빌드가 "오류 0건"으로 통과했다. 정상이라면 `CS0246`이 나야 한다. 원인을 추적한 결과:

> `Assembly-CSharp`가 ProBuilder 프로젝트를 `ProjectReference`로 물고 있는데, **ProBuilder 패키지가 자체 버그(`ObjectPool<>` 이름 모호성, CS0104)로 컴파일에 실패**한다. 그러면 `Assembly-CSharp`는 아예 빌드 대상에서 제외되고 로그에 등장조차 하지 않는다. "오류 0건"은 **우리 코드가 컴파일된 적이 없다**는 뜻이었다.

Phase 1~4에서 보고한 검증이 모두 이 상태였다. 대체 수단으로 **독립 검증 프로젝트**를 만들었다.

- `Assets/MyAssets/Scripts/**/*.cs` 를 `<Compile>` 로 지정
- 참조는 `Assembly-CSharp.csproj`의 `<HintPath>` 281개(Unity 엔진 DLL) + `Library/ScriptAssemblies/*.dll` 중 `Assembly-CSharp*.dll`을 제외한 74개(TMPro·UnityEngine.UI 등 패키지 어셈블리)
- **`ProjectReference`는 넣지 않는다** — 실패하는 ProBuilder를 물지 않기 위해

이 방식으로 다시 검증하니 **오류 0개 / 경고 58개**(전부 `[SerializeField]` 필드의 CS0649로, Unity가 직렬화로 값을 넣어주므로 정상)가 나왔다. 이 컴파일에는 스크립트 50개 전체가 들어가므로 **Phase 1~4의 코드도 이때 함께 검증됐다.**

교훈은 "검증 수단 자체가 실제로 동작하는지 확인해야 한다"는 것이다. 통과 신호가 **통과했기 때문에 나온 것인지, 아예 검사하지 않아서 나온 것인지**를 구분하지 않으면 검증은 없는 것과 같다.

---

## 2026-09-03 — 승패 판정 버그 수정 (2020년 출시판부터 있던 결함)

### ① 문제 상황

Phase 5 이후 플레이 테스트에서 발견. **최대 턴에 도달해 게임이 끝나면 조각 수와 무관하게 항상 AI(Player_2)가 승자로 표시**됐다.

먼저 이 버그가 리팩토링으로 생긴 것인지 확인했다. `ae059ef`(리팩토링 이전, 2020년 출시판 코드)에 동일한 로직이 있었다 — **출시 당시부터 존재하던 결함**이고, 리팩토링은 동작을 그대로 보존했을 뿐이다.

### ② 이전 코드

```csharp
// 조각이 0이 된 사람이 있으면 종료
if (playerList.Find(player => player.PieceCount <= 0) != null)
{
    isPlaying = false;
}

// 최대 턴에 도달해도 종료
if (currentSetting.maxTurn < __TURN_INFINITY__)
{
    if (currentSetting.maxTurn == turnCount && gameState == E_GameState.Player_2)
    {
        isPlaying = false;
    }
}

if (isPlaying == false)
{
    winPlayer = gameState;          // ← 승자 = 그냥 "현재 턴 플레이어"
    gameState = E_GameState.End;
    GameFSM();
}
```

두 종료 조건이 하나의 `isPlaying` 플래그로 합쳐진 뒤, 승자를 **무조건 현재 턴 플레이어**로 정하고 있었다. 조각 수 비교가 코드에 아예 없다.

### ③ 변경 후 코드

```csharp
bool someoneEmptiedPieces = playerList.Find(player => player.PieceCount <= 0) != null;

bool maxTurnReached = currentSetting.maxTurn < __TURN_INFINITY__
                      && turnState.IsMaxTurnReached(currentSetting.maxTurn);

// 조각을 다 털어낸 사람이 승리한다. 조각 수는 자기 턴에만 변하므로(놓으면 -1, 붙으면 +N)
// 0이 된 사람은 항상 현재 턴 플레이어다.
if (someoneEmptiedPieces)
{
    isPlaying = false;
    turnState.FinishWith(turnState.Current);
    GameFSM();
}
// 최대 턴까지 아무도 못 털어냈으면 남은 조각이 더 적은 쪽이 승리한다.
else if (maxTurnReached)
{
    isPlaying = false;
    turnState.FinishWith(DecideWinnerByFewestPieces());
    GameFSM();
}
else { /* 턴 전환 */ }
```

```csharp
/// <summary>
/// 최대 턴까지 승부가 나지 않았을 때의 승자. 남은 조각이 더 적은 쪽이 이기고, 같으면 무승부다.
/// </summary>
private E_GameState DecideWinnerByFewestPieces()
{
    int player1Pieces = playerList[__PLAYER__1].PieceCount;
    int player2Pieces = playerList[__PLAYER__2].PieceCount;

    if (player1Pieces == player2Pieces) { return E_GameState.None; }

    return player1Pieces < player2Pieces ? E_GameState.Player_1 : E_GameState.Player_2;
}

/// <summary>결과 화면에 표시할 승자 이름. AI 모드에서 Player_2는 "AI"로 보여준다.</summary>
private string GetWinnerDisplayName()
{
    if (turnState.WinPlayer == E_GameState.None) { return "DRAW"; }
    if (currentSetting.gameMode == GameMode.AI && turnState.WinPlayer == E_GameState.Player_2) { return "AI"; }
    return turnState.WinPlayer.ToString();
}
```

`TurnStateMachine`도 승자를 외부에서 지정할 수 있게 바꿨다.

```csharp
// 이전 — 승자를 항상 현재 턴 플레이어로 고정
public void FinishWithCurrentPlayerAsWinner() { WinPlayer = Current; Current = E_GameState.End; }

// 이후 — 승자 판정은 호출부가 하고, 상태머신은 확정만 한다
public void FinishWith(E_GameState winner) { WinPlayer = winner; Current = E_GameState.End; }
```

`EndBattle()`의 3중 분기 결과 표시도 `GetWinnerDisplayName()` 한 줄로 정리됐다.

### ④ 판단 근거

**게임 규칙(사용자 확인):** 번갈아 자석을 놓고(놓을 때마다 조각 -1), 대기 시간 동안 부딪힌 자석 개수가 자기 조각에 추가된다(+N). **최대 턴 전에 자기 조각이 0이 되면 그 사람이 승리**하고, **최대 턴까지 아무도 0이 못 되면 남은 조각이 더 적은 사람이 승리**한다. 즉 조각을 털어내는 것이 목표다.

**두 증상 중 하나는 사실 정상 동작이었다.** "AI 조각이 0이 되자 AI 승리"도 함께 보고됐는데, 분석해보니 이건 규칙대로다. 조각 수는 **자기 턴에만** 변하므로(놓기 -1, 흡수 +N), 0이 되는 사람은 필연적으로 그 시점의 현재 턴 플레이어다. 따라서 기존의 `winPlayer = gameState`가 이 경우에는 **우연히** 규칙과 일치했다. 이 관계를 코드 주석으로 남겨, 나중에 읽는 사람이 `FinishWith(turnState.Current)`를 보고 의아해하지 않도록 했다.

**진짜 결함은 최대 턴 경로였다.** `IsMaxTurnReached`는 `Current == Player_2`일 때만 참이므로, 이 경로로 끝나면 승자가 **항상 Player_2로 고정**된다. AI 모드에서는 무조건 AI 승리였다.

**무승부 처리는 규칙에 없어 직접 정했다.** 최대 턴에서 양쪽 조각 수가 같을 수 있는데 규칙에 언급이 없었다. 어느 한쪽을 임의로 고르는 대신 `E_GameState.None`을 승자로 두고 결과 화면에 `DRAW`로 표시하도록 했다.

### ⑤ 결과

- 최대 턴 종료 시 **조각 수를 실제로 비교**해 승자를 정한다
- 무승부가 표현 가능해졌다(`WINNER - DRAW`)
- 승자 판정 책임이 분리됐다 — `TurnStateMachine`은 "확정"만 하고, "누가 이겼는가"는 조각 수를 아는 `GameDirector`가 판단한다
- `EndBattle()`의 중첩 분기가 한 줄로 축소
- 컴파일 검증 오류 0개

### ⑥ 계획 대비 달라진 점

리팩토링 계획에 없던 **기능 버그**였다. Phase 5에서 턴 상태머신을 추출하지 않았다면 발견하기 어려웠을 가능성이 높다. `winPlayer = gameState`라는 한 줄은 그 자체로는 이상해 보이지 않지만, 이를 `FinishWithCurrentPlayerAsWinner()`라는 **이름 있는 메서드로 만드는 순간 "현재 턴 플레이어를 승자로"라는 규칙이 문장으로 드러났고**, 그게 실제 게임 규칙과 다르다는 점이 눈에 띄었다.

리팩토링의 목적은 동작을 바꾸지 않는 것이지만, **의도를 이름으로 드러내는 과정에서 원래 있던 결함이 노출되는** 부수 효과가 있다는 것을 보여주는 사례다.

---

<!-- 이후 작업은 이 아래에 최신순으로 추가 -->
