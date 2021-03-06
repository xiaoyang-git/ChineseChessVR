// --------------------------------------------------------------------------------------------------------------------
// <copyright file=ChessmanController.cs company=League of HTC Vive Developers>
/*
11111111111111111111111111111111111111001111111111111111111111111
11111111111111111111111111111111111100011111111111111111111111111
11111111111111111111111111111111100001111111111111111111111111111
11111111111111111111111111111110000111111111111111111111111111111
11111111111111111111111111111000000111111111111111111111111111111
11111111111111111111111111100000011110001100000000000000011111111
11111111111111111100000000000000000000000000000000011111111111111
11111111111111110111000000000000000000000000000011111111111111111
11111111111111111111111000000000000000000000000000000000111111111
11111111111111111110000000000000000000000000000000111111111111111
11111111111111111100011100000000000000000000000000000111111111111
11111111111111100000110000000000011000000000000000000011111111111
11111111111111000000000000000100111100000000000001100000111111111
11111111110000000000000000001110111110000000000000111000011111111
11111111000000000000000000011111111100000000000000011110001111111
11111110000000011111111111111111111100000000000000001111100111111
11111111000001111111111111111111110000000000000000001111111111111
11111111110111111111111111111100000000000000000000000111111111111
11111111111111110000000000000000000000000000000000000111111111111
11111111111111111100000000000000000000000000001100000111111111111
11111111111111000000000000000000000000000000111100000111111111111
11111111111000000000000000000000000000000001111110000111111111111
11111111100000000000000000000000000000001111111110000111111111111
11111110000000000000000000000000000000111111111110000111111111111
11111100000000000000000001110000001111111111111110001111111111111
11111000000000000000011111111111111111111111111110011111111111111
11110000000000000001111111111111111100111111111111111111111111111
11100000000000000011111111111111111111100001111111111111111111111
11100000000001000111111111111111111111111000001111111111111111111
11000000000001100111111111111111111111111110000000111111111111111
11000000000000111011111111111100011111000011100000001111111111111
11000000000000011111111111111111000111110000000000000011111111111
11000000000000000011111111111111000000000000000000000000111111111
11001000000000000000001111111110000000000000000000000000001111111
11100110000000000001111111110000000000000000111000000000000111111
11110110000000000000000000000000000000000111111111110000000011111
11111110000000000000000000000000000000001111111111111100000001111
11111110000010000000000000000001100000000111011111111110000001111
11111111000111110000000000000111110000000000111111111110110000111
11111110001111111100010000000001111100000111111111111111110000111
11111110001111111111111110000000111111100000000111111111111000111
11111111001111111111111111111000000111111111111111111111111100011
11111111101111111111111111111110000111111111111111111111111001111
11111111111111111111111111111110001111111111111111111111100111111
11111111111111111111111111111111001111111111111111111111001111111
11111111111111111111111111111111100111111111111111111111111111111
11111111111111111111111111111111110111111111111111111111111111111
*/
//   
// </copyright>
// <summary>
//  Chinese Chess VR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 3/24/2017

// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using VRTK;
using Lean;

/// <summary>
/// FileName: ChessmanController.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 负责控制棋子的行为
/// DateTime: 3/24/2017
/// </summary>
public class ChessmanController : VRTK_InteractableObject {



    #region Public Variables  //公共变量区域
    [Tooltip("棋子的音效")]
	public AudioClip spawnMusic,awakeMusic,ArrivalAC,RunAC,DieAC;

    [Tooltip("战斗UI游戏对象预设")]
    public GameObject warUiPrefab;

    [Tooltip("选中棋子的id")]
    public int ChessmanId;

    #endregion


    #region Private Variables   //私有变量区域
	AudioSource As;
	Animator ani;

	private LeanPool pointerPool; //指针对象池 
	float step=3f;	//单位步长
    private float pointerHeight = 1.6f;
    private ChessmanManager chessmanManager;
	bool isRed;
    #endregion


