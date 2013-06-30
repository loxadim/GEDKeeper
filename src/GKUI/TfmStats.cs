﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using GedCom551;
using GKCore;
using GKUI.Controls;
using ZedGraph;

/// <summary>
/// Localization: dirty
/// </summary>

namespace GKUI
{
	public partial class TfmStats : Form, ILocalization
	{
		private struct TTitleRec
		{
			public LSID Title;
			public LSID Cap;

			public TTitleRec(LSID title, LSID cap) {
				this.Title = title;
				this.Cap = cap;
			}
		}

		private enum TChartStyle : byte { csBar, csPoint }

		private static TTitleRec[] StTitles;
		private GKListView ListStats;
		private ZedGraphControl zgc;
		private TfmBase FBase;
		private string ChartTitle;
		private string ChartXTitle;
		private string ChartYTitle;
		private TreeStats FTreeStats;

		public TfmBase Base
		{
			get { return this.FBase; }
		}

		private static double SafeDiv(double aDividend, double aDivisor)
		{
			return ((aDivisor == (double)0f) ? 0.0 : (aDividend / aDivisor));
		}

		private void PrepareArray(GraphPane gPane, TChartStyle style, bool excludeUnknowns)
		{
			gPane.Title.Text = ChartTitle;
			gPane.XAxis.Title.Text = ChartXTitle;
			gPane.YAxis.Title.Text = ChartYTitle;
			PointPairList ppList = new PointPairList();

			int num = this.ListStats.Items.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				string s = this.ListStats.Items[i].Text;
				int lab;
				if (s == "?")
				{
					lab = 0;
				}
				else
				{
					lab = int.Parse(s);
				}
				if (lab != 0 || !excludeUnknowns)
				{
					int val = int.Parse(this.ListStats.Items[i].SubItems[1].Text);
					ppList.Add((double)lab, (double)val);
				}
			}
			ppList.Sort();

			switch (style) {
				case TChartStyle.csBar:
					gPane.AddBar("-", ppList, Color.Green);
					break;
				case TChartStyle.csPoint:
					gPane.AddCurve("-", ppList, Color.Green, SymbolType.Diamond).Symbol.Size = 3;
					break;
			}
		}

		private void CalcStats(TGEDCOMTree aTree, TreeStats.TStatMode aMode)
		{
			this.ListStats.SortColumn = 0;
			this.ListStats.Columns[0].Text = LangMan.LSList[(int)StTitles[(int)aMode].Cap - 1];
			this.ListStats.Columns[1].Text = LangMan.LSList[202];

			this.ListStats.Sorting = SortOrder.None;
			this.ListStats.SortColumn = -1;
			this.ListStats.BeginUpdate();
			this.ListStats.Items.Clear();
			
			List<TreeStats.TListVal> vals = new List<TreeStats.TListVal>();
			try
			{
				FTreeStats.GetSpecStats(aMode, vals);
				ListViewItem[] items = new ListViewItem[vals.Count];

				int i = 0;
				foreach (TreeStats.TListVal lv in vals)
				{
					ListViewItem item = new ListViewItem();
					item.Text = lv.Item;
					item.SubItems.Add(lv.Count.ToString());

					items[i] = item;
					i++;
				}

				this.ListStats.Items.AddRange(items);
			}
			finally
			{
				//items.Dispose();
				//vals.Dispose();
				this.ListStats.EndUpdate();
			}

			GraphPane gPane = this.zgc.GraphPane;
			try
			{
				gPane.CurveList.Clear();
				this.ChartTitle = LangMan.LSList[(int)StTitles[(int)aMode].Title - 1];

				switch (aMode) {
					case TreeStats.TStatMode.smAge: {
						this.ChartXTitle = LangMan.LSList[305];
						this.ChartYTitle = LangMan.LSList[533];
						this.PrepareArray(gPane, TfmStats.TChartStyle.csPoint, true);
						break;
					}

					case TreeStats.TStatMode.smLifeExpectancy: {
						this.ChartXTitle = LangMan.LSList[306];
						this.ChartYTitle = LangMan.LSList[533];
						this.PrepareArray(gPane, TfmStats.TChartStyle.csPoint, true);
						break;
					}

					case TreeStats.TStatMode.smBirthYears:
					case TreeStats.TStatMode.smBirthTenYears:
					case TreeStats.TStatMode.smDeathYears: 
					case TreeStats.TStatMode.smDeathTenYears: {
						switch (aMode) {
							case TreeStats.TStatMode.smBirthYears:
							case TreeStats.TStatMode.smDeathYears: 
								this.ChartXTitle = LangMan.LS(LSID.LSID_Years);
								break;
							case TreeStats.TStatMode.smBirthTenYears:
							case TreeStats.TStatMode.smDeathTenYears: 
								this.ChartXTitle = LangMan.LS(LSID.LSID_Decennial);
								break;
						}

						switch (aMode) {
							case TreeStats.TStatMode.smBirthYears:
          					case TreeStats.TStatMode.smBirthTenYears:
								this.ChartYTitle = LangMan.LS(LSID.LSID_HowBirthes);
          						break;
          					case TreeStats.TStatMode.smDeathYears:
          					case TreeStats.TStatMode.smDeathTenYears:
          						this.ChartYTitle = LangMan.LS(LSID.LSID_HowDeads);
          						break;
						}

        				this.PrepareArray(gPane, TfmStats.TChartStyle.csPoint, true);
						break;
					}

					case TreeStats.TStatMode.smChildsDistribution: {
						this.ChartXTitle = LangMan.LSList[118];
						this.ChartYTitle = LangMan.LSList[152];
						this.PrepareArray(gPane, TfmStats.TChartStyle.csBar, true);
						break;
					}
				}
			}
			finally
			{
				zgc.AxisChange();
				zgc.Invalidate();
			}
		}

