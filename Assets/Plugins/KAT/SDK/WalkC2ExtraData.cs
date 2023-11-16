using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WalkC2ExtraData
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct extraInfo
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool isLeftGround;
		[MarshalAs(UnmanagedType.U1)]
		public bool isRightGround;
		[MarshalAs(UnmanagedType.U1)]
		public bool isLeftStatic;
		[MarshalAs(UnmanagedType.U1)]
		public bool isRightStatic;

		[MarshalAs(UnmanagedType.U4)]
		public int motionType;

		public Vector3 skatingSpeed;
		public Vector3 lFootSpeed;
		public Vector3 rFootSpeed;
	};


	public static extraInfo GetExtraInfoC2(KATNativeSDK.TreadMillData data)
	{
		GCHandle handle = GCHandle.Alloc(data.extraData, GCHandleType.Pinned);
		extraInfo info;
		try
		{
			info = (extraInfo)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(extraInfo));
		}
		finally
		{
			handle.Free();
		}

		return info;
	}
}