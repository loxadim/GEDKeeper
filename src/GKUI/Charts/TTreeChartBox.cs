﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using Ext.Tween;
using Ext.Utils;
using GedCom551;
using GKCore;

/// <summary>
/// Localization: clean
/// </summary>

namespace GKUI.Charts
{
	public delegate void ThumbMoved(int position);
	
	public class TScaleControl
	{
		private static Rectangle ControlsScaleRect = new Rectangle(0, 0, 26, 320);
		private static Rectangle ControlsThumbRect = new Rectangle(0, 322, 26, 11);
		private static int ScaleY1 = 22;
		private static int ScaleY2 = 297;

		private Bitmap ControlsImage;
		private TTreeChartBox FChart;
		private int FDCount = 11;
		private int FThumbPos = 5;
		private bool FVisible;
		private bool FThumbCaptured;
		private string FTip;
		private Rectangle FDestRect;

		public int Width
		{
			get { return 26; }
		}

		public int Height
		{
			get { return 320; }
		}

		public int DCount
		{
			get { return this.FDCount; }
			set { this.FDCount = value; }
		}

		public int ThumbPos
		{
			get { return this.FThumbPos; }
			set { this.FThumbPos = value; }
		}
		
		public string Tip
		{
			get { return this.FTip; }
			set { this.FTip = value; }
		}
		
		public bool Visible
		{
			get { return this.FVisible; }
			set { 
				this.FVisible = value;
				this.FChart.Invalidate();
			}
		}

		public TScaleControl(TTreeChartBox chart)
		{
			this.FChart = chart;
			this.ControlsImage = GKResources.iChartControls;
		}

		public void Update()
		{
			Rectangle cr = FChart.ClientRectangle;
			this.FDestRect = new Rectangle(cr.Right - (10 + this.Width), 10, this.Width, this.Height);
		}

		public void Draw(Graphics aCanvas)
		{
			aCanvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
			aCanvas.SmoothingMode = SmoothingMode.HighQuality;
			aCanvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
			aCanvas.CompositingQuality = CompositingQuality.HighQuality;
			aCanvas.DrawImage(ControlsImage, FDestRect, ControlsScaleRect, GraphicsUnit.Pixel);

			if (this.FDCount == 0) return;
			aCanvas.DrawImage(ControlsImage, this.GetDRect(FThumbPos), ControlsThumbRect, GraphicsUnit.Pixel);
		}

		private Rectangle GetDRect(int d)
		{
			int dH = ((ScaleY2 - ScaleY1) - ControlsThumbRect.Height) / (this.FDCount - 1);
			int thumbY = FDestRect.Top + ScaleY1 + (d - 1) * dH;
			return new Rectangle(FDestRect.Left, thumbY, FDestRect.Width, ControlsThumbRect.Height);
		}

		public bool Contains(int X, int Y)
		{
			return FDestRect.Contains(X, Y);
		}

		public void MouseDown(int X, int Y)
		{
			FThumbCaptured = (this.GetDRect(FThumbPos).Contains(X, Y) && !FThumbCaptured);
		}

		public void MouseMove(int X, int Y, ThumbMoved thumbMoved)
		{
			if (!FThumbCaptured) return;
			
			for (int i = 1; i <= FDCount; i++) {
				Rectangle r = GetDRect(i);
				if (r.Contains(X, Y)) {
					FThumbPos = i;
					FChart.Invalidate();
					if (thumbMoved != null) thumbMoved(i);
					break;
				}
			}
		}

		public void MouseUp(int X, int Y)
		{
			if (FThumbCaptured) FThumbCaptured = false;
		}
	}


	public class TTreeChartBox : Panel, IDisposable
	{
		private class TRelLink
		{
			public TreeChartPerson xFrom;
			public TreeChartPerson xTo;
			public KinshipsMan.TRelationKind xRel;
		}

		public enum TChartKind : byte
		{
			ckAncestors,
			ckDescendants,
			ckBoth
		}

		private int FHMax;
		private int FWMax;

		private int FBranchDistance;
		private int FDepthLimit;
		private Font FDrawFont;
		private TGenEngine FEngine;
		private TChartFilter FFilter;
		private TGraph FGraph;
		private TChartKind FKind;
		private TreeChartPerson FKinRoot;
		private int FLevelDistance;
		private int FMargins;
		private TreeChartOptions FOptions;
		private bool FPathDebug;
		private TPersonList FPersons;
		private List<string> FPreparedFamilies = new List<string>();
		internal List<string> FPreparedIndividuals = new List<string>();
		private TreeChartPerson FRoot;
		private float FScale;
		private TScaleControl FScaleControl;
		private TreeChartPerson FSelected;
		private TGenEngine.TShieldState FShieldState;
		private string[] FSignsData;
		private Bitmap[] FSignsPic;
		private int FSpouseDistance;
		private bool FTraceSelected;
		private TGEDCOMTree FTree;

		public enum TDrawMode
		{
			dmDefault,
			dmToFile
		}

		private int FBorderWidth;
		private int FLeftPos;
		private int FTopPos;
		private Point FRange;

		protected int FImageHeight;
		protected int FImageWidth;
		protected int FSPX;
		protected int FSPY;
		protected TRect FVisibleArea;

		//private int FGensCount;

		private Pen FLinePen;
		private Pen FDecorativeLinePen;
		private SolidBrush FSolidBlack;

		public int BorderWidth
		{
			get { return this.FBorderWidth; }
			set { this.SetBorderWidth(value); }
		}

		public int BranchDistance
		{
			get { return this.FBranchDistance; }
			set { this.FBranchDistance = value; }
		}

		public int DepthLimit
		{
			get { return this.FDepthLimit; }
			set { this.FDepthLimit = value; }
		}

		public Font DrawFont
		{
			get { return this.FDrawFont; }
		}

		public TGenEngine Engine
		{
			get { return this.FEngine; }
			set { this.FEngine = value; }
		}

		public TChartFilter Filter
		{
			get { return this.FFilter; }
		}

//		public int GensCount
//		{
//			get { return this.FGensCount; }
//		}

		public int IndividualsCount
		{
			get { return this.FPreparedIndividuals.Count; }
		}

		public TChartKind Kind
		{
			get { return FKind; }
			set { FKind = value; }
		}

		public int LeftPos
		{
			get { return this.FLeftPos; }
			set { this.SetLeftPos(value); }
		}

		public int Margins
		{
			get { return this.FMargins; }
			set { this.FMargins = value; }
		}

		public TreeChartOptions Options
		{
			get { return this.FOptions; }
			set { this.FOptions = value; }
		}

		public bool PathDebug
		{
			get { return this.FPathDebug; }
			set { this.FPathDebug = value; }
		}

		public TreeChartPerson Root
		{
			get { return this.FRoot; }
		}

		public new float Scale
		{
			get { return this.FScale; }
			set {
				this.FScale = value;

				FScaleControl.ThumbPos = (int)Math.Round((value -  0.4f) / 0.1f);

				this.Predef();
				this.RecalcChart();
				this.ScrollRange();

				if (this.FTraceSelected && this.Selected != null) {
					this.CenterPerson(this.Selected, false);
				}
			}
		}

		public new TScaleControl ScaleControl
		{
			get { return this.FScaleControl; }
		}

		public TreeChartPerson Selected
		{
			get { return this.FSelected; }
			set { this.SetSelected(value); }
		}

		public TGenEngine.TShieldState ShieldState
		{
			get { return this.FShieldState; }
			set { this.FShieldState = value; }
		}

		public int TopPos
		{
			get { return this.FTopPos; }
			set { this.SetTopPos(value); }
		}

		public bool TraceSelected
		{
			get { return this.FTraceSelected; }
			set { this.FTraceSelected = value; }
		}

		public TGEDCOMTree Tree
		{
			get { return this.FTree; }
			set { this.FTree = value; }
		}

		/*protected override CreateParams CreateParams
		{ 
			get {
				CreateParams cp = base.CreateParams;
				cp.Style |= Win32Native.WS_HSCROLL | Win32Native.WS_VSCROLL;
				return cp;
			}
		}*/

