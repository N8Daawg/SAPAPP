using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPAPP.Settings
{
    /// <summary>
    /// Class to allow editing of settings objects by user
    /// </summary>
	public static class Settings
    {
        /// <summary>
        /// Retrieve an object of the specified type from the specified settings file
        /// </summary>
        /// <typeparam name="T">the type the settings object</typeparam>
        /// <param name="filepath">path to XML file where settings are stored</param>
        /// <returns>an object containing settings from the file</returns>
        public static T Load<T>(string filepath)
        {
            // Start with a null settings class.
            T settings = default;

            // If the file exists...
            if (File.Exists(filepath))
            {
                // Retrieve settings from file into the object.
                settings = Serializer.DeserializeXML<T>(filepath);
            }

            // If the file didn't exist or otherwise couldn't be deserialized,
            // put default values into the settings object.
            if (settings == null)
            {
                settings = Activator.CreateInstance<T>();
            }

            return settings;
        }

        /// <summary>
        /// Save the object of the specified type to the specified local settings file
        /// </summary>
        /// <typeparam name="T">the type the settings object</typeparam>
        /// <param name="settings">Object to be serialized to XML</param>
        /// <param name="filepath">path to XML file where settings are stored</param>
        public static void Save<T>(T settings, string filepath)
        {
            // Save the settings as an XML file.
            Serializer.SerializeXML(settings, filepath);
        }

        #region HelperMethods

        /// <summary>
        /// Get the full path to an XML file where settings are stored.
        /// </summary>
        /// <param name="settingsName">name of the file (not including the extension)</param>
        /// <returns>full file path</returns>
        public static string GetFilePath(string directory, string fileName)
        {
            // Remove the file extension if one exists.
            // But preserve path name if it exists.
            fileName = Path.ChangeExtension(fileName, null);

            return Path.Combine(directory, fileName.Replace(' ', '_') + ".xml");
        }

        #endregion
    }
}