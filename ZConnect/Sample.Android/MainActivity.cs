using Android.App;
using Android.Content.PM;
using Android.Widget;
using ZXing.Mobile;
using NanoLib;
using Android.Content;
using System;
using Codeplex.Data;
using Android.Graphics;
using Android.OS;
using NanoLib.AndroidLib;
using NanoLib.HeartRails;

namespace Sample.MyAndroid
{
	[Activity (Label = "ZConnect.Main", MainLauncher = true, ConfigurationChanges=ConfigChanges.Orientation|ConfigChanges.KeyboardHidden)]
	public class Activity1 : Activity
	{
		Button buttonScanDefaultView;
		MobileBarcodeScanner scanner;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			UIHelper.init(this);
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			//Create a new instance of our Scanner
			scanner = new MobileBarcodeScanner(this);
			buttonScanDefaultView = this.FindViewById<Button>(Resource.Id.mainButtonNew);
			buttonScanDefaultView.Click += async delegate {
				//Tell our scanner to use the default overlay
				scanner.UseCustomOverlay = false;
				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";
				//Start scanning
				var result = await scanner.Scan();
				HandleScanResult(result);
			};

			var buttonBook = this.FindViewById<Button>(Resource.Id.mainButtonBook);
			buttonBook.Click += delegate
			{
				UIHelper.DelayedToast("Book", 10);
				var activity2 = new Intent(this, typeof(Activity2));
				activity2.PutExtra("genre", "Book");
				StartActivity(activity2);  
			};
			var buttonGame = this.FindViewById<Button>(Resource.Id.mainButtonGame);
			buttonGame.Click += delegate
			{
				UIHelper.DelayedToast("Game", 10);
				var activity2 = new Intent(this, typeof(Activity2));
				activity2.PutExtra("genre", "game");
				StartActivity(activity2);
			};
			var buttonCd = this.FindViewById<Button>(Resource.Id.mainButtonCD);
			buttonCd.Click += delegate
			{
				UIHelper.DelayedToast("CD", 10);
				var activity2 = new Intent(this, typeof(Activity2));
				activity2.PutExtra("genre", "CD");
				StartActivity(activity2);
			};
			var buttonDVD = this.FindViewById<Button>(Resource.Id.mainButtonDVD);
			buttonDVD.Click += delegate
			{
				UIHelper.DelayedToast("DVD", 10);
				var activity2 = new Intent(this, typeof(Activity2));
				activity2.PutExtra("genre", "DVD");
				StartActivity(activity2);
			};

			//	円グラフ読み込み及び作成
			var imageViewPieChart = FindViewById<ImageView>(Resource.Id.mainImageViewPieChart);
			if (SDCardHelper.ISExistFile(Constant.APP_NAME + "/PieChart.jpg"))
			{
				byte[] bytes = SDCardHelper.loadBytes(Constant.APP_NAME + "/PieChart.jpg");
				Bitmap bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
				imageViewPieChart.SetImageBitmap(bitmap);
			}
			var heartRailsAPI = new HeartRailsAPI();
			string[] genres = {"Book", "Game", "CD", "DVD"};
			int cnt = 0;
			for (int i = 0; i < 4; i++)
			{
				string datDirectoryPath = MyData.GetDatDirectory(genres[i]);
				if (!SDCardHelper.ISExistDirectory(datDirectoryPath)) continue;
				int num = SDCardHelper.GetFiles(datDirectoryPath).Length;
				heartRailsAPI.AddElement(genres[i], num);
				cnt++;
			}
			if (cnt == 0) heartRailsAPI.AddElement("None", 100);
//			heartRailsAPI.Query.title_top = "Genre Pie Chart";
			heartRailsAPI.CallBackBytes += (byte[] bytes) =>
			{
				SDCardHelper.save(Constant.APP_NAME + "/PieChart.jpg", bytes);
				Bitmap bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
				imageViewPieChart.SetImageBitmap(bitmap);
			};
			heartRailsAPI.start();
		}

		protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (resultCode == Android.App.Result.Ok && requestCode == 7083)
			{
				double latitude  = data.GetDoubleExtra("Latitude", 0.0);
				double longitude = data.GetDoubleExtra("Longitude", 0.0);
				UIHelper.DelayedToast("Latitude" + latitude, 0);
				UIHelper.DelayedToast("Longitude" + longitude, 0);
			}
			UIHelper.DelayedToast("active", 0);
		}

		void HandleScanResult (ZXing.Result result)
		{
			string msg = "";
			if (result != null && !string.IsNullOrEmpty(result.Text))
			{
				string isbn = result.Text;
				msg = "Found Barcode: " + isbn;
				SetImageViewFromRakutenTotal(isbn);
			}
			else
			{
				msg = "Scanning Canceled!";
			}
			this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
		}

		void SetImageViewFromRakutenTotal(string isbnjan)
		{
			var rakutenAPI = new RakutenTotalAPI();
			rakutenAPI.Query.isbnjan = isbnjan;
			SetImageViewFromRakuten(rakutenAPI);
		}
		void SetImageViewFromRakutenDVD(string jan)
		{
			var rakutenAPI = new RakutenDVDAPI();
			rakutenAPI.Query.jan = jan;
			SetImageViewFromRakuten(rakutenAPI);
		}
		void SetImageViewFromRakutenCD(string jan)
		{
			var rakutenAPI = new RakutenCDAPI();
			rakutenAPI.Query.jan = jan;
			SetImageViewFromRakuten(rakutenAPI);
		}
		void SetImageViewFromRakutenBook(string isbn) {
			var rakutenAPI = new RakutenBookAPI();
			rakutenAPI.Query.isbn = isbn;
			SetImageViewFromRakuten(rakutenAPI);
		}
		void SetImageViewFromRakuten(RakutenAPI rakutenAPI)
		{
			rakutenAPI.Query.hits = 5;
			rakutenAPI.CallBackString += (string json) =>
			{
				try
				{
					var parse = DynamicJson.Parse(json);
					//	検索結果商品数
					int count = (int)parse.count;
					int cnt = 0;
					for (int i = 0; i < count; i++)
 					{
						var item = parse.Items[i].Item;
						string isbn = (string)item.isbn;
						string jan = (string)item.jan;
						string title = (string)item.title;
						string largeImageUrl = (string)item.largeImageUrl;
						string itemCaption = (string)item.itemCaption;
						//	先頭から3文字を取得
						//	ex)	"001004008006/001017005004"	→	"001"	→	"Book"
						int genreId = int.Parse(((string)item.booksGenreId).Substring(0, 3));
						string genreName = RakutenAPI.GetGenreName(genreId);

						var downloadHelper = new DownloadHelper();
						downloadHelper.CallBackBytes += (byte[] bytes) =>
						{
							string imgFilePath = MyData.GetImgFilePath(genreName, isbn, jan);
							string datFilePath = MyData.GetDatFilePath(genreName, isbn, jan);
							var obj = new
							{
								isbn = isbn,
								jan = jan,
								title = title,
								itemCaption = itemCaption,
								genreName = genreName
							};
							SDCardHelper.save(datFilePath, DynamicJson.Serialize(obj));
							SDCardHelper.save(imgFilePath, bytes);
							Bitmap bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
							UIHelper.showImageToast(title, bitmap, itemCaption);
						};
						downloadHelper.BytesFromUrl(largeImageUrl);
					}
				}
				catch (Exception ee)
				{
					UIHelper.DelayedToast("データの解析失敗…", 10);
				}
			};
			rakutenAPI.start();
			UIHelper.DelayedToast("情報取得開始", 10);
		}
	}
}