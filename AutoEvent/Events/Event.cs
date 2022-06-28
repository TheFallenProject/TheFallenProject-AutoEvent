using AutoEvent.Functions;
using AutoEvent.Interfaces;
using MEC;
using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;

namespace AutoEvent
{
    internal class Event : IEvent
    {
        /// <summary>Название</summary>
        public string Name { get; } = "Обычная Игра";

        /// <summary>Цвет</summary>
        public string Color { get; } = "ffffff";

        /// <summary>Описание</summary>
        public string Description { get; }

        /// <summary>Голоса</summary>
        public int Votes { get; set; }

        /// <summary>При запуске</summary>
        public virtual void OnStart() { }

        /// <summary>При окончании</summary>
        public void OnStop() { }
    }
}