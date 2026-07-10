# Skill Example Notes

이 문서는 `New Scene`에서 스킬 SO가 실제로 어떻게 나가는지 확인하기 위해 정리한 메모입니다.

## 작업 기준

- 기존 스킬/AI 코드는 수정하지 않는다.
- 테스트용 코드는 `Assets/BattleK/Scripts/AI/Skill/skill_example` 안에만 둔다.
- `New Scene`은 현재 테스트 기준 씬으로 사용한다.
- 기존 코드 수정이 필요하면 먼저 확인받고 진행한다.

## New Scene 테스트 구성

`New Scene`에는 스킬 테스트를 위해 다음 오브젝트를 사용합니다.

- `SkillExampleBattleStarter`
- `AI_Manager`
- `AstarManager`
- `CameraBounds`
- 테스트용 Team 1 캐릭터
- 테스트용 Team 2 캐릭터

`SkillExampleBattleStarter`는 씬 시작 시 Team 1, Team 2 캐릭터를 `AI_Manager`에 등록하고 스킬이 자동으로 나가는지 확인하기 위한 테스트 스크립트입니다.

현재 설정 기준으로 Team 2는 움직이지 않게 처리되어 있습니다. Team 1이 Team 2를 대상으로 스킬을 사용하는지 확인하는 용도입니다.

## SkillExampleBattleStarter 역할

파일 위치:

`Assets/BattleK/Scripts/AI/Skill/skill_example/SkillExampleBattleStarter.cs`

주요 역할:

- Team 1 / Team 2 캐릭터를 등록한다.
- `AI_Manager`에 각 캐릭터를 Player/Enemy 팀으로 등록한다.
- Team 1의 초기 타겟을 Team 2 중 가까운 캐릭터로 잡는다.
- Team 2는 테스트용으로 정지 상태를 유지할 수 있다.
- 캐릭터의 `Stat > Skills`에 들어간 SkillSO가 실제 전투 흐름에서 실행되는지 확인한다.

주의:

- 이 스크립트는 테스트용이다.
- 실제 전투 시스템 자체를 고치는 목적이 아니다.

## SkillSO 실행 흐름

기준 파일:

`Assets/BattleK/Scripts/AI/Skill/Base/SkillSO.cs`

대략적인 흐름:

1. `StaticAICore`가 SkillSO 실행 조건을 확인한다.
2. SkillSO가 `WindupTime`만큼 기다린다.
3. `SkillPrefab`을 Owner 또는 Target 위치에 생성한다.
4. 생성된 프리팹 안의 `LogicProcessor`들을 찾는다.
5. `LogicProcessor.Initialize(...)`로 owner, target, skill logic, lifetime을 넘긴다.
6. `LogicProcessor.StartProcess()`가 실행된다.
7. Processor가 대상에게 `SkillLogics`를 적용한다.
8. 프리팹은 `ActiveTime` 이후 Destroy된다.

중요:

- `ActiveTime`은 스킬 프리팹/이펙트가 살아있는 시간에 가깝다.
- `DotDamageLogic`의 도트 지속시간은 `ActiveTime`이 아니라 `DotDamageLogic.Duration` 쪽이다.

## Logic Type 코드 위치

Inspector의 `Skill Logics > Logic Type`에 뜨는 것들은 보통 `ISkillLogic`을 구현한 클래스입니다.

주요 위치:

- `DotDamageLogic`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/DotDamageAction.cs`
- `RemoveDebuffLogic`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/RemoveDebuffLogic.cs`
- `SwordDamageLogic`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/SwordDamageLogic.cs`
- `SPMultipleDamage`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/SPMultipleDamage.cs`
- `DfMultipleDamage`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/DfMultipleDamage.cs`
- `HealthPerDamageLogic`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/HealthPerDamageLogic.cs`
- `HealthPerHealLogic`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/HealthPerHeal.cs`
- `ApplyCC`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/ExecuteLogic/ApplyCC.cs`

Processor 계열 위치:

- `LogicProcessor`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/LogicBase/LogicProcessor.cs`
- `BoxLogicHandler`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/AttackSkillLogics/BoxLogicHandler.cs`
- `InstantTargetProcessor`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/ExecuteLogic/InstantTargetProcessor.cs`
- `AllAllyProcessor`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/ExecuteLogic/AllAllyProcessor.cs`
- `KnockbackProcessor`: `Assets/BattleK/Scripts/AI/Skill/Base/Logic/ExecuteLogic/Knockback.cs`

