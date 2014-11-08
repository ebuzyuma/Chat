using System;
using System.Collections.Generic;
using System.Collections;

namespace Chat.Core
{
	public interface IListViewAdapter
	{
		void AddAll(ICollection collection);
		void Clear ();
	}
}

