using DebugTool.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.EventManager.Events;
using MyEventManager = Utilities.EventManager.EventManager;

namespace DebugTool.UI
{
    public class DebugButtonView : DebugToolsView
    {
        private Button m_Button;
        private TextMeshProUGUI m_ButtonLabel;
        
        void Start()
        {
            if(m_StartCalled) return;
            m_Button = GetComponentInChildren<Button>();
            
            m_ButtonLabel = GetComponentInChildren<TextMeshProUGUI>();
            
            m_Button.onClick.AddListener(OnClick);
            m_StartCalled = true;
        }

        private void OnClick()
        {
            MyEventManager.TriggerEvent<int>(Events.Debug.k_OnDebugingSettingChanged, Id);
        }

        public override void Initialize(DTEntry entryData)
        {
            if (entryData.m_Type != DTType.Button)
            {
                Debug.LogError("Trying to create an Button for a non Button Type : " + entryData.m_Type);
                return;
            }
            base.Initialize(entryData);
            
            if(!m_StartCalled) this.Start();

            DTButton button = (DTButton)entryData.m_Data;
            m_ButtonLabel.text = string.IsNullOrEmpty(button.m_Label) ? "A" : button.m_Label;
        }
    }
}
