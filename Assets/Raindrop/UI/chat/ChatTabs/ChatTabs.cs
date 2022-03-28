// using System.Collections.Generic;
// using OpenMetaverse;
// using Raindrop.Presenters;
//
// namespace Raindrop
// {
//     public class ChatTabs : Dictionary<string, IChatTabUI>
//     {
//         public ChatTabs()
//         {
//         }
//
//         public bool TabExists(string imFromAgentName)
//         {
//             return true;
//         }
//         
//         public void SelectTab(string name)
//         {
//             if (TabExists(name.ToLower()))
//                 this[name.ToLower()].Select();
//         }
//
//         public void AddIMTab(UUID agentID, UUID selfAgentID, string label)
//         {
//             
//             presenter
//             
//             this[label] = new ChatTabPresenter();
//             RadegastTab tab = AddTab(session.ToString(), "IM: " + targetName, imTab);
//             imTab.SelectIMInput();
//
//             return imTab;
//         }
//     }
// }