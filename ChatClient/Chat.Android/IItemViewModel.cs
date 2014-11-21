using System;

namespace Chat.Core
{
	public interface IItemViewModel
	{
		int Id { get; set; }

		void MapFrom (IItemViewModel newModel);
	}
}

