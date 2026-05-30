using System;
using System.Collections.Generic;
using System.Linq;

namespace DebugHeroFileDungeonRPG
{
    public sealed class NpcDialogueEntry
    {
        public readonly int Order;
        public readonly int StageIndex;
        public readonly string Category;
        public readonly string Title;
        public readonly string Body;

        public NpcDialogueEntry(int order, int stageIndex, string category, string title, string body)
        {
            Order = order;
            StageIndex = stageIndex;
            Category = category ?? string.Empty;
            Title = title ?? string.Empty;
            Body = body ?? string.Empty;
        }

        public string ToDisplayText()
        {
            string head = string.IsNullOrWhiteSpace(Title) ? Category : Title;
            if (string.IsNullOrWhiteSpace(head)) return Body;
            if (string.IsNullOrWhiteSpace(Body)) return head;
            return head + "\n\n" + Body;
        }
    }

    public sealed class NpcEndingText
    {
        public readonly string Title;
        public readonly string Body;
        public NpcEndingText(string title, string body) { Title = title ?? string.Empty; Body = body ?? string.Empty; }
    }

    /// <summary>
    /// NPC 대사 중앙 관리 파일입니다. 이 파일을 수정하면 인트로, 프로필 입력, 바탕화면 안내,
    /// 스테이지 NPC 힌트, 클리어창, 최종 입력창, 엔딩 문구가 같은 값을 사용합니다.
    /// NPC 대화창 UI와 NPC 이미지는 기존 SystemWindowUI / Renderer를 그대로 사용합니다.
    /// </summary>
    public static class NpcDialogueData
    {
        public const int SourceParagraphCount = 427;
        public const int SourceNonEmptyParagraphCount = 153;
        public const int SourceTableCount = 261;
        public const int SourceDocumentEntryCount = 261;

        public const string AssistantTitle = "Windows Recovery Assistant";
        public const string ProfileSetupTitle = "Recovery Profile Setup";
        public const string ProfileSetupLabel = "복구 프로필 이름을 입력하세요.";
        public const string ProfileSetupDescription = "이 이름은 캐릭터 이름이 아니라 복구 기록과 진행 상황을 저장하는 프로필 이름입니다.";
        public const string ProfileSetupCloseWarning = "복구 프로필이 없으면 진행 상황을 저장할 수 없습니다.\n프로필 이름을 입력해주세요.";
        public const string ProfileNameRequired = "프로필 이름 필요";
        public const string DesktopNoticeBody = "Recovery Program이 생성되었습니다.\n바탕화면에 보이는 파일 바로가기를 실행해 복구 절차를 \n진행하세요.\n아직 보이지 않는 파일은 이전 스테이지를 완료해야 \n생성됩니다.";
        public const string FinalInputInstruction =
    "삭제할 프로세스 이름을 입력하십시오.\n\n" +
    "정리되지 않은 항목이 아직 남아 있습니다.\n" +
    "주변의 파일명과 기록을 확인한 뒤,\n" +
    "종료할 대상을 직접 입력해야 합니다.\n\n" +
    "정확한 이름을 입력해야 절차가 완료됩니다.";

        private sealed class RuntimeNpcDialog
        {
            public readonly NpcMood Mood;
            public readonly string Text;

            public RuntimeNpcDialog(NpcMood mood, string text)
            {
                Mood = mood;
                Text = text ?? string.Empty;
            }
        }

        private static RuntimeNpcDialog D(NpcMood mood, string text)
        {
            return new RuntimeNpcDialog(mood, text);
        }

        public static readonly string[] IntroMessages = new string[]
        {
            "안녕하세요.\n저는 Windows Recovery Assistant입니다.\n시스템의 손상된 항목을 확인하고,\n복구 과정을 안내하는 보조 프로그램이죠!\n",
            "왜 제가 나타났는지 궁금하셨나요?\n방금 시스템에서 예기치 못한 오류가 감지되어\n제가 자동으로 실행되었습니다.\n",
            "걱정하지 마세요.\n아직 시스템은 완전히 정지하지 않았습니다.\n지금부터 손상된 기록과 불필요한 오류를 확인하겠습니다.\n",
            "당신은 이 절차를 함께 진행하기 위해 실행된 복구 프로그램입니다.\n이제부터 제가 옆에서 당신을 도와드리겠습니다!\n복구 작업을 시작하기 전에 프로필 이름 설정이 필요합니다."
        };

        // 실제 게임 스테이지 힌트창에 표시되는 문구입니다. 이 부분을 수정하면 게임 화면에 바로 반영됩니다.
        private static readonly Dictionary<int, RuntimeNpcDialog[]> StageRuntimeDialogs = new Dictionary<int, RuntimeNpcDialog[]>
{
    { 1, new RuntimeNpcDialog[]
        {
            D(NpcMood.Happy, "먼저 간단한 복구 작업을 시작해볼까요?\n위험한 건 아니에요. 가벼운 정리 작업이라고 생각하시면 됩니다!"),
            D(NpcMood.Question, "정리 대상이 한 번에 활성화되었습니다.\n괜찮아요!\n천천히 하나씩 처리하면 \n됩니다."),
            
        }
    },

    { 2, new RuntimeNpcDialog[]
        {
            D(NpcMood.Basic, "Driver Vault에 도착했습니다!\n이곳은 장치 드라이버 정보를 보관하는 공간이에요.\n몇몇 장치에서 충돌 신호가 감지되고 있습니다."),
            D(NpcMood.Question, "앗.\n알 수 없는 장치가 응답하고 있어요.\nDriver-K가 복구 명령을 거부하고 있습니다.\n충돌이 계속되면 시스템 안정성이 떨어질 수 있어요."),
            D(NpcMood.Warning, "Driver-K가 활성화되었습니다.\n드라이버 충돌의 중심으로 보여요.\n조심해서 접근해주세요!")
        }
    },

    { 3, new RuntimeNpcDialog[]
        {
            D(NpcMood.Progress, "Windows Update 연구소에 도착했습니다!\n업데이트 상태를 확인하고, 멈춘 구성 요소를 정리하면 됩니다."),
            D(NpcMood.Thinking, "진행률이 약간 불안정해 보이네요.\n괜찮아요.\nWindows Update에서는 가끔 있는 일이에요.\n창을 닫지 말고 잠시 기다려주세요."),
            D(NpcMood.Progress, "업데이트 구성 요소가 활성화되었습니다.\n멈춘 패키지부터 차례대로 정리해주세요.")
        }
    },

    { 4, new RuntimeNpcDialog[]
        {
            D(NpcMood.Basic, "System32 영역에 진입했습니다.\n이곳은 시스템 핵심 파일이 보관된 구역입니다.\n불필요한 조작은 권장되지 않습니다."),
            D(NpcMood.Warning, "High-Kernel이 보호 절차를 시작했습니다.\n시스템 핵심 파일에 접근하려면 먼저 보호막을 해제해야 합니다.\n신중하게 진행해주세요."),
            //D(NpcMood.Thinking, "High-Kernel이 핵심 파일 보호를 우선하고 있습니다.\n하지만 보호 절차가 계속되면 복구가 지연됩니다.\n작업을 계속해주세요.")
        }
    },

    { 5, new RuntimeNpcDialog[]
        {
            D(NpcMood.Basic, "Network Port 항구에 진입했습니다.\n현재 여러 개의 외부 연결이 감지되고 있습니다.\n불필요한 연결은 시스템 안정성을 위해 차단하는 것이 좋습니다."),
            D(NpcMood.Thinking, "연결 상태를 분류하는 중입니다.\n정상 패킷과 이상 패킷을 구분하여 처리해주세요."),
            D(NpcMood.Warning, "알 수 없는 연결은 안전하지 않을 수 있습니다.\n확실하지 않은 연결은 차단하는 편이 안전합니다.")
        }
    },

    { 6, new RuntimeNpcDialog[]
        {
            D(NpcMood.Loading, "시스템 안정성 검사를 시작합니다.\n일부 항목에서 과부하 신호가 감지되었습니다.\n검사를 계속합니다."),
            D(NpcMood.Bsod, "오류가 발생했습니다.\n복구 절차를...\n계속합니다."),
            //D(NpcMood.Damaged, "시스템 충돌은 일시적으로 회피되었습니다.\n원인 추적을 위해 기록 영역을 확인해야 합니다.")
        }
    },

    { 7, new RuntimeNpcDialog[]
        {
            D(NpcMood.Log, "Registry Hive에 진입했습니다.\n이곳에는 시스템 설정값과 실행 기록이 저장되어 있습니다.\n값을 임의로 수정하지 않도록 주의해주세요."),
            D(NpcMood.Log, "기록은 복구에 필요합니다.\n손상된 값만 분리하고, 나머지는 유지해주세요."),
            //D(NpcMood.Thinking, "프로필 정보가 확인되었습니다.\n복구 절차를 계속합니다.")
        }
    },

    { 8, new RuntimeNpcDialog[]
        {
            D(NpcMood.Warning, "오류창이 다수 발생했습니다.\n창 사이에 숨어 있는 중심 오류를 찾아 격리해주세요.")
            //D(NpcMood.Warning, "오류창이 다수 발생했습니다.\n순서대로 닫아주세요.\n창을 닫으면 해결됩니다."),
            //D(NpcMood.Error, "오류창이 계속 생성되고 있습니다.\n닫지 않은 오류는 복구를 방해합니다.\n모두 닫아주세요."),
            //D(NpcMood.Damaged, "처리되지 않은 오류가 누적되었습니다.\n닫힌 창을 복원합니다.")
        }
    },

    { 9, new RuntimeNpcDialog[]
        {
            D(NpcMood.Log, "임시 저장소에 남은 항목이 많습니다.\n불필요한 항목은 삭제해주세요."),
            //D(NpcMood.Warning, "확인되지 않은 임시 항목이 많습니다.\n불필요한 캐시부터 정리해주세요."),
            //D(NpcMood.Damaged, "남은 기록은 복구를 지연시킵니다.\n기록을 줄여야 합니다.")
        }
    },

    { 10, new RuntimeNpcDialog[]
        {
            D(NpcMood.Basic, "이제 마지막 정리만 남았습니다.\n휴지통을 비우면 복구 절차가 완료됩니다."),
            //D(NpcMood.Warning, "삭제된 것들은 사라지지 않습니다.\n보관될 뿐입니다."),
            //D(NpcMood.Damaged, "삭제 대상을 입력하세요.\n정확한 이름을 입력해야 합니다.")
        }
    },
};
        private static readonly Dictionary<int, RuntimeNpcDialog> StageClearDialogs = new Dictionary<int, RuntimeNpcDialog>
{
    { 1, D(NpcMood.Happy,
        "완료!\n정말 깔끔해졌네요.\n처음치고는 아주 좋은 결과예요.\n무사히 시스템을 구할 수 있을 것 같아요!") },

    { 2, D(NpcMood.Happy,
        "좋아요!\n드라이버 충돌이 해결되었습니다.\n장치 목록이 훨씬 안정적으로 보이네요.") },

    { 3, D(NpcMood.Progress,
        "완료되었습니다!\n업데이트 상태가 정상으로 돌아왔어요.\n조금 오래 걸리긴 했지만요. 아하하!") },

    { 4, D(NpcMood.Basic,
        "System32 무결성 검사가 완료되었습니다.\n핵심 파일 상태는 안정적입니다.\n복구 작업을 계속할 수 있습니다.") },

    { 5, D(NpcMood.Basic,
        "네트워크 연결 상태가 안정화되었습니다.\n불필요한 연결이 차단되었고,\n패킷 흐름이 정상 범위로 돌아왔습니다.") },

    { 6, D(NpcMood.Damaged,
        "시스템 충돌은 일시적으로 회피되었습니다.\n재부팅은 보류되었습니다.\n원인 추적을 위해 기록 영역을 확인해야 합니다.") },

    { 7, D(NpcMood.Log,
        "Registry Hive 정리가 완료되었습니다.\n설정 기록은 안정화되었습니다.\n하지만 일부 예외 메시지가 아직 남아 있습니다.") },

    { 8, D(NpcMood.Damaged,
        "오류창 격리가 완료되었습니다.\n일부 임시 보고서가 캐시에 남아 있습니다.\nTemp Cache를 확인해야 합니다.") },

    { 9, D(NpcMood.Warning,
        "임시 저장소 정리가 완료되었습니다.\n삭제된 항목은 Recycle Bin에 보관됩니다.\n최종 정리를 진행해야 합니다.") },

    { 10, D(NpcMood.Damaged,
        "최종 정리 절차가 완료되었습니다.\n삭제 대상 입력 결과를 확인합니다.") },
};
        public static int GetStageDialogCount(int stageIndex)
        {
            RuntimeNpcDialog[] lines;
            if (StageRuntimeDialogs.TryGetValue(stageIndex, out lines) && lines != null && lines.Length > 0)
                return lines.Length;

            return GetStageDialogLines(stageIndex).Length;
        }

        public static NpcMood GetStageHintMood(int stageIndex, int dialogIndex, NpcMood fallbackMood)
        {
            RuntimeNpcDialog[] lines;

            if (StageRuntimeDialogs.TryGetValue(stageIndex, out lines) && lines != null && lines.Length > 0)
            {
                int idx = Math.Max(0, Math.Min(dialogIndex, lines.Length - 1));
                return lines[idx].Mood;
            }

            return fallbackMood;
        }
        public static NpcMood GetStageClearMood(StageInfo st)
        {
            if (st == null) return NpcMood.Basic;

            RuntimeNpcDialog clearDialog;
            if (StageClearDialogs.TryGetValue(st.Index, out clearDialog) && clearDialog != null)
                return clearDialog.Mood;

            return st.NpcMood;
        }

