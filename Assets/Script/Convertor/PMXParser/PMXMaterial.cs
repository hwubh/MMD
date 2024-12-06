using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMD_URP
{
    public class PMXMaterial
    {
		[Flags]
		public enum Flag : byte
		{
			NoBackCulling = 0x00,
			Reversible = 0x01,
			CastShadow = 0x02, 
			CastSelfShadow = 0x04, 
			ReceiveSelfShadow = 0x08, 
			Edge = 0x10, 
			PointDrawing = 0x20,
			LineDrawing = 0x40,
		}
		public enum EnvironmentBlendMode
		{
			Null,       //�o��
			MulSphere,  //�\�㥹�ե���
			AddSphere,  //���㥹�ե���
			SubTexture, //���֥ƥ�������
		}
		public Material material;
		public byte flag;
		public int diffuseTextureIndex;
		public int environmentTextureIndex;
		public EnvironmentBlendMode blendMode;
		public byte toonTexture;
		public int toonTextureIndex;
		public string memo;
		public int surfaceCount; // ��픵��� // ����ǥå����ˉ�Q������Ϥϡ����|0����혤˼���
	}
}