		public TTreeChartBox()
		{
			base.BorderStyle = BorderStyle.Fixed3D;
			base.DoubleBuffered = true;
			base.TabStop = true;
			this.TopPos = 0;
			this.LeftPos = 0;
			base.BackColor = Color.White;

			this.InitSigns();

			this.FPersons = new TPersonList(true);
			this.FFilter = new TChartFilter();
			this.FSpouseDistance = 10;
			this.FBranchDistance = 40;
			this.FLevelDistance = 46;
			this.FMargins = 40;
			this.FDepthLimit = -1;
			this.FSelected = null;
			this.FScale = 1.0f;
			this.FTraceSelected = true;
			this.FGraph = new TGraph();

			this.FScaleControl = new TScaleControl(this);

			//this.AutoScroll = false;
			//this.HorizontalScroll.Visible = true;
			//this.VerticalScroll.Visible = true;

			//Win32Native.EnableScrollBar(this.Handle, Win32Native.SB_HORZ, Win32Native.ESB_ENABLE_BOTH);
			//Win32Native.EnableScrollBar(this.Handle, Win32Native.SB_VERT, Win32Native.ESB_ENABLE_BOTH);

			this.InitGraphics();
		}

		protected override void Dispose(bool Disposing)
		{
			if (Disposing)
			{
				this.DoneGraphics();

				this.FGraph.Dispose();
				this.FFilter.Free();
				this.FPersons.Dispose();
				this.DoneSigns();

                if (FDrawFont != null) FDrawFont.Dispose();
			}
			base.Dispose(Disposing);
		}

		private void InitSigns()
		{
			FSignsData = new string[] { "GEORGE_CROSS", "SOLDIER", "SOLDIER_FALL", "VETERAN_REAR" };

			FSignsPic = new Bitmap[4];

			FSignsPic[0] = GKResources.iTGGeorgeCross;
			FSignsPic[0].MakeTransparent(this.FSignsPic[0].GetPixel(0, 0));

			FSignsPic[1] = GKResources.iTGSoldier;
			FSignsPic[1].MakeTransparent(this.FSignsPic[1].GetPixel(0, 0));

			FSignsPic[2] = GKResources.iTGSoldierFall;
			FSignsPic[2].MakeTransparent(this.FSignsPic[2].GetPixel(0, 0));

			FSignsPic[3] = GKResources.iTGVeteranRear;
			FSignsPic[3].MakeTransparent(this.FSignsPic[3].GetPixel(0, 0));
		}

		private void DoneSigns()
		{
			// dummy
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.ResetView();
			FScaleControl.Update();
		}

		private bool IsChildless(TGEDCOMIndividualRecord iRec)
		{
			string exp = TGenEngine.GetLifeExpectancy(iRec);
			return (exp != "" && exp != "?" && int.Parse(exp) < 15);
		}

		private bool hasMediaFail = false;

		private TreeChartPerson AddDescPerson(TreeChartPerson aParent, TGEDCOMIndividualRecord iRec, TreeChartPerson.TPersonKind aKind, int aGeneration)
		{
			TreeChartPerson Result;
			if (this.FRoot != null && object.Equals(this.FRoot.Rec, iRec))
			{
				Result = this.FRoot;
				Result.Parent = aParent;
				Result.Kind = aKind;
			} else {
				Result = new TreeChartPerson(this);
				Result.BuildBy(iRec, ref hasMediaFail);
				Result.Generation = aGeneration;
				Result.Parent = aParent;
				Result.Kind = aKind;
				this.FPersons.Add(Result);

				if (this.FOptions.Kinship)
				{
					Result.Node = this.FGraph.CreateNode(Result);
				}

				if (aKind != TreeChartPerson.TPersonKind.pkSpouse && aParent != null)
				{
					aParent.AddChild(Result);
				}
			}
			return Result;
		}

		private TreeChartPerson DoAncestorsStep(TreeChartPerson aChild, TGEDCOMIndividualRecord aPerson, int aGeneration, bool dup_flag)
		{
			TreeChartPerson Result = null;

			if (aPerson != null)
			{
				Result = new TreeChartPerson(this);
				Result.BuildBy(aPerson, ref hasMediaFail);
				Result.Generation = aGeneration;
				this.FPersons.Add(Result);

				if (aChild != null)
				{
					Result.AddChild(aChild);
				}

				if (this.FOptions.Kinship)
				{
					Result.Node = this.FGraph.CreateNode(Result);
				}

				if ((this.FDepthLimit <= -1 || aGeneration != this.FDepthLimit) && aPerson.ChildToFamilyLinks.Count > 0 && !dup_flag)
				{
					TGEDCOMFamilyRecord family = aPerson.ChildToFamilyLinks[0].Family;

					bool is_dup = (this.FPreparedFamilies.IndexOf(family.XRef) >= 0);
					if (!is_dup) this.FPreparedFamilies.Add(family.XRef);

					if (TGenEngine.IsRecordAccess(family.Restriction, this.FShieldState))
					{
						TGEDCOMIndividualRecord iFather = family.Husband.Value as TGEDCOMIndividualRecord;
						TGEDCOMIndividualRecord iMother = family.Wife.Value as TGEDCOMIndividualRecord;
						bool divorced = (family.GetTagStringValue("_STAT") == "NOTMARR");

						if (iFather != null && TGenEngine.IsRecordAccess(iFather.Restriction, this.FShieldState))
						{
							Result.Father = this.DoAncestorsStep(Result, iFather, aGeneration + 1, is_dup);
							if (Result.Father != null)
							{
								Result.Father.Divorced = divorced;
								Result.Father.IsDup = is_dup;
								if (this.FOptions.Kinship)
								{
									this.FGraph.CreateLink(Result.Node, Result.Father.Node, 1, 1, 3);
								}
							}
						} else {
							Result.Father = null;
						}

						if (iMother != null && TGenEngine.IsRecordAccess(iMother.Restriction, this.FShieldState))
						{
							Result.Mother = this.DoAncestorsStep(Result, iMother, aGeneration + 1, is_dup);
							if (Result.Mother != null)
							{
								Result.Mother.Divorced = divorced;
								Result.Mother.IsDup = is_dup;
								if (this.FOptions.Kinship)
								{
									this.FGraph.CreateLink(Result.Node, Result.Mother.Node, 1, 1, 3);
								}
							}
						} else {
							Result.Mother = null;
						}

						if (Result.Father != null && Result.Mother != null && this.FOptions.Kinship)
						{
							this.FGraph.CreateLink(Result.Father.Node, Result.Mother.Node, 1, 2, 2);
						}
					}
				}
			}

			return Result;
		}

