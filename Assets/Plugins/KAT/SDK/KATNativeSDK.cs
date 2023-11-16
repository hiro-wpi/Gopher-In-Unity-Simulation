using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


/// <summary>
/// Unity Warpper for KAT Native SDK
/// </summary>
public class KATNativeSDK
{
	/// <summary>
	/// Description of KAT Devices
	/// </summary>
	/// 
	[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
	public struct DeviceDescription
	{
		//Device Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string device;

		//Device Serial Number
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string serialNumber;

		//Device PID
		public int pid;

		//Device VID
		public int vid;

		//Device Type
		//0. Err 1. Tread Mill 2. Tracker 
		public int deviceType;
	};

	/// <summary>
	/// Device Status Data
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DeviceData
	{
		//Is Calibration Button Pressed?
		[MarshalAs(UnmanagedType.I1)]
		public bool btnPressed;
		[MarshalAs(UnmanagedType.I1)]
		//Is Battery Charging?
		public bool isBatteryCharging;
		//Battery Used
		public float batteryLevel;
		[MarshalAs(UnmanagedType.I1)]
		public byte firmwareVersion;
	};

	/// <summary>
	/// TreadMill Device Data
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
	public struct TreadMillData
	{
		//Device Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string deviceName;
		[MarshalAs(UnmanagedType.I1)]
		//Is Device Connected
		public bool connected;
		//Last Update Time
		public double lastUpdateTimePoint;

		//Body Rotation(Quaternion), for treadmill it will cause GL
		public Quaternion bodyRotationRaw;

		//Target Move Speed With Direction
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public Vector3 moveSpeed;

		//Sensor Device Datas
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public DeviceData[] deviceDatas;

		//Extra Data of TreadMill
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		public byte[] extraData;
	};

	//Get Device Count
	[DllImport("KATNativeSDK.dll")]
	public static extern int DeviceCount();

	//Get Device Description
	[DllImport("KATNativeSDK.dll")]
	public static extern DeviceDescription GetDevicesDesc(uint index);

	//Get Treadmill Data, main Func
	[DllImport("KATNativeSDK.dll")]
	public static extern TreadMillData GetWalkStatus(string sn = "");

	//KAT Extensions, Only for WalkCoord2 and later device
	public class KATExtension
	{
		//KAT Extensions, amplitude: 0(close) - 1.0(max)

		[DllImport("KATNativeSDK.dll")]
		public static extern void VibrateConst(float amplitude);

		[DllImport("KATNativeSDK.dll")]
		public static extern void LEDConst(float amplitude);

		//Vibrate in duration
		[DllImport("KATNativeSDK.dll")]
		public static extern void VibrateInSeconds(float amplitude, float duration);

		//Vibrate once, simulate a "Click" like function
		[DllImport("KATNativeSDK.dll")]
		public static extern void VibrateOnce(float amplitude);

		//Vibrate with a frequency in duration
		[DllImport("KATNativeSDK.dll")]
		public static extern void VibrateFor(float duration, float frequency, float amplitude);

		//Lighting LED in Seconds
		[DllImport("KATNativeSDK.dll")]
		public static extern void LEDInSeconds(float amplitude, float duration);

		//Lighting once
		[DllImport("KATNativeSDK.dll")]
		public static extern void LEDOnce(float amplitude);

		//Vibrate with a frequency in duration
		[DllImport("KATNativeSDK.dll")]
		public static extern void LEDFor(float duration, float frequency, float amplitude);

	}
}
