using System;
using System.Collections.Generic;

    [Serializable] public class EventData {
        public string ID;
        public string NodeType;
        public string Title;
        public string Description;
        public List<EventOption> Options;
    }

    [Serializable] public class EventOption {
        public string Text;
        public string ResultText;
        public List<Consequence> Consequences;
    }

    [Serializable] public class Consequence {
        public string Type;
        public int Amount;

        public void Apply() {
            switch (Type) {
                case "StartCombat":
                    EventManager.Instance.InitiateCombat();
                    break;
                case "ResourceChange":
                    GameManager.Instance.ModifySupplies(Amount);
                    break;
                case "AddSoldier":
                    GameManager.Instance.AddSoldier();
                    break;
                case "RemoveSoldier":
                    GameManager.Instance.RemoveSoldier();
                    break;    
                case "DamageSquad":
                    GameManager.Instance.DamageSquad();
                    break;
                default:
                    break;
            }
            CampaignMapManager.Instance.RefreshUI();
        }
    }