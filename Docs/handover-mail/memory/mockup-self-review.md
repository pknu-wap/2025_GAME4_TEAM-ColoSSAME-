---
name: mockup-self-review
description: "UI 시안은 제시 전에 스스로 여러 번 비판적으로 검증·개선한 최적안을 보여 줄 것 (공통 UI 기준, 빈 공간·중복 정보·상태별 값까지)"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 2916fbc0-17c8-486e-9504-5f7dca738162
  modified: 2026-11-24T16:31:12.352Z
---

시안(HTML)은 한 번 만들고 바로 보여 주지 말고, 스스로 여러 차례 검증·개선을 돌린 뒤 최적안을 제시한다. 사용자가 "일" 화면에서 개선점 제시→반영을 여러 번 반복한 방식을 마음에 들어 했고, 나머지 화면에도 자동으로 적용하라고 함 (2026-11-25).

**Why:** 첫 "일" 시안은 공통 UI 페이지 미확인, 시스템 글꼴, 빈 공간, 중복 정보 등으로 사용자가 직접 지적해야 했음. 반복 검증 후 결과가 크게 좋아짐.

**How to apply:** 시안을 보여 주기 전에 아래를 직접 확인하고 고친 뒤, 확인한 내용과 남은 선택지만 짧게 보고:
- Notion "공통 UI" 페이지 규칙·톤 배정(2안)·예시 좌표(`/Users/lss/Assets/Collo/ColoSSAME_UI_Claude/tools/mock_v8.py`, `00_preview/sample_*_scheme2.png`)과 맞는지, 글꼴 DNFBitBitv2
- 빈 공간·정렬(좌우 페이지 끝선, 간격 통일)·글자 넘침
- 같은 정보가 두 번 나오지 않는지(상단 바와 책 내용 포함)
- 데이터 상태별 표시: 0경기·기록 적음·영어 문구·값 0
- 실제 데이터로 만들 수 있는지(코드 확인), 의미 없는 값(예: 순위용 가짜 점수) 표시 금지
- 비판적 개선점이 남아 있으면 반영하거나 선택지로 묻기
관련: [[propose-before-work]], [[colossame-ui-rollout-workflow]], [[text-color-match-background]]
