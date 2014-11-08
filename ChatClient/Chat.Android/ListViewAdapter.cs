using System;
using Android.Widget;
using System.Collections.Generic;
using Chat.Core;
using Android.App;
using System.Collections;

namespace Chat.Droid
{
	//TODO add generic type instead of string if needed
	public class ListViewAdapter : IListViewAdapter
	{
		private ArrayAdapter<string> _arrayAdapter; 
		private Activity _activity;

		public ListViewAdapter (ArrayAdapter<string> arrayAdapter, Activity activity)
		{
			_activity = activity;
			_arrayAdapter = arrayAdapter;
		}

		public void AddAll(ICollection collection)
		{

			_activity.RunOnUiThread(() => _arrayAdapter.AddAll (collection));
		}

		public void Clear()
		{
			_activity.RunOnUiThread(() =>_arrayAdapter.Clear ());
		}
	}
}