		private void TfmStats_Load(object sender, EventArgs e)
		{
			FTreeStats = new TreeStats(this.Base.Engine);
			
			TreeStats.TCommonStats stats;
			FTreeStats.GetCommonStats(out stats);

			this.ListCommon.Items.Clear();

			ListViewItem item = this.ListCommon.Items.Add(LangMan.LSList[533]);
			item.SubItems.Add(stats.persons.ToString());
			item.SubItems.Add(stats.persons_m.ToString() + this.GetPercent(stats.persons_m, stats.persons));
			item.SubItems.Add(stats.persons_f.ToString() + this.GetPercent(stats.persons_f, stats.persons));

			item = this.ListCommon.Items.Add(LangMan.LSList[538]);
			item.SubItems.Add(stats.lives.ToString());
			item.SubItems.Add(stats.lives_m.ToString());
			item.SubItems.Add(stats.lives_f.ToString());

			item = this.ListCommon.Items.Add(LangMan.LSList[539]);
			item.SubItems.Add((stats.persons - stats.lives).ToString());
			item.SubItems.Add((stats.persons_m - stats.lives_m).ToString());
			item.SubItems.Add((stats.persons_f - stats.lives_f).ToString());

			checked
			{
				item = this.ListCommon.Items.Add(LangMan.LSList[540]);
				item.SubItems.Add((Math.Round(SafeDiv(stats.age, stats.age_cnt))).ToString());
				item.SubItems.Add((Math.Round(SafeDiv(stats.age_m, stats.age_m_cnt))).ToString());
				item.SubItems.Add((Math.Round(SafeDiv(stats.age_f, stats.age_f_cnt))).ToString());

				item = this.ListCommon.Items.Add(LangMan.LSList[541]);
				item.SubItems.Add((Math.Round(SafeDiv(stats.life, stats.life_cnt))).ToString());
				item.SubItems.Add((Math.Round(SafeDiv(stats.life_m, stats.life_m_cnt))).ToString());
				item.SubItems.Add((Math.Round(SafeDiv(stats.life_f, stats.life_f_cnt))).ToString());

				item = this.ListCommon.Items.Add(LangMan.LSList[542]);
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.childs, stats.childs_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.childs_m, stats.childs_m_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.childs_f, stats.childs_f_cnt)));

				item = this.ListCommon.Items.Add(LangMan.LSList[543]);
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.fba, stats.fba_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.fba_m, stats.fba_m_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.fba_f, stats.fba_f_cnt)));

				item = this.ListCommon.Items.Add(LangMan.LSList[544]);
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.marr, stats.marr_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.marr_m, stats.marr_m_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.marr_f, stats.marr_f_cnt)));

				item = this.ListCommon.Items.Add(LangMan.LSList[545]);
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.mage, stats.mage_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.mage_m, stats.mage_m_cnt)));
				item.SubItems.Add(string.Format("{0:0.00}", SafeDiv(stats.mage_f, stats.mage_f_cnt)));
			}
		}

		private void TfmStats_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape) base.Close();
		}

		private void cbType_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.CalcStats(this.Base.Tree, (TreeStats.TStatMode)this.cbType.SelectedIndex);
		}

		public TfmStats(TfmBase aBase)
		{
			this.InitializeComponent();
			base.MdiParent = GKUI.TfmGEDKeeper.Instance;
			this.FBase = aBase;
			this.zgc = new ZedGraphControl();
			this.zgc.IsShowPointValues = true;
			this.zgc.Dock = DockStyle.Right;
			this.zgc.Size = new Size(400, 200);
			Splitter spl = new Splitter();
			spl.Dock = DockStyle.Right;
			spl.Size = new Size(4, 290);
			spl.MinExtra = 100;
			spl.MinSize = 100;
			this.Panel1.Controls.Add(this.zgc);
			this.Panel1.Controls.Add(spl);

			this.Base.CreateListView(this.Panel1, out this.ListStats);
			this.ListStats.AddListColumn("-", 250, false);
			this.ListStats.AddListColumn("-", 150, false);

			this.Panel1.Controls.SetChildIndex(this.ListStats, 0);
			this.Panel1.Controls.SetChildIndex(spl, 2);
			this.Panel1.Controls.SetChildIndex(this.zgc, 3);
			this.Panel1.Controls.SetChildIndex(this.ToolBar1, 4);
			this.cbType.Items.Clear();

			for (TreeStats.TStatMode i = TreeStats.TStatMode.smAncestors; i <= TreeStats.TStatMode.smAAF_2; i++)
			{
				TTitleRec tr = StTitles[(int)i];
				this.cbType.Items.Add(LangMan.LSList[(int)tr.Title - 1]);
			}

			(this as ILocalization).SetLang();
		}

		void ILocalization.SetLang()
		{
			this.Text = LangMan.LS(LSID.LSID_MIStats);
		}

		static TfmStats()
		{
			StTitles = new TTitleRec[34];
			StTitles[0] = new TTitleRec(LSID.LSID_AncestorsCount, LSID.LSID_Name);
			StTitles[1] = new TTitleRec(LSID.LSID_DescendantsCount, LSID.LSID_Name);
			StTitles[2] = new TTitleRec(LSID.LSID_GenerationsCount, LSID.LSID_Name);
			StTitles[3] = new TTitleRec(LSID.LSID_Surname, LSID.LSID_Surname);
			StTitles[4] = new TTitleRec(LSID.LSID_Name, LSID.LSID_Name);
			StTitles[5] = new TTitleRec(LSID.LSID_Patronymic, LSID.LSID_Patronymic);
			StTitles[6] = new TTitleRec(LSID.LSID_Age, LSID.LSID_Age);
			StTitles[7] = new TTitleRec(LSID.LSID_LifeExpectancy, LSID.LSID_Age);
			StTitles[8] = new TTitleRec(LSID.LSID_BirthYears, LSID.LSID_BirthYears);
			StTitles[9] = new TTitleRec(LSID.LSID_BirthYearsDec, LSID.LSID_BirthYears);
			StTitles[10] = new TTitleRec(LSID.LSID_DeathYears, LSID.LSID_DeathYears);
			StTitles[11] = new TTitleRec(LSID.LSID_DeathYearsDec, LSID.LSID_DeathYears);
			StTitles[12] = new TTitleRec(LSID.LSID_ChildsCount, LSID.LSID_Name);
			StTitles[13] = new TTitleRec(LSID.LSID_DistrChilds, LSID.LSID_ChildsCount);
			StTitles[14] = new TTitleRec(LSID.LSID_BirthPlace, LSID.LSID_BirthPlace);
			StTitles[15] = new TTitleRec(LSID.LSID_DeathPlace, LSID.LSID_DeathPlace);
			StTitles[16] = new TTitleRec(LSID.LSID_Residence, LSID.LSID_Residence);
			StTitles[17] = new TTitleRec(LSID.LSID_Occupation, LSID.LSID_Occupation);
			StTitles[18] = new TTitleRec(LSID.LSID_Religion, LSID.LSID_Religion);
			StTitles[19] = new TTitleRec(LSID.LSID_Nationality, LSID.LSID_Nationality);
			StTitles[20] = new TTitleRec(LSID.LSID_Education, LSID.LSID_Education);
			StTitles[21] = new TTitleRec(LSID.LSID_Caste, LSID.LSID_Caste);
			StTitles[22] = new TTitleRec(LSID.LSID_AgeFirstborn, LSID.LSID_Name);
			StTitles[23] = new TTitleRec(LSID.LSID_MarriagesCount, LSID.LSID_Name);
			StTitles[24] = new TTitleRec(LSID.LSID_MarriagesAge, LSID.LSID_Name);
			StTitles[25] = new TTitleRec(LSID.LSID_DiffSpouses, LSID.LSID_Family);
			StTitles[26] = new TTitleRec(LSID.LSID_Hobby, LSID.LSID_Hobby);
			StTitles[27] = new TTitleRec(LSID.LSID_Award, LSID.LSID_Award);
			StTitles[28] = new TTitleRec(LSID.LSID_Mili, LSID.LSID_Mili);
			StTitles[29] = new TTitleRec(LSID.LSID_MiliInd, LSID.LSID_MiliInd);
			StTitles[30] = new TTitleRec(LSID.LSID_MiliDis, LSID.LSID_MiliDis);
			StTitles[31] = new TTitleRec(LSID.LSID_MiliRank, LSID.LSID_MiliRank);

			StTitles[32] = new TTitleRec(LSID.LSID_AAF_1, LSID.LSID_AAF_1);
			StTitles[33] = new TTitleRec(LSID.LSID_AAF_2, LSID.LSID_AAF_2);
		}

		private string GetPercent(int aDividend, int aDivisor)
		{
			double val = ((aDivisor == 0) ? 0.0 : ((double)aDividend / (double)aDivisor * 100.0));
			return string.Format(" ({0:0.00}%)", val);
		}
	}
}
