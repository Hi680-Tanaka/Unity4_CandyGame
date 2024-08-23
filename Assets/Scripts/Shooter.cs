using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    const int MaxShotPower = 5;//最大（5）連続投入できる設定
    const int RecoverySeconds = 3;//回復までのタイム

    int shotPower = MaxShotPower;//MaxShotPowerで設定した数がそのままshotパワーになっている

    AudioSource shotSound;

    //public GameObject[] candyPrefab;//Instantiateで生成する対象
    public GameObject[] candyPrefabs;//Instantiateで生成する対象（配列）
    public Transform candyParentTransform;//生成されたCandyの親役

    public CandyManager candyManager;//CandyManagerクラスの変数を使えるようにする


    public float shotForce;//AddForceで使うパワー
    public float shotTorque;//AddTorqueで使う回転力
    public float baseWidth;//Candyが飛んでいく位置の上限幅 "5"の幅をめがけて飛んでいく

    void Start()
    {
        shotSound = GetComponent<AudioSource>();
    }

    void Update()
    {
        //特定のボタンが押された時にShot()メソッドを発動
        if (Input.GetButtonDown("Fire1")) Shot();
    }

    // キャンディのプレハブからランダムに1つ選ぶ※voidじゃなくGameObjectが返ってくる
    //配列candyPrefabsの中からランダムにオブジェクトを1個取り出す
    GameObject sampleCandy()
    {
        int index = Random.Range(0, candyPrefabs.Length);
        return candyPrefabs[index];
    }

    //voidじゃなくVector3が返ってくる
    //マウスが押された位置と連動するようにBaseのどこをめがけてCandyを飛ばすか、
    //その位置を決めている
    Vector3 GetInstantiatePosition()
    {
        // 画面のサイズとInputの割合からキャンディ生成のポジションを計算
        float x = baseWidth *
            (Input.mousePosition.x / Screen.width) - (baseWidth / 2);
        return transform.position + new Vector3(x, 0, 0);
    }

    public void Shot()
    {
        // キャンディを生成できる条件外ならばShotしない
        if (candyManager.GetCandyAmount() <= 0) return;
        if (shotPower <= 0) return;

        // プレハブからCandyオブジェクトを生成※①Candyの生成Instantiate（対象物、位置、回転）
        GameObject candy = (GameObject)Instantiate(SampleCandy(),GetInstantiatePosition(),Quaternion.identity);

        // 生成したCandyオブジェクトの親をcandyParentTransformに設定する
        candy.transform.parent = candyParentTransform;

        // CadnyオブジェクトのRigidbodyを取得し力と回転を加える※②生成したCandyのRigidbodyを使えるようにしている
        //transform.forward→オブジェクトの前方
        Rigidbody candyRigidBody = candy.GetComponent<Rigidbody>();
        //※③生成したCandyにAddForce()メソッドをかけて飛ばしている
        candyRigidBody.AddForce(transform.forward * shotForce);
        //④横にスピンさせる力
        candyRigidBody.AddTorque(new Vector3(0, shotTorque, 0));

        // Candyのストックを消費
        candyManager.ConsumeCandy();
        // ShotPowerを消費
        ConsumePower();

        // サウンドを再生
        shotSound.Play();
    }

    void OnGUI()
    {
        GUI.color = Color.black;

        // ShotPowerの残数を+の数で表示 
        string label = "";
        for (int i = 0; i < shotPower; i++) label = label + "+";

        GUI.Label(new Rect(50, 65, 100, 30), label);
    }

    void ConsumePower()
    {
        // ShotPowerを消費すると同時に回復のカウントをスタート
        shotPower--;
        StartCoroutine(RecoverPower());
    }

    IEnumerator RecoverPower()
    {
        // 一定秒数待った後にshotPowerを回復
        yield return new WaitForSeconds(RecoverySeconds);
        shotPower++;
    }
}