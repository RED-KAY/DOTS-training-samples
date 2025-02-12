using Utilities.EventManager;
using Utilities.EventManager.Events;
using TMPro;
using MyEventManager = Utilities.EventManager.EventManager;

namespace DebugTool.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEditor;

    [RequireComponent(typeof(ToggleGroup))]
    public class DebugToggleView : DebugToolsView
    {
        private ToggleGroup m_ToggleGroup;

        [SerializeField] private RectTransform m_Content;
        [SerializeField] private Toggle m_Toggle;
        private RectTransform m_MyRectTransform;
        
        private void Start()
        {
            if(m_StartCalled) return;
            m_ToggleGroup = GetComponent<ToggleGroup>();
            m_MyRectTransform = GetComponent<RectTransform>();

            m_MyRectTransform.sizeDelta = new Vector2(m_MyRectTransform.sizeDelta.x, 40f);
            m_StartCalled = true;
        }

        public override void Initialize(DTEntry entryData)
        {
            if (entryData.m_Type != DTType.Toggles)
            {
                Debug.LogError("Trying to create a ToggleView for a non Toggle Type : " + entryData.m_Type);
                return;
            }

            base.Initialize(entryData);
            
            if(!m_StartCalled) this.Start();

            ref string[] labels = ref ((DTToggles)entryData.m_Data).m_Labels;
            ref bool[] values = ref ((DTToggles)entryData.m_Data).m_Values;

            for (int i = 0; i < labels.Length; i++)
            {
                Toggle t = Instantiate(m_Toggle, m_Content);
                t.gameObject.SetActive(true);
                m_MyRectTransform.sizeDelta += new Vector2(0, 20f);
                TextMeshProUGUI tmp = t.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                tmp.text = labels[i];
                if (values.Length <= 0 || values.Length - 1 < i)
                    t.isOn = false;
                else
                    t.isOn = values[i];

                t.group = m_ToggleGroup;

                t.onValueChanged.AddListener(OnValueChanged);
            }
        }

        private void OnValueChanged(bool arg0)
        {
            if (!arg0) return;
            MyEventManager.TriggerEvent<int, bool>(Events.Debug.k_OnDebugingSettingChanged, Id, arg0);
        }
    }
}