using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bloque : MonoBehaviour {
	void Start () {
		if(transform.localScale.z > 2){
			GetComponent<SpriteRenderer>().color = Main.col_str;
		}
	}	
}