    #region MonoBehaviour CallBacks //回调函数区域

    // Use this for initialization
    void Start () {
		PlaySound (spawnMusic);
        ChessmanId =int.Parse(this.gameObject.name);
		isRed = ChessmanId<16;
		ani = transform.GetComponent<Animator> ();
		if (this.As == null) this.As = FindObjectOfType<AudioSource>();

		pointerPool = transform.parent.GetComponent<LeanPool> ();	//获取指针的对象池组件
        chessmanManager= transform.parent.GetComponent<ChessmanManager>();
        //创建战斗UI
        if (this.warUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(this.warUiPrefab, Vector3.zero, Quaternion.identity, transform);
			_uiGo.transform.localPosition = new Vector3(0, 4f, 0);
        }
        else
        {
            Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

    }
		
    #endregion

    #region Public Methods	//公共方法区域
    public override void StartUsing(GameObject usingObject)
	{
		base.StartUsing(usingObject);
		trySelectChessman ();
	}

	public override void StopUsing(GameObject usingObject)
	{
		base.StopUsing(usingObject);

	}

	public void trySelectChessman(){
		if (NetworkTurn.Instance._selectedId != ChessmanId) {
			NetworkTurn.Instance.OnSelectChessman(ChessmanId, transform.localPosition.x, transform.localPosition.z);
		}
	}

	public void SelectedChessman(){
        
		Search ();
		PlaySound (awakeMusic);
		ani.SetTrigger ("TH Sword Jump");
	}

	/// <summary>
	/// 设置目标位置.
	/// </summary>
	/// <param name="targetPosition">目标位置.</param>
	public void SetTarget(float x,float z){
		Vector3 targetPosition = new Vector3 (x, 0.57f, z);
		chessmanManager.hidePointer ();
		Hashtable ht = new Hashtable ();
		ht.Add ("position", targetPosition);
		//ht.Add ("orienttopath", true);
		ht.Add ("onstart", "Move");
		ht.Add ("oncomplete","Stop");
		ht.Add ("islocal",true);
		ht.Add ("speed", 2.0f);
		iTween.MoveTo (gameObject, ht);
		ht.Clear ();
		float _x = z == 0 ? 0 : (z / 3f) * 58f;
		float _y = x == 0 ? 0 : (x / 3f) * 58f;
		targetPosition = new Vector3 (_x,_y,0);
		ht.Add ("position", targetPosition);
		ht.Add ("islocal",true);
		ht.Add ("speed", 3.0f);
		iTween.MoveTo (ChessMap.chessman[ChessmanId].go,ht);
		ChessmanManager.chessman[ChessmanId]._x = x;
		ChessmanManager.chessman[ChessmanId]._z = z;
	}

	/// <summary>
	/// 切换死亡
	/// </summary>
	public void SwitchDead()
	{
		HitHapticPulse (500);
		ani.SetTrigger ("TH Sword Die");
        PlaySound(DieAC);
	}

    #endregion


    #region Private Methods //私有方法

    void Search(){
		chessmanManager.hidePointer ();
		float x = ChessmanManager.chessman[ChessmanId]._x;	//棋子位置x
		float z= ChessmanManager.chessman[ChessmanId]._z;
		switch (ChessmanManager.chessman[ChessmanId]._type)
		{
		case ChessmanManager.Chessman.TYPE.KING:
			ShowKingWay(x,z);
			break;
		case ChessmanManager.Chessman.TYPE.GUARD:
			ShowGuardWay(x,z);
			break;
		case ChessmanManager.Chessman.TYPE.ELEPHANT:
			ShowElephantWay(x,z);
			break;
		case ChessmanManager.Chessman.TYPE.HORSE:
			ShowHorseWay(x,z);
			break;
		case ChessmanManager.Chessman.TYPE.ROOK:
			ShowRookWay(x,z);
			break;
		case ChessmanManager.Chessman.TYPE.CANNON:
			ShowRookWay(x,z);
			break;
		case ChessmanManager.Chessman.TYPE.PAWN:
			ShowPawnWay(x,z);
			break;
		}
	}

	void Move(){
		ani.SetBool ("TH Sword Run",true);
		PlaySound (RunAC);
	}

	void Stop(){
		ani.SetBool ("TH Sword Run",false);
		PlaySound (ArrivalAC);
	}
		
	void PureDead(){
		ChessMap.chessman [ChessmanId].go.SetActive (false);
		gameObject.SetActive (false);
	}

	/// <summary>
	/// 播放音效
	/// </summary>
	/// <param name="ac">声音</param>
	void PlaySound(AudioClip Ac)
	{

		if (Ac!=null && !As.isPlaying)
		{
			As.PlayOneShot (Ac);
		}

	}
		

	/// <summary>
	/// 震动
	/// </summary>
	/// <param name="duration">震动时间.</param>
	void HitHapticPulse(ushort duration)
	{
		var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
		var deviceIndex1 = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
		SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(duration);
		SteamVR_Controller.Input(deviceIndex1).TriggerHapticPulse(duration);
	}
		
	#endregion

	#region ToolMethods

	/// <summary>
	/// Shows the king way.将帅的路径，总结为十字路口
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	void ShowKingWay(float x,float z){
		/*
		1.将帅被限制在九宫格内移动
		2.移动的步长为一格，一格的距离为3
		3.将帅不能在一条直线上面对面（中间无棋子遮挡）,如一方占据中路三线中的一线,在无遮挡的情况下,另一方必须回避该线,否则会被对方秒杀
		*/
		if (Mathf.Abs(x)>13.5f || Mathf.Abs(z)>3f || Mathf.Abs(x)<7.5f) {
			Debug.LogError ("越界棋子：" + gameObject);
			return;
		}
		float xStep = step;
		float otherKingZ=ChessmanManager.chessman[0]._z;	//另外一个将帅的Z轴值
		if (isRed) {
			otherKingZ=ChessmanManager.chessman[16]._z;
			xStep = -step; //红方取反
		}
		float absX = Mathf.Abs (x);
        
		if (absX>7.5f) {	//正前方指针
		    GameObject go=pointerPool.FastSpawn (new Vector3 (x+xStep, pointerHeight, z),Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go);
        }
		if (absX<13.5f) {	//正后方
			GameObject go=pointerPool.FastSpawn (new Vector3 (x-xStep, pointerHeight, z),Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go);
        }

		if (z>-step && (z-step)!=otherKingZ) {		//左
			GameObject go=pointerPool.FastSpawn (new Vector3 (x, pointerHeight, z-step),Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go);
        }

		if (z<step && (z+step)!=otherKingZ) {	//右
			GameObject go=pointerPool.FastSpawn (new Vector3 (x, pointerHeight, z+step),Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go);
        }
	}

	/// <summary>
	/// Shows the guard way.士的路径，斜十字算法
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	void ShowGuardWay(float x,float z){
		/* 
         * 1.目标位置在九宫格内 
         * 2.只许沿着九宫中的斜线行走一步（方格的对角线） 
        */
		if (Mathf.Abs(x)>13.5f || Mathf.Abs(z)>3f || Mathf.Abs(x)<7.5f) {
			Debug.LogError ("越界棋子：" + gameObject);
			return;
		}
		if (z != 0f) {
			if (isRed) {
				GameObject go=pointerPool.FastSpawn (new Vector3 (10.5f, pointerHeight, 0f), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            } else {
				GameObject go=pointerPool.FastSpawn (new Vector3 (-10.5f, pointerHeight, 0f), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            }
		} else {
			GameObject go=pointerPool.FastSpawn (new Vector3 (x+step, pointerHeight, -step), Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go);
            GameObject go1=pointerPool.FastSpawn (new Vector3 (x-step, pointerHeight, -step), Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go1);
            GameObject go2=pointerPool.FastSpawn (new Vector3 (x+step, pointerHeight, step), Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go2);
            GameObject go3=pointerPool.FastSpawn (new Vector3 (x-step, pointerHeight, step), Quaternion.identity,transform.parent) as GameObject;
            chessmanManager.spawnedPointers.Add(go3);
        }
	}

    /// <summary>
    /// 象采用探测方法
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
	void ShowElephantWay(float x,float z){
        /* 
         * 1.目标位置不能越过河界走入对方的领地 
         * 2.只能斜走（两步），可以使用汉字中的田字形象地表述：田字格的对角线，即俗称象（相）走田字 
         * 3.当象（相）行走的路线中，及田字中心有棋子时（无论己方或者是对方的棋子），则不允许走过去，俗称：塞象（相）眼。 
        */
		if (Mathf.Abs(x)>13.5f || Mathf.Abs(z)>12f) {
			Debug.LogError ("越界棋子：" + gameObject);
			return;
		}
        float xStep = step;
        if (isRed)
        {
            xStep = -step;
        }

		if (z == 0f)
		{

			GameObject go=pointerPool.FastSpawn(new Vector3(x + xStep, pointerHeight, z - step), Quaternion.identity,transform.parent) as GameObject;
			movePointer(go, new Vector3(x + 2 * xStep, pointerHeight, z - 2 * step));
			GameObject go1 = pointerPool.FastSpawn(new Vector3(x + xStep, pointerHeight, z + step), Quaternion.identity,transform.parent) as GameObject;
			movePointer(go1, new Vector3(x + 2 * xStep, pointerHeight, z + 2 * step));

			GameObject go2 = pointerPool.FastSpawn(new Vector3(x - xStep, pointerHeight, z - step), Quaternion.identity,transform.parent) as GameObject;
			movePointer(go2, new Vector3(x - 2 * xStep, pointerHeight, z - 2 * step));
			GameObject go3 = pointerPool.FastSpawn(new Vector3(x - xStep, pointerHeight, z + step), Quaternion.identity,transform.parent) as GameObject;
			movePointer(go3, new Vector3(x - 2 * xStep, pointerHeight, z + 2 * step));

			return;
		}

		if (Mathf.Abs (z)==6f)
        {

            if (Mathf.Abs(x)== 13.5f)
            {
				GameObject go = pointerPool.FastSpawn(new Vector3(x+xStep, pointerHeight, z-step), Quaternion.identity,transform.parent) as GameObject;
                movePointer(go, new Vector3(x+2*xStep, pointerHeight, z-2*step));
				GameObject go1 = pointerPool.FastSpawn(new Vector3(x + xStep, pointerHeight, z+step), Quaternion.identity,transform.parent) as GameObject;
                movePointer(go1, new Vector3(x + 2 * xStep, pointerHeight, z + 2*step));
            }
            else
            {
				GameObject go = pointerPool.FastSpawn(new Vector3(x - xStep, pointerHeight, z - step), Quaternion.identity,transform.parent) as GameObject;
                movePointer(go, new Vector3(x - 2 * xStep, pointerHeight, z - 2 * step));
				GameObject go1 = pointerPool.FastSpawn(new Vector3(x - xStep, pointerHeight, z + step), Quaternion.identity,transform.parent) as GameObject;
                movePointer(go1, new Vector3(x - 2 * xStep, pointerHeight, z + 2 * step));
            }
            return;
        }
		float zStep = step;
		if (z>0) {
			zStep = -step;
		}
		if (Mathf.Abs (z) == 12f)
        {
			GameObject go = pointerPool.FastSpawn(new Vector3(x - xStep, pointerHeight, z - zStep), Quaternion.identity,transform.parent) as GameObject;
            movePointer(go, new Vector3(x - 2 * xStep, pointerHeight, z - 2 * zStep));
			GameObject go1 = pointerPool.FastSpawn(new Vector3(x + xStep, pointerHeight, z - zStep), Quaternion.identity,transform.parent) as GameObject;
            movePointer(go1, new Vector3(x + 2 * xStep, pointerHeight, z - 2 * zStep));
        }
			
    }

	/// <summary>
	/// Shows the horse way.马走日算法，详细的参考Tower象棋规范文档中的图例
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	void ShowHorseWay(float x,float z){
        /* 
        * 1.马走日字（斜对角线） 
        * 2.可以将马走日分解为：先一步直走（或一横）再一步斜走 
        * 3.如果在要去的方向，第一步直行处（或者横行）有别的棋子挡住，则不许走过去（俗称：蹩马腿） 
        */

        float absX = Mathf.Abs(x);
		float absZ = Mathf.Abs(z);
		if (absX>13.5f || absZ>12f) {
			Debug.LogError ("越界棋子：" + gameObject);
			return;
		}

        float xStep = step;

		if (x>0) //过河取反
        {
            xStep = -step;
        }

		float zStep = step;
		if (z>=0) {
			zStep = -step;
		}

		if (absZ==12f) {

			GameObject go = pointerPool.FastSpawn(new Vector3(x+xStep, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject as GameObject;
			movePointer(go, new Vector3(x+2*xStep, pointerHeight, z + zStep));
			GameObject go1 = pointerPool.FastSpawn(new Vector3(x, pointerHeight, z+zStep), Quaternion.identity,transform.parent) as GameObject;
			movePointer(go1, new Vector3(x+xStep, pointerHeight, z+2*zStep));

			if (absX<13.5f) {
				GameObject go2 = pointerPool.FastSpawn(new Vector3(x, pointerHeight, z+zStep), Quaternion.identity,transform.parent) as GameObject;
				movePointer(go2, new Vector3(x-xStep, pointerHeight, z+2*zStep));
			}

			if (absX<10.5f) {
                GameObject go3 = pointerPool.FastSpawn(new Vector3(x - xStep, pointerHeight, z), Quaternion.identity, transform.parent) as GameObject;
				movePointer(go3, new Vector3(x-2*xStep, pointerHeight, z - zStep));
			}
		}else {
			GameObject go = pointerPool.FastSpawn(new Vector3(x+xStep, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
			movePointer(go, new Vector3(x+2*xStep, pointerHeight, z - step));
			GameObject go1 = pointerPool.FastSpawn(new Vector3(x+xStep, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
			movePointer(go1, new Vector3(x+2*xStep, pointerHeight, z+step));
			if (absZ<9f) {
				GameObject go2 = pointerPool.FastSpawn(new Vector3(x, pointerHeight, z-step), Quaternion.identity,transform.parent) as GameObject;
				movePointer(go2, new Vector3(x+xStep, pointerHeight, z - 2*step));
				GameObject go3 = pointerPool.FastSpawn(new Vector3(x, pointerHeight, z+step), Quaternion.identity,transform.parent) as GameObject;
				movePointer(go3, new Vector3(x+xStep, pointerHeight, z + 2*step));
				if (absX<13.5f) {
					GameObject go4 = pointerPool.FastSpawn(new Vector3(x, pointerHeight, z-step), Quaternion.identity,transform.parent) as GameObject;
					movePointer(go4, new Vector3(x-xStep, pointerHeight, z -2*step));
					GameObject go5 = pointerPool.FastSpawn(new Vector3(x+xStep, pointerHeight, z+step), Quaternion.identity,transform.parent) as GameObject;
					movePointer(go5, new Vector3(x-xStep, pointerHeight, z+2*step));
				}
			}
			if (absX<10.5f) {
				GameObject go6 = pointerPool.FastSpawn(new Vector3(x-xStep, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
				movePointer(go6, new Vector3(x-2*xStep, pointerHeight, z - step));
				GameObject go7 = pointerPool.FastSpawn(new Vector3(x+xStep, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
				movePointer(go7, new Vector3(x-2*xStep, pointerHeight, z+step));
			}
		}
			
    }

	/// <summary>
	/// Shows the rook way.先横向纵向生成指针，再剔除多余的
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	void ShowRookWay(float x,float z){
		/* 
         * 1.每行一步棋可以上、下直线行走（进、退）；左、右横走 
         * 2.中间不能隔棋子 
         * 3.行棋步数不限 
         */

		if (Mathf.Abs(x)>13.5f || Mathf.Abs(z)>12f) {
			Debug.LogError ("越界棋子：" + gameObject);
			return;
		}

		for (int i = 1; i < 10; i++) {	//循环生成指针
			if (Mathf.Abs(x+step*i)<16.5f) {
				GameObject go=pointerPool.FastSpawn(new Vector3(x+step*i, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            }
			if (Mathf.Abs(x-step*i)<16.5f) {
				GameObject go=pointerPool.FastSpawn(new Vector3(x-step*i, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            }
			if (Mathf.Abs(z+step*i)<15f) {
				GameObject go=pointerPool.FastSpawn(new Vector3(x, pointerHeight, z+step*i), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            }
			if (Mathf.Abs(z-step*i)<15f) {
				GameObject go=pointerPool.FastSpawn(new Vector3(x, pointerHeight, z-step*i), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            }
		}
        //Debug.Log(chessmanManager.DetectedObstacles.Count.ToString() + "++++");
        

	}		

	/// <summary>
	/// Shows the pawn way.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	void ShowPawnWay(float x,float z){
		/* 
         * 0.不能后退，且每次直走一步
         * 1.在没有过河界前，只能向前，不能横着走
         * 2.过了河界之后，每行一步棋可以向前直走，或者横走（左、右）一步
         */
		if (Mathf.Abs(x)>13.5f || Mathf.Abs(z)>12f) {
			Debug.LogError ("越界棋子：" + gameObject);
			return;
		}

		if (isRed) {
			if (x > -13.5f) {
				GameObject go=pointerPool.FastSpawn (new Vector3 (x - step, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            }
			if (x < 0) {
				if (z > -12f) {
					GameObject go=pointerPool.FastSpawn (new Vector3 (x, pointerHeight, z - step), Quaternion.identity,transform.parent) as GameObject;
                    chessmanManager.spawnedPointers.Add(go);
                }
				if (z < 12f) {
					GameObject go=pointerPool.FastSpawn (new Vector3 (x, pointerHeight, z + step), Quaternion.identity,transform.parent) as GameObject;
                    chessmanManager.spawnedPointers.Add(go);
                }
				
			}
		} else {
			if (x < 13.5f) {
				GameObject go=pointerPool.FastSpawn (new Vector3 (x + step, pointerHeight, z), Quaternion.identity,transform.parent) as GameObject;
                chessmanManager.spawnedPointers.Add(go);
            }
			if (x > 0) {
				if (z > -12f) {
					GameObject go=pointerPool.FastSpawn (new Vector3 (x, pointerHeight, z - step), Quaternion.identity,transform.parent) as GameObject;
                    chessmanManager.spawnedPointers.Add(go);
                }
				if (z < 12f) {
					GameObject go=pointerPool.FastSpawn (new Vector3 (x, pointerHeight, z + step), Quaternion.identity,transform.parent) as GameObject;
                    chessmanManager.spawnedPointers.Add(go);
                }
			}
		}
	}

    void movePointer(GameObject go,Vector3 position)
    {
        chessmanManager.spawnedPointers.Add(go);
        Hashtable ht = new Hashtable();
        ht.Add("position", position);
        ht.Add("islocal", true);
        ht.Add("time", 0.2f);
        iTween.MoveTo(go, ht);
    }
	#endregion
	
}
