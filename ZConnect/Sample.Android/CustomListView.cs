using Android.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.MyAndroid
{
	class CustomListView : BaseAdapter<ListViewItem>
	{
		public List<ListViewItem> items;
		public Activity activity;
		public CustomListView(Activity activity, List<ListViewItem> items)
			: base()
		{
			this.activity = activity;
			this.items = items;
		}
		public override long GetItemId(int position)
		{
			return position;
		}
		public override ListViewItem this[int position]
		{
			get { return items[position]; }
		}
		public override int Count
		{
			get { return items.Count; }
		}
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = items[position];
			View view = convertView;
			if (view == null)
			{
				view = activity.LayoutInflater.Inflate(Resource.Layout.CustomImageToast, null);
			}
			view.FindViewById<TextView>(Resource.Id.customImageToastTextViewTitle).Text = item.Title;
			view.FindViewById<ImageView>(Resource.Id.customImageToastImageView).SetImageBitmap(item.Thumbnail);
			view.FindViewById<TextView>(Resource.Id.customImageToastTextViewCaption).Text = item.Caption;
			return view;
		}
	}
}
