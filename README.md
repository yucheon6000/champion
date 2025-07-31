# LLM-Game-Maker: Behavior Tree 기반 2D 게임 엔진

## 개요

이 프로젝트는 **JSON 기반**으로 2D 게임을 정의하고, **Behavior Tree(BT)** 시스템을 통해 엔티티의 동작을 유연하게 제어할 수 있는 게임 엔진입니다. 프로그래머, 디자이너, AI 모두가 협업할 수 있도록 설계되었습니다.

### 시연 영상 (25년 7월 30일 업데이트)
[![시연영상](https://img.youtube.com/vi/i-PmtaKsXmA/0.jpg)](https://www.youtube.com/watch?v=i-PmtaKsXmA)

- LLM-BT 기반 게임 생성
- 자동 스프라이트 생성

## 주요 구조

### 1. JSON 기반 게임 정의

- 게임 전체는 하나의 JSON 파일로 정의됩니다.
- 주요 키:
  - `globalVariables`: 전역 변수 (접두사 `g_` 필수)
  - `controllers`: 입력 장치 정의
  - `presets`: 엔티티 템플릿(플레이어, 적, 아이템 등)
  - `scenes`: 각 레벨의 엔티티 배치

#### 예시

```json
{
  "globalVariables": {
    "g_i_lives": 3,
    "g_f_gameSpeed": 1.0
  },
  "controllers": [
    { "type": "Controller2D", "id": "Movement" },
    { "type": "ControllerButton", "id": "Jump", "keyCode": "Space" }
  ],
  "presets": {
    "player": {
      "tags": ["Player"],
      "variables": { "i_lives": 3, "f_moveSpeed": 5.0 },
      "behaviorTree": { ... }
    }
  },
  "scenes": [
    {
      "id": "Level_1",
      "entities": [
        { "presetId": "player", "position": [0, 0] }
      ]
    }
  ]
}
```


### 2. 핵심 클래스 구조 (UML 요약)

- **Entity**: 게임 내 모든 오브젝트의 베이스.  
  - *Blackboard*: 엔티티별 변수 저장소
  - *BehaviorTreeRunner*: 행동트리 실행기

- **Blackboard**:  
  - 엔티티별 변수와 전역 변수(`g_` 접두사)를 구분하여 저장/조회
  - 타입별 getter/setter 제공

- **BehaviorTreeRunner**:  
  - 루트 노드부터 행동트리를 실행, 각 노드의 상태 관리

- **Node 계층**:  
  - *BranchNode* (Composite/Decorator): 자식 노드 제어
  - *LeafNode* (ActionNode/ConditionNode): 실제 행동/조건 수행

- **Component**:  
  - 예: Movement, Gravity 등 엔티티에 부착되는 물리/로직 컴포넌트


### 3. 동작 흐름 (Manager Sequence)

1. **GameCreator**가 JSON을 읽어 전체 게임을 초기화
2. 각 Manager(Controller, Preset, Entity 등)가 자신의 역할에 맞게 데이터 세팅
3. EntityManager가 엔티티를 생성, 각 엔티티는 자신의 BT와 컴포넌트, 변수(Blackboard)를 초기화
4. 각 Entity는 매 프레임 자신의 BehaviorTreeRunner를 통해 행동트리를 실행


### 4. Behavior Tree (BT) 시스템

- **BT 노드 정의**:  
  - 모든 노드는 `INode` 인터페이스를 구현
  - `Evaluate()`, `ToJson()`, `FromJson()` 메서드 필수

- **노드 확장**:  
  - 새 액션/조건 노드는  
    - `Assets/Script for Game/Nodes/Actions/`  
    - `Assets/Script for Game/Nodes/Conditions/`  
    에 각각 추가

- **BT에서 변수 사용**:  
  - `{f_moveSpeed}`처럼 중괄호로 감싸면 블랙보드 변수 참조
  - `g_`로 시작하면 전역 변수

- **BT 예시**
```json
"behaviorTree": {
  "type": "composite",
  "name": "Sequence",
  "children": [
    { "type": "condition", "name": "CheckCollision", "direction": "down", "targetTags": ["Ground"], "collisionType": "stay", "outputTarget": "{e_groundEntity}" },
    { "type": "action", "name": "MoveByController", "controllerId": "Movement", "moveSpeed": "{f_moveSpeed}" },
    { "type": "condition", "name": "IsButtonDown", "buttonId": "Jump" },
    { "type": "action", "name": "Jump", "jumpForce": "{f_jumpForce}" }
  ]
}
```


## 확장/커스텀 방법

- **새 액션/조건 노드 추가**:  
  - `ActionNode`/`ConditionNode` 상속, `Evaluate()` 구현
  - `FromJson()`에서 파라미터 파싱
  - `NodeParam`, `CustomNodeParam` 등 어트리뷰트로 JSON 매핑

- **새 컴포넌트 추가**:  
  - `Assets/Script for Game/Components/`에 MonoBehaviour로 구현
  - 엔티티 프리팹/JSON에 type으로 명시

- **전역 변수 사용**:  
  - `g_` 접두사로 선언, 모든 엔티티/노드에서 접근 가능


## 참고 UML/시퀀스 다이어그램

- `UML/ClassUML.wsd` : 전체 클래스 구조, 상속, 컴포지션, 주요 메서드
- `UML/Manager Sequence.wsd` : 게임 초기화 및 매니저 간 상호작용 시퀀스


## 주요 파일/폴더 구조

- `Assets/Scripts/BehaviorTree/` : BT 프레임워크(런타임, 노드, 에디터)
- `Assets/Script for Game/Nodes/Actions/` : 게임별 액션 노드
- `Assets/Script for Game/Nodes/Conditions/` : 게임별 조건 노드
- `Assets/Script for Game/Components/` : 게임별 컴포넌트
- `Assets/Resources/` : JSON 데이터, 프리셋, 리소스


## 예시 데이터/게임

- `Assets/Resources/data.json` 등에서 실제 게임 예시와 BT 구조를 참고할 수 있습니다.

