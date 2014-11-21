using System;
using System.Collections.Generic;
using System.Collections;

namespace Chat.Core
{
	public interface IListViewAdapter<T> where T : IItemViewModel
	{
		List<T> Data { get; set; } 

		void Add (T obj);
		void AddAll(ICollection collection);
		void Update (T obj);
		void Remove (T obj);
		void Clear ();
	}
}

