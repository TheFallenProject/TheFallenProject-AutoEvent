using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns
{
    internal interface IMutator
    {
        /// <summary>
        /// CodeName for mutator (is not expected to be TMP'roed)
        /// </summary>
        string devName { get; }

        /// <summary>
        /// Display name for mutator (expected to be TMP'roed)
        /// </summary>
        string dispName { get; }

        /// <summary>
        /// Short summary of what this mutator does. For example: "Engages alpha warhead" or "Destroys all doors" etc.\n
        /// </summary>
        string description { get; }

        /// <summary>
        /// Code, which executes upon mutator selection.
        /// </summary>
        void Engaged();

        /// <summary>
        /// Code, which executes upon mutator deselection.
        /// </summary>
        void DisEngaged();

        /// <summary>
        /// Use it for conflict checking. For example, you don't want LightsOut with 939Vision
        /// </summary>
        /// <returns>true - Will engage
        /// false - Won't engage</returns>
        bool DoIWantToEngage();
    }
}
