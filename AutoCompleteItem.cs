﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaEditor
{
    abstract class IAutoCompleteItem : IComparable
    {
        public abstract string getName();
        public abstract string getACString();
        public abstract string getToolTipString();

        public Int32 CompareTo(IAutoCompleteItem other)
        {
            return getName().CompareTo(other.getName());
        }

        public int CompareTo(Object obj)
        {
            IAutoCompleteItem item = obj as IAutoCompleteItem;
            if (item != null)
                return CompareTo(item);
            else {
                throw new ArgumentException("Object is not a IAutoCompleteItem");
            }
        }
    }
}