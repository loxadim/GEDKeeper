﻿using System;
using System.Windows.Forms;
using GKCommon;
using GKCore;

namespace GKUI.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
	public partial class TfmAbout : Form
	{
		public TfmAbout()
		{
			this.InitializeComponent();
			this.LabelCite.Text = GKData.APP_CITES;
			this.Text = LangMan.LS(LSID.LSID_MIAbout);
			this.btnClose.Text = LangMan.LS(LSID.LSID_DlgClose);
		}

		private void LabelMail_Click(object sender, EventArgs e)
		{
			GKUtils.LoadExtFile(this.LabelMail.Text);
		}

		public static void ShowAbout()
		{
			string copyright, version;
			GKUtils.GetAssemblyVersion(out copyright, out version);

			TfmAbout dlg = new TfmAbout();
			try
			{
				dlg.LabelProduct.Text = GKData.APP_TITLE;
				dlg.LabelVersion.Text = @"Version " + version;
				dlg.LabelCopyright.Text = copyright;
				dlg.ShowDialog();
			}
			finally
			{
				dlg.Dispose();
			}
		}

		private void LabelProductClick(object sender, EventArgs e)
		{
			string msg = SysInfo.GetPlatformID().ToString() + "/" + SysInfo.GetDesktopType().ToString() + 
				"/IsUnix:" + SysInfo.IsUnix().ToString();
			GKUtils.ShowMessage(msg);
		}
	}
}
