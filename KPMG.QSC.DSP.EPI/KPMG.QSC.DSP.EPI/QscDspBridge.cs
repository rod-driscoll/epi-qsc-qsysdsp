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
using Newtonsoft.Json;

namespace QSC.DSP.EPI
{
	public static class QscDspDeviceApiExtensions 
	{
		public static void LinkToApiExt(this QscDsp DspDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{


			QscDspDeviceJoinMap joinMap = new QscDspDeviceJoinMap();

			var JoinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(JoinMapSerialized))
				joinMap = JsonConvert.DeserializeObject<QscDspDeviceJoinMap>(JoinMapSerialized);

			joinMap.OffsetJoinNumbers(joinStart);
			Debug.Console(1, DspDevice, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			ushort x = 1;
			var comm = DspDevice as ICommunicationMonitor;
			DspDevice.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
			trilist.SetStringSigAction(joinMap.Prefix, (s) => { DspDevice.SetPrefix(s);});
			trilist.SetStringSigAction(joinMap.Address, (s) => { DspDevice.SetIpAddress(s); });

			foreach (var channel in DspDevice.LevelControlPoints)
			{
				//var QscChannel = channel.Value as QSC.DSP.EPI.QscDspLevelControl;
				Debug.Console(2, "QscChannel {0} connect", x);

				var genericChannel = channel.Value as IBasicVolumeWithFeedback;
				if (channel.Value.Enabled)
				{
					trilist.StringInput[joinMap.ChannelName + x].StringValue = channel.Value.LevelCustomName;
					trilist.UShortInput[joinMap.ChannelType + x].UShortValue = (ushort)channel.Value.Type;
					trilist.BooleanInput[joinMap.ChannelVisible + x].BoolValue = true;

					genericChannel.MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.ChannelMuteToggle + x]);
					genericChannel.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.ChannelVolume + x]);

					trilist.SetSigTrueAction(joinMap.ChannelMuteToggle + x, () => genericChannel.MuteToggle());
					trilist.SetSigTrueAction(joinMap.ChannelMuteOn + x, () => genericChannel.MuteOn());
					trilist.SetSigTrueAction(joinMap.ChannelMuteOff + x, () => genericChannel.MuteOff());

					trilist.SetBoolSigAction(joinMap.ChannelVolumeUp + x, b => genericChannel.VolumeUp(b));
					trilist.SetBoolSigAction(joinMap.ChannelVolumeDown + x, b => genericChannel.VolumeDown(b));

