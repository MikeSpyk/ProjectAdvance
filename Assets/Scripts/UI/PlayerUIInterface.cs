using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using BasicTools.UI.Tooltip;
using BasicTools.UI;
using BasicTools.UI.Inventory;

public class PlayerUIInterface : MonoBehaviour
{
    public static PlayerUIInterface Singleton
    {
        get
        {
            return singleton;
        }
    }
    private static PlayerUIInterface singleton;

    [Header("Stats Bars")]
    [SerializeField] private Slider m_healtbarSlider;
    [SerializeField] private Text m_healtbarText;
    [SerializeField] private Slider m_staminaSlider;
    [SerializeField] private Text m_staminaText;
    [SerializeField] private Slider m_foodSlider;
    [SerializeField] private Text m_foodText;
    [SerializeField] private Slider m_waterSlider;
    [SerializeField] private Text m_waterText;
    [Header("Switchable Menus")]
    [SerializeField] private GameObject m_inventoryParent;
    [SerializeField] private GameObject m_inventoryMenu;
    [SerializeField] private GameObject m_characterItemsMenu;
    [SerializeField] private GameObject m_externalContainerMenu;
    [SerializeField] private GameObject m_skillsMenu;
    [SerializeField] private GameObject m_BuildingMenu;
    [SerializeField] private GameObject m_ingamePanel;
    [SerializeField] private GameObject m_deadPanel;
    [SerializeField] private GameObject m_dead_villagersScrollviewContent;
    [SerializeField] private GameObject m_infoMenu;
    [SerializeField] private GameObject m_mainMenu;
    [SerializeField] private GameObject m_gameOverMenu;
    [SerializeField] private GameObject m_gameWonMenu;
    [Header("Input Keys")]
    [SerializeField] private KeyCode m_inventoryKey = KeyCode.I;
    [SerializeField] private KeyCode m_skillsKey = KeyCode.J;
    [SerializeField] private KeyCode m_buildingKey = KeyCode.B;
    [Header("General")]
    [SerializeField] private GameObject m_GameObject_EventSystem;
    [SerializeField] private GameObject m_GUI_Canvas;
    [SerializeField] private UIItemContainer m_playerInventoryContainer;
    [SerializeField] private UIItemContainer m_playerEquipmentContainer;
    [SerializeField] private UIItemContainer m_externalContainer;
    [SerializeField] private ContextMenu m_contextMenu;
    [SerializeField] private BasicTools.GameObjects.ObjectPlacer m_buildingPlacer;
    [SerializeField] private BasicTools.GameObjects.ObjectPlacer m_furniturePlacer;
    [SerializeField] private GameObject m_scrollViewChildPrefab;
    [SerializeField] private Button m_playerDeadRespawnButton;
    [Header("Info Text")]
    [SerializeField] private FadingTextList m_infoTextUI;

    private bool m_actionTextThisFrame = false;
    private int m_lastFrameActionText = 0;
    private Action m_actionTextClickAction = null;
    private UIBuilder m_skillsMenuBuilder;
    private EventSystem m_EventSystem;
    private GraphicRaycaster m_Raycaster;
    private ItemContainerWorld m_lastOpenedExternalContainer = null;
    private NPCHuman m_selectedRespawnHuman = null;
    private int m_noTriggerLayerMask = 0;
    private List<RaycastResult> m_lastUiRaycastResult = new List<RaycastResult>();
    private int m_lastFrameUiRaycast = 0;

    public UIBuilder getSkillsMenuBuilder()
    {
        return m_skillsMenuBuilder;
    }

    public UIItemContainer getPlayerInventoryContainer()
    {
        return m_playerInventoryContainer;
    }

    public UIItemContainer getPlayerEquipmentContainer()
    {
        return m_playerEquipmentContainer;
    }

    void Awake()
    {
        singleton = this;
        m_skillsMenuBuilder = m_skillsMenu.GetComponent<UIBuilder>();
        m_noTriggerLayerMask = (int.MaxValue-(int)Mathf.Pow(2,10)); // everything except layer 10
    }

    // Start is called before the first frame update
    void Start()
    {
        m_EventSystem = m_GameObject_EventSystem.GetComponent<EventSystem>();
        m_Raycaster = m_GUI_Canvas.GetComponent<GraphicRaycaster>();

        //setHealth(50,100);
        setStamina(UnityEngine.Random.value * 100f, 100f);
        setFood(UnityEngine.Random.value * 100f, 100f);
        setWater(UnityEngine.Random.value * 100f, 100f);

        switchGameobjectActiveInactive(m_inventoryParent);

        addManagedMenu(m_inventoryMenu, true);
        addManagedMenu(m_inventoryParent, true);
        addManagedMenu(m_skillsMenu, true);
        addManagedMenu(m_BuildingMenu, true);
        addManagedMenu(m_infoMenu, true);
        addManagedMenu(m_mainMenu, true);
    }