		private TreeChartPerson DoDescendantsStep(TreeChartPerson aParent, TGEDCOMIndividualRecord aPerson, int aLevel)
		{
			TreeChartPerson Result = null;
			if (aPerson != null && (!this.FOptions.ChildlessExclude || aLevel <= 1 || aPerson.SpouseToFamilyLinks.Count != 0 || !this.IsChildless(aPerson)))
			{
				CustomFilter.TGroupMode sourceMode = this.FFilter.SourceMode;

				if (sourceMode != CustomFilter.TGroupMode.gmNone)
				{
					if (sourceMode != CustomFilter.TGroupMode.gmAny)
					{
						if (sourceMode == CustomFilter.TGroupMode.gmSelected)
						{
							TGEDCOMSourceRecord filter_source;

							if (this.FFilter.SourceRef == "")
							{
								filter_source = null;
							} else {
								filter_source = this.FTree.XRefIndex_Find(this.FFilter.SourceRef) as TGEDCOMSourceRecord;
							}

							if (aPerson.IndexOfSource(filter_source) < 0)
							{
								return Result;
							}
						}
					} else {
						if (aPerson.SourceCitations.Count == 0)
						{
							return Result;
						}
					}
				} else {
					if (aPerson.SourceCitations.Count != 0)
					{
						return Result;
					}
				}

				TChartFilter.TBranchCut branchCut = this.FFilter.BranchCut;
				if (branchCut != TChartFilter.TBranchCut.bcNone)
				{
					if (!(bool)aPerson.ExtData)
					{
						return Result;
					}
				}

				TreeChartPerson res = this.AddDescPerson(aParent, aPerson, TreeChartPerson.TPersonKind.pkDefault, aLevel);
				Result = res;

				int num = aPerson.SpouseToFamilyLinks.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					TGEDCOMFamilyRecord family = aPerson.SpouseToFamilyLinks[i].Family;

					bool is_dup = (this.FPreparedFamilies.IndexOf(family.XRef) >= 0);
					if (!is_dup) this.FPreparedFamilies.Add(family.XRef);

					if (TGenEngine.IsRecordAccess(family.Restriction, this.FShieldState))
					{
						TreeChartPerson res_parent = null;
						TGEDCOMSex sex = aPerson.Sex;
						TreeChartPerson ft = null;
						TreeChartPerson mt = null;
						TreeChartPerson.TPersonFlag desc_flag = TreeChartPerson.TPersonFlag.pfDescByFather;
						if (sex != TGEDCOMSex.svMale)
						{
							if (sex == TGEDCOMSex.svFemale)
							{
								TGEDCOMIndividualRecord sp = family.Husband.Value as TGEDCOMIndividualRecord;
								res_parent = this.AddDescPerson(null, sp, TreeChartPerson.TPersonKind.pkSpouse, aLevel);
								res_parent.Sex = TGEDCOMSex.svMale;
								ft = res_parent;
								mt = res;
								desc_flag = TreeChartPerson.TPersonFlag.pfDescByFather;
							}
						}
						else
						{
							TGEDCOMIndividualRecord sp = family.Wife.Value as TGEDCOMIndividualRecord;
							res_parent = this.AddDescPerson(null, sp, TreeChartPerson.TPersonKind.pkSpouse, aLevel);
							res_parent.Sex = TGEDCOMSex.svFemale;
							ft = res;
							mt = res_parent;
							desc_flag = TreeChartPerson.TPersonFlag.pfDescByMother;
						}

						if (this.FOptions.Kinship)
						{
							this.FGraph.CreateLink(res.Node, res_parent.Node, 1, 2, 2);
						}

						if (res_parent != null)
						{
							res.AddSpouse(res_parent);
							res_parent.BaseSpouse = res;
						}
						else
						{
							res_parent = res;
						}

						ft.IsDup = is_dup;
						mt.IsDup = is_dup;

						if ((this.FDepthLimit <= -1 || aLevel != this.FDepthLimit) && (!is_dup))
						{
							int num2 = family.Childrens.Count - 1;
							for (int j = 0; j <= num2; j++)
							{
								TGEDCOMIndividualRecord child_rec = family.Childrens[j].Value as TGEDCOMIndividualRecord;
								if (TGenEngine.IsRecordAccess(child_rec.Restriction, this.FShieldState))
								{
									TreeChartPerson child = this.DoDescendantsStep(res_parent, child_rec, aLevel + 1);
									if (child != null)
									{
										child.Father = ft;
										child.Mother = mt;
										//int d = (int)desc_flag;
										child.FFlags.Include(desc_flag);
										if (this.FOptions.Kinship)
										{
											this.FGraph.CreateLink(child.Node, ft.Node, 1, 1, 3);
											this.FGraph.CreateLink(child.Node, mt.Node, 1, 1, 3);
										}
									}
								}
							}
						}
					}
				}
			}
			return Result;
		}

		private string FindRelationship(TreeChartPerson aTarget)
		{
			string Result = "";
			TList path = new TList(true);
			try
			{
				TGraph.TGraphLink link = aTarget.Node.LinkIn;
				if (link != null)
				{
					do
					{
						this.FixLink(path, link.Node1.ExtObj as TreeChartPerson, link.Node2.ExtObj as TreeChartPerson, (KinshipsMan.TRelationKind)link.ExtData);
						link = link.Node1.LinkIn;
					}
					while (link != null);
				}
				string tmp = "";
				KinshipsMan.TRelationKind prev_rel = KinshipsMan.TRelationKind.rkNone;
				KinshipsMan.TRelationKind fin_rel = KinshipsMan.TRelationKind.rkNone;
				int great = 0;

				for (int i = path.Count - 1; i >= 0; i--)
				{
					TTreeChartBox.TRelLink L = path[i] as TTreeChartBox.TRelLink;
					KinshipsMan.TRelationKind cur_rel = L.xRel;
					if (this.FPathDebug)
					{
						if (tmp != "")
						{
							tmp += ", ";
						}
						if (L.xFrom.Rec != null)
						{
							tmp = string.Concat(new string[]
							{
								tmp, L.xFrom.Rec.XRef, ">", TGenEngine.RelationSigns[(int)cur_rel], ">"
							});
						}
						if (L.xTo.Rec != null)
						{
							tmp += L.xTo.Rec.XRef;
						}
					}
					if (prev_rel != KinshipsMan.TRelationKind.rkUndefined)
					{
						int g;
						int lev;
						fin_rel = KinshipsMan.FindKinship(prev_rel, cur_rel, out g, out lev);
						great += g;
						prev_rel = fin_rel;
					}
				}

				if (this.FPathDebug)
				{
					if (aTarget.Rec != null)
					{
						aTarget.FPathDebug = aTarget.Rec.XRef + " ";
					}
					aTarget.FPathDebug = aTarget.FPathDebug + " [" + tmp + "]";
				}
				Result = "[" + this.FixRelation(aTarget, fin_rel, great) + "]";
			}
			finally
			{
				path.Dispose();
			}
			return Result;
		}

		private void FixLink(TList path, TreeChartPerson f, TreeChartPerson t, KinshipsMan.TRelationKind rel)
		{
			TTreeChartBox.TRelLink L = new TTreeChartBox.TRelLink();
			L.xFrom = f;
			L.xTo = t;
			if (rel != KinshipsMan.TRelationKind.rkParent)
			{
				if (rel != KinshipsMan.TRelationKind.rkSpouse)
				{
					if (rel != KinshipsMan.TRelationKind.rkChild)
					{
						L.xRel = rel;
					}
					else
					{
						TGEDCOMSex sex = L.xTo.Sex;
						if (sex != TGEDCOMSex.svMale)
						{
							if (sex == TGEDCOMSex.svFemale)
							{
								L.xRel = KinshipsMan.TRelationKind.rkDaughter;
							}
						}
						else
						{
							L.xRel = KinshipsMan.TRelationKind.rkSon;
						}
					}
				}
				else
				{
					TGEDCOMSex sex2 = L.xTo.Sex;
					if (sex2 != TGEDCOMSex.svMale)
					{
						if (sex2 == TGEDCOMSex.svFemale)
						{
							L.xRel = KinshipsMan.TRelationKind.rkWife;
						}
					}
					else
					{
						L.xRel = KinshipsMan.TRelationKind.rkHusband;
					}
				}
			}
			else
			{
				TGEDCOMSex sex3 = L.xTo.Sex;
				if (sex3 != TGEDCOMSex.svMale)
				{
					if (sex3 == TGEDCOMSex.svFemale)
					{
						L.xRel = KinshipsMan.TRelationKind.rkMother;
					}
				}
				else
				{
					L.xRel = KinshipsMan.TRelationKind.rkFather;
				}
			}
			path.Add(L);
		}

		private string FixRelation(TreeChartPerson aTarget, KinshipsMan.TRelationKind Rel, int Great)
		{
			string tmp = "";
			if (Great != 0)
			{
				if (Rel >= KinshipsMan.TRelationKind.rkUncle && Rel < KinshipsMan.TRelationKind.rkNephew)
				{
					tmp = TGenEngine.Numerals[Great] + TGenEngine.NumKinship[(int)aTarget.Sex] + " ";
					if (Rel == KinshipsMan.TRelationKind.rkUncle)
					{
						Rel = KinshipsMan.TRelationKind.rkGrandfather;
					}
					if (Rel == KinshipsMan.TRelationKind.rkAunt)
					{
						Rel = KinshipsMan.TRelationKind.rkGrandmother;
					}
				}
				else
				{
					if (Rel != KinshipsMan.TRelationKind.rkUndefined)
					{
						tmp = this.GetGreat(Great);
					}
				}
			}
			else
			{
				tmp = "";
			}
			return tmp + LangMan.LSList[(int)TGenEngine.RelationKinds[(int)Rel] - 1];
		}