        public static string GetIntroTitle(int index)
        {
            return index < 2 ? AssistantTitle : ProfileSetupTitle;
        }

        public static string GetIntroMessage(int index)
        {
            if (IntroMessages.Length == 0) return string.Empty;
            if (index < 0) index = 0;
            if (index >= IntroMessages.Length) index = IntroMessages.Length - 1;
            return IntroMessages[index];
        }

        public static string[] GetStageDialogLines(int stageIndex)
        {
            RuntimeNpcDialog[] runtimeLines;

            if (StageRuntimeDialogs.TryGetValue(stageIndex, out runtimeLines) &&
                runtimeLines != null &&
                runtimeLines.Length > 0)
            {
                return runtimeLines.Select(x => x.Text).ToArray();
            }

            List<string> fallback = new List<string>();

            for (int i = 0; i < DocumentEntries.Length; i++)
            {
                NpcDialogueEntry e = DocumentEntries[i];
                if (e.StageIndex != stageIndex) continue;

                if (e.Category.IndexOf("Windows Recovery Assistant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    e.Body.IndexOf("Windows Recovery Assistant", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fallback.Add(CleanForRuntime(e.Body));
                    if (fallback.Count >= 3) break;
                }
            }

            if (fallback.Count == 0)
                fallback.Add("복구 절차를 계속 진행합니다.");

            return fallback.ToArray();
        }

        public static void ApplyToStages(List<StageInfo> stages)
        {
            if (stages == null) return;
            for (int i = 0; i < stages.Count; i++)
            {
                StageInfo st = stages[i];
                if (st == null) continue;
                st.Dialogs = GetStageDialogLines(st.Index);
            }
        }

        public static string GetStageHintText(int stageIndex, string stageName, int dialogIndex)
        {
            string[] lines = GetStageDialogLines(stageIndex);

            if (lines.Length == 0)
                return string.Empty;

            int idx = Math.Max(0, Math.Min(dialogIndex, lines.Length - 1));
            string text = lines[idx];

            if (idx == 0)
            {
                return "STAGE " + stageIndex.ToString("00") + "  " + stageName + "\n\n" + text;
            }

            return text;
        }


        public static string BuildStageClearText(StageInfo st, int clearStage, IList<StageInfo> stages)
        {
            if (st == null) return string.Empty;

            RuntimeNpcDialog clearDialog;
            string body;

            if (StageClearDialogs.TryGetValue(st.Index, out clearDialog) && clearDialog != null)
                body = clearDialog.Text + "\n";
            else
                body = st.Name + " 정리가 완료되었습니다.\n";

            if (st.IsBossStage)
                body += "\n보스 개체 [" + st.BossName + "]는 완전히 삭제되지 않고 격리 기록으로 보관됩니다.\n";

            if (stages != null && clearStage < stages.Count)
                body += "\n새 바로가기 생성: " + stages[clearStage].FileName;
            else
                body += "\n최종 입력 절차로 이동합니다.";

            return body;
        }

        public static NpcEndingText ResolveEnding(string finalInput, string profileName)
        {
            string input = (finalInput ?? string.Empty).Trim();
            string profile = (profileName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input)) return new NpcEndingText("회피 루프 엔딩", "결정이 보류되었습니다.\n복구 절차를 다시 예약합니다.\n[15분 후 다시 알림]");
            if (!string.IsNullOrEmpty(profile) && input.Equals(profile, StringComparison.OrdinalIgnoreCase))
                return new NpcEndingText("진엔딩: 프로필 복구 완료", "프로필 이름과 현재 세션이 일치합니다.\n삭제 대상: 현재 세션\n현재 세션이 종료되었습니다.\n남은 항목은 더 이상 실행되지 않습니다.");
            if (input.IndexOf("Assistant", StringComparison.OrdinalIgnoreCase) >= 0 || input.IndexOf("Recovery", StringComparison.OrdinalIgnoreCase) >= 0)
                return new NpcEndingText("Assistant 루프 엔딩", "삭제 실패.\n해당 프로세스는 복구 절차에 의해 보호되고 있습니다.\n저를 종료하려고 하셨나요?\n괜찮습니다. 혼란이 있었던 것 같네요.\n복구를 처음부터 다시 진행하겠습니다.");
            if (input.IndexOf("Driver", StringComparison.OrdinalIgnoreCase) >= 0 || input.IndexOf("Kernel", StringComparison.OrdinalIgnoreCase) >= 0 || input.IndexOf("BSOD", StringComparison.OrdinalIgnoreCase) >= 0 || input.IndexOf("Exception", StringComparison.OrdinalIgnoreCase) >= 0 || input.IndexOf("Binny", StringComparison.OrdinalIgnoreCase) >= 0)
                return new NpcEndingText("일반 엔딩: 외부 대상 삭제", "입력값 확인 중...\n대상 프로세스 발견: 입력한 보스 이름\n프로세스를 종료합니다.\n표면적인 문제는 사라졌지만, 복구 기록은 완전히 닫히지 않았습니다.");
            return new NpcEndingText("회피 루프 엔딩", "대상을 찾을 수 없습니다.\n삭제할 수 없습니다.\n결정이 보류되었습니다.\n복구 절차를 다시 예약합니다.");
        }

        public static string GetStageDocumentText(int stageIndex)
        {
            List<string> parts = new List<string>();
            for (int i = 0; i < DocumentEntries.Length; i++)
            {
                if (DocumentEntries[i].StageIndex == stageIndex) parts.Add(DocumentEntries[i].ToDisplayText());
            }
            return string.Join("\n\n", parts.ToArray());
        }

        public static string GetCoverageReport()
        {
            return "ALL_STAGE_DIALOGUE_COLLECTION_with_actions.docx 반영 확인\n" +
                   "원본 문단 수: " + SourceParagraphCount + "\n" +
                   "비어 있지 않은 문단 수: " + SourceNonEmptyParagraphCount + "\n" +
                   "원본 표 수: " + SourceTableCount + "\n" +
                   "DocumentEntries 수: " + DocumentEntries.Length;
        }

        private static string CleanForRuntime(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            string[] raw = text.Replace("\r", "").Split(new char[] { '\n' }, StringSplitOptions.None);
            List<string> list = new List<string>();
            for (int i = 0; i < raw.Length; i++)
            {
                string line = raw[i].Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("[") && line.EndsWith("]")) continue;
                if (line == "Windows Recovery Assistant" || line == "NPC" || line == "CRT 모니터") continue;
                list.Add(line);
            }
            return string.Join("\n", list.ToArray());
        }

