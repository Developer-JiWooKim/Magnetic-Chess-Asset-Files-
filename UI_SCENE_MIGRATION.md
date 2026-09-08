# 씬 재구성 절차

코드는 전부 끝났다. 이 문서는 Unity 에디터에서 손으로 해야 하는 일만 담는다.
순서대로 하면 되고, **각 절이 끝날 때마다 저장**한다.

기준: `UI_REFACTORING_PLAN.md`의 목표 구조.
코드 쪽 변경 내역은 `REFACTORING_LOG.md`에 있다.

> **처음 열면 Missing Script와 빈 참조가 많이 뜬다. 정상이다.**
> 스크립트 9개를 지웠고 직렬화 필드 이름도 바뀌었다. 이 문서가 그것들을 하나씩 없앤다.

---

## 0. 시작 전

1. 브랜치를 확인한다 (`unity6-upgrade`). 작업 전 커밋해 두면 되돌리기 쉽다.
2. Unity를 켜고 컴파일이 끝날 때까지 기다린다. **콘솔에 컴파일 오류가 없어야** 다음으로 간다.
   (오류가 있으면 여기서 멈춘다 — 코드 문제다.)

---

## 1. 사라진 스크립트 정리

지운 스크립트가 붙어 있던 오브젝트에는 **Missing Script 컴포넌트가 남는다.**
이걸 안 치우면 3절에서 프리팹을 만들 때 이렇게 막힌다:

> You are trying to save a Prefab with a missing script.
> Please change the script or remove it from the GameObject 'Exit Button'.

**한 번에 치우는 법**

`Tools > Magnetic Chess > Remove Missing Scripts (MyAssets 전체)`
— MyAssets 아래의 모든 씬과 프리팹을 훑어 제거하고 저장한다. 콘솔에 어디를 고쳤는지 찍는다.

계층에서 고른 것만 훑으려면
`Tools > Magnetic Chess > Remove Missing Scripts (계층에서 선택한 오브젝트)`.
프리팹으로 만들기 직전에 그 오브젝트만 확인할 때 쓴다.

> 프리팹 **인스턴스**의 컴포넌트는 인스턴스에서 지울 수 없다(Unity 제약).
> 그런 것이 있으면 도구가 건너뛰면서 어느 원본 프리팹을 고쳐야 하는지 콘솔에 알려 준다.
> 그 프리팹을 열어 같은 메뉴를 다시 돌리면 된다.

하나씩 지우려면 인스펙터에서 `Missing (Mono Script)` 헤더 우클릭 → Remove Component.

**어디에 남아 있나**

| 지운 스크립트 | 붙어 있던 곳 |
|---|---|
| `DontDestroyMenu` | TitleScene / `Canvas_DontDestroyMenu` |
| `MenuManager` | TitleScene / `Canvas_Menu`, GameScene / `Menu_Canvas` |
| `AddResumeAction` | GameScene / `Menu_Canvas` |
| `TabletLogic` | `Tablet_UI` 프리팹 루트 |
| `PanelBase` | 추상 클래스, 붙은 곳 없음 |
| `ListButton` | TitleScene / `Canvas_DontDestroyMenu/MenuBar/Menu List/List Button` |
| `OptionButton` | 〃 `/Option Button` |
| `ExitButton` | 〃 `/Exit Button` |
| `ResumeButton` | 〃 `/Resume Button` |

---

## 2. S0 — 태블릿 복제 UI 제거

1. TitleScene 계층 **루트**의 `Tablet_UI` 오브젝트를 **삭제**한다.
   World Space 캔버스 프리팹 인스턴스다. `Meshes/Tablet_Window` 메시는 **남긴다** — 화면이 꺼진 소품.
2. `Assets/MyAssets/Prefabs/UI_Panel/Tablet_UI.prefab` 을 **삭제**한다.
   나중에 World Space로 다시 넣을 때는 처음부터 새로 짓는다 (계획서 4절).

> 애니메이션 이벤트 이름을 `LoadingUIShow` → `OnCameraArrivedAtMenu` 로 바꿔 두었다
> (`Animation/Camera Animations/CameraMoving.anim`). 클립은 이미 수정돼 있으니 손댈 것 없다.

---

## 3. `CommonMenu` 프리팹 만들기

두 씬이 똑같이 쓸 메뉴 묶음이다. TitleScene의 `Canvas_DontDestroyMenu` 안에 있는 것을 그대로 쓴다.

1. `Canvas_DontDestroyMenu` 아래에 빈 오브젝트 `CommonMenu` 를 만든다
   (RectTransform, 앵커는 stretch 전체).
