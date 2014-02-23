using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Locations;
using Android.Util;
using Android.Widget;
using ZXing;
using ZXing.Mobile;
using NanoLib;
using System.Net;
using Android.Content;
using Java.Interop;
using System;
using Codeplex.Data;
using Android.Graphics;
using Android.OS;
using System.IO;
using NanoLib.AndroidLib;

namespace Sample.MyAndroid
{
	[Activity(Label = "ZConnect.List")]
	public class Activity2 : Activity
	{
		private List<ListViewItem> items = new List<ListViewItem>();
		private ListView listView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			UIHelper.init(this);
			string genre = Intent.GetStringExtra("genre") ?? null;
			if (genre == null)
			{
				UIHelper.ShowToast("There is no genre...");
				Finish();
			}
			else UIHelper.ShowToast("This genre is " + genre);
			Bitmap noImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.no_image);

			string datDirectoryPath = MyData.GetDatDirectory(genre);
			string imgDirectoryPath = MyData.GetImgDirectory(genre);
			if (SDCardHelper.ISExistDirectory(datDirectoryPath))
			{
				string[] datFiles = SDCardHelper.GetFiles(datDirectoryPath);
				foreach (var filePath in datFiles)
				{
					try
					{
						string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
						string json = SDCardHelper.loadText(datDirectoryPath + "/" + fileName + ".dat");
						var parse = DynamicJson.Parse(json);
						string title = (string)parse.title;
						string caption = (string)parse.itemCaption;
						var listViewItem = new ListViewItem()
						{
							Title = title,
							Thumbnail = noImage,
							Caption = caption
						};
						byte[] bytes = SDCardHelper.loadBytes(imgDirectoryPath + "/" + fileName + ".jpg");
						Bitmap bitmap = null;
						if (bytes != null) bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
						if (bitmap != null) {
//							UIHelper.showImageToast(title, bitmap, caption);
							listViewItem.Thumbnail = bitmap;
						}
						items.Add(listViewItem);
					}
					catch (Exception e)
					{
						UIHelper.ShowToast(e.Message);
					}
				}
			}

			listView = new ListView(this);
			listView.Adapter = new CustomListView(this, items);
			SetContentView(listView);
		}
	}
}