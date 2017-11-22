namespace client
{
	public class ProgressObj
	{
		private string fileName;
		private string fileSize;
		private string fileTotalSize;
		private string progress;
		private int state;
		public virtual string FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				this.fileName = value;
			}
		}
		public virtual string FileSize
		{
			get
			{
				return fileSize;
			}
			set
			{
				this.fileSize = value;
			}
		}
		public virtual string FileTotalSize
		{
			get
			{
				return fileTotalSize;
			}
			set
			{
				this.fileTotalSize = value;
			}
		}
		public virtual string Progress
		{
			get
			{
				return progress;
			}
			set
			{
				this.progress = value;
			}
		}
		public virtual int State
		{
			get
			{
				return state;
			}
			set
			{
				this.state = value;
			}
		}


	}

}