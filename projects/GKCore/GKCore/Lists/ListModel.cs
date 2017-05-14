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
using GKCommon;
using GKCommon.GEDCOM;
using GKCore.Interfaces;
using GKCore.Operations;
using GKCore.Types;

namespace GKCore.Lists
{
    public delegate void ModifyEventHandler(object sender, ModifyEventArgs eArgs);
    public delegate void ItemValidatingEventHandler(object sender, ItemValidatingEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public class ModifyEventArgs : EventArgs
    {
        public RecordAction Action { get; private set; }
        public object ItemData { get; set; }

        public ModifyEventArgs(RecordAction action, object itemData)
        {
            Action = action;
            ItemData = itemData;
        }
    }

    public class ItemValidatingEventArgs : EventArgs
    {
        private bool fIsAvailable;
        private object fItem;

        public bool IsAvailable
        {
            get { return fIsAvailable; }
            set { fIsAvailable = value; }
        }

        public object Item
        {
            get { return fItem; }
            set { fItem = value; }
        }

        public ItemValidatingEventArgs() : this(null)
        {
        }

        public ItemValidatingEventArgs(object item)
        {
            fItem = item;
        }
    }


    public enum SheetButton
    {
        lbAdd,
        lbEdit,
        lbDelete,
        lbJump,
        lbMoveUp,
        lbMoveDown
    }


    public interface ISheetList
    {
        EnumSet<SheetButton> Buttons { get; set; }

        void AddColumn(string caption, int width, bool autoSize);
        void BeginUpdate();
        void EndUpdate();
        void ClearColumns();
        void ClearItems();
        IListItem AddItem(object itemValue, object data);
        void ResizeColumn(int columnIndex);
        void UpdateSheet();
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class ListModel : BaseObject, IListSource
    {
        protected ISheetList fSheetList;
        protected readonly IBaseWindow fBaseWin;
        protected readonly ChangeTracker fUndoman;
        protected GEDCOMObject fDataOwner;

        public GEDCOMObject DataOwner
        {
            get {
                return fDataOwner;
            }
            set {
                fDataOwner = value;
                UpdateContent();
            }
        }

        public ISheetList SheetList
        {
            get {
                return fSheetList;
            }
            set {
                if (fSheetList != value) {
                    fSheetList = value;
                    InitView();
                }
            }
        }

        protected ListModel(IBaseWindow baseWin, ChangeTracker undoman)
        {
            fBaseWin = baseWin;
            fUndoman = undoman;
        }

        public abstract void InitView();
        public abstract void UpdateContent();
        public abstract void Modify(object sender, ModifyEventArgs eArgs);
    }
}
