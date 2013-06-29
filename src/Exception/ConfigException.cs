/* NuGet Install
 * Visual Studio 2005 or 2008
    * Install log4net -OutputDirectory .\packages
    * Add reference from "net20-full" for Visual Studio 2005 or "net35-full" for Visual Studio 2008
 * Visual Studio 2010 or higher
    * Install-Package log4net
    * Reference is auto-added 
*/
using log4net;

namespace PayPal.Exception
{
    public class ConfigException : System.Exception
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static ILog logger = LogManagerWrapper.GetLogger(typeof(ConfigException));

		/// <summary>
		/// Represents application configuration errors 
		/// </summary>
        public ConfigException() : base() { }
        
		/// <summary>
		/// Represents errors that occur during application execution
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public ConfigException(string message): base(message)
		{
			if (logger.IsErrorEnabled)
			{
				logger.Error(message, this);
			}
		}

		/// <summary>
		/// Represents errors that occur during application execution
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="cause">The exception that is the cause of the current exception</param>
		public ConfigException(string message, System.Exception cause): base(message, cause)
		{
			if (logger.IsErrorEnabled) 
			{
				logger.Error(message, this);
			}
		}
	}
}
