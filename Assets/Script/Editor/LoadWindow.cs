using UnityEngine;
using UnityEditor;
using MMD_URP;
using System.IO;

namespace MMD_URP 
{
	public class LoaderWindow : EditorWindow
	{
		private Object pmxFile = null;
		//Object vmdFile;

		[MenuItem("MMD for Unity/PMX(PMD) Loader")]
		static void Init()
		{
			var window = GetWindow<LoaderWindow>(true, "Import MMD Files");
			window.Show();
		}

		void OnEnable()
		{

		}

		void OnGUI()
		{
			//GUI.enabled = !EditorApplication.isPlaying;

			pmxFile = EditorGUILayout.ObjectField("PMX File", pmxFile, typeof(Object), false);
			//vmdFile = EditorGUILayout.ObjectField("VMD File", vmdFile, typeof(Object), false);

			{
				bool gui_enabled_old = GUI.enabled;
				GUI.enabled = (pmxFile != null);
				if (GUILayout.Button("Convert"))
				{
					string pmxPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), AssetDatabase.GetAssetPath(pmxFile));
					Convertor.Instance.Import(pmxPath);
				}
				GUI.enabled = gui_enabled_old;
			}
		}
	}
}