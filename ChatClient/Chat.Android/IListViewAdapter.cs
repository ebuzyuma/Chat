using System;
using System.Collections.Generic;
using System.Collections;

namespace Chat.Core
{
	public interface IListViewAdapter
	{
		void Add (string obj);
		void AddAll(ICollection collection);
		void Remove (string obj);
		void Clear ();
	}
}

