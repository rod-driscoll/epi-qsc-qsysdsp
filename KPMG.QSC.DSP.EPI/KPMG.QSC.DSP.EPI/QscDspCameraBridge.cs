using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Bridges;
using QSC.DSP.EPI;

namespace QSC.DSP.EPI
{
	public static class QscDspCameraDeviceApiExtensions
	{
		public static void LinkToApiExt(this QscDspCamera camera, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as QscDspCameraDeviceJoinMap;

			if (joinMap == null)
				joinMap = new QscDspCameraDeviceJoinMap();

			joinMap.OffsetJoinNumbers(joinStart);
			Debug.Console(1, camera, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			trilist.SetBoolSigAction(joinMap.Up, (b) =>
				{
					if (b) { camera.MoveCamera(eCameraPtzControls.TiltUp); }
					else { camera.MoveCamera(eCameraPtzControls.Stop); }

				});
			trilist.SetBoolSigAction(joinMap.Down, (b) =>
			{
				if (b) { camera.MoveCamera(eCameraPtzControls.TiltDown); }
				else { camera.MoveCamera(eCameraPtzControls.Stop); }

			});
			trilist.SetBoolSigAction(joinMap.Left, (b) =>
			{
				if (b) { camera.MoveCamera(eCameraPtzControls.PanLeft); }
				else { camera.MoveCamera(eCameraPtzControls.Stop); }

			});
			trilist.SetBoolSigAction(joinMap.Right, (b) =>
			{
				if (b) { camera.MoveCamera(eCameraPtzControls.PanRight); }
				else { camera.MoveCamera(eCameraPtzControls.Stop); }

			});
			trilist.SetBoolSigAction(joinMap.ZoomIn, (b) =>
			{
				if (b) { camera.MoveCamera(eCameraPtzControls.ZoomIn); }
				else { camera.MoveCamera(eCameraPtzControls.Stop); }

			});
			trilist.SetBoolSigAction(joinMap.ZoomOut, (b) =>
			{
				if (b) { camera.MoveCamera(eCameraPtzControls.ZoomOut); }
				else { camera.MoveCamera(eCameraPtzControls.Stop); }

			});
			ushort x = 1;
			foreach (var preset in camera.Config.Presets)
			{
				var temp = x;
				trilist.SetSigTrueAction(joinMap.PresetRecallStart + x, () => camera.RecallPreset(temp));
				trilist.SetSigTrueAction(joinMap.PresetStoreStart + x, () => camera.SavePreset(temp));
				trilist.SetStringSigAction(joinMap.PresetNamesStart + temp, (s) =>
				{
					camera.WritePresetName(s, temp);
				});
				preset.Value.LabelFeedback.LinkInputSig(trilist.StringInput[joinMap.PresetNamesStart + temp]);

				x++;
			}

		}
	}
	public class QscDspCameraDeviceJoinMap : JoinMapBase
	{

		public uint Up { get; set; }
		public uint Down { get; set; }
		public uint Left { get; set; }
		public uint Right { get; set; }
		public uint ZoomIn { get; set; }
		public uint ZoomOut { get; set; }
		public uint PresetRecallStart { get; set; }
		public uint PresetStoreStart { get; set; }
		public uint PresetNamesStart { get; set; }



		public QscDspCameraDeviceJoinMap()
		{
			// Arrays
			Up = 1;
			Down = 2;
			Left = 3;
			Right = 4;
			ZoomIn = 5;
			ZoomOut = 6;
			PresetRecallStart = 10;
			PresetStoreStart = 30;
			PresetNamesStart = 1;
		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;
			Up = Up + joinOffset;
			Down = Down + joinOffset;
			Left = Left + joinOffset;
			Right = Right + joinOffset;
			ZoomIn = ZoomIn + joinOffset;
			PresetNamesStart = PresetNamesStart + joinOffset;
			ZoomOut = ZoomOut + joinOffset;
			PresetRecallStart = PresetRecallStart + joinOffset;
			PresetStoreStart = PresetStoreStart + joinOffset;

		}
	}

}