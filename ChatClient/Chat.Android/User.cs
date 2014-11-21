using System;

namespace Chat.Core
{
	public class User : IItemViewModel
	{
		// TODO implement if needed
		public int Id {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

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

