using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickUpWaterSource : MonoBehaviour
{
    private Collider m_collider;

    public Collider surfaceCollider{get{return m_collider;}}

    void Awake()
    {
        m_collider = GetComponent<Collider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider.gameObject == this.gameObject)
            {
                PlayerUIInterface.Singleton.setActionText("Collect Water", new System.Action(pickUpWater));
            }
        }
    }

    private void pickUpWater()
    {
        PlayerController player = CustomGameManager.Singleton.getActivePlayerController();

        if(player != null)
        {
            if(player.addToPlayerInventory(0))
            {
                PlayerUIInterface.Singleton.printInformationText("+ Water");
                BasicTools.Audio.SoundManager.singleton.playGlobalSound(23);
            }
        }
    }
}
