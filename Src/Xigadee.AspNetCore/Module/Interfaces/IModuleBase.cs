﻿using Microsoft.Extensions.Logging;

namespace Xigadee
{
    /// <summary>
    /// This interface contains the base settings for all modules.
    /// </summary>
    public interface IModuleBase
    {
        /// <summary>
        /// Gets or sets the module logger.
        /// </summary>
        ILogger Logger { get; set; }
    }
}
