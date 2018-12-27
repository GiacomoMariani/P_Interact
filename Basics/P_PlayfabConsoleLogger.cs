using JReact;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace JReact.Playfab_Interact
{

    public class P_PlayfabConsoleLogger
    {
        /// <summary>
        /// logs a message related to playfab
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the object sending the message</param>
        public static void DisplayMessage(string message, string sender, ConsoleMessageTypes logType = ConsoleMessageTypes.Playfab_Interact)
        {
            string messageTag = string.Format("-{0}-", logType.ToString());
            HelperConsole.DisplayMessage(string.Format("{0}-{1}- {2}", messageTag, sender, message));
        }

        /// <summary>
        /// logs a warning related to playfab
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the object sending the message</param>
        internal static void DisplayWarning(string message, string sender)
        {
            HelperConsole.DisplayWarning(string.Format("{0}-{1}- {2}", P_Constants.DEBUG_PlayfabInteract, sender, message));
        }
        
        //the callback when an error appens
        public static void LogErrorFrom(PlayFabError error, string sender)
        {
            HelperConsole.DisplayError(string.Format("{0}-{1}-Error {2}: {3}\n{4}",
                                                     P_Constants.DEBUG_PlayfabInteract, sender, error.Error, error.ErrorMessage,
                                                     error.GenerateErrorReport()));
        }
        
        /// <summary>
        /// this is used to display all the logs from an external call
        /// </summary>
        /// <param name="result"></param>
        public static void DisplayAllLogs(ExecuteCloudScriptResult result)
        {
            foreach (var log in result.Logs)
            {
                HelperConsole.DisplayMessage(string.Format("{0}- Log {1}", P_Constants.DEBUG_PlayfabInteract, log.Message));
            }
        }
    }
}
