using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns.Muts
{
    internal class example : IMutator
    {
        public string devName => "ExampleMutator";

        public string dispName => "<b>Тестовый мутатор</b>";

        public string description => "Это пример мутатора. Он не будет никогда запущен, так как в DoIWantToEngage всегда возвращается false.";

        public void DisEngaged()
        {

        }

        public bool DoIWantToEngage()
        {
            // Мутатор не будет выбран, так как данный метод вернул false. Чтобы он запустился, нужно вернуть true.
            return false;
        }

        public void Engaged()
        {

        }
    }
}
