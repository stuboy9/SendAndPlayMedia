using System.Collections.Generic;

namespace command
{

	public class Command_http
	{
		public Command_http(string name, string command, Dictionary<string, string> param) : base()
		{
			this.name = name;
			this.command = command;
			this.param = param;
		}
		public string name;
		public string command;
		public Dictionary<string, string> param;
		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				this.name = value;
			}
		}
        public virtual string Command
        {
            get
            {
                return command;
            }
            set
            {
                this.command = value;
            }
        }
		public virtual Dictionary<string, string> Param
		{
			get
			{
				return param;
			}
			set
			{
				this.param = value;
			}
		}

	}

}