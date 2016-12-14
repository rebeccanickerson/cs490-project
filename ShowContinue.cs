using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class ShowContinue : MonoBehaviour {

	void Awake () {
		FileInfo info = new FileInfo (Path.Combine (Application.persistentDataPath, "plantData.xml"));
		if (info == null || info.Exists == false) {
			this.gameObject.SetActive(false);
		}
	}
}
