using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 0f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int direction = 1;

    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.flipX = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (transform.position.x < minX)
        {
            spriteRenderer.flipX = true;
            direction = 1;
        }
        else if (transform.position.x > maxX)
        {
            spriteRenderer.flipX = false;
            direction = -1;
        }
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);


    }


}