        public static readonly NpcDialogueEntry[] DocumentEntries = new NpcDialogueEntry[]
        {
            new NpcDialogueEntry(1, 0, "문서 목적", "Stage 1부터 엔딩까지 등장하는 알림창, 시스템 메시지, 보스 대사, 입력창, 선택지 문구를 정리한다. 이번 버전은 X 버튼, 취소, 나중에, 닫기, 삭제, 허용 같은 플레이어 행동 반응과 후반 회수 기록까지 포함한다.", "문서 목적 | Stage 1부터 엔딩까지 등장하는 알림창, 시스템 메시지, 보스 대사, 입력창, 선택지 문구를 정리한다. 이번 버전은 X 버튼, 취소, 나중에, 닫기, 삭제, 허용 같은 플레이어 행동 반응과 후반 회수 기록까지 포함한다.\n표기 원칙 | 실제 사용자 이름은 사용하지 않는다. 모든 예시는 “입력한 이름”, “복구 프로필 이름”, “ProfileName”, “플레이어명” 같은 일반 표현으로 처리한다.\n구성 방식 | 기존 Stage 1~10 대사를 유지하고, 각 스테이지 뒤에 “플레이어 선택 / 닫기 반응”과 “행동 기록 / 후반 회수” 항목을 추가한다.\n엔딩 반영 | 창을 닫음, 업데이트를 미룸, 권한을 허용함, 오류 보고를 무시함, 연결을 차단함, 파일을 삭제함 등의 행동이 Stage 7 이후 기록으로 드러나고 Stage 10 엔딩 분기에 반영된다."),
            new NpcDialogueEntry(2, 0, "알림창 / Windows Recovery Assistant | 시스템 메시지 / 입력창 | 보스 대사 / 경고창 | 보안 경고 / 권한 확인 | 오류창 / STOP 메시지 | 플레이어 선택 / 닫기 반응 | 행동 기록 / 후반 회수", "", "알림창 / Windows Recovery Assistant | 시스템 메시지 / 입력창 | 보스 대사 / 경고창 | 보안 경고 / 권한 확인 | 오류창 / STOP 메시지 | 플레이어 선택 / 닫기 반응 | 행동 기록 / 후반 회수"),
            new NpcDialogueEntry(3, 0, "기획 메모", "기본 원칙", "플레이어는 대사를 많이 하지 않는다.\n\n대신 창을 닫고, 취소하고, 나중에 처리하고, 권한을 허용하고, 연결을 차단하고, 파일을 삭제하는 행동이 플레이어의 대사 역할을 한다.\n\n따라서 모든 중요한 창은 정상 진행 버튼뿐 아니라 X 버튼, 취소, 나중에, 닫기 시도를 기록할 수 있어야 한다."),
            new NpcDialogueEntry(4, 0, "플레이어 행동", "즉시 반응", "플레이어 행동 | 즉시 반응 | 기록명 | 후반 회수\nX 버튼 클릭 | Assistant가 다시 안내하거나 후반부에서는 창이 늘어남 | CloseAttempt / ErrorWindowClosed | Stage 7, 8, 10\n나중에 선택 | 알림이 연기되거나 임시 저장됨 | DelayCount / Deferred | Stage 3, 7, 9, 10\n취소 선택 | 작업이 보류되고 다시 시도 유도 | CancelCount | Stage 4, 10\n허용 선택 | 권한 확인 후 진행 가능 | PermissionAllowed | Stage 4, 7, 10\n차단 선택 | 연결 차단 및 보안 처리 | PortBlocked / PortDecision | Stage 5, 7, 10\n삭제 선택 | 파일이 이동되거나 복사본 생성 | DeleteCount / UnsentReport | Stage 9, 10\n빈 입력 / 잘못된 입력 | 대상을 찾지 못하고 루프 또는 실패 | FinalInputInvalid | Stage 10"),
            new NpcDialogueEntry(5, 0, "알림창 / Windows Recovery Assistant", "필수 진행 창의 X 버튼 공통 반응", "Windows Recovery Assistant\n\n아직 필요한 안내가 남아 있습니다.\n\n복구 절차를 계속하려면 확인이 필요합니다.\n\n[확인]\n\n기록: CloseAttempt += 1"),
            new NpcDialogueEntry(6, 1, "스테이지", "STAGE 01 일반 [File Explorer 숲]", "스테이지 | STAGE 01 일반 [File Explorer 숲]\n기존 대사/창 개수 | 16개\n반영 요소 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(7, 1, "효과음", "01. 효과음", "효과음\n띵."),
            new NpcDialogueEntry(8, 1, "알림창 / Windows Recovery Assistant", "02. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n안녕하세요!\n\n저는 Windows Recovery Assistant입니다.\n당신은 이 시스템의 손상된 항목을 확인하고,\n불필요한 오류를 정리하기 위해 실행된 복구 프로그램입니다.\n걱정하지 마세요.\n제가 옆에서 도와드릴게요!\n\n현재 바탕화면에 정리되지 않은 파일 개체가 많아 보여요.\n\n[확인]"),
            new NpcDialogueEntry(9, 1, "알림창 / Windows Recovery Assistant", "03. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n먼저 간단한 복구 테스트를 시작해볼까요?\n\n위험한 건 아니에요.\n가벼운 정리 작업이라고 생각하시면 됩니다!\n[시작하기]"),
            new NpcDialogueEntry(10, 1, "알림창 / Windows Recovery Assistant", "04. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n복구 작업을 시작하기 전에\n프로필 이름을 설정해주세요.\n\n이 이름은 복구 기록과 진행 상황을 저장하는 데 사용됩니다.\n[확인]"),
            new NpcDialogueEntry(11, 1, "시스템 메시지 / 입력창", "05. Recovery Profile Setup", "Recovery Profile Setup\n복구 프로필 이름을 입력하세요.\n\n[________________]\n\n[확인]\n[닫기]"),
            new NpcDialogueEntry(12, 1, "플레이어 선택 / 닫기 반응", "추가 03. 필수 창 닫기 제한", "플레이어가 이름 입력창을 닫으려는 경우\n시무룩한 표정\nWindows Recovery Assistant\n\n복구 프로필이 없으면 진행 상황을 저장할 수 없습니다.\n\n프로필 이름을 입력해주세요.\n\n[확인]\n\n기록: ProfileSetupCloseAttempt += 1"),
            new NpcDialogueEntry(13, 1, "알림창 / Windows Recovery Assistant", "06. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n프로필 생성이 완료되었습니다.\n\n이제 복구 프로그램을 준비할게요!"),
            new NpcDialogueEntry(14, 1, "알림창 / Windows Recovery Assistant", "07. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n좋아요!\n\n이제 직접 움직여볼까요?\n\n바탕화면 위의 파일 개체들을 정리하려면 먼저 이동이 필요합니다.\n[확인]"),
            new NpcDialogueEntry(15, 1, "알림창 / Windows Recovery Assistant", "08. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n\n정상적으로 이동하고 있어요.\n아주 부드럽네요!\n[확인]"),
            new NpcDialogueEntry(16, 1, "알림창 / Windows Recovery Assistant", "09. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n 느낌표 얼굴(놀란 얼굴)\n정리 대상이 한 번에 활성화되었습니다.\n\n괜찮아요!\n천천히 하나씩 처리하면 됩니다.\n[확인]"),
            new NpcDialogueEntry(17, 1, "시스템 메시지 / 입력창", "10. 상호작용 표시", "상호작용 표시\n[Enter] 휴지통 비우기"),
            new NpcDialogueEntry(18, 1, "시스템 메시지 / 입력창", "11. 파일 삭제 확인", "파일 삭제 확인\n이 항목들을 완전히 삭제하시겠습니까?\n\n[예] [아니오]"),
            new NpcDialogueEntry(19, 1, "시스템 메시지 / 입력창", "12. 알림", "알림\n바탕화면 정리 완료\n시스템 상태가 소폭 개선되었습니다.\n\n[확인]"),
            new NpcDialogueEntry(20, 1, "알림창 / Windows Recovery Assistant", "13. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n완료!\n\n정말 깔끔해졌네요.\n처음치고는 아주 좋은 결과예요.\n무사히 시스템을 구할 수 있을 것 같아요!\n\n[확인]"),
            new NpcDialogueEntry(21, 1, "알림창 / Windows Recovery Assistant", "14. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n이제 다음 구역을 확인해볼 차례입니다.\n\n놀란 얼굴\n장치 드라이버 쪽에서 충돌 알림이 들어왔어요.\n아마 오래된 장치가 말썽을 부리는 것 같네요.\n[확인]"),
            new NpcDialogueEntry(22, 1, "알림창 / Windows Recovery Assistant", "15. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nNPC\nCRT 모니터\n웃는 얼굴\n준비되면 이 아이콘을 실행해주세요.\n\n제가 계속 안내해드릴게요!\n[확인]"),
            new NpcDialogueEntry(23, 1, "시스템 메시지 / 입력창", "16. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(24, 1, "알림창 / Windows Recovery Assistant", "추가 01. 복구 프로필 생성 완료 후 역할 소개", "Windows Recovery Assistant\n\n복구 프로필이 정상적으로 생성되었습니다.\n\n당신은 지금부터 시스템 복구 작업을 수행하는 Recovery Program으로 실행됩니다.\n\n손상된 항목을 확인하고, 불필요한 오류를 정리하는 것이 주요 작업입니다.\n\n걱정하지 마세요!\n저는 --님이 잘 하실거라고 믿어요!!\n\n[시작하기]\n[닫기]"),
            new NpcDialogueEntry(25, 1, "플레이어 선택 / 닫기 반응", "추가 02. X 버튼 클릭 시 반응", "플레이어가 첫 안내창의 X 버튼을 누른 경우\n\nWindows Recovery Assistant\n\n앗, 아직 안내가 끝나지 않았어요.\n\n처음 복구 작업이니까 조금만 따라와 주세요!\n\n[확인]\n\n기록: CloseAttempt_Stage01 += 1"),
            new NpcDialogueEntry(26, 2, "스테이지", "STAGE 02 보스 [Driver Vault 격납고]", "스테이지 | STAGE 02 보스 [Driver Vault 격납고]\n기존 대사/창 개수 | 26개\n반영 요소 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(27, 2, "시스템 메시지 / 입력창", "01. 바탕화면 아이콘", "바탕화면 아이콘\nDriver Vault.exe\n\n[Enter] 실행"),
            new NpcDialogueEntry(28, 2, "보스 대사 / 경고창", "02. Driver Vault", "Driver Vault\n+ Display Adapter\n+ Sound Driver\n+ USB Controller     ⚠\n+ Network Adapter\n+ Unknown Device     ⚠\n+ Legacy Device      ⚠"),
            new NpcDialogueEntry(29, 2, "알림창 / Windows Recovery Assistant", "03. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nCRT 모니터\n안내 표정\nDriver Vault에 도착했습니다!\n\n이곳은 장치 드라이버 정보를 보관하는 공간이에요.\n\n몇몇 장치에서 충돌 신호가 감지되고 있습니다.\n[확인]"),
            new NpcDialogueEntry(30, 2, "알림창 / Windows Recovery Assistant", "04. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nCRT 모니터\n살짝 놀람\n앗.\n\n알 수 없는 장치가 응답하고 있어요.\n\nDriver-K가 복구 명령을 거부하고 있습니다.\n\n충돌이 계속되면 시스템 안정성이 떨어질 수 있어요.\n\n필요한 부분만 정리해볼까요?\n[확인]"),
            new NpcDialogueEntry(31, 2, "보스 대사 / 경고창", "05. 장치 이름 변화", "장치 이름 변화\nUnknown Device\nUnknown Device?\nUnknown Device_K\nDriver-K"),
            new NpcDialogueEntry(32, 2, "시스템 메시지 / 입력창", "06. 새 하드웨어 발견", "새 하드웨어 발견\n알 수 없는 장치\n\n[드라이버 설치] [취소]"),
            new NpcDialogueEntry(33, 2, "시스템 메시지 / 입력창", "07. 장치 오류", "장치 오류\n이 장치의 드라이버를 로드할 수 없습니다.\nCode 39\n\n[확인]"),
            new NpcDialogueEntry(34, 2, "알림창 / Windows Recovery Assistant", "취소 선택시", "Windows Recovery Assistant\nWindows Recovery Assistant\nCRT 모니터\n설치를 취소하셨네요..\n\n하지만 장치 충돌이 계속 감지되고 있어요.\n\n문제를 해결하려면 드라이버 상태를 다시 확인해야 합니다."),
            new NpcDialogueEntry(35, 2, "시스템 경고창", "08. Unknown Device", "Unknown Device\n접근 거부.\n\n이 장치는 외부 복구 명령을 허용하지 않습니다.\n[확인]"),
            new NpcDialogueEntry(36, 2, "보스 대사 / 경고창", "09. Driver-K", "Driver-K\n새로운 드라이버는 필요 없다.\n\n나는 아직 작동 중이다.\n[확인]"),
            new NpcDialogueEntry(37, 2, "보스 대사 / 경고창", "10. Driver-K", "Driver-K\n복구 프로그램 확인.\n\n장치 제거 권한 감지.\n\n접근을 차단합니다.\n[확인]"),
            new NpcDialogueEntry(38, 2, "알림창 / Windows Recovery Assistant", "11. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nCRT 모니터\n경고 표정\nDriver-K가 활성화되었습니다.\n\n드라이버 충돌의 중심으로 보여요.\n\n조심해서 접근해주세요!"),
            new NpcDialogueEntry(39, 2, "플레이어 선택 / 닫기 반응", "추가 01. 보스전 안내창 X 버튼 클릭 시", "플레이어가 Driver-K 안내창을 닫으려는 경우\n\nWindows Recovery Assistant\n\nDriver-K가 이미 활성화되었습니다.\n\n안내를 닫아도 충돌 상태는 유지됩니다.\n\n조심해서 접근해주세요.\n\n[확인]\n\n기록: BossWarningClosed_Stage02 += 1"),
            new NpcDialogueEntry(40, 2, "보스 대사 / 경고창", "12. Driver-K", "Driver-K\nHP: 미정\n상태: 드라이버 충돌"),
            new NpcDialogueEntry(41, 2, "보스 대사 / 경고창", "13. 패턴 | 화면/기능 | 대사 또는 시스템 메시지", "패턴\n화면/기능\n대사 또는 시스템 메시지\nUSB 케이블 공격\nDriver-K의 팔이 USB 케이블처럼 늘어나 플레이어를 공격한다.\nUSB Controller\n비정상 전류가 감지되었습니다.\n드라이버 충돌\n장치 목록 일부가 깜빡이며 플레이어의 이동을 방해한다.\nDriver-K\n충돌이 아니다.\n호환되지 않는 것은 너희 쪽이다.\n노란 느낌표 경고\n공격 직전 바닥이나 장치 항목 위에 노란 느낌표가 표시된다.\n시스템 경고\n장치 충돌 위험.\n새 하드웨어 발견\n작은 장치 아이콘들이 잠시 나타나 보조 장애물처럼 작동한다.\n새 하드웨어를 찾았습니다.\n알 수 없는 장치.\n드라이버 재설치\nDriver-K가 잠시 멈춰 체력을 회복하거나 방어 상태가 된다.\nDriver-K\n나는 교체되지 않는다."),
            new NpcDialogueEntry(42, 2, "시스템 메시지 / 입력창", "14. Legacy Device", "Legacy Device\n이 장치는 오래된 드라이버를 사용하고 있습니다.\n\n장치를 계속 사용하려면 드라이버가 필요합니다."),
            new NpcDialogueEntry(43, 2, "보스 대사 / 경고창", "15. Driver-K", "Driver-K\n나는 오래된 것이 아니다.\n\n나는 필요한 것이다.\n[확인]"),
            new NpcDialogueEntry(44, 2, "보스 대사 / 경고창", "16. Driver-K", "Driver-K\n제거하지 마라.\n\n아직 연결된 장치가 있다.\n[확인]"),
            new NpcDialogueEntry(45, 2, "알림창 / Windows Recovery Assistant", "17. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nCRT 모니터\n생각 중\n음...\n\n오래된 장치 정보가 함께 묶여 있는 것 같아요.\n\n하지만 충돌이 계속되면 시스템이 불안정해집니다.\n\n정리를 계속해주세요.\n[확인]"),
            new NpcDialogueEntry(46, 2, "보스 대사 / 경고창", "18. Unknown Device", "Unknown Device\n장치 응답 불안정.\n\n연결 유지 시도 중...\n[확인]"),
            new NpcDialogueEntry(47, 2, "보스 대사 / 경고창", "19. Driver-K", "Driver-K\n끊기면...\n\n다시는 인식되지 않는다.\n[확인]"),
            new NpcDialogueEntry(48, 2, "보스 대사 / 경고창", "20. Driver-K 위 상호작용 표시", "Driver-K 위 상호작용 표시\n[Enter] 드라이버 복구"),
            new NpcDialogueEntry(49, 2, "시스템 메시지 / 입력창", "21. Driver Recovery", "Driver Recovery\n충돌 중인 드라이버를 복구하시겠습니까?\n\n[복구] [취소]"),
            new NpcDialogueEntry(50, 2, "시스템 메시지 / 입력창", "22. Driver Recovery", "Driver Recovery\n\n드라이버 복구를 시작합니다.\n\n충돌 항목을 정리하는 중...\n\n[■■■■□□□]"),
            new NpcDialogueEntry(51, 2, "시스템 메시지 / 입력창", "22. Driver Recovery", "Driver Recovery\n드라이버 충돌이 해결되었습니다.\n장치 상태: 안정됨"),
            new NpcDialogueEntry(52, 2, "플레이어 선택 / 닫기 반응", "추가 02. 보스전 안내창 X 버튼 클릭 시", "Windows Recovery Assistant\n\n복구가 취소되었습니다.\n\n하지만 Driver-K의 충돌 신호가 아직 남아 있어요.\n장치 상태를 안정화하려면 복구를 완료해야 합니다.\n\n[다시 시도]\n\n기록: BossWarningClosed_Stage02 += 1"),
            new NpcDialogueEntry(53, 2, "플레이어 선택 / 닫기 반응", "추가 03. 보스전 안내창 X 버튼 2번째 클릭 시", "Windows Recovery Assistant\n복구 창이 반복적으로 닫혔습니다.\n이 작업을 완료해야 장치 상태를 안정화할 수 있습니다.\n드라이버 복구를 다시 시도합니다."),
            new NpcDialogueEntry(54, 2, "알림창 / Windows Recovery Assistant", "23. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nCRT 모니터\n웃는 얼굴\n좋아요!\n\n드라이버 충돌이 해결되었습니다.\n\n장치 목록이 훨씬 안정적으로 보이네요.\n[확인]\n[확인]"),
            new NpcDialogueEntry(55, 2, "시스템 메시지 / 입력창", "24. 장치 상태", "장치 상태\n드라이버 복구 완료\n재시작은 필요하지 않습니다.\n\n[확인]"),
            new NpcDialogueEntry(56, 2, "알림창 / Windows Recovery Assistant", "25. Windows Recovery Assistant | Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Recovery Assistant\nCRT 모니터\n웃는 얼굴\n다음은 업데이트 상태를 확인해볼 차례입니다.\n\n일부 업데이트가 오래 지연된 것 같아요.\n\n아하하, Windows Update는 원래 조금 오래 걸리죠!\n[확인]"),
            new NpcDialogueEntry(57, 2, "시스템 메시지 / 입력창", "26. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(58, 2, "플레이어 선택 / 닫기 반응", "추가 01. 보스전 안내창 X 버튼 클릭 시", "플레이어가 Driver-K 안내창을 닫으려는 경우\n\nWindows Recovery Assistant\n\nDriver-K가 이미 활성화되었습니다.\n\n안내를 닫아도 충돌 상태는 유지됩니다.\n\n조심해서 접근해주세요.\n\n[확인]\n\n기록: BossWarningClosed_Stage02 += 1"),
            new NpcDialogueEntry(59, 2, "플레이어 선택 / 닫기 반응", "추가 02. 보스전 안내창 X 버튼 클릭 시", "Windows Recovery Assistant\n\n복구가 취소되었습니다.\n\n하지만 Driver-K의 충돌 신호가 아직 남아 있어요.\n장치 상태를 안정화하려면 복구를 완료해야 합니다.\n\n[다시 시도]\n\n기록: BossWarningClosed_Stage02 += 1"),
            new NpcDialogueEntry(60, 2, "플레이어 선택 / 닫기 반응", "추가 03. 보스전 안내창 X 버튼 2번째 클릭 시", "Windows Recovery Assistant\n복구 창이 반복적으로 닫혔습니다.\n이 작업을 완료해야 장치 상태를 안정화할 수 있습니다.\n드라이버 복구를 다시 시도합니다."),
            new NpcDialogueEntry(61, 3, "스테이지", "STAGE 03 일반 [Windows Update 연구소]", "스테이지 | STAGE 03 일반 [Windows Update 연구소]\n기존 대사/창 개수 | 16개\n반영 요소 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(62, 3, "시스템 메시지 / 입력창", "01. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(63, 3, "시스템 메시지 / 입력창", "02. 시스템 창", "시스템 창\nWindows Update Lab.exe\n\n업데이트 구성 요소를 불러오는 중...\n\n[■■□□□□□□□]"),
            new NpcDialogueEntry(64, 3, "시스템 메시지 / 입력창", "03. 업데이트 창 내부", "업데이트 창 내부\n업데이트 준비 중...\n진행률: 0%\n\n- 보안 업데이트 KB0001\n- 드라이버 업데이트 KB0002\n- 시스템 안정성 업데이트 KB0003"),
            new NpcDialogueEntry(65, 3, "알림창 / Windows Recovery Assistant", "04. 알림창 - Windows Recovery Assistant", "Windows Recovery Assistant\nWindows Update 연구소에 도착했습니다!\n\n업데이트 상태를 확인하고, 멈춘 구성 요소를 정리하면 됩니다.\n\n업데이트는 시스템 안정성을 위해 필요해요.\n\n[확인]"),
            new NpcDialogueEntry(66, 3, "알림창 / Windows Recovery Assistant", "05. 알림창 - Windows Recovery Assistant", "Windows Recovery Assistant\n진행률이 약간 불안정해 보이네요.\n\n괜찮아요.\nWindows Update에서는 가끔 있는 일이에요.\n\n창을 닫지 말고 잠시 기다려주세요.\n\n[확인]"),
            new NpcDialogueEntry(67, 3, "플레이어 선택 / 닫기 반응", "추가 03. 업데이트 창 X 버튼 클릭 시", "플레이어가 업데이트 진행창을 X 버튼으로 닫으려는 경우\n\nWindows Recovery Assistant\n\n업데이트 창을 닫아도 설치 상태는 유지됩니다.\n\n진행이 멈춘 것처럼 보여도 잠시 기다려주세요.\n\n[확인]\n\n기록: UpdateWindowCloseAttempt += 1"),
            new NpcDialogueEntry(68, 3, "시스템 메시지 / 입력창", "업데이트 창 내부", "Windows Update\n\n업데이트를 준비하는 중...\n\n0%\n\n[취소]"),
            new NpcDialogueEntry(69, 3, "플레이어 선택 / 닫기 반응", "추가 취소 선택 시", "Windows Recovery Assistant\n\n업데이트 준비가 취소되었습니다.\n하지만 필요한 구성 요소가 아직 남아 있습니다.\n업데이트 확인을 다시 시작합니다.\n\n기록:\nUpdatePrepareCancelled += 1\nCancelCount += 1"),
            new NpcDialogueEntry(70, 3, "플레이어 선택 / 닫기 반응", "X 버튼 선택 시", "Windows Recovery Assistant\n업데이트 창이 닫혔습니다.\n준비 중인 업데이트는 아직 완료되지 않았습니다.\n다시 시작합니다.\n\n기록:\nUpdateWindowCloseAttempt += 1"),
            new NpcDialogueEntry(71, 3, "알림창 / Windows Recovery Assistant", "06. 알림창 - Windows Recovery Assistant", "알림창 - Windows Recovery Assistant\n업데이트 구성 요소가 활성화되었습니다.\n\n멈춘 패키지부터 차례대로 정리해주세요."),
            new NpcDialogueEntry(72, 3, "알림창 / Windows Recovery Assistant", "07. Update Patch 조각 | 파란 업데이트 패키지 아이콘이 변형된 기본 적. 여러 개가 몰려오는 느낌을", "Update Patch 조각\n파란 업데이트 패키지 아이콘이 변형된 기본 적. 여러 개가 몰려오는 느낌을 준다.\nLoading Bar Slime\n진행률 표시줄이 늘어졌다 줄어드는 적. 업데이트가 멈춘 듯한 답답함을 담당한다.\nRestart Reminder\n재시작 알림창 형태의 적. 팝업을 띄워 플레이어의 시야와 이동을 방해한다.\nFailed Update\n빨간 X가 붙은 업데이트 파일. 실패 후 재시도되는 느낌을 준다."),
            new NpcDialogueEntry(73, 3, "시스템 메시지 / 입력창", "08. 시스템 메시지", "시스템 메시지\n업데이트를 완료하려면 컴퓨터를 다시 시작해야 합니다.\n\n[지금 다시 시작] [15분 후 알림] [나중에]"),
            new NpcDialogueEntry(74, 3, "알림창 / Windows Recovery Assistant", "09. [지금 다시 시작] | 현재 전투 중이므로 사용할 수 없음. 시스템이 “작업 진행 중” 메시지를 띄운다.", "[지금 다시 시작]\nWindows Update\n현재 작업이 진행 중입니다.\n업데이트 구성이 완료된 후 다시 시작할 수 있습니다.\n(현재 전투 중이므로 사용할 수 없음. 시스템이 “작업 진행 중” 메시지를 띄운다.)\n\n[15분 후 알림]\nWindows Update\n다시 시작 알림을 15분 후로 연기했습니다.\n(Restart Reminder가 잠시 사라졌다가 일정 시간 뒤 다시 등장한다.)\n\n[나중에]\nWindows Update\n\n다시 시작을 나중에 진행합니다.\n알림창은 닫히지만 이후 더 많은 Restart Reminder가 나타날 수 있다.\n\n[X 버튼 클릭 시]\nWindows Recovery Assistant\n재시작 알림창이 닫혔습니다.\n\n하지만 다시 시작 요청은 아직 남아 있습니다.\n\n필요한 시점에 다시 표시됩니다."),
            new NpcDialogueEntry(75, 3, "시스템 메시지 / 입력창", "11. 시스템 메시지", "시스템 메시지\n업데이트 설치 실패\n\n일부 업데이트를 설치하지 못했습니다.\n\n[다시 시도] [나중에]"),
            new NpcDialogueEntry(76, 3, "시스템 메시지 / 입력창", "12. 시스템 메시지", "시스템 메시지\n업데이트를 다시 시도합니다...\n\n[■■■■■■■■□]"),
            new NpcDialogueEntry(77, 3, "시스템 메시지 / 입력창", "13. 시스템 메시지", "시스템 메시지\n업데이트 완료\n\n시스템 업데이트가 성공적으로 설치되었습니다.\n\n[확인]"),
            new NpcDialogueEntry(78, 3, "알림창 / Windows Recovery Assistant", "14. 알림창 - Windows Recovery Assistant", "Windows Recovery Assistant\n완료되었습니다!\n\n업데이트 상태가 정상으로 돌아왔어요.\n\n조금 오래 걸리긴 했지만요. 아하하!\n\n[확인]"),
            new NpcDialogueEntry(79, 3, "알림창 / Windows Recovery Assistant", "15. 알림창 - Windows Recovery Assistant", "Windows Recovery Assistant\n다음은 시스템 핵심 파일을 확인할 차례입니다.\n\n일부 보호된 영역에서 접근 제한 알림이 발생했어요.\n\n걱정하지 마세요. 필요한 부분만 확인하면 됩니다.\n\n[확인]"),
            new NpcDialogueEntry(80, 3, "시스템 메시지 / 입력창", "16. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(81, 3, "플레이어 선택 / 닫기 반응", "추가 01. 재시작 알림 선택지", "Windows Update\n\n업데이트를 완료하려면 다시 시작해야 합니다.\n\n[지금 다시 시작] [15분 후 알림] [나중에]\n\n[지금 다시 시작] 선택: 현재 복구 작업 중이라 즉시 재시작할 수 없습니다.\n[15분 후 알림] 선택: RestartPostponed += 1\n[나중에] 선택: UpdateReminder = Delayed"),
            new NpcDialogueEntry(82, 3, "알림창 / Windows Recovery Assistant", "추가 02. 나중에 선택 시 Assistant 반응", "Windows Recovery Assistant\n\n재시작 알림이 연기되었습니다.\n\n업데이트는 계속 보류 상태로 남습니다.\n\n[확인]"),
            new NpcDialogueEntry(83, 3, "플레이어 선택 / 닫기 반응", "추가 03. 업데이트 창 X 버튼 클릭 시", "플레이어가 업데이트 진행창을 X 버튼으로 닫으려는 경우\n\nWindows Recovery Assistant\n\n업데이트 창을 닫아도 설치 상태는 유지됩니다.\n\n진행이 멈춘 것처럼 보여도 잠시 기다려주세요.\n\n[확인]\n\n기록: UpdateWindowCloseAttempt += 1"),
            new NpcDialogueEntry(84, 4, "스테이지", "STAGE 04 보스 [System32 금지구역]", "스테이지 | STAGE 04 보스 [System32 금지구역]\n기존 대사/창 개수 | 22개\n추가 반영 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(85, 4, "시스템 메시지 / 입력창", "01. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(86, 4, "시스템 메시지 / 입력창", "02. 시스템 창", "시스템 창\nSystem32 Check.exe\n\n시스템 핵심 파일 정보를 불러오는 중...\n\n[■■■□□□□□□]"),
            new NpcDialogueEntry(87, 4, "알림창 / Windows Recovery Assistant", "03. 알림창", "알림창\nWindows Recovery Assistant\n\nSystem32 영역에 진입했습니다.\n\n이곳은 시스템 핵심 파일이 보관된 구역입니다.\n\n불필요한 조작은 권장되지 않습니다.\n\n[계속]"),
            new NpcDialogueEntry(88, 4, "알림창 / Windows Recovery Assistant", "04. 알림창", "알림창\nWindows Recovery Assistant\n\n핵심 파일 무결성 검사를 시작합니다.\n\n보호된 항목은 자동으로 건너뜁니다.\n\n잠시만 기다려주세요."),
            new NpcDialogueEntry(89, 4, "시스템 메시지 / 입력창", "05. 시스템 경고", "시스템 경고\n보호된 시스템 파일\n\n이 파일은 Windows가 정상적으로 작동하는 데 필요합니다.\n\n수정하거나 이동하지 않는 것이 좋습니다."),
            new NpcDialogueEntry(90, 4, "보스 대사 / 경고창", "06. 보안 경고", "보안 경고\nHigh-Kernel\n\n보호된 시스템 파일 접근이 감지되었습니다.\n\n접근 요청을 검토합니다."),
            new NpcDialogueEntry(91, 4, "보스 대사 / 경고창", "07. 보안 경고", "보안 경고\nHigh-Kernel\n\n요청 출처를 확인할 수 없습니다.\n\nSystem32 보호 절차를 시작합니다.\n\n[확인]"),
            new NpcDialogueEntry(92, 4, "알림창 / Windows Recovery Assistant", "08. 알림창", "알림창\nWindows Recovery Assistant\n\nHigh-Kernel이 보호 절차를 시작했습니다.\n\n시스템 핵심 파일에 접근하려면 먼저 보호막을 해제해야 합니다.\n\n신중하게 진행해주세요.\n\n[확인]"),
            new NpcDialogueEntry(93, 4, "보안 경고 / 권한 확인", "09. 시스템 메시지", "시스템 메시지\n접근 거부\n\n보호된 시스템 파일은 수정할 수 없습니다.\n\n[확인]"),
            new NpcDialogueEntry(94, 4, "보안 경고 / 권한 확인", "10. 시스템 확인창", "시스템 확인창\n시스템 확인\n\n이 작업을 계속하려면 추가 권한이 필요합니다.\n\n계속하시겠습니까?\n\n[허용] [취소]"),
            new NpcDialogueEntry(95, 4, "보스 대사 / 경고창", "11. 보안 경고", "보안 경고\nHigh-Kernel\n\n권한 확인 완료.\n\n그러나 요청 작업은 여전히 안전하지 않습니다.\n\n보호 절차를 계속합니다.\n\n[확인]"),
            new NpcDialogueEntry(96, 4, "알림창 / Windows Recovery Assistant", "12. 알림창", "알림창\nWindows Recovery Assistant\n\n추가 권한이 확인되었습니다.\n\n이제 보호막 일부를 해제할 수 있습니다.\n\n핵심 파일에는 직접 접근하지 않도록 주의해주세요.\n\n[확인]"),
            new NpcDialogueEntry(97, 4, "보스 대사 / 경고창", "13. 보안 경고", "보안 경고\nHigh-Kernel\n\n핵심 파일 보호 수준이 감소했습니다.\n\n시스템 안정성 저하 가능성이 있습니다.\n\n[확인]"),
            new NpcDialogueEntry(98, 4, "보스 대사 / 경고창", "14. 보안 경고", "보안 경고\nHigh-Kernel\n\n복구 명령과 삭제 명령은 구분되어야 합니다.\n\n현재 명령은 안전하지 않습니다.\n\n[확인]"),
            new NpcDialogueEntry(99, 4, "알림창 / Windows Recovery Assistant", "15. 알림창", "알림창\nWindows Recovery Assistant\n\nHigh-Kernel이 핵심 파일 보호를 우선하고 있습니다.\n\n하지만 보호 절차가 계속되면 복구가 지연됩니다.\n\n작업을 계속해주세요.\n\n[확인]"),
            new NpcDialogueEntry(100, 4, "시스템 메시지 / 입력창", "16. 상호작용 표시", "상호작용 표시\n[Enter] 무결성 확인"),
            new NpcDialogueEntry(101, 4, "보안 경고 / 권한 확인", "17. 시스템 확인창", "시스템 확인창\nSystem32 Integrity Check\n\n핵심 파일 무결성을 확인하시겠습니까?\n\n[확인] [취소]"),
            new NpcDialogueEntry(102, 4, "보안 경고 / 권한 확인", "18. 시스템 메시지", "시스템 메시지\nSystem32 Integrity Check\n\n핵심 파일 무결성 확인 완료.\n\n보호 절차가 정상 종료되었습니다.\n\n[확인]"),
            new NpcDialogueEntry(103, 4, "알림창 / Windows Recovery Assistant", "19. 알림창", "알림창\nWindows Recovery Assistant\n\nSystem32 무결성 검사가 완료되었습니다.\n\n핵심 파일 상태는 안정적입니다.\n\n복구 작업을 계속할 수 있습니다.\n\n[확인]"),
            new NpcDialogueEntry(104, 4, "알림창 / Windows Recovery Assistant", "20. 알림창", "알림창\nWindows Recovery Assistant\n\n다음은 네트워크 포트 상태를 확인할 차례입니다.\n\n외부 연결이 불안정해 보입니다.\n\n필요 없는 연결은 차단하는 것이 좋습니다.\n\n[확인]"),
            new NpcDialogueEntry(105, 4, "시스템 메시지 / 입력창", "21. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(106, 4, "보스 대사 / 경고창", "22. 핵심 감각", "핵심 감각\nWindows XP 탐색기 안의 System32 폴더에서\n시스템 핵심 파일을 보호하는 High-Kernel과 대치하는\n조용하고 차가운 첫 긴장 구간"),
            new NpcDialogueEntry(107, 4, "플레이어 선택 / 닫기 반응", "추가 01. 추가 권한 확인창 선택", "시스템 확인\n\n이 작업을 계속하려면 추가 권한이 필요합니다.\n\n계속하시겠습니까?\n\n[허용] [취소]\n\n[허용] 선택: AccessPermission = Allowed\n[취소] 선택: PermissionPrompt_Cancelled += 1"),
            new NpcDialogueEntry(108, 4, "알림창 / Windows Recovery Assistant", "추가 02. 취소 선택 시 Assistant 반응", "Windows Recovery Assistant\n\n작업이 보류되었습니다.\n\n하지만 무결성 검사를 완료하려면 접근 확인이 필요합니다.\n\n[다시 시도]"),
            new NpcDialogueEntry(109, 4, "플레이어 선택 / 닫기 반응", "추가 03. X 버튼 비활성화 안내", "플레이어가 System32 안내창을 닫으려는 경우\n\n시스템 메시지\n\n이 창은 보호 절차가 진행 중인 동안 닫을 수 없습니다.\n\n[확인]\n\n기록: ProtectedWindowCloseAttempt += 1"),
            new NpcDialogueEntry(110, 5, "스테이지", "STAGE 05 일반 [Network Port 항구]", "스테이지 | STAGE 05 일반 [Network Port 항구]\n기존 대사/창 개수 | 10개\n추가 반영 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(111, 5, "시스템 메시지 / 입력창", "01. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(112, 5, "시스템 메시지 / 입력창", "02. 시스템 창", "시스템 창\nNetwork Port.exe\n\n네트워크 연결 정보를 불러오는 중...\n\n[■■■■□□□□□]"),
            new NpcDialogueEntry(113, 5, "알림창 / Windows Recovery Assistant", "03. 알림창", "알림창\nWindows Recovery Assistant\n\nNetwork Port 항구에 진입했습니다.\n\n현재 여러 개의 외부 연결이 감지되고 있습니다.\n\n불필요한 연결은 시스템 안정성을 위해 차단하는 것이 좋습니다.\n\n[확인]"),
            new NpcDialogueEntry(114, 5, "알림창 / Windows Recovery Assistant", "04. 알림창", "알림창\nWindows Recovery Assistant\n\n연결 상태를 분류하는 중입니다.\n\n정상 패킷과 이상 패킷을 구분하여 처리해주세요.\n\n[확인]"),
            new NpcDialogueEntry(115, 5, "알림창 / Windows Recovery Assistant", "05. 알림창", "알림창\nWindows Recovery Assistant\n\n알 수 없는 연결은 안전하지 않을 수 있습니다.\n\n확실하지 않은 연결은 차단하는 편이 안전합니다.\n\n[확인]"),
            new NpcDialogueEntry(116, 5, "시스템 메시지 / 입력창", "06. 방화벽 게이트", "방화벽 게이트\n[차단] [허용] [나중에 결정]"),
            new NpcDialogueEntry(117, 5, "시스템 메시지 / 입력창", "07. 시스템 확인창", "시스템 확인창\nWindows Firewall\n\n알 수 없는 연결 요청이 감지되었습니다.\n\n이 연결을 어떻게 처리하시겠습니까?\n\n[차단] [허용] [나중에 결정]"),
            new NpcDialogueEntry(118, 5, "알림창 / Windows Recovery Assistant", "08. 알림창", "알림창\nWindows Recovery Assistant\n\n네트워크 연결 상태가 안정화되었습니다.\n\n불필요한 연결이 차단되었고, 패킷 흐름이 정상 범위로 돌아왔습니다.\n\n[확인]"),
            new NpcDialogueEntry(119, 5, "알림창 / Windows Recovery Assistant", "09. 알림창", "알림창\nWindows Recovery Assistant\n\n다음 구역에서 시스템 과부하 신호가 감지되었습니다.\n\n화면 출력 장치가 불안정해질 수 있습니다.\n\n침착하게 진행해주세요.\n\n[확인]"),
            new NpcDialogueEntry(120, 5, "시스템 메시지 / 입력창", "10. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(121, 5, "플레이어 선택 / 닫기 반응", "추가 01. 방화벽 게이트 선택지", "Firewall Gate\n\n알 수 없는 연결이 감지되었습니다.\n\n[차단] [허용] [나중에 결정]\n\n[차단] 선택: PortDecision = Blocked\n[허용] 선택: PortDecision = Allowed\n[나중에 결정] 선택: PortDecision = Deferred"),
            new NpcDialogueEntry(122, 5, "알림창 / Windows Recovery Assistant", "추가 02. 차단 선택 시 Assistant 반응", "Windows Recovery Assistant\n\n연결이 차단되었습니다.\n\n시스템 안정성이 개선될 수 있습니다.\n\n[확인]"),
            new NpcDialogueEntry(123, 5, "알림창 / Windows Recovery Assistant", "추가 03. 나중에 결정 선택 시 Assistant 반응", "Windows Recovery Assistant\n\n결정이 보류되었습니다.\n\n연결 상태가 임시 저장됩니다.\n\n[확인]"),
            new NpcDialogueEntry(124, 6, "스테이지", "STAGE 06 보스 [Blue Screen Tower]", "스테이지 | STAGE 06 보스 [Blue Screen Tower]\n기존 대사/창 개수 | 20개\n추가 반영 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(125, 6, "시스템 메시지 / 입력창", "01. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(126, 6, "시스템 메시지 / 입력창", "02. 시스템 창", "시스템 창\nSystem Stability Check\n\n시스템 안정성을 확인하는 중...\n\n[■■□□□□□□□]"),
            new NpcDialogueEntry(127, 6, "시스템 메시지 / 입력창", "03. 시스템 메시지", "시스템 메시지\n응답 지연이 감지되었습니다.\n검사를 계속합니다.\n\n[확인]"),
            new NpcDialogueEntry(128, 6, "알림창 / Windows Recovery Assistant", "04. Windows Recovery Assistant", "Windows Recovery Assistant\n시스템 안정성 검사를 시작합니다.\n\n일부 항목에서 과부하 신호가 감지되었습니다.\n\n검사를 계속합니다.\n\n[확인]"),
            new NpcDialogueEntry(129, 6, "알림창 / Windows Recovery Assistant", "05. Windows Recovery Assistant", "Windows Recovery Assistant\n응답 지연이 감지되었습니다.\n\n창을 닫지 마세요.\n\n복구 절차를 유지합니다.\n\n[확인]"),
            new NpcDialogueEntry(130, 6, "오류창 / STOP 메시지", "06. 블루스크린 메시지", "블루스크린 메시지\nA problem has been detected and the system has been stopped.\n\nSTOP: 0x0000007E\nMEMORY_CHECK_FAILED\nDRIVER_STATE_CONFLICT\nSYSTEM_THREAD_EXCEPTION"),
            new NpcDialogueEntry(131, 6, "시스템 메시지 / 입력창", "07. 블루스크린 메시지", "블루스크린 메시지\nA problem has been detected and the system has been stopped.\n\nDamage prevention mode has been activated."),
            new NpcDialogueEntry(132, 6, "알림창 / Windows Recovery Assistant", "08. Windows Recovery Assistant", "Windows Recovery Assistant\n오류가 발생했습니다.\n\n복구 절차를...\n\n계속합니다.\n\n[확인]"),
            new NpcDialogueEntry(133, 6, "시스템 메시지 / 입력창", "09. 보스 상태 표시", "보스 상태 표시\nBSOD / Blue Screen Sentinel\n\n상태: Damage Prevention Mode\nCrash Dump: 0%\nSystem Stability: 감소 중"),
            new NpcDialogueEntry(134, 6, "시스템 메시지 / 입력창", "10. 상호작용 표시", "상호작용 표시\n[Enter] 안정화"),
            new NpcDialogueEntry(135, 6, "오류창 / STOP 메시지", "11. STOP ERROR", "STOP ERROR\nSYSTEM_STABILITY_FAILURE\n\n작업을 계속할 수 없습니다."),
            new NpcDialogueEntry(136, 6, "오류창 / STOP 메시지", "12. STOP ERROR", "STOP ERROR\nSYSTEM_LOAD_EXCEEDED\n\n현재 작업으로 인해 시스템 부하가 증가했습니다."),
            new NpcDialogueEntry(137, 6, "알림창 / Windows Recovery Assistant", "13. Windows Recovery Assistant", "Windows Recovery Assistant\n복구 절차 유지 중...\n\n응답 지연...\n\n안정화 지점을 확인하세요.\n\n[확인]"),
            new NpcDialogueEntry(138, 6, "오류창 / STOP 메시지", "14. STOP ERROR", "STOP ERROR\nSYSTEM HALTED\n\n안정화 절차를 완료하십시오."),
            new NpcDialogueEntry(139, 6, "시스템 메시지 / 입력창", "15. 상호작용 표시", "상호작용 표시\n[Enter] 최종 안정화"),
            new NpcDialogueEntry(140, 6, "시스템 메시지 / 입력창", "16. System Recovery", "System Recovery\n충돌 회피 절차를 실행하시겠습니까?\n\n[안정화] [취소]"),
            new NpcDialogueEntry(141, 6, "시스템 메시지 / 입력창", "17. 시스템 메시지", "시스템 메시지\nSystem Recovery\n\n시스템 충돌은 일시적으로 회피되었습니다.\n\n재부팅이 보류되었습니다.\n\n[확인]"),
            new NpcDialogueEntry(142, 6, "알림창 / Windows Recovery Assistant", "18. Windows Recovery Assistant", "Windows Recovery Assistant\n시스템 충돌은 일시적으로 회피되었습니다.\n\n원인 추적을 위해 기록 영역을 확인해야 합니다.\n\nRegistry Hive로 이동합니다.\n\n[확인]"),
            new NpcDialogueEntry(143, 6, "시스템 메시지 / 입력창", "19. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(144, 6, "시스템 메시지 / 입력창", "20. 다음 스테이지", "다음 스테이지\nSTAGE 07 일반\n[Registry Hive 보관소]"),
            new NpcDialogueEntry(145, 6, "플레이어 선택 / 닫기 반응", "추가 01. 강제 재시작 경고 선택지", "The system will restart to prevent damage.\n\n[Restart Now] [Wait]\n\n[Restart Now] 선택: 현재 세션이 강제 종료될 위험이 있어 Assistant가 중단시킴\n[Wait] 선택: 시간을 벌지만 CPU Load가 상승함\n\n기록: ForcedRestartChoice = RestartNow / Wait"),
            new NpcDialogueEntry(146, 6, "알림창 / Windows Recovery Assistant", "추가 02. Wait 선택 시 Assistant 반응", "Windows Recovery Assistant\n\n재시작을 지연합니다.\n\n시스템 부하가 증가할 수 있습니다.\n\n안정화 절차를 서둘러주세요.\n\n[확인]"),
            new NpcDialogueEntry(147, 6, "플레이어 선택 / 닫기 반응", "추가 03. 블루스크린 위 알림창 닫기 시", "플레이어가 깨진 Assistant 알림창을 닫으려는 경우\n\nWindows Recovery Assistant\n\n창을 닫지 마세요.\n\n복구 절차를 유지합니다.\n\n[확인]\n\n기록: CrashNoticeCloseAttempt += 1"),
            new NpcDialogueEntry(148, 7, "스테이지", "STAGE 07 일반 [Registry Hive 보관소]", "스테이지 | STAGE 07 일반 [Registry Hive 보관소]\n기존 대사/창 개수 | 13개\n추가 반영 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(149, 7, "시스템 메시지 / 입력창", "01. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(150, 7, "시스템 메시지 / 입력창", "02. 시스템 창", "시스템 창\nRegistry Editor\n\n레지스트리 데이터를 불러오는 중...\n\n[■■□□□□□□□]"),
            new NpcDialogueEntry(151, 7, "알림창 / Windows Recovery Assistant", "03. Windows Recovery Assistant", "Windows Recovery Assistant\nRegistry Hive에 진입했습니다.\n\n이곳에는 시스템 설정값과 실행 기록이 저장되어 있습니다.\n\n값을 임의로 수정하지 않도록 주의해주세요.\n\n[확인]"),
            new NpcDialogueEntry(152, 7, "알림창 / Windows Recovery Assistant", "04. Windows Recovery Assistant", "Windows Recovery Assistant\n기록은 복구에 필요합니다.\n\n삭제하지 말고 확인만 진행해주세요.\n\n[확인]"),
            new NpcDialogueEntry(153, 7, "시스템 메시지 / 입력창", "05. 선택 버튼 예시", "선택 버튼 예시\n[정리] [검사] [보관] [무시]"),
            new NpcDialogueEntry(154, 7, "알림창 / Windows Recovery Assistant", "06. Windows Recovery Assistant", "Windows Recovery Assistant\n복구 과정 중 생성된 기록입니다.\n\n문제가 있는 값만 정리하면 됩니다.\n\n[확인]"),
            new NpcDialogueEntry(155, 7, "알림창 / Windows Recovery Assistant", "07. Windows Recovery Assistant", "Windows Recovery Assistant\n프로필 정보가 확인되었습니다.\n\n복구 절차를 계속합니다.\n\n[확인]"),
            new NpcDialogueEntry(156, 7, "시스템 메시지 / 입력창", "08. Registry Value Check", "Registry Value Check\nBroken Key가 발견되었습니다.\n\n처리 방식을 선택하세요.\n\n[정리] [검사] [보관]"),
            new NpcDialogueEntry(157, 7, "알림창 / Windows Recovery Assistant", "09. Windows Recovery Assistant", "Windows Recovery Assistant\n이 값은 복구 기록에 사용됩니다.\n\n삭제하지 않는 것이 좋습니다.\n\n[확인]"),
            new NpcDialogueEntry(158, 7, "시스템 메시지 / 입력창", "10. Registry Backup", "Registry Backup\n변경 전 레지스트리 상태를 백업하시겠습니까?\n\n[예] [아니오]"),
            new NpcDialogueEntry(159, 7, "시스템 메시지 / 입력창", "11. 시스템 메시지", "시스템 메시지\nRegistry Backup\n\n백업 파일이 생성되었습니다.\n\nBackup_RecoveryProfile_입력한이름.reg\n\n[확인]"),
            new NpcDialogueEntry(160, 7, "알림창 / Windows Recovery Assistant", "12. Windows Recovery Assistant", "Windows Recovery Assistant\nRegistry Hive 정리가 완료되었습니다.\n\n설정 기록은 안정화되었습니다.\n\n하지만 일부 예외 메시지가 아직 남아 있습니다.\n\n[확인]"),
            new NpcDialogueEntry(161, 7, "시스템 메시지 / 입력창", "13. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(162, 7, "플레이어 선택 / 닫기 반응", "추가 01. 레지스트리 값 선택지", "Broken Key 발견\n\n[정리] [보관] [검사]\n\n[정리] 선택: RegistryCleanCount += 1\n[보관] 선택: RegistryKeepCount += 1\n[검사] 선택: RegistryInspectCount += 1"),
            new NpcDialogueEntry(163, 7, "알림창 / Windows Recovery Assistant", "추가 02. Recent Trace 삭제 시도", "플레이어가 Recent Trace를 정리하려는 경우\n\nWindows Recovery Assistant\n\n이 값은 복구 기록에 사용됩니다.\n\n삭제하지 않는 것이 좋습니다.\n\n[확인]\n\n기록: RecentTraceDeleteAttempt += 1"),
            new NpcDialogueEntry(164, 7, "행동 기록 / 후반 회수", "추가 03. 레지스트리에 표시되는 행동 기록", "RecentActions\n\nUpdateReminder = Delayed\nAccessPermission = Allowed\nPortDecision = Blocked\nWindowCloseAttempt = 누적값\nErrorReport = Skipped 또는 Deferred\nProfileName = 입력한 이름"),
            new NpcDialogueEntry(165, 8, "스테이지", "STAGE 08 보스 [Popup Error 미궁]", "스테이지 | STAGE 08 보스 [Popup Error 미궁]\n기존 대사/창 개수 | 22개\n추가 반영 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(166, 8, "보스 대사 / 경고창", "01. 스테이지 한 줄 요약", "스테이지 한 줄 요약\nStage 8은 플레이어가 지금까지 닫고 무시했던 오류창들이 Exception Queen으로 되돌아오는 UI 붕괴형 보스전이다."),
            new NpcDialogueEntry(167, 8, "시스템 메시지 / 입력창", "02. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(168, 8, "시스템 메시지 / 입력창", "03. ! 시스템 메시지", "! 시스템 메시지\nException Report\n\n처리되지 않은 오류 기록을 불러오는 중...\n[■■■■□□□]"),
            new NpcDialogueEntry(169, 8, "알림창 / Windows Recovery Assistant", "04. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n오류창이 다수 발생했습니다.\n순서대로 닫아주세요.\n창을 닫으면 해결됩니다.\n\n[확인]"),
            new NpcDialogueEntry(170, 8, "알림창 / Windows Recovery Assistant", "05. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n오류창이 계속 생성되고 있습니다.\n닫지 않은 오류는 복구를 방해합니다.\n모두 닫아주세요.\n\n[확인]"),
            new NpcDialogueEntry(171, 8, "오류창 / STOP 메시지", "06. X 오류창 / 보스 대사", "X 오류창 / 보스 대사\nUnhandled Exception\n\n처리되지 않은 오류가 누적되었습니다.\n닫힌 창을 복원합니다.\n\n[확인]"),
            new NpcDialogueEntry(172, 8, "보스 대사 / 경고창", "07. X 오류창 / 보스 대사", "X 오류창 / 보스 대사\nException Queen\n\n또 닫으려고?\n항상 그렇게 했잖아.\n\n[확인]"),
            new NpcDialogueEntry(173, 8, "보스 대사 / 경고창", "08. 보스 상태", "보스 상태\nException Queen\nHP: 미정\n상태: 처리되지 않은 오류 누적"),
            new NpcDialogueEntry(174, 8, "오류창 / STOP 메시지", "09. X 오류창 / 보스 대사", "X 오류창 / 보스 대사\n응용 프로그램 오류\n\n알 수 없는 예외가 발생했습니다.\n\n[확인]"),
            new NpcDialogueEntry(175, 8, "시스템 메시지 / 입력창", "10. 버튼 변화 예시", "버튼 변화 예시\n[확인] [취소]\n-> [확인] [확인]\n-> [닫기] [닫지 않음]"),
            new NpcDialogueEntry(176, 8, "알림창 / Windows Recovery Assistant", "11. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n오류창이 너무 많습니다.\n닫아주세요.\n\n[확인]"),
            new NpcDialogueEntry(177, 8, "알림창 / Windows Recovery Assistant", "12. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n닫아주세요.\n닫아주세요.\n닫아주세요.\n\n[확인]"),
            new NpcDialogueEntry(178, 8, "보스 대사 / 경고창", "13. X 오류창 / 보스 대사", "X 오류창 / 보스 대사\nException Queen\n\n또 닫네.\n그래서 우리가 여기까지 왔잖아.\n\n[확인]"),
            new NpcDialogueEntry(179, 8, "시스템 메시지 / 입력창", "14. ! 시스템 메시지", "! 시스템 메시지\n오류 보고\n\n이 문제를 보고하시겠습니까?\n\n[보고함] [보고하지 않음] [나중에]"),
            new NpcDialogueEntry(180, 8, "알림창 / Windows Recovery Assistant", "15. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n오류창을 닫아주세요.\n복구를 계속해야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(181, 8, "보스 대사 / 경고창", "16. X 오류창 / 보스 대사", "X 오류창 / 보스 대사\nException Queen\n\n닫으면 해결된다고?\n누가 그렇게 가르쳤지?\n\n[확인]"),
            new NpcDialogueEntry(182, 8, "오류창 / STOP 메시지", "17. 중심 오류창", "중심 오류창\nUnhandled_Exception_Core\n또는\nExceptionQueen.exe"),
            new NpcDialogueEntry(183, 8, "시스템 메시지 / 입력창", "18. 상호작용 표시", "상호작용 표시\n[Enter] 오류 격리"),
            new NpcDialogueEntry(184, 8, "시스템 메시지 / 입력창", "19. ! 시스템 메시지", "! 시스템 메시지\nException Report\n\n처리되지 않은 오류를 격리하시겠습니까?\n\n[격리] [취소]"),
            new NpcDialogueEntry(185, 8, "시스템 메시지 / 입력창", "20. 남는 파일", "남는 파일\nUNSENT_REPORT.tmp"),
            new NpcDialogueEntry(186, 8, "알림창 / Windows Recovery Assistant", "21. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n오류창 격리가 완료되었습니다.\n일부 임시 보고서가 캐시에 남아 있습니다.\nTemp Cache를 확인해야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(187, 8, "알림창 / Windows Recovery Assistant", "22. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n남은 임시파일을 정리하면 복구를 계속할 수 있습니다.\n이동해주세요.\n\n[확인]"),
            new NpcDialogueEntry(188, 8, "플레이어 선택 / 닫기 반응", "추가 01. 오류창 X 버튼 역효과", "플레이어가 오류창 X 버튼을 누른 경우\n\nException Queen\n\n또 닫네.\n\n그래서 우리가 여기까지 왔잖아.\n\n[확인]\n\n연출: 닫힌 오류창 위치에 새로운 오류창 2개가 생성됨\n기록: ErrorWindowClosed += 1"),
            new NpcDialogueEntry(189, 8, "플레이어 선택 / 닫기 반응", "추가 02. 오류 보고 선택지", "오류 보고\n\n이 문제를 보고하시겠습니까?\n\n[보고함] [보고하지 않음] [나중에]\n\n[보고함] 선택: Exception Queen - 이제 와서?\n[보고하지 않음] 선택: Exception Queen - 늘 그렇지.\n[나중에] 선택: Exception Queen - 좋아하는 말이네.\n\n기록: ErrorReport = Sent / Skipped / Deferred"),
            new NpcDialogueEntry(190, 8, "알림창 / Windows Recovery Assistant", "추가 03. Assistant 반복 지시", "Windows Recovery Assistant\n\n오류창을 닫아주세요.\n\n복구를 계속해야 합니다.\n\n[확인]\n\n반복 발생 시:\n\nWindows Recovery Assistant\n\n닫아주세요.\n\n닫아주세요.\n\n닫아주세요.\n\n[확인]"),
            new NpcDialogueEntry(191, 9, "스테이지", "STAGE 09 일반 [Temp Cache 동굴]", "스테이지 | STAGE 09 일반 [Temp Cache 동굴]\n기존 대사/창 개수 | 22개\n추가 반영 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(192, 9, "시스템 메시지 / 입력창", "01. 상호작용 표시", "상호작용 표시\n[Enter] 실행"),
            new NpcDialogueEntry(193, 9, "시스템 메시지 / 입력창", "02. ! 시스템 메시지", "! 시스템 메시지\nTemp Cache.exe\n\n임시 저장소를 불러오는 중...\nTemporary Files...\nErrorReport...\nRecent...\nCache...\n\n[■■■■□□□]"),
            new NpcDialogueEntry(194, 9, "알림창 / Windows Recovery Assistant", "03. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n임시 저장소에 남은 항목이 많습니다.\n불필요한 항목은 삭제해주세요.\n\n[확인]"),
            new NpcDialogueEntry(195, 9, "알림창 / Windows Recovery Assistant", "04. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n열어보지 않아도 됩니다.\n삭제하면 정리됩니다.\n\n[확인]"),
            new NpcDialogueEntry(196, 9, "알림창 / Windows Recovery Assistant", "05. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n남은 기록은 복구를 지연시킵니다.\n기록을 줄여야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(197, 9, "시스템 메시지 / 입력창", "06. ! 시스템 메시지", "! 시스템 메시지\n파일 처리\n\n이 항목을 삭제하시겠습니까?\n\n[삭제] [보관] [열기]"),
            new NpcDialogueEntry(198, 9, "시스템 메시지 / 입력창", "07. 파일명", "파일명\nUNSENT_REPORT.tmp"),
            new NpcDialogueEntry(199, 9, "시스템 메시지 / 입력창", "08. ! 시스템 메시지", "! 시스템 메시지\n오류 보고서\n\n이전에 닫힌 오류 보고서가 전송되지 않았습니다.\n보고서를 확인하시겠습니까?\n\n[열기] [삭제] [나중에]"),
            new NpcDialogueEntry(200, 9, "오류창 / STOP 메시지", "09. 선택 | 결과", "선택\n결과\n[열기]\nStage 8에서 닫았던 오류창 대사 일부가 다시 나타난다.\n[삭제]\n파일이 사라지는 듯하다가 이름만 바뀌어 복사본이 생성된다.\n[나중에]\n보고서가 임시 저장소에 계속 남는다는 Assistant 알림이 뜬다."),
            new NpcDialogueEntry(201, 9, "시스템 메시지 / 입력창", "10. X 경고창 / 오류 기록", "X 경고창 / 오류 기록\n오류 보고서 내용\n\n닫힌 창은 사라지지 않아.\n뒤로 밀려날 뿐이야.\n\n[확인]"),
            new NpcDialogueEntry(202, 9, "시스템 메시지 / 입력창", "11. ! 시스템 메시지", "! 시스템 메시지\n파일명 변경\n\nUNSENT_REPORT.tmp\n→ UNSENT_REPORT_복사본.tmp"),
            new NpcDialogueEntry(203, 9, "알림창 / Windows Recovery Assistant", "12. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n확인하지 않은 보고서는 임시 저장소에 남습니다.\n정리를 계속합니다.\n\n[확인]"),
            new NpcDialogueEntry(204, 9, "시스템 메시지 / 입력창", "13. 파일명", "파일명\nRecoveryProfile.cache"),
            new NpcDialogueEntry(205, 9, "시스템 메시지 / 입력창", "14. ! 시스템 메시지", "! 시스템 메시지\nRecoveryProfile.cache\n\nProfileName = 입력한 이름\nLastSession = Not Closed Properly\nRecentCommand = Close / Delay / Allow / Block\n\n[확인]"),
            new NpcDialogueEntry(206, 9, "알림창 / Windows Recovery Assistant", "15. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n복구 프로필 캐시가 확인되었습니다.\n불필요한 항목을 정리합니다.\n\n[확인]"),
            new NpcDialogueEntry(207, 9, "시스템 메시지 / 입력창", "16. ! 시스템 메시지", "! 시스템 메시지\n툴팁\n\n최근 실행됨"),
            new NpcDialogueEntry(208, 9, "시스템 메시지 / 입력창", "17. ! 시스템 메시지", "! 시스템 메시지\n툴팁\n\n처리 완료"),
            new NpcDialogueEntry(209, 9, "알림창 / Windows Recovery Assistant", "18. 파일명 | 의미", "파일명\n의미\ncache_Driver-K.tmp\nStage 2 보스 기록\nreport_High-Kernel.tmp\nStage 4 보스 기록\ndump_BSOD.tmp\nStage 6 보스 기록\nexception_ExceptionQueen.tmp\nStage 8 보스 기록\nprofile_입력한 이름.cache\nStage 1 복구 프로필 기록\nassistant_recovery.tmp\nWindows Recovery Assistant 관련 기록"),
            new NpcDialogueEntry(210, 9, "시스템 메시지 / 입력창", "19. 상호작용 표시", "상호작용 표시\n[Enter] 삭제 항목 이동"),
            new NpcDialogueEntry(211, 9, "시스템 메시지 / 입력창", "20. ! 시스템 메시지", "! 시스템 메시지\nCache 정리 완료\n\n삭제된 항목이 Recycle Bin으로 이동되었습니다.\n\n[확인]"),
            new NpcDialogueEntry(212, 9, "알림창 / Windows Recovery Assistant", "21. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n임시 저장소 정리가 완료되었습니다.\n삭제된 항목은 Recycle Bin에 보관됩니다.\n최종 정리를 진행해야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(213, 9, "시스템 메시지 / 입력창", "22. 다음 스테이지", "다음 스테이지\nSTAGE 10 최종\n[Recycle Bin 던전]"),
            new NpcDialogueEntry(214, 9, "플레이어 선택 / 닫기 반응", "추가 01. 미전송 오류 보고서 선택지", "오류 보고서\n\n이전에 닫힌 오류 보고서가 전송되지 않았습니다.\n\n보고서를 확인하시겠습니까?\n\n[열기] [삭제] [나중에]\n\n[열기] 선택: UnsentReport = Opened\n[삭제] 선택: UnsentReport = Deleted\n[나중에] 선택: UnsentReport = Deferred"),
            new NpcDialogueEntry(215, 9, "시스템 메시지 / 입력창", "추가 02. 삭제 선택 시 파일명 변경", "UNSENT_REPORT.tmp\n\n파일이 삭제되었습니다.\n\n복사본을 생성하는 중...\n\nUNSENT_REPORT.tmp\n→ UNSENT_REPORT_복사본.tmp\n\n기록: DeleteCount += 1"),
            new NpcDialogueEntry(216, 9, "알림창 / Windows Recovery Assistant", "추가 03. 나중에 선택 시 Assistant 반응", "Windows Recovery Assistant\n\n확인하지 않은 보고서는 임시 저장소에 남습니다.\n\n정리를 계속합니다.\n\n[확인]"),
            new NpcDialogueEntry(217, 9, "행동 기록 / 후반 회수", "추가 04. 복구 프로필 캐시", "RecoveryProfile.cache\n\nProfileName = 입력한 이름\nLastSession = Not Closed Properly\nRecentCommand = Close / Delay / Allow / Block\n\n기록: ProfileCacheFound = True"),
            new NpcDialogueEntry(218, 10, "스테이지", "STAGE 10 최종 [Recycle Bin 던전]", "스테이지 | STAGE 10 최종 [Recycle Bin 던전]\n기존 대사/창 개수 | 35개\n추가 반영 | X 버튼, 취소, 나중에, 삭제, 허용 등 플레이어 행동 반응 및 기록 대사"),
            new NpcDialogueEntry(219, 10, "시스템 메시지 / 입력창", "01. ! 시스템 메시지", "! 시스템 메시지\n상호작용 표시\n\n[Enter] Recycle Bin 열기"),
            new NpcDialogueEntry(220, 10, "알림창 / Windows Recovery Assistant", "02. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n이제 마지막 정리만 남았습니다.\n휴지통을 비우면 복구 절차가 완료됩니다.\n\n[확인]"),
            new NpcDialogueEntry(221, 10, "보스 대사 / 경고창", "03. 구간 | 회수 요소", "구간\n회수 요소\nStage 1\n복구 프로필 이름, 새 폴더, 진짜최종 파일, 휴지통 비우기\nStage 2\nDriver-K, Unknown Device, 드라이버 충돌 기록\nStage 3\n업데이트 연기, 나중에, 15분 후 알림, 재시작 지연\nStage 4\n추가 권한 허용, System32 접근 기록, High-Kernel\nStage 5\n차단된 연결, PortBlocked, localhost 또는 unknown-port 기록\nStage 6\nCrash Dump, BSOD, System Stability Failure\nStage 7\nProfileName, RecentActions, LastSession\nStage 8\n확인 / 취소 / 나중에 / 닫기, Exception Queen, UNSENT_REPORT\nStage 9\n임시파일, 캐시, 삭제된 항목이 Recycle Bin으로 이동됨, 보스 이름 후보들"),
            new NpcDialogueEntry(222, 10, "보스 대사 / 경고창", "04. X 경고창 / 오류 메시지", "X 경고창 / 오류 메시지\nIllegal_Binny\n\n비워진 적 없는 항목이 너무 많습니다.\n삭제된 것들은 사라지지 않습니다.\n보관될 뿐입니다.\n\n[확인]"),
            new NpcDialogueEntry(223, 10, "보스 대사 / 경고창", "05. X 경고창 / 오류 메시지", "X 경고창 / 오류 메시지\nIllegal_Binny\n\n너는 계속 정리했습니다.\n계속 닫았습니다.\n계속 미뤘습니다.\n이제 무엇을 비울 건가요?\n\n[확인]"),
            new NpcDialogueEntry(224, 10, "보스 대사 / 경고창", "06. ! 시스템 메시지", "! 시스템 메시지\n보스 상태\n\nIllegal_Binny\nHP: 미정\n상태: 삭제 항목 과부하"),
            new NpcDialogueEntry(225, 10, "시스템 메시지 / 입력창", "07. ! 시스템 메시지", "! 시스템 메시지\n상호작용 표시\n\n[Enter] 격리"),
            new NpcDialogueEntry(226, 10, "보스 대사 / 경고창", "08. ! 시스템 메시지", "! 시스템 메시지\nRecycle Bin\n\nIllegal_Binny.dat 항목을 격리하시겠습니까?\n\n[격리] [취소]"),
            new NpcDialogueEntry(227, 10, "시스템 메시지 / 입력창", "09. ! 시스템 메시지", "! 시스템 메시지\n시스템 입력창\n\n삭제할 프로세스 이름을 입력하십시오.\n[________________]\n\n[실행]"),
            new NpcDialogueEntry(228, 10, "알림창 / Windows Recovery Assistant", "10. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n삭제 대상을 입력하세요.\n정확한 이름을 입력해야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(229, 10, "알림창 / Windows Recovery Assistant", "11. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n잠시만요.\n그 이름은 입력하지 않아도 됩니다.\n다른 대상을 선택하세요.\n\n[확인]"),
            new NpcDialogueEntry(230, 10, "알림창 / Windows Recovery Assistant", "12. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n그 이름은 대상이 아닙니다.\n다시 입력하세요.\n다시 입력하세요.\n다시 입력하세요.\n\n[확인]"),
            new NpcDialogueEntry(231, 10, "알림창 / Windows Recovery Assistant", "13. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n종료하면 안 됩니다.\n종료하면 복구를 계속할 수 없습니다.\n당신은 아직 여기 있어야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(232, 10, "알림창 / Windows Recovery Assistant", "14. 엔딩 | 입력 조건 | 의미", "엔딩\n입력 조건\n의미\n진엔딩\nStage 1에서 입력한 복구 프로필 이름\n플레이어가 현재 세션의 원인임을 받아들이고 종료를 선택함\n일반 엔딩\n보스 이름\n문제를 외부 대상으로 돌리고 해당 보스만 종료함\nAssistant 루프 엔딩\nWindows Recovery Assistant\n도우미를 삭제하려 하지만 보호되어 실패하고 루프\n잘못된 입력 엔딩\n빈칸 또는 존재하지 않는 이름\n대상을 찾지 못하고 복구 절차가 계속됨"),
            new NpcDialogueEntry(233, 10, "시스템 메시지 / 입력창", "15. ! 시스템 메시지", "! 시스템 메시지\n진엔딩 조건\n\n플레이어가 Stage 1에서 입력한 복구 프로필 이름을 입력한다.\n문서에서는 실제 이름 예시를 쓰지 않고 “입력한 이름”으로 표기한다."),
            new NpcDialogueEntry(234, 10, "시스템 메시지 / 입력창", "16. ! 시스템 메시지", "! 시스템 메시지\n시스템 메시지\n\n입력값 확인 중...\n프로필 이름과 현재 세션이 일치합니다.\n삭제 대상:\n현재 세션\n정말 종료하시겠습니까?\n\n[예] [아니오]"),
            new NpcDialogueEntry(235, 10, "알림창 / Windows Recovery Assistant", "17. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n아니요.\n그 항목은 삭제 대상이 아닙니다.\n복구 절차를 계속해야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(236, 10, "시스템 메시지 / 입력창", "18. ! 시스템 메시지", "! 시스템 메시지\n시스템 메시지\n\n현재 세션 종료 시 복구 절차가 중단됩니다.\n계속하시겠습니까?\n\n[예] [아니오]"),
            new NpcDialogueEntry(237, 10, "시스템 메시지 / 입력창", "19. ! 시스템 메시지", "! 시스템 메시지\n마지막 시스템 메시지\n\n현재 세션이 종료되었습니다.\n남은 항목은 더 이상 실행되지 않습니다."),
            new NpcDialogueEntry(238, 10, "보스 대사 / 경고창", "20. ! 시스템 메시지", "! 시스템 메시지\n일반 엔딩 조건\n\n플레이어가 보스 이름을 입력한다.\n예시 후보: Driver-K, High-Kernel, BSOD, Exception Queen, Illegal_Binny"),
            new NpcDialogueEntry(239, 10, "시스템 메시지 / 입력창", "21. ! 시스템 메시지", "! 시스템 메시지\n시스템 메시지\n\n입력값 확인 중...\n대상 프로세스 발견:\n입력한 보스 이름\n프로세스를 종료합니다.\n\n[확인]"),
            new NpcDialogueEntry(240, 10, "알림창 / Windows Recovery Assistant", "22. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n복구가 완료되었습니다.\n문제가 되는 항목이 제거되었습니다.\n시스템을 다시 검사합니다.\n\n[확인]"),
            new NpcDialogueEntry(241, 10, "시스템 메시지 / 입력창", "23. ! 시스템 메시지", "! 시스템 메시지\n마지막 시스템 메시지\n\n복구 절차를 다시 시작합니다."),
            new NpcDialogueEntry(242, 10, "알림창 / Windows Recovery Assistant", "24. ! 시스템 메시지", "! 시스템 메시지\nAssistant 루프 엔딩 조건\n\n플레이어가 Windows Recovery Assistant를 입력한다."),
            new NpcDialogueEntry(243, 10, "알림창 / Windows Recovery Assistant", "25. ! 시스템 메시지", "! 시스템 메시지\n시스템 메시지\n\n입력값 확인 중...\n대상:\nWindows Recovery Assistant\n삭제 권한을 확인하는 중..."),
            new NpcDialogueEntry(244, 10, "시스템 메시지 / 입력창", "26. ! 시스템 메시지", "! 시스템 메시지\n시스템 메시지\n\n삭제 실패.\n해당 프로세스는 복구 절차에 의해 보호되고 있습니다."),
            new NpcDialogueEntry(245, 10, "알림창 / Windows Recovery Assistant", "27. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n저를 종료하려고 하셨나요?\n괜찮습니다.\n혼란이 있었던 것 같네요.\n복구를 처음부터 다시 진행하겠습니다.\n\n[확인]"),
            new NpcDialogueEntry(246, 10, "시스템 메시지 / 입력창", "28. ! 시스템 메시지", "! 시스템 메시지\n잘못된 입력 조건\n\n플레이어가 존재하지 않는 이름, 빈칸, 오타, 인식할 수 없는 문자열을 입력한다."),
            new NpcDialogueEntry(247, 10, "시스템 메시지 / 입력창", "29. ! 시스템 메시지", "! 시스템 메시지\n시스템 메시지\n\n대상을 찾을 수 없습니다.\n삭제할 수 없습니다.\n복구 절차를 계속합니다."),
            new NpcDialogueEntry(248, 10, "알림창 / Windows Recovery Assistant", "30. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n정확한 이름을 입력해야 합니다.\n대상을 다시 선택해주세요.\n\n[확인]"),
            new NpcDialogueEntry(249, 10, "알림창 / Windows Recovery Assistant", "31. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n이제 마지막 정리만 남았습니다.\n휴지통을 비우면 복구가 완료됩니다.\n\n[확인]"),
            new NpcDialogueEntry(250, 10, "알림창 / Windows Recovery Assistant", "32. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n불필요한 항목을 삭제하세요.\n삭제하세요.\n삭제하세요.\n\n[확인]"),
            new NpcDialogueEntry(251, 10, "알림창 / Windows Recovery Assistant", "33. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n그 이름은 대상이 아닙니다.\n다시 입력하세요.\n다시 입력하세요.\n다시 입력하세요.\n\n[확인]"),
            new NpcDialogueEntry(252, 10, "알림창 / Windows Recovery Assistant", "34. ▣ 알림창 - Windows Recovery Assistant", "▣ 알림창 - Windows Recovery Assistant\nWindows Recovery Assistant\n\n종료하면 안 됩니다.\n종료하면 복구를 계속할 수 없습니다.\n당신은 아직 여기 있어야 합니다.\n\n[확인]"),
            new NpcDialogueEntry(253, 10, "시스템 메시지 / 입력창", "35. 핵심 감각", "핵심 감각\nRecycle Bin 던전 안에서\n플레이어가 지금까지 삭제한 모든 흔적과 마주하고\n직접 삭제할 대상을 입력하여\n자각 또는 루프 엔딩으로 갈라지는 최종 스테이지"),
            new NpcDialogueEntry(254, 10, "행동 기록 / 후반 회수", "추가 01. 최종 입력 전 행동 기록 표시", "Recent Actions\n\nCloseAttempt = 누적값\nUpdateReminder = Delayed\nAccessPermission = Allowed\nPortDecision = Blocked 또는 Deferred\nErrorReport = Deferred 또는 Skipped\nUnsentReport = Deleted 또는 Deferred\nProfileName = 입력한 이름"),
            new NpcDialogueEntry(255, 10, "시스템 메시지 / 입력창", "추가 02. 최종 삭제 대상 입력창", "시스템 입력창\n\n삭제할 프로세스 이름을 입력하십시오.\n\n[________________]\n\n[실행]\n\n입력값에 따라 진엔딩, 일반 엔딩, Assistant 루프 엔딩, 회피 루프 엔딩으로 분기한다."),
            new NpcDialogueEntry(256, 10, "알림창 / Windows Recovery Assistant", "추가 03. 회피 루프 엔딩 조건 추가", "조건 예시\n\nFinalInput이 빈칸이거나 존재하지 않는 이름\n또는 DelayCount / ReportSkipped / Deferred 선택이 일정 수 이상 누적된 경우\n\nWindows Recovery Assistant\n\n결정이 보류되었습니다.\n\n복구 절차를 다시 예약합니다.\n\n[15분 후 다시 알림]\n\n의미: 플레이어가 끝까지 결정을 미루고 루프로 돌아감"),
            new NpcDialogueEntry(257, 10, "행동 기록 / 후반 회수", "행동 기록 관리", "기록할 행동\n\nCloseAttempt\nCancelCount\nDelayCount\nPermissionAllowed\nPortDecision\nErrorReport\nUnsentReport\nDeleteCount\nProfileNameEntered\nFinalInput\n\n이 기록은 즉시 큰 변화를 만들지 않고, Stage 7 이후 레지스트리와 임시파일, Stage 10 최종 입력창에서 회수한다."),
            new NpcDialogueEntry(258, 10, "시스템 메시지 / 입력창", "진엔딩 조건", "조건\nFinalInput == Stage 1에서 입력한 복구 프로필 이름\n\n의미\n플레이어가 문제를 외부 보스가 아니라 현재 세션과 자신에게서 찾는다.\n\n시스템 메시지\n\n프로필 이름과 현재 세션이 일치합니다.\n삭제 대상: 현재 세션\n정말 종료하시겠습니까?\n\n[예] [아니오]"),
            new NpcDialogueEntry(259, 10, "시스템 메시지 / 입력창", "일반 엔딩 조건", "조건\nFinalInput == 보스 이름\n\n예시 후보\nDriver-K / High-Kernel / BSOD / Exception Queen / Illegal_Binny\n\n의미\n플레이어가 문제를 외부 대상으로 돌린다.\n해당 보스만 종료되고 복구 절차는 다시 시작된다."),
            new NpcDialogueEntry(260, 10, "알림창 / Windows Recovery Assistant", "Assistant 루프 엔딩 조건", "조건\nFinalInput == Windows Recovery Assistant\n\n시스템 메시지\n\n삭제 실패.\n해당 프로세스는 복구 절차에 의해 보호되고 있습니다.\n\nWindows Recovery Assistant\n\n저를 종료하려고 하셨나요?\n\n괜찮습니다.\n혼란이 있었던 것 같네요.\n\n복구를 처음부터 다시 진행하겠습니다.\n\n[확인]"),
            new NpcDialogueEntry(261, 10, "알림창 / Windows Recovery Assistant", "회피 루프 엔딩 조건", "조건 예시\nFinalInput이 빈칸이거나 존재하지 않는 이름\n또는 DelayCount / ReportSkipped / Deferred 선택이 일정 수 이상 누적된 경우\n\nWindows Recovery Assistant\n\n결정이 보류되었습니다.\n\n복구 절차를 다시 예약합니다.\n\n[15분 후 다시 알림]\n\n의미\n플레이어가 끝까지 결정을 미루고 루프로 돌아간다."),
        };

        public static readonly string[] DocumentParagraphs = new string[]
        {
            "[Admin]",
            "전체 대사 정리",
            "공통 규칙. 창 닫기 / 취소 / 나중에 행동 처리",
            "목차",
            "STAGE 01 일반 [File Explorer 숲]",
            "STAGE 02 보스 [Driver Vault 격납고]",
            "STAGE 03 일반 [Windows Update 연구소]",
            "STAGE 04 보스 [System32 금지구역]",
            "STAGE 05 일반 [Network Port 항구]",
            "STAGE 06 보스 [Blue Screen Tower]",
            "STAGE 07 일반 [Registry Hive 보관소]",
            "STAGE 08 보스 [Popup Error 미궁]",
            "STAGE 09 일반 [Temp Cache 동굴]",
            "STAGE 10 최종 [Recycle Bin 던전]",
            "부록. 행동 기록과 엔딩 분기",
            "STAGE 01 일반\n[File Explorer 숲]",
            "4. Windows Recovery Assistant 등장",
            "5. 복구 프로필 생성",
            "플레이어 행동 분기",
            "7. 기본 조작 안내",
            "9. 일반 전투 구간",
            "10. 전투 종료 흐름",
            "11. Stage 1 클리어 연출",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 02 보스\n[Driver Vault 격납고]",
            "3. 진입 연출",
            "4. Driver Vault 화면 구성",
            "5. Windows Recovery Assistant 안내",
            "6. 보스 등장: Driver-K",
            "7. 보스전 구간",
            "8. 보스전 중간 연출",
            "9. 보스전 종료 흐름",
            "10. Stage 2 클리어 연출",
            "추가. 창 닫기 / 선택 행동 반응 모음",
            "플레이어 행동 분기",
            "STAGE 03 일반\n[Windows Update 연구소]",
            "3. 진입 연출",
            "4. Windows Update 연구소 화면 구성",
            "5. Windows Recovery Assistant 안내",
            "6. 일반 전투 구간",
            "7. 재시작 알림 이벤트",
            "8. 업데이트 진행률 정지 구간 / 업데이트 실패 및 재시도",
            "10. Stage 3 클리어 연출",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 04 보스\n[System32 금지구역]",
            "보스 말투 수정 필요",
            "3. 진입 연출",
            "5. Windows Recovery Assistant 안내",
            "6. 접근 제한 이벤트",
            "7. 보스 등장: High-Kernel",
            "8. 보스전 구간",
            "9. 추가 권한 확인 이벤트",
            "10. 보스전 중간 연출",
            "11. 보스전 종료 흐름",
            "12. Stage 4 클리어 연출",
            "14. Stage 4 기획 메모",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 05 일반\n[Network Port 항구]",
            "3. 진입 연출",
            "5. Windows Recovery Assistant 안내",
            "7. 일반 전투 구간 2: 이상 패킷 제거",
            "8. 방화벽 게이트 이벤트",
            "11. Stage 5 클리어 연출",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 06 보스\n[Blue Screen Tower]",
            "3. 진입 연출",
            "4. System Stability Check 화면 구성",
            "5. Windows Recovery Assistant 안내",
            "6. Blue Screen Tower 진입",
            "7. 보스 등장: BSOD / Blue Screen Sentinel",
            "8. 보스전 구간",
            "9. 임시 보스 패턴 방향",
            "10. 보스전 중간 연출",
            "11. 보스전 종료 흐름",
            "12. Stage 6 클리어 연출",
            "13. 다음 스테이지 연결",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 07 일반\n[Registry Hive 보관소]",
            "3. 진입 연출",
            "5. Windows Recovery Assistant 안내",
            "6. 일반 전투 구간",
            "7. 최근 실행 기록 발견",
            "8. 복구 프로필 이름 재등장",
            "9. 선택 이벤트: 정리할 값과 남길 값",
            "10. 일반 전투 종료 흐름",
            "11. Stage 7 클리어 연출",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 08 보스\n[Popup Error 미궁]",
            "3. 진입 연출",
            "5. Windows Recovery Assistant 안내",
            "6. 보스 등장: Exception Queen",
            "7. 보스전 구간",
            "8. 임시 보스 패턴 방향",
            "9. 핵심 이벤트",
            "11. 보스전 종료 흐름",
            "12. Stage 8 클리어 연출",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 09 일반\n[Temp Cache 동굴]",
            "3. 진입 연출",
            "5. Windows Recovery Assistant 안내",
            "7. 탐색형 일반 전투 흐름",
            "8. 핵심 이벤트 1: 미전송 오류 보고서",
            "9. 핵심 이벤트 2: 복구 프로필 캐시",
            "10. 핵심 이벤트 3: 최근 실행 잔상",
            "11. 멀티엔딩 후보 기록",
            "12. 마무리 전투: Cache Heap",
            "13. Stage 9 클리어 연출",
            "14. 다음 스테이지 연결",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "STAGE 10 최종\n[Recycle Bin 던전]",
            "3. 진입 연출",
            "5. 이전 스테이지 회수 요소",
            "6. 표면상 최종 보스: Illegal_Binny",
            "7. Illegal_Binny 최종 전투",
            "8. Illegal_Binny 격리 흐름",
            "9. 최종 입력창 등장",
            "10. Windows Recovery Assistant 개입",
            "11. 엔딩 분기 구조",
            "12. 진엔딩",
            "13. 일반 엔딩",
            "14. Windows Recovery Assistant 입력 엔딩",
            "15. 잘못된 입력 엔딩",
            "16. Windows Recovery Assistant 최종 성격",
            "18. Stage 10 구성 요약",
            "추가. 창 닫기 / 선택 행동 반응",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "플레이어 행동 분기",
            "부록. 행동 기록과 엔딩 분기",
        };
    }
}