    // Update is called once per frame
    void Update()
    {
        notifyWorldObjectsUnderCursor();

        UITooltipData tooltipData = getUIComponentUnderMouse<UITooltipData>();

        if(tooltipData != null)
        {
            showGenericTooltip(tooltipData);
        }

        if(Input.GetKeyDown(m_inventoryKey))
        {
            toggleManagedMenu(m_inventoryParent);
        }

        if(Input.GetKeyDown(m_skillsKey))
        {
            toggleManagedMenu(m_skillsMenu);
        }

        if(Input.GetKeyDown(m_buildingKey))
        {
            toggleManagedMenu(m_BuildingMenu);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject menu = closeLatestAvailableMenu();

            if(menu == null)
            {
                openMenu(m_mainMenu);
            }
        }

        if(m_actionTextThisFrame)
        {
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                if(m_actionTextClickAction != null)
                {
                    m_actionTextClickAction();
                }
            }
        }

        m_actionTextThisFrame = false;
    }

    public void showGameWonScreen()
    {
        m_gameWonMenu.SetActive(true);
    }

    public void hideGameWonScreen()
    {
        m_gameWonMenu.SetActive(false);
    }

    public void showGameOverScreen()
    {
        m_gameOverMenu.SetActive(true);
    }

    public void onButtenRestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void onButtenMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void onMenuClosed(GameObject menu)
    {
        if(menu == m_inventoryParent)
        {
            onInventoryClosed();
        }
        else if(menu == m_mainMenu)
        {
            Time.timeScale = 1;
        }
    }
    private void onMenuOpened(GameObject menu)
    {
        if (menu == m_mainMenu)
        {
            Time.timeScale = 0.000001f;
            Debug.Log("Time.timeScale: " + Time.timeScale);
        }
    }

    private void onInventoryClosed()
    {
        if (m_externalContainerMenu.activeSelf)
        {
            Debug.Log("onInventoryClosed");
            onPlayerCloseExternalContainer();
            m_externalContainerMenu.SetActive(false);
        }
    }

    Dictionary<GameObject, float> m_menuLastTimeToggled = new Dictionary<GameObject, float>();
    Dictionary<GameObject, bool> m_menuCanBeClosedByUser = new Dictionary<GameObject, bool>();
    Dictionary<GameObject, int> m_menuNestedLevel = new Dictionary<GameObject, int>();
    private List<GameObject> m_activeGameObjects = new List<GameObject>();
    private List<GameObject> m_managedMenus = new List<GameObject>();

    /// <summary>
    /// Adds Menu to be managed by this object. Managed Menus can be closed in the right order by using closeLatestAvailabelMenu()
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="closeEscape">can this menu be closed by the user?</param>
    protected void addManagedMenu(GameObject menu, bool canBeClosedByUser)
    {
        if(m_managedMenus.Contains(menu))
        {
            throw new System.ArgumentException("item is already in collection: " + menu.name);
        }

        if (menu.activeInHierarchy)
        {
            m_activeGameObjects.Add(menu);
        }
        m_menuCanBeClosedByUser.Add(menu,canBeClosedByUser);
        m_menuLastTimeToggled.Add(menu,Time.time);
        m_menuNestedLevel.Add(menu,getNestedLevel(menu.transform));
        m_managedMenus.Add(menu);
    }

    protected GameObject closeLatestAvailableMenu()
    {
        float latestTimeMenuToggled = float.MinValue;
        int latestMenuIndex = -1;

        for (int i = 0; i < m_activeGameObjects.Count; i++)
        {
            if (m_menuCanBeClosedByUser[m_activeGameObjects[i]] &&
                m_menuLastTimeToggled[m_activeGameObjects[i]] > latestTimeMenuToggled)
            {
                latestTimeMenuToggled = m_menuLastTimeToggled[m_activeGameObjects[i]];
                latestMenuIndex = i;
            }
        }

        if (latestMenuIndex > -1)
        {
            GameObject returnValue = m_activeGameObjects[latestMenuIndex];

            closeMenu(m_activeGameObjects[latestMenuIndex]);

            return returnValue;
        }

        return null;
    }

    protected void toggleManagedMenu(GameObject menu)
    {
        if (menu.activeInHierarchy)
        {
            closeMenu(menu);
        }
        else
        {
            openMenu(menu);
        }
    }

    protected virtual void closeMenu(GameObject menu)
    {
        menu.SetActive(false);
        m_activeGameObjects.Remove(menu);

        onMenuClosed(menu); // TODO: delte when converting to base class
    }

    protected virtual void openMenu(GameObject menu)
    {
        menu.SetActive(true);
        m_menuLastTimeToggled.Remove(menu);
        m_menuLastTimeToggled.Add(menu, Time.time);

        if (m_activeGameObjects.Contains(menu))
        {
            Debug.LogWarning("Menu was disabled out of manager control. GameObject: \"" + menu.name + "\". Use closeMenu()");
        }
        else
        {
            m_activeGameObjects.Add(menu);
        }

        onMenuOpened(menu); // TODO: delte when converting to base class
    }

    private List<int> findDeepstNestedLevelMenus()
    {
        int deepestGameObjectLevel = int.MinValue;
        List<int> deepestLevelGameObjectsIndex = new List<int>();

        for (int i = 0; i < m_activeGameObjects.Count; i++)
        {
            if (m_activeGameObjects[i].activeInHierarchy)
            {
                if (m_menuNestedLevel[m_activeGameObjects[i]] > deepestGameObjectLevel)
                {
                    deepestGameObjectLevel = m_menuNestedLevel[m_activeGameObjects[i]];
                    deepestLevelGameObjectsIndex.Clear();
                }

                if (m_menuNestedLevel[m_activeGameObjects[i]] == deepestGameObjectLevel)
                {
                    deepestLevelGameObjectsIndex.Add(i);
                }
            }
            else
            {
                Debug.LogWarning("Menu was disabled out of manager control. GameObject: \"" + m_activeGameObjects[i].name + "\". Use closeMenu()");
                m_activeGameObjects.RemoveAt(i);
                i--;
            }
        }

        return deepestLevelGameObjectsIndex;
    }

    private int getNestedLevel(Transform transform)
    {
        int counter = 0;

        while(transform.parent != null && counter < 10000)
        {
            transform = transform.parent;
            counter++;
        }

        return counter;
    }

    public bool isMouseOnUi()
    {
        if(Time.frameCount - m_lastFrameActionText < 2)
        {
            return true;
        }

        List<RaycastResult> uiElements = getUIGameobjectsUnderMouse();

        if(uiElements.Count == 0 || uiElements.Count == 1 && uiElements[0].gameObject == m_ingamePanel)
        {
            return false;
        }

        return true;
    }

    public void switchInfoScreen()
    {
        toggleManagedMenu(m_infoMenu);
    }

    public void showPlayerDeadScreen(List<NPCHuman> respawnNPCs)
    {
        RectTransform buttonPrefabRect = m_scrollViewChildPrefab.transform as RectTransform;
        RectTransform scrollViewRect = m_dead_villagersScrollviewContent.transform as RectTransform;

        for(int i = 0; i < m_dead_villagersScrollviewContent.transform.childCount; i++)
        {
            Destroy(m_dead_villagersScrollviewContent.transform.GetChild(i).gameObject);
        }

        scrollViewRect.sizeDelta = new Vector2(scrollViewRect.sizeDelta.x,respawnNPCs.Count * buttonPrefabRect.sizeDelta.y);

        for(int i = 0; i < respawnNPCs.Count; i++)
        {
            GameObject villagerButton = Instantiate(m_scrollViewChildPrefab, m_dead_villagersScrollviewContent.transform);
            villagerButton.transform.localPosition = new Vector3(buttonPrefabRect.sizeDelta.x/2,-buttonPrefabRect.sizeDelta.y/2 -buttonPrefabRect.sizeDelta.y * i,0);

            villagerButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Villager " + (i+1);
            Button button = villagerButton.GetComponent<Button>();

            {
                NPCHuman tempVillager = respawnNPCs[i];
                Button tempButton = button;
                button.onClick.AddListener(()=>{onRespawnVillagerSelected(tempVillager, tempButton);});
            }

            UITooltipData tooltip = villagerButton.GetComponent<UITooltipData>();

            var npcToolTipData = respawnNPCs[i].getRespawnInfoData();
            TextData[] toolTipData = new TextData[npcToolTipData.Count];

            for(int j = 0; j< npcToolTipData.Count; j++)
            {
                toolTipData[j] = new TextData(){m_leftAlignedText = npcToolTipData[j].Item1, m_rightAlignedText = npcToolTipData[j].Item2};
            }

            tooltip.textData = toolTipData;
        }

        m_playerDeadRespawnButton.enabled = false;

        m_ingamePanel.SetActive(false);
        m_deadPanel.SetActive(true);
    }

    private void onRespawnVillagerSelected(NPCHuman villager, Button button)
    {
        if(villager == null)
        {
            Destroy(button.gameObject);
        }
        else
        {
            m_selectedRespawnHuman = villager;
            m_playerDeadRespawnButton.enabled = true;
        }
    }

    public void onRespawnButtonClick()
    {
        if(m_selectedRespawnHuman != null)
        {
            CustomGameManager.Singleton.transformNPCToPlayer(m_selectedRespawnHuman);
            m_ingamePanel.SetActive(true);
            m_deadPanel.SetActive(false);
        }
    }

    public void onPlayerOpenItemContainer(ItemContainerWorld container)
    {
        const int rowSize = 10;

        m_lastOpenedExternalContainer = container;

        int rows = container.itemSlots / rowSize;
        int additionalSlots = container.itemSlots % rowSize;
    
        if(additionalSlots != 0)
        {
            Debug.LogError("container slot count is not dividable by" + rowSize);
        }

        if(!m_externalContainerMenu.activeInHierarchy)
        {
            m_externalContainerMenu.SetActive(true);
        }

        if(!m_inventoryParent.activeInHierarchy)
        {
            toggleManagedMenu(m_inventoryParent);
        }

        m_externalContainer.updateLayout(rowSize, rows);
        m_externalContainer.setItems(container.items_itemIndex_additionalData);
    }

    private void onPlayerCloseExternalContainer()
    {
        if(m_lastOpenedExternalContainer != null)
        {
            m_lastOpenedExternalContainer.updateItems(m_externalContainer.getItemsData());
            m_lastOpenedExternalContainer = null;
        }
    }

    public void onPlayerClickBuildHouse()
    {
        if(checkPlayerBuildHouseConditions())
        {
            m_buildingPlacer.startPlacingMode(0, CustomGameManager.Singleton.getActivePlayerController().gameObject, new Func<bool>(checkPlayerBuildHouseConditions), new Action(onPlayerBuiltHouse));
            switchGameobjectActiveInactive(m_BuildingMenu);
        }
    }

    public void onPlayerClickBuildBox()
    {
        if(checkPlayerBuildBoxConditions())
        {
            m_furniturePlacer.startPlacingMode(1, CustomGameManager.Singleton.getActivePlayerController().gameObject, new Func<bool>(checkPlayerBuildBoxConditions), new Action(onPlayerBuiltBox));
            switchGameobjectActiveInactive(m_BuildingMenu);
        }
    }

    private bool checkPlayerBuildHouseConditions()
    {
        
        if(m_playerInventoryContainer.getItemCount(4) < 30)
        {
            printInformationText("Not enough stone");
            return false;
        }
        else if(m_playerInventoryContainer.getItemCount(5) < 30)
        {
            printInformationText("Not enough wood");
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool checkPlayerBuildBoxConditions()
    {
        if(m_playerInventoryContainer.getItemCount(5) < 2)
        {
            printInformationText("Not enough wood");
            return false;
        }
        else
        {
            return true;
        }
    }

    private void onPlayerBuiltHouse()
    {
        m_playerInventoryContainer.removeItems(4, 30); // stone
        m_playerInventoryContainer.removeItems(5, 30); // wood
    }

    private void onPlayerBuiltBox()
    {
        m_playerInventoryContainer.removeItems(5, 2); // wood
    }

    private void notifyWorldObjectsUnderCursor()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit,float.MaxValue, m_noTriggerLayerMask))
        {
            CursorRayCastTarget target = hit.collider.GetComponent<CursorRayCastTarget>();
            //Debug.Log("under Mouse: " + hit.collider.gameObject.name);

            if(target != null)
            {
                target.onCursorOver();
            }
        }
    }

    public void showContextMenu(ContextMenuItemBase[] elements)
    {
        m_contextMenu.showContextMenu(elements);
    }

    public void setActiveSkills(Skills skills)
    {
        skills.setUI(m_skillsMenuBuilder);
    }

    public void setHealth(float current, float max)
    {
        m_healtbarSlider.value = current/max;
        m_healtbarText.text = string.Format("{0}/{1}", (int)current,(int)max);
    }

    public void setStamina(float current, float max)
    {
        m_staminaSlider.value = current/max;
        m_staminaText.text = string.Format("{0}/{1}", (int)current,(int)max);
    }

    public void setFood(float current, float max)
    {
        m_foodSlider.value = current/max;
        m_foodText.text = string.Format("{0}/{1}", (int)current,(int)max);
    }

    public void setWater(float current, float max)
    {
        m_waterSlider.value = current/max;
        m_waterText.text = string.Format("{0}/{1}", (int)current,(int)max);
    }

    public void setActionText(string text, Action onClick)
    {
        m_actionTextThisFrame = true;
        m_lastFrameActionText = Time.frameCount;

        TooltipManager.Singleton.setTooltipData(text.GetHashCode(), new TextData(){m_leftAlignedText=text});

        m_actionTextClickAction = onClick;
    }

    public void printInformationText(string text)
    {
        m_infoTextUI.printText(text);
    }

    public void showItemTooltip(UIItemData itemData)
    {
        // TODO: hier auch hash überprüfen und daten-erzeugung überspringen

        string name = ItemsInterface.singleton.getDisplayName(itemData.itemIndex);
        string description = ItemsInterface.singleton.getDescription(itemData.itemIndex);

        List<TextData> textDataList = new List<TextData>();

        textDataList.Add(new TextData(){m_leftAlignedText = name, m_fontStyle = FontStyle.Bold});
        textDataList.Add(new TextData(){m_leftAlignedText = description});

        if(ItemsInterface.singleton.getItemType(itemData.itemIndex) == StorableItemTemplate.ItemType.WeaponDefault)
        {
            textDataList.Add(new TextData(){m_leftAlignedText = " "}); // newline

            float damageSum = 0f;
            const float SPACE_EFFECTNAME_AMOUNT = 30f;

            string physicalDamage = itemData.getAdditionalData("physicalDamage");
            if(physicalDamage != null)
            {
                damageSum += float.Parse(physicalDamage);
                textDataList.Add(new TextData(){
                                                            m_leftAlignedText = "Physical Damage",
                                                            m_rightAlignedText = physicalDamage,
                                                            m_blankSpaceWidth = SPACE_EFFECTNAME_AMOUNT,
                                                            m_color = Color.black
                                                            });
            }

            string fireDamage = itemData.getAdditionalData("fireDamage");
            if(fireDamage != null)
            {
                damageSum += float.Parse(fireDamage);
                textDataList.Add(new TextData(){
                                                            m_leftAlignedText = "Fire Damage",
                                                            m_rightAlignedText = fireDamage,
                                                            m_blankSpaceWidth = SPACE_EFFECTNAME_AMOUNT,
                                                            m_color = Color.red
                                                            });
            }

            string iceDamage = itemData.getAdditionalData("iceDamage");
            if(iceDamage != null)
            {
                damageSum += float.Parse(iceDamage);
                textDataList.Add(new TextData(){
                                                            m_leftAlignedText = "Ice Damage",
                                                            m_rightAlignedText = iceDamage,
                                                            m_blankSpaceWidth = SPACE_EFFECTNAME_AMOUNT,
                                                            m_color = Color.blue
                                                            });
            }

            string lightningDamage = itemData.getAdditionalData("lightningDamage");
            if(lightningDamage != null)
            {
                damageSum += float.Parse(lightningDamage);
                textDataList.Add(new TextData(){
                                                            m_leftAlignedText = "Lightning Damage",
                                                            m_rightAlignedText = lightningDamage,
                                                            m_blankSpaceWidth = SPACE_EFFECTNAME_AMOUNT,
                                                            m_color = Color.yellow
                                                            });
            }

            textDataList.Add(new TextData(){
                                                            m_rightAlignedText = String.Format("= {0}", damageSum),
                                                            m_color = Color.black
                                                            });
        }

        TooltipManager.Singleton.setTooltipData(itemData.GetHashCode(), textDataList.ToArray());
    }

    private void showGenericTooltip(UITooltipData data)
    {
        TooltipManager.Singleton.setTooltipData(data.GetHashCode(), data.textData);
    }

    private static void switchGameobjectActiveInactive(GameObject gameObject)
    {
        if(gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public T getUIComponentUnderMouse<T>()
    {
        List<RaycastResult> results = getUIGameobjectsUnderMouse();

        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        foreach (RaycastResult result in results)
        {
            T component = result.gameObject.GetComponent<T>();

            if (component != null)
            {
                return component;
            }
        }

        return default(T);
    }

    public List<RaycastResult> getUIGameobjectsUnderMouse()
    {
        if(Time.frameCount != m_lastFrameUiRaycast)
        {
            //Set up the new Pointer Event
            PointerEventData m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            m_lastUiRaycastResult.Clear();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, m_lastUiRaycastResult);

            m_lastFrameUiRaycast = Time.frameCount;
        }

        return m_lastUiRaycastResult;
    }
}
