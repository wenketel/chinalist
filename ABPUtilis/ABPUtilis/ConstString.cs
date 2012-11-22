using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABPUtils
{
    internal class ConstString
    {
        public const string PATCH_FILE = "patch.xml";
        public const string EASYLIST = "easylist.txt";
        public const string CHINALIST_LAZY_HEADER = @"[Adblock Plus 1.2]
!  Adblock Plus List with Main Focus on Chinese Sites.
!  Last Modified:  
!  Homepage: http://adblock-chinalist.googlecode.com/
!
!  ChinaList Lazy = Part of EasyList + ChinaList + Part of EasyPrivacy
!  If you need to know the details,
!  please visit: https://code.google.com/p/adblock-chinalist/wiki/something_about_ChinaList_Lazy
!
!  If you need help or have any question,
!  please visit: http://adblock-chinalist.googlecode.com/
!
!  coding: utf-8, expires: 5 days
!--CC-BY-SA 3.0 + Licensed, NO WARRANTY but Best Wishes----
";
        public const string HELP_INFO = @"Copyright (C) 2008 - 2012 Adblock Plus ChinaList Project
This is free software. You may redistribute copies of it under the terms of
the GNU LGPL License <http://www.gnu.org/copyleft/lesser.html>.
Usage: ABPUtils.exe -n -d=google.com -dns=8.8.8.8
       ABPUtils.exe -v -i=adblock.txt
       ABPUtils.exe -u -i=adblock.txt
       ABPUtils.exe -m -i=adblock.txt -patch -o=adblock-lazy.txt

  version        Show ABPUtils version.

  c, check       Check the domains in the specific input file.

  d, domain      The domain need to be checked (required).

  m, merge       Merge the specific input file with Part of EasyList and
                 EasyPrivacy.

  n, nsookup     Show the ns server of the specific domain.

  u, update      Update and validate the checksum of the specific input file.

  v, validate    Validate the checksum of the specific input file.

  i, input       Input file with filters to process (required).

  o, output      Output file with processed filters.

  patch          Use the patch.xml.

  p, proxy       Proxy server used when download the lastest EasyList and EasyPrivacy files (optional).

  dns            DNS server (optional).

  h, help        Dispaly this help screen.";

        public const string EASYLIST_URL = "https://easylist-downloads.adblockplus.org/easylist.txt";
        public const string EASYPRIVACY = "easyprivacy.txt";
        public const string EASYPRIVACY_URL = "https://easylist-downloads.adblockplus.org/easyprivacy.txt";
        public const string CHINALIST_LAZY_HEADER_MARK = "!----------------------------White List--------------------";
        public const string CHINALIST_END_MARK = "!------------------------End of List-------------------------";

        public const string EASYLIST_EASYLIST_GENERAL_BLOCK = "easylist:easylist/easylist_general_block.txt";
        public const string EASYLIST_EASYLIST_GENERAL_HIDE = "easylist:easylist/easylist_general_hide.txt";
        public const string EASYLIST_EASYLIST_GENERAL_POPUP = "easylist:easylist/easylist_general_block_popup.txt";
        public const string EASYLIST_GENERAL_BLOCK_DIMENSIONS = "easylist:easylist/easylist_general_block_dimensions.txt";
        public const string EASYLIST_EASYLIST_ADSERVERS = "easylist:easylist/easylist_adservers.txt";
        public const string EASYLIST_ADSERVERS_POPUP = "easylist:easylist/easylist_adservers_popup.txt";
        public const string EASYLIST_EASYLIST_THIRDPARTY = "easylist:easylist/easylist_thirdparty.txt";
        public const string EASYLIST_THIRDPARTY_POPUP = "easylist:easylist/easylist_thirdparty_popup.txt";

        public const string EASYPRIVACY_TRACKINGSERVERS_INTERNATIONAL = "easylist:easyprivacy/easyprivacy_trackingservers_international.txt";
        public const string EASYPRIVACY_THIRDPARTY_INTERNATIONAL = "easylist:easyprivacy/easyprivacy_thirdparty_international.txt";
        public const string EASYPRIVACY_SPECIFIC_INTERNATIONAL = "easylist:easyprivacy/easyprivacy_specific_international.txt";
        public const string EASYPRIVACY_WHITELIST = "easylist:easyprivacy/easyprivacy_whitelist.txt";
        public const string EASYPRIVACY_WHITELIST_INTERNATIONAL = "easylist:easyprivacy/easyprivacy_whitelist_international.txt";
        public const string HEAD = "[Adblock Plus";
    }
}