2. 아래 오브젝트들을 `CommonMenu` 안으로 **옮긴다**:
   - `Help Button`
   - `MenuBar` — `Background`, `Menu List`(→ Exit / Resume / List / Option Button) 포함
   - `Help Panel`
   - `Exit Panel`
   - `Option Panel` (프리팹 인스턴스)

3. `CommonMenu` 오브젝트에 **`CommonMenu` 컴포넌트**를 붙이고 채운다.

   | 필드 | 넣을 것 |
   |---|---|
   | `_menuBar` | `MenuBar` 의 MenuBar |
   | `_menuList` | `MenuBar/Menu List` 의 MenuList |
   | `_helpButton` | `Help Button` 의 Button |
   | `_optionButton` | `Menu List/Option Button` 의 Button |
   | `_exitButton` | `Menu List/Exit Button` 의 Button |
   | `_resumeButton` | `Menu List/Resume Button` 의 Button |
   | `_helpPanel` | `Help Panel` 의 HelpPanel |
   | `_optionPanel` | `Option Panel` 의 OptionPanel |
   | `_exitPanel` | `Exit Panel` 의 ExitPanel |

4. `MenuBar` 컴포넌트를 채운다.

   | 필드 | 값 |
   |---|---|
   | `_background` | `MenuBar/Background` 의 RectTransform |
   | `_menu` | `MenuBar/Menu List` 의 MenuList |
   | `_listButton` | `Menu List/List Button` 의 Button |
   | `_sizeUpSpeed` | 1000 |
   | `_collapsedWidth` | 130 |
   | `_expandedWidth` | **365** — 타이틀 기준. 게임 씬 인스턴스에서 470으로 덮는다 |

5. `Menu List` 의 `MenuList` 컴포넌트를 채운다.

   | 필드 | 값 |
   |---|---|
   | `_items` | `Exit Button`, `Resume Button`, `Option Button` **3개**. `List Button`은 넣지 않는다 — 목록을 여는 버튼 자신이다 |
   | `_spacing` | **-80** — 게임 씬 인스턴스에서 20으로 덮는다 |

6. `Help Panel` 의 `HelpPanel` 컴포넌트를 채운다.

   | 필드 | 넣을 것 |
   |---|---|
   | `_root` | `Help Panel` 자신 |
   | `_rulePage` | `Help Panel/Rule Page Background/Rule` |
   | `_descriptionPage` | `Help Panel/Rule Page Background/Description` |
   | `_closeButton` | `Rule Page Background/Close Button` |
   | `_toDescriptionButton` | `Rule/Right Move Butoon` |
   | `_toRuleButton` | `Description/Left Move Button` |

7. `Exit Panel` 의 `ExitPanel` 컴포넌트를 채운다.

   | 필드 | 넣을 것 |
   |---|---|
   | `_exitPanel` | `Exit Panel` 자신 (기존 값이면 그대로) |
   | `_yesButton` | `Exit Dialogue/Yse Button` |
   | `_noButton` | `Exit Dialogue/No Button` |

8. `Option Panel` **프리팹을 열어** `OptionPanel` 컴포넌트를 채운다 (두 씬에 함께 반영된다).

   | 필드 | 넣을 것 |
   |---|---|
   | `_root` | 옵션 창의 최상위 오브젝트 |
   | `_sliderBGM` / `_sliderSFX` | 기존 값 그대로 |
   | `_saveButton` | 저장 버튼 |

   해상도 드롭다운의 `SetResolution` 바인딩은 **가리키는 메소드가 없다** — 이전부터 죽어 있었다.
   쓰지 않는다면 지운다.

9. `CommonMenu` 오브젝트를 `Assets/MyAssets/Prefabs/UI_Panel/CommonMenu.prefab` 으로 끌어 **프리팹으로 만든다**.

---

## 4. S4 — TitleScene 재구성

### 4-1. 캔버스 하나로 합치기

1. `Canvas_Menu` 의 이름을 **`TitleUI`** 로 바꾼다.
2. `CommonMenu` 인스턴스를 `TitleUI` 아래로 옮긴다.
3. `Canvas_DontDestroyMenu` 에 남은 것을 처리한다.
   - `Loading Window` (→ `Percent/Percent Text`, `Backgound`) → `TitleUI` 아래로 **옮긴다**
   - `Resume Panel` → 대전 씬 것이다. `Assets/MyAssets/Prefabs/UI_Panel/ResumePanel.prefab` 으로
     **프리팹화한 뒤 TitleScene에서 삭제**한다.
4. 텅 빈 `Canvas_DontDestroyMenu` 오브젝트를 **삭제**한다.

> 이것으로 `DontDestroyOnLoad` UI가 사라진다.
> 이제 씬을 넘어 사는 것은 `GameManager` · `DataManager` · `SoundManager` 셋뿐이다.

