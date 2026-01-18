using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject Melee;
    bool isAttacking = false;
    float atkDuration = 0.3f;
    float atkTimer = 0f;

    //range
    public Transform aim;
    public GameObject bullet;
    public float fireforce = 10f;
    public float shootCooldown = 0.25f;
    public float shootTimer = 0.5f;

    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        CheckMeleeTimer();
        shootTimer += Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            OnAttack();
        }
        if (Input.GetMouseButton(1)) {
            OnShoot();
        }
    }

    void OnShoot()
    {
        if (shootTimer > shootCooldown)
        {
            shootTimer = 0f;
            animator.SetTrigger("Ranged");
            GameObject intBullet = Instantiate(bullet, aim.position, aim.rotation);
            intBullet.GetComponent<Rigidbody2D>().AddForce(-aim.up * fireforce, ForceMode2D.Impulse);
            Destroy(intBullet, 2f);
        }
    }

    void OnAttack()
    {
        if (!isAttacking)
        {
            Melee.SetActive(true);
            isAttacking = true;
            //call animator?
        }
    }

    void CheckMeleeTimer()
    {
        if (isAttacking)
        {
            atkTimer += Time.deltaTime;
            if (atkTimer >= atkDuration)
            {
                atkTimer = 0;
                Melee.SetActive(false);
            }
        }
    }
}