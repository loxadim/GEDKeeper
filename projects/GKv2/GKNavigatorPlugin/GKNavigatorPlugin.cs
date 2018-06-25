﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2017 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;
using System.Runtime.InteropServices;

using GKCore;
using GKCore.Interfaces;
using GKCore.Plugins;
using GKCore.Types;

[assembly: AssemblyTitle("GKNavigatorPlugin")]
[assembly: AssemblyDescription("GEDKeeper Navigator plugin")]
[assembly: AssemblyProduct("GEDKeeper")]
[assembly: AssemblyCopyright("Copyright © 2016 by Sergey V. Zhdanovskih")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

namespace GKNavigatorPlugin
{
    public enum PLS
    {
        /* 001 */ LSID_Navigator,
    }

    public sealed class Plugin : OrdinaryPlugin, IWidget, ISubscriber
    {
        private string fDisplayName = "GKNavigatorPlugin";
        private ILangMan fLangMan;
        private readonly NavigatorData fData;

        public override string DisplayName { get { return fDisplayName; } }
        public override ILangMan LangMan { get { return fLangMan; } }
        public override IImage Icon { get { return null; } }
        public override PluginCategory Category { get { return PluginCategory.Tool; } }

        private NavigatorWidget fForm;

        internal NavigatorData Data
        {
            get { return fData; }
        }

        public Plugin()
        {
            fData = new NavigatorData();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (fForm != null) {
                    fForm.Dispose();
                    fForm = null;
                }
            }
            base.Dispose(disposing);
        }

        internal void CloseForm()
        {
            if (fForm != null) {
                fForm = null;
            }
        }

        #region IPlugin support

        public override void Execute()
        {
            if (!Host.IsWidgetActive(this)) {
                fForm = new NavigatorWidget(this);
                fForm.Show();
            } else {
                fForm.Close();
                fForm = null;
            }
        }

        public override void OnLanguageChange()
        {
            try {
                fLangMan = Host.CreateLangMan(this);
                fDisplayName = fLangMan.LS(PLS.LSID_Navigator);

                if (fForm != null) fForm.SetLang();
            } catch (Exception ex) {
                Logger.LogWrite("GKNavigatorPlugin.OnLanguageChange(): " + ex.Message);
            }
        }

        public override bool Shutdown()
        {
            bool result = true;
            try {
                CloseForm();
            } catch (Exception ex) {
                Logger.LogWrite("GKNavigatorPlugin.Shutdown(): " + ex.Message);
                result = false;
            }
            return result;
        }

        #endregion

        #region IWidget support

        void IWidget.WidgetInit(IHost host) {}

        void IWidget.BaseChanged(IBaseWindow baseWin)
        {
            if (fForm != null) {
                fForm.BaseChanged(baseWin);
            }
        }

        void IWidget.BaseClosed(IBaseWindow baseWin)
        {
            if (fForm != null) {
                fForm.BaseClosed(baseWin);
            }
        }

        void IWidget.BaseRenamed(IBaseWindow baseWin, string oldName, string newName)
        {
            fData.RenameBase(oldName, newName);
        }

        void IWidget.WidgetEnable() {}

        #endregion

        #region ISubscriber support

        public void NotifyRecord(IBaseWindow baseWin, object record, RecordAction action)
        {
            if (baseWin == null || record == null) return;

            string baseName = baseWin.Context.FileName;
            fData[baseName].NotifyRecord(baseWin, record, action);
        }

        #endregion
    }
}
