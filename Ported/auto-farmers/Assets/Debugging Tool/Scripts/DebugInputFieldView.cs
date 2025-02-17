using DebugTool.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.EventManager.Events;
using MyEventManager = Utilities.EventManager.EventManager;

namespace DebugTool.UI
{
    public class DebugInputFieldView : DebugToolsView
    {
        private TMP_InputField m_InputField;
        private Button m_Button;
        private TextMeshProUGUI m_Placeholder, m_ButtonLabel;
        
        void Start()
        {
            if(m_StartCalled) return;
            m_InputField = GetComponentInChildren<TMP_InputField>();
            m_Button = GetComponentInChildren<Button>();
            
            m_ButtonLabel = m_Button.GetComponentInChildren<TextMeshProUGUI>();
            m_Placeholder = m_InputField.transform.Find("Text Area/Placeholder").GetComponent<TextMeshProUGUI>();
            
            m_Button.onClick.AddListener(OnClick);
            m_StartCalled = true;
        }

        private void OnClick()
        {
            MyEventManager.TriggerEvent<int, string>(Events.Debug.k_OnDebugingSettingChanged, Id, m_InputField.text);
            m_EntryData.m_Action?.Invoke(Id, m_InputField.text);
        }

        public override void Initialize(DTEntry entryData)
        {
            if (entryData.m_Type != DTType.InputField)
            {
                Debug.LogError("Trying to create an InputField for a non InputField Type : " + entryData.m_Type);
                return;
            }
            base.Initialize(entryData);
            
            if(!m_StartCalled) this.Start();

            DTInputField inputField = (DTInputField)entryData.m_Data;
            m_Placeholder.text = inputField.m_InputFieldPlaceholder;
            m_ButtonLabel.text = string.IsNullOrEmpty(inputField.m_ButtonLabel) ? "A" : inputField.m_ButtonLabel;
        }
    }
}
