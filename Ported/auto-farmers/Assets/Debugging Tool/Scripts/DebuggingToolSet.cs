using System;
using AutoFarmers.Tools;
using UnityEngine;

namespace DebugTool.UI
{
    public class DTEntry
    {
        public int m_Id;
        public string m_Title;
        public DTType m_Type;
        public DTTypeData m_Data;
        public Action<int, object> m_Action;
    }
    
    public enum DTType
    {
        None,
        Toggles,
        InputField,
        Button
    }

    public interface DTTypeData
    {
        
    }

    public class DTToggles : DTTypeData
    {
        public string[] m_Labels;
        public bool[] m_Values;
    }

    public class DTInputField : DTTypeData
    {
        public string m_InputFieldPlaceholder;
        public string m_ButtonLabel;

        public DTInputField(string mButtonLabel, string mInputFieldPlaceholder)
        {
            m_ButtonLabel = mButtonLabel;
            m_InputFieldPlaceholder = mInputFieldPlaceholder;
        }
    }

    public class DTButton : DTTypeData
    {
        public string m_Label;

        public DTButton(string mLabel)
        {
            m_Label = mLabel;
        }
    }

    public class DebuggingToolSet : MonoBehaviour
    {
        private DTEntry[] m_Entries;
        
        [SerializeField] private DebugToggleView m_DebugToggleViewPrefab;
        [SerializeField] private DebugInputFieldView m_DebugInputFieldPrefab;
        [SerializeField] private DebugButtonView m_DebugButtonPrefab;

        [SerializeField] private RectTransform m_Contents;

        void DefineEntriesHERE()
        {
            m_Entries = new[]
            {
                new DTEntry() //id = 1
                {
                    m_Title = "Debug Tool 1",
                    m_Type = DTType.Toggles,
                    m_Data = new DTToggles()
                    {
                        m_Labels = new string[]
                        {
                            "All",
                            "OG",
                            "None"
                        },

                        m_Values = new[] { false, false, false }
                    },
                    m_Action = (id, data) =>
                    {
                        string l = (string)data;
                        
                    }
                },

                new DTEntry() //id = 2
                {
                    m_Title = "Debug Tool 2",
                    m_Type = DTType.InputField,
                    m_Data = (DTTypeData)new DTInputField(mButtonLabel: "A", mInputFieldPlaceholder: "id")
                },

                new DTEntry() //id = 3
                {
                    m_Title = "Debug Tool 3",
                    m_Type = DTType.Button,
                    m_Data = (DTTypeData)new DTButton(string.Empty)
                }
            };
        }
        
        void Awake()
        {
            DebugToolsDataEntries.Instance.Init();
            //DefineEntriesHERE();
            InitilizeTool();
        }

        private void InitilizeTool()
        {
            m_Entries = DebugToolsDataEntries.Instance.Entries;
            for (int i=0; i<m_Entries.Length; i++)
            {
                DTEntry entry = m_Entries[i];
                entry.m_Id = i+1;

                switch (entry.m_Type)
                {
                    case DTType.Toggles:
                        DebugToggleView toggleView = Instantiate(m_DebugToggleViewPrefab, m_Contents);
                        toggleView.Initialize(entry);
                        break;
                    
                    case DTType.InputField:
                        DebugInputFieldView inputFieldView = Instantiate(m_DebugInputFieldPrefab, m_Contents);
                        inputFieldView.Initialize(entry);
                        break;
                    
                    case DTType.Button:
                        DebugButtonView debugButtonView = Instantiate(m_DebugButtonPrefab, m_Contents);
                        debugButtonView.Initialize(entry);
                        break;
                    
                    default:
                        Debug.LogError("Unknown DTType: " + entry.m_Type);
                        return;
                }
            }
        }
    }


}
