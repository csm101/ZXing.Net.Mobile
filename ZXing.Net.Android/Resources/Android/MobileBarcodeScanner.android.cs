using System;
using System.Threading.Tasks;
using Android.Content;
using ZXing;
using Android.OS;
using Microsoft.Maui.ApplicationModel;

namespace ZXing.Mobile
{

	public partial class MobileBarcodeScanner : MobileBarcodeScannerBase
	{
		public const string TAG = "ZXing.Net.Mobile";

		public Android.Views.View CustomOverlay { get; set; }

        Context GetContext(Context context)
			=> Platform.CurrentActivity ?? Platform.AppContext;

		internal void PlatformScanContinuously(MobileBarcodeScanningOptions options, Action<Result> scanHandler)
			=> ScanContinuously(null, options, scanHandler);

		public void ScanContinuously(Context context, MobileBarcodeScanningOptions options, Action<Result> scanHandler)
		{
			var ctx = GetContext(context);
			var scanIntent = new Intent(ctx, typeof(ZxingActivity));

			scanIntent.AddFlags(ActivityFlags.NewTask);

			ZxingActivity.UseCustomOverlayView = this.UseCustomOverlay;
			ZxingActivity.CustomOverlayView = this.CustomOverlay;
			ZxingActivity.ScanningOptions = options;
			ZxingActivity.ScanContinuously = true;
			ZxingActivity.TopText = TopText;
			ZxingActivity.BottomText = BottomText;

			ZxingActivity.ScanCompletedHandler = (Result result)
				=> scanHandler?.Invoke(result);

			ctx.StartActivity(scanIntent);
		}

		internal Task<Result> PlatformScan(MobileBarcodeScanningOptions options)
			=> Scan(null, options);

		public Task<Result> Scan(Context context, MobileBarcodeScanningOptions options)
		{
			var ctx = GetContext(context);

			var task = Task.Factory.StartNew(() =>
			{

				var waitScanResetEvent = new System.Threading.ManualResetEvent(false);

				var scanIntent = new Intent(ctx, typeof(ZxingActivity));

				scanIntent.AddFlags(ActivityFlags.NewTask);

				ZxingActivity.UseCustomOverlayView = this.UseCustomOverlay;
				ZxingActivity.CustomOverlayView = this.CustomOverlay;
				ZxingActivity.ScanningOptions = options;
				ZxingActivity.ScanContinuously = false;
				ZxingActivity.TopText = TopText;
				ZxingActivity.BottomText = BottomText;

				Result scanResult = null;

				ZxingActivity.CanceledHandler = () => waitScanResetEvent.Set();

				ZxingActivity.ScanCompletedHandler = (Result result) =>
				{
					scanResult = result;
					waitScanResetEvent.Set();
				};

				ctx.StartActivity(scanIntent);

				waitScanResetEvent.WaitOne();

				return scanResult;
			});

			return task;
		}

		internal void PlatformCancel()
			=> ZxingActivity.RequestCancel();

		internal void PlatformAutoFocus()
			=> ZxingActivity.RequestAutoFocus();

		internal void PlatformTorch(bool on)
		{
			PlatformIsTorchOn = on;
			ZxingActivity.RequestTorch(on);
		}

		internal void PlatformToggleTorch()
			=> Torch(!PlatformIsTorchOn);

		internal void PlatformPauseAnalysis()
			=> ZxingActivity.RequestPauseAnalysis();

		internal void PlatformResumeAnalysis()
			=> ZxingActivity.RequestResumeAnalysis();

        internal bool PlatformIsTorchOn { get; private set; } = false;

        internal static void LogDebug(string format, params object[] args)
			=> Android.Util.Log.Debug("ZXING", format, args);

		internal static void LogError(string format, params object[] args)
			=> Android.Util.Log.Error("ZXING", format, args);

		internal static void LogInfo(string format, params object[] args)
			=> Android.Util.Log.Info("ZXING", format, args);

		internal static void LogWarn(string format, params object[] args)
			=> Android.Util.Log.Warn("ZXING", format, args);
	}
}
