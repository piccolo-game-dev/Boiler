using System.Collections;
using UnityEngine;

using Garbage;

public class GarbageCollector : MonoBehaviour
{
    [Header("吸い込み設定")]
    [SerializeField] private float _maxDistance = 5f; // レイキャストが届く最大距離
    [SerializeField] private float _suckSpeed = 8f; // ゴミが手元に吸い込まれる速度
    [SerializeField] private Transform _suckTarget; // 吸い込みの目的地(プレイヤーの手元など)
    [Header("エフェクト・SE")]
    [SerializeField] private AudioClip collectSound; // 回収時の「ポンッ」というSE
    private AudioSource audioSource;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // 左クリック入力(押し続けにも対応させる場合は GetMouseButton)
        if (Input.GetMouseButtonDown(0))
        {
            TrySuckGarbage();
        }
    }

    private void TrySuckGarbage()
    {
        // 画面中央からレイを飛ばす
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, _maxDistance))
        {
            // 対象が「Garbage」タグを持っているか確認
            if (hit.collider.CompareTag(GarbageConstant.GARBAGE_TAG))
            {
                // すでに吸い込み中でなければ、コルーチンを開始
                GameObject garbage = hit.collider.gameObject;
                if (garbage.GetComponent<GarbageItem>() == null)
                {
                    StartCoroutine(SuckRoutine(garbage));
                }
            }
        }
    }

    private IEnumerator SuckRoutine(GameObject garbage)
    {
        // 重力や物理衝突が吸い込みの邪魔をしないよう、一時的にコンポーネントを無効化
        Rigidbody rb = garbage.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Collider col = garbage.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        // 念のため重複吸い込み防止用マーカーコンポーネントを付与
        garbage.AddComponent<GarbageItem>();
        // 手元のターゲット位置(未設定ならこのスクリプトの位置)
        Vector3 targetPos = _suckTarget != null ? _suckTarget.position : transform.position;
        // ターゲットに近づくまでループ移動&縮小演出
        while (Vector3.Distance(garbage.transform.position, targetPos) > 0.1f)
        {
            targetPos = _suckTarget != null ? _suckTarget.position : transform.position;

            // 移動速度をなめらかに補間
            garbage.transform.position = Vector3.MoveTowards(
                garbage.transform.position,
                targetPos,
                _suckSpeed * Time.deltaTime
                );

            // 吸い込まれながら徐々に小さくする演出(チル感を出す視覚効果)
            garbage.transform.localScale = Vector3.Lerp(
                garbage.transform.localScale,
                Vector3.zero,
                _suckSpeed * Time.deltaTime
                );

            yield return null;

        }
        // 手元に到達したらSEを鳴らしてオブジェクトを完全に削除
        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        Destroy(garbage);
    }
}

// 重複吸い込みを検知するための軽量クラス
public class GarbageItem : MonoBehaviour { }
