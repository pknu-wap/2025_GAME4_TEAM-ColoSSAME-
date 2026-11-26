---
name: user-work-scope
description: "ColoSSAME 프로젝트에서 사용자(이승수)가 맡아 온 작업 범위 — 시설 6개, 팀관리, UI 업데이트"
metadata:
  node_type: memory
  type: project
  originSessionId: b64e22d3-77dd-4ef4-8f14-4c6d384ec204
  modified: 2026-11-16T14:30:53.064Z
---

2026-11-16 기준 사용자가 직접 밝힌 담당 작업:
- 시설 및 시설 6개 (관련 코드 예: `Assets/Scripts/Facilities/`)
- 팀관리
- UI 업데이트 (진행 중) — Notion `Design Collossam UI` 순서대로 화면 적용, PR은 화면 묶음 단위(#170 = MainMenu·설정, 로딩창부터 다음 PR)

**Why:** 다른 팀원 담당 영역(전투, 리그 등)과 구분해, 수정·리팩터링·버그 제보 범위를 판단하기 위함.
**How to apply:** UI 작업 중 시설·팀관리 화면은 사용자 본인 코드라 구조를 이해하고 있으니 연결 방식을 물어봐도 됨. 그 밖의 영역 코드는 건드리지 말고 발견한 문제는 PR "발견된 버그"에 적기만. 관련: [[colossame-ui-rollout-workflow]], [[text-color-match-background]]
