using System.Collections.Generic;
using System.Collections;

namespace ABPUtils
{
    public class Configurations
    {
        public Configurations()
        {
            EasyListFlag = new List<string>();
            EasyPrivacyFlag = new List<string>();
            RemovedItems = new List<string>();
            NewItems = new List<string>();
            ModifyItems = new List<ModifyItem>();
        }

        public List<string> EasyListFlag
        {
            get;
            set;
        }

        public List<string> EasyPrivacyFlag
        {
            get;
            set;
        }

        public List<string> RemovedItems
        {
            get;
            set;
        }

        public List<string> NewItems
        {
            get;
            set;
        }

        public List<ModifyItem> ModifyItems
        {
            get;
            set;
        }

    }

    public class ModifyItem
    {
        public string OldItem
        {
            get;
            set;
        }

        public string NewItem
        {
            get;
            set;
        }
    }
}