		private string GetGreat(int n)
		{
			string Result = "";
			int num = n;
			int i = 1;
			num = num - i + 1;
			if (num >= i)
			{
				do
				{
					Result += "пра";
					num--;
				}
				while (num != 0);
			}
			return Result;
		}

		private void InitEdges(ref int[] edges)
		{
			for (int i = 0; i <= 255; i++) edges[i] = 0;
		}

		public bool PersonIsVisible(TRect pn_rect)
		{
			int pL, pR, pT, pB;
			pL = pn_rect.Left;
			pR = pn_rect.Right;
			pT = pn_rect.Top;
			pB = pn_rect.Bottom;

			return (FVisibleArea.Contains(pL, pT) || FVisibleArea.Contains(pR, pT) ||
			        FVisibleArea.Contains(pL, pB) || FVisibleArea.Contains(pR, pB));
		}

		private bool LineIsVisible(int X1, int Y1, int X2, int Y2)
		{
			return (FVisibleArea.Contains(X1, Y1) || FVisibleArea.Contains(X2, Y2));
		}
		
		private void DrawLine(Graphics aCanvas, int X1, int Y1, int X2, int Y2)
		{
			//if (!LineIsVisible(X1, Y1, X2, Y2)) return;
			
			int sX = this.FSPX + X1;
			int sX2 = this.FSPX + X2;
			int sY = this.FSPY + Y1;
			int sY2 = this.FSPY + Y2;
			aCanvas.DrawLine(FLinePen, sX, sY, sX2, sY2);

			if (this.FOptions.Decorative) {
				if (sX == sX2) {
					aCanvas.DrawLine(FDecorativeLinePen, sX + 1, sY + 1, sX2 + 1, sY2 - 1);
				} else {
					if (sY == sY2) {
						aCanvas.DrawLine(FDecorativeLinePen, sX + 1, sY + 1, sX2 + 0, sY2 + 1);
					}
				}
			}
		}

		private void InitGraphics()
		{
			FLinePen = new Pen(Color.Black, 1f);
			FDecorativeLinePen = new Pen(Color.Silver, 1f);
			FSolidBlack = new SolidBrush(Color.Black);
		}

		private void DoneGraphics()
		{
			this.FLinePen.Dispose();
			this.FDecorativeLinePen.Dispose();
			this.FSolidBlack.Dispose();
		}

		private void TextOut(Graphics aCanvas, TRect rt, string s, int h, ref int line)
		{
			int stw = aCanvas.MeasureString(s, this.DrawFont).ToSize().Width;
			int rx = rt.Left + ((rt.Right - rt.Left + 1) - stw) / 2;
			int ry = rt.Top + (10 + (h * line));
			aCanvas.DrawString(s, this.DrawFont, FSolidBlack, (float)rx, (float)ry);
			line++;
		}

		private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
	    {
			int xw = x + width;
			int yh = y + height;
			int xwr = xw - radius;
			int yhr = yh - radius;
			int xr = x + radius;
			int yr = y + radius;
			int r2 = radius * 2;
			int xwr2 = xw - r2;
			int yhr2 = yh - r2;

			GraphicsPath p = new GraphicsPath();
			p.StartFigure();

			//Top Left Corner
			p.AddArc(x, y, r2, r2, 180, 90);

			//Top Edge
			p.AddLine(xr, y, xwr, y);

			//Top Right Corner
			p.AddArc(xwr2, y, r2, r2, 270, 90);

			//Right Edge
			p.AddLine(xw, yr, xw, yhr);

			//Bottom Right Corner
			p.AddArc(xwr2, yhr2, r2, r2, 0, 90);

			//Bottom Edge
			p.AddLine(xwr, yh, xr, yh);

			//Bottom Left Corner
			p.AddArc(x, yhr2, r2, r2, 90, 90);

			//Left Edge
			p.AddLine(x, yhr, x, yr);

			p.CloseFigure();
			return p;
		}

		private void DrawPathWithFuzzyLine(GraphicsPath path, Graphics gr, Color base_color, int max_opacity, int width, int opaque_width)
		{
			int num_steps = width - opaque_width + 1;       // Number of pens we will use.
			float delta = (float)max_opacity / num_steps / num_steps;   // Change in alpha between pens.
			float alpha = delta;                            // Initial alpha.
			for (int thickness = width; thickness >= opaque_width; thickness--)
			{
				Color color = Color.FromArgb(
					(int)alpha,
					base_color.R,
					base_color.G,
					base_color.B);
				using (Pen pen = new Pen(color, thickness))
				{
					pen.EndCap = LineCap.Round;
					pen.StartCap = LineCap.Round;
					gr.DrawPath(pen, path);
				}
				alpha += delta;
			}
		}

		private void DrawBorder(Graphics aCanvas, Pen xpen, TRect rt, bool dead, TreeChartPerson person)
		{
			Rectangle rect = rt.ToRectangle();
			Color b_color;
			switch (person.Sex) {
				case TGEDCOMSex.svMale:
				{
					if (!dead) {
						if (person.IsDup) {
							b_color = Color.FromArgb(192, 192, 192);
						} else {
							if (person.Divorced) {
								b_color = this.Options.UnHusbandColor;
							} else {
								b_color = this.Options.MaleColor;
							}
						}
					} else {
						b_color = Color.Black;
					}
					aCanvas.FillRectangle(new SolidBrush(b_color), rect.Left, rect.Top, rect.Width, rect.Height);
					aCanvas.DrawRectangle(xpen, rect.Left, rect.Top, rect.Width, rect.Height);
					break;
				}

				case TGEDCOMSex.svFemale:
				{
					if (!dead) {
						if (person.IsDup) {
							b_color = Color.FromArgb(192, 192, 192);
						} else {
							if (person.Divorced) {
								b_color = this.Options.UnWifeColor;
							} else {
								b_color = this.Options.FemaleColor;
							}
						}
					} else {
						b_color = Color.Black;
					}

					GraphicsPath path = CreateRoundedRectangle(rect.Left, rect.Top, rect.Width, rect.Height, 5);
					
					//aCanvas.TranslateTransform(3, 3);
					//DrawPathWithFuzzyLine(path, aCanvas, Color.Black, 200, 20, 2);
					//aCanvas.ResetTransform();

					aCanvas.FillPath(new SolidBrush(b_color), path);
					aCanvas.DrawPath(xpen, path);
					break;
				}

				default: 
				{
					if (!dead) {
						b_color = this.Options.UnkSexColor;
					} else {
						b_color = Color.Black;
					}
					aCanvas.FillRectangle(new SolidBrush(b_color), rect.Left, rect.Top, rect.Width, rect.Height);
					aCanvas.DrawRectangle(xpen, rect.Left, rect.Top, rect.Width, rect.Height);
					break;
				}
			}
		}