### 4-2. 페이드 오브젝트 만들기

`TitleUI` 아래 **가장 마지막 자식**으로 `Scene Fade` 를 만든다.

- `Image` — 검정, 알파 1, 화면 전체 stretch, Raycast Target 켬
- `CanvasGroup` 추가

처음에 켜져 있든 꺼져 있든 상관없다. `SceneFadeIn` 이 Awake에서 켜고 Start에서 걷는다.

### 4-3. `TitleUIController` 붙이기

`TitleUI` 오브젝트에 컴포넌트 **3개**를 붙인다:
`TitleUIController`, `LoadingScreen`, `SceneFadeIn`.

**`TitleUIController`**

| 필드 | 넣을 것 |
|---|---|
| `_startPanel` | `TitleUI/Start Panel` |
| `_modeSelectPanel` | `TitleUI/Mode Select Panel` |
| `_gameSettingPanel` | `TitleUI/GameSettingPanel` |
| `_commonMenu` | `TitleUI/CommonMenu` |
| `_loadingScreen` | `TitleUI` 자신의 LoadingScreen |
| `_cameraAnimator` | `MainCamera` 의 Animator |

**`LoadingScreen`**

| 필드 | 넣을 것 |
|---|---|
| `_root` | `TitleUI/Loading Window` |
| `_canvasGroup` | `Loading Window` 의 CanvasGroup |
| `_percent` | `Loading Window/Percent` |
| `_percentText` | `Percent/Percent Text` |
| `_fadeInTime` | 1.5 |

**`SceneFadeIn`**

| 필드 | 넣을 것 |
|---|---|
| `_root` | `TitleUI/Scene Fade` |
| `_canvasGroup` | `Scene Fade` 의 CanvasGroup |
| `_fadeOutTime` | 1 |

> `LoadingScreen` 과 `SceneFadeIn` 은 **자기가 켜고 끄는 오브젝트 위에 두면 안 된다.**
> 꺼진 오브젝트에서는 Awake가 돌지 않는다. 그래서 캔버스에 붙이고 자식을 가리키게 한다.

### 4-4. 패널 참조 채우기

**`Start Panel` 의 `StartPanel`**

| 필드 | 넣을 것 |
|---|---|
| `_canvasGroup` | 자신의 CanvasGroup |
| `_startButton` | `Start Panel/Start Button` |

`_animatorCamera` 필드는 없어졌다 — 카메라는 이제 컨트롤러가 움직인다.

**`Mode Select Panel` 프리팹의 `ModeSelectPanel`**

| 필드 | 넣을 것 |
|---|---|
| `_canvasGroup` | 자신의 CanvasGroup |
| `_modeButtons` | 모드 버튼들의 부모 (ModeButtons 컴포넌트가 붙은 오브젝트) |

**`GameSettingPanel` 프리팹의 `GameSettingPanel`**

| 필드 | 넣을 것 |
|---|---|
| `_pieceCountAIOption` | 기존 값 그대로 |
| `_canvasGroup` | 자신의 CanvasGroup |
| `_playButton` | 시작(Game Play) 버튼 |
| `_backButton` | 뒤로 버튼 |

`_fadeWindow`, `_animatorCamera` 필드는 없어졌다.

**`MainCamera` 의 `CameraAnimationEvent`**

| 필드 | 넣을 것 |
|---|---|
| `_titleUIController` | `TitleUI` 의 TitleUIController |

---

## 5. S3 — GameScene 재구성

### 5-1. 지울 것

`Menu_Canvas` 아래에서 **삭제**한다.

- `GameSettingPanel` (프리팹 인스턴스)
- `Mode Select Panel` (프리팹 인스턴스)

> 판을 다시 시작하는 방법이 "씬 다시 읽기"로 바뀌었고, 모드를 바꾸려면 타이틀로 돌아간다.
> 그래서 대전 씬이 설정 패널을 다시 띄울 이유가 없어졌다.
> `GameSettingPanel.OnClickGamePlayButtonGameScene` 이 사라진 것도 같은 이유다.

### 5-2. 더할 것

`Menu_Canvas` 의 이름을 **`MatchUI`** 로 바꾸고 아래에 넣는다.

- `CommonMenu` 프리팹 인스턴스
  - `MenuBar` 의 `_expandedWidth` 를 **470** 으로 오버라이드
  - `Menu List` 의 `_spacing` 을 **20** 으로 오버라이드
- `ResumePanel` 프리팹 인스턴스 (4-1에서 만든 것)
- `Scene Fade` (4-2와 같은 구성으로 새로 만든다)
- `Loading Window` — TitleScene 것을 프리팹으로 만들어 함께 쓰거나,
  같은 구성(CanvasGroup + `Percent/Percent Text`)으로 하나 만든다

