using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MMD_URP
{
    public class ImportData
    {
        public List<ModelData> models = new List<ModelData>();

        public struct ModelData
        {
            public GameObject characterRoot;
            public BoneManager boneManager;

            public string PMXPath;
            public string VMDPath;
            public string TexPath;
        }

        public bool IsModelExist(GameObject gameObject, out BoneManager BoneManagerSelected)
        {
            foreach (var model in models)
            {
                if (model.characterRoot == gameObject)
                {
                    BoneManagerSelected = model.boneManager;
                    return true;
                }
            }
            BoneManagerSelected = null;
            return false;
        }

        public string PMXPath;
        public string VMDPath;
        public string TexPath;
    }
}