		private void DrawPerson(Graphics aCanvas, int SPX, int SPY, TreeChartPerson person)
		{
			TRect rt = person.Rect;
			if (!this.PersonIsVisible(rt)) return;

			rt = rt.GetOffset(SPX, SPY);

			int h = aCanvas.MeasureString("A", this.DrawFont).ToSize().Height;
			bool has_port = this.Options.PortraitsVisible && person.Portrait != null;

			if (person.IsDead) {
				TRect dt = rt;
				dt = dt.GetOffset(-2, -2);
				this.DrawBorder(aCanvas, FLinePen, dt, true, person);
			}

			Pen xpen = null;
			try
			{
				if (person.Selected) {
					int pen_width = 2;
					Color pen_color;
					switch (person.Sex) {
						case TGEDCOMSex.svMale:
							pen_color = Color.Blue;
							break;
						case TGEDCOMSex.svFemale:
							pen_color = Color.Red;
							break;
						default:
							pen_color = Color.Black;
							break;
					}
					xpen = new Pen(pen_color, ((float)((double)pen_width)));
				} else {
					xpen = new Pen(Color.Black, 1f);
				}

				this.DrawBorder(aCanvas, xpen, rt, false, person);
			}
			finally
			{
				xpen.Dispose();
			}

			if (has_port)
			{
				TRect port_rt = rt;
				port_rt.Right = port_rt.Left + person.PortraitWidth;
				port_rt.OffsetEx(3, 3);
				aCanvas.DrawImage(person.Portrait, person.GetDestRect(port_rt, person.Portrait));
				rt.Left += person.PortraitWidth;
			}

			int line = 0;
			this.TextOut(aCanvas, rt, person.Family, h, ref line);

			if (!this.Options.DiffLines) {
				this.TextOut(aCanvas, rt, person.GetFullName(), h, ref line);
			} else {
				this.TextOut(aCanvas, rt, person.Name, h, ref line);
				this.TextOut(aCanvas, rt, person.Patronymic, h, ref line);
			}

			if (!this.Options.OnlyYears) {
				if (this.Options.BirthDateVisible) {
					this.TextOut(aCanvas, rt, person.BirthDate, h, ref line);
				}

				if (this.Options.DeathDateVisible) {
					this.TextOut(aCanvas, rt, person.DeathDate, h, ref line);
				}
			} else {
				this.TextOut(aCanvas, rt, person.GetLifeYears(), h, ref line);
			}

			if (this.Options.Kinship)
			{
				this.TextOut(aCanvas, rt, person.Kinship, h, ref line);
			}

			if (this.Options.SignsVisible && !person.Signs.IsEmpty())
			{
				int i = 0;
				for (TGenEngine.TChartPersonSign cps = TGenEngine.TChartPersonSign.urRI_StGeorgeCross;
				     cps <= TGenEngine.TChartPersonSign.urUSSR_RearVeteran; cps++)
				{
					if (person.Signs.InSet(cps))
					{
						Bitmap pic = this.FSignsPic[(int)cps - 1];
						aCanvas.DrawImage(pic, rt.Right, rt.Top - 21 + i * pic.Height);
						i++;
					}
				}
			}

			if (this.PathDebug)
			{
				this.TextOut(aCanvas, rt, person.FPathDebug, h, ref line);
			}
		}

		private void Predef()
		{
			double sc = (double)(this.FScale);
			int fsz = (int)checked((long)Math.Round(unchecked((double)this.FOptions.DefFont_Size * sc)));
			string f_name;

			if (fsz <= 7) {
				f_name = "Small Fonts";
			} else {
				f_name = this.FOptions.DefFont_Name;
			}

			this.FDrawFont = new Font(f_name, ((float)fsz), FontStyle.Regular, GraphicsUnit.Point);
			this.FSpouseDistance = (int)checked((long)Math.Round(10.0 * sc));
			this.FBranchDistance = (int)checked((long)Math.Round(40.0 * sc));
			this.FLevelDistance = (int)checked((long)Math.Round(46.0 * sc));
			this.FMargins = (int)checked((long)Math.Round(40.0 * sc));
		}

		private void RecalcAncestorsChart()
		{
			int[] edges = new int[256];
			this.InitEdges(ref edges);
			TList prev = new TList();
			try
			{
				this.RecalcAnc(prev, ref edges, this.FRoot, new Point(this.FMargins, this.FMargins));
			}
			finally
			{
				prev.Dispose();
			}
		}

		private void RecalcDescendantsChart(bool aPreDef)
		{
			int[] edges = new int[256];
			this.InitEdges(ref edges);
			this.RecalcDesc(ref edges, this.FRoot, new Point(this.FMargins, this.FMargins), aPreDef);
		}

		private void RecalcChart()
		{
			if (this.FOptions.Kinship) {
				this.FGraph.FindPathTree(this.FKinRoot.Node);
			}

			int num = this.FPersons.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				TreeChartPerson p = this.FPersons[i];

				if (this.FOptions.Kinship) {
					p.Kinship = this.FindRelationship(p);
				}

				p.CalcBounds();
			}

			this.FHMax = 0;
			this.FWMax = 0;
			TTreeChartBox.TChartKind fKind = this.FKind;

			switch (fKind) {
				case TTreeChartBox.TChartKind.ckAncestors:
					this.RecalcAncestorsChart();
					break;
				case TTreeChartBox.TChartKind.ckDescendants:
					this.RecalcDescendantsChart(true);
					break;
				case TTreeChartBox.TChartKind.ckBoth:
					this.RecalcAncestorsChart();
					this.RecalcDescendantsChart(false);
					break;
			}

