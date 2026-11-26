# 데스크탑 Claude Code 세션에 붙여 넣을 프롬프트

(저장소를 받아 `Design/Mail` 브랜치로 바꾼 뒤, 저장소 폴더에서 로컬 세션을 열고 아래를 그대로 붙여 넣기)

---

ColoSSAME Unity 프로젝트에서 노트북 세션의 UI 작업을 이어서 한다. 브랜치는 `Design/Mail`이고, 새 브랜치나 브랜치 이동은 내가 지시할 때만 한다.

먼저 할 일:
1. `git pull`로 최신 상태를 받는다.
2. 메모리를 설치한다. Windows면 `powershell -ExecutionPolicy Bypass -File Docs\handover-mail\install-memory.ps1`, macOS/Linux면 `bash Docs/handover-mail/install-memory.sh`. 설치된 경로를 알려 줘.
3. `Docs/handover-mail/WORKFLOW.md`(작업 방식)와 `Docs/handover-mail/README.md`(현재 상태·승인된 소식통 시안·적용 계획·미결 질문)를 읽는다. 승인된 시안은 `Docs/handover-mail/mockups/mail.html`.
4. Unity로 `Main_Project`가 열려 있으면 Unity MCP 연결과 컴파일 상태를 확인한다.

그다음 할 일: 소식통(Mail) 화면 Unity 적용. 시작 전에 README 4장의 미결 질문 2개(기존 투기장 소식 처리, 기존 PredictionUI 교체)를 먼저 묻고, 적용 계획을 번호 목록으로 보여 준 뒤 내 승인을 받고 시작한다.

답은 한국어로, 내가 판단해야 할 핵심만 짧게(결과 2~4줄 + 질문 1~2개).
