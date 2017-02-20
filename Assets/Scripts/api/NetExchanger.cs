using UnityEngine;
using System.Collections;

public class NetExchanger : MonoBehaviour {
    MAIN main = MAIN.getMain;

    // Use this for initialization
    void Start () {
        //main.network.Exchange();
    }
	
	// Update is called once per frame
	void Update () {
        if (main.network != null) {
            //print("ok");
            main.network.Exchange();
        }
    }
}