## DotDamageLogic 관련

`DotDamageLogic`는 도트 데미지 수치와 지속시간을 가지고 있습니다.

관련 흐름:

- `DotDamageLogic.Execute(...)`
- `StatusEffectManager.ApplyDotDamage(...)`
- `StatusEffectManager.DotDamageRoutine(...)`

도트 데미지 지속시간:

- `DotDamageLogic.Duration`

도트 데미지 틱 간격:

- `DotDamageLogic.TickInterval`

도트 데미지가 들어갈 때 피격 애니메이션이 잠깐 나오는 이유:

- 도트 틱마다 `StaticAICore.OnTakeDamage(...)`가 호출된다.
- 사망하지 않았고 현재 Override 상태가 없으면 `StaticHitState`로 들어간다.
- `StaticHitState` 기본 지속시간은 0.5초다.

## 군단병 스킬 판단

현재 군단병 전용 SO 폴더/에셋은 없음.

기획서 기준 군단병 스킬은 다음과 같은 판단입니다.

### 현재 구조로 가능하거나 비슷하게 가능한 것

- `2티어 1` 기절: `ApplyCC`로 가능.
- `2티어 2` 단순 피해: 기존 공격 로직과 근접 프리팹을 조합해 비슷하게 가능.
- `3티어 2` 근접 강화 공격 이펙트: 검투사 프리팹을 재사용하면 비슷하게 가능.
- 근접 범위 이펙트 구조: 검투사 스킬 프리팹에 `BoxLogicHandler`가 있어서 재사용 가능.

검투사 재사용 후보:

- `Assets/BattleK/Prefabs/Skills/Gladiator/GLD_T2_1_P.prefab`
- `Assets/BattleK/Prefabs/Skills/Gladiator/GLD_T2_2_P.prefab`
- `Assets/BattleK/Prefabs/Skills/Gladiator/GLD_T3_2_P.prefab`

### 현재 구조만으로 정확히 어려운 것

- 궁극기 무적: 현재 `StatusType`에 무적 상태가 없고, `OnTakeDamage`는 최소 1 데미지가 들어간다. 정확한 무적은 새 SkillLogic 또는 피격 처리 쪽 지원이 필요하다.
- 도발: `ForcedTargetZone2D`는 있지만 현재 SkillSO가 자동으로 owner를 넣어주는 구조는 아니다. 스킬 프리팹이 캐릭터 자식으로 생성되지 않기 때문에 `_owner`가 비어 있을 수 있다.
- 다음 기본공격 1회 강화: 현재 `AttackDamageMultiplier`로 일정 시간 공격력 버프처럼 흉내는 가능하지만, "다음 평타 1회만 강화"는 별도 로직이 필요하다.

## ForcedTargetZone2D 판단

파일 위치:

`Assets/BattleK/Scripts/AI/Skill/provoke/ForcedTargetZone2D.cs`

동작 요약:

- Trigger Collider 안에 들어온 적의 `StaticAICore.Target`을 owner로 강제 변경한다.
- 시간이 지나면 이전 타겟으로 되돌린다.
- `_owner`를 직접 넣거나 부모에서 자동 탐색한다.

현재 SkillSO와 바로 연결하기 애매한 이유:

- SkillSO는 생성한 프리팹의 `LogicProcessor`만 초기화한다.
- `ForcedTargetZone2D`는 `LogicProcessor`가 아니다.
- SkillSO가 `ForcedTargetZone2D.Initialize(owner, duration)`를 호출하지 않는다.
- 스킬 프리팹이 owner의 자식으로 붙는 구조도 아니라 `_autoFindOwnerInParent`만으로는 안정적이지 않다.

정확한 도발 스킬을 만들려면 `ForcedTargetZone2D`를 SkillSO 흐름에 연결하는 추가 로직이 필요할 가능성이 높다.

## 현재 결론

현재 있는 것들만으로 군단병 스킬을 전부 정확하게 만들기는 어렵다.

다만 아래는 만들 수 있거나 비슷하게 구현할 수 있다.

- 기절
- 단순 근접 피해
- 범위 근접 이펙트
- 일정 시간 공격력/방어력/이동속도 같은 스탯 변화

아래는 새 코드가 필요할 가능성이 높다.

- 진짜 무적
- 정확한 도발
- 다음 기본공격 1회 강화

