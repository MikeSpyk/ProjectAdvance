using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BasicTools.UI.Tooltip;

public class SkillLevelUpEventArgs: EventArgs
{
    public string name {get;set;}
    public int level {get;set;}
}

public class SkillExperienceAddedEventArgs: EventArgs
{
    public string name {get;set;}
    public int level {get;set;}
    public float experience {get;set;}
    public float maxExperience {get;set;}
}

public class Skills : MonoBehaviour
{
    [SerializeField] private string[] m_skillsName;
    [SerializeField] private int[] m_skillsLevel;
    [SerializeField] private float[] m_skillsExperience;
    [SerializeField] private float[] m_skillsExperienceMax;

    public string[] skillsName{get{return m_skillsName;}}
    public event EventHandler<SkillLevelUpEventArgs> skillLeveledUp;
    public event EventHandler<SkillExperienceAddedEventArgs> experienceAdded;

    private Dictionary<string,int> m_skillName_index = new Dictionary<string, int>();
    private UIBuilder m_ui = null;

    void Awake()
    {
        initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private static float getMaxExperience(int level)
    {
        return Mathf.Pow(2,level);
    }

    private void initialize()
    {
        if(m_skillsLevel == null || m_skillsLevel.Length == 0)
        {
            m_skillsLevel = new int[m_skillsName.Length];
        }
        else if(m_skillsLevel.Length != m_skillsName.Length)
        {
            string carrierName = "";

            if(transform.parent != null)
            {
                carrierName = "(" + transform.parent.gameObject.name + ")";
            }

            Debug.LogWarning("m_skillsLevel.Length != m_skillsName.Length: reseting skills levels " + carrierName);
            m_skillsLevel = new int[m_skillsName.Length];
        }

        m_skillsExperience = new float[m_skillsName.Length];
        m_skillsExperienceMax = new float[m_skillsName.Length];

        for(int i = 0; i< m_skillsName.Length; i++)
        {
            m_skillName_index.Add(m_skillsName[i],i);
            m_skillsExperienceMax[i] = getMaxExperience(m_skillsLevel[i]);
        }
    }

    public void setUI(UIBuilder builder)
    {
        builder.clear();
        m_ui = builder;

        const float UI_SLIDER_WIDTH = 100f;

        List<UIBuilder.UIBuilderRowData> uiData = new List<UIBuilder.UIBuilderRowData>();

        uiData.Add(new UIBuilder.UIBuilderRowData(){
            controlsType = new UIBuilder.ControlType[]{UIBuilder.ControlType.Text},
            controlsContent = new string[]{ "Skills"},
            fontStyle = FontStyle.Bold
        });

        uiData.Add(new UIBuilder.UIBuilderRowData(){
            controlsType = new UIBuilder.ControlType[]{UIBuilder.ControlType.Text},
            controlsContent = new string[]{ " "},
        }); // newline

        uiData.Add(new UIBuilder.UIBuilderRowData(){
                controlsType = new UIBuilder.ControlType[]{UIBuilder.ControlType.Text,UIBuilder.ControlType.Text, UIBuilder.ControlType.Text},
                controlsContent = new string[]{"Skill;"+TextAnchor.MiddleCenter.ToString(),"Level;"+TextAnchor.MiddleCenter.ToString(),"Experience;"+TextAnchor.MiddleCenter.ToString()},
                identifier = "Headline"
        });

        for(int i = 0; i < m_skillsName.Length; i++)
        {
            uiData.Add(new UIBuilder.UIBuilderRowData(){
                controlsType = new UIBuilder.ControlType[]{UIBuilder.ControlType.Text,UIBuilder.ControlType.Text, UIBuilder.ControlType.Slider},
                controlsContent = new string[]{ m_skillsName[i], string.Format("{0};{1}", m_skillsLevel[i].ToString(),TextAnchor.MiddleCenter.ToString()), string.Format("{0};{1};{2}", m_skillsExperience[i], m_skillsExperienceMax[i], UI_SLIDER_WIDTH )},
                identifier = m_skillsName[i],
                tooltipData = new TextData[]{new TextData(){m_leftAlignedText=getTooltipText(i)}}
            });
        }

        builder.buildUI(uiData.ToArray());
    }

    public int getSkillLevel(string skillName)
    {
        return m_skillsLevel[m_skillName_index[skillName]];
    }

    public void addExperienceSkill(string skill, float experience)
    {
        int skillIndex = m_skillName_index[skill];

        m_skillsExperience[skillIndex] += experience;

        while(m_skillsExperience[skillIndex] > m_skillsExperienceMax[skillIndex])
        {
            m_skillsExperience[skillIndex] -= m_skillsExperienceMax[skillIndex];
            levelUp(skillIndex);
        }

        onExperienceAdded(new SkillExperienceAddedEventArgs()
                            {
                                name = m_skillsName[skillIndex],
                                level = m_skillsLevel[skillIndex],
                                experience = m_skillsExperience[skillIndex],
                                maxExperience = m_skillsExperienceMax[skillIndex]
                            });
    }

    private void onExperienceAdded(SkillExperienceAddedEventArgs args)
    {
        EventHandler<SkillExperienceAddedEventArgs> handler = experienceAdded;
        if (handler != null)
        {
            handler(this, args);
        }
    }

    private void levelUp(int skillIndex)
    {
        m_skillsLevel[skillIndex]++;
        m_skillsExperienceMax[skillIndex] = getMaxExperience(m_skillsLevel[skillIndex]);

        onLevelUp(new SkillLevelUpEventArgs(){level = m_skillsLevel[skillIndex], name = m_skillsName[skillIndex]});
    }

    private void onLevelUp(SkillLevelUpEventArgs args)
    {
        EventHandler<SkillLevelUpEventArgs> handler = skillLeveledUp;
        if (handler != null)
        {
            handler(this, args);
        }
    }

    public string getTooltipText(string skillName)
    {
        return getTooltipText(m_skillName_index[skillName]);
    }
    private string getTooltipText(int skillIndex)
    {
        string skillName = m_skillsName[skillIndex];
        int skillLevel = m_skillsLevel[skillIndex];

        switch(skillName)
        {
            case "Spears":
                return string.Format( "Physical damage with spears +{0}%",((skillLevel+1)*10) - 10);
            default:
                return "ERROR";
        }

    }

    public Tuple<string,string>[] getTextNameLevel()
    {
        Tuple<string,string>[] result = new Tuple<string, string>[m_skillsName.Length];

        for(int i = 0; i < m_skillsName.Length; i++)
        {
            result[i] = new Tuple<string, string>(string.Format("{0}: ", m_skillsName[i]), m_skillsLevel[i].ToString());
        }

        return result;
    }

}
