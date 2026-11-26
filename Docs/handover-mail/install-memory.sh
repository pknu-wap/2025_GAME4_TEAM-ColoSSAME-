#!/usr/bin/env bash
# 노트북 Claude 메모리 사본을 이 PC의 Claude Code 프로젝트 메모리로 복사한다.
# 저장소 루트(2025_GAME4_TEAM-ColoSSAME-)에서 실행. 기존 파일은 덮어쓰지 않는다.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SRC="$ROOT/Docs/handover-mail/memory"
for DIR in "$ROOT" "$ROOT/Main_Project"; do
  KEY="$(echo "$DIR" | sed 's#[^A-Za-z0-9]#-#g')"
  DEST="$HOME/.claude/projects/$KEY/memory"
  mkdir -p "$DEST"
  cp -n "$SRC"/*.md "$DEST"/ 2>/dev/null || true
  echo "메모리 복사: $DEST"
done
