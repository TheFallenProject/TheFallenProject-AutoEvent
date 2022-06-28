using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Interfaces
{
    public interface IEvent
    {
        /// <summary>Название</summary>
        string Name { get; }

        /// <summary>Цвет</summary>
        string Color { get; }

        /// <summary>Описание</summary>
        string Description { get; }

        int Votes { get; set; }

        /// <summary>При запуске</summary>
        void OnStart();

        /// <summary>При окончании</summary>
        void OnStop();
    }
}
