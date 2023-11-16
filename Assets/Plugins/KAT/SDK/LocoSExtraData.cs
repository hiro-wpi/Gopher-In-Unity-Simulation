using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class LocoSExtraData : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct extraBaseSDKInfo
	{
		//Not used in Loco/LocoS
		public bool isLeftGround;
		//Not used in Loco/LocoS
		public bool isRightGround;
		//Not used in Loco/LocoS
		public bool isLeftStatic;
		//Not used in Loco/LocoS
		public bool isRightStatic;

		//Not used in Loco/LocoS
		public int motionType;
		public int action;
		public bool isMoving;
		public int isForward;
		public double calorie;
		//Not used in Loco/LocoS
		public Vector3 skatingSpeed;
		//Not used in Loco/LocoS
		public Vector3 lFootSpeed;
		//Not used in Loco/LocoS
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
	};

	public static extraInfo GetExtraInfoLoco(KATNativeSDK.TreadMillData data)
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
