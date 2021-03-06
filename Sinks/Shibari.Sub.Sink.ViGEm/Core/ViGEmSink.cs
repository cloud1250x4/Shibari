﻿#define X360

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Serilog;
using Shibari.Sub.Core.Shared.Types.Common;
using Shibari.Sub.Core.Shared.Types.Common.Sinks;
using Shibari.Sub.Core.Shared.Types.DualShock3;


namespace Shibari.Sub.Sink.ViGEm.Core
{
#if DS4

    [ExportMetadata("Name", "ViGEm Sink")]
    [Export(typeof(ISinkPlugin))]
    public class ViGEmSink : ISinkPlugin
    {
        private readonly Dictionary<DualShock3Buttons, DualShock4Buttons> _btnMap;
        private readonly ViGEmClient _client;

        private readonly Dictionary<IDualShockDevice, DualShock4Controller> _deviceMap =
            new Dictionary<IDualShockDevice, DualShock4Controller>();

        public ViGEmSink()
        {
            _btnMap = new Dictionary<DualShock3Buttons, DualShock4Buttons>
            {
                {DualShock3Buttons.Select, DualShock4Buttons.Share},
                {DualShock3Buttons.LeftThumb, DualShock4Buttons.ThumbLeft},
                {DualShock3Buttons.RightThumb, DualShock4Buttons.ThumbRight},
                {DualShock3Buttons.Start, DualShock4Buttons.Options},
                {DualShock3Buttons.LeftTrigger, DualShock4Buttons.TriggerLeft},
                {DualShock3Buttons.RightTrigger, DualShock4Buttons.TriggerRight},
                {DualShock3Buttons.LeftShoulder, DualShock4Buttons.ShoulderLeft},
                {DualShock3Buttons.RightShoulder, DualShock4Buttons.ShoulderRight},
                {DualShock3Buttons.Triangle, DualShock4Buttons.Triangle},
                {DualShock3Buttons.Circle, DualShock4Buttons.Circle},
                {DualShock3Buttons.Cross, DualShock4Buttons.Cross},
                {DualShock3Buttons.Square, DualShock4Buttons.Square}
            };

            _client = new ViGEmClient();
        }

        public event RumbleRequestReceivedEventHandler RumbleRequestReceived;

        public void DeviceArrived(IDualShockDevice device)
        {
            Log.Information("Device {Device} got attached", device);

            var target = new DualShock4Controller(_client);

            _deviceMap.Add(device, target);

            target.FeedbackReceived += (sender, args) =>
            {
                var source = _deviceMap.First(m => m.Value.Equals(sender)).Key;

                RumbleRequestReceived?.Invoke(source, new RumbleRequestEventArgs(args.LargeMotor, args.SmallMotor));
            };

            try
            {
                Log.Information("Connecting ViGEm target {Target}", target);
                target.Connect();
                Log.Information("ViGEm target {Target} connected successfully", target);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to connect target {@Target}: {Exception}", target, ex);
            }
        }

        public void DeviceRemoved(IDualShockDevice device)
        {
            Log.Information("Device {Device} got removed", device);

            _deviceMap[device].Dispose();
            _deviceMap.Remove(device);
        }

        public void InputReportReceived(IDualShockDevice device, IInputReport report)
        {
            switch (device.DeviceType)
            {
                case DualShockDeviceType.DualShock3:

                    var target = _deviceMap[device];

                    var ds3Report = (DualShock3InputReport)report;
                    var ds4Report = new DualShock4Report();

                    ds4Report.SetAxis(DualShock4Axes.LeftThumbX, ds3Report[DualShock3Axes.LeftThumbX]);
                    ds4Report.SetAxis(DualShock4Axes.LeftThumbY, ds3Report[DualShock3Axes.LeftThumbY]);
                    ds4Report.SetAxis(DualShock4Axes.RightThumbX, ds3Report[DualShock3Axes.RightThumbX]);
                    ds4Report.SetAxis(DualShock4Axes.RightThumbY, ds3Report[DualShock3Axes.RightThumbY]);
                    ds4Report.SetAxis(DualShock4Axes.LeftTrigger, ds3Report[DualShock3Axes.LeftTrigger]);
                    ds4Report.SetAxis(DualShock4Axes.RightTrigger, ds3Report[DualShock3Axes.RightTrigger]);

                    ds4Report.SetButtons(_btnMap.Where(m => ds3Report.EngagedButtons.Contains(m.Key))
                        .Select(m => m.Value).ToArray());

                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadUp))
                        ds4Report.SetDPad(DualShock4DPadValues.North);
                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadRight))
                        ds4Report.SetDPad(DualShock4DPadValues.East);
                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadDown))
                        ds4Report.SetDPad(DualShock4DPadValues.South);
                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadLeft))
                        ds4Report.SetDPad(DualShock4DPadValues.West);

                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadUp)
                        && ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadRight))
                        ds4Report.SetDPad(DualShock4DPadValues.Northeast);
                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadRight)
                        && ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadDown))
                        ds4Report.SetDPad(DualShock4DPadValues.Southeast);
                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadDown)
                        && ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadLeft))
                        ds4Report.SetDPad(DualShock4DPadValues.Southwest);
                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadLeft)
                        && ds3Report.EngagedButtons.Contains(DualShock3Buttons.DPadUp))
                        ds4Report.SetDPad(DualShock4DPadValues.Northwest);

                    if (ds3Report.EngagedButtons.Contains(DualShock3Buttons.Ps))
                        ds4Report.SetSpecialButtons(DualShock4SpecialButtons.Ps);

                    target.SendReport(ds4Report);

                    break;
            }
        }
    }
