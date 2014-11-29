using System;

namespace Chat.Core
{
	public class User : IItemViewModel
	{

		public int Id { get; set; }

		public string Name { get; set; }

		public User ()
		{
		}

		public User (string name)
		{
			this.Name = name;
		}
		
		public void MapFrom (IItemViewModel old)
		{
			Name = ((User)old).Name;
		}

		public override string ToString ()
		{
			return Name;
		}
	}
}

