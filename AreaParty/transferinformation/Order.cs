using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transferinformation
{
    public class Order
    {
        public const int success = 200;
        public const int failure = 404;

        public const string monitorData_name = "MONITORDATA";
        public const string monitorData_command = "GET";

        public const string ownProgressAction_name = "PC";
        public const string processAction_name = "PROCESS";
        public const string processAction_command = "CLOSE";
        public const string ownProgressAction_Close = "close";

        public const string computerAction_name = "SERVERCOMPUTER";
        public const string computerAction_command_reboot = "REBOOT";
        public const string computerAction_command_shutdown = "SHUTDOWN";

        public const string fileAction_name = "FILE";
        public const string folderAction_name = "FOLDER";
        public const string fileAction_open_command = "OPENFILE";
        public const string folderAction_open_command = "OPENFOLDER";
        public const string fileOrFolderAction_delete_command = "DELETE";
        public const string fileOrFolderAction_rename_command = "RENAME";
        public const string fileOrFolderAction_copy_command = "COPY";
        public const string fileOrFolderAction_cut_command = "CUT";
        public const string folderAction_add_command = "ADDFOLDER";
        public const string fileAction_share_command = "SHAREFILE";
        public const string folderAction_addtohttp_command = "ADDTOHTTP";

        public const string folderAction_open_more_message = "MOREFILE";
        public const string folderAction_open_finish_message = "FINISHFILE";
        public const string folderAction_open_more_param = "GETMORE";

        public const string diskAction_name = "DISK";
        public const string diskAction_get_command = "GETDISKLIST";

        public const string appAction_name = "EXE";
        public const string appAction_get_command = "GETEXELIST";
        public const string appAction_get_more_message = "MOREEXE";
        public const string appAction_get_finish_message = "FINISHEXE";
        public const string appAction_get_more_param = "GETMOREEXE";

        public const string get_areaparty_path = "GETAREAPARTYPATH";

        public const string nasAction_name = "NAS";
        public const string nasAction_manager = "NASMANAGER";
        public const string nasAction_add = "NASADD";
        public const string nasAction_delete = "NASDELETE";
    
    }
}
