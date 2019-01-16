using JReact;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace JReact.Playfab_Interact
{
    public class PConsole
    {
        /// <summary>
        /// logs a message related to playfab
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the object sending the message</param>
        public static void Log(string message, string sender, Object context = null)
        {
            JConsole.Log($"-{sender}- {message}", P_Constants.DEBUG_PlayfabInteract, context);
        }

        /// <summary>
        /// logs a warning related to playfab
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the object sending the message</param>
        internal static void Warning(string message, string sender, Object context = null)
        {
            JConsole.Warning($"-{sender}- {message}", P_Constants.DEBUG_PlayfabInteract, context);
        }

        //the callback when an error happens
        public static void ErrorFrom(PlayFabError error, string sender, Object context = null)
        {
            JConsole.Error($"-{sender}-Error {error.Error}: {error.ErrorMessage}\n{error.GenerateErrorReport()}",
                           P_Constants.DEBUG_PlayfabInteract, context);
        }

        /// <summary>
        /// this is used to display all the logs from an external call
        /// </summary>
        /// <param name="result"></param>
        public static void DisplayAllLogs(ExecuteCloudScriptResult result)
        {
            foreach (var log in result.Logs)
                JConsole.Log($"{P_Constants.DEBUG_PlayfabInteract}- Log {log.Message}");
        }

        public static void Log(string message) { throw new System.NotImplementedException(); }
    }
}
