using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.UI;

public class CustomGameManager : MonoBehaviour
{
    private static CustomGameManager singleton;
    public static CustomGameManager Singleton
    {
        get
        {
            return singleton;
        }
    }

    [SerializeField] private SocietyManager m_villageSociety;
    [SerializeField] private PlayerController m_playerControllerPrefab;

    private PlayerController m_activePlayerController = null;
    public void setActivePlayerController(PlayerController playerController)
    {
        m_activePlayerController = playerController;
    }

    public PlayerController getActivePlayerController()
    {
        return m_activePlayerController;
    }

    public void onPlayerDied()
    {
        if (m_villageSociety.getMembers().Count > 0)
        {
            PlayerUIInterface.Singleton.showPlayerDeadScreen(m_villageSociety.getMembers());
        }
        else
        {
            PlayerUIInterface.Singleton.showGameOverScreen();
        }
    }

    public void transformNPCToPlayer(NPCHuman npc)
    {
        GameObject npcGameObject = npc.gameObject;
        PlayerController playerController = copyComponent<PlayerController>(m_playerControllerPrefab, npcGameObject); // create component form prefab
        //PlayerController playerController = npcGameObject.AddComponent<PlayerController>();
        playerController.m_health = npc.m_health;
        playerController.m_gender = npc.getGender();
        playerController.m_ageYears = npc.m_ageYears;
        playerController.m_food = npc.m_food;
        playerController.m_water = npc.m_water;
        playerController.m_skills = npc.m_skills;
        playerController.m_weaponMountPoint = npc.m_weaponMountPoint;
        playerController.m_animator = npc.m_animator;
        playerController.m_animationEventReceiver = npc.m_animationEventReceiver;
        playerController.m_punchTrigger = npc.m_punchTrigger;

        removeGameobjectChildren(npcGameObject, "PerceptionTrigger");
        Destroy(npcGameObject.GetComponent<NPCHuman>());
        Destroy(npcGameObject.GetComponent<CursorRayCastTarget>());
        Destroy(npcGameObject.GetComponent<UnityEngine.AI.NavMeshAgent>());

        Rigidbody rigidbody = npc.GetComponent<Rigidbody>();
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        npcGameObject.tag = "Player";
        npcGameObject.name = "converted Player";
    }

    private T copyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    private bool removeGameobjectChildren(GameObject parent, string name)
    {
        for(int i = 0; i < parent.transform.childCount; i++)
        {
            if(parent.transform.GetChild(i).name.Equals(name))
            {
                Destroy(parent.transform.GetChild(i).gameObject);
                return true;
            }
            else
            {
                if(removeGameobjectChildren(parent.transform.GetChild(i).gameObject, name))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void Awake()
    {
        singleton = this;
        Time.timeScale = 1f;
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
