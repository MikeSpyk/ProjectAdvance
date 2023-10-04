using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerAreaRectangle : SpawnerArea
{
    void OnDrawGizmosSelected()
    {
        Vector3 bottom_down_left = transform.position - transform.localScale/2;
        Vector3 bottom_up_left = bottom_down_left + new Vector3(0,0,transform.localScale.z);
        Vector3 bottom_up_right = bottom_up_left + new Vector3(transform.localScale.x,0,0);
        Vector3 bottom_down_right = bottom_down_left + new Vector3(transform.localScale.x,0,0);

        Vector3 top_up_left = bottom_up_left + new Vector3(0,transform.localScale.y,0);
        Vector3 top_down_left = bottom_down_left + new Vector3(0,transform.localScale.y,0);
        Vector3 top_up_right = bottom_up_right + new Vector3(0,transform.localScale.y,0);
        Vector3 top_down_right = bottom_down_right + new Vector3(0,transform.localScale.y,0);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(bottom_up_left, bottom_up_right);
        Gizmos.DrawLine(bottom_up_right, bottom_down_right);
        Gizmos.DrawLine(bottom_down_right, bottom_down_left);
        Gizmos.DrawLine(bottom_down_left, bottom_up_left);

        Gizmos.DrawLine(top_up_left, top_up_right);
        Gizmos.DrawLine(top_up_right, top_down_right);
        Gizmos.DrawLine(top_down_right, top_down_left);
        Gizmos.DrawLine(top_down_left, top_up_left);

        Gizmos.DrawLine(bottom_down_left, top_down_left);
        Gizmos.DrawLine(bottom_up_left, top_up_left);
        Gizmos.DrawLine(bottom_up_right, top_up_right);
        Gizmos.DrawLine(bottom_down_right, top_down_right);
    }

    public override float getSurfaceArea()
    {
        return transform.localScale.x * transform.localScale.z;
    }

    public override Vector2 getRandomPositionWithin(float seed)
    {
        float randomX = BasicTools.Random.RandomValuesSeed.getRandomValueSeed(seed, seed * 1.33f);
        float randomY = BasicTools.Random.RandomValuesSeed.getRandomValueSeed(seed, seed * 1.71f);

        return new Vector2( transform.position.x - transform.localScale.x/2 + transform.localScale.x *randomX,
                            transform.position.z - transform.localScale.z/2 + transform.localScale.z *randomY   );
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