### 5-3. `MatchUIController` 붙이기

`MatchUI` 오브젝트에 `MatchUIController`, `LoadingScreen`, `SceneFadeIn` 을 붙인다.
`LoadingScreen` · `SceneFadeIn` 은 4-3과 같은 방식으로 채운다.

**`MatchUIController`**

| 필드 | 넣을 것 |
|---|---|
| `_resultPanel` | `MatchUI/ResultPanel` |
| `_resumePanel` | `MatchUI/Resume Panel` |
| `_inGameUIManager` | `InGameUI_Canvas` 의 InGameUIManager |
| `_commonMenu` | `MatchUI/CommonMenu` |
| `_loadingScreen` | `MatchUI` 자신의 LoadingScreen |
| `_gameDirector` | `GameDirector` |
| `_startButton` | `MatchUI/StartButton` |
| `_background` | `MatchUI/Background` |

**`ResumePanel` 프리팹**

| 필드 | 넣을 것 |
|---|---|
| `_resumePanel` | 대화상자 루트 |
| `_replayButton` | `Resume Dialogue/Buttons/Replay Button` |
| `_selectModeButton` | 〃 `/Select Mode Button` |
| `_continueButton` | 〃 `/Continue Button` ← **새 필드** |

**`ResultPanel`** — `_replayButton` · `_selectModeButton` · `_quitButton` 은 그대로.
런타임에 `ResumePanel` · `ExitPanel` 을 찾던 코드가 사라졌으므로 더 채울 것은 없다.

---

## 6. S6 — 남은 인스펙터 바인딩 지우기

버튼은 이제 전부 코드로 연결된다. 인스펙터에 남은 OnClick 목록은
**대부분 이미 죽었고**(메소드가 private이 되었거나 사라졌다),
**일부는 살아 있어 같은 일을 두 번 한다.**

두 번 실행되는 것 — 반드시 지운다.

| 어디 | 남은 호출 |
|---|---|
| GameScene `MatchUI/StartButton` | `GameDirector.GamePlay` |
| TitleScene 메뉴 버튼들 | `HelpPanel.Show` / `OptionPanel.Show` / `ExitPanel.Show` / `ResumePanel.Show` |
| 여기저기 | `GameObject.SetActive` |

**한 번에 지우는 법**

1. 계층에서 대상(또는 캔버스 루트)을 고른다.
2. `Tools > Magnetic Chess > Clear UnityEvent Bindings (계층에서 선택한 오브젝트)`
3. 프리팹은 프로젝트 창에서 골라 `Clear UnityEvent Bindings (선택한 에셋)`
   — `GameSettingPanel.prefab`, `Mode Select Panel.prefab`, `OptionPanel.prefab`, `CommonMenu.prefab`, `ResumePanel.prefab`

---

## 7. 검증

| # | 할 일 | 통과 기준 |
|---|---|---|
| 1 | Unity 콘솔 | 컴파일 오류 0 |
| 2 | `Tools > Magnetic Chess > Validate UnityEvent Bindings` | 끊어진 바인딩 0 |
| 3 | `Tools > Magnetic Chess > Validate No Inspector Bindings` | **남은 바인딩 0** (이번에 추가한 검사) |
| 4 | `Tools > Magnetic Chess > Validate Prefab Overrides` | 고아 오버라이드 0 |
| 5 | 플레이 | 아래 목록 |

플레이 확인 목록

- [ ] 타이틀에서 Start → 카메라 이동 → 모드 선택이 뜬다
- [ ] 오프라인 2인 → 설정 패널 → Game Play → 로딩 → 대전 씬
- [ ] 대전 씬에서 Start 버튼 → 판 시작
- [ ] 메뉴 막대 펼침/접힘, 도움말 · 옵션 · 종료 대화상자
- [ ] 도움말의 좌/우 버튼으로 규칙 ↔ 설명 넘김
- [ ] 대전 중 메뉴 → 이어하기 → Replay → **판 재시작** (여기서 씬이 다시 읽힌다)
- [ ] 재시작한 판을 **한 번 더** 재시작 (2회차 — 구독 누수가 있으면 여기서 드러난다)
- [ ] 판 종료 → 결과 화면 → Replay / Mode Select / Quit
- [ ] Mode Select → 타이틀로 돌아가되 **모드 선택 화면부터** 시작
- [ ] AI 모드에서 설정 패널에 AI 조각 수 항목이 뜬다
- [ ] 옵션에서 볼륨을 바꾸고 저장 → 다시 열었을 때 유지
- [ ] 타이틀 BGM → 씬 전환 BGM → 대전 BGM 순으로 바뀐다
