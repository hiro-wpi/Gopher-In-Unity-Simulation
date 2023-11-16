using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WalkCExtraData 
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct extraBaseSDKInfo
	{
		//Not used in WalkC
		public bool isLeftGround;
		//Not used in WalkC
		public bool isRightGround;
		//Not used in WalkC
		public bool isLeftStatic;
		//Not used in WalkC
		public bool isRightStatic;

		//Not used in WalkC
		public int motionType;
		public int action;
		public bool isMoving;
		public int isForward;
		public double calorie;
		//Not used in WalkC
		public Vector3 skatingSpeed;
		//Not used in WalkC
		public Vector3 lFootSpeed;
		//Not used in WalkC
		public Vector3 rFootSpeed;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct extraInfo
	{
		public extraBaseSDKInfo baseSDKinfo;
		public uint L_Status;
		public float L_Pitch;
		public float L_Roll;
		public uint R_Status;
		public float R_Pitch;
		public float R_Roll;
		public uint Hall_Status;
	};

	public static extraInfo GetExtraInfoC(KATNativeSDK.TreadMillData data)
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
