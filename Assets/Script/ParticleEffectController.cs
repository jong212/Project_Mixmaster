using UnityEngine;

public class ParticleEffectController : MonoBehaviour
{
    public GameObject particlePrefab;

    private Unit mc;
    public Unit Mc
    {
        get
        {
            if (mc == null)
            {
                mc = gameObject.GetComponent<Unit>();
                if (mc == null)
                {
                    Debug.LogError("MonsterController component not found");
                }
            }
            return mc;
        }
    }


    private ParticleSystem particleSystemInstance;
    public ParticleSystem ParticleSystemInstance
    {
        get
        {
            if (particleSystemInstance == null)
            {
                if (particlePrefab == null)
                {
                    Debug.LogError("particlePrefab is null");
                    return null;
                }

                if (Mc == null)
                {
                    Debug.LogError("Mc is null");
                    return null;
                }

                particleSystemInstance = Instantiate(particlePrefab, Mc.target.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
                if (particleSystemInstance == null)
                {
                    Debug.LogError("Failed to instantiate particle system");
                }
            }
            return particleSystemInstance;
        }
    }

    private void Start()
    {

    }

    public void PerformAttack()
    {
        ParticleSystem ps = ParticleSystemInstance;
        if (ps != null)
        {
            // 공격을 실행할 때 파티클 이펙트를 재생합니다.
            ps.transform.position = Mc.target.transform.position;
            ps.Play();
        }
        else
        {
            Debug.LogError("Failed to play particle system");
        }
    }
}
