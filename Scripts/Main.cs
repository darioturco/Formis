using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Main : MonoBehaviour {
	public GameObject punto_pref, col_obj, aux, menu_obj;
	public GameObject[] pref, puntos;
	public List<GameObject> objetos;
	public PolygonCollider2D col;
	public ContactFilter2D filter;
	public Collider2D[] resul;
	public Color[] claro, solido, victoria;
	public Animator st_anim;
	public bool pone, menu, start, isProcessing, isFocus;
	public int i, d, x, num_puntos, ran, good, creara, score, cont_add;
	public Vector2 size;
	public Vector2[] pun_v;
	public LayerMask objeto_mask, own_mask;
	public Text win_text, vol_text, com_text;
	public Image win_img, vol_img, com_img; 
	public Boton start_bot, vol_bot, com_bot, rojo, verde, azul;
	public UnityADs ads;
	public static Color col_str;
	void Start () {
		ads = GetComponent<UnityADs>();
		objetos = new List<GameObject>();
		size = new Vector2(Camera.main.scaledPixelWidth/100,Camera.main.scaledPixelHeight/100);
		filter.layerMask = objeto_mask;
		cambia_color(2);
		score = 0;
		num_puntos = 0;
	}
	IEnumerator crea(int cant){
		pone = false;
		for(i=0;i<cant;i++){
			aux = Instantiate(pref[Random.Range(0,5)]);
			aux.transform.position = new Vector2(Random.Range(-size.x/2,size.x/2),Random.Range(-size.y/2,size.y/2));
			aux.transform.rotation = Quaternion.Euler(0,0,Random.Range(53,62));
			aux.name = "N: "+objetos.Count;//se puede omitir
			if(Random.Range(0,100) > 65){
				aux.transform.localScale = new Vector3(0.5f,0.5f,1); 
			}else{
				aux.transform.localScale = new Vector3(0.5f,0.5f,3);
			}
			yield return new WaitForSeconds(0.02f);
			if(aux != null){
				if(aux.GetComponent<Collider2D>().IsTouchingLayers(objeto_mask)){
					DestroyImmediate(aux);
					i--;
				}else{
					objetos.Add(aux);
				}
			}	
		}
		for(x=0,good=0;x<objetos.Count;x++){
			if(objetos[x].transform.localScale.z > 2){
				good++;
			}
		}
		if(good == 0){
			for(x=0;x<3;x++){
				DestroyImmediate(objetos[x]);
				objetos.RemoveAt(x);
			}
			StartCoroutine(crea(3));
		}else{
			pone = true;
		}
	}
	IEnumerator coloca(){
		pone = false;
		puntos[num_puntos] = Instantiate(punto_pref);
		puntos[num_puntos].transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
		yield return new WaitForSeconds(0.025f);
		if(Physics2D.OverlapCircle(puntos[num_puntos].transform.position,0.1f,objeto_mask)){
			DestroyImmediate(puntos[num_puntos]);
		}else{
			num_puntos++;
			if(num_puntos >= 3){
				for(d=0;d<3;d++){
					pun_v[d] = puntos[d].transform.position;
				}
				col.SetPath(0,pun_v);
				yield return new WaitForSeconds(0.025f);
				if(col.IsTouchingLayers(objeto_mask)){//se fija si encierra un objeto
					col.OverlapCollider(filter,resul);
					for(d=0,creara=0;d<resul.Length;d++){
						if(resul[d] != null){
							if(resul[d].transform.localScale.z > 1){//se fija de que tipo es
								if(objetos.Contains(resul[d].gameObject)){
									good = objetos.IndexOf(resul[d].gameObject);
								}
								DestroyImmediate(resul[d].gameObject);
								objetos.RemoveAt(good);
								resul[d] = null;
								creara += Random.Range(2,4);
								score++;
							}else{
								perdio();
								break;
							}
						}
					}
				}else{
					perdio();
				}
				StartCoroutine(crea(creara));
				if(menu == false){
					ran = Random.Range(0,3);
					for(d=0;d<3;d++){
						if(d != ran){
							DestroyImmediate(puntos[d]);
						}
					}
					puntos[0] = puntos[ran];
					num_puntos = 1;
				}
			}
		}
		pone = true;
	}
	IEnumerator reinicia(){
		cont_add++;
		if(cont_add >= 2){
			cont_add = 0;
			ads.ShowAds();
			yield return new WaitUntil(() => ads.startAd == false);
		}
		for(x=0;x<objetos.Count;x++){
			DestroyImmediate(objetos[x]);
		}
		for(x=0;x<puntos.Length;x++){
			pun_v[x] = Vector2.zero;
			DestroyImmediate(puntos[x]);
		}
		objetos.Clear();
		menu_obj.SetActive(false);
		StartCoroutine(crea(12));
		score = 0;
		num_puntos = 0;
		menu = false;
	}
	IEnumerator ShareScreenshot(){
		isProcessing = true;
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot("screenshot.png", 2);
		string destination = System.IO.Path.Combine(Application.persistentDataPath, "screenshot.png");
		yield return new WaitForSecondsRealtime(0.3f);
		if(!Application.isEditor){
			AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
			intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
			AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
			AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + destination);
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"),uriObject);
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),"Can you beat my score?");
			intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");
			AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser",intentObject, "Share your new score");
			currentActivity.Call("startActivity", chooser);
			yield return new WaitForSecondsRealtime(1);
		}
		yield return new WaitUntil(() => isFocus);
		isProcessing = false;
	}
	void perdio(){
		menu = true;
		menu_obj.SetActive(true);
		win_text.text = "Felizidades!!!\nDestruiste\n"+score+" Formas";
	}
	void cambia_color(int num){
		col_str = new Color(solido[num].r,solido[num].g,solido[num].b,solido[num].a);
		vol_text.color = new Color(solido[num].r,solido[num].g,solido[num].b,vol_text.color.a);
		com_text.color = new Color(solido[num].r,solido[num].g,solido[num].b,vol_text.color.a);
		win_text.color = new Color(victoria[num].r,victoria[num].g,victoria[num].b,win_text.color.a);
		win_img.color = new Color(solido[num].r,solido[num].g,solido[num].b,win_img.color.a);
		vol_img.color = new Color(claro[num].r,claro[num].g,claro[num].b,vol_img.color.a);
		com_img.color = new Color(claro[num].r,claro[num].g,claro[num].b,com_img.color.a);
	}
	public void ShareBtnPress(){
		if(!isProcessing){
			StartCoroutine(ShareScreenshot());
		}
	}
	private void OnApplicationFocus (bool focus) {
		isFocus = focus;
	}
	void Update () {
		if(start == true){
			if(menu == true){
				if(vol_bot.up == true){
					StartCoroutine("reinicia");
				}
				if(com_bot.up == true){
					ShareBtnPress();
				}
				if(rojo.up == true){
					cambia_color(0);
				}
				if(verde.up == true){
					cambia_color(1);
				}
				if(azul.up == true){
					cambia_color(2);
				}
			}else{
				if(Input.GetMouseButtonUp(0) && pone == true){
					StartCoroutine("coloca");
				}
			}
		}else{
			if(start_bot.up == true){
				StartCoroutine(crea(12));
				st_anim.SetBool("ok",true);
				start = true;
			}
		}
	}
}