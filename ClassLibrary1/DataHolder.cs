namespace ClassLibrary1
{
   public class DataHolder
   {
		private object obj;

		public DataHolder(object Object)
		{
			this.obj = Object;
		}
		public void SetData(object Object)
		{
			obj = Object;
		}
		public void SetData(bool Object)
		{
			obj = Object;
		}

		public void SetData(int Object)
		{
			obj = Object;
		}

		public void GetData(out byte[] Data)
		{
			Data = (byte[])obj;
		}

		private void GetData_bool(out bool Data)
		{
			Data = (bool)obj;
		}

		private void GetData_int(out bool Data)
		{
			Data = (bool)obj;
		}

	}
}
