---
name: text-color-match-background
description: UI 배경 스프라이트를 바꾸거나 적용할 때마다 그 위 Text 색도 배경 색에 맞춰 함께 조정
metadata:
  node_type: memory
  type: feedback
  originSessionId: b64e22d3-77dd-4ef4-8f14-4c6d384ec204
  modified: 2026-11-16T14:20:18.879Z
---

배경 Asset(버튼·패널 스프라이트)을 적용·교체하면 그 위 Text(TMP) 색을 배경 색에 맞게 같이 설정한다. 사용자가 "앞으로도 Text 색깔을 배경 Asset 색깔에 맞춰서 잘 설정해라"라고 지시함 (2026-11-16, 지원 에셋 버튼 적용 때 글자색을 안 바꿔서).

**Why:** 스프라이트만 바꾸면 기존 글자색이 새 배경에서 안 읽히거나 어울리지 않음.
**How to apply:** 스프라이트 가운데 색을 값으로 뽑아 명암 대비(4.5:1 이상 목표)를 계산하고, 같은 팔레트 계열(어두운 바탕 → 밝은 크림/금색, 밝은 양피지 → 진한 갈색 #592a16 계열)로 고른다. PixelButton은 상태별 글자색을 `labelColor`/`labelDisabledColor`로 덮어쓰므로 TMP 색이 아니라 그 필드를 바꾼다. 비활성 상태 배경에도 대비를 확인. 사용자가 설정 패널(popup_sheet_tall 양피지) 드롭다운 이름(RowLabel)을 직접 검은색 #000000으로 바꿈 — 사용자가 직접 정한 글자색은 덮어쓰지 말 것. 관련: [[colossame-ui-rollout-workflow]]
