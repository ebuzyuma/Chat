using System;
using Android.Widget;
using System.Collections.Generic;
using Chat.Core;
using Android.App;
using System.Collections;
using Android.Content;
using System.Linq;

namespace Chat.Droid
{
	public sealed class ListViewAdapter<T> : ArrayAdapter<T>, IListViewAdapter<T> 
		where T : class, IItemViewModel
	{
		public List<T> Data { get; set; } 

		public ListViewAdapter (Context context, int textViewResourceId, List<T> objects) : base (context, textViewResourceId, objects)
		{
			Data = objects;
			SetNotifyOnChange (true);
		}		

		public ListViewAdapter (Context context, int textViewResourceId) : this (context, textViewResourceId, new List<T>())
		{
		}
		

		public new void Add(T obj)
		{
			base.Add (obj);
			Data.Add (obj);
		}

		public override void AddAll(ICollection collection)
		{
			base.AddAll (collection);
			Data.AddRange (collection.OfType<T>());
		}

		public void Update (T obj)
		{
			T item = Data.FirstOrDefault (p => p.Id == obj.Id);
			if (item != null)
				item.MapFrom (obj);
			NotifyDataSetChanged ();
		}

		public new void Remove(T obj)
		{
			base.Remove (obj);
			Data.Remove (obj);
		}

		public override void Clear()
		{
			base.Clear();
			Data.Clear();
		}
	}
}

