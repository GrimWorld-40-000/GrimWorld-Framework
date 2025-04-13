// Assembly-CSharp, Version=1.5.9214.33606, Culture=neutral, PublicKeyToken=null
// RimWorld.QuestGen.QuestNode_LendColonistsToFaction
using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace eridanus_quests
{
	public class QuestNode_LendAstartesToFaction : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignalEnable;

		[NoTranslate]
		public SlateRef<string> outSignalComplete;

		[NoTranslate]
		public SlateRef<string> outSignalColonistsDied;

		public SlateRef<Thing> shuttle;

		public SlateRef<Faction> lendColonistsToFactionOf;

		public SlateRef<int> returnLentColonistsInTicks;

		public override void RunInt()
		{
			Slate slate = QuestGen.slate;
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			QuestPart_LendAstartesToFaction questPart_LendAstartesToFaction = new QuestPart_LendAstartesToFaction
			{
				inSignalEnable = inSignal,
				shuttle = shuttle.GetValue(slate),
				lendColonistsToFaction = lendColonistsToFactionOf.GetValue(slate),
				returnLentColonistsInTicks = returnLentColonistsInTicks.GetValue(slate),
				returnMap = slate.Get<Map>("map").Parent
			};
			if (!outSignalComplete.GetValue(slate).NullOrEmpty())
			{
				questPart_LendAstartesToFaction.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
			}
			if (!outSignalColonistsDied.GetValue(slate).NullOrEmpty())
			{
				questPart_LendAstartesToFaction.outSignalColonistsDied = QuestGenUtility.HardcodedSignalWithQuestID(outSignalColonistsDied.GetValue(slate));
			}
			QuestGen.quest.AddPart(questPart_LendAstartesToFaction);
			QuestGen.quest.TendPawnsWithMedicine(ThingDefOf.MedicineUltratech, allowSelfTend: true, null, shuttle.GetValue(slate), inSignal);
		}

		public override bool TestRunInt(Slate slate)
		{

			return true;

            List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if(questsListForReading[i] != null)
				{

				}
			}

            return true;
		}
	}
}