			this.FHMax = this.FHMax + this.FMargins - 1;
			this.FWMax = this.FWMax + this.FMargins - 1;
			this.FImageHeight = this.FHMax;
			this.FImageWidth = this.FWMax;
		}

		private void ShiftAnc(ref int[] edges, TreeChartPerson aPerson, int aOffset)
		{
			TreeChartPerson pp = aPerson;
			if (pp != null)
			{
				do
				{
					pp.PtX += aOffset;
					edges[pp.Generation] = pp.Rect.Right;
					if (pp.GetChildsCount() < 1) {
						pp = null;
					} else {
						pp = pp.GetChild(0);
					}
				}
				while (pp != null);
			}
		}

		private void RecalcAnc(TList prev, ref int[] edges, TreeChartPerson aPerson, Point aPt)
		{
			if (aPerson != null)
			{
				aPerson.Pt = aPt;
				int gen = aPerson.Generation;
				int offset;
				if (edges[gen] > 0) {
					offset = this.FBranchDistance;
				} else {
					offset = this.FMargins;
				}
				if (aPerson.Rect.Left <= edges[gen] + offset) {
					this.ShiftAnc(ref edges, aPerson, edges[gen] + offset - aPerson.Rect.Left);
				}
				edges[gen] = aPerson.Rect.Right;
				prev.Add(aPerson);
				if (aPerson.Rect.Top < 0)
				{
					offset = 0 - aPerson.Rect.Top + this.FMargins;
					int num = prev.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						TreeChartPerson pp = prev[i] as TreeChartPerson;
						pp.PtY += offset;
					}
				}

				if (aPerson.Father != null && aPerson.Mother != null)
				{
					Point xpt = new Point(aPerson.PtX - (this.FSpouseDistance + aPerson.Father.Width / 2), aPerson.PtY - this.FLevelDistance - aPerson.Height);
					this.RecalcAnc(prev, ref edges, aPerson.Father, xpt);
					xpt = new Point(aPerson.PtX + (this.FSpouseDistance + aPerson.Mother.Width / 2), aPerson.PtY - this.FLevelDistance - aPerson.Height);
					this.RecalcAnc(prev, ref edges, aPerson.Mother, xpt);
					aPerson.PtX = (aPerson.Father.PtX + aPerson.Mother.PtX) / 2;
					edges[aPerson.Generation] = aPerson.Rect.Right;
				}
				else
				{
					Point xpt = new Point(aPerson.PtX, aPerson.PtY - this.FLevelDistance - aPerson.Height);
					if (aPerson.Father != null)
					{
						this.RecalcAnc(prev, ref edges, aPerson.Father, xpt);
					}
					else
					{
						if (aPerson.Mother != null)
						{
							this.RecalcAnc(prev, ref edges, aPerson.Mother, xpt);
						}
					}
				}

				if (this.FWMax < aPerson.Rect.Right) this.FWMax = aPerson.Rect.Right;
				if (this.FHMax < aPerson.Rect.Bottom) this.FHMax = aPerson.Rect.Bottom;
			}
		}

		private void ShiftDesc(TreeChartPerson aPerson, int aOffset, bool aSingle)
		{
			if (aPerson != null)
			{
				if (object.Equals(aPerson, this.FRoot))
				{
					aSingle = false;
				}
				aPerson.PtX += aOffset;
				if (aPerson.BaseSpouse != null && (aPerson.BaseSpouse.Sex == TGEDCOMSex.svFemale || aPerson.BaseSpouse.GetSpousesCount() == 1))
				{
					this.ShiftDesc(aPerson.BaseSpouse, aOffset, aSingle);
				}
				else
				{
					if (!aSingle)
					{
						this.ShiftDesc(aPerson.Father, aOffset, aSingle);
						this.ShiftDesc(aPerson.Mother, aOffset, aSingle);
					}
					else
					{
						if (aPerson.FFlags.InSet(TreeChartPerson.TPersonFlag.pfDescByFather))
						{
							this.ShiftDesc(aPerson.Father, aOffset, aSingle);
						}
						else
						{
							if (aPerson.FFlags.InSet(TreeChartPerson.TPersonFlag.pfDescByMother))
							{
								this.ShiftDesc(aPerson.Mother, aOffset, aSingle);
							}
						}
					}
				}
			}
		}

		private void RecalcDescChilds(ref int[] edges, TreeChartPerson aPerson)
		{
			//edges = (int[])edges.Clone();
			if (aPerson.GetChildsCount() != 0)
			{
				bool fix_pair = aPerson.BaseSpouse != null && aPerson.BaseSpouse.GetSpousesCount() == 1;
				int cent_x = 0;
				if (fix_pair)
				{
					TGEDCOMSex sex = aPerson.Sex;
					if (sex != TGEDCOMSex.svMale)
					{
						if (sex == TGEDCOMSex.svFemale)
						{
							cent_x = (aPerson.BaseSpouse.Rect.Right + aPerson.Rect.Left) / 2;
						}
					}
					else
					{
						cent_x = (aPerson.Rect.Right + aPerson.BaseSpouse.Rect.Left) / 2;
					}
				}
				else
				{
					cent_x = aPerson.PtX;
				}
				int cur_y = aPerson.PtY + this.FLevelDistance + aPerson.Height;
				int childs_width = (aPerson.GetChildsCount() - 1) * this.FBranchDistance;

				int num = aPerson.GetChildsCount() - 1;
				for (int i = 0; i <= num; i++)
				{
					childs_width += aPerson.GetChild(i).Width;
				}

				int cur_x = cent_x - childs_width / 2;

				int num2 = aPerson.GetChildsCount() - 1;
				for (int i = 0; i <= num2; i++)
				{
					TreeChartPerson child = aPerson.GetChild(i);
					this.RecalcDesc(ref edges, child, new Point(cur_x + child.Width / 2, cur_y), true);
					cur_x = child.Rect.Right + this.FBranchDistance;
				}

				cur_x = aPerson.GetChild(0).PtX;
				if (aPerson.GetChildsCount() > 1)
				{
					cur_x += (aPerson.GetChild(aPerson.GetChildsCount() - 1).PtX - cur_x) / 2;
				}
				if (fix_pair)
				{
					TGEDCOMSex sex2 = aPerson.Sex;
					if (sex2 != TGEDCOMSex.svMale)
					{
						if (sex2 == TGEDCOMSex.svFemale)
						{
							this.ShiftDesc(aPerson, cur_x + (this.BranchDistance + aPerson.Width) / 2 - aPerson.PtX, true);
							this.ShiftDesc(aPerson.BaseSpouse, cur_x - (this.BranchDistance + aPerson.BaseSpouse.Width) / 2 + 1 - aPerson.BaseSpouse.PtX, true);
						}
					}
					else
					{
						this.ShiftDesc(aPerson, cur_x - (this.BranchDistance + aPerson.Width) / 2 + 1 - aPerson.PtX, true);
						this.ShiftDesc(aPerson.BaseSpouse, cur_x + (this.BranchDistance + aPerson.BaseSpouse.Width) / 2 - aPerson.BaseSpouse.PtX, true);
					}
				}
				else
				{
					this.ShiftDesc(aPerson, cur_x - aPerson.PtX, true);
				}
			}
		}

		private void RecalcDesc(ref int[] edges, TreeChartPerson aPerson, Point aPt, bool aPreDef)
		{
			//edges = (int[])edges.Clone();
			if (aPerson != null)
			{
				int gen = aPerson.Generation;
				if (aPreDef)
				{
					aPerson.Pt = aPt;
				}
				int offset;
				if (edges[gen] > 0)
				{
					offset = this.FBranchDistance;
				}
				else
				{
					offset = this.FMargins;
				}
				if (aPerson.Rect.Left <= edges[gen] + offset)
				{
					this.ShiftDesc(aPerson, edges[gen] + offset - aPerson.Rect.Left, true);
				}
				if (aPerson.Sex == TGEDCOMSex.svMale)
				{
					this.RecalcDescChilds(ref edges, aPerson);
					edges[gen] = aPerson.Rect.Right;
				}

				if (aPerson.GetSpousesCount() > 0)
				{
					TreeChartPerson prev = aPerson;
					int num = aPerson.GetSpousesCount() - 1;
					for (int i = 0; i <= num; i++)
					{
						TreeChartPerson sp = aPerson.GetSpouse(i);
						TGEDCOMSex sex = aPerson.Sex;
						Point sp_pt = new Point();
						if (sex != TGEDCOMSex.svMale)
						{
							if (sex == TGEDCOMSex.svFemale)
							{
								sp_pt = new Point(prev.Rect.Left - (this.FBranchDistance + sp.Width / 2), aPerson.PtY);
							}
						}
						else
						{
							sp_pt = new Point(prev.Rect.Right + (this.FBranchDistance + sp.Width / 2), aPerson.PtY);
						}
						this.RecalcDesc(ref edges, sp, sp_pt, true);
						if (sp.Sex != TGEDCOMSex.svMale)
						{
							prev = sp;
						}
					}
				}

				if (aPerson.Sex == TGEDCOMSex.svFemale)
				{
					this.RecalcDescChilds(ref edges, aPerson);
					edges[gen] = aPerson.Rect.Right;
				}
				if (this.FWMax < aPerson.Rect.Right)
				{
					this.FWMax = aPerson.Rect.Right;
				}
				if (this.FHMax < aPerson.Rect.Bottom)
				{
					this.FHMax = aPerson.Rect.Bottom;
				}
			}
		}

		private void DrawAncestors(Graphics aCanvas, TreeChartPerson aPerson)
		{
			this.Draw(aCanvas, aPerson.Father, TTreeChartBox.TChartKind.ckAncestors);
			this.Draw(aCanvas, aPerson.Mother, TTreeChartBox.TChartKind.ckAncestors);
			int cr_y = aPerson.PtY - this.FLevelDistance / 2;
			if (aPerson.Father != null)
			{
				this.DrawLine(aCanvas, aPerson.Father.PtX, cr_y, aPerson.PtX, cr_y);
				this.DrawLine(aCanvas, aPerson.Father.PtX, aPerson.Father.PtY + aPerson.Father.Height, aPerson.Father.PtX, cr_y);
			}
			if (aPerson.Mother != null)
			{
				this.DrawLine(aCanvas, aPerson.PtX, cr_y, aPerson.Mother.PtX, cr_y);
				this.DrawLine(aCanvas, aPerson.Mother.PtX, aPerson.Mother.PtY + aPerson.Mother.Height, aPerson.Mother.PtX, cr_y);
			}
			if (aPerson.Father != null || aPerson.Mother != null)
			{
				this.DrawLine(aCanvas, aPerson.PtX, cr_y, aPerson.PtX, aPerson.PtY);
			}
		}

		private void DrawDescendants(Graphics aCanvas, TreeChartPerson aPerson)
		{
			int num = aPerson.GetChildsCount() - 1;
			for (int i = 0; i <= num; i++)
			{
				this.Draw(aCanvas, aPerson.GetChild(i), TTreeChartBox.TChartKind.ckDescendants);
			}

			int spb_ofs = (aPerson.Height - 10) / (aPerson.GetSpousesCount() + 1);
			int spb_beg = aPerson.PtY + (aPerson.Height - spb_ofs * (aPerson.GetSpousesCount() - 1)) / 2;

			TGEDCOMSex sex = aPerson.Sex;
			if (sex != TGEDCOMSex.svMale)
			{
				if (sex == TGEDCOMSex.svFemale)
				{
					int num2 = aPerson.GetSpousesCount() - 1;
					for (int i = 0; i <= num2; i++)
					{
						int spb_v = spb_beg + spb_ofs * i;
						this.DrawLine(aCanvas, aPerson.GetSpouse(i).Rect.Right + 1, spb_v, aPerson.Rect.Left, spb_v);
					}
				}
			}
			else
			{
				int num3 = aPerson.GetSpousesCount() - 1;
				for (int i = 0; i <= num3; i++)
				{
					int spb_v = spb_beg + spb_ofs * i;
					this.DrawLine(aCanvas, aPerson.Rect.Right + 1, spb_v, aPerson.GetSpouse(i).Rect.Left, spb_v);
				}
			}

			int num4 = aPerson.GetSpousesCount() - 1;
			for (int i = 0; i <= num4; i++)
			{
				this.Draw(aCanvas, aPerson.GetSpouse(i), TTreeChartBox.TChartKind.ckDescendants);
			}

			int cr_y = aPerson.PtY + aPerson.Height + this.FLevelDistance / 2;
			int cx = 0;
			if (aPerson.BaseSpouse == null || (aPerson.BaseSpouse != null && aPerson.BaseSpouse.GetSpousesCount() > 1))
			{
				cx = aPerson.PtX;
				spb_beg = aPerson.PtY + aPerson.Height - 1;
			}
			else
			{
				TGEDCOMSex sex2 = aPerson.Sex;
				if (sex2 != TGEDCOMSex.svMale)
				{
					if (sex2 == TGEDCOMSex.svFemale)
					{
						cx = (aPerson.BaseSpouse.Rect.Right + aPerson.Rect.Left) / 2;
					}
				}
				else
				{
					cx = (aPerson.Rect.Right + aPerson.BaseSpouse.Rect.Left) / 2;
				}
				spb_beg -= spb_ofs / 2;
			}

			if (aPerson.GetChildsCount() != 0)
			{
				this.DrawLine(aCanvas, cx, spb_beg, cx, cr_y);
				if (aPerson.GetChildsCount() == 1)
				{
					Point child_pt = aPerson.GetChild(0).Pt;
					this.DrawLine(aCanvas, child_pt.X, cr_y, child_pt.X, child_pt.Y);
				}
				else
				{
					int bpx = aPerson.GetChild(0).PtX;
					int epx = aPerson.GetChild(aPerson.GetChildsCount() - 1).PtX;
					this.DrawLine(aCanvas, bpx, cr_y, epx, cr_y);
					int num5 = aPerson.GetChildsCount() - 1;
					for (int i = 0; i <= num5; i++)
					{
						Point child_pt = aPerson.GetChild(i).Pt;
						this.DrawLine(aCanvas, child_pt.X, cr_y, child_pt.X, child_pt.Y);
					}
				}
			}
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == Win32Native.WM_SIZE)
			{
				this.ScrollRange();
			}
			else if (m.Msg == Win32Native.WM_GETDLGCODE)
			{
				m.Result = (IntPtr)(m.Result.ToInt32() | 
				                    Win32Native.DLGC_WANTARROWS | Win32Native.DLGC_WANTTAB | 
				                    Win32Native.DLGC_WANTCHARS | Win32Native.DLGC_WANTALLKEYS);
			}
			else if (m.Msg == Win32Native.WM_HSCROLL)
			{
				int page = this.ClientSize.Width / 10;
				uint wParam = (uint)m.WParam.ToInt32();
				int new_pos = SysUtils.DoScroll((uint)this.Handle.ToInt32(), wParam, 0, this.LeftPos, 0, this.FRange.X, 1, page);
				this.SetLeftPos(new_pos);
			}
			else if (m.Msg == Win32Native.WM_VSCROLL)
			{
				int page = this.ClientSize.Height / 10;
				uint wParam = (uint)m.WParam.ToInt32();
				int new_pos = SysUtils.DoScroll((uint)this.Handle.ToInt32(), wParam, 1, this.TopPos, 0, this.FRange.Y, 1, page);
				this.SetTopPos(new_pos);
			}
		}

		protected void ScrollRange()
		{
			if (this.FImageWidth < base.ClientRectangle.Width) {
				this.FRange.X = 0;
				this.LeftPos = (base.ClientRectangle.Width - this.FImageWidth) / 2;
			} else {
				this.FRange.X = this.FImageWidth - base.ClientRectangle.Width;
			}

			if (this.FImageHeight < base.ClientRectangle.Height) {
				this.FRange.Y = 0;
				this.TopPos = (base.ClientRectangle.Height - this.FImageHeight) / 2;
			} else {
				this.FRange.Y = this.FImageHeight - base.ClientRectangle.Height;
			}

            Win32Native.SetScrollRange((uint)this.Handle.ToInt32(), 0, 0, this.FRange.X, false);
            Win32Native.SetScrollRange((uint)this.Handle.ToInt32(), 1, 0, this.FRange.Y, false);

			base.Invalidate();
		}

		private void SetBorderWidth(int Value)
		{
			if (this.FBorderWidth != Value)
			{
				this.FBorderWidth = Value;
				base.Invalidate();
			}
		}

		private void SetLeftPos(int Value)
		{
			if (Value < 0) Value = 0;
			if (Value > this.FRange.X) Value = this.FRange.X;

			if (this.FLeftPos != Value)
			{
				TRect dummy = TRect.Empty();
				TRect R = TRect.Empty();
                Win32Native.ScrollWindowEx((uint)this.Handle.ToInt32(), this.FLeftPos - Value, 0, ref dummy, ref dummy, 0, out R, 0u);
                Win32Native.SetScrollPos((uint)this.Handle.ToInt32(), 0, this.FLeftPos, true);
				base.Invalidate();
				this.FLeftPos = Value;
				
				this.ResetView();
			}
		}

		private void SetTopPos(int Value)
		{
			if (Value < 0) Value = 0;
			if (Value > this.FRange.Y) Value = this.FRange.Y;

			if (this.FTopPos != Value)
			{
				TRect dummy = TRect.Empty();
				TRect R = TRect.Empty();
                Win32Native.ScrollWindowEx((uint)this.Handle.ToInt32(), 0, this.FTopPos - Value, ref dummy, ref dummy, 0, out R, 0u);
                Win32Native.SetScrollPos((uint)this.Handle.ToInt32(), 1, this.FTopPos, true);
				base.Invalidate();
				this.FTopPos = Value;
				
				this.ResetView();
			}
		}

		protected void ResetView()
		{
			Size sz = this.ClientSize;
			FVisibleArea = TRect.Bounds(FLeftPos, FTopPos, sz.Width, sz.Height);
		}
		
		private void InternalDraw(Graphics aCanvas, TDrawMode mode)
		{
			Rectangle imgRect = new Rectangle(0, 0, FImageWidth, FImageHeight);
			if (this.BackgroundImage == null) {
				using (Brush brush = new SolidBrush(this.BackColor)) {
					aCanvas.FillRectangle(brush, imgRect);
				}
			} else {
				//ControlPaint.DrawBackgroundImage( aCanvas, this.BackgroundImage, BackColor, this.BackgroundImageLayout, CR, CR);
				using (TextureBrush textureBrush = new TextureBrush(this.BackgroundImage, WrapMode.Tile))
				{
					aCanvas.FillRectangle(textureBrush, imgRect);
				}
			}

			Rectangle CR = base.ClientRectangle;

			if (mode == TDrawMode.dmDefault)
			{
				this.FSPX = this.FBorderWidth - this.FLeftPos;
				this.FSPY = this.FBorderWidth - this.FTopPos;
				if (this.FImageWidth < CR.Width)
				{
					this.FSPX += (CR.Width - this.FImageWidth) / 2;
				}
				if (this.FImageHeight < CR.Height)
				{
					this.FSPY += (CR.Height - this.FImageHeight) / 2;
				}
			}
			else
			{
				this.FSPX = 0;
				this.FSPY = 0;
			}

			// this overrided routines

			this.Draw(aCanvas, this.FRoot, this.FKind);

			if (FScaleControl.Visible) FScaleControl.Draw(aCanvas);
		}

		protected void Draw(Graphics aCanvas, TreeChartPerson aPerson, TChartKind aDirKind)
		{
			if (aPerson != null)
			{
				switch (this.FKind) {
					case TTreeChartBox.TChartKind.ckAncestors:
						this.DrawAncestors(aCanvas, aPerson);
						break;

					case TTreeChartBox.TChartKind.ckDescendants:
						this.DrawDescendants(aCanvas, aPerson);
						break;

					case TTreeChartBox.TChartKind.ckBoth:
						if (aPerson == this.FRoot || aDirKind == TTreeChartBox.TChartKind.ckAncestors) this.DrawAncestors(aCanvas, aPerson);
						if (aPerson == this.FRoot || aDirKind == TTreeChartBox.TChartKind.ckDescendants) this.DrawDescendants(aCanvas, aPerson);
						break;
				}

				this.DrawPerson(aCanvas, this.FSPX, this.FSPY, aPerson);
			}
		}

		public void GenChart(TGEDCOMIndividualRecord aPerson, TChartKind aKind)
		{
			this.InternalGenChart(aPerson, aKind);
			this.ScrollRange();
		}

		protected void InternalGenChart(TGEDCOMIndividualRecord aPerson, TChartKind aKind)
		{
			try
			{
				this.FKind = aKind;
				this.FSelected = null;
				this.FPersons.Clear();
				this.Predef();
				this.FGraph.Clear();
				this.DoFilter(aPerson);
				this.FRoot = null;
				TTreeChartBox.TChartKind fKind = this.FKind;

				FPreparedIndividuals.Clear();

				switch (fKind) {
					case TChartKind.ckAncestors:
						this.FPreparedFamilies.Clear();
						this.FRoot = this.DoAncestorsStep(null, aPerson, 1, false);
						break;
						
					case TChartKind.ckDescendants:
						this.FPreparedFamilies.Clear();
						this.FRoot = this.DoDescendantsStep(null, aPerson, 1);
						break;

					case TChartKind.ckBoth:
						this.FPreparedFamilies.Clear();
						this.FRoot = this.DoAncestorsStep(null, aPerson, 1, false);
						this.FPreparedFamilies.Clear();
						this.DoDescendantsStep(null, aPerson, 1);
						break;
				}

				this.FKinRoot = this.FRoot;
				this.RecalcChart();
			}
			catch (Exception E)
			{
				SysUtils.LogWrite("TAncestryChartBox.InternalGenChart(): " + E.Message);
			}
		}

		public void DoFilter(TGEDCOMIndividualRecord aRoot)
		{
			if (this.FFilter.BranchCut != TChartFilter.TBranchCut.bcNone)
			{
				TGenEngine.InitExtCounts(this.FTree, 0);
				this.DoDescendantsFilter(aRoot);
				aRoot.ExtData = true;
			}
		}

		public EnumSet GetPersonSign(TGEDCOMIndividualRecord iRec)
		{
			EnumSet result = new EnumSet();
			int num = iRec.UserReferences.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				string rs = iRec.UserReferences[i].StringValue;
				for (TGenEngine.TChartPersonSign cps = TGenEngine.TChartPersonSign.urRI_StGeorgeCross; 
				     cps <= TGenEngine.TChartPersonSign.urLast; cps++)
				{
					if (rs == TGenEngine.UserRefs[(int)cps]) result.Include(cps);
				}
			}
			return result;
		}

		public void RebuildKinships()
		{
			try
			{
				if (this.FOptions.Kinship)
				{
					TreeChartPerson p = this.FSelected;
					if (p != null)
					{
						this.FKinRoot = p;
						this.RecalcChart();
						this.ScrollRange();
					}
				}
			}
			catch (Exception E)
			{
				SysUtils.LogWrite("TAncestryChartBox.RebuildKinships(): " + E.Message);
			}
		}

		public void SaveSnapshot(string aFileName)
		{
			string ext = Path.GetExtension(aFileName).ToLower();

			if ((ext == ".bmp" || ext == ".jpg") && this.FImageWidth >= 65535)
			{
				TGenEngine.ShowError(LangMan.LSList[380]);
			}
			else
			{
				Image pic = new Bitmap(this.FImageWidth, this.FImageHeight, PixelFormat.Format24bppRgb);
				Graphics canv = Graphics.FromImage(pic);
				try
				{
					try
					{
						this.Predef();
						this.InternalDraw(canv, TDrawMode.dmToFile);
					}
					finally
					{
						canv.Dispose();
					}

					if (ext == ".bmp") pic.Save(aFileName, ImageFormat.Bmp);
					else
					if (ext == ".emf") pic.Save(aFileName, ImageFormat.Emf);
					else 
					if (ext == ".jpg") pic.Save(aFileName, ImageFormat.Jpeg);
				}
				finally
				{
					SysUtils.Free(pic);
				}
			}
		}

		private void SetSelected(TreeChartPerson Value)
		{
			if (this.FSelected != null) this.FSelected.Selected = false;
			this.FSelected = Value;
			if (this.FSelected != null) this.FSelected.Selected = true;

			base.Invalidate();
		}

		public TreeChartPerson FindPersonByCoords(int aX, int aY)
		{
			TreeChartPerson result = null;
			
			aX -= this.FSPX;
			aY -= this.FSPY;
			int num = this.FPersons.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				TreeChartPerson p = this.FPersons[i];
				if (p.Rect.Contains(aX, aY))
				{
					result = p;
					break;
				}
			}

			return result;
		}

		public void SelectBy(int aX, int aY, bool needCenter)
		{
			TreeChartPerson p = this.FindPersonByCoords(aX, aY);
			this.SetSelected(p);
			if (p != null && needCenter && this.FTraceSelected) CenterPerson(p);
		}

		public void SelectByRec(TGEDCOMIndividualRecord iRec)
		{
			int num = this.FPersons.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				TreeChartPerson p = this.FPersons[i];
				if (p.Rec == iRec)
				{
					this.SetSelected(p);

					if (this.FTraceSelected) CenterPerson(p);

					return;
				}
			}
			this.SetSelected(null);			
		}

		public void CenterPerson(TreeChartPerson p, bool animation = true)
		{
			Point pt = p.Pt;
			int dst_x = (pt.X) - (this.ClientSize.Width / 2);
			int dst_y = (pt.Y + (p.Height / 2)) - (this.ClientSize.Height / 2);

			if (dst_x < 0) dst_x = dst_x + (0 - dst_x);
			if (dst_y < 0) dst_y = dst_y + (0 - dst_y);

			if ((this.LeftPos == dst_x) && (this.TopPos == dst_y)) return;

			if (animation) {
					TweenLibrary tween = new TweenLibrary();
					tween.startTweenEvent(delegate(int newX, int newY) { this.LeftPos = newX; this.TopPos = newY; },
					                      this.LeftPos, this.TopPos, dst_x, dst_y, "easeinoutquad", 50);
			} else {
				this.LeftPos = dst_x; 
				this.TopPos = dst_y;
			}
		}

		private bool DoDescendantsFilter(TGEDCOMIndividualRecord aPerson)
		{
			bool Result = false;
			if (aPerson != null)
			{
				TChartFilter.TBranchCut branchCut = this.FFilter.BranchCut;
				switch (branchCut) {
					case TChartFilter.TBranchCut.bcYears:
						int year = TGenEngine.GetIndependentYear(aPerson, "BIRT");
						Result = (year >= this.FFilter.BranchYear);
						break;

					case TChartFilter.TBranchCut.bcPersons:
						Result = (this.FFilter.BranchPersons.IndexOf(aPerson.XRef + ";") >= 0);
						break;
				}

				int num = aPerson.SpouseToFamilyLinks.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					TGEDCOMFamilyRecord family = aPerson.SpouseToFamilyLinks[i].Family;

					int num2 = family.Childrens.Count - 1;
					for (int j = 0; j <= num2; j++)
					{
						TGEDCOMIndividualRecord child = family.Childrens[j].Value as TGEDCOMIndividualRecord;
						bool res_child = DoDescendantsFilter(child);
						Result |= res_child;
					}
				}
				aPerson.ExtData = Result;
			}
			return Result;
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			this.InternalDraw(pe.Graphics, TDrawMode.dmDefault);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (!this.Focused) base.Focus();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if (ModifierKeys == Keys.Control) {
				float newScale = this.Scale;
				
				if (e.Delta > 0) {
					newScale -= 0.05f;
				} else {
					newScale += 0.05f;
				}
				
				if (newScale < 0.5 || newScale > 1.5) return;
				this.Scale = newScale;
			} else {
				var direction = e.Delta > 0 ? Win32Native.SB_PAGELEFT : Win32Native.SB_PAGERIGHT;
				uint msg = (ModifierKeys == Keys.Shift) ? Win32Native.WM_HSCROLL : Win32Native.WM_VSCROLL;

				Win32Native.SendMessage(this.Handle, msg, (IntPtr)direction, IntPtr.Zero);
			}
		}

	}
}
