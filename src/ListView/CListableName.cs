/* CListableName.cs
 * 
 * Copyright 2009 Alexander Curtis <alex@logicmill.com>
 * This file is part of GEDmill - A family history website creator
 * 
 * GEDmill is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * GEDmill is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with GEDmill.  If not, see <http://www.gnu.org/licenses/>.
 *
 *
 * History:  
 * 10Dec08 AlexC          Migrated from GEDmill 1.10
 *
 */

using System;
using System.Windows.Forms;
using GKCommon.GEDCOM;

namespace GEDmill.ListView
{
    /// <summary>
    /// Holds a name (both surname and first names) for use in a list box and provides sorting by surname then firstname.
    /// </summary>
    public class CListableName : ListViewItem.ListViewSubItem
    {
        // The string to display if no other name is present in this instance
        public static string UnknownName;

        // The surname (for sorting)
        private string fSurname;

        // The first name(s)
        private string fFirstName;

        // The record that this item represents
        private GEDCOMIndividualRecord fRecord;


        public CListableName(GEDCOMIndividualRecord ir, string surname, string firstname)
        {
            fRecord = ir;
            fSurname = surname;
            fFirstName = firstname;

            base.Text = ToString();
        }

        // To display the name in the list
        public override string ToString()
        {
            string name = "";
            if (fFirstName != "" && fSurname != "") {
                name = String.Concat(fSurname, ", ", fFirstName);
            } else if (fSurname != "") {
                name = fSurname;
            } else {
                name = fFirstName;
            }

            if (name == "") {
                name = UnknownName;
            }

            return name;
        }

        // To sort the list
        public int CompareTo(CListableName other)
        {
            if (fSurname != "" && other.fSurname != "") {
                int result = fSurname.CompareTo(other.fSurname);
                if (result != 0) {
                    return result;
                }
            } else if (fSurname != "") {
                return 1;
            } else if (other.fSurname != "") {
                return -1;
            }

            if (fFirstName != "" && other.fFirstName != "") {
                return fFirstName.CompareTo(other.fFirstName);
            }

            if (fFirstName != "") {
                return 1;
            }

            if (other.fFirstName != "") {
                return -1;
            }

            return 0;
        }
    }
}