#endif

#if X360

    internal class ಠ_ಠAttribute : Attribute
    { }

    [ExportMetadata("Name", "ViGEm Xbox 360 Sink")]
    [Export(typeof(ISinkPlugin))]
    public class ViGEmSink : ISinkPlugin
    {
        private readonly Dictionary<DualShock3Buttons, Xbox360Buttons> _btnMap;
        private readonly Dictionary<DualShock3Axes, Xbox360Axes> _XaxisMap;
        private readonly Dictionary<DualShock3Axes, Xbox360Axes> _YaxisMap;
        private readonly Dictionary<DualShock3Axes, Xbox360Axes> _triggerAxisMap;
        private readonly ViGEmClient _client;

        private readonly Dictionary<IDualShockDevice, Xbox360Controller> _deviceMap =
            new Dictionary<IDualShockDevice, Xbox360Controller>();

        public ViGEmSink()
        {
            _btnMap = new Dictionary<DualShock3Buttons, Xbox360Buttons>
            {
                {DualShock3Buttons.Select, Xbox360Buttons.Back},
                {DualShock3Buttons.LeftThumb, Xbox360Buttons.LeftThumb},
                {DualShock3Buttons.RightThumb, Xbox360Buttons.RightThumb},
                {DualShock3Buttons.Start, Xbox360Buttons.Start},
                {DualShock3Buttons.LeftShoulder, Xbox360Buttons.LeftShoulder},
                {DualShock3Buttons.RightShoulder, Xbox360Buttons.RightShoulder},
                {DualShock3Buttons.Triangle, Xbox360Buttons.Y},
                {DualShock3Buttons.Circle, Xbox360Buttons.B},
                {DualShock3Buttons.Cross, Xbox360Buttons.A},
                {DualShock3Buttons.Square, Xbox360Buttons.X},
                {DualShock3Buttons.DPadUp, Xbox360Buttons.Up},
                {DualShock3Buttons.DPadDown, Xbox360Buttons.Down},
                {DualShock3Buttons.DPadLeft, Xbox360Buttons.Left},
                {DualShock3Buttons.DPadRight, Xbox360Buttons.Right},
                {DualShock3Buttons.Ps, Xbox360Buttons.Guide}
            };

            _XaxisMap = new Dictionary<DualShock3Axes, Xbox360Axes>
            {
                {DualShock3Axes.LeftThumbX, Xbox360Axes.LeftThumbX},
                {DualShock3Axes.RightThumbX, Xbox360Axes.RightThumbX}
            };

            _YaxisMap = new Dictionary<DualShock3Axes, Xbox360Axes>
            {
                {DualShock3Axes.LeftThumbY, Xbox360Axes.LeftThumbY},
                {DualShock3Axes.RightThumbY, Xbox360Axes.RightThumbY},
            };

            _triggerAxisMap = new Dictionary<DualShock3Axes, Xbox360Axes>
            {
                {DualShock3Axes.LeftTrigger, Xbox360Axes.LeftTrigger},
                {DualShock3Axes.RightTrigger, Xbox360Axes.RightTrigger},
            };

            _client = new ViGEmClient();
        }

        public event RumbleRequestReceivedEventHandler RumbleRequestReceived;

        public void DeviceArrived(IDualShockDevice device)
        {
            var target = new Xbox360Controller(_client);

            _deviceMap.Add(device, target);

            target.FeedbackReceived += (sender, args) =>
            {
                var source = _deviceMap.First(m => m.Value.Equals(sender)).Key;

                RumbleRequestReceived?.Invoke(source, new RumbleRequestEventArgs(args.LargeMotor, args.SmallMotor));
            };

            try
            {
                Log.Information("Connecting ViGEm target {Target}", target);
                target.Connect();
                Log.Information("ViGEm target {Target} connected successfully", target);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to connect target {@Target}: {Exception}", target, ex);
            }
        }

        public void DeviceRemoved(IDualShockDevice device)
        {
            _deviceMap[device].Dispose();
            _deviceMap.Remove(device);
        }

        public void InputReportReceived(IDualShockDevice device, IInputReport report)
        {
            switch (device.DeviceType)
            {
                case DualShockDeviceType.DualShock3:

                    var target = _deviceMap[device];

                    var ds3Report = (DualShock3InputReport)report;
                    var xb360Report = new Xbox360Report();

                    foreach (var axis in _XaxisMap)
                    {
                        xb360Report.SetAxis(axis.Value, Scale(ds3Report[axis.Key], false));
                    }

                    foreach (var axis in _YaxisMap)
                    {
                        xb360Report.SetAxis(axis.Value, Scale(ds3Report[axis.Key], true));
                    }

                    foreach (var axis in _triggerAxisMap)
                    {
                        xb360Report.SetAxis(axis.Value, ds3Report[axis.Key]);
                    }

                    xb360Report.SetButtons(_btnMap.Where(m => ds3Report.EngagedButtons.Contains(m.Key))
                        .Select(m => m.Value).ToArray());

                    target.SendReport(xb360Report);

                    break;
            }
        }

        [ಠ_ಠ]
        private static short Scale(byte value, bool invert)
        {
            int intValue = (value - 0x80);
            if (intValue == -128) intValue = -127;

            var wtfValue = intValue * 258.00787401574803149606299212599f; // what the fuck?

            return (short)(invert ? -wtfValue : wtfValue);
        }
    }
#endif

}