					trilist.SetUShortSigAction(joinMap.ChannelVolume + x, u => genericChannel.SetVolume(u));
						
					
					
				}
				x++;
			}


			//Presets 
			x = 0;
			trilist.SetStringSigAction(joinMap.Presets, s => DspDevice.RunPreset(s));
			foreach (var preset in DspDevice.PresetList)
			{
				var temp = x;
				trilist.StringInput[joinMap.Presets + temp + 1].StringValue = preset.label;
				trilist.SetSigTrueAction(joinMap.Presets + temp + 1, () => DspDevice.RunPresetNumber(temp));
				x++;
			}

			// VoIP Dialer
			uint lineOffset = 0;
			foreach (var line in DspDevice.Dialers)
			{
				var dialer = line;
				var dialerLineOffset = lineOffset;
				Debug.Console(2, "AddingDialerBRidge {0} {1} Offset", dialer.Key, dialerLineOffset);
				trilist.SetSigTrueAction((joinMap.Keypad0 + dialerLineOffset), () => DspDevice.Dialers[dialer.Key].SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num0));
				trilist.SetSigTrueAction((joinMap.Keypad1 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num1));
				trilist.SetSigTrueAction((joinMap.Keypad2 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num2));
				trilist.SetSigTrueAction((joinMap.Keypad3 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num3));
				trilist.SetSigTrueAction((joinMap.Keypad4 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num4));
				trilist.SetSigTrueAction((joinMap.Keypad5 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num5));
				trilist.SetSigTrueAction((joinMap.Keypad6 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num6));
				trilist.SetSigTrueAction((joinMap.Keypad7 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num7));
				trilist.SetSigTrueAction((joinMap.Keypad8 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num8));
				trilist.SetSigTrueAction((joinMap.Keypad9 + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Num9));
				trilist.SetSigTrueAction((joinMap.KeypadStar + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Star));
				trilist.SetSigTrueAction((joinMap.KeypadPound + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Pound));
				trilist.SetSigTrueAction((joinMap.KeypadClear + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Clear));
				trilist.SetSigTrueAction((joinMap.KeypadBackspace + dialerLineOffset), () => dialer.Value.SendKeypad(QSC.DSP.EPI.QscDspDialer.eKeypadKeys.Backspace));

				trilist.SetSigTrueAction(joinMap.Dial + dialerLineOffset, () => dialer.Value.Dial());
				trilist.SetSigTrueAction(joinMap.DoNotDisturbToggle + dialerLineOffset, () => dialer.Value.DoNotDisturbToggle());
				trilist.SetSigTrueAction(joinMap.DoNotDisturbOn + dialerLineOffset, () => dialer.Value.DoNotDisturbOn());
				trilist.SetSigTrueAction(joinMap.DoNotDisturbOff + dialerLineOffset, () => dialer.Value.DoNotDisturbOff());
				trilist.SetSigTrueAction(joinMap.AutoAnswerToggle + dialerLineOffset, () => dialer.Value.AutoAnswerToggle());
				trilist.SetSigTrueAction(joinMap.AutoAnswerOn + dialerLineOffset, () => dialer.Value.AutoAnswerOn());
				trilist.SetSigTrueAction(joinMap.AutoAnswerOff + dialerLineOffset, () => dialer.Value.AutoAnswerOff());

				dialer.Value.DoNotDisturbFeedback.LinkInputSig(trilist.BooleanInput[joinMap.DoNotDisturbToggle + dialerLineOffset]);
				dialer.Value.DoNotDisturbFeedback.LinkInputSig(trilist.BooleanInput[joinMap.DoNotDisturbOn + dialerLineOffset]);
				dialer.Value.DoNotDisturbFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.DoNotDisturbOff + dialerLineOffset]);

				dialer.Value.AutoAnswerFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoAnswerToggle + dialerLineOffset]);
				dialer.Value.AutoAnswerFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoAnswerOn + dialerLineOffset]);
				dialer.Value.AutoAnswerFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.AutoAnswerOff + dialerLineOffset]);

				dialer.Value.OffHookFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Dial + dialerLineOffset]);
				dialer.Value.DialStringFeedback.LinkInputSig(trilist.StringInput[joinMap.DialStringCmd + dialerLineOffset]);
				lineOffset = lineOffset + 50;
			}

		}
	}
	public class QscDspDeviceJoinMap : JoinMapBase
	{
		public uint IsOnline { get; set; }
		public uint Address { get; set; }
		public uint Prefix { get; set; }
		public uint ChannelMuteToggle { get; set; }
		public uint ChannelMuteOn { get; set; }
		public uint ChannelMuteOff { get; set; }
		public uint ChannelVolume { get; set; }
		public uint ChannelType { get; set; }
		public uint ChannelName { get; set; }
		public uint ChannelVolumeUp { get; set; }
		public uint ChannelVolumeDown { get; set; }
		public uint Presets { get; set; }
		public uint DialStringCmd { get; set; }
		public uint Keypad0 { get; set; }
		public uint Keypad1 { get; set; }
		public uint Keypad2 { get; set; }
		public uint Keypad3 { get; set; }
		public uint Keypad4 { get; set; }
		public uint Keypad5 { get; set; }
		public uint Keypad6 { get; set; }
		public uint Keypad7 { get; set; }
		public uint Keypad8 { get; set; }
		public uint Keypad9 { get; set; }
		public uint KeypadStar { get; set; }
		public uint KeypadPound { get; set; }
		public uint KeypadClear { get; set; }
		public uint KeypadBackspace { get; set; }
		public uint Dial { get; set; }
		public uint DoNotDisturbToggle { get; set; }
		public uint DoNotDisturbOn { get; set; }
		public uint DoNotDisturbOff { get; set; }
		public uint AutoAnswerToggle { get; set; }
		public uint AutoAnswerOn { get; set; }
		public uint AutoAnswerOff { get; set; }
		public uint ChannelVisible { get; set; }

		public QscDspDeviceJoinMap()
		{
			// Arrays
			ChannelName = 200;
			ChannelMuteToggle = 400;
			ChannelMuteOn = 600; 
			ChannelMuteOff = 800;
			ChannelVolume = 200;
			ChannelVolumeUp = 1000;
			ChannelVolumeDown = 1200; 
			ChannelType = 400;
			Presets = 100;
			ChannelVisible = 200;

			// SIngleJoins
			IsOnline = 1;
			Prefix = 2;
			Address = 1;
			Presets = 100;
			DialStringCmd = 3100;
			Keypad0 = 3110;
			Keypad1 = 3111; 
			Keypad2 = 3112;
			Keypad3 = 3113; 
			Keypad4 = 3114;
			Keypad5 = 3115;
			Keypad6 = 3116;
			Keypad7 = 3117;
			Keypad8 = 3118;
			Keypad9 = 3119;
			KeypadStar = 3120;
			KeypadPound = 3121;
			KeypadClear = 3122;
			KeypadBackspace = 3123;
			DoNotDisturbToggle = 3132;
			DoNotDisturbOn = 3133;
			DoNotDisturbOff = 3134;
			AutoAnswerToggle = 3127;
			AutoAnswerOn = 3125;
			AutoAnswerOff = 3126; 
			Dial = 3124;


		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;
			Address = Address + joinOffset;
			Prefix = Prefix + joinOffset;
			ChannelName = ChannelName + joinOffset; 
			ChannelMuteToggle = ChannelMuteToggle + joinOffset;
			ChannelMuteOn = ChannelMuteOn + joinOffset;
			ChannelMuteOff = ChannelMuteOff + joinOffset;
			ChannelVolume = ChannelVolume + joinOffset;
			ChannelVolumeUp = ChannelVolumeUp + joinOffset;
			ChannelVolumeDown = ChannelVolumeDown + joinOffset;
			ChannelType = ChannelType + joinOffset;
			Presets = Presets + joinOffset;
			ChannelVisible = ChannelVisible + joinOffset;

			IsOnline = IsOnline + joinOffset; 
			DialStringCmd = DialStringCmd + joinOffset;
			Keypad0 = Keypad0 + joinOffset;
			Keypad1 = Keypad1 + joinOffset;
			Keypad2 = Keypad2 + joinOffset;
			Keypad3 = Keypad3 + joinOffset;
			Keypad4 = Keypad4 + joinOffset;
			Keypad5 = Keypad5 + joinOffset;
			Keypad6 = Keypad6 + joinOffset; 
			Keypad7 = Keypad7 + joinOffset; 
			Keypad8 = Keypad8 + joinOffset; 
			Keypad9 = Keypad9 + joinOffset; 
			KeypadStar = KeypadStar + joinOffset;
			KeypadPound = KeypadPound + joinOffset;
			KeypadClear = KeypadClear + joinOffset;
			KeypadBackspace = KeypadBackspace + joinOffset;
			DoNotDisturbToggle = DoNotDisturbToggle + joinOffset;
			DoNotDisturbOn = DoNotDisturbOn + joinOffset;
			DoNotDisturbOff = DoNotDisturbOff + joinOffset;
			AutoAnswerToggle = AutoAnswerToggle + joinOffset;
			AutoAnswerOn = AutoAnswerOn + joinOffset;
			AutoAnswerOff = AutoAnswerOff + joinOffset;
			Dial = Dial + joinOffset;
		}
